using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using System.Linq;
using PnP.Framework.RER.Common.Helpers;
using PnP.Framework.RER.Common.EventReceivers;
using System;
using System.Web.Http;
using PnP.Framework.RER.Common.Tokens;
using Microsoft.SharePoint.Client;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace PnP.Framework.RER.Functions
{
    public class RemoteEventFunctions
    {
        private readonly TokenManagerFactory _tokenManagerFactory;
        private readonly IHostingEnvironment _hostingEnvironment;

        public RemoteEventFunctions(TokenManagerFactory tokenManagerFactory, IHostingEnvironment hostingEnvironment)
        {
            _tokenManagerFactory = tokenManagerFactory;
            _hostingEnvironment = hostingEnvironment;
        }

        [FunctionName("ProcessItemEvents")]
        public async Task<IActionResult> ProcessItemEvents([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var xdoc = XDocument.Parse(requestBody);

                var eventRoot = xdoc.Root.Descendants().First().Descendants().First();

                if (eventRoot.Name.LocalName != "ProcessEvent" && eventRoot.Name.LocalName != "ProcessOneWayEvent")
                {
                    throw new Exception($"Unable to resolve event type");
                }

                var payload = eventRoot.FirstNode.ToString();
                var eventProperties = SerializerHelper.Deserialize<SPRemoteEventProperties>(payload);

                var host = req.Host.Host;
                if (_hostingEnvironment.IsDevelopment())
                {
                    host = Environment.GetEnvironmentVariable("ngrokHost");
                }

                var tokenManager = _tokenManagerFactory.Create(eventProperties, host);

                var context = await tokenManager.GetUserClientContextAsync(eventProperties.ItemEventProperties.WebUrl);
                context.Load(context.Web);
                await context.ExecuteQueryRetryAsync();

                if (eventRoot.Name.LocalName == "ProcessEvent")
                {
                    return await ProcessSyncEvent(eventProperties, context);
                }

                if (eventRoot.Name.LocalName == "ProcessOneWayEvent")
                {
                    return await ProcessAsyncEvent(eventProperties, context);
                }

                throw new Exception($"Unable to resolve event type");
            }
            catch (Exception ex)
            {
                log.LogError(new EventId(), ex, ex.Message);
                var result = new SPRemoteEventResult
                {
                    Status = SPRemoteEventServiceStatus.CancelWithError,
                    ErrorMessage = ex.Message
                };

                return new ContentResult
                {
                    Content = CreateEventResponse(result),
                    ContentType = "text/xml",
                    StatusCode = (int?)HttpStatusCode.InternalServerError
                };
            }
        }

        // -ing events, i.e ItemAdding
        private async Task<IActionResult> ProcessSyncEvent(SPRemoteEventProperties properties, ClientContext context)
        {
            switch (properties.EventType)
            {
                case SPRemoteEventType.ItemAdding:
                    {
                        // do things
                        break;
                    }
                //etc
                default: { break; }
            }
            var result = new SPRemoteEventResult
            {
                Status = SPRemoteEventServiceStatus.Continue
            };

            return new ContentResult
            {
                Content = CreateEventResponse(result),
                ContentType = "text/xml",
                StatusCode = (int?)HttpStatusCode.OK
            };
        }

        // -ed events, i.e. ItemAdded
        private async Task<IActionResult> ProcessAsyncEvent(SPRemoteEventProperties properties, ClientContext context)
        {
            switch (properties.EventType)
            {
                case SPRemoteEventType.ItemAdded:
                    {
                        // do things
                        break;
                    }
                //etc
                default: { break; }
            }
            return new OkResult();
        }

        private string CreateEventResponse(SPRemoteEventResult eventResult)
        {
            var responseTemplate = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
                                        <s:Body>{0}</s:Body>
                                    </s:Envelope>";
            var result = new ProcessEventResponse
            {
                ProcessEventResult = eventResult
            };
            var content = SerializerHelper.Serialize(result);

            return string.Format(responseTemplate, content);
        }
    }
}
