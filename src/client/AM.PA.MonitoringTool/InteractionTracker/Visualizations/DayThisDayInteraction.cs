// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Timers;
using InteractionTracker.Data;
using Shared;
using Shared.Data;


namespace InteractionTracker.Visualizations
{
    internal class DayThisDayInteraction : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public DayThisDayInteraction(DateTimeOffset date)
        {
            this._date = date;

            Title = "Today's Interaction Details";
            IsEnabled = true; //todo: handle by user
            Order = 2; //todo: handle by user
            Size = VisSize.Wide;
            Type = VisType.Day;
        }

        public override string GetHtml()
        {
            //var startTime = Database.GetInstance().GetUserWorkStart(_date);

            // get now data
            var now = _date.Date.AddHours(18);
            var numMeetingsNow = Queries.GetMeetingsFromSixAm(now).Count;
            var numEmailsReceivedNow = Queries.GetEmailsSentOrReceivedFromSixAm(now, "received").Count;
            var numEmailsSentNow = Queries.GetEmailsSentOrReceivedFromSixAm(now, "sent").Count;
            var numChatsNow = Queries.GetChatsSentOrReceivedFromSixAm(now).Count;

            // get previous data
            var numMeetingsPrevious = 0;
            var numEmailsReceivedPrevious = 0;
            var numEmailsSentPrevious = 0;
            var numChatsPrevious = 0;
            var j = -6;
            for (var i = -1; i > j; i--)
            {
                var previous = now.AddDays(i);
                if (previous.DayOfWeek == DayOfWeek.Saturday || previous.DayOfWeek == DayOfWeek.Sunday)
                {
                    j--;
                    continue;
                }
                numMeetingsPrevious += Queries.GetMeetingsFromSixAm(previous).Count;
                numEmailsReceivedPrevious += Queries.GetEmailsSentOrReceivedFromSixAm(previous, "received").Count;
                numEmailsSentPrevious += Queries.GetEmailsSentOrReceivedFromSixAm(previous, "sent").Count;
                numChatsPrevious += Queries.GetChatsSentOrReceivedFromSixAm(previous).Count;
            }
            numMeetingsPrevious /= ((j + 1) * -1);
            numEmailsReceivedPrevious /= ((j + 1) * -1);
            numEmailsSentPrevious /= ((j + 1) * -1);
            numChatsPrevious /= ((j + 1) * -1);

            // generate html where queries were successful
            var html = string.Empty;
            html += "Previous Average - Today's Total<br />";
            html += numMeetingsPrevious + " - " + numMeetingsNow + "<br />";
            html += numEmailsReceivedPrevious + " - " + numEmailsReceivedNow + "<br />";
            html += numEmailsSentPrevious + " - " + numEmailsSentNow + "<br />";
            html += numChatsPrevious + " - " + numChatsNow + "";

            return html;
        }
    }
}
