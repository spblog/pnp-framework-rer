using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PnP.Framework.RER.Common.EventReceivers
{
    public enum SPRemoteEventServiceStatus
    {
        Continue,
        CancelNoError,
        CancelWithError,
        [Obsolete("Default list forms are committed through asynchronous XmlHttpRequests, so redirect urls specified in this way aren't followed by default.  In order to force a list form to follow a cancelation redirect url, set the list form web part's CSRRenderMode property to CSRRenderMode.ServerRender")]
        CancelWithRedirectUrl
    }

    [DataContract(Name = "ProcessEventResult", Namespace = "http://schemas.microsoft.com/sharepoint/remoteapp/")]
    public class SPRemoteEventResult
    {
        private Dictionary<string, object> changedItemProperties;

        [DataMember]
        public SPRemoteEventServiceStatus Status
        {
            get;
            set;
        }

        [DataMember]
        public string ErrorMessage
        {
            get;
            set;
        }

        [DataMember]
        [Obsolete("Default list forms are committed through asynchronous XmlHttpRequests, so redirect urls specified in this way aren't followed by default.  In order to force a list form to follow a cancelation redirect url, set the list form web part's CSRRenderMode property to CSRRenderMode.ServerRender")]
        public string RedirectUrl
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<string, object> ChangedItemProperties
        {
            get
            {
                if (changedItemProperties == null)
                {
                    changedItemProperties = new Dictionary<string, object>();
                }

                return changedItemProperties;
            }
        }
    }
}
