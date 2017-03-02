// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2015-10-20
// 
// Licensed under the MIT License.
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using UserInterruptibilityTracker.Models;

namespace UserInterruptibilityTracker
{
    public enum PostPoneSurvey
    {
        None,
        Skipped,
        Postpone1h,
        Postpone2h
    };

    /// <summary>
    /// This pop-up is shown in an interval
    /// 
    /// Interaction logic for IntervalProductivityPopUp.xaml
    /// </summary>
    public partial class InterruptibilityPopUp : Window
    {
        private InterruptibilitySurveyEntry _previousSurveyEntry;
        public int UserSelectedInterruptibility { get; set; }
        public PostPoneSurvey PostPoneSurvey { get; set; }
        private DispatcherTimer _closeIfNotAnsweredAfterHoursTimer;

        public InterruptibilityPopUp(InterruptibilitySurveyEntry previousSurveyEntry)
        {
            this.InitializeComponent();

            // set default values
            _previousSurveyEntry = previousSurveyEntry;

            if (_previousSurveyEntry != null && _previousSurveyEntry.TimeStampFinished  != null && // if available
                _previousSurveyEntry.TimeStampFinished.Day == DateTime.Now.Day) // only if it was answered today
            {
                var hint = string.Format(CultureInfo.InvariantCulture, "Last entry was: {0} {1}",
                    _previousSurveyEntry.TimeStampFinished.ToShortDateString(),
                    _previousSurveyEntry.TimeStampFinished.ToShortTimeString());

                if (_previousSurveyEntry.Interruptibility > 0 && _previousSurveyEntry.Interruptibility < 7)
                    hint += ", you answered: " + _previousSurveyEntry.Interruptibility;

                LastTimeFilledOut.Text = hint;
            }
        }

        /// <summary>
        /// override ShowDialog method to place it on the bottom right corner
        /// of the developer's screen
        /// </summary>
        /// <returns></returns>
        public new bool? ShowDialog()
        {
            const int windowWidth = 510; //this.ActualWidth;
            const int windowHeight = 295; //this.ActualHeight;

            this.Topmost = true;
            this.ShowActivated = false;
            this.ShowInTaskbar = false;
            this.ResizeMode = ResizeMode.NoResize;
            //this.Owner = Application.Current.MainWindow;
            
            this.Closed += this.IntervalProductivityPopUp_OnClosed;

            this.Left = SystemParameters.PrimaryScreenWidth - windowWidth;
            var top = SystemParameters.PrimaryScreenHeight - windowHeight;

            foreach (Window window in Application.Current.Windows)
            {
                var windowName = window.GetType().Name;

                if (!windowName.Equals("InterruptibilityPopUp") || window == this) continue;
                window.Topmost = true;
                top = window.Top - windowHeight;
            }

            this.Top = top;
            return base.ShowDialog();
        }

        /// <summary>
        /// todo: unsure if still needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntervalProductivityPopUp_OnClosed(object sender, EventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                var windowName = window.GetType().Name;

                if (!windowName.Equals("InterruptibilityPopUp") || window == this) continue;

                // Adjust any windows that were above this one to drop down
                if (window.Top < this.Top)
                {
                    window.Top = window.Top + this.ActualHeight;
                }
            }
        }

        /// <summary>
        /// If the user uses shortcuts to escape or fill out the survey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Postpone2hClicked(null, null);
            }
            else if (e.Key == Key.D5)
            {
                UserFinishedSurvey(5);
            }
            else if (e.Key == Key.D4)
            {
                UserFinishedSurvey(4);
            }
            else if (e.Key == Key.D3)
            {
                UserFinishedSurvey(3);
            }
            else if (e.Key == Key.D2)
            {
                UserFinishedSurvey(2);
            }
            else if (e.Key == Key.D1)
            {
                UserFinishedSurvey(1);
            }
            // else: do nothing
        }

        /// <summary>
        /// Close the pop-up and save the value.
        /// </summary>
        /// <param name="selectedProductivityValue"></param>
        private void UserFinishedSurvey(int selectedProductivityValue)
        {
            // reset timer
            if (_closeIfNotAnsweredAfterHoursTimer != null)
            {
                _closeIfNotAnsweredAfterHoursTimer.Stop();
                _closeIfNotAnsweredAfterHoursTimer = null;
            }

            // set responses
            UserSelectedInterruptibility = selectedProductivityValue;
            PostPoneSurvey = PostPoneSurvey.None;

            // close window
            try
            {
                DialogResult = true;
            }
            catch { } // sometimes crashes unexpectedly
            this.Close(); // todo: enable
        }

        #region Productivity Radio Button

        private void Interruptibility5_Checked(object sender, RoutedEventArgs e)
        {
            UserFinishedSurvey(5);
        }

        private void Interruptibility4_Checked(object sender, RoutedEventArgs e)
        {
            UserFinishedSurvey(4);
        }

        private void Interruptibility3_Checked(object sender, RoutedEventArgs e)
        {
            UserFinishedSurvey(3);
        }

        private void Interruptibility2_Checked(object sender, RoutedEventArgs e)
        {
            UserFinishedSurvey(2);
        }

        private void Interruptibility1_Checked(object sender, RoutedEventArgs e)
        {
            UserFinishedSurvey(1);
        }

        #endregion

        #region Button Events

        private void SkipClicked(object sender, RoutedEventArgs e)
        {
            UserSelectedInterruptibility = 0; // didn't take it
            PostPoneSurvey = PostPoneSurvey.Skipped;
            DialogResult = true;
            this.Close();
        }

        private void Postpone1hClicked(object sender, RoutedEventArgs e)
        {
            UserSelectedInterruptibility = 0; // didn't take it
            PostPoneSurvey = PostPoneSurvey.Postpone1h;
            DialogResult = true;
            this.Close();
        }

        private void Postpone2hClicked(object sender, RoutedEventArgs e)
        {
            UserSelectedInterruptibility = 0; // didn't take it
            PostPoneSurvey = PostPoneSurvey.Postpone2h;
            DialogResult = true;
            this.Close();
        }

        #endregion
    }
}