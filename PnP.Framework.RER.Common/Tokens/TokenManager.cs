using Microsoft.IdentityModel.Tokens;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using PnP.Framework.RER.Common.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security;
using System.Threading.Tasks;

namespace PnP.Framework.RER.Common.Tokens
{
    public class TokenManager
    {
        private readonly SharePointAppCreds _sharePointAppCreds;
        private const string AcsPrincipalName = "00000001-0000-0000-c000-000000000000";
        private bool _validated = false;
        private JwtSecurityToken _parsedToken;
        private readonly HttpClient _httpClient;

        public TokenManager(SharePointAppCreds sharePointAppCreds, HttpClient httpClient)
        {
            _sharePointAppCreds = sharePointAppCreds;
            _httpClient = httpClient;
        }

        public void ValidateToken(string contextToken, string host)
        {
            var key = new SymmetricSecurityKey(Convert.FromBase64String(_sharePointAppCreds.ClientSecret));
            var handler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;
            var token = handler.ReadJwtToken(contextToken);

            var audienceValue = token.Claims.Single(c => c.Type == "aud").Value;
            var tenantId = audienceValue.Substring(audienceValue.IndexOf('@') + 1);

            handler.ValidateToken(contextToken, new TokenValidationParameters
            {
                IssuerSigningKey = key,
                ValidateAudience = true,
                ValidAudience = $"{_sharePointAppCreds.ClientId}/{host}@{tenantId}",
                ValidateIssuer = true,
                ValidIssuer = $"{AcsPrincipalName}@{tenantId}",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            }, out validatedToken);

            _validated = true;
            _parsedToken = validatedToken as JwtSecurityToken;
        }

        public async Task<ClientContext> GetClientContextAsync(string siteUrl)
        {
            var data = await GetAccessTokenAsync(siteUrl);
            var accessToken = new SecureString();
            Array.ForEach(data.access_token.ToArray(), accessToken.AppendChar);
            var authManager = new AuthenticationManager(accessToken);

            return await authManager.GetContextAsync(siteUrl);
        }

        public async Task<AccessTokenResponse> GetAccessTokenAsync(string siteUrl)
        {
            if (!_validated)
            {
                throw new Exception("You should validate token first");
            }

            var sharepointHost = new Uri(siteUrl).Authority;
            var targetPrincipal = _parsedToken.Claims.Single(c => c.Type == "appctxsender").Value.Split("@")[0];
            var refreshToken = _parsedToken.Claims.Single(c => c.Type == "refreshtoken").Value;
            var appCtx = JsonConvert.DeserializeObject<AppCtx>(_parsedToken.Claims.Single(c => c.Type == "appctx").Value);
            var audienceValue = _parsedToken.Claims.Single(c => c.Type == "aud").Value;
            var tenantId = audienceValue.Substring(audienceValue.IndexOf('@') + 1);

            var resource = GetFormattedPrincipal(targetPrincipal, sharepointHost, tenantId);
            var clientId = GetFormattedPrincipal(_sharePointAppCreds.ClientId, null, tenantId);

            var stsUrl = await GetStsUrlAsync(appCtx.SecurityTokenServiceUri, tenantId);

            return await GetAccessTokenAsync(stsUrl, clientId, resource, refreshToken);
        }

        private async Task<AccessTokenResponse> GetAccessTokenAsync(string stsUrl, string clientId, string resource, string refreshToken)
        {
            var formContent = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "grant_type", "refresh_token" },
                { "client_id", clientId },
                { "client_secret", _sharePointAppCreds.ClientSecret },
                { "refresh_token", refreshToken },
                { "resource", resource },
            });

            var client = new HttpClient();
            var result = await client.PostAsync(stsUrl, formContent);
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AccessTokenResponse>(content);
        }

        private string GetFormattedPrincipal(string principalName, string hostName, string realm)
        {
            if (!string.IsNullOrEmpty(hostName))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}/{1}@{2}", principalName, hostName, realm);
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}@{1}", principalName, realm);
        }

        private async Task<string> GetStsUrlAsync(string securityTokenServiceUri, string realm)
        {
            var metadataUrl = $"{new Uri(securityTokenServiceUri).GetLeftPart(UriPartial.Authority)}/metadata/json/1?realm={realm}";

            var result = await _httpClient.GetFromJsonAsync<JsonMetadataDocument>(metadataUrl);

            return result.Endpoints.Single(e => !string.IsNullOrEmpty(e.Protocol) && e.Protocol.Equals("OAuth2", StringComparison.OrdinalIgnoreCase)).Location;
        }
    }
}
