// Created by Paige Rodeghero (paige.rodeghero@us.abb.com) from ABB USCRC
// Created: 2016-4-18
// 
// Licensed under the MIT License.


using System;
using MsOfficeTracker.Data;
using Shared;

namespace MsOfficeTracker.Visualizations
{
    internal class DayMeetingsAttended : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public DayMeetingsAttended(DateTimeOffset date)
        {
            _date = date;

            Title = "Total Meetings";
            IsEnabled = false;
            Order = 5;
            Size = VisSize.Small;
            Type = VisType.Day;
        }

        /// <summary>
        /// Grabs number of meetings attended and puts into the html string
        /// </summary>
        /// <returns>html string</returns>
        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var numberOfMeetings = Queries.GetMeetingsForDate(_date.Date).ToString();
            html += "<p style='text-align: center; margin-top:-0.7em;'><strong style='font-size:2.7em;'>" +
                    numberOfMeetings + "</strong></p>";
            return html;
        }
    }
}