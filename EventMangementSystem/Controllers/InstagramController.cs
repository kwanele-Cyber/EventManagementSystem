using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EventMangementSystem.Controllers
{
   

    public class InstagramController : Controller
    {
        private readonly string appId = System.Configuration.ConfigurationManager.AppSettings["InstagramAppId"];
        private readonly string appSecret = System.Configuration.ConfigurationManager.AppSettings["InstagramAppSecret"];
        private readonly string redirectUri = System.Configuration.ConfigurationManager.AppSettings["InstagramRedirectUri"];
        private readonly string accessToken = System.Configuration.ConfigurationManager.AppSettings["InstagramAccessToken"];

        // GET: Instagram/Authorize
        public ActionResult Authorize()
        {
            var authUrl = $"https://api.instagram.com/oauth/authorize?client_id={appId}&redirect_uri={redirectUri}&scope=user_profile,user_media&response_type=code";
            return Redirect(authUrl);
        }

        // GET: Instagram/Callback
        public async Task<ActionResult> Callback(string code)
        {
            var accessToken = await GetAccessToken(code);
            var userProfile = await GetUserProfile(accessToken);

            ViewBag.UserProfile = userProfile;
            return View();
        }

        private async Task<string> GetAccessToken(string code)
        {
            using (var client = new HttpClient())
            {
                var requestUrl = "https://api.instagram.com/oauth/access_token";
                var parameters = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("client_id", appId),
                new KeyValuePair<string, string>("client_secret", appSecret),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("code", code),
            });

                var response = await client.PostAsync(requestUrl, parameters);
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JObject.Parse(responseContent);

                return responseObject["access_token"].ToString();
            }
        }

        private async Task<JObject> GetUserProfile(string accessToken)
        {
            using (var client = new HttpClient())
            {
                var requestUrl = $"https://graph.instagram.com/me?fields=id,username,account_type,media_count&access_token={accessToken}";
                var response = await client.GetStringAsync(requestUrl);
                return JObject.Parse(response);
            }
        }

        // POST: Instagram/Deauthorize
        [HttpPost]
        public ActionResult Deauthorize()
        {
            // Handle deauthorization logic here
            return new HttpStatusCodeResult(200);
        }

        // POST: Instagram/DataDeletion
        [HttpPost]
        public ActionResult DataDeletion()
        {
            // Handle data deletion request here
            return new HttpStatusCodeResult(200);
        }

        // POST: Instagram/PublishPost
        [HttpPost]
        public async Task<ActionResult> PublishPost(string message)
        {
            using (var client = new HttpClient())
            {
                var requestUrl = $"https://graph.instagram.com/me/media?access_token={accessToken}";
                var parameters = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("caption", message),
                new KeyValuePair<string, string>("access_token", accessToken)
            });

                var response = await client.PostAsync(requestUrl, parameters);
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JObject.Parse(responseContent);

                return Json(responseObject);
            }
        }
    }


}