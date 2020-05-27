using Facades.Models;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Facades
{
    public class MSGraph
    {
        public bool GetToken(ApplicationUser user)
        {
            bool result = false;
            var url = $"https://login.microsoftonline.com/" + user.AccessList.LastOrDefault().TenantID + "/oauth2";// AAD
            if (user.AccessList.LastOrDefault().GrantType == "password" || !user.IsAADUser || user.AccessList.LastOrDefault().AADEndPoint == "v1.0" || string.IsNullOrEmpty(user.AccessList.LastOrDefault().AADEndPoint))
            {
                url += "/token";// live API
            }
            else
            {
                url += "/" + user.AccessList.LastOrDefault().AADEndPoint + "/token";// AAD
            }
            using (var httpClient = new HttpClient())
            {
                var properties = "client_id=" + user.AccessList.LastOrDefault().ClientId + "&client_secret=" + user.AccessList.LastOrDefault().Secret;
                if (!string.IsNullOrEmpty(user.AccessList.LastOrDefault().Redirect.AbsoluteUri)) properties += "&redirect_uri=" + user.AccessList.LastOrDefault().Redirect.AbsoluteUri;
                if (user.AccessList.LastOrDefault().GrantType.Contains("password"))
                {
                    //var account = JsonConvert.DeserializeObject<JObject>(user.AccessList.LastOrDefault().GrantType);
                    var account = JsonDocument.Parse(user.AccessList.LastOrDefault().GrantType);
                    //properties += "&grant_type=password&username=" + account.RootElement.GetString().FirstOrDefault ["username"];
                    properties += "&grant_type=password&username=" + account.RootElement.GetProperty("username").GetString();
                    //properties += "&password=" + account["password"];
                    properties += "&password=" + account.RootElement.GetProperty("password").GetString();
                    properties += "&resource=https://graph.microsoft.com";
                }
                else
                {
                    properties += "&scope=" + user.AccessList.LastOrDefault().Scope;
                }
                if (user.AccessList.LastOrDefault().GrantType == "refresh") properties += "&refresh_token=" + user.AccessList.LastOrDefault().AuthTokens.refresh_token + "&grant_type=refresh_token";
                if (user.AccessList.LastOrDefault().GrantType == "client_credentials") properties += "&grant_type=client_credentials";
                if (!string.IsNullOrEmpty(user.AccessList.LastOrDefault().AuthCode)) properties += "&code=" + user.AccessList.LastOrDefault().AuthCode + "&grant_type=authorization_code";
                var content = new StringContent(properties, Encoding.UTF8, "application/x-www-form-urlencoded");
                var res = httpClient.PostAsync(url, content).Result;
                string resultJson = res.Content.ReadAsStringAsync().Result;
                if (res.IsSuccessStatusCode)
                {
                    //user.AccessList.LastOrDefault().AuthTokens = JsonConvert.DeserializeObject<MSGraphAuthTokens>(resultJson);
                    user.AccessList.LastOrDefault().AuthTokens = JsonSerializer.Deserialize<MSGraphAuthTokens>(resultJson);
                    result = true;
                }
            }
            return result;
        }
        public string GetToken(string clientid, string secret, string redirect, string refreshtoken, string authCode, string tenant, string resource = "user.read")
        {
            string result = string.Empty;
            MSGraphAuthTokens tokens = null;
            var url = $"https://login.microsoftonline.com/" + tenant + "/oauth2/v2.0/token";// AAD
            url = $"https://login.microsoftonline.com/common/oauth2/token";// live API
            using (var httpClient = new HttpClient())
            {
                var properties = "client_id=" + clientid + "&client_secret=" + secret + "&scope=" + resource + "&redirect_uri=" + redirect;
                if (!string.IsNullOrEmpty(refreshtoken))
                {
                    properties += "&refresh_token=" + refreshtoken + "&grant_type=refresh_token";
                }
                if (!string.IsNullOrEmpty(authCode))
                {
                    properties += "&code=" + authCode + "&grant_type=authorization_code";
                }
                var content = new StringContent(properties, Encoding.UTF8, "application/x-www-form-urlencoded");
                var res = httpClient.PostAsync(url, content).Result;
                string resultJson = res.Content.ReadAsStringAsync().Result;
                if (res.IsSuccessStatusCode)
                {
                    //tokens = JsonConvert.DeserializeObject<MSGraphAuthTokens>(resultJson);
                    tokens = JsonSerializer.Deserialize<MSGraphAuthTokens>(resultJson);
                    result = tokens.access_token;
                }
            }
            return result;
        }
        public string GetUser(string token)
        {
            string result = string.Empty;
            MSGraphUser user = null;
            var url = $"https://graph.microsoft.com/v1.0/me/";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var res = httpClient.GetAsync(url).Result;
                string resultJson = res.Content.ReadAsStringAsync().Result;
                if (res.IsSuccessStatusCode)
                {
                    //user = JsonConvert.DeserializeObject<MSGraphUser>(resultJson);
                    user = JsonSerializer.Deserialize<MSGraphUser>(resultJson);
                    result = user.displayName;
                }
            }
            return result;
        }
        public void SetUserInfo(ApplicationUser account)
        {
            var token = account.AccessList.Where(t => Regex.IsMatch(t.Scope, @"user\.read", RegexOptions.IgnoreCase)).FirstOrDefault();
            if (token == null) return;
            MSGraphUser user = null;
            var url = $"https://graph.microsoft.com/v1.0/me/";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.AuthTokens.access_token);
                var res = httpClient.GetAsync(url).Result;
                string resultJson = res.Content.ReadAsStringAsync().Result;
                if (res.IsSuccessStatusCode)
                {
                    //user = JsonConvert.DeserializeObject<MSGraphUser>(resultJson);
                    user = JsonSerializer.Deserialize<MSGraphUser>(resultJson);
                    account.businessPhones = user.businessPhones;
                    account.displayName = user.displayName;
                    account.givenName = user.givenName;
                    account.id = user.id;
                    account.jobTitle = user.jobTitle;
                    account.mail = user.mail;
                    account.mobilePhone = user.mobilePhone;
                    account.officeLocation = user.officeLocation;
                    account.preferredLanguage = user.preferredLanguage;
                    account.surname = user.surname;
                    account.userPrincipalName = user.userPrincipalName;
                }
            }
            return;
        }
        public string GetLink(string id, ApplicationUser account)
        {
            string result = string.Empty;
            var token = account.AccessList.Where(t => Regex.IsMatch(t.Scope, @"files\.readwrite", RegexOptions.IgnoreCase)).FirstOrDefault();
            if (token == null) return result;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.AuthTokens.access_token);
                var postBody = "{\"type\": \"edit\", \"scope\": \"anonymous\"}";
                var content = new StringContent(postBody, Encoding.UTF8, "application/json");
                var responseMessage = httpClient.PostAsync(string.Format("https://graph.microsoft.com/v1.0/me/drive/items/{0}/createLink", id), content).Result;
                var response = responseMessage.Content.ReadAsStringAsync().Result;
                if (responseMessage.IsSuccessStatusCode)
                {
                    //var resLink = JsonConvert.DeserializeObject<MSGraphLink>(response);
                    var resLink = JsonSerializer.Deserialize<MSGraphLink>(response);
                    result = resLink.link.webUrl;
                }
            }
            return result;
        }
    }
}
