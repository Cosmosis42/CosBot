using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CosBot.Classes
{
    public enum RequestHttpMethod
    {
        Get,
        Post
    }

    public static class SearchHelper
    {
        private static DateTime lastRefreshed = DateTime.MinValue;
        private static string token { get; set; } = "";
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<Stream> GetResponseStreamAsync(string url,
            IEnumerable<KeyValuePair<string, string>> headers = null, RequestHttpMethod method = RequestHttpMethod.Get)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            //if its a post or there are no headers, use static httpclient
            // if there are headers and it's get, it's not threadsafe
            var cl = headers == null || method == RequestHttpMethod.Post ? httpClient : new HttpClient();
            cl.DefaultRequestHeaders.Clear();
            switch (method)
            {
                case RequestHttpMethod.Get:
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            cl.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                    return await cl.GetStreamAsync(url).ConfigureAwait(false);
                case RequestHttpMethod.Post:
                    FormUrlEncodedContent formContent = null;
                    if (headers != null)
                    {
                        formContent = new FormUrlEncodedContent(headers);
                    }
                    var message = await cl.PostAsync(url, formContent).ConfigureAwait(false);
                    return await message.Content.ReadAsStreamAsync().ConfigureAwait(false);
                default:
                    throw new NotImplementedException("That type of request is unsupported.");
            }
        }

        public static async Task<string> GetResponseStringAsync(string url,
            IEnumerable<KeyValuePair<string, string>> headers = null,
            RequestHttpMethod method = RequestHttpMethod.Get)
        {

            using (var streamReader = new StreamReader(await GetResponseStreamAsync(url, headers, method).ConfigureAwait(false)))
            {
                return await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        private static async Task RefreshAnilistToken()
        {
            if (DateTime.Now - lastRefreshed > TimeSpan.FromMinutes(29))
                lastRefreshed = DateTime.Now;
            else
            {
                return;
            }
            var headers = new Dictionary<string, string> {
                {"grant_type", "client_credentials"},
                {"client_id", "kwoth-w0ki9"},
                {"client_secret", "Qd6j4FIAi1ZK6Pc7N7V4Z"},
            };
            var content = await GetResponseStringAsync(
                            "http://anilist.co/api/auth/access_token",
                            headers,
                            RequestHttpMethod.Post).ConfigureAwait(false);

            token = JObject.Parse(content)["access_token"].ToString();
        }

        public static async Task<bool> ValidateQuery(Discord.Channel ch, string query)
        {
            if (!string.IsNullOrEmpty(query.Trim())) return true;
            await ch.SendMessage("Please specify search parameters.").ConfigureAwait(false);
            return false;
        }

        internal static async Task<string> GetE621ImageLink(string tags)
        {
            try
            {
                var headers = new Dictionary<string, string>() {
                    {"User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1"},
                    {"Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
                };
                var data = await GetResponseStreamAsync(
                    "http://e621.net/post/index.xml?tags=" + Uri.EscapeUriString(tags) + "%20order:random&limit=1",
                    headers);
                var doc = XDocument.Load(data);
                return doc.Descendants("file_url").FirstOrDefault().Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in e621 search: \n" + ex);
                return "Error, do you have too many tags?";
            }
        }
    }
}