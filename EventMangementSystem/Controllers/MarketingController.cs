using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EventMangementSystem.Controllers
{
    public class MarketingController : Controller
    {
        private string pageAccessToken;
        private string longLivedUserToken;
        private readonly string pageId = ConfigurationManager.AppSettings["FacebookPageId"];
        private readonly string appId = ConfigurationManager.AppSettings["FacebookAppId"];
        private readonly string appSecret = ConfigurationManager.AppSettings["FacebookAppSecret"];
        private readonly string shortLivedUserToken = ConfigurationManager.AppSettings["FacebookUserAccessToken"];
        private DateTime tokenExpiry = DateTime.MinValue;

        // GET: Marketing/CreatePromotion
        public ActionResult CreatePromotion()
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Facebook Page ID (at controller initialization): {pageId}");
            if (string.IsNullOrWhiteSpace(pageId) || pageId == "0")
            {
                throw new ApplicationException("Invalid Facebook Page ID in configuration");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreatePromotion(string message, HttpPostedFileBase poster)
        {
            try
            {
                await EnsureAccessTokenValidAsync();

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Facebook Page Access Token: {pageAccessToken}");

                if (string.IsNullOrWhiteSpace(pageId) || pageId == "0")
                {
                    throw new ApplicationException("Invalid Facebook Page ID");
                }

                string postId;
                if (poster != null && poster.ContentLength > 0)
                {
                    postId = await PostPhotoToFacebook(message, poster);
                }
                else
                {
                    postId = await PostToFacebook(message);
                }

                ViewBag.Result = $"Post created successfully. Post ID: {postId}";
            }
            catch (Exception ex)
            {
                ViewBag.Result = $"An error occurred: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[ERROR] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] Exception Stack Trace: {ex.StackTrace}");
            }

            return View("PromotionResult");
        }

        private async Task EnsureAccessTokenValidAsync()
        {
            if (tokenExpiry <= DateTime.UtcNow || string.IsNullOrEmpty(pageAccessToken))
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Access token expired or invalid, refreshing token...");
                pageAccessToken = await GetPageAccessTokenAsync();
                tokenExpiry = DateTime.UtcNow.AddHours(1);  // Assuming the token expiry is set to 1 hour
                ConfigurationManager.AppSettings["FacebookPageAccessToken"] = pageAccessToken;
            }
        }

        private async Task<string> PostPhotoToFacebook(string message, HttpPostedFileBase poster)
        {
            if (poster == null || poster.ContentLength <= 0)
            {
                throw new ArgumentException("Poster file is invalid", nameof(poster));
            }

            using (var client = new HttpClient())
            {
                var requestUrl = $"https://graph.facebook.com/v12.0/{pageId}/photos";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Request URL for photo: {requestUrl}");

                using (var content = new MultipartFormDataContent())
                {
                    byte[] data;
                    using (var br = new BinaryReader(poster.InputStream))
                    {
                        data = br.ReadBytes(poster.ContentLength);
                    }

                    if (data == null || data.Length == 0)
                    {
                        throw new ArgumentException("Poster data is invalid", nameof(poster));
                    }

                    var fileContent = new ByteArrayContent(data);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    content.Add(fileContent, "source", Path.GetFileName(poster.FileName));
                    content.Add(new StringContent(pageAccessToken), "access_token");
                    content.Add(new StringContent(message), "caption");

                    // Add appsecret_proof to the request
                    var appSecretProof = GenerateAppSecretProof(pageAccessToken, appSecret);
                    content.Add(new StringContent(appSecretProof), "appsecret_proof");

                    var response = await client.PostAsync(requestUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Response Status Code: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Response Content for photo post: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] API Error: {responseContent}");
                        throw new ApplicationException($"API Error: {responseContent}");
                    }

                    var responseObject = JObject.Parse(responseContent);
                    return responseObject["id"].ToString();
                }
            }
        }

        private async Task<string> PostToFacebook(string message)
        {
            using (var client = new HttpClient())
            {
                var requestUrl = $"https://graph.facebook.com/v12.0/{pageId}/feed";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Request URL for message: {requestUrl}");

                var parameters = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("message", message),
                    new KeyValuePair<string, string>("access_token", pageAccessToken),
                    new KeyValuePair<string, string>("appsecret_proof", GenerateAppSecretProof(pageAccessToken, appSecret))
                };

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Request Parameters:");
                foreach (var param in parameters)
                {
                    System.Diagnostics.Debug.WriteLine($"{param.Key}: {param.Value}");
                }

                var content = new FormUrlEncodedContent(parameters);

                var response = await client.PostAsync(requestUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Response Status Code: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Response Content for message post: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] API Error: {responseContent}");
                    throw new ApplicationException($"API Error: {responseContent}");
                }

                var responseObject = JObject.Parse(responseContent);
                return responseObject["id"].ToString();
            }
        }

        private async Task<string> GetPageAccessTokenAsync()
        {
            using (var client = new HttpClient())
            {
                // Step 1: Exchange short-lived user token for long-lived user token
                if (string.IsNullOrEmpty(longLivedUserToken) || tokenExpiry <= DateTime.UtcNow)
                {
                    var longLivedUserTokenUrl = $"https://graph.facebook.com/oauth/access_token?grant_type=fb_exchange_token&client_id={appId}&client_secret={appSecret}&fb_exchange_token={shortLivedUserToken}";
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Request URL for long-lived user token: {longLivedUserTokenUrl}");

                    var userTokenResponse = await client.GetAsync(longLivedUserTokenUrl);
                    var userTokenContent = await userTokenResponse.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Response Content for long-lived user token: {userTokenContent}");

                    if (!userTokenResponse.IsSuccessStatusCode)
                    {
                        throw new ApplicationException($"API Error: {userTokenContent}");
                    }

                    var userTokenObject = JObject.Parse(userTokenContent);
                    longLivedUserToken = userTokenObject["access_token"]?.ToString();

                    if (userTokenObject["expires_in"] != null)
                    {
                        tokenExpiry = DateTime.UtcNow.AddSeconds(userTokenObject["expires_in"].ToObject<int>());
                    }
                    else
                    {
                        tokenExpiry = DateTime.UtcNow.AddHours(1); // Default to 1 hour if expires_in is not present
                    }
                }

                // Step 2: Use long-lived user token to get page access token
                var pageAccessTokenUrl = $"https://graph.facebook.com/{pageId}?fields=access_token&access_token={longLivedUserToken}&appsecret_proof={GenerateAppSecretProof(longLivedUserToken, appSecret)}";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Request URL for page access token: {pageAccessTokenUrl}");

                var pageTokenResponse = await client.GetAsync(pageAccessTokenUrl);
                var pageTokenContent = await pageTokenResponse.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Response Content for page access token: {pageTokenContent}");

                if (!pageTokenResponse.IsSuccessStatusCode)
                {
                    throw new ApplicationException($"API Error: {pageTokenContent}");
                }

                var pageTokenObject = JObject.Parse(pageTokenContent);
                return pageTokenObject["access_token"].ToString();
            }
        }

        private string GenerateAppSecretProof(string accessToken, string appSecret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(accessToken));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
