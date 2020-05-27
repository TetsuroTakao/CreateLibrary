using System;
using System.Collections.Generic;
using System.Text;

namespace Facades.Models
{
    public class MSGraphAuthTokens
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
        public string refresh_token { get; set; }
        public string id_token { get; set; }
    }
    public class ApplicationUser : MSGraphUser
    {
        public bool IsAADUser { get; set; }
        public List<AccessHistory> AccessList { get; set; }
        public string GlobalName { get; set; }
    }
    public class AccessHistory
    {
        // to select AAD user or Live ID
        public string AADEndPoint { get; set; }
        public string TenantID { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public Uri Redirect { get; set; }
        public string AuthCode { get; set; }
        public string Resource { get; set; }
        public string Scope { get; set; }
        public string GrantType { get; set; }
        public string ResponseType { get; set; }
        // to connect token and how to acquired
        public MSGraphAuthTokens AuthTokens { get; set; }
    }
    public class MSGraphUser
    {
        public string displayName { get; set; }
        public string surname { get; set; }
        public string givenName { get; set; }
        public string id { get; set; }
        public string userPrincipalName { get; set; }
        public List<string> businessPhones { get; set; }
        public string jobTitle { get; set; }
        public string mail { get; set; }
        public string mobilePhone { get; set; }
        public string officeLocation { get; set; }
        public string preferredLanguage { get; set; }
    }
}
