// Created by Sebastian Müller at University of Zurich
// Created: 2016-12-20
// 
// Licensed under the MIT License. 

using Shared;
using Shared.Data;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Net.NetworkInformation;
using System.Linq;

namespace PersonalAnalytics.Upload
{
    public partial class UploadToDatabaseWizard : Window
    {
        Uploader uploader;
        private bool forceClose;
        private string userID;
        
        public UploadToDatabaseWizard()
        {
            InitializeComponent();
            uploader = new Uploader();
            StartStep1();
        }
        
        private void StartStep1()
        {
            Step1.Visibility = Visibility.Visible;
            PrePopulateUserUploadSettings();
            QuickUploadEnabled.IsEnabled = true;

            tbOneClickUploadSettingsTxt.Text = CreateOneClickUploadSettingsTxt(); 
        }

        private string CreateOneClickUploadSettingsTxt()
        {
            var obfuscateMeetingTitles = (RBObfuscateMeetingTitles.IsChecked.Value) ? "yes" : "no";
            var obfuscateWindowTitles = (RBObfuscateWindowTitles.IsChecked.Value) ? "yes" : "no";

            return String.Format("One-Click Upload enabled for User {0} (using previous settings; obfuscate meeting subjects = {1}, obfuscate window titles = {2}).", TbParticipantId.Text, obfuscateMeetingTitles, obfuscateWindowTitles);
        }

        private void PrePopulateUserUploadSettings()
        {
            TbParticipantId.Text = Database.GetInstance().GetSettingsString(Settings.ParticipantID, GenerateNewUserID());
            userID = TbParticipantId.Text;
            RBObfuscateMeetingTitles.IsChecked = Database.GetInstance().GetSettingsBool(Settings.ObfuscatedMeetingTitles, false);
            RBObfuscateWindowTitles.IsChecked = Database.GetInstance().GetSettingsBool(Settings.ObfuscatedWindowTitles, false);
        }

        private string GenerateNewUserID()
        {
            var macAddr = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                           where nic.OperationalStatus == OperationalStatus.Up
                           select nic.GetPhysicalAddress().ToString()
                          ).FirstOrDefault();
            return macAddr.ToString();
        }

        private async void QuickUploadNext_Clicked(object sender, EventArgs e)
        {
            Step1.Visibility = Visibility.Collapsed;
            
            var res = await Task.Run(() => uploader.RunQuickUploadToDatabase());
            if (res)
            {
                StartStep6();
            }
            else
            {
                CloseWindow();
            }
        }

        private void InsertInfosNext_Clicked(object sender, EventArgs e)
        {
            StartStep2();
        }

        private void StartStep2()
        {
            Step1.Visibility = Visibility.Collapsed;
            Step2.Visibility = Visibility.Visible;
        }

        private void SaveUploadUserDetailsToDb()
        {
            var obfuscateMeetingTitles = (RBObfuscateMeetingTitles.IsChecked.HasValue) ? RBObfuscateMeetingTitles.IsChecked.Value : false;
            Database.GetInstance().SetSettings(Settings.ObfuscatedMeetingTitles, obfuscateMeetingTitles);

            var obfuscateWindowTitles = (RBObfuscateWindowTitles.IsChecked.HasValue) ? RBObfuscateWindowTitles.IsChecked.Value : false;
            Database.GetInstance().SetSettings(Settings.ObfuscatedWindowTitles, obfuscateWindowTitles);
        }

        private void AnonymizedNext_Clicked(object sender, EventArgs e)
        {
            SaveUploadUserDetailsToDb();
            StartStep3();
        }

        private async void StartStep3()
        {
            Step2.Visibility = Visibility.Collapsed;
            Step3.Visibility = Visibility.Visible;

            var obfuscateMeetingTitles = (RBObfuscateMeetingTitles.IsChecked.HasValue) ? RBObfuscateMeetingTitles.IsChecked.Value : false;
            var obfuscateWindowTitles = (RBObfuscateWindowTitles.IsChecked.HasValue) ? RBObfuscateWindowTitles.IsChecked.Value : false;

            var anonymizedDbFilePath = await Task.Run(() => uploader.AnonymizeCollectedData(obfuscateMeetingTitles, obfuscateWindowTitles));
            if (string.IsNullOrEmpty(anonymizedDbFilePath))
            {
                CloseWindow(); // stop upload wizard if error occurred
                return;
            }
            
            StartStep4();
        }

        private void StartStep4()
        {
            Step3.Visibility = Visibility.Collapsed;
            Step4.Visibility = Visibility.Visible;
        }

        private void UploadNow_Clicked(object sender, EventArgs e)
        {
            StartStep5();
        }

        private async void StartStep5()
        {
            Step4.Visibility = Visibility.Collapsed;
            Step5.Visibility = Visibility.Visible;

            bool success = await Task.Run(() => uploader.UploadToDatabase());
            if (!success)
            {
                CloseWindow(); // stop upload wizard if error occurred
                return;
            }

            StartStep6();
        }

        private void StartStep6()
        {
            Step5.Visibility = Visibility.Collapsed;
            Step6.Visibility = Visibility.Visible;

            forceClose = true; // can now close the window without a prompt
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            if (forceClose)
            {
                Database.GetInstance().LogInfo("Upload Wizard closed.");
                base.OnClosing(e);
            }
            else
            {
                var res = MessageBox.Show("Are you sure you want to cancel the Upload Wizard? Please contact us in case you have any questions or troubles concerning the upload. Thank you!\n\nDo you want to cancel the upload?", "Upload Wizard: Cancel the Upload?", MessageBoxButton.YesNo);

                if (res == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    Database.GetInstance().LogInfo("Upload Wizard closed.");
                    base.OnClosing(e);
                }
            }
        }

        private void Feedback_Clicked(object sender, EventArgs e)
        {
            Retrospection.Handler.GetInstance().SendFeedback("Feedback Upload", "User ID: " + userID);
        }
        
        private void CloseUploadWizard_Click(object sender, EventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            forceClose = true;
            Database.GetInstance().LogInfo("Upload Wizard closed.");
            this.Close();
        }

    }
}