using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PnP.Framework.RER.Common.Model;
using PnP.Framework.RER.Common.Tokens;

[assembly: FunctionsStartup(typeof(PnP.Framework.RER.Functions.Startup))]
namespace PnP.Framework.RER.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            var sharepointCreds = builder.GetContext().Configuration.GetSection(SharePointAppCreds.SectionName).Get<SharePointAppCreds>();
            builder.Services.AddSingleton(sharepointCreds);
            builder.Services.AddTransient<TokenManager>();
        }
    }
}
