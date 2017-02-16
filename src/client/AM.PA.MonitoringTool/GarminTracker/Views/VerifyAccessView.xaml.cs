using System;
using System.Web;
using System.Windows.Controls;
using System.Windows.Navigation;
using OAuth;

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
                    OAuthResponse response = oauth.AcquireAccessToken(Settings.ACCESS_TOKEN_URL, "POST", verifier);
                    string accessToken = response["oauth_token"];
                    string tokenSecret = response["oauth_token_secret"];

                    SecretStorage.SaveAccessToken(accessToken);
                    SecretStorage.SaveTokenSecret(tokenSecret);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
        }

    }

}