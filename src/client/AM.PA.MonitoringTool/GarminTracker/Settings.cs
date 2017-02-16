// Created by Sebastian Mueller (smueller@ifi.uzh.ch) from the University of Zurich
// Created: 2017-02-10
// 
// Licensed under the MIT License.


using Shared.Data;

namespace GarminTracker
{
    internal class Settings
    {

        internal const string DAILY_URL = "https://gcsapitest.garmin.com/wellness-api/rest/dailies?uploadStartTimeInSeconds=1452470400&uploadEndTimeInSeconds=1452556800";
        internal const string ACCESS_TOKEN_URL = "http://connectapitest.garmin.com/oauth-service-1.0/oauth/access_token";
        internal const string OAUTH_CONFIRM_URL = "http://connecttest.garmin.com/oauthConfirm?oauth_token=";
        internal const string REQUEST_TOKEN_URL = "http://connectapitest.garmin.com/oauth-service-1.0/oauth/request_token";
        internal const string TRACKER_NAME = "GarminTracker";
        internal const bool IS_DETAILED_COLLECTION_AVAILABLE = true;
        internal const string TRACKER_ENABLED = "GarminTrackerEnabled";
        internal const string CONSUMER_KEY_ID = "GarminConsumerKey";
        internal const string CONSUMER_SECRET_ID = "GarminConsumerSecret";
        internal static readonly string CONSUMER_KEY = Database.GetInstance().GetSettingsString(CONSUMER_KEY_ID, string.Empty);
        internal static readonly string CONSUMER_SECRET = Database.GetInstance().GetSettingsString(CONSUMER_SECRET_ID, string.Empty);

    }

}