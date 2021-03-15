using Microsoft.Extensions.DependencyInjection;
using PnP.Framework.RER.Common.EventReceivers;
using PnP.Framework.RER.Common.Model;
using System;
using System.Net.Http;

namespace PnP.Framework.RER.Common.Tokens
{
    public class TokenManagerFactory
    {
        private IServiceProvider _serviceProvider;

        public TokenManagerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TokenManager Create(SPRemoteEventProperties eventProperties, string host)
        {
            if (!string.IsNullOrEmpty(eventProperties.ErrorMessage))
            {
                throw new Exception($"Event data contains error. Message: {eventProperties.ErrorMessage}. Code: {eventProperties.ErrorCode}");
            }

            if (string.IsNullOrEmpty(eventProperties.ContextToken))
            {
                throw new Exception($"Context token is empty");
            }

            return new TokenManager(
                _serviceProvider.GetRequiredService<SharePointAppCreds>(),
                _serviceProvider.GetRequiredService<HttpClient>(),
                eventProperties.ContextToken, host);
        }
    }
}
