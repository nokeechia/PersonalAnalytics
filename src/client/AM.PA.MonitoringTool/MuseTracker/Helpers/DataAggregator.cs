using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime + lines);
                }
            }
        }
    }
}
