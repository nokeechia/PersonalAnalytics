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

            Title = "Interaction Summary"; // Interaction Summary
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
            var numMeetingsPrevious = 0d;
            var numEmailsReceivedPrevious = 0d;
            var numEmailsSentPrevious = 0d;
            var numChatsPrevious = 0d;
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

            numMeetingsPrevious = Math.Ceiling(numMeetingsPrevious);
            numEmailsReceivedPrevious = Math.Ceiling(numEmailsReceivedPrevious);
            numEmailsSentPrevious = Math.Ceiling(numEmailsSentPrevious);
            numChatsPrevious = Math.Ceiling(numChatsPrevious);

            var meetingsImage = "meetingsIcon";
            var emailsReceivedImage = "emailsReceivedIcon";
            var emailsSentImage = "emailsSentIcon";
            var chatsImage = "chatsIcon";
            string meetingsIcon = "<img src=\"" + meetingsImage + ".png\">";
            string emailsReceivedIcon = "<img src=\"" + emailsReceivedImage + ".png\">";
            string emailsSentIcon = "<img src=\"" + emailsSentImage + ".png\">";
            string chatsIcon = "<img src=\"" + chatsImage + ".png\">";

            var okayColor = "#4F8A10"; // green
            var warningThreshold = 1.25;
            var warningColor = "#9F6000"; // yellow
            var errorThreshold = 1.5;
            var errorColor = "#D8000C"; // red
            var meetingsColor = okayColor;
            if (numMeetingsNow > numMeetingsPrevious * errorThreshold)
                meetingsColor = errorColor;
            else if (numMeetingsNow > numMeetingsPrevious * warningThreshold)
                meetingsColor = warningColor;
            var emailsReceivedColor = okayColor;
            if (numEmailsReceivedNow > numEmailsReceivedPrevious * errorThreshold)
                emailsReceivedColor = errorColor;
            else if (numEmailsReceivedNow > numEmailsReceivedPrevious * warningThreshold)
                emailsReceivedColor = warningColor;
            var emailsSentColor = okayColor;
            if (numEmailsSentNow > numEmailsSentPrevious * errorThreshold)
                emailsSentColor = errorColor;
            else if (numEmailsSentNow > numEmailsSentPrevious * warningThreshold)
                emailsSentColor = warningColor;
            var chatsColor = okayColor;
            if (numChatsNow > numChatsPrevious * errorThreshold)
                chatsColor = errorColor;
            else if (numChatsNow > numChatsPrevious * warningThreshold)
                chatsColor = warningColor;
            
            // generate html where queries were successful
            var html = string.Empty;
            html += "<table border=\"0\" cellpadding=\"2\" cellspacing=\"2\">";
            html += "<tr><th>Interaction</th><th>Today's Total</th><th>Previous Average</th></tr>";
            html += "<tr><td>" + meetingsIcon + "</td><td style=\"color: " + meetingsColor + "\">" + numMeetingsNow + "</td><td>" + numMeetingsPrevious.ToString() + "</td></tr>";
            html += "<tr><td>" + chatsIcon + "</td><td style=\"color: " + chatsColor + "\">" + numChatsNow + "</td><td>" + numChatsPrevious.ToString() + "</td></tr>";
            html += "<tr><td>" + emailsSentIcon + "</td><td style=\"color: " + emailsSentColor + "\">" + numEmailsSentNow + "</td><td>" + numEmailsSentPrevious.ToString() + "</td></tr>";
            html += "<tr><td>" + emailsReceivedIcon + "</td><td style=\"color: " + emailsReceivedColor + "\">" + numEmailsReceivedNow + "</td><td>" + numEmailsReceivedPrevious.ToString() + "<br />";
            html += "</table>";

            return html;
        }
    }
}
