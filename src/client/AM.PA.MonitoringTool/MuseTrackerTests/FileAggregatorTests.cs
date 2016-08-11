using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using MuseTracker.Helpers;
using System.Reflection;

namespace MuseTrackerTests

{
    [TestClass]
    public class FileAggregatorTests
    {
        public string EEGbandFilePath = Path.Combine(Environment.CurrentDirectory,@"Data\", "Frequencybands.txt");
        [TestMethod]
        public void TestAggregatingFrequencyBandsPerTimeInterval()
        {
            DataAggregator.processFileStream(new FileStream(EEGbandFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));           
        }
    }
}
