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
    internal class MonthMuseEngagementVisualization : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public MonthMuseEngagementVisualization(DateTimeOffset date)
        {
            this._date = date;

            Title = "Engagement Overview (EEG Index)";
            IsEnabled = true;
            Order = 2;
            Size = VisSize.Wide;
            Type = VisType.Month;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var eegData = Queries.GetEEGIndexOverAYear(_date);
            if (eegData.Count < 1) //Todo: have to set a min limit
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
            html += "<div id='" + VisHelper.CreateChartHtmlTitle(Title) + "' style='height:75%;' align='center'></div>";
            html += "<p style='text-align: center; font-size: 0.66em;'>Hint: Shows average EEG Index per day which indicates your engagement level.</p>";

            var dataInJSFormat = VisHelper.CreateJavaScriptArrayOfObjectsDouble(eegData);

            /////////////////////
            // JS
            /////////////////////
            html += "<script type='text/javascript'>";
            html += "var parseDate = d3.time.format('%d/%m/%Y %H:%M:%S').parse;";
            html += "var dataInJSFormat = [" + dataInJSFormat + "];";
            html += "var now = moment().endOf('day').toDate();";
            html += "var yearAgo = moment().startOf('day').subtract(1, 'year').toDate();";
            html += "var chartData2 = dataInJSFormat.map(function(dateElement) {" +
                "return {" +
                "date: parseDate(dateElement.date)," +
                "count: dateElement.count" +
                "};" +
            "});";

            html += "var heatmap = calendarHeatmap()" +
                            ".data(chartData2)" +
                            ".selector('#engagementoverview')" +
                            ".tooltipEnabled(true)" +
                            ".colorRange(['#ffcce5', '#ff3399'])" +
                      ".onClick(function(data) {" +
                "console.log('data', data);" +
            "});";
            html += "heatmap();  // render the chart";

            html += "</script>";

            return html;
        }
    }
}
