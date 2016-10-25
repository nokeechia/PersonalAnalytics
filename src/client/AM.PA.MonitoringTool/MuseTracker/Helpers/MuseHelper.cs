using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Shared.Helpers.VisHelper;
using System.Web.Script.Serialization;
using Shared.Helpers;
using Shared;
using UserEfficiencyTracker.Data;

namespace MuseTracker.Helper
{
    public static class HelperMethods
    {
        private static List<Tuple<DateTime, double>> GetLogBlinks(List<Tuple<DateTime, int>> blinks)
        {
            return blinks.Select(i => new Tuple<DateTime, double>(i.Item1, Math.Log10(i.Item2) * -1)).ToList(); //log transform because of huge differences in ranges, -1 because reverse blinks indicate more attention
        }

        private static double GetMinBlinks(List<Tuple<DateTime, double>> blinks)
        {
            return (blinks.Count == 0)? 0 : blinks.Min(i => i.Item2);
        }

        private static double GetMaxBlinks(List<Tuple<DateTime, double>> blinks)
        {
            return (blinks.Count == 0) ? 0 : blinks.Max(i => i.Item2);
        }

        /// <summary>
        /// Transform original blink values into DateElementExtended items with normalized values
        /// </summary>
        /// <param name="blinks"></param>
        /// <returns>List with DateElements which contain original, normal and addition values</returns>
        /// 
        public static List<DateElementExtended<double>> TransformBlinksToExtendedDateElements(List<Tuple<DateTime, int>> blinks)
        {

            List<Tuple<DateTime, double>> logblinks = GetLogBlinks(blinks); //log transform because of huge differences in ranges, -1 because reverse blinks indicate more attention
            var minBlinks = GetMinBlinks(logblinks);
            var maxBlinks = GetMaxBlinks(logblinks);

            List<DateElementExtended<double>> normalizedBlinks = logblinks.Select(i => new DateElementExtended<double>
            {
                date = i.Item1.ToString(),
                normalizedvalue = Math.Round(VisHelper.Normalize(i.Item2, minBlinks, maxBlinks), 2),
                originalvalue = Math.Pow(10, i.Item2 * -1), //because log before
                extraInfo = new JavaScriptSerializer().Serialize(FromDateToExtraInfo(i.Item1))
            }).ToList();

            return normalizedBlinks;
        }

        private static ExtraInfo FromDateToExtraInfo(DateTime _date)
        {
            return new ExtraInfo { switches = GetNrOfTopProgramSwitchesOfTheDay(_date, VisType.Day), topPgms = Queries.GetTopProgramsUsed(_date, VisType.Day, 3).Aggregate("", (current, p) => current + ProcessNameHelper.GetFileDescription(p) + ", ").Trim().TrimEnd(',') };
        }


        public static int GetNoOftopProgramSwitchesBetweenTimes(DateTime from, DateTime to)
        {
            var programsUsed = GetTop20Programs(from);
            if (programsUsed.Count < 1)
            {
                return 0;
            }

            return ComputeSwitches(TransformDictionaryToFlatDto(programsUsed, from, to));
        }

        private static Dictionary<String, List<TopProgramTimeDto>> GetTop20Programs(DateTime date)
        {
            return Queries.GetTopProgramsUsedWithTimes(date, VisType.Day, 20); // 20 is assumption
        }

        private static int ComputeSwitches(List<TopProgramFlatDto> list)
        {
            //sort list by From Date
            var sortedList = list.OrderBy(x => x.From.TimeOfDay).ThenBy(x => x.DurInMins).ToList();
            //count switches

            var oldName = "";
            var switches = 0;
            foreach (TopProgramFlatDto dt in sortedList)
            {
                if (oldName.Length > 0 && !dt.Name.Equals(oldName))
                {
                    switches += 1;
                }
                oldName = dt.Name;
            }

            return switches;
        }

        private static List<TopProgramFlatDto> TransformDictionaryToFlatDto(Dictionary<String, List<TopProgramTimeDto>> dict)
        {
            var list = new List<TopProgramFlatDto>();
            foreach (KeyValuePair<string, List<TopProgramTimeDto>> entry in dict)
            {
                if (entry.Value.Count > 0)
                {
                    foreach (TopProgramTimeDto dt in entry.Value)
                    {
                        list.Add(new TopProgramFlatDto(entry.Key, dt.From, dt.To, dt.DurInMins));
                    }
                }
            }
            return list;
        }

        private static List<TopProgramFlatDto> TransformDictionaryToFlatDto(Dictionary<String, List<TopProgramTimeDto>> dict, DateTime from, DateTime to)
        {
            var list = new List<TopProgramFlatDto>();
            foreach (KeyValuePair<string, List<TopProgramTimeDto>> entry in dict)
            {
                if (entry.Value.Count > 0)
                {
                    foreach (TopProgramTimeDto dt in entry.Value)
                    {
                        if (dt.From >= from && dt.To <= to)
                        {
                            list.Add(new TopProgramFlatDto(entry.Key, dt.From, dt.To, dt.DurInMins));
                        }
                    }
                }
            }
            return list;
        }

        private static int GetNrOfTopProgramSwitchesOfTheDay(DateTime date, VisType type)
        {

            var programsUsed = GetTop20Programs(date);
            if (programsUsed.Count < 1)
            {
                return 0;
            }

            return ComputeSwitches(TransformDictionaryToFlatDto(programsUsed));
        }

