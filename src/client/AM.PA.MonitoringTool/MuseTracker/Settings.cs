using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseTracker
{
    public static class Settings
    {
        public static bool IsEnabledByDefault = true;

        public const int MuseIoPort = 5000;

        public const string DbTableMuseEEGData = "muse_input_eeg";
        public const string DbTableMuseEEGDataQuality = "muse_input_eeg_quality";

        public const string DbTableMuseBlink = "muse_input_blink";
        public const string DbTableMuseConcentration = "muse_input_concentration";
        public const string DbTableMuseMellow = "muse_input_mellow";

        public static string CmdToRunMuseIo = "/C muse-io --device-search muse --osc osc.udp://localhost:" + MuseIoPort;

        private const int IntervalSaveToDatabaseInSeconds = 60;
        public static TimeSpan SaveToDatabaseInterval = TimeSpan.FromSeconds(IntervalSaveToDatabaseInSeconds);
        public static TimeSpan CheckMuseConnectionInterval = TimeSpan.FromMinutes(2);

        public const double MaxAvgValue = 9999.99;
    }
}
