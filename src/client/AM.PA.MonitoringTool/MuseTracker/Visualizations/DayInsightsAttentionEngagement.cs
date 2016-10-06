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
    internal class DayInsightsAttentionEngagement: BaseVisualization, IVisualization
    {

        private readonly DateTimeOffset _date;
        private string _notEnoughMuseMsg = "It is not possible to give you insights because no Muse data was found.";

        public DayInsightsAttentionEngagement(DateTimeOffset date)
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

            var allTopPgmsOfTheDay = UserEfficiencyTracker.Data.Queries.GetTopProgramsUsedWithTimes(_date, VisType.Day, 7);

            //calculate user input metrics
            var userInput = UserInputTracker.Data.Queries.GetUserInputTimelineData(_date);
            var userInputAvg = userInput.Average(i => i.Value);
            var preposition = "";
            var dtFrom = maxEEGItem.Item1.AddMinutes(-5);
            var dtTo = maxEEGItem.Item1.AddMinutes(5);
            var maxPgmInThisTime = findTopUsedPgmInThisTime(allTopPgmsOfTheDay, dtFrom, dtTo);
            var maxInput = userInput.Where(i => i.Key <= dtTo && i.Key >= dtFrom).OrderByDescending(i => i.Value).First();
            var topPgm = maxPgmInThisTime.Item2 > 0 ? ProcessNameHelper.GetFileDescription(maxPgmInThisTime.Item1) : "?";
            preposition = getPreposition(maxInput.Value, userInputAvg); 

            html += "<p style='text-align: center; margin-top:-0.3em;font-size: 0.89em;'>You had the <strong style='color:#007acc;'>highest Engagement</strong> today at <strong style='color:#007acc;'>" + maxEEGItem.Item1.ToString("HH:mm tt") + "</strong> while you were working in program <strong style='color:#007acc;'>" + topPgm + "</strong> with a user input, <strong style='color:#007acc;'>" + preposition + "</strong> average.</p>";

            //processing for min engagement 
            dtFrom = minEEGItem.Item1.AddMinutes(-5);
            dtTo = minEEGItem.Item1.AddMinutes(5);
            maxPgmInThisTime = findTopUsedPgmInThisTime(allTopPgmsOfTheDay, dtFrom, dtTo);
            maxInput = userInput.Where(i => i.Key <= dtTo && i.Key >= dtFrom).OrderByDescending(i => i.Value).First();
            topPgm = maxPgmInThisTime.Item2 > 0 ? ProcessNameHelper.GetFileDescription(maxPgmInThisTime.Item1) : "?";
            preposition = getPreposition(maxInput.Value, userInputAvg);

            html += "<p style='text-align: center; margin-top:-0.3em;font-size: 0.89em;'>You had the <strong style='color:#007acc;'>lowest Engagement</strong> today at <strong style='color:#007acc;'>" + minEEGItem.Item1.ToString("HH:mm tt") + "</strong> while you were working in program <strong style='color:#007acc;'>" + topPgm + "</strong> with a user input, <strong style='color:#007acc;'>" + preposition + "</strong> average.</p>";

            //processing for min attention 
            dtFrom = minBlinkItem.Item1.AddMinutes(-5);
            dtTo = minBlinkItem.Item1.AddMinutes(5);
            maxPgmInThisTime = findTopUsedPgmInThisTime(allTopPgmsOfTheDay, dtFrom, dtTo);
            maxInput = userInput.Where(i => i.Key <= dtTo && i.Key >= dtFrom).OrderByDescending(i => i.Value).First();
            topPgm = maxPgmInThisTime.Item2 > 0 ? ProcessNameHelper.GetFileDescription(maxPgmInThisTime.Item1) : "?";
            preposition = getPreposition(maxInput.Value, userInputAvg);

            html += "<p style='text-align: center; margin-top:-0.3em;font-size: 0.89em;'>You had the <strong style='color:#007acc;'>lowest Attention</strong> today at <strong style='color:#007acc;'>" + minBlinkItem.Item1.ToString("HH:mm tt") + "</strong> while you were working in program <strong style='color:#007acc;'>" + topPgm + "</strong> with a user input, <strong style='color:#007acc;'>" + preposition + "</strong> average.</p>";

            //processing for max attention 
            dtFrom = maxBlinkItem.Item1.AddMinutes(-5);
            dtTo = maxBlinkItem.Item1.AddMinutes(5);
            maxPgmInThisTime = findTopUsedPgmInThisTime(allTopPgmsOfTheDay, dtFrom, dtTo);
            maxInput = userInput.Where(i => i.Key <= dtTo && i.Key >= dtFrom).OrderByDescending(i => i.Value).First();
            topPgm = maxPgmInThisTime.Item2 > 0 ? ProcessNameHelper.GetFileDescription(maxPgmInThisTime.Item1) : "?";
            preposition = getPreposition(maxInput.Value, userInputAvg);

            html += "<p style='text-align: center; margin-top:-0.3em;font-size: 0.89em;'>You had the <strong style='color:#007acc;'>highest Attention</strong> today at <strong style='color:#007acc;'>" + maxBlinkItem.Item1.ToString("HH:mm tt") + "</strong> while you were working in program <strong style='color:#007acc;'>" + topPgm + "</strong> with a user input, <strong style='color:#007acc;'>" + preposition + "</strong> average.</p>";

            html += InsightsComparedToYesterday(_date);
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


        private static String InsightsComparedToYesterday(DateTimeOffset _date)
        {
            var html = "";

            /////////////////////
            // fetch data sets
            /////////////////////
            var avgBlinks = MuseTracker.Data.Queries.GetAvgBlinksByDate(_date);
            var referenceDate = _date.AddDays(-1);
            var avgBlinkReference = MuseTracker.Data.Queries.GetAvgBlinksByDate(referenceDate);

            var avgEEG = MuseTracker.Data.Queries.GetAvgEEGIndexByDate(_date);
            var avgEEGReference = MuseTracker.Data.Queries.GetAvgEEGIndexByDate(referenceDate);

            /////////////////////
            // HTML
            /////////////////////

            var msgBlinks = CreateMessageIfNoValues(avgBlinks, avgBlinkReference, MuseMetric.Attention);
            var msgEEG = CreateMessageIfNoValues(avgEEG, avgEEGReference, MuseMetric.Engagement);

            if (msgBlinks.Length > 0) html += msgBlinks;
            else
            {
                var insightBlinks = InsightBuilder(avgBlinks, avgBlinkReference, MuseDataType.Blinks);
                html += "<p style='text-align: center; margin-top:-0.3em; margin-bottom:-0.7em;font-size: 0.89em;'>" + insightBlinks + "</p>";
            }


            if (msgEEG.Length > 0) html += msgEEG;
            else
            {
                var insightEEG = InsightBuilder(avgEEG, avgEEGReference, MuseDataType.EEG);
                if (html.Contains("lower") && insightEEG.Contains("lower") || html.Contains("higher") && insightEEG.Contains("higher"))
                {
                    html += "<p style='text-align: center; margin-top:-0.3em; margin-bottom:-0.7em;font-size: 0.89em;'>" + " and " + "</p>";
                }
                else
                {
                    html += "<p style='text-align: center; margin-top:0.4em; margin-bottom:0.4em;font-size: 0.89em;'>" + " but " + "</p>";

                }
                html += "<p style='text-align: center; margin-top:-0.3em; margin-bottom:-0.7em;font-size: 0.89em;'>" + insightEEG + "</p>";
            }


            return html;
        }

        private static String CreateMessageIfNoValues(double value, double referenceValue, MuseMetric m)
        {
            var msg = "";
            if (value == 0.0 && referenceValue == 0.0)
            {
                msg = "It is not possible to compare the dates because no data was found.";
            }
            else if (value == 0.0)
            {
                msg = "It is not possible to compare the dates because no data was found for today.";
            }
            else if (referenceValue == 0.0)
            {
                msg = "It is not possible to compare the dates because no data was found for yesterday.";
            }

            if (msg.Length > 0)
            {
                return "<p style = 'text-align: center; margin-top:-0.3em; font-size: 0.66em;' >" + m.ToString() + ": " + msg + "</p>";
            }
            return "";
        }

        private static String InsightBuilder(double value, double referenceValue, MuseDataType type)
        {
            var color = "#007acc";

            if (type == MuseDataType.Blinks)
            {
                //less blinks indicate more attention 
                if (value < referenceValue)
                {
                    return ConstructText(MuseMetric.Attention, "higher", "than", color, color);
                }
                else if (value > referenceValue)
                {
                    return ConstructText(MuseMetric.Attention, "lower", "than", color, color);
                }
                else if (value == referenceValue)
                {
                    return ConstructText(MuseMetric.Attention, "equal", "as", color, color);
                }

                return "No insights possible";
            }

            if (type == MuseDataType.EEG)
            {
                if (value < referenceValue)
                {
                    return ConstructText(MuseMetric.Engagement, "lower", "than", color, color);
                }
                else if (value > referenceValue)
                {
                    return ConstructText(MuseMetric.Engagement, "higher", "than", color, color);
                }
                else if (value == referenceValue)
                {
                    return ConstructText(MuseMetric.Engagement, "equal", "as", color, color);
                }

                return "No insights possible";
            }

            return "";
        }
        private static String ConstructText(MuseMetric metric, String adj, String prep, String metricColor, String trendColor)
        {
            return "Your average <strong style='color:" + metricColor + ";'>" + metric.ToString().ToLower() + "</strong> is <strong style='color:" + trendColor + ";'>" + adj + "</strong> " + prep + " yesterday";
        }
    }
}
