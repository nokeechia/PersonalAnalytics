using System;
using Shared;
using Shared.Helpers;
using MuseTracker.Data;
using MuseTracker.Models;
using System.Linq;
using UserEfficiencyTracker.Data;
using System.Collections.Generic;

namespace MuseTracker.Visualizations
{
    internal class InsightAttentionOfDay: BaseVisualization, IVisualization
    {

        private readonly DateTimeOffset _date;
        private string _notEnoughMuseMsg = "It is not possible to give you insights because no Muse data was found.";

        public InsightAttentionOfDay(DateTimeOffset date)
        {
            this._date = date;

            Title = "Insights into Attention and Engagement";
            IsEnabled = true;
            Order = 21;
            Size = VisSize.Square;
            Type = VisType.Day;


        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////

            /////////////////////
            // HTML
            /////////////////////

            var eegValuesWithTimes = Data.Queries.GetEEGIndex(_date);
            var blinkWithTimes = Data.Queries.GetBlinks(_date);

            if (eegValuesWithTimes.Count < 1)
            {
                html += VisHelper.NotEnoughData(_notEnoughMuseMsg);
                return html;
            }

            var minEEGItem = eegValuesWithTimes.OrderBy(item => item.Item2).First();
            var maxEEGItem = eegValuesWithTimes.OrderByDescending(item => item.Item2).First();

            var minBlinkItem = blinkWithTimes.OrderByDescending(item => item.Item2).First();
            var maxBlinkItem = blinkWithTimes.OrderBy(item => item.Item2).First();

            //nicht auf minuten ebene sonder auf Hour. !! --> Problem bekomme keine Resultate
            var allTopPgmsOfTheDay = UserEfficiencyTracker.Data.Queries.GetTopProgramsUsedWithTimes(_date, VisType.Day, 7);

            //calculate user input metrics
            var userInput = UserInputTracker.Data.Queries.GetUserInputTimelineData(_date);
            var userInputAvg = userInput.Average(i => i.Value);
            var preposition = "";
            var dtFrom = maxEEGItem.Item1.AddMinutes(-5);
            var dtTo = maxEEGItem.Item1.AddMinutes(5);
            var maxPgmInThisTime = findTopUsedPgmInThisTime(allTopPgmsOfTheDay, dtFrom, dtTo);
            var maxEEGUserInput = userInput.Where(i => i.Key <= dtTo && i.Key >= dtFrom).OrderByDescending(i => i.Value).First();
            var topPgm = maxPgmInThisTime.Item2 > 0 ? ProcessNameHelper.GetFileDescription(maxPgmInThisTime.Item1) : "?";
            preposition = getPreposition(maxEEGUserInput.Value, userInputAvg); 

            html += "<p style='text-align: center; margin-top:-0.7em;'>You had the <strong style='color:#007acc;'>highest Engagement</strong> today at <strong style='color:#007acc;'>" + maxEEGItem.Item1.ToString("HH:mm tt") + "</strong> while you were working in program " + topPgm + " with a user input, " + preposition + " average.</p>";

            //processing for min engagement 
            dtFrom = minEEGItem.Item1.AddMinutes(-5);
            dtTo = minEEGItem.Item1.AddMinutes(5);
            maxPgmInThisTime = findTopUsedPgmInThisTime(allTopPgmsOfTheDay, dtFrom, dtTo);
            maxEEGUserInput = userInput.Where(i => i.Key <= dtTo && i.Key >= dtFrom).OrderByDescending(i => i.Value).First();
            topPgm = maxPgmInThisTime.Item2 > 0 ? ProcessNameHelper.GetFileDescription(maxPgmInThisTime.Item1) : "?";

            html += "<p style='text-align: center; margin-top:-0.7em;'>You had the <strong style='color:#007acc;'>lowest Engagement</strong> today at <strong style='color:#007acc;'>" + minEEGItem.Item1.ToString("HH:mm tt") + "</strong> while you were working in program " + topPgm + " with a user input, " + preposition + " average.</p>";

            //processing for min attention 
            dtFrom = minBlinkItem.Item1.AddMinutes(-5);
            dtTo = minBlinkItem.Item1.AddMinutes(5);
            maxPgmInThisTime = findTopUsedPgmInThisTime(allTopPgmsOfTheDay, dtFrom, dtTo);
            maxEEGUserInput = userInput.Where(i => i.Key <= dtTo && i.Key >= dtFrom).OrderByDescending(i => i.Value).First();
            topPgm = maxPgmInThisTime.Item2 > 0 ? ProcessNameHelper.GetFileDescription(maxPgmInThisTime.Item1) : "?";

            html += "<p style='text-align: center; margin-top:-0.7em;'>You had the <strong style='color:#007acc;'>lowest Attention</strong> today at <strong style='color:#007acc;'>" + minBlinkItem.Item1.ToString("HH:mm tt") + "</strong> while you were working in program " + topPgm + " with a user input, " + preposition + " average.</p>";

            //processing for max attention 
            dtFrom = maxBlinkItem.Item1.AddMinutes(-5);
            dtTo = maxBlinkItem.Item1.AddMinutes(5);
            maxPgmInThisTime = findTopUsedPgmInThisTime(allTopPgmsOfTheDay, dtFrom, dtTo);
            maxEEGUserInput = userInput.Where(i => i.Key <= dtTo && i.Key >= dtFrom).OrderByDescending(i => i.Value).First();
            topPgm = maxPgmInThisTime.Item2 > 0 ? ProcessNameHelper.GetFileDescription(maxPgmInThisTime.Item1) : "?";

            html += "<p style='text-align: center; margin-top:-0.7em;'>You had the <strong style='color:#007acc;'>highest Attention</strong> today at <strong style='color:#007acc;'>" + maxBlinkItem.Item1.ToString("HH:mm tt") + "</strong> while you were working in program " + topPgm + " with a user input, " + preposition + " average.</p>";


            return html;
        }

        public static String getPreposition(int val, double avg)
        {
            return (val > avg) ? "above" : (val == avg) ? "equal" : "below";
        }

        public static Tuple<String, int> findTopUsedPgmInThisTime(Dictionary<String, List<TopProgramTimeDto>> allTopPgmsOfTheDay, DateTime dtFrom, DateTime dtTo)
        {
            var pgmsInThisTimes = new List<Tuple<String, int>>();
            foreach (var p in allTopPgmsOfTheDay)
            {
                var process = p.Key;

                foreach (var dto in p.Value)
                {
                    if (dto.From >= dtFrom && dto.To <= dtTo)
                    {
                        pgmsInThisTimes.Add(new Tuple<String, int>(process, dto.DurInMins));
                    }
                }

            }

            var topPgmInThisTime = new Tuple<String, int>("", 0);

            if (pgmsInThisTimes.Count > 0)
            {
                topPgmInThisTime = pgmsInThisTimes.OrderByDescending(i => i.Item2).First();
            }

            return topPgmInThisTime;
        }
    }
}
