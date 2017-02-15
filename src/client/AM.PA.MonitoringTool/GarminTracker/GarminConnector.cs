using OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GarminTracker
{
    public class GarminConnector
    {
        private static string CONSUMER_KEY = "4722b3ef-95dc-47d5-9394-af5c2fd41339";
        private static string CONSUMER_SECRET = "Z0ZVpJgUT3w7oPSZVbyFTu9jJOD2ShBjnFd";

        public static void GetAccessToken()
        {

            //STEP 1
            var oauth = new Manager();
            // the URL to obtain a temporary "request token"
            var rtUrl = "http://connectapitest.garmin.com/oauth-service-1.0/oauth/request_token";
            oauth["consumer_key"] = CONSUMER_KEY;
            oauth["consumer_secret"] = CONSUMER_SECRET;

            OAuthResponse response = oauth.AcquireRequestToken(rtUrl, "POST");
            Console.WriteLine(response.AllText);

            var token = response["oauth_token"];
            var secret = response["oauth_token_secret"];

            Console.WriteLine(token);
            Console.WriteLine(secret);

            //STEP 2

            var url = "http://connecttest.garmin.com/oauthConfirm?oauth_token=" + oauth["token"];
            VerifyAccessView browser = new VerifyAccessView(url, oauth);
            Window browserWindow = new Window
            {
                Content = browser
            };

            browserWindow.ShowDialog();
        }


    }

  
}
