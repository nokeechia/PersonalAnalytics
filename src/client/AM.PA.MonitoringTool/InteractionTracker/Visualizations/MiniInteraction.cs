// Created by André Meyer from the University of Zurich and Paige Rodeghero (ABB)
// Created: 2016-04-26
// 
// Licensed under the MIT License.

using System;
using Shared;

namespace InteractionTracker.Visualizations
{
    internal class MiniInteraction : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public MiniInteraction(DateTimeOffset date)
        {
            this._date = date;

            Title = "Interaction Summary";
            IsEnabled = true; //todo: handle by user
            Order = 1; //todo: handle by user
            Size = VisSize.Small;
            Type = VisType.Mini;
        }

        public override string GetHtml()
        {
            return "<p>babedibubedi</p>";
        }
    }
}
