using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MuseTracker
{

    [TestClass]
    public class MuseHelperTests
    {
        private List<Tuple<string, int, double>> rawEegData;

        [TestInitialize]
        public void Initialize()
        {
            // list contains string for name, int for durInMins, double for eeg value
            this.rawEegData = new List<Tuple<string, int, double>>();

            this.rawEegData.Add(new Tuple<string, int, double>("chrome", 2, 0.54));
            this.rawEegData.Add(new Tuple<string, int, double>("devenv", 0, 0.54));
            this.rawEegData.Add(new Tuple<string, int, double>("chrome", 9, 0.33));
            this.rawEegData.Add(new Tuple<string, int, double>("devenv", 2, 0.0));
        }
        [TestMethod]
        public void TestCalculationOfEEGWeightedAverages()
        { 
            var weightedAvgPerPgms = Helper.HelperMethods.CalculateWeightedAvgPerPgm(rawEegData);

            var e = weightedAvgPerPgms.Where(x => x.Item1 == "chrome").Select(x => x.Item2).First();
            Assert.AreEqual((2 * 0.54 + 9 * 0.33) / 11, e);
            e = weightedAvgPerPgms.Where(x => x.Item1 == "devenv").Select(x => x.Item2).First();
            Assert.AreEqual(0, e);
        }

        [TestMethod]
        public void TestCalculationOfTotalEEGWeightedAverage()
        {

        }
    }
}
