// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using InteractionTracker.Data;
using Shared;

namespace InteractionTracker.Visualizations
{
    internal class MiniLastHourInteraction : BaseVisualization, IVisualization
    {
        public MiniLastHourInteraction()
        {
            Title = "Last hour";
            IsEnabled = true; //todo: handle by user
            Order = 1; //todo: handle by user
            Size = VisSize.Small;
            Type = VisType.Mini;
        }

        public override string GetHtml()
        {
            // todo: handle -1

            // focused time last hour
            var focusedTimeLastHour = Queries.GetFocusTimeInLastHour();
            var focusedTimeLastHourInMins = Math.Round((double)focusedTimeLastHour/60, 0);

            // context switches
            var noInteractionSwitches = Queries.GetNoInteractionSwitches();
            var noMeetingSwitches = Queries.GetMeetingsForLastHour();

            var html = focusedTimeLastHourInMins + " mins focused<br />"
                       + (noInteractionSwitches + noMeetingSwitches) + " switches";

            return html;
        }
    }
}
