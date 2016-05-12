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

            Title = "Averages Per Hour";
            IsEnabled = true; //todo: handle by user
            Order = 2; //todo: handle by user
            Size = VisSize.Small;
            Type = VisType.Mini;
        }

        public override string GetHtml()
        {
            //var startTime = Database.GetInstance().GetUserWorkStart(_date);

            // get data
            var numMeetings = Queries.GetNumMeetingsForDate(DateTime.Now.Date);
            var numEmailsReceived = Queries.GetSentOrReceivedEmails(_date, "received");
            var numEmailsSent = Queries.GetSentOrReceivedEmails(_date, "sent");
            var numChats = Queries.GetNumCallsOrChats(_date, Settings.ChatsTable);
            var numCalls = Queries.GetNumCallsOrChats(_date, Settings.CallsTable);

            // generate html where queries were successful
            var html = string.Empty;
            if (numMeetings > -1) html += numMeetings + " total meetings<br />";
            if (numChats > -1) html += numChats + " chats<br />";
            if (numEmailsSent > -1) html += numEmailsSent + " emails sent<br />";
            if (numEmailsReceived > -1) html += numEmailsReceived + " emails received<br />";
            if (numCalls > -1) html += numCalls + " calls";

            return html;
        }
    }
}
