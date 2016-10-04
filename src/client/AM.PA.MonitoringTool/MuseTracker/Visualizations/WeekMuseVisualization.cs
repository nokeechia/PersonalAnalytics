﻿using System;
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
            List<DateElementExtended<double>> normalizedBlinks = Helpers.Helper.NormalizeBlinks(blinks);
            List<DateElementExtended<double>> normalizedEEG = Helpers.Helper.NormalizeEEGIndices(eegData);

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
            html += "<p style='text-align: center; font-size: 0.66em;'>Hint: Shows attention via reverse number of blinks per day. Less blinks indicate more attention and average EEG Index per day which indicates your engagement level. All values were normalized regarding the current week and are therefore not directly comparable to other weeks.</p>";


            var dataInJSFormatBlinks = VisHelper.CreateJavaScriptArrayOfObjectsDoubleWithAddtionalInfo(normalizedBlinks);
            var dataInJSFormatEEG = VisHelper.CreateJavaScriptArrayOfObjectsDoubleWithAddtionalInfo(normalizedEEG);

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
                "count: dateElement.normalizedvalue," +
                "tp: {original: dateElement.originalvalue, extra_info: dateElement.extraInfo}" +
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



    }


}
