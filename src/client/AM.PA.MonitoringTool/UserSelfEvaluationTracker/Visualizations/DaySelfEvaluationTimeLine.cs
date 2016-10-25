using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Shared.Helpers;
using static Shared.Helpers.VisHelper;
using MuseTracker.Helper;

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
            Size = VisSize.Wide;
            Type = VisType.Day;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var chartQueryResultsLocal = Data.Queries.GetSelfEvaluationTimelineData(_date, VisType.Day);
            var minutesInterval = 1;
            var blinks = MuseTracker.Data.Queries.GetBlinksByMinutesInterval(_date, minutesInterval);
            var eegIndices = MuseTracker.Data.Queries.GetEEGIndexGroupedByMinutesInterval(_date, minutesInterval);

            if (chartQueryResultsLocal.Count < 3 && blinks.Count < 3 && eegIndices.Count < 3)
            {
                html += VisHelper.NotEnoughData("It is not possible to give you insights into your attention and engagement levels as you didn't fill out the pop-up often enough or not enough data from device is available. Try to fill it out at least 3 times per day and to wear the headband device.");
                return html;
            }

            /////////////////////
            // normalize data sets
            /////////////////////
            List<DateElementExtended<double>> normalizedBlinks = HelperMethods.TransformBlinksToExtendedDateElements(blinks);
            List<DateElementExtended<double>> normalizedEEG = HelperMethods.TransformEEGToExtendedDateElements(eegIndices);

            /////////////////////
            // CSS
            /////////////////////
            html += "<style type='text/css'>";
