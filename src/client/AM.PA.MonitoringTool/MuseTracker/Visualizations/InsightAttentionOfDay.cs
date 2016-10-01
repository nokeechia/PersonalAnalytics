using System;
using Shared;
using Shared.Helpers;
using MuseTracker.Data;
using MuseTracker.Models;

namespace MuseTracker.Visualizations
{
    internal class InsightAttentionOfDay: BaseVisualization, IVisualization
    {

        private readonly DateTimeOffset _date;

        public InsightAttentionOfDay(DateTimeOffset date)
        {
            this._date = date;

            Title = "Attention of the Day";
            IsEnabled = true;
            Order = 21;
            Size = VisSize.Small;
            Type = VisType.Day;


        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////

            /////////////////////
            // HTML
            /////////////////////

            html += "<p style='text-align: center; margin-top:-0.7em;'>At <strong style='font-size:1.25em; color:#007acc;'>" + "11:15 PM" + "</strong> using </p>";
            html += "<p style='text-align: center; margin-top:-0.7em;'><strong style='font-size:1.25em; color:#007acc;'>" + "Visual Studio" + "</strong></p>";
            html += "<p style='text-align: center; margin-top:-0.7em;'>and your physical activity was <strong style='color:#007acc;'>over</strong> the daily average.</p>";

            return html;
        }
    }
}
