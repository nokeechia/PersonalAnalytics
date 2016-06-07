// Created by André Meyer at UZH and Paige Rodeghero (ABB)
// Created: 2016/04/26
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractionTracker.Visualizations;
using Shared;
using System.Windows.Threading;
using InteractionTracker.Data;

namespace InteractionTracker
{
    public class Daemon : BaseTracker, ITracker
    {
        #region ITracker Stuff

        public Daemon()
        {
            Name = "Interaction Tracker";
        }

        public override void Start()
        {
            IsRunning = true;
        }

        public override void Stop()
        {
            IsRunning = false;
        }

        public override void CreateDatabaseTablesIfNotExist()
        {
            // no tables to create
        }

        public override bool IsEnabled()
        {
            return Settings.IsEnabled;
        }

        public override List<IVisualization> GetVisualizationsMini(DateTimeOffset date)
        {
            var vis = new MiniThisDayInteraction(date);
            return new List<IVisualization> { vis };
        }

        public override List<IVisualization> GetVisualizationsDay(DateTimeOffset date)
        {
            var vis1 = new DayInteractionStepChart();
            var vis2 = new DayThisDayInteraction(date);
            return new List<IVisualization> { vis1, vis2 };
        }

        #endregion

        #region Other Methods

        public InteractionDataSet GetInteractionDataSet(DateTimeOffset date)
        {
            return InteractionDataHelper.GetAllInteractionData(date);
        }

        #endregion
    }
}
