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

            Title = "Attention & Engagement<br/>Today vs. Yesterday";
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

            var msgBlinks = CreateMessageIfNoValues(avgBlinks, avgBlinkReference, MuseMetric.Attention);
            var msgEEG = CreateMessageIfNoValues(avgEEG, avgEEGReference, MuseMetric.Engagement);

            if (msgBlinks.Length > 0) html += msgBlinks;
            else
            {
                var insightBlinks = InsightBuilder(avgBlinks, avgBlinkReference, MuseDataType.Blinks);
                html += "<p style='text-align: center; margin-top:-0.7em; margin-bottom:-0.7em;'>" + insightBlinks + "</p>";
            }


            if (msgEEG.Length > 0) html += msgEEG; else 
            {
                var insightEEG = InsightBuilder(avgEEG, avgEEGReference, MuseDataType.EEG);
                if (html.Contains("lower") && insightEEG.Contains("lower") || html.Contains("higher") && insightEEG.Contains("higher"))
                {
                    html += "<p style='text-align: center; margin-top:-0.7em; margin-bottom:-0.7em;'>" + " and " + "</p>";
                }
                else {
                    html += "<p style='text-align: center; margin-top:0.4em; margin-bottom:0.4em;'>" + " but " + "</p>";

                }
                html += "<p style='text-align: center; margin-top:-0.7em; margin-bottom:-0.7em;'>" + insightEEG + "</p>";            
        }


            return html;
        }

        private String CreateMessageIfNoValues(double value, double referenceValue, MuseMetric m)
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

            if (msg.Length > 0) {
                return "<p style = 'text-align: center; margin-top:-0.7em; font-size: 0.66em;' >" + m.ToString() + ": " + msg + "</p>";
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
            return "Your average <strong style='color:" + metricColor + ";'>" + metric.ToString().ToLower() + "</strong> is <strong style='color:" + trendColor + ";'>" + adj + "</strong> " + prep + " yesterday";
        }
    }
}
