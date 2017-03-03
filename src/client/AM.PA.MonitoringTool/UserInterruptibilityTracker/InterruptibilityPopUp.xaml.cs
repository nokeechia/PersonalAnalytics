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
        public int UserSelectedInterruptibility { get; set; }
        public PostPoneSurvey PostPoneSurvey { get; set; }

        public InterruptibilityPopUp()
        {
            this.InitializeComponent();
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
            
            this.Closed += this.InterruptibilityPopUp_OnClosed;

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
        private void InterruptibilityPopUp_OnClosed(object sender, EventArgs e)
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
                SkipClicked(null, null);
            }
            else if (e.Key == Key.D7)
            {
                UserFinishedSurvey(7);
            }
            else if (e.Key == Key.D6)
            {
                UserFinishedSurvey(6);
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
        /// <param name="selectedInterruptibilityValue"></param>
        private void UserFinishedSurvey(int selectedInterruptibilityValue)
        {
            // set responses
            UserSelectedInterruptibility = selectedInterruptibilityValue;
            PostPoneSurvey = PostPoneSurvey.None;

            // close window
            try
            {
                DialogResult = true;
            }
            catch { } // sometimes crashes unexpectedly
            this.Close(); // todo: enable
        }

        #region Interruptibility Radio Button

        private void Interruptibility7_Checked(object sender, RoutedEventArgs e)
        {
            UserFinishedSurvey(7);
        }

        private void Interruptibility6_Checked(object sender, RoutedEventArgs e)
        {
            UserFinishedSurvey(6);
        }

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