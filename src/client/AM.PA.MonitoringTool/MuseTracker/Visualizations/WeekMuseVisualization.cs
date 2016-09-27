using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Helpers;
using MuseTracker.Data;

namespace MuseTracker.Visualizations
{
    internal class WeekMuseVisualization : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public WeekMuseVisualization(DateTimeOffset date)
        {
            this._date = date;

            Title = "Attention Overview (# blinks) and Engagement Overview (EEG Index) ";
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

            if (blinks.Count < 1 && eegData.Count < 1) //Todo: have to set a min limit
            {
                html += VisHelper.NotEnoughData("It is not possible to give you insights into your productivity.");
                return html;
            }


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
            html += "<div id='attentionoverview' style='width:50%; display:inline-block;' align='center'></div>";
            html += "<div id='engagementoverview' style='width:50%; display:inline-block;' align='center'></div>";
            html += "<p style='text-align: center; font-size: 0.66em;'>Hint: Shows attention via reverse number of blinks per day. Less blinks indicate more attention and average EEG Index per day which indicates your engagement level.</p>";


            var dataInJSFormatBlinks = VisHelper.CreateJavaScriptArrayOfObjects(blinks);
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
                "count: dateElement.count" +
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
                      ".onClick(function(data) {" +
                "console.log('data', data);" +
            "});";
            html += "heatmapEEG();";


            html += "</script>";

            return html;
        }
    }
}
