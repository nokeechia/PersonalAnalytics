// Created by Paige Rodeghero (paige.rodeghero@us.abb.com) from ABB USCRC
// Created: 2016-4-18
// 
// Licensed under the MIT License.

using Shared;
using Shared.Helpers;
using System;
using MsOfficeTracker.Data;
using System.Globalization;

namespace MsOfficeTracker.Visualizations
{
    internal class DayCallsReceivedAndSent : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;

        public DayCallsReceivedAndSent(DateTimeOffset date)
        {
            this._date = date;

            Title = "Received Calls vs.<br />Number Calls Sent";
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

            // get the latest stored call entry
            var callsSentResult = Queries.GetSentOrReceivedCalls(_date, "sent");
            var callsSent = callsSentResult.Item2;
            var inboxReceivedResult = Queries.GetSentOrReceivedCalls(_date, "received");
            var callsReceived = inboxReceivedResult.Item2;

            var isToday = (_date.Date == DateTime.Now.Date);
            var lastUpdatedMinsAgo = Math.Abs((DateTime.Now - callsSentResult.Item1).TotalMinutes);

            // if database entry is outdated or not there, create a live API call and override entries
            if ((callsSentResult.Item1 == DateTime.MinValue || inboxReceivedResult.Item1 == DateTime.MinValue) ||// no calls stored yet
                (isToday && lastUpdatedMinsAgo > Settings.SaveEmailCountsIntervalInMinutes) || // request is for today and saved results are too old
                (callsSent == -1 || callsReceived == -1)) // could not fetch sent/received calls
            {
                // create and save a new call snapshot (inbox, sent, received)
                var res = Queries.CreateChatsAndCallsSnapshot(_date.Date, false);
                callsSent = res.Item3;
                callsReceived = res.Item4;
            }

            // error (only if no data at all)
            if (callsReceived == -1 && callsSent == -1)
            {
                return VisHelper.NotEnoughData(Dict.NotEnoughData);
            }

            // no calls sent/received
            if (callsReceived == 0 && callsSent == 0)
            {
                return VisHelper.NotEnoughData("You didn't receive or send any calls today.");
            }

            // as a goodie get this too :)
            //var timeSpentInOutlook = Queries.TimeSpentInOutlook(_date);

            /////////////////////
            // HTML
            /////////////////////

            var callInboxString = (callsReceived == -1) ? "?" : callsReceived.ToString(CultureInfo.InvariantCulture);
            var callsSentString = (callsSent == -1) ? "?" : callsSent.ToString(CultureInfo.InvariantCulture);

            html += "<p style='text-align: center; margin-top:-0.7em;'><strong style='font-size:2.7em;'>" + callInboxString + " | " + callsSentString + "</strong></p>";

            //if (timeSpentInOutlook > 1)
            //{
            //    html += "<p style='text-align: center; margin-top:-0.7em;'>time spent in Outlook: " + Math.Round(timeSpentInOutlook, 0) + "min</p>";
            //}

            return html;
        }
    }
}
