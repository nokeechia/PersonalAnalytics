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
    internal class MonthMuseAttentionVisualization : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public MonthMuseAttentionVisualization(DateTimeOffset date) {
            this._date = date;

            Title = "Attention Overview (# blinks)";
            IsEnabled = true;
            Order = 1;
            Size = VisSize.Wide;
            Type = VisType.Month;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var blinks = Queries.GetBlinksByYear(_date);
            if (blinks.Count < 1 ) //Todo: have to set a min limit
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
            html += "<p style='text-align: center; font-size: 0.66em;'>Hint: Interpolates your perceived engagement and attention states, based on your pop-up responses.</p>";

            var dataInJSFormat = VisHelper.CreateJavaScriptArrayOfObjects(blinks);
            
            /////////////////////
            // JS
            /////////////////////
            html += "<script type='text/javascript'>";
            html += "var parseDate = d3.time.format('%Y-%m-%d').parse;";
            html += "var dataInJSFormat = [" + dataInJSFormat + "];";
            html += "var now = moment().endOf('day').toDate();";
            html += "var yearAgo = moment().startOf('day').subtract(1, 'year').toDate();";
            html += "var chartData2 = dataInJSFormat.map(function(dateElement) {" +
                "return {" +
                "date: parseDate(dateElement.date)," +
                "count: dateElement.count" +
                "};" +
            "});";

            html += "var chartData = d3.time.days(yearAgo, now).map(function(dateElement) {" +
                "return {" +
                "date: dateElement," +
                "count:" +
                    "(dateElement.getDay() !== 0 && dateElement.getDay() !== 6) ? Math.floor(Math.random() * 60) : Math.floor(Math.random() * 10)" +
                "};" +
            "});";

            html += "var heatmap = calendarHeatmap()" +
                            ".data(chartData2)" +
                            ".selector('#attentionoverview')" +
                            ".tooltipEnabled(true)" +
                            ".colorRange(['#f4f7f7', '#79a8a9'])" +
                      ".onClick(function(data) {" +
                "console.log('data', data);" +
            "});";
            html += "heatmap();  // render the chart";

            html += "</script>";

            return html;
        }
    }
}
