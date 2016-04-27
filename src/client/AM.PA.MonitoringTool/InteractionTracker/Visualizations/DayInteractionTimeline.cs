// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-27
// 
// Licensed under the MIT License.

using Shared;

namespace InteractionTracker.Visualizations
{
    internal class DayInteractionTimeline : BaseVisualization, IVisualization
    {
        public DayInteractionTimeline()
        {
            Title = "Today's Interaction Breakdown";
            IsEnabled = true; //todo: handle by user
            Order = 1; //todo: handle by user
            Size = VisSize.Wide;
            Type = VisType.Day;
        }

        public override string GetHtml()
        {
            return "here we are";
        }
    }
}
