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
    internal class MiniThisDayInteraction : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public MiniThisDayInteraction(DateTimeOffset date)
        {
            this._date = date;

            Title = "Averages Per Day";
            IsEnabled = true; //todo: handle by user
            Order = 2; //todo: handle by user
            Size = VisSize.Small;
            Type = VisType.Mini;
        }

        public override string GetHtml()
        {
            //var startTime = Database.GetInstance().GetUserWorkStart(_date);

            // get data
            var numMeetings = Queries.GetNumMeetingsForDate(_date);
            var numEmailsReceived = Queries.GetSentOrReceivedEmails(_date, "received");
            var numEmailsSent = Queries.GetSentOrReceivedEmails(_date, "sent");
            var numChats = Queries.GetNumCallsOrChats(_date, Settings.ChatsTable);
            var numCalls = Queries.GetNumCallsOrChats(_date, Settings.CallsTable);

            // generate html where queries were successful
            var change = 1.0;
            var html = string.Empty;
            if (numMeetings > -1) html += (numMeetings / change).ToString("n2") + " meetings<br />";
            if (numChats > -1) html += (numChats / change).ToString("n2") + " chats<br />";
            if (numEmailsSent > -1) html += (numEmailsSent / change).ToString("n2") + " emails sent<br />";
            if (numEmailsReceived > -1) html += (numEmailsReceived / change).ToString("n2") + " emails received<br />";
            if (numCalls > -1) html += (numCalls / change).ToString("n2") + " calls";

            return html;
        }
    }
}
