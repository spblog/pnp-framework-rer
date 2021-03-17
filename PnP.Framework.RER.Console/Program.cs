using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Framework.RER.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddUserSecrets<Program>()
                .Build();
            var creds = configuration.Get<SharePointAppCreds>();

            var authManager = new AuthenticationManager();

            var context = authManager.GetACSAppOnlyContext(creds.SiteUrl, creds.ClientId, creds.ClientSecret);
            context.Load(context.Web);
            var list = context.Web.GetListByUrl("Lists/TestList");
            await context.ExecuteQueryRetryAsync();

            //await AddEventReceiver(list, "https://0a3a15020784.ngrok.io/api/ProcessItemEvents", EventReceiverType.ItemAdded);
            //await AddEventReceiver(list, "https://0a3a15020784.ngrok.io/api/ProcessItemEvents", EventReceiverType.ItemAdding);
            //await RemoveEventReceiver(list, "azfunc-added");
        }

        private static async Task AddEventReceiver(List list, string url, EventReceiverType type)
        {
            var eventReceiver =
                new EventReceiverDefinitionCreationInformation
                {
                    EventType = type,
                    ReceiverName = "azfunc-added",
                    ReceiverUrl = url,
                    SequenceNumber = 1000
                };

            list.EventReceivers.Add(eventReceiver);

            await list.Context.ExecuteQueryRetryAsync();
        }

        private static async Task RemoveEventReceiver(List list, string name)
        {
            var receivers = list.EventReceivers;
            list.Context.Load(receivers);
            await list.Context.ExecuteQueryRetryAsync();

            var toDelete = receivers.ToList().SingleOrDefault(r => r.ReceiverName == name);
            if(toDelete != null)
            {
                toDelete.DeleteObject();
                await list.Context.ExecuteQueryRetryAsync();
            }
        }
    }
}
