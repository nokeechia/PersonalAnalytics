using System;
using Shared;
using Shared.Helpers;
using MuseTracker.Data;
using MuseTracker.Models;

namespace MuseTracker.Visualizations
{
  internal class DayMuseInsightCompareYesterday : BaseVisualization, IVisualization
  {
        private readonly DateTimeOffset _date;

        public DayMuseInsightCompareYesterday(DateTimeOffset date)
        {
            this._date = date;

            Title = "Attention & Engagement<br/>today vs. yesterday";
            IsEnabled = true;
            Order = 21;
            Size = VisSize.Small;
            Type = VisType.Day;


    }

    public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var avgBlinks = Queries.GetAvgBlinksByDate(_date);
            var referenceDate = _date.AddDays(-1); 
            var avgBlinkReference = Queries.GetAvgBlinksByDate(referenceDate);

            var avgEEG = Queries.GetAvgEEGIndexByDate(_date);
            var avgEEGReference = Queries.GetAvgEEGIndexByDate(referenceDate);


            /////////////////////
            // HTML
            /////////////////////

            var msg = CreateMessageIfNoValues(avgBlinks, avgBlinkReference, MuseDataType.Blinks);
            if (msg.Length > 0) html += msg;
            else
            {
                var insightBlinks = InsightBuilder(avgBlinks, avgBlinkReference, MuseDataType.Blinks);
                html += "<p style='text-align: center; margin-top:-0.7em;'>" + insightBlinks + "</p>";
            }

            var msgEEG = CreateMessageIfNoValues(avgEEG, avgEEGReference, MuseDataType.EEG);

            if (msgEEG.Length > 0) html += msgEEG; else 
            {
                var insightBlinks = InsightBuilder(avgEEG, avgEEGReference, MuseDataType.EEG);
                html += "<p style='text-align: center; margin-top:-0.7em;'>" + insightBlinks + "</p>";
            }

            return html;
        }

        private String CreateMessageIfNoValues(double value, double referenceValue, MuseDataType type)
        {
            if (value == 0.0 && referenceValue == 0.0)
            {
                return VisHelper.NotEnoughData(type.ToString() + " : It is not possible to compare the dates because no data was found.");
            }
            else if (value == 0.0)
            {
                return VisHelper.NotEnoughData(type.ToString() + ": It is not possible to compare the dates because no data was found for today.");
            }
            else if (referenceValue == 0.0)
            {
                return VisHelper.NotEnoughData(type.ToString() + ": It is not possible to compare the dates because no data was found for yesterday.");
            }

            return "";
        }

        private String InsightBuilder(double value, double referenceValue, MuseDataType type) {
            if (type == MuseDataType.Blinks)
            {
                //less blinks indicate more attention 
                if (value < referenceValue)
                {
                    return ConstructText(MuseMetric.Attention, "higher", "than", Settings.attentionBlinkColor, Settings.positiveTrendColor);
                }
                else if (value > referenceValue)
                {
                    return ConstructText(MuseMetric.Attention, "lower", "than", Settings.attentionBlinkColor, Settings.negativeTrendColor);
                }
                else if (value == referenceValue)
                {
                    return ConstructText(MuseMetric.Attention, "equal", "as", Settings.attentionBlinkColor, Settings.neutralTrendColor);
                }

                return "No insights possible";
            }

            if (type == MuseDataType.EEG)
            {
                if (value < referenceValue)
                {
                    return ConstructText(MuseMetric.Engagement, "lower", "than", Settings.engagementEEGColor, Settings.negativeTrendColor);
                }
                else if (value > referenceValue)
                {
                    return ConstructText(MuseMetric.Engagement, "higher", "than", Settings.engagementEEGColor, Settings.positiveTrendColor);
                }
                else if (value == referenceValue)
                {
                    return ConstructText(MuseMetric.Engagement, "equal", "as", Settings.engagementEEGColor, Settings.neutralTrendColor);
                }

                return "No insights possible";
            }

            return "";
        }
        private String ConstructText(MuseMetric metric, String adj, String prep, String metricColor, String trendColor) {
            return "Your average <strong style='color:" + metricColor + ";'>" + metric.ToString().ToLower() + "</strong> is <strong style='font-size:1.25em; color:" + trendColor + ";'>" + adj + "</strong> " + prep + " yesterday";
        }
    }
}
