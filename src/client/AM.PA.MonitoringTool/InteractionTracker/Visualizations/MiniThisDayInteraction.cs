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
            // todo: handle -1

            // total meetings
            var numMeetings = Queries.GetMeetingsForDate();

            // EMAILS SENT averages per hour
            var startTime = Database.GetInstance().GetUserWorkStart(_date);

            var numEmailsReceived = Queries.GetSentOrReceivedEmails(_date, "received");
            var numEmailsSent = Queries.GetSentOrReceivedEmails(_date, "sent");
            var numChats = Queries.GetCallsOrChats(_date, Settings.ChatsTable);
            var numCalls = Queries.GetCallsOrChats(_date, Settings.CallsTable);

            var html = numMeetings + " total meetings<br />"
                       + numEmailsReceived + " emails received<br />"
                       + numEmailsSent + " emails sent<br />"
                       + numChats + " chats<br />"
                       + numCalls + " calls";

            return html;
        }
    }
}
