using System;
using System.Threading;
using System.Windows;
using Shared;
using System.Windows.Threading;
using UserSelfEvaluationTracker.Models;
using UserSelfEvaluationTracker.Data;
using Shared.Data;
using System.Globalization;
using UserSelfEvaluationTracker.Visualizations;
using System.Collections.Generic;

namespace UserSelfEvaluationTracker
{
    public enum SurveyMode
    {
        IntervalPopUp,
        DailyPopUp
    }

    public class Daemon : BaseTracker, ITracker
    {
        private DispatcherTimer _timer;
        private static TimeSpan _timeRemainingUntilNextSurvey;
        private SelfEvaluationSurveyEntry _currentSurveyEntry;
        private TimeSpan _popUpIntervalInMins;
        private bool _popUpIsEnabled;

        public Daemon()
        {
            Name = "User Self Evaluation Survey";
            _timeRemainingUntilNextSurvey = PopUpIntervalInMins;

        }

        public TimeSpan PopUpIntervalInMins
        {
            get
            {
                var value = Database.GetInstance().GetSettingsInt("PopUpInterval", Settings.DefaultPopUpInterval);
                _popUpIntervalInMins = TimeSpan.FromMinutes(value);
                return _popUpIntervalInMins;
            }
            set
            {
                var updatedInterval = value;

                // only update if settings changed
                if (updatedInterval == _popUpIntervalInMins) return;
                _popUpIntervalInMins = updatedInterval;

                // update settings
                Database.GetInstance().SetSettings("PopUpInterval", updatedInterval.TotalMinutes.ToString(CultureInfo.InvariantCulture));

                // update interval time
                _timeRemainingUntilNextSurvey = _popUpIntervalInMins;

                // log
                Database.GetInstance().LogInfo("The participant updated the setting 'PopUpInterval' to " + _popUpIntervalInMins);
            }
        }

        public bool PopUpEnabled
        {
            get
            {
                _popUpIsEnabled = Database.GetInstance().GetSettingsBool("PopUpEnabled", Settings.DefaultPopUpIsEnabled);
                return _popUpIsEnabled;
            }
            set
            {
                var updatedIsEnabled = value;

                // only update if settings changed
                if (updatedIsEnabled == _popUpIsEnabled) return;
                _popUpIsEnabled = updatedIsEnabled;

                // update settings
                Database.GetInstance().SetSettings("PopUpEnabled", value);

                // start/stop timer if necessary
                if (!updatedIsEnabled && _timer.IsEnabled)
                {
                    _timer.Stop();
                    _popUpIntervalInMins = TimeSpan.MinValue;
                }
                else if (updatedIsEnabled && !_timer.IsEnabled)
                {
                    _timer.Start();
                    _popUpIntervalInMins = PopUpIntervalInMins;
                }

                // log
                Database.GetInstance().LogInfo("The participant updated the setting 'PopUpEnabled' to " + updatedIsEnabled);
            }
        }

        /// <summary>
        /// loop runs in a separate thread
        /// </summary>
        private void TimerTick(object sender, EventArgs args)
        {
            // daily survey
            //if (DateTime.Now.Date != _lastDailyPopUpResponse.Date &&  // no pop-up today yet
            //    DateTime.Now.TimeOfDay >= Settings.DailyPopUpEarliestMoment && // not before 04.00 am
            //    Queries.GetPreviousDailyPopUpResponse() != DateTime.Now.Date) // only if there is a previous work day
            //{
            //    RunSurvey(SurveyMode.DailyPopUp);
            //    return; // don't immediately show interval survey
            //}

            // inverval survey
            if (_timeRemainingUntilNextSurvey > Settings.SurveyCheckerInterval)
            {
                _timeRemainingUntilNextSurvey = _timeRemainingUntilNextSurvey.Subtract(Settings.SurveyCheckerInterval);
                return; // not necessary
            }
            else
            {
                RunSurvey(SurveyMode.IntervalPopUp);
            }
        }

