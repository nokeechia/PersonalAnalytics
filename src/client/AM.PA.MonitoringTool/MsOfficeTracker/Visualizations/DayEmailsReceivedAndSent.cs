// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2015-12-11
// 
// Licensed under the MIT License.
//
// Modified by Paige Rodeghero (paige.rodeghero@us.abb.com) from ABB USCRC

using Shared;
using Shared.Helpers;
using System;
using MsOfficeTracker.Data;
using System.Globalization;

namespace MsOfficeTracker.Visualizations
{
    internal class DayEmailsReceivedAndSent : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public DayEmailsReceivedAndSent(DateTimeOffset date)
        {
            this._date = date;

            Title = "Received Emails vs.<br />Number Emails Sent";
            IsEnabled = false; //todo: handle by user
            Order = 4; //todo: handle by user
            Size = VisSize.Small;
            Type = VisType.Day;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////

            // get the latest stored email entry
            var emailsSentResult = Queries.GetSentOrReceivedEmails(_date, "sent");
            var emailsSent = emailsSentResult.Item2;
            var inboxReceivedResult = Queries.GetSentOrReceivedEmails(_date, "received");
            var emailsReceived = inboxReceivedResult.Item2;

            var isToday = (_date.Date == DateTime.Now.Date);
            var lastUpdatedMinsAgo = Math.Abs((DateTime.Now - emailsSentResult.Item1).TotalMinutes);

            // if database entry is outdated or not there, create a live API call and override entries
            if ((emailsSentResult.Item1 == DateTime.MinValue || inboxReceivedResult.Item1 == DateTime.MinValue) || // no emails stored yet
                (isToday && lastUpdatedMinsAgo > Settings.SaveEmailCountsIntervalInMinutes) || // request is for today and saved results are too old
                (emailsSent == -1 || emailsReceived == -1)) // could not fetch sent/received emails
            {
                // create and save a new email snapshot (inbox, sent, received)
                var res = Queries.CreateEmailsSnapshot(_date.Date, false);
                emailsSent = res.Item1;
                emailsReceived = res.Item2; 
            }

            // error (only if no data at all)
            if (emailsReceived == -1 && emailsSent == -1)
            {
                return VisHelper.NotEnoughData(Dict.NotEnoughData);
            }

            // no emails sent/received
            if (emailsReceived == 0 && emailsSent == 0)
            {
                return VisHelper.NotEnoughData("You didn't receive or send any emails today.");
            }

            // as a goodie get this too :)
            //var timeSpentInOutlook = Queries.TimeSpentInOutlook(_date);

            /////////////////////
            // HTML
            /////////////////////

            var emailInboxString = (emailsReceived == -1) ? "?" : emailsReceived.ToString(CultureInfo.InvariantCulture);
            var emailsSentString = (emailsSent == -1) ? "?" : emailsSent.ToString(CultureInfo.InvariantCulture);

            html += "<p style='text-align: center; margin-top:-0.7em;'><strong style='font-size:2.7em;'>" + emailInboxString + " | " + emailsSentString + "</strong></p>";

            //if (timeSpentInOutlook > 1)
            //{
            //    html += "<p style='text-align: center; margin-top:-0.7em;'>time spent in Outlook: " + Math.Round(timeSpentInOutlook, 0) + "min</p>";
            //}

            return html;
        }
    }
}
