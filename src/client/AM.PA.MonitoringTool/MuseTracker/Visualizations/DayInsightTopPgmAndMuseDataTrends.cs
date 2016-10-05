using System;
using Shared;
using MuseTracker.Data;
using MuseTracker.Models;
using UserEfficiencyTracker.Data;
using Shared.Helpers;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace MuseTracker.Visualizations
{

    internal class DayInsightTopPgmAndMuseDataTrends : BaseVisualization, IVisualization
    {
        private int maxNumberOfTopPrograms = 7;
        private readonly DateTimeOffset _date;
        private VisType _type;
        private string _notEnoughPgmsMsg = "It is not possible to give you insights because no used programs were found.";
        private string _notEnoughMuseMsg = "It is not possible to give you insights because no Muse data was found.";

        public DayInsightTopPgmAndMuseDataTrends(DateTimeOffset date)
        {
            this._date = date;

            Title = "Attention & Engagement per Top Used Programs ";
            this._date = date;
            this._type = VisType.Day;
            IsEnabled = true; 
            Order = 9; //todo: handle by user
            Size = VisSize.Square;
    }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var programsUsed = UserEfficiencyTracker.Data.Queries.GetTopProgramsUsedWithTimes(_date, _type, maxNumberOfTopPrograms);
            if (programsUsed.Count < 1)
            {
                html += VisHelper.NotEnoughData(_notEnoughPgmsMsg);
                return html;
            }

            /////////////////////
            // prepare data sets
            /////////////////////

            var avgEEGPerPgm = new List<Tuple<String, double>>();
            var avgBlinksPerPgm = new List<Tuple<String, double>>();

            foreach (KeyValuePair<string, List<TopProgramTimeDto>> entry in programsUsed)
            {
                //processing for eeg data
                var eegIndicesList = new List<double>();
                var eegIndices = Data.Queries.GetEEGIndexWithinTimerange(_date.Date, entry.Value);
                if (eegIndices.Count > 0) eegIndicesList.Add(eegIndices.Average(item => item.Item2));
                if(eegIndicesList.Count > 0) avgEEGPerPgm.Add(new Tuple<String, double>(entry.Key, eegIndicesList.Average(i => i)));

                //processing for blink data
                var blinks = Data.Queries.GetBlinksWithinTimerange(_date.Date, entry.Value);
                if (blinks > 0) avgBlinksPerPgm.Add(new Tuple<String, double>(entry.Key, blinks));
            }

            if (avgEEGPerPgm.Count < 1 || avgBlinksPerPgm.Count < 1)
            {
                html += VisHelper.NotEnoughData(_notEnoughPgmsMsg);
                return html;
            }

            var eegValuesWithTimes = Data.Queries.GetEEGIndex(_date);
            var eegValuesOnly = eegValuesWithTimes.Select(item => item.Item2).ToList();
            var eegStdev = Helpers.Helper.StdDev(eegValuesOnly);
            var eegAvg = Math.Round(eegValuesOnly.Average(), 2);

            var blinkValuesWithTimes = Data.Queries.GetBlinks(_date);
            var blinkValuesOnly = blinkValuesWithTimes.Select(item => item.Item2).ToList();
            var blinkStdev = Helpers.Helper.StdDev(eegValuesOnly);
            var blinkAvg = Math.Round(blinkValuesOnly.Average(), 2);

            /////////////////////
            // visualize data sets
            /////////////////////

            // create blank table
            html += string.Format(CultureInfo.InvariantCulture, "<table id='{0}'>", VisHelper.CreateChartHtmlTitle(Title));
            html += "<thead><tr><th>Program</th><th>Attention</th><th>Engagement</th></tr></thead>";
            html += "<tbody>";


            foreach (var p in avgEEGPerPgm)
            {
                html += "<tr>";
                html += "<td>" + ProcessNameHelper.GetFileDescription(p.Item1) + "</td>";

                var blink = avgBlinksPerPgm.Find(x => x.Item1.Equals(p.Item1)).Item2;
                var percBlink = ((blink / blinkAvg) - 1) * 100;

                //if blink difference is equal or greater 2*Stdev then show, otherwise difference is not significant (assumption)
                if (Math.Abs(blinkAvg - blink) >= 2 * blinkStdev)
                {
                    //blinks are reverse -> better attention when less blinks
                    if (percBlink > 0)
                    {
                        html += "<td style='color:red;'>" + "<strong>&#9730; </strong>" + "-" + Math.Round(percBlink) + "%</td>";
                    }
                    else if (percBlink < 0)
                    {
                        html += "<td style='color:green;'>" + "<strong>&#9728; </strong> +" + -1 * Math.Round(percBlink) + "%</td>";
                    }
                    else
                    {
                        html += "<td>" + "<strong>&#9729; </strong>" + Math.Round(percBlink) + "%</td>";
                    }
                }
                else {
                    html += "<td>" + "<strong> - </td>";
                }


                var eeg = Math.Round(p.Item2, 2);
                var percEEG = ((eeg / eegAvg) - 1) * 100;

                //if eeg difference is equal or greater 2*Stdev then show, otherwise difference is not significant (assumption)
                if (Math.Abs(eegAvg - eeg) >= 2 * eegStdev)
                {
                    if (percEEG > 0)
                    {
                        html += "<td style='color:green;'>" + "<strong>&#9728; </strong>" + "+" + Math.Round(percEEG) + "%</td>";
                    }
                    else if (percEEG < 0)
                    {
                        html += "<td style='color:red;'>" + "<strong>&#9730; </strong>" + Math.Round(percEEG) + "%</td>";
                    }
                    else
                    {
                        html += "<td>" + "<strong>&#9729; </strong>" + Math.Round(percEEG) + "%</td>";
                    }
                }
                else
                {
                    html += "<td>" + "<strong> - </td>";
                }


                html += "</tr>";
            }
            html += "</tbody>";
            html += "<p style='text-align: center; font-size: 0.66em;'>Hint: Presents the daily attention and engagement when using the top used programs and compares them with averages.</p>";

            html += "</table>";


            /////////////////////
            // create & add javascript
            ////////////////////
            var js = "<script type='text/javascript'>"
                    + "var tf = new TableFilter('" + VisHelper.CreateChartHtmlTitle(Title) + "', { base_path: '/', "
                    + "col_widths:[ '12.5em', '6.25em', '6.25em'], " // fixed columns sizes
                    + "col_0: 'none', col_1: 'none', col_2: 'none', "
                    + "alternate_rows: true, " // styling options
                    + "grid_layout: true, grid_cont_css_class: 'grd-main-cont', grid_tblHead_cont_css_class: 'grd-head-cont', " // styling & behavior of the table                                                         
                                                                                                                                //+ "extensions: [{name: 'sort', types: [ 'string', 'number', 'number'] }], "
                    + "}); " // no content options
                    + "tf.init(); "
                    + "</script>";

            html += " " + js;
            return html;
        }

    }
}
