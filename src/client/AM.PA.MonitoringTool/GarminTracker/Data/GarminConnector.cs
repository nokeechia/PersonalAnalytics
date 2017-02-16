using OAuth;
using System.Windows;
using System.Net;
using System.IO;
using System;

namespace GarminTracker
{
    public class GarminConnector
    {

        private static Manager InitializeOauthManager()
        {
            var oauth = new Manager();
            oauth["consumer_key"] = Settings.CONSUMER_KEY;
            oauth["consumer_secret"] = Settings.CONSUMER_SECRET;
            if (SecretStorage.GetAccessToken() != null) {
                oauth["token"] = SecretStorage.GetAccessToken();
            }
            if (SecretStorage.GetTokenSecret() != null)
            {
                oauth["token_secret"] = SecretStorage.GetTokenSecret();
            }

            return oauth;
        }

        public static void GetDaily()
        {
            var oauth = InitializeOauthManager();

            var authzHeader = oauth.GenerateAuthzHeader(Settings.DAILY_URL, "GET");
            var request = (HttpWebRequest)WebRequest.Create(Settings.DAILY_URL);
            request.Method = "GET";
            request.PreAuthenticate = true;
            request.AllowWriteStreamBuffering = true;
            request.Headers.Add("Authorization", authzHeader);

            using (var r = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = new StreamReader(r.GetResponseStream()))
                {

                    var objText = reader.ReadToEnd();
                    Console.WriteLine(objText);
                }
            }
        }

        public static void GetAccessToken()
        {
            var oauth = InitializeOauthManager();

            OAuthResponse response = oauth.AcquireRequestToken(Settings.REQUEST_TOKEN_URL, "POST");
            
            var token = response["oauth_token"];
            var secret = response["oauth_token_secret"];
            
            //Step 2
            var url = Settings.OAUTH_CONFIRM_URL + oauth["token"];
            VerifyAccessView browser = new VerifyAccessView(url, oauth);
            Window browserWindow = new Window
            {
                Content = browser
            };

            browserWindow.ShowDialog();
        }

    }
 
}