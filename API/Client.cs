/*
VimeoDotNet 3.0 By Saeed Afshari (saeed@saeedoo.com)
To support this project, please visit http://saeedoo.com
for more information.

Copyright (c) 2014 Saeed Afshari

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;

namespace Vimeo
{
    public partial class VimeoClient
    {
        string clientId;
        string secret;
        string apiRoot;

        JavaScriptSerializer json = new JavaScriptSerializer();

        /// <summary>
        /// This contains all JSON data that is received after login.
        /// </summary>
        public Dictionary<string, object> AccessJson { get; private set; }

        /// <summary>
        /// User data. contains information such as display name, user uri, etc.
        /// </summary>
        public Dictionary<string, object> User { get; private set; }

        /// <summary>
        /// Access token after login. Save this for relogins.
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Scopes (permissions) you have. e.g. "private public"
        /// </summary>
        //public 
            string[] Scope { get; set; }

        /// <summary>
        /// token_type. e.g. "bearer"
        /// </summary>
        //public 
            string TokenType { get;  set; }

        void loadAccessData()
        {
            AccessToken = AccessJson["access_token"].ToString();
            Scope = AccessJson["scope"].ToString().Split(' ');
            TokenType = AccessJson["token_type"].ToString();
            User = AccessJson["user"] as Dictionary<string, object>;
        }

        /// <summary>
        /// After the user opens the URL generated with Auth.GetAuthURL
        /// you pass the "code" (access code) with your API keys to this
        /// method. This logs you in and you can start accessing vimeo.
        /// YOU DON'T NEED TO CALL GetAccessToken AS IT WILL BE CALLED HERE.
        /// 
        /// If getting access token fails, it will throw different HTTP errors,
        /// one of which is 404. Surround with try/catch to handle it.
        /// </summary>
        /// <param name="authCode">Auth code that you get after opening the URL from Auth.GetAuthURL</param>
        /// <param name="cid">Your Client/Application ID</param>
        /// <param name="secret">Your Client/Application Secret</param>
        /// <param name="redirect">The Redirect URL That You Specified In API Page</param>
        /// <param name="apiRoot">API Endpoint (optional)</param>
        public static VimeoClient Authorize(
        string authCode,
        string cid,
        string secret,
        string redirect,
        string apiRoot = "https://api.vimeo.com")
        {
            VimeoClient vc = new VimeoClient();
            vc.clientId = cid;
            vc.secret = secret;
            vc.apiRoot = apiRoot;

            vc.AccessJson = Auth.GetAccessToken(authCode, cid, secret, redirect, apiRoot);
            vc.loadAccessData();

            return vc;
        }

        /// <summary>
        /// Relogin with an existing access token.
        /// </summary>
        /// <param name="accessToken">An access token that you have already acquired.</param>
        /// <param name="cid">Your Client/Application ID</param>
        /// <param name="secret">Your Client/Application Secret</param>
        /// <param name="apiRoot">API Endpoint (optional)</param>
        public static VimeoClient ReAuthorize(
            string accessToken,
            string cid,
            string secret,
            string apiRoot = "https://api.vimeo.com")
        {
            VimeoClient vc = new VimeoClient();
            vc.clientId = cid;
            vc.secret = secret;
            vc.AccessToken = accessToken;

            vc.User = vc.Request("/me", null, "GET");
            return vc;
        }

        /// <summary>
        /// Get an Auth URL. This should be step 1 in a new authentication process.
        /// </summary>
        /// <param name="cid">Your Client/Application ID</param>
        /// <param name="scopes">List of scopes (permissions), e.g. {"public"}</param>
        /// <param name="redirect">Redirect URL (same as what you put in your app description)</param>
        /// <param name="apiRoot">API Endpoint (optional)</param>
        /// <returns></returns>
        public static string GetAuthURL(
            string cid,
            List<string> scopes = null,
            string redirect = "",
            string apiRoot = "https://api.vimeo.com")
        {
            return Auth.GetAuthURL(cid, scopes, redirect, apiRoot);
        }

        string jsonEncode(Dictionary<string, string> parameters)
        {
            return json.Serialize(parameters);
        }

        /// <summary>
        /// This is your portal to Vimeo. Use it to call a method with a bunch of parameters.
        /// Example: get your own profile by calling 
        /// Request("/me", null, "GET")
        /// </summary>
        /// <param name="url">The endpoint. It should be a relative URL specifying the API method you want to call, such as "/me"</param>
        /// <param name="parameters">Call parameters. Put null if you don't feel like it.</param>
        /// <param name="method">HTTP method: e.g. "GET", "POST", "PUT", etc.</param>
        /// <param name="jsonBody">true: set content type to json</param>
        /// <returns>Deserialized JSON as Dictionary of strings to objects</returns>
        public Dictionary<string, object> Request(
            string url,
            Dictionary<string, string> parameters,
            string method,
            bool jsonBody = true)
        { 
            var headers = new WebHeaderCollection()
            {
                { "Authorization", String.Format("Bearer {0}", AccessToken) }
            };
            method = method.ToUpper();
            url = apiRoot + url;
            string body = "";
            string contentType = "application/x-www-form-urlencoded";

            if (parameters != null && parameters.Count > 0)
            {
                if (method == "GET")
                {
                    url += "?" + Helpers.KeyValueToString(parameters);
                }
                else if (method == "POST" || method == "PATCH" || method == "PUT" || method == "DELETE")
                {
                    if (jsonBody)
                    {
                        contentType = "application/json";
                        body = jsonEncode(parameters);
                    }
                    else
                    {
                        body = Helpers.KeyValueToString(parameters);
                    }
                }
            }

            return json.Deserialize<Dictionary<string, object>>(
                Helpers.HTTPFetch(url, method, headers, body, contentType));
        }
    }
}
