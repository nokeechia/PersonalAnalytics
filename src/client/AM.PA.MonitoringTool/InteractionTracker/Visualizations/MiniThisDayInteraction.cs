// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using InteractionTracker.Data;
using Shared;
using Shared.Helpers;
using System.Collections.Generic;

namespace InteractionTracker.Visualizations
{
    internal class MiniThisDayInteraction : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public MiniThisDayInteraction(DateTimeOffset date)
        {
            this._date = date;

            Title = string.Empty; // Interaction Summary
            IsEnabled = true; //todo: handle by user
            Order = 2; //todo: handle by user
            Size = VisSize.Mini;
            Type = VisType.Mini;
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
            string meetingsIcon = "<img src=\"" + meetingsImage + ".png\" title=\"Meetings scheduled\"> ";
            string emailsReceivedIcon = "<img src=\"" + emailsReceivedImage + ".png\" title=\"Emails received\"> ";
            string emailsSentIcon = "<img src=\"" + emailsSentImage + ".png\" title=\"Emails sent\"> ";
            string chatsIcon = "<img src=\"" + chatsImage + ".png\" title=\"Chat messages\"> ";

            // colors
            var okayColor = "#4F8A10"; // green
            var warningColor = "#9F6000"; // yellow
            var errorColor = "#D8000C"; // red

            // base colors
            Dictionary<string, string> messageColors = new Dictionary<string, string>();
            //var meetingsColor = okayColor;
            messageColors.Add("meetings", okayColor); // meetingsColor
            //var emailsReceivedColor = okayColor;
            messageColors.Add("received emails", okayColor); // emailsReceivedColor
            //var emailsSentColor = okayColor;
            messageColors.Add("sent emails", okayColor); // emailsSentColor
            //var chatsColor = okayColor;
            messageColors.Add("chats", okayColor); // chatsColor

            var meetings = data.NumMeetingsNow + "</td><td>" + data.AvgMeetingsPrevious.ToString() + "</td></tr>";
            var chats = data.NumChatsNow + "</td><td>" + data.AvgChatsPrevious.ToString() + "</td></tr>";
            var emailsSent = data.NumEmailsSentNow + "</td><td>" + data.AvgEmailsSentPrevious.ToString() + "</td></tr>";
            var emailsReceived = data.NumEmailsReceivedNow + "</td><td>" + data.AvgEmailsReceivedPrevious.ToString() + "</td></tr>";

            if (data.NumMeetingsNow >= data.AvgMeetingsPrevious + (data.MeetingsSD))
            {
                messageColors["meetings"] = errorColor;
                meetings = "<b>" + data.NumMeetingsNow + "</b></td><td><b>" + data.AvgMeetingsPrevious.ToString() + "</b></td></tr>";
            }
            else if (data.NumMeetingsNow >= data.AvgMeetingsPrevious + (data.MeetingsSD/2))
                messageColors["meetings"] = warningColor;

            if (data.NumEmailsReceivedNow >= data.AvgEmailsReceivedPrevious + (data.EmailsReceivedSD))
            {
                messageColors["received emails"] = errorColor;
                emailsReceived = "<b>" + data.NumEmailsReceivedNow + "</b></td><td><b>" + data.AvgEmailsReceivedPrevious.ToString() + "</b></td></tr>";
            }
            else if (data.NumEmailsReceivedNow >= data.AvgEmailsReceivedPrevious + (data.EmailsReceivedSD/2))
                messageColors["received emails"] = warningColor;

            if (data.NumEmailsSentNow >= data.AvgEmailsSentPrevious + (data.EmailsSentSD))
            {
                messageColors["sent emails"] = errorColor;
                emailsSent = "<b>" + data.NumEmailsSentNow + "</b></td><td><b>" + data.AvgEmailsSentPrevious.ToString() + "</b></td></tr>";
            }
            else if (data.NumEmailsSentNow >= data.AvgEmailsSentPrevious + (data.EmailsSentSD/2))
                messageColors["sent emails"] = warningColor;

            
            if (data.NumChatsNow >= data.AvgChatsPrevious + (data.ChatsSD))
            {
                messageColors["chats"] = errorColor;
                chats = "<b>" + data.NumChatsNow + "</b></td><td><b>" + data.AvgChatsPrevious.ToString() + "</b></td></tr>";
            }
            else if (data.NumChatsNow >= data.AvgChatsPrevious + (data.ChatsSD/2))
                messageColors["chats"] = warningColor;

            // generate html where queries were successful
            var html = string.Empty;
            //            var categorySurpased ="";
            var msgColor = okayColor;
            if (messageColors.ContainsValue(warningColor)) msgColor = warningColor;
            else if (messageColors.ContainsValue(errorColor)) msgColor = errorColor;

            html += "<p style=\"color: " + msgColor + "\">";
            if (msgColor == okayColor)
            {
                html += "Your overall communications are going well!";
            }
            else if (msgColor == warningColor)
            {
                html += "You have had a few too many ";
                foreach (var message in messageColors)
                    if (message.Value == warningColor)
                        html += message.Key + " and ";
                html = html.Substring(0, html.Length - 5) + "!";
            }
            else // errorColor
            {
                html += "You have had way too many ";
                foreach (var message in messageColors)
                    if (message.Value == errorColor)
                        html += message.Key + " and ";
                html = html.Substring(0, html.Length - 5) + "!";
            }
            html += "</p>";
            html += "<table class=\"interactions\" border=\"0\" cellpadding=\"2\" cellspacing=\"2\">";
            html += "<tr><th>Communication</th><th>Today's Total</th><th>Previous Average (5-day)</th></tr>";
            html += "<tr><td>" + meetingsIcon + "</td><td style=\"color: " + messageColors["meetings"] + "\">" + meetings;
            html += "<tr><td>" + chatsIcon + "</td><td style=\"color: " + messageColors["chats"] + "\">" + chats;
            html += "<tr><td>" + emailsSentIcon + "</td><td style=\"color: " + messageColors["sent emails"] + "\">" + emailsSent;
            html += "<tr><td>" + emailsReceivedIcon + "</td><td style=\"color: " + messageColors["received emails"] + "\">" + emailsReceived;
  //          html += "<tr><td>" + "You have " + categorySurpased + "today";
            html += "</table>";

            return html;
        }
    }
}
