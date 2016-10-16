using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MuseTracker
{

    [TestClass]
    public class MuseHelperTests
    {
        private List<Tuple<string, int, double>> eegData;
        private List<Tuple<string, int, double>> blinkData;

        [TestInitialize]
        public void Initialize()
        {
            // list contains string for name, int for durInMins, double for eeg value
            this.eegData = new List<Tuple<string, int, double>>();

            this.eegData.Add(new Tuple<string, int, double>("chrome", 2, 0.54));
            this.eegData.Add(new Tuple<string, int, double>("devenv", 0, 0.54));
            this.eegData.Add(new Tuple<string, int, double>("chrome", 9, 0.33));
            this.eegData.Add(new Tuple<string, int, double>("devenv", 2, 0.0));

            this.blinkData = new List<Tuple<string, int, double>>();

            this.blinkData.Add(new Tuple<string, int, double>("chrome", 2, 15));
            this.blinkData.Add(new Tuple<string, int, double>("devenv", 0, 54));
            this.blinkData.Add(new Tuple<string, int, double>("chrome", 9, 3));
            this.blinkData.Add(new Tuple<string, int, double>("devenv", 2, 23));

        }
        [TestMethod]
        public void TestCalculationOfEEGWeightedAverages()
        { 
            var weightedAvgPerPgms = Helper.HelperMethods.CalculateWeightedAvgPerPgm(eegData);

            var e = weightedAvgPerPgms.Where(x => x.Item1 == "chrome").Select(x => x.Item2).First();
            Assert.AreEqual((2 * 0.54 + 9 * 0.33) / 11, e);
            e = weightedAvgPerPgms.Where(x => x.Item1 == "devenv").Select(x => x.Item2).First();
            Assert.AreEqual(0, e);
        }

        [TestMethod]
        public void TestCalculationOfTotalEEGWeightedAverage()
        {
            var actual = Helper.HelperMethods.CalculateTotalWeightedAvg(eegData);
            Assert.AreEqual(0.31, actual);
        }

        [TestMethod]
        public void TestCalculationOfTotalBlinkWeightedAverage()
        {
            var actual = Helper.HelperMethods.CalculateTotalWeightedAvg(blinkData);
            Assert.AreEqual(7.92, actual);
        }
    }
}
