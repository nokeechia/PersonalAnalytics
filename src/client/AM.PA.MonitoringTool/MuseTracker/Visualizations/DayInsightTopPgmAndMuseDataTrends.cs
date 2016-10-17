using System;
using Shared;
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
            Order = 9;
            Size = VisSize.Square;
    }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var programsUsed = Queries.GetTopProgramsUsedWithTimes(_date, _type, maxNumberOfTopPrograms);

            if (programsUsed.Count < 1)
            {
                html += VisHelper.NotEnoughData(_notEnoughPgmsMsg);
                return html;
            }

            /////////////////////
            // prepare data sets
            /////////////////////

            var flatProgramStruct4EEG = new Dictionary<String, List<Tuple<DateTime, double>>>();
            var flatProgramStruct4Blinks = new Dictionary<String, List<Tuple<DateTime, double>>>();

            foreach (KeyValuePair<string, List<TopProgramTimeDto>> entry in programsUsed)
            {
                if (entry.Value.Count > 0)
                {
                    //processing for eeg data
                    var eegIndicesList = new List<double>();

                    var eegIndices = Data.Queries.GetEEGIndexWithinTimerange(_date.Date, entry.Value);
                    if (eegIndices.Count > 0) flatProgramStruct4EEG.Add(entry.Key, eegIndices);

                    //processing for blink data
                    var avgBlinks = Data.Queries.GetBlinksWithinTimerange(_date.Date, entry.Value);
                    if (avgBlinks.Count > 0) flatProgramStruct4Blinks.Add(entry.Key, avgBlinks);
                }
            }

            if (flatProgramStruct4EEG.Count < 1 || flatProgramStruct4Blinks.Count < 1 || programsUsed.Count < 1)
            {
                html += VisHelper.NotEnoughData(_notEnoughPgmsMsg);
                return html;
            }

            var eegDataWithWeights = Helper.HelperMethods.GetFilteredEntries(flatProgramStruct4EEG, programsUsed);
            var blinkDataWithWeights = Helper.HelperMethods.GetFilteredEntries(flatProgramStruct4Blinks, programsUsed);

            if (eegDataWithWeights.Count < 1 || blinkDataWithWeights.Count < 1)
            {
                html += VisHelper.NotEnoughData(_notEnoughPgmsMsg);
                return html;
            }

            var weightedEegAvg = Helper.HelperMethods.CalculateTotalWeightedAvg(eegDataWithWeights);
            var weightedBlinkAvg = Helper.HelperMethods.CalculateTotalWeightedAvg(blinkDataWithWeights);
            var weightedAvgEEGPerPgm = Helper.HelperMethods.CalculateWeightedAvgPerPgm(eegDataWithWeights);
            var weightedAvgBlinksPerPgm = Helper.HelperMethods.CalculateWeightedAvgPerPgm(blinkDataWithWeights);
                
            /////////////////////
            // visualize data sets
            /////////////////////

            if (weightedAvgEEGPerPgm.Count > 0 && weightedAvgBlinksPerPgm.Count > 0)
            {
                var diffToAvgPerProgram = Helper.HelperMethods.CalculateDiffToAvg(weightedAvgEEGPerPgm, weightedAvgBlinksPerPgm, weightedEegAvg, weightedBlinkAvg);
                html += CreateHtmlTable(diffToAvgPerProgram, Title);
            }

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

        private static String CreateHtmlTable(List<Tuple<String, double, double>> diffToAvgPerProgram, String title)
        {
            var html = "";
            // create blank table
            html += string.Format(CultureInfo.InvariantCulture, "<table id='{0}'>", VisHelper.CreateChartHtmlTitle(title));
            html += "<thead><tr><th>Program</th><th>Attention</th><th>Engagement</th></tr></thead>";
            html += "<tbody>";

            //filter only programs that have at least either blink or eeg differences > 5%
            var filteredPgmList = diffToAvgPerProgram.Where(x => Math.Abs(x.Item2) > 5 || Math.Abs(x.Item3) > 5).ToList();

            foreach (var p in filteredPgmList)
            {
                html += "<tr>";
                html += "<td>" + ProcessNameHelper.GetFileDescription(p.Item1) + "</td>";

                var percBlink = p.Item2;

                if (percBlink == Settings.MaxAvgValue)
                {
                    html += "<td style ='color:grey;'> - </td>";
                }    
                else if (Math.Abs(percBlink) > 5) // assumption - we consider only those values which are > than +- 5% of average
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
                else
                {
                    html += "<td style ='color:grey;'> ~average </td>";
                }


                var percEEG = p.Item3;

                if (percEEG == Settings.MaxAvgValue)
                {
                    html += "<td style ='color:grey;'> - </td>";
                }
                else if (Math.Abs(percEEG) > 5) // assumption - we consider only those values which are > than +- 5% of average
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
                    html += "<td style ='color:grey;'> ~average </td>";
                }

                html += "</tr>";
            }
            html += "</tbody>";
            html += "<p style='text-align: center; font-size: 0.66em;'>Hint: Presents the daily attention and engagement when using the top used programs and compares them with averages.</p>";

            html += "</table>";
            return html;
        }
    }
}
