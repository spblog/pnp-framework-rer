using System.Runtime.Serialization;

namespace PnP.Framework.RER.Common.EventReceivers
{
    [DataContract(Name = "RemoteEntityEventProperties", Namespace = "http://schemas.microsoft.com/sharepoint/remoteapp/")]
    public class SPRemoteEntityInstanceEventProperties
    {
        [DataMember]
        public string EntityName
        {
            get;
            set;
        }

        [DataMember]
        public string EntityNamespace
        {
            get;
            set;
        }

        [DataMember]
        public string NotificationContext
        {
            get;
            set;
        }

        [DataMember]
        public string LobSystemInstanceName
        {
            get;
            set;
        }

        [DataMember]
        public byte[] NotificationMessage
        {
            get;
            set;
        }
    }
}
