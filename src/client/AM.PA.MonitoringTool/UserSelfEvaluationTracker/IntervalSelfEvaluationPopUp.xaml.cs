using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UserSelfEvaluationTracker.Models;

namespace UserSelfEvaluationTracker
{

    public enum PostPoneSurvey
    {
        None,
        Postpone1,
        Postpone2,
        Postpone3
    };

    /// <summary>
    /// This pop-up is shown in an interval
    /// 
    /// Interaction logic for IntervalSelfEvaluationPopUp.xaml
    /// </summary>
    public partial class IntervalSelfEvaluationPopUp : Window
    {
        private SelfEvaluationSurveyEntry _previousSurveyEntry;
        public int UserSelectedEngagement { get; set; }
        public int UserSelectedAttention { get; set; }
        public PostPoneSurvey PostPoneSurvey { get; set; }
        private DispatcherTimer _closeIfNotAnsweredAfterHoursTimer;

        private int _engagement = 0;
        private int _attention = 0;
        private TextBlock fieldErrorMsg;

        public IntervalSelfEvaluationPopUp(SelfEvaluationSurveyEntry previousSurveyEntry)
        {
            this.InitializeComponent();

            // set default values
            _previousSurveyEntry = previousSurveyEntry;

            if (_previousSurveyEntry != null && _previousSurveyEntry.TimeStampFinished != null && // if available
                _previousSurveyEntry.TimeStampFinished.Day == DateTime.Now.Day) // only if it was answered today
            {
                var hint = string.Format(CultureInfo.InvariantCulture, "Last entry was: {0} {1}",
                    _previousSurveyEntry.TimeStampFinished.ToShortDateString(),
                    _previousSurveyEntry.TimeStampFinished.ToShortTimeString());

                if (_previousSurveyEntry.Engagement > 0 && _previousSurveyEntry.Engagement < 7)
                    hint += ", engagement: " + _previousSurveyEntry.Engagement;

                if (_previousSurveyEntry.Attention > 0 && _previousSurveyEntry.Attention < 7)
                    hint += ", attention: " + _previousSurveyEntry.Attention;

                LastTimeFilledOut.Text = hint;
            }

            // start timer to close if not responded within a few hours
            _closeIfNotAnsweredAfterHoursTimer = new DispatcherTimer();
            _closeIfNotAnsweredAfterHoursTimer.Interval = Settings.IntervalCloseIfNotAnsweredInterval;
            _closeIfNotAnsweredAfterHoursTimer.Tick += NotAnsweredAfterHours;
            _closeIfNotAnsweredAfterHoursTimer.Start();
        }

        /// <summary>
        /// closes the survey pop-up if the user didn't fill out the survey 
        /// after x hours
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotAnsweredAfterHours(object sender, EventArgs e)
        {
            UserFinishedSurvey(-1, -1); // user didn't work
        }

        /// <summary>
        /// override ShowDialog method to place it on the bottom right corner
        /// of the developer's screen
        /// </summary>
        /// <returns></returns>
        public new bool? ShowDialog()
        {
            const int windowWidth =  510; 
            const int windowHeight = 500; 

            this.Topmost = true;
            this.ShowActivated = false;
            this.ShowInTaskbar = false;
            this.ResizeMode = ResizeMode.NoResize;
            //this.Owner = Application.Current.MainWindow;

            this.Closed += this.IntervalSelfEvaluationPopUp_OnClosed;

            this.Left = SystemParameters.PrimaryScreenWidth - windowWidth;
            var top = SystemParameters.PrimaryScreenHeight - windowHeight;

            foreach (Window window in Application.Current.Windows)
            {
                var windowName = window.GetType().Name;

                if (!windowName.Equals("IntervalSelfEvaluationPopUp") || window == this) continue;
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
        private void IntervalSelfEvaluationPopUp_OnClosed(object sender, EventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                var windowName = window.GetType().Name;

                if (!windowName.Equals("IntervalSelfEvaluationPopUp") || window == this) continue;

                // Adjust any windows that were above this one to drop down
                if (window.Top < this.Top)
                {
                    window.Top = window.Top + this.ActualHeight;
                }
            }
        }

        /// <summary>
        /// Close the pop-up and save the value.
        /// </summary>
        /// <param name="selectedEngagementValue"></param>
        /// <param name="selectedAttentionValue"></param>
        private void UserFinishedSurvey(int selectedEngagementValue, int selectedAttentionValue)
        {
            if (selectedEngagementValue != 0 && selectedAttentionValue != 0)
            {
                // reset timer
                _closeIfNotAnsweredAfterHoursTimer.Stop();
                _closeIfNotAnsweredAfterHoursTimer = null;

                // set responses
                UserSelectedEngagement = selectedEngagementValue;
                UserSelectedAttention = selectedAttentionValue;
                PostPoneSurvey = PostPoneSurvey.None;

                fieldErrorMsg = (TextBlock)this.FindName("Errormessage");
                fieldErrorMsg.Visibility = System.Windows.Visibility.Hidden;

                // close window
                DialogResult = true;
                this.Close();
            }
            else {
                fieldErrorMsg = (TextBlock)this.FindName("Errormessage");
                fieldErrorMsg.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void ShowErrorMessage() {
            fieldErrorMsg = (TextBlock)this.FindName("Errormessage");
            fieldErrorMsg.Visibility = System.Windows.Visibility.Visible;
        }

        private void Finishing(int engagement, int attention) {
            _attention = attention;
            _engagement = engagement;
            if (attention == 0 || engagement == 0)
            {
                ShowErrorMessage();
            }
            else {
                UserFinishedSurvey(engagement, attention);
            }
        }
        #region Engagement Radio Button

        private void Engagement7_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(7, _attention);
        }

        private void Engagement6_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(6, _attention);
        }

        private void Engagement5_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(5, _attention);
        }

        private void Engagement4_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(4, _attention);
        }

        private void Engagement3_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(3, _attention);
        }

        private void Engagement2_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(2, _attention);
        }

        private void Engagement1_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(1, _attention);
        }

        #endregion

        #region Attention Radio Button

        private void Attention7_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(_engagement, 7);
        }

        private void Attention6_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(_engagement, 6);
        }

        private void Attention5_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(_engagement, 5);
        }

        private void Attention4_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(_engagement, 4);
        }

        private void Attention3_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(_engagement, 3);
        }

        private void Attention2_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(_engagement, 2);
        }

        private void Attention1_Checked(object sender, RoutedEventArgs e)
        {
            Finishing(_engagement, 1);
        }
        #endregion
        #region Button Events

        private void Postpone0Clicked(object sender, RoutedEventArgs e)
        {
            UserFinishedSurvey(-1, -1); // user didn't work
        }

        private void Postpone1Clicked(object sender, RoutedEventArgs e)
        {
            UserSelectedEngagement = -1; // didn't take it
            UserSelectedAttention = -1; // didn't take it

            PostPoneSurvey = PostPoneSurvey.Postpone1;
            DialogResult = true;
            this.Close();
        }

        private void Postpone2Clicked(object sender, RoutedEventArgs e)
        {
            UserSelectedEngagement = -1; // didn't take it
            UserSelectedAttention = -1; // didn't take it

            PostPoneSurvey = PostPoneSurvey.Postpone2;
            DialogResult = true;
            this.Close();
        }

        private void Postpone3Clicked(object sender, RoutedEventArgs e)
        {
            UserSelectedEngagement = -1; // didn't take it
            UserSelectedAttention = -1; // didn't take it

            PostPoneSurvey = PostPoneSurvey.Postpone3;
            DialogResult = true;
            this.Close();
        }

        #endregion
    }
}
