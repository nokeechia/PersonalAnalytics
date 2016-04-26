// Created by Paige Rodeghero (paige.rodeghero@us.abb.com) from ABB USCRC
// Created: 2016-4-18
// 
// Licensed under the MIT License.

using Shared;
using Shared.Helpers;
using System;
using MsOfficeTracker.Data;
using System.Globalization;

namespace MsOfficeTracker.Visualizations
{
    internal class DayChatsReceivedAndSent : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public DayChatsReceivedAndSent(DateTimeOffset date)
        {
            this._date = date;

            Title = "Received Chats vs.<br />Number Chats Sent";
            IsEnabled = true; //todo: handle by user
            Order = 4; //todo: handle by user
            Size = VisSize.Small;
            Type = VisType.Day;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////

            // get the latest stored chat entry
            var chatsSentResult = Queries.GetSentOrReceivedChats(_date, "sent");
            var chatsSent = chatsSentResult.Item2;
            var inboxReceivedResult = Queries.GetSentOrReceivedChats(_date, "received");
            var chatsReceived = inboxReceivedResult.Item2;

            var isToday = (_date.Date == DateTime.Now.Date);
            var lastUpdatedMinsAgo = Math.Abs((DateTime.Now - chatsSentResult.Item1).TotalMinutes);

            // if database entry is outdated or not there, create a live API chat and override entries
            if ((chatsSentResult.Item1 == DateTime.MinValue || inboxReceivedResult.Item1 == DateTime.MinValue) || // no chats stored yet
                (isToday && lastUpdatedMinsAgo > Settings.SaveEmailCountsIntervalInMinutes) || // request is for today and saved results are too old
                (chatsSent == -1 || chatsReceived == -1)) // could not fetch sent/received chats
            {
                // create and save a new chat snapshot (inbox, sent, received)
                var res = Queries.CreateChatsAndCallsSnapshot(_date.Date, false);
                chatsSent = res.Item1;
                chatsReceived = res.Item2;
            }

            // error (only if no data at all)
            if (chatsReceived == -1 && chatsSent == -1)
            {
                return VisHelper.NotEnoughData(Dict.NotEnoughData);
            }

            // no chats sent/received
            if (chatsReceived == 0 && chatsSent == 0)
            {
                return VisHelper.NotEnoughData("You didn't receive or send any chats today.");
            }

            // as a goodie get this too :)
            //var timeSpentInOutlook = Queries.TimeSpentInOutlook(_date);

            /////////////////////
            // HTML
            /////////////////////

            var chatInboxString = (chatsReceived == -1) ? "?" : chatsReceived.ToString(CultureInfo.InvariantCulture);
            var chatsSentString = (chatsSent == -1) ? "?" : chatsSent.ToString(CultureInfo.InvariantCulture);

            html += "<p style='text-align: center; margin-top:-0.7em;'><strong style='font-size:2.7em;'>" + chatInboxString + " | " + chatsSentString + "</strong></p>";

            //if (timeSpentInOutlook > 1)
            //{
            //    html += "<p style='text-align: center; margin-top:-0.7em;'>time spent in Outlook: " + Math.Round(timeSpentInOutlook, 0) + "min</p>";
            //}

            return html;
        }
    }
}
