// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using InteractionTracker.Data;
using Shared;
using Shared.Data;

namespace InteractionTracker.Visualizations
{
    internal class MiniThisDayInteraction : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public MiniThisDayInteraction(DateTimeOffset date)
        {
            this._date = date;

            Title = "Today's Interactions";
            IsEnabled = true; //todo: handle by user
            Order = 2; //todo: handle by user
            Size = VisSize.Small;
            Type = VisType.Mini;
        }

        public override string GetHtml()
        {
            // todo: handle -1

            // total meetings
            var noMeetings = Queries.GetMeetingsForDate();

            // EMAILS SENT averages per hour
            var startTime = Database.GetInstance().GetUserWorkStart(_date);

            var numEmailsReceived = Queries.GetSentOrReceivedEmails(_date, "received");
            var numEmailsSent = Queries.GetSentOrReceivedEmails(_date, "sent");
            var numChats = Queries.GetCallsOrChats(_date, Settings.ChatsTable);
            var numCalls = Queries.GetCallsOrChats(_date, Settings.CallsTable);

            var html = noMeetings + " meetings today<br />"
                       + numEmailsReceived + " emails received<br />"
                       + numEmailsSent + " emails sent<br />"
                       + numChats + " chats<br />"
                       + numCalls + " calls";

            return html;
        }

        //private static int GetAvgEmailsPerHour(DateTimeOffset date, DateTime startTime, string sentOrReceived)
        //{
        //    var hourDict = new Dictionary<DateTimeOffset, int>();

        //    // fill dictionary
        //    var tempTime = startTime;
        //    do
        //    {
        //        hourDict.Add(tempTime, 0);
        //        tempTime = tempTime.AddHours(1);
        //    }
        //    while (tempTime < DateTime.Now);

        //    // emails per hour
        //    var emailsSent = Queries.GetSentOrReceivedEmails(date, sentOrReceived);

        //    foreach (var email in emailsSent)
        //    {
        //        foreach (var hourKey in hourDict.Keys.ToList())
        //        {
        //            if (email.Item1 > hourKey && email.Item1 < hourKey.AddHours(1))
        //            {
        //                hourDict[hourKey] += email.Item2;
        //            }
        //        }
        //    }

        //    // calculate average
        //    var avg = hourDict.Values.Average();

        //    return (int)Math.Round(avg, 0);
        //}
    }
}
