using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Shared.Helpers;
using MuseTracker.Data;
using static Shared.Helpers.VisHelper;
using System.Web.Script.Serialization;

namespace MuseTracker.Visualizations
{
    internal class WeekMuseVisualization : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public WeekMuseVisualization(DateTimeOffset date)
        {
            this._date = date;

            Title = "Attention Overview (#blinks) and Engagement Overview (EEG Index) ";
            IsEnabled = true;
            Order = 1;
            Size = VisSize.Square;
            Type = VisType.Week;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var blinks = Queries.GetBlinksOfWeek(_date);
            var eegData = Queries.GetEEGIndexOfWeek(_date);

            if (blinks.Count < 1 && eegData.Count < 1)
            {
                html += VisHelper.NotEnoughData("It is not possible to give you insights. Either because of no blink or EEG data.");
                return html;
            }

            /////////////////////
            // normalize data sets
            /////////////////////

            //because more blinks indicate less attention we have to reverse the values
            List<Tuple<DateTime, double>> logblinks = blinks.Select(i => new Tuple<DateTime, double>(i.Item1, Math.Log10(i.Item2)*-1)).ToList(); //log transform because of huge differences in ranges, -1 because reverse blinks indicate more attention
            var minBlinks = logblinks.Min(i => i.Item2);
            var maxBlinks = logblinks.Max(i => i.Item2);
            
         
            List<DateElementExtended<double>> normalizedBlinks = logblinks.Select(i => new DateElementExtended<double> { date = i.Item1.ToString(),
                normalizedvalue = Math.Round(VisHelper.Rescale(i.Item2, minBlinks, maxBlinks, 0.0, 1.0), 2),
                originalvalue = Math.Pow(10, i.Item2*-1), //because log befored
                extraInfo = new JavaScriptSerializer().Serialize(FromDateToExtraInfo(i.Item1))
            }).ToList();


            /////////////////////
            // CSS
            /////////////////////
            html += "<style type='text/css'>";
            html += ".c3-line { stroke-width: 2px; }";
            html += ".c3-grid text, c3.grid line { fill: gray; }";
            html += "</style>";


            /////////////////////
            // HTML
            /////////////////////
            html += "<div id='attentionoverview' align='center'></div>";
            html += "<div id='engagementoverview' align='center'></div>";
            html += "<p style='text-align: center; font-size: 0.66em;'>Hint: Shows attention via reverse number of blinks per day. Less blinks indicate more attention and average EEG Index per day which indicates your engagement level.</p>";


            var dataInJSFormatBlinks = VisHelper.CreateJavaScriptArrayOfObjectsDoubleWithAddtionalInfo(normalizedBlinks);
            var dataInJSFormatEEG = VisHelper.CreateJavaScriptArrayOfObjectsDouble(eegData);

            /////////////////////
            // JS
            /////////////////////
            html += "<script type='text/javascript'>";
            html += "var parseDate = d3.time.format('%m/%d/%Y %H:%M:%S %p').parse;";
            html += "var dataInJSFormatBlinks = [" + dataInJSFormatBlinks + "];";
            html += "var beginDate = moment(parseDate('"+ _date.DateTime + "')).isoWeekday(1);";
            html += "var endDate = moment(parseDate('" + _date.DateTime + "')).isoWeekday(7);";
            html += "var chartDataBlinks = dataInJSFormatBlinks.map(function(dateElement) {" +
                "return {" +
                "date: parseDate(dateElement.date)," +
                "count: dateElement.normalizedvalue," +
                "tp: {original: dateElement.originalvalue, extra_info: dateElement.extraInfo}" +
                "};" +
            "});";

            html += "var heatmapBlinks = calendarHeatmap()" +
                            ".data(chartDataBlinks)" +
                            ".selector('#attentionoverview')" +
                            ".tooltipEnabled(true)" +
                            ".tooltipUnit('#Blink')" +
                            ".colorRange(['#cce4f4', '#007acb'])" +
                            ".begin(beginDate)" +
                            ".end(endDate)" +
                            ".mode('WEEK')" +
                            ".onClick(function(data) {" +
                "console.log('data', data);" +
            "});";
            html += "heatmapBlinks();";

            //render EEG chart
            html += "var dataInJSFormatEEG = [" + dataInJSFormatEEG + "];";
            html += "var chartDataEEG = dataInJSFormatEEG.map(function(dateElement) {" +
                "return {" +
                "date: parseDate(dateElement.date)," +
                "count: dateElement.count" +
                "};" +
            "});";

            html += "var heatmapEEG = calendarHeatmap()" +
                            ".data(chartDataEEG)" +
                            ".selector('#engagementoverview')" +
                            ".tooltipEnabled(true)" +
                            ".tooltipUnit('Avg EEG Indice')" +
                            ".colorRange(['#ffcee8', '#FF0A8D'])" +
                            ".begin(beginDate)" +
                            ".end(endDate)" +
                            ".mode('WEEK')" +
                      ".onClick(function(data) {" +
                "console.log('data', data);" +
            "});";
            html += "heatmapEEG();";


            html += "</script>";

            return html;
        }

        public static ExtraInfo FromDateToExtraInfo(DateTime _date)
        {
            return new ExtraInfo { switches = UserEfficiencyTracker.Data.Queries.GetNrOfProgramSwitches(_date, VisType.Day), topPgms = UserEfficiencyTracker.Data.Queries.GetTopProgramsUsed(_date, VisType.Day, 3).Aggregate("", (current, p) => current + ProcessNameHelper.GetFileDescription(p) + ", ").Trim().TrimEnd(',') };
        }

    }


}
