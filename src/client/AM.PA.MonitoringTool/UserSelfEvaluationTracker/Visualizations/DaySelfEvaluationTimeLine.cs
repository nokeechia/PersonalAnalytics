using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Shared.Helpers;
using UserSelfEvaluationTracker.Data;
using MuseTracker.Data;

namespace UserSelfEvaluationTracker.Visualizations
{

    internal class DaySelfEvaluationTimeLine : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public DaySelfEvaluationTimeLine(DateTimeOffset date)
        {
            this._date = date;

            Title = "Daily Engagement and Attention Overview";
            IsEnabled = true;
            Order = 9;
            Size = VisSize.Square;
            Type = VisType.Day;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var chartQueryResultsLocal = UserSelfEvaluationTracker.Data.Queries.GetSelfEvaluationTimelineData(_date, VisType.Day);
            var blinks = MuseTracker.Data.Queries.GetBlinks(_date);
            var eegIndexes = MuseTracker.Data.Queries.GetEEGIndex(_date);
            if (chartQueryResultsLocal.Count < 3 && blinks.Count < 3 && eegIndexes.Count < 3)
            {
                html += VisHelper.NotEnoughData("It is not possible to give you insights into your productivity as you didn't fill out the pop-up often enough. Try to fill it out at least 3 times per day.");
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
            html += "<div id='" + VisHelper.CreateChartHtmlTitle(Title) + "' style='height:75%;' align='center'></div>";
            html += "<p style='text-align: center; font-size: 0.66em;'>Hint: Interpolates your perceived engagement and attention states, based on your pop-up responses.</p>";


            /////////////////////
            // JS
            /////////////////////
            var ticks = CalculateLineChartAxisTicks(_date);
            var timeAxis = chartQueryResultsLocal.Aggregate("", (current, a) => current + (DateTimeHelper.JavascriptTimestampFromDateTime(a.Item1) + ", ")).Trim().TrimEnd(',');

            var timeAxis2 = blinks.Aggregate("", (current, a) => current + (DateTimeHelper.JavascriptTimestampFromDateTime(Convert.ToDateTime(a.Item1)) + ", ")).TrimEnd(',');
            var timeAxis3 = eegIndexes.Aggregate("", (current, a) => current + (DateTimeHelper.JavascriptTimestampFromDateTime(Convert.ToDateTime(a.Item1)) + ", ")).TrimEnd(',');

            var engagementFormattedData = chartQueryResultsLocal.Aggregate("", (current, p) => current + (p.Item2 + ", ")).Trim().TrimEnd(',');
            var attentionFormattedData = chartQueryResultsLocal.Aggregate("", (current, p) => current + (p.Item3 + ", ")).Trim().TrimEnd(',');
            var blinkData = blinks.Aggregate("", (current, p) => current + (p.Item2 + ", ")).TrimEnd(',');
            var eegData = eegIndexes.Aggregate("", (current, p) => current + (Math.Round(p.Item2, 2) + ", ")).TrimEnd(',');
            const string colorPerceivedEngagement = "'User_Input_Level' : '#007acb'";

            var data = "xs: {'Engagement':'timeAxis', 'Attention': 'timeAxis', 'Blinks': 'timeAxis2', 'EEGIndex': 'timeAxis2'}, columns: [['timeAxis', " + timeAxis + "], ['timeAxis2', " + timeAxis2 + "], ['Engagement', " + engagementFormattedData + " ], ['Attention', " + attentionFormattedData + " ], ['Blinks', " + blinkData + " ], ['EEGIndex', " + eegData + " ] ], types: {Engagement:'line', Attention:'line', Blinks:'area', EEGIndex:'area'  }, colors: { " + colorPerceivedEngagement + " }, axes: { Engagement: 'y',  Attention: 'y', Blinks:'y2', EEGIndex:'y2' } "; // type options: spline, step, line

            //var grid = "y: { lines: [ { value: 1, text: 'not at all' }, { value: 4, text: 'moderately' }, { value: 7, text: 'very strong' } ] } ";
            var axis = "x: { localtime: true, type: 'timeseries', tick: { values: [ " + ticks + "], format: function(x) { return formatDate(x.getHours()); }}  }, y: { min: 1, max: 7 }, y2: {show: true}"; // show: false, 
            var tooltip = "show: true, format: { title: function(d) { return 'Timestamp: ' + formatTime(d.getHours(),d.getMinutes()); }}";
            var parameters = " bindto: '#" + VisHelper.CreateChartHtmlTitle(Title) + "', data: { " + data + " }, padding: { left: 45, right: 45, bottom: -10, top: 0}, legend: { show: true }, axis: { " + axis + " }, tooltip: { " + tooltip + " }, point: { show: true }";


            html += "<script type='text/javascript'>";
            html += "var formatDate = function(hours) { var suffix = 'AM'; if (hours >= 12) { suffix = 'PM'; hours = hours - 12; } if (hours == 0) { hours = 12; } if (hours < 10) return '0' + hours + ' ' + suffix; else return hours + ' ' + suffix; };";
            html += "var formatTime = function(hours, minutes) { var minFormatted = minutes; if (minFormatted < 10) minFormatted = '0' + minFormatted; var suffix = 'AM'; if (hours >= 12) { suffix = 'PM'; hours = hours - 12; } if (hours == 0) { hours = 12; } if (hours < 10) return '0' + hours + ':' + minFormatted + ' ' + suffix; else return hours + ':' + minFormatted + ' ' + suffix; };";
            html += "var " + VisHelper.CreateChartHtmlTitle(Title) + " = c3.generate({ " + parameters + " });"; // return x.getHours() + ':' + x.getMinutes();
            html += "</script>";

            return html;
        }

        /// <summary>
        /// Creates a list of one-hour axis times
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static string CalculateLineChartAxisTicks(DateTimeOffset date)
        {
            var dict = new Dictionary<DateTime, int>();
            VisHelper.PrepareTimeAxis(date, dict, 60);

            return dict.Aggregate("", (current, a) => current + (DateTimeHelper.JavascriptTimestampFromDateTime(a.Key) + ", ")).Trim().TrimEnd(',');
        }
    }

}
