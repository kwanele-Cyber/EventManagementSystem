using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EventMangementSystem.Models
{
   

    public class FacebookService
    {
        private readonly string _pageAccessToken;

        public FacebookService(string pageAccessToken)
        {
            _pageAccessToken = pageAccessToken;
        }

        public async Task<string> CreatePostAsync(string message)
        {
            using (var client = new HttpClient())
            {
                var requestUrl = $"https://graph.facebook.com/v12.0/me/feed?access_token={_pageAccessToken}";
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("message", message)
            });

                var response = await client.PostAsync(requestUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JObject.Parse(responseString);

                return responseJson["id"]?.ToString();
            }
        }
    }

}