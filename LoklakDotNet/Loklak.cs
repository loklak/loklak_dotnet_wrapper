﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LoklakDotNet
{
    public class Loklak
    {
        private string apiUrl;
        private HttpClient client;

        private async Task<string> ProcessUrlAsync(string method, Dictionary<string,string> parameters=null)
        {
            var uriString = apiUrl + method;
            if(method=="markdown.png" || method == "map.png")
            {
                uriString = uriString.Replace("api", "vis");
            }
            UriBuilder uri = new UriBuilder(uriString);
            if(!object.ReferenceEquals(null, parameters) && parameters.Count!=0)
            {
                uri.Query = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;
            }
            System.Diagnostics.Debug.WriteLine(uri.Uri.AbsoluteUri);
            var result = await client.GetStringAsync(uri.Uri.AbsoluteUri);
            System.Diagnostics.Debug.WriteLine(result);
            return result;
        }

        /// <summary>
        /// Initializes the Loklak API wrapper
        /// </summary>
        /// <param name="apiUrl">Optional API Url</param>
        public Loklak(String apiUrl = "http://api.loklak.org/api/")
        {
            this.apiUrl = apiUrl;
            client = new HttpClient();
        }

        /// <summary>
        /// The status servlet shows the size of the internal Elasticsearch search index for messages and users. Furthermore, the servlet reflects the current browser clients settings in the client_info.
        /// </summary>
        /// <returns></returns>
        public async Task<string> status()
        {
            return (await ProcessUrlAsync("status.json"));
        }

        /// <summary>
        /// The hello servlet is part of the loklak peer-to-peer bootstrap process and shall be used to announce that a new client has been started up. The hello request is done automatically after a loklak startup against the loklak backend as configured in the settings in field backend. The back-end server then does not return any data, just an 'ok' string object.
        /// </summary>
        /// <returns>{"status":"ok"}</returns>
        public async Task<string> hello()
        {
            return (await ProcessUrlAsync("hello.json"));
        }

        /// <summary>
        /// This servlet combined the result of the hello calls from all peers and provides a list of addresses where the remote peers can be accessed.
        /// </summary>
        /// <returns></returns>
        public async Task<string> peers()
        {
            return (await ProcessUrlAsync("peers.json"));
        }

        /// <summary>
        /// This servlet provides geocoding of place names to location coordinates and also reverse geocoding of location coordinates to place names.
        /// </summary>
        /// <param name="places">List of place names</param>
        /// <returns></returns>
        public async Task<string> geocode(IList<string> places)
        {
            Dictionary<string,string> qs = new Dictionary<string,string>();
            qs.Add("places", string.Join(",", places));
            return (await ProcessUrlAsync("geocode.json", qs));
        }

        /// <summary>
        /// This servlet provides the retrieval of user followers and the accounts which the user is following.
        /// </summary>
        /// <param name="screen_name">The screen name of the Twitter user without "@"</param>
        /// <param name="follower_count">The maximum number of follower profiles to be fetched</param>
        /// <param name="following_count">The maximum number of following profiles to be fetched</param>
        /// <returns></returns>
        public async Task<string> user(string screen_name, int follower_count=0, int following_count=0)
        {
            Dictionary<string,string> qs = new Dictionary<string,string>();
            qs.Add("screen_name", screen_name);
            qs.Add("followers", follower_count.ToString());
            qs.Add("following", following_count.ToString());
            return (await ProcessUrlAsync("user.json", qs));
        }

        /// <summary>
        /// Get a search result from the server
        /// </summary>
        /// <param name="q">query term</param>
        /// <param name="count">the wanted number of results</param>
        /// <param name="source">the source for the search cache|backend|twitter|all</param>
        /// <param name="fields">aggregation fields for search facets</param>
        /// <param name="limit">a limitation of number of facets for each aggregation</param>
        /// <param name="timeZoneOffset">offset applied on since:, until: and the date histogram</param>
        /// <returns></returns>
        public async Task<string> search(LoklakSearchTerm q, int count = 100, string source="cache", IList<string> fields = null, int limit=-1, int timeZoneOffset=-1)
        {
            Dictionary<string,string> qs = new Dictionary<string,string>();
            qs.Add("q", q.getQueryString());
            qs.Add("count", count.ToString());
            qs.Add("source", source);
            if(fields!= null)
            {
                qs.Add("fields", string.Join(",", fields));
            }
            if(limit!=-1)
            {
                qs.Add("limit", limit.ToString());
            }
            if(timeZoneOffset!=-1)
            {
                qs.Add("timeZoneOffset", timeZoneOffset.ToString());
            }
            return (await ProcessUrlAsync("search.json", qs));
        }

        /// <summary>
        /// This servlet provides an image with text on it.
        /// </summary>
        /// <param name="text">text to be printed, markdown possible</param>
        /// <param name="color_text">text color</param>
        /// <param name="color_background">background color</param>
        /// <param name="padding">space around text</param>
        /// <param name="uppercase"></param>
        /// <returns>Image object in string</returns>
        public async Task<string> markdown(string text, String color_text="", String color_background="", int padding=0, bool uppercase=true)
        {
            Dictionary<string,string> qs = new Dictionary<string,string>();
            qs.Add("text", text);
            qs.Add("padding", padding.ToString());
            qs.Add("uppercase", uppercase ? "1" : "0");
            if (!string.IsNullOrWhiteSpace(color_text))
            {
                qs.Add("color_text", color_text);
            }
            if (!string.IsNullOrWhiteSpace(color_background))
            {
                qs.Add("color_background", color_background);
            }
            return (await ProcessUrlAsync("markdown.png", qs));
        }

    }

    /// <summary>
    /// Constructs a search term for the Loklak.search() method
    /// </summary>
    public class LoklakSearchTerm
    {
        public string terms = "";
        public string from = "";
        public string near = "";
        public DateTime since;
        public DateTime until;
        
        public string getQueryString()
        {
            var qs = terms;
            if(!string.IsNullOrWhiteSpace(from))
            {
                qs += " from:" + from;
            }
            if (!string.IsNullOrWhiteSpace(near))
            {
                qs += " near:" + near;
            }
            if(since.Ticks>0)
            {
                qs += " since:" + since.ToString("yyyy-MM-dd");
            }
            if (until.Ticks > 0)
            {
                qs += " until:" + until.ToString("yyyy-MM-dd");
            }
            return qs;
        }
    }
}