        /// <summary>
        /// runs the survey and handles the response
        /// </summary>
        /// <returns></returns>
        private void RunSurvey(SurveyMode mode)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                () =>
                {
                    //var previousActiveWorkday = Queries.GetPreviousActiveWorkDay();

                    // set previous entry to show previous entry time in popup
                    var popup = (Window)new IntervalSelfEvaluationPopUp(Queries.GetPreviousIntervalSurveyEntry());

                    // if it's the first time the notification is shown
                    if (_currentSurveyEntry == null)
                    {
                        _currentSurveyEntry = new SelfEvaluationSurveyEntry();
                        _currentSurveyEntry.TimeStampNotification = DateTime.Now;
                        //if (previousActiveWorkday > DateTime.MinValue) _currentSurveyEntry.PreviousWorkDay = previousActiveWorkday;
                    }

                    // (re-)set the timestamp of filling out the survey
                    _currentSurveyEntry.TimeStampStarted = DateTime.Now;

                    // show popup & handle response
                    //if (mode == SurveyMode.DailyPopUp
                    //    && ((DailyProductivityPopUp)popup).ShowDialog() == true)
                    //{
                    //    HandleDailyPopUpResponse((DailyProductivityPopUp)popup);
                    //}
                    if (mode == SurveyMode.IntervalPopUp
                        && ((IntervalSelfEvaluationPopUp)popup).ShowDialog() == true)
                    {
                        HandleIntervalPopUpResponse((IntervalSelfEvaluationPopUp)popup);
                    }
                    else
                    {
                        //when do we get here?
                        Database.GetInstance().LogErrorUnknown(Name);
                    }
                }));
            }
            catch (ThreadAbortException e) { Database.GetInstance().LogError(Name + ": " + e.Message); }
            catch (Exception e) { Database.GetInstance().LogError(Name + ": " + e.Message); }
        }

        /// <summary>
        /// handles the response to the interval popup
        /// </summary>
        /// <param name="popup"></param>
        private void HandleIntervalPopUpResponse(IntervalSelfEvaluationPopUp popup)
        {
            // user took the survey || user didn't work
            if (
                ((popup.UserSelectedEngagement >= 1 && popup.UserSelectedEngagement <= 7) || popup.UserSelectedEngagement == -1) &&
                ((popup.UserSelectedAttention >= 1 && popup.UserSelectedAttention <= 7) || popup.UserSelectedAttention == -1) )
            {
                SaveIntervalSurvey(popup);
               Database.GetInstance().LogInfo("The participant completed the self evaluation interval-survey.");
               Database.GetInstance().LogInfo("The participant didn't work when the self evaluation interval-survey was shown.");
            }
            // user postponed the survey
            else if (popup.PostPoneSurvey != PostPoneSurvey.None)
            {
                PostponeIntervalSurvey(popup);
                Database.GetInstance().LogInfo(string.Format(CultureInfo.InvariantCulture, "The participant postponed the interval-survey ({0}).", popup.PostPoneSurvey));
            }
            // something strange happened
            else
            {
                _currentSurveyEntry = null;
                _timeRemainingUntilNextSurvey = Settings.IntervalPostponeShortInterval;
            }
        }

        /// <summary>
        /// Saves the interval-survey results in the db & resets some items
        /// </summary>
        /// <param name="popup"></param>
        private void SaveIntervalSurvey(IntervalSelfEvaluationPopUp popup)
        {
            _timeRemainingUntilNextSurvey = PopUpIntervalInMins; // set new default interval

            _currentSurveyEntry.Engagement = popup.UserSelectedEngagement;
            _currentSurveyEntry.Attention = popup.UserSelectedAttention;
            _currentSurveyEntry.TimeStampFinished = DateTime.Now;
            Queries.SaveIntervalEntry(_currentSurveyEntry);
            _currentSurveyEntry = null; // reset
        }

        /// <summary>
        /// handler to postpone the survey for the selected time
        /// Hint: the selected time (e.g. postpone 1 hour) equals 1 hour of computer running (i.e. developer working) time
        /// </summary>
        /// <param name="notify"></param>
        private void PostponeIntervalSurvey(IntervalSelfEvaluationPopUp notify)
        {
            switch (notify.PostPoneSurvey)
            {
                case (PostPoneSurvey.Postpone1):
                    _timeRemainingUntilNextSurvey = Settings.IntervalPostponeShortInterval;  // set new interval
                    break;
                case (PostPoneSurvey.Postpone2):
                    _timeRemainingUntilNextSurvey = TimeSpan.FromHours(1); // in one hour
                    break;
                case (PostPoneSurvey.Postpone3):
                    _timeRemainingUntilNextSurvey = TimeSpan.FromHours(6); // in one workday
                    break;
                default:
                    _timeRemainingUntilNextSurvey = Settings.IntervalPostponeShortInterval;  // set new interval
                    break;
            }
        }

        /// <summary>
        /// manually run survey (click on ContextMenu)
        /// </summary>
        public void ManualTakeSurveyNow()
        {
            RunSurvey(SurveyMode.IntervalPopUp);
        }

        public override void CreateDatabaseTablesIfNotExist()
        {
            Queries.CreateSelfEvaluationTables();
        }

        public override bool IsEnabled()
        {
            return true; // currently, it is always enabled
        }

        public override void Start()
        {
            if (_timer != null)
                Stop();
            _timer = new DispatcherTimer();
            _timer.Interval = Settings.SurveyCheckerInterval;
            _timer.Tick += TimerTick;

            // only start timer if user wants it
            if (PopUpEnabled)
            {
                _timer.Start();
            }

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
            // no database updates necessary yet
        }

        public override List<IVisualization> GetVisualizationsDay(DateTimeOffset date)
        {
            var vis1 = new DaySelfEvaluationTimeLine(date);
            return new List<IVisualization> { vis1 };
        }
    }
}
