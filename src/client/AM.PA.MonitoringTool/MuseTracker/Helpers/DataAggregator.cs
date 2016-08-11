using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MuseTracker.Models;

namespace MuseTracker.Helpers
{
    public static class DataAggregator
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

        //public static List<IMuseTrackerInput> filterFileStream(FileStream fs)
          
        //{
        //    using (fs)
        //    {
        //        using (StreamReader sr = new StreamReader(fs))
        //        {
        //            string lines = sr.ReadToEnd();

        //            Console.Write("The Elapsed event was raised at {0}", lines);
        //        }
        //    }
        //}

        public static double aggregateDataToTimestamp(String freqName, List<double> freqValues, String timestamp, int interval)
        {
            return 0.0;
        }

        public static List<Double> filterValuesFromTimestampAndInterval(String freqName, List<Double> freqValues, String timestamp, int interval)
        {
            var output = new List<Double>();
            freqValues.ForEach(item => output.Add(item));
            return output;
        }
    }
}
