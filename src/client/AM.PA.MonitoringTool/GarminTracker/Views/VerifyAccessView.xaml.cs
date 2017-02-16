using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OAuth;
using System.Net;
using System.IO;

namespace GarminTracker
{
    /// <summary>
    /// Interaction logic for VerifyAccessView.xaml
    /// </summary>
    public partial class VerifyAccessView : UserControl
    {
        private Manager oauth;
        
        public VerifyAccessView(string url, Manager oauth)
        {
            this.oauth = oauth;
            InitializeComponent();
            WebBrowser.Navigated += WebBrowser_Navigated;
            WebBrowser.Navigate(url);
        }

        private void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            Console.WriteLine(e.Uri.ToString());

            if (e.Uri.ToString().Contains("oauth_verifier"))
            {
                var queryDict = HttpUtility.ParseQueryString(e.Uri.Query);
                string verifier = queryDict.Get("oauth_verifier");

                try
                {
                    OAuthResponse response = oauth.AcquireAccessToken("http://connectapitest.garmin.com/oauth-service-1.0/oauth/access_token", "POST", verifier);
                    string accessToken = response["oauth_token"];
                    string tokenSecret = response["oauth_token_secret"];

                    SecretStorage.SaveAccessToken(accessToken);
                    SecretStorage.SaveTokenSecret(tokenSecret);

                    Console.WriteLine(accessToken);
                    Console.WriteLine(tokenSecret);

                    var appUrl = "https://gcsapitest.garmin.com/wellness-api/rest/dailies?uploadStartTimeInSeconds=1452470400&uploadEndTimeInSeconds=1452556800";
                    var authzHeader = oauth.GenerateAuthzHeader(appUrl, "GET");
                    var request = (HttpWebRequest)WebRequest.Create(appUrl);
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
        }
    }
}
