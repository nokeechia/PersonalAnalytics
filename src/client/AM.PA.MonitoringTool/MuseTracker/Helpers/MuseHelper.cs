using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Shared.Helpers.VisHelper;
using System.Web.Script.Serialization;
using Shared.Helpers;
using Shared;
using UserEfficiencyTracker.Data;

namespace MuseTracker.Helpers
{
    public static class MuseHelper
    {
        public static void processFileStream(FileStream fs)
        {
            using (fs)
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string lines = sr.ReadToEnd();
                    
                    Console.Write("The Elapsed event was raised at {0}", lines);
                }
            }
        }
    }

    public static class Helper
    {
        //http://stackoverflow.com/questions/2253874/linq-equivalent-for-standard-deviation

        public static double StdDev(this IEnumerable<double> values)
        {
            double ret = 0;
            int count = values.Count();
            if (count > 1)
            {
                //Compute the Average
                double avg = values.Average();

                //Perform the Sum of (value-avg)^2
                double sum = values.Sum(d => (d - avg) * (d - avg));

                //Put it all together
                ret = Math.Sqrt(sum / count-1); //used sample formula thus -1
            }
            return ret;
        }

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

        public static List<DateElementExtended<double>> NormalizeBlinks(List<Tuple<DateTime, int>> blinks)
        {

            List<Tuple<DateTime, double>> logblinks = GetLogBlinks(blinks); //log transform because of huge differences in ranges, -1 because reverse blinks indicate more attention
            var minBlinks = GetMinBlinks(logblinks);
            var maxBlinks = GetMaxBlinks(logblinks);

            List<DateElementExtended<double>> normalizedBlinks = logblinks.Select(i => new DateElementExtended<double>
            {
                date = i.Item1.ToString(),
                normalizedvalue = Math.Round(VisHelper.Rescale(i.Item2, minBlinks, maxBlinks), 2),
                originalvalue = Math.Pow(10, i.Item2 * -1), //because log before
                extraInfo = new JavaScriptSerializer().Serialize(FromDateToExtraInfo(i.Item1))
            }).ToList();

            return normalizedBlinks;
        }

        private static ExtraInfo FromDateToExtraInfo(DateTime _date)
        {
            return new ExtraInfo { switches = GetNrOfTopProgramSwitchesOfTheDay(_date, VisType.Day), topPgms = Queries.GetTopProgramsUsed(_date, VisType.Day, 3).Aggregate("", (current, p) => current + ProcessNameHelper.GetFileDescription(p) + ", ").Trim().TrimEnd(',') };
        }


        public static int GetNrOftopProgramSwitchesBetweenTimes(DateTime from, DateTime to)
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

        public static List<DateElementExtended<double>> NormalizeEEGIndices(List<Tuple<DateTime, double>> eegData)
        {
            List<Tuple<DateTime, double>> logEEG = eegData.Select(i => new Tuple<DateTime, double>(i.Item1, Math.Log10(i.Item2))).ToList(); //log transform because of huge differences in ranges, -1 because reverse blinks indicate more attention
            var minEEG = (eegData.Count == 0) ? 0 : eegData.Min(i => i.Item2);
            var maxEEG = (eegData.Count == 0) ? 0 : eegData.Max(i => i.Item2);


            List<DateElementExtended<double>> normalizedEEG = eegData.Select(i => new DateElementExtended<double>
            {
                date = i.Item1.ToString(),
                normalizedvalue = Math.Round(VisHelper.Rescale(i.Item2, minEEG, maxEEG), 2),
                originalvalue = i.Item2, //because log befored
                extraInfo = new JavaScriptSerializer().Serialize(FromDateToExtraInfo(i.Item1))
            }).ToList();

            return normalizedEEG;
        }
    }




}
