using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace UserInterruptibilityTracker
{
    public class Daemon : BaseTracker, ITracker
    {
        private DispatcherTimer _timer;
        private static TimeSpan _timeRemainingUntilNextSurvey;
        private InterruptibilitySurveyEntry _currentSurveyEntry;
        private TimeSpan _popUpIntervalInMins;

        #region ITracker Stuff

        public Daemon()
        {
            Name = "User Interruptibility Survey";
            _timeRemainingUntilNextSurvey = _popUpIntervalInMins;
        }
        public override void CreateDatabaseTablesIfNotExist()
        {
            //throw new NotImplementedException();
        }

        public override string GetVersion()
        {
            return "1";
            //throw new NotImplementedException();
        }

        public override bool IsEnabled()
        {
            return true;
        }

        public override void Start()
        {
            if (_timer != null)
                Stop();
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, new Random().Next(Settings.PopupIntervalMin, Settings.PopupIntervalMax));
            _timer.Tick += TimerTick;
            _timer.Start();


            IsRunning = true;
        }

        public override void Stop()
        {
            //throw new NotImplementedException();
        }

        public override void UpdateDatabaseTables(int version)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region Popup Daemon

        private void TimerTick(object sender, EventArgs args)
        {
            //RunSurvey(SurveyMode.IntervalPopUp);
            MessageBox.Show("The update timer has elapsed");
            
            _timer.Interval = new TimeSpan(0, 0, new Random().Next(Settings.PopupIntervalMin, Settings.PopupIntervalMax));
        }

        #endregion
    }
}
