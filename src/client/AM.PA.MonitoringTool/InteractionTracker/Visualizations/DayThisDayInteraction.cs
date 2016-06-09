// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using InteractionTracker.Data;
using Shared;
using Shared.Helpers;


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
            // get interaction data
            var data = InteractionDataHelper.GetAllInteractionData(_date);

            if (data == null)
            {
                return VisHelper.Error();
            }

            // prepare icons
            var meetingsImage = "meetingsIcon";
            var emailsReceivedImage = "emailsReceivedIcon";
            var emailsSentImage = "emailsSentIcon";
            var chatsImage = "chatsIcon";
            string meetingsIcon = "<img src=\"" + meetingsImage + ".png\">";
            string emailsReceivedIcon = "<img src=\"" + emailsReceivedImage + ".png\">";
            string emailsSentIcon = "<img src=\"" + emailsSentImage + ".png\">";
            string chatsIcon = "<img src=\"" + chatsImage + ".png\">";

            // colors
            var okayColor = "#4F8A10"; // green
            var warningColor = "#9F6000"; // yellow
            var errorColor = "#D8000C"; // red

            // base colors
            var meetingsColor = okayColor;
            var emailsReceivedColor = okayColor;
            var emailsSentColor = okayColor;
            var chatsColor = okayColor;

            var meetings = data.NumMeetingsNow + "</td><td>" + data.AvgMeetingsPrevious.ToString() + "</td></tr>";
            var chats = data.NumChatsNow + "</td><td>" + data.AvgChatsPrevious.ToString() + "</td></tr>";
            var emailsSent = data.NumEmailsSentNow + "</td><td>" + data.AvgEmailsSentPrevious.ToString() + "</td></tr>";
            var emailsReceived = data.NumEmailsReceivedNow + "</td><td>" + data.AvgEmailsReceivedPrevious.ToString() + "</td></tr>";

            if (data.NumMeetingsNow >= data.AvgMeetingsPrevious + (data.MeetingsSD))
            {
                meetingsColor = errorColor;
                meetings = "<b>" + data.NumMeetingsNow + "</b></td><td><b>" + data.AvgMeetingsPrevious.ToString() + "</b></td></tr>";
            }
            else if (data.NumMeetingsNow >= data.AvgMeetingsPrevious + (data.MeetingsSD / 2))
                meetingsColor = warningColor;

            if (data.NumEmailsReceivedNow >= data.AvgEmailsReceivedPrevious + (data.EmailsReceivedSD))
            {
                emailsReceivedColor = errorColor;
                emailsReceived = "<b>" + data.NumEmailsReceivedNow + "</b></td><td><b>" + data.AvgEmailsReceivedPrevious.ToString() + "</b></td></tr>";
            }
            else if (data.NumEmailsReceivedNow >= data.AvgEmailsReceivedPrevious + (data.EmailsReceivedSD / 2))
                emailsReceivedColor = warningColor;

            if (data.NumEmailsSentNow >= data.AvgEmailsSentPrevious + (data.EmailsSentSD))
            {
                emailsSentColor = errorColor;
                emailsSent = "<b>" + data.NumEmailsSentNow + "</b></td><td><b>" + data.AvgEmailsSentPrevious.ToString() + "</b></td></tr>";
            }
            else if (data.NumEmailsSentNow >= data.AvgEmailsSentPrevious + (data.EmailsSentSD / 2))
                emailsSentColor = warningColor;


            if (data.NumChatsNow >= data.AvgChatsPrevious + (data.ChatsSD))
            {
                chatsColor = errorColor;
                chats = "<b>" + data.NumChatsNow + "</b></td><td><b>" + data.AvgChatsPrevious.ToString() + "</b></td></tr>";
            }
            else if (data.NumChatsNow >= data.AvgChatsPrevious + (data.ChatsSD / 2))
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
    }
}