        private class TopProgramFlatDto
        {
            public String Name { get; private set; }
            public DateTime From { get; private set; }
            public DateTime To { get; private set; }
            public int DurInMins { get; private set; }

            public TopProgramFlatDto(String name, DateTime from, DateTime to, int durInMins)
            {
                Name = name;
                From = from;
                To = to;
                DurInMins = durInMins;
            }
        }

        /// <summary>
        /// Transform original EEG values into DateElementExtended items with normalized values
        /// </summary>
        /// <param name="eegData"></param>
        /// <returns>List with DateElements which contain original, normal and addition values</returns>
        /// 
        public static List<DateElementExtended<double>> TransformEEGToExtendedDateElements(List<Tuple<DateTime, double>> eegData)
        {
            List<Tuple<DateTime, double>> logEEG = eegData.Select(i => new Tuple<DateTime, double>(i.Item1, Math.Log10(i.Item2))).ToList(); //log transform because of huge differences in ranges, -1 because reverse blinks indicate more attention
            var minEEG = (eegData.Count == 0) ? 0 : eegData.Min(i => i.Item2);
            var maxEEG = (eegData.Count == 0) ? 0 : eegData.Max(i => i.Item2);


            List<DateElementExtended<double>> transformedEEG = eegData.Select(i => new DateElementExtended<double>
            {
                date = i.Item1.ToString(),
                normalizedvalue = Math.Round(VisHelper.Normalize(i.Item2, minEEG, maxEEG), 2),
                originalvalue = i.Item2, //because log befored
                extraInfo = new JavaScriptSerializer().Serialize(FromDateToExtraInfo(i.Item1))
            }).ToList();

            return transformedEEG;
        }

        public static List<Tuple<string, double>> CalculateWeightedAvgPerPgm(List<Tuple<string, int, double>> rawData)
        {
            return rawData.GroupBy(x => x.Item1).Select( g => Tuple.Create(g.Key, g.Sum(x => x.Item2 * x.Item3)/g.Sum(x => x.Item2))).ToList();
        }

        public static double CalculateTotalWeightedAvg(List<Tuple<string, int, double>> data)
        {
            return Math.Round((data.Sum(i => i.Item2 * i.Item3) / data.Sum(x => x.Item2)), 2);
        }


        /// <summary>
        /// Filter only list entries with duration > 1 min
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="programs"></param>
        /// <returns>List with entries that fulfill filter criteria</returns>
        /// 
        public static List<Tuple<string, int, double>> GetFilteredEntries(Dictionary<String, List<Tuple<DateTime, double>>> dict, Dictionary<string, List<TopProgramTimeDto>> programs)
        {
            
            var list = new List<Tuple<string, int, double>>();

            foreach (KeyValuePair<string, List<Tuple<DateTime, double>>> entry in dict)
            {
                if (entry.Value.Count > 0)
                {

                    foreach (Tuple<DateTime, double> e in entry.Value)
                    {
                        //select only entries within time range and duration > 1 min
                        var durInMins = programs.SelectMany(pair => pair.Value.Where(dto => dto.From <= e.Item1 && dto.To >= e.Item1 && dto.DurInMins > 1))
                                                     .Select(dto => dto.DurInMins)
                                                     .ToList();
                        if (durInMins.Sum() > 0) list.Add(new Tuple<string, int, double>(entry.Key, durInMins.Sum(), e.Item2));
                    }
                }
            }           
            return list;
        }

        /// <summary>
        /// Calculates percentual difference of |total average - average per program | for eeg and blink data
        /// </summary>
        /// <param name="avgEEG"></param>
        /// <param name="avgBlinks"></param>
        /// <param name="totalAvgEEG"></param>
        /// <param name="totalAvgBlink"></param>
        /// <returns>List with % differences of blink and eeg data per program and their total averages</returns>
        /// 
        public static List<Tuple<String, double, double>> CalculateDiffToAvg(List<Tuple<String, double>> avgEEG, List<Tuple<String, double>> avgBlinks, double totalAvgEEG, double totalAvgBlink)
        {
            var diffToAvgPerProgram = new List<Tuple<String, double, double>>();

            // process eeg data and blink data separately because they can contain different programs

            foreach (var p in avgEEG)
            {
                var avgEEGPerProgram = Math.Round(p.Item2, 2);
                var percEEG = ((avgEEGPerProgram / totalAvgEEG) - 1) * 100;
                diffToAvgPerProgram.Add(Tuple.Create(p.Item1, Settings.MaxAvgValue, percEEG)); // max value for default
            }

            foreach (var p in avgBlinks)
            {
                var avgBlinksPerProgram = Math.Round(p.Item2, 2);
                var percBlink = ((avgBlinksPerProgram / totalAvgBlink) - 1) * 100;

                var existingEntry = diffToAvgPerProgram.Find(i => i.Item1 == p.Item1);
                if (existingEntry.Equals(null))
                {
                    // No entry exists, add new one
                    diffToAvgPerProgram.Add(Tuple.Create(p.Item1, percBlink, Settings.MaxAvgValue)); // max value for default
                }
                else
                {
                    var newEntry = Tuple.Create(existingEntry.Item1, percBlink, existingEntry.Item3);
                    diffToAvgPerProgram.Remove(existingEntry);
                    diffToAvgPerProgram.Add(newEntry);
                }
            }
            return diffToAvgPerProgram;
        }
    }
}
