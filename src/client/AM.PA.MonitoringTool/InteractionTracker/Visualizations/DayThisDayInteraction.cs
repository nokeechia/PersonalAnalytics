// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using InteractionTracker.Data;
using Shared;


namespace InteractionTracker.Visualizations
{
    internal class DayThisDayInteraction : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public DayThisDayInteraction(DateTimeOffset date)
        {
            this._date = date;

            Title = " ";
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
            var numMeetingsPrevious = new List<double>();
            var numEmailsReceivedPrevious = new List<double>();
            var numEmailsSentPrevious = new List<double>();
            var numChatsPrevious = new List<double>();
            var j = -6;
            for (var i = -1; i > j; i--)
            {
                var previous = now.AddDays(i);
                if (previous.DayOfWeek == DayOfWeek.Saturday || previous.DayOfWeek == DayOfWeek.Sunday)
                {
                    j--;
                    continue;
                }
                numMeetingsPrevious.Add(Queries.GetMeetingsFromSixAm(previous).Count);
                numEmailsReceivedPrevious.Add(Queries.GetEmailsSentOrReceivedFromSixAm(previous, "received").Count);
                numEmailsSentPrevious.Add(Queries.GetEmailsSentOrReceivedFromSixAm(previous, "sent").Count);
                numChatsPrevious.Add(Queries.GetChatsSentOrReceivedFromSixAm(previous).Count);
            }

            var avgMeetingsPrevious = Math.Ceiling(numMeetingsPrevious.Average());
            var avgEmailsReceivedPrevious = Math.Ceiling(numEmailsReceivedPrevious.Average());
            var avgEmailsSentPrevious = Math.Ceiling(numEmailsSentPrevious.Average());
            var avgChatsPrevious = Math.Ceiling(numChatsPrevious.Average());

            var meetingsImage = "meetingsIcon";
            var emailsReceivedImage = "emailsReceivedIcon";
            var emailsSentImage = "emailsSentIcon";
            var chatsImage = "chatsIcon";
            string meetingsIcon = "<img src=\"" + meetingsImage + ".png\">";
            string emailsReceivedIcon = "<img src=\"" + emailsReceivedImage + ".png\">";
            string emailsSentIcon = "<img src=\"" + emailsSentImage + ".png\">";
            string chatsIcon = "<img src=\"" + chatsImage + ".png\">";

            var okayColor = "#4F8A10"; // green
            var warningColor = "#9F6000"; // yellow
            var errorColor = "#D8000C"; // red

            var meetingsColor = okayColor;
            var emailsReceivedColor = okayColor;
            var emailsSentColor = okayColor;
            var chatsColor = okayColor;

            var meetings = numMeetingsNow + "</td><td>" + avgMeetingsPrevious.ToString() + "</td></tr>";
            var chats = numChatsNow + "</td><td>" + avgChatsPrevious.ToString() + "</td></tr>";
            var emailsSent = numEmailsSentNow + "</td><td>" + avgEmailsSentPrevious.ToString() + "</td></tr>";
            var emailsReceived = numEmailsReceivedNow + "</td><td>" + avgEmailsReceivedPrevious.ToString() + "<br />";

            var meetingsSD = Math.Ceiling(CalculateStdDev(numMeetingsPrevious));
            if (numMeetingsNow >= avgMeetingsPrevious + (meetingsSD))
            {
                meetingsColor = errorColor;
                meetings = "<b>" + numMeetingsNow + "</b></td><td><b>" + avgMeetingsPrevious.ToString() + "</b></td></tr>";
            }
            else if (numMeetingsNow >= avgMeetingsPrevious + (meetingsSD/2))
                meetingsColor = warningColor;

            var emailsReceivedSD = Math.Ceiling(CalculateStdDev(numEmailsReceivedPrevious));
            if (numEmailsReceivedNow >= avgEmailsReceivedPrevious + (emailsReceivedSD))
            {
                emailsReceivedColor = errorColor;
                emailsReceived = "<b>" + numEmailsReceivedNow + "</b></td><td><b>" + avgEmailsReceivedPrevious.ToString() + "</b><br />";
            }
            else if (numEmailsReceivedNow >= avgEmailsReceivedPrevious + (emailsReceivedSD/2))
                emailsReceivedColor = warningColor;

            var emailsSentSD = Math.Ceiling(CalculateStdDev(numEmailsSentPrevious));
            if (numEmailsSentNow >= avgEmailsSentPrevious + (emailsSentSD))
            {
                emailsSentColor = errorColor;
                emailsSent = "<b>" + numEmailsSentNow + "</b></td><td><b>" + avgEmailsSentPrevious.ToString() + "</b></td></tr>";
            }
            else if (numEmailsSentNow >= avgEmailsSentPrevious + (emailsSentSD/2))
                emailsSentColor = warningColor;

            var chatsSD = Math.Ceiling(CalculateStdDev(numChatsPrevious));
            if (numChatsNow >= avgChatsPrevious + (chatsSD))
            {
                chatsColor = errorColor;
                chats = "<b>" + numChatsNow + "</b></td><td><b>" + avgChatsPrevious.ToString() + "</b></td></tr>";
            }
            else if (numChatsNow >= avgChatsPrevious + (chatsSD/2))
                chatsColor = warningColor;

            // generate html where queries were successful
            var html = string.Empty;
            html += "<table class=\"interactions\" border=\"0\" cellpadding=\"2\" cellspacing=\"2\">";
            html += "<tr><th>Interaction</th><th>Today's Total</th><th>Previous Average</th></tr>";
            html += "<tr><td>" + meetingsIcon + "</td><td style=\"color: " + meetingsColor + "\">" + meetings;
            html += "<tr><td>" + chatsIcon + "</td><td style=\"color: " + chatsColor + "\">" + chats;
            html += "<tr><td>" + emailsSentIcon + "</td><td style=\"color: " + emailsSentColor + "\">" + emailsSent;
            html += "<tr><td>" + emailsReceivedIcon + "</td><td style=\"color: " + emailsReceivedColor + "\">" + emailsReceived;
            html += "</table>";

            return html;
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }
    }
}
