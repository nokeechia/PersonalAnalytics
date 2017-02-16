// Created by Sebastian Mueller (smueller@ifi.uzh.ch) from the University of Zurich
// Created: 2017-02-16
// 
// Licensed under the MIT License.

using System;
using System.Linq;
using Windows.Security.Credentials;

namespace GarminTracker
{

    //Stores access token. See: https://social.msdn.microsoft.com/Forums/en-US/8160064a-dd96-463f-b3b7-4243f20c13e4/recommended-way-to-store-oauth-clientid-and-secret-for-rest-services-in-a-xaml-metro-app?forum=winappswithcsharp
    public class SecretStorage
    {
        private const string RESOURCE_NAME = "Garmin_OAuth_Token";
        private const string ACCESS_TOKEN = "accessToken";
        private const string TOKEN_SECRET = "tokenSecret";

        public static void SaveAccessToken(string accessToken)
        {
            var vault = new PasswordVault();
            var credential = new PasswordCredential(RESOURCE_NAME, ACCESS_TOKEN, accessToken);
            vault.Add(credential);
        }

        public static void SaveTokenSecret(string tokenSecret)
        {
            var vault = new PasswordVault();
            var credential = new PasswordCredential(RESOURCE_NAME, TOKEN_SECRET, tokenSecret);
            vault.Add(credential);
        }
        
        public static string GetAccessToken()
        {
            return GetCredential(ACCESS_TOKEN);
        }

        public static string GetTokenSecret()
        {
            return GetCredential(TOKEN_SECRET);
        }

        private static string GetCredential(string kind)
        {
            var vault = new PasswordVault();
            try
            {
                var credential = vault.FindAllByResource(RESOURCE_NAME).FirstOrDefault();
                if (credential != null)
                {
                    return vault.Retrieve(RESOURCE_NAME, kind).Password;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }

}