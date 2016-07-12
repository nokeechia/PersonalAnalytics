// Created by Monica Trink from the University of Zurich
// Created: 2016-07-09
// 
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using Shared.Data;
namespace MuseTracker
{
    public class Daemon : BaseTracker, ITracker
    {
        public Daemon()
        {
            Name = "Muse Tracker";
        }
        public override void CreateDatabaseTablesIfNotExist()
        {
            //do nothing yet
        }

        public override bool IsEnabled()
        {
            return true;
        }

        public bool MuseTrackerEnabled() {
            return true;
        }

        public override void Start()
        {
            IsRunning = true;
            Console.Write("++++ muse tracker started");
        }

        public override void Stop()
        {
            IsRunning = false;
            Console.Write("++++ muse tracker stopped");
        }

        public override void UpdateDatabaseTables(int version)
        {
            // no database updates necessary yet
        }
    }
}