//            html += "#dailyengagementandattentionoverview .c3-event-rects { fill:yellow; fill-opacity:0.9; }";
            html += ".c3-line { stroke-width: 2px; }";
            html += ".c3-region.regionX { fill-opacity:0.1; }";
            html += "</style>";

            /////////////////////
            // HTML
            /////////////////////
            html += "<div id='" + VisHelper.CreateChartHtmlTitle(Title) + "' style='height:75%;' align='center'></div>";
            html += "<p style='text-align: center; font-size: 0.66em;'>Hint: Interpolates your perceived engagement and attention (from ratings) and computed EEGIndex and Blinks from Muse.<br>All values were normalized on a per day base and therefore not directly comparable to other days.</p>";


            /////////////////////
            // JS
            /////////////////////
            var ticks = CalculateLineChartAxisTicks(_date);
            var timeAxis = chartQueryResultsLocal.Aggregate("", (current, a) => current + (DateTimeHelper.JavascriptTimestampFromDateTime(a.Item1) + ", ")).Trim().TrimEnd(',');
            var timeAxisMuse = blinks.Aggregate("", (current, a) => current + (DateTimeHelper.JavascriptTimestampFromDateTime(Convert.ToDateTime(a.Item1)) + ", ")).TrimEnd(',');

            // Transform data into arrays for visualization
            var engagementFormattedData = chartQueryResultsLocal.Aggregate("", (current, p) => current + (VisHelper.Normalize(p.Item2,1,7) + ", ")).Trim().TrimEnd(',');
            var attentionFormattedData = chartQueryResultsLocal.Aggregate("", (current, p) => current + (VisHelper.Normalize(p.Item3, 1, 7) + ", ")).Trim().TrimEnd(',');
            var eegData = normalizedEEG.Aggregate("", (current, p) => current + p.normalizedvalue + ", ").Trim().TrimEnd(',');
            var originalEegData = normalizedEEG.Aggregate("", (current, p) => current + Math.Round(p.originalvalue, 2) + ", ").Trim().TrimEnd(',');
            var blinkData = normalizedBlinks.Aggregate("", (current, p) => current + p.normalizedvalue + ", ").Trim().TrimEnd(',');
            var originalBlinkData = normalizedBlinks.Aggregate("", (current, p) => current + p.originalvalue + ", ").Trim().TrimEnd(',');

            List<Tuple<DateTime, List<String>>> programsUsedAtTimes = new List<Tuple<DateTime, List<string>>>();
            List<Tuple<DateTime, int>> pgmSwitchesAtTimesT1 = new List<Tuple<DateTime, int>>();

            //List contains same time points as for engagementFormattedData and attentionFormattedData (timeAxis)
            foreach (Tuple<DateTime, int, int> t in chartQueryResultsLocal) {
                List<String> programs = UserEfficiencyTracker.Data.Queries.GetTopProgramsUsed(t.Item1, VisType.Hour, 2);
                var descPgm = programs.Select(p => ProcessNameHelper.GetFileDescription(p)).ToList();
                programsUsedAtTimes.Add(new Tuple<DateTime, List<String>>(t.Item1, descPgm));
                int switches = GetSwitches(t.Item1);
                pgmSwitchesAtTimesT1.Add(new Tuple<DateTime, int>(t.Item1, switches));
            }

            var usedPgmsT1 = programsUsedAtTimes.Aggregate("", (current, a) => current + ("{tsd:" + DateTimeHelper.JavascriptTimestampFromDateTime(a.Item1) + ", value:[" + string.Concat(a.Item2.Select(n => "'" + n + "',")).TrimEnd(',') + "]}, ")).Trim().TrimEnd(',');
            var switchesT1 = pgmSwitchesAtTimesT1.Aggregate("", (current, a) => current + ("{tsd:" + DateTimeHelper.JavascriptTimestampFromDateTime(a.Item1) + ", value:'"+ a.Item2 + "'}, ")).Trim().TrimEnd(',');

            List<Tuple<DateTime, List<String>>> programsUsedAtTimesT2 = new List<Tuple<DateTime, List<string>>>();
            List<Tuple<DateTime, int>> pgmSwitchesAtTimesT2 = new List<Tuple<DateTime, int>>();

            //List contains same time points as for blinkData and eegData (timeAxisMuse)
            foreach (Tuple<DateTime, int> t in blinks)
            {
                List<String> programs = UserEfficiencyTracker.Data.Queries.GetTopProgramsUsed(t.Item1, VisType.Hour, 2);
                var descPgm = programs.Select(p => ProcessNameHelper.GetFileDescription(p)).ToList();
                programsUsedAtTimesT2.Add(new Tuple<DateTime, List<String>>(t.Item1, descPgm));
                int switches = GetSwitches(t.Item1);
                pgmSwitchesAtTimesT2.Add(new Tuple<DateTime, int>(t.Item1, switches));
            }

            var usedPgmsT2 = programsUsedAtTimesT2.Aggregate("", (current, a) => current + ("{tsd:" + DateTimeHelper.JavascriptTimestampFromDateTime(a.Item1) + ", value:[" + string.Concat(a.Item2.Select(n => "'" + n + "',")).TrimEnd(',') + "]}, ")).Trim().TrimEnd(',');
            var switchesT2 = pgmSwitchesAtTimesT2.Aggregate("", (current, a) => current + ("{tsd:" + DateTimeHelper.JavascriptTimestampFromDateTime(a.Item1) + ", value:'" + a.Item2 + "'}, ")).Trim().TrimEnd(',');


            const string colorsUsed = "Engagement: '#990654', Attention: '#004979', EEGIndex: '#FF0A8D', Blinks: '#007acb' ";

            var regions = "{'Blinks':[ " + calculateIdleRegions(blinks) + " ] }";
            var names = "Engagement: 'Engagement(Ratings)', Attention: 'Attention(Ratings)', Blinks: 'Attention(#Blinks)', EEGIndex: 'Engagement(EEGIndex)'";
            var data = "xs: {'Engagement':'timeAxis', 'Attention': 'timeAxis', 'Blinks': 'timeAxisMuse', 'EEGIndex': 'timeAxisMuse'}, columns: [['timeAxis', " + timeAxis + "], ['timeAxisMuse', " + timeAxisMuse + "],['Engagement', " + engagementFormattedData + " ], ['Attention', " + attentionFormattedData + " ], ['Blinks', " + blinkData + " ], ['EEGIndex', " + eegData + " ] ], types: {Engagement:'line', Attention:'line', Blinks:'area', EEGIndex:'area'  }, colors: { " + colorsUsed + " }, axes: { Engagement: 'y',  Attention: 'y', Blinks:'y', EEGIndex:'y'} , names: {" + names + "}, regions: " + regions; // type options: spline, step, line
            var axis = "x: { localtime: true, type: 'timeseries', tick: { values: [ " + ticks + "], format: function(x) { return formatDate(x.getHours()); }}  }, y: { show:true, label: {text: 'Normalized Values', position: 'outer-middle'} }";
            var tooltip = "show: true, format: { title: function(d) { return 'Timestamp: ' + formatTime(d.getHours(),d.getMinutes()); } }, contents: function(d, defaultTitleFormat, defaultValueFormat, color){ return createCustomTooltip(d, defaultTitleFormat, defaultValueFormat, color, [" + originalBlinkData + "], [" + originalEegData + "], [" + usedPgmsT1 + "], [" + usedPgmsT2 + "], [" + switchesT1 + "], [" + switchesT2 + "]);} ";
            var parameters = " bindto: '#" + VisHelper.CreateChartHtmlTitle(Title) + "', data: { " + data + " }, padding: { left: 55, right: 55, bottom: 0, top: 0}, legend: { show: true }, axis: { " + axis + " }, tooltip: { " + tooltip + " }, point: { show: true }";

            html += "<script type='text/javascript'>";
            html += "var formatDate = function(hours) { var suffix = 'AM';\n if (hours >= 12) { suffix = 'PM'; hours = hours - 12; } \n if (hours == 0) { hours = 12; } \n if (hours < 10) return '0' + hours + ' ' + suffix; \n else return hours + ' ' + suffix; };\n";
            html += "var formatTime = function(hours, minutes) { var minFormatted = minutes; if (minFormatted < 10) minFormatted = '0' + minFormatted; var suffix = 'AM'; if (hours >= 12) { suffix = 'PM'; hours = hours - 12; } if (hours == 0) { hours = 12; } if (hours < 10) return '0' + hours + ':' + minFormatted + ' ' + suffix; else return hours + ':' + minFormatted + ' ' + suffix; };\n";

            html += "var " + VisHelper.CreateChartHtmlTitle(Title) + " = c3.generate({ " + parameters + " });\n";
            html += "</script>";

            return html;
        }

        private static int GetSwitches(DateTime date) {
            DateTime from = date.AddMinutes(-7);
            DateTime to = date.AddMinutes(7);
            return HelperMethods.GetNoOftopProgramSwitchesBetweenTimes(from, to);
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
