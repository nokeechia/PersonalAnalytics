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

            string awardImageToShow ="";

            // focused time last hour
            var focusedTimeLastHour = Queries.GetFocusTimeInLastHour();
            var focusedTimeLastHourInMins = Math.Round((double)focusedTimeLastHour/60, 0);

            // context switches
            var numInteractionSwitches = Queries.GetNoInteractionSwitches();
            var numMeetingSwitches = Queries.GetMeetingsForLastHour();

            if (numInteractionSwitches > 10)
            {
                awardImageToShow = "redAward";
            }else if (numInteractionSwitches > 5)
            {
                awardImageToShow = "yellowAward";
            }
            else
            {
                awardImageToShow = "greenAward";
            }

            var html = "<br><div style=\"float: left; width: 20 %; \"><img src=\"" + awardImageToShow + ".png\" width=\"50\" height=\"80\"></div><div style=\"float: right; width: 80 %; \">" + focusedTimeLastHourInMins + " mins focused<br />"
                       + (numInteractionSwitches + numMeetingSwitches) + " task switches</div>"; 
            return html;
        }
    }
}
