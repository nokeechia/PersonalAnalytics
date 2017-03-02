using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Reflection;
using UserInterruptibilityTracker.Models;
using UserInterruptibilityTracker.Data;
using Shared.Data;
using System.Globalization;
using System.Data;
using System.Threading;

namespace UserInterruptibilityTracker
{
    public class Daemon : BaseTracker, ITracker
    {
        private DispatcherTimer _timer;
        private static TimeSpan _timeRemainingUntilNextSurvey;
        private InterruptibilitySurveyEntry _currentSurveyEntry;

        #region ITracker Stuff

        public Daemon()
        {
            Name = "User Interruptibility Survey";
            _timeRemainingUntilNextSurvey = GetNewRandomizedInterval();
        }
        public override void CreateDatabaseTablesIfNotExist()
        {
            Queries.CreateUserInterruptibilityTables();
        }

        public override string GetVersion()
        {
            var v = new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version;
            return Shared.Helpers.VersionHelper.GetFormattedVersion(v);
        }

        public override bool IsEnabled()
        {
            return true; // always enabled
        }

        public override void Start()
        {
            if (_timer != null)
                Stop();
            _timer = new DispatcherTimer();
            _timer.Interval = Settings.TimerCheckInterval;
            _timer.Tick += TimerTick;
            _timer.Start();

            IsRunning = true;
        }

        public override void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            IsRunning = false;
        }

        public override void UpdateDatabaseTables(int version)
        {
            // nothing to update yet
        }

        #endregion

        #region Popup Daemon

        private void TimerTick(object sender, EventArgs args)
        {
            if (_timeRemainingUntilNextSurvey > Settings.TimerCheckInterval)
            {
                _timeRemainingUntilNextSurvey = _timeRemainingUntilNextSurvey.Subtract(Settings.TimerCheckInterval);
                return; // not necessary
            }
            else
            {
                RunSurvey();
            }
        }

        private TimeSpan GetNewRandomizedInterval()
        {
            return new TimeSpan(0, new Random().Next(Settings.PopupIntervalMin, Settings.PopupIntervalMax), 0);
        }

        /// <summary>
        /// runs the survey and handles the response
        /// </summary>
        /// <returns></returns>
        private void RunSurvey()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                () =>
                {
                    // if it's the first time the notification is shown
                    if (_currentSurveyEntry == null)
                    {
                        _currentSurveyEntry = new InterruptibilitySurveyEntry();
                        _currentSurveyEntry.TimeStampNotification = DateTime.Now;
                    }

                    // (re-)set the timestamp of filling out the survey
                    _currentSurveyEntry.TimeStampStarted = DateTime.Now;

                    // set previous entry to show previous entry time in popup
                    var popup = new InterruptibilityPopUp(Queries.GetPreviousInterruptibilitySurveyEntry());

                    // show popup & handle response                    
                    if (((InterruptibilityPopUp)popup).ShowDialog() == true)
                    {
                        HandleIntervalPopUpResponse((InterruptibilityPopUp)popup);
                    }
                    else
                    {
                        // we get here when DialogResult is set to false (which never happens) 
                        Database.GetInstance().LogErrorUnknown(Name);

                        // to ensure it still shows some pop-ups later 
                        _timeRemainingUntilNextSurvey = GetNewRandomizedInterval();
                    }
                }));
            }
            catch (ThreadAbortException e) { Database.GetInstance().LogError(Name + ": " + e.Message); }
            catch (Exception e) { Database.GetInstance().LogError(Name + ": " + e.Message); }
        }

        /// <summary>
        /// handles the response to the interruptibility popup
        /// </summary>
        /// <param name="popup"></param>
        private void HandleIntervalPopUpResponse(InterruptibilityPopUp popup)
        {
            // user took the survey
            if ((popup.UserSelectedInterruptibility >= 1 && popup.UserSelectedInterruptibility <= 5))
            {
                SaveInterruptibilitySurvey(popup);
            }
            // user postponed the survey
            else if (popup.PostPoneSurvey != PostPoneSurvey.None)
            {
                PostponeInterruptibilitySurvey(popup);
                Database.GetInstance().LogInfo(string.Format(CultureInfo.InvariantCulture, "The participant postponed the interruptibility-survey ({0}).", popup.PostPoneSurvey));
            }
            // something strange happened
            else
            {
                _currentSurveyEntry = null;
                _timeRemainingUntilNextSurvey = GetNewRandomizedInterval();
            }
        }

        /// <summary>
        /// Saves the interruptibility-survey results in the db & resets some items
        /// </summary>
        /// <param name="popup"></param>
        private void SaveInterruptibilitySurvey(InterruptibilityPopUp popup)
        {
            _currentSurveyEntry.Interruptibility = popup.UserSelectedInterruptibility;
            _currentSurveyEntry.TimeStampFinished = DateTime.Now;
            Queries.SaveInterruptibilityEntry(_currentSurveyEntry);
            _currentSurveyEntry = null; // reset
        }

        /// <summary>
        /// handler to postpone the survey for the selected time
        /// Hint: the selected time (e.g. postpone 1 hour) equals 1 hour of computer running (i.e. developer working) time
        /// </summary>
        /// <param name="notify"></param>
        private void PostponeInterruptibilitySurvey(InterruptibilityPopUp notify)
        {
            switch (notify.PostPoneSurvey)
            {
                case (PostPoneSurvey.Skipped):
                    _timeRemainingUntilNextSurvey = GetNewRandomizedInterval();  // set new interval
                    break;
                case (PostPoneSurvey.Postpone1h):
                    _timeRemainingUntilNextSurvey = TimeSpan.FromHours(1) + GetNewRandomizedInterval(); // in one hour + random interval
                    break;
                case (PostPoneSurvey.Postpone2h):
                    _timeRemainingUntilNextSurvey = TimeSpan.FromHours(2) + GetNewRandomizedInterval(); // in two hours + random interval
                    break;
                default:
                    _timeRemainingUntilNextSurvey = GetNewRandomizedInterval();  // set new interval
                    break;
            }
        }

        /// <summary>
        /// manually run survey (click on ContextMenu)
        /// </summary>
        public void ManualTakeSurveyNow()
        {
            RunSurvey();
        }

        #endregion
    }
}
