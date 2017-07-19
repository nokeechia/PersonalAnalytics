// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2016-02-14
// 
// Licensed under the MIT License.

using System.Windows;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System;
using Shared;
using Shared.Helpers;
using System.Globalization;

namespace Retrospection
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ObjectForScriptingHelper
    {
        public void JS_SendFeedback()
        {
            Handler.GetInstance().SendFeedback();
        }

        public void JS_ThumbsVote(string voteType, string chartTitle, string typeString, string dateString)
        {
            try
            {
                var type = (VisType)Enum.Parse(typeof(VisType), typeString);
                var date = DateTimeOffset.Parse(dateString, CultureInfo.InvariantCulture);

                var up = (voteType == "up") ? 1 : 0;
                var down = (voteType == "down") ? 1 : 0;

                FeedbackThumbs.GetInstance().SetFeedback(type, date, chartTitle, up, down);
            }
            catch { }
        }

        #region Events from Pomodoro Visualizations

        public delegate void OnPomodoroTimerStarted();
        public static event OnPomodoroTimerStarted PomodoroTimerStarted;
        public delegate void OnPomodoroTimerPaused();
        public static event OnPomodoroTimerPaused PomodoroTimerPaused;
        public delegate void OnPomodoroTimerStopped();
        public static event OnPomodoroTimerStopped PomodoroTimerStopped;
        public delegate void OnPomodoroTimerCompleted();
        public static event OnPomodoroTimerCompleted PomodoroTimerCompleted;

        public static void RemoveAllPomodoroSubscriptions()
        {
            PomodoroTimerStarted = null;
            PomodoroTimerPaused = null;
            PomodoroTimerStopped = null;
            PomodoroTimerCompleted = null;
        }

        public void JS_PomodoroTimerStarted()
        {
            PomodoroTimerStarted?.Invoke();
        }

        public void JS_PomodoroTimerPaused()
        {
            PomodoroTimerPaused?.Invoke();
        }

        public void JS_PomodoroTimerStopped()
        {
            PomodoroTimerStopped?.Invoke();
        }

        public void JS_PomodoroTimerCompleted()
        {
            PomodoroTimerCompleted?.Invoke();
        }

        #endregion
    }
}
