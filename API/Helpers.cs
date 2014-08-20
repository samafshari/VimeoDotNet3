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
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.Diagnostics;
using System.IO;

namespace Vimeo
{
    public static class Helpers
    {
        public static IWebProxy Proxy = null;

        public static byte[] ToByteArray(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static string ToBase64(string s)
        {
            return Convert.ToBase64String(ToByteArray(s));
        }

        public static string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
                throw new ArgumentNullException("hashAlgorithm");

            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException("data");

            byte[] dataBuffer = Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);
            return Convert.ToBase64String(hashBytes);
        }

        public static string PercentEncode(string value)
        {
            const string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            var result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                    result.Append(symbol);
                else
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
            }

            return result.ToString();
        }

        public static string KeyValueToString(Dictionary<string, string> payload)
        {
            string body = "";
            foreach (var item in payload)
                body += String.Format("{0}={1}&",
                    Helpers.PercentEncode(item.Key),
                    Helpers.PercentEncode(item.Value));
            if (body[body.Length - 1] == '&') body = body.Substring(0, body.Length - 1);
            return body;
        }

        public static async Task<string> HTTPFetchAsync(string url, string method,
            WebHeaderCollection headers, Dictionary<string, string> payload,
            string contentType="application/x-www-form-urlencoded")
        {
            return await HTTPFetchAsync(url, method, headers, KeyValueToString(payload), contentType);
        }

        public static async Task<string> HTTPFetchAsync(string url, string method, WebHeaderCollection headers, string payload, 
            string contentType="application/x-www-form-urlencoded")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(url);
            if (Proxy != null) request.Proxy = Proxy;

            request.Headers = headers;
            request.Method = method;
            request.Accept = "application/vnd.vimeo.*+json; version=3.2";
            request.ContentType = contentType;
            request.KeepAlive = false;

            var streamBytes = Helpers.ToByteArray(payload);
            request.ContentLength = streamBytes.Length;
            Stream dataStream = await request.GetRequestStreamAsync();
            await dataStream.WriteAsync(streamBytes, 0, streamBytes.Length);
            dataStream.Close();

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
            Debug.WriteLine(((HttpWebResponse)response).StatusDescription);

            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();

            response.Close();

            Debug.WriteLine(String.Format("Response from URL {0}:", url), "HTTPFetch");
            Debug.WriteLine(responseFromServer, "HTTPFetch");
            return responseFromServer;
        }

        public static string HTTPFetch(string url, string method, 
            WebHeaderCollection headers, Dictionary<string, string> payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            return HTTPFetch(url, method, headers, KeyValueToString(payload), contentType);
        }

        public static string HTTPFetch(string url, string method, 
            WebHeaderCollection headers, string payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(url);
            if (Proxy != null) request.Proxy = Proxy;

            request.Headers = headers;
            request.Method = method;
            request.Accept = "application/vnd.vimeo.*+json; version=3.2";
            request.ContentType = contentType;
            request.KeepAlive = false;

            if (!String.IsNullOrWhiteSpace(payload))
            {
                var streamBytes = Helpers.ToByteArray(payload);
                request.ContentLength = streamBytes.Length;
                Stream reqStream = request.GetRequestStream();
                reqStream.Write(streamBytes, 0, streamBytes.Length);
                reqStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)(request.GetResponse());
            Debug.WriteLine(((HttpWebResponse)response).StatusDescription);

            var dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();

            response.Close();

            Debug.WriteLine(String.Format("Response from URL {0}:", url), "HTTPFetch");
            Debug.WriteLine(responseFromServer, "HTTPFetch");
            return responseFromServer;
        }
    }
}
