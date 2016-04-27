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

            Title = "Interactions: This Day";
            IsEnabled = true; //todo: handle by user
            Order = 1; //todo: handle by user
            Size = VisSize.Small;
            Type = VisType.Mini;
        }

        public override string GetHtml()
        {
            // todo: handle -1

            var noMeetings = 0;
            var noEmailsReceived = 0;
            var noEmailsSent = 0;
            var noChats = 0;

            var html = noMeetings + " meetings today<br />"
                       + noEmailsReceived + " emails received<br />"
                       + noEmailsSent + " emails sent<br />"
                       + noChats + " chats";

            return html;        }
    }
}
