using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using PnP.Framework.RER.Common.Tokens;
using PnP.Framework.RER.Common.Model;

namespace PnP.Framework.RER.Functions
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddUserSecrets<Program>();
                })
                .ConfigureServices((context, services) =>
                {
                    var sharepointCreds = context.Configuration.GetSection(SharePointAppCreds.SectionName).Get<SharePointAppCreds>();
                    services.AddHttpClient();
                    services.AddSingleton<TokenManagerFactory>();
                    services.AddSingleton(sharepointCreds);
                })
                .Build();

            host.Run();
        }
    }
}