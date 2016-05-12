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
            //// Start Email Count Timer
            //if (_timer != null)
            //    Stop();

            //// initialize a new timer
            //var interval = (int)TimeSpan.FromMinutes(Settings.SaveEmailCountsIntervalInMinutes).TotalMilliseconds;
            //_timer = new Timer(new TimerCallback(TimerTick), // callback
            //                null,  // no idea
            //                10000, // start immediately after 10 seconds
            //                interval); // interval

            IsRunning = true;
        }

        public override void Stop()
        {
            //    if (_timer != null)
            //    {
            //        _timer.Dispose();
            //        _timer = null;
            //    }

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
            var vis1 = new MiniLastHourInteraction();
            var vis2 = new MiniThisDayInteraction(date);
            return new List<IVisualization> { vis1, vis2 };
        }

        public override List<IVisualization> GetVisualizationsDay(DateTimeOffset date)
        {
            var vis = new DayInteractionStepChart();
            return new List<IVisualization> { vis };
        }

        #endregion
    }
}
