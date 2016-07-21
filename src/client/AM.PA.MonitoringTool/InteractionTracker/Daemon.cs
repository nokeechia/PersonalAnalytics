// Created by André Meyer at UZH and Paige Rodeghero (ABB)
// Created: 2016/04/26
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractionTracker.Visualizations;
using Shared;
using System.Windows.Threading;
using InteractionTracker.Data;

namespace InteractionTracker
{
    public class Daemon : BaseTracker, ITracker
    {
        #region ITracker Stuff

        public event EventHandler StatusChanged;

        public Daemon()
        {
            Name = "Interaction Tracker";
        }

        public override void Start()
        {
            IsRunning = true;
            OnStatusChanged(new EventArgs());
        }

        public override void Stop()
        {
            IsRunning = false;
            OnStatusChanged(new EventArgs());
        }

        protected virtual void OnStatusChanged(EventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }

        public override void CreateDatabaseTablesIfNotExist()
        {
            // no tables to create
        }

        public override bool IsEnabled()
        {
            return Settings.IsEnabled;
        }

        public override List<IVisualization> GetVisualizationsMini(DateTimeOffset date)
        {
            var vis = new MiniThisDayInteraction(date);
            return new List<IVisualization> { vis };
        }

        public override List<IVisualization> GetVisualizationsDay(DateTimeOffset date)
        {
            var vis1 = new DayInteractionStepChart();
            var vis2 = new DayThisDayInteraction(date);
            return new List<IVisualization> { vis1, vis2 };
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Gets the interaction data (meetings, emails sent/received and chats) and calculates
        /// if they are higher than the threshold (which is the average + 1 standard deviation)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool ThresholdReached(DateTime date, bool force=false)
        {
            var data = InteractionDataHelper.GetAllInteractionData(date, force);

            var thresholdReached = false;
            if (data.NumMeetingsNow >= data.AvgMeetingsPrevious + data.MeetingsSD) thresholdReached = true;
            else if (data.NumEmailsReceivedNow >= data.AvgEmailsReceivedPrevious + data.EmailsReceivedSD) thresholdReached = true;
            else if (data.NumEmailsSentNow >= data.AvgEmailsSentPrevious + data.EmailsSentSD) thresholdReached = true;
            else if (data.NumChatsNow >= data.AvgChatsPrevious + data.ChatsSD) thresholdReached = true;

            return thresholdReached;
        }

        #endregion
    }
}
