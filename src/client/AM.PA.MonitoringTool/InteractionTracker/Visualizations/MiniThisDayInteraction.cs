// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using InteractionTracker.Data;
using Shared;


namespace InteractionTracker.Visualizations
{
    internal class MiniThisDayInteraction : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public MiniThisDayInteraction(DateTimeOffset date)
        {
            this._date = date;

            Title = "Today - Previous Average";
            IsEnabled = true; //todo: handle by user
            Order = 2; //todo: handle by user
            Size = VisSize.Small;
            Type = VisType.Mini;
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

            string meetingsIcon = "";
            string emailsReceivedIcon = "";
            string emailsSentIcon = "";
            string chatsIcon = "";
            string callsIcon = "";

            // generate html where queries were successful
            var html = string.Empty;
           // html += "Today's Total - Previous Average<br />";
            html += meetingsIcon + numMeetingsNow + "" + numMeetingsPrevious + "<br />";
            html += emailsReceivedIcon + numEmailsReceivedNow + "" + numEmailsReceivedPrevious + "<br />";
            html += emailsSentIcon + numEmailsSentNow + "" + numEmailsSentPrevious + "<br />";
            html += chatsIcon + numChatsNow + "" + numChatsPrevious + "";

            return html;
        }
    }
}
