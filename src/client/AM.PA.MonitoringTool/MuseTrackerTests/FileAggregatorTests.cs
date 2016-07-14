using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace MuseTrackerTests

{
    [TestClass]
    public class FileAggregatorTests
    {
        public const string eegbandFilePath = @"C:\Users\seal\eegbandFileTest.txt";

        [TestMethod]
        public void TestAggregatingFrequencyBandsPerTimeInterval()
        {
            using (FileStream fss = new FileStream(eegbandFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fss))
                {
                    string lines = sr.ReadToEnd();
                    Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime + lines);
                    //Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime + "nr of lines " + lines.Length);
                }
            }
        }
    }
}
