using System.Runtime.Serialization;

namespace PnP.Framework.RER.Common.EventReceivers
{
    [DataContract(Name = "RemoteWebEventProperties", Namespace = "http://schemas.microsoft.com/sharepoint/remoteapp/")]
    public class SPRemoteWebEventProperties
    {
        [DataMember]
        public string FullUrl
        {
            get;
            set;
        }

        [DataMember]
        public string ServerRelativeUrl
        {
            get;
            set;
        }

        [DataMember]
        public string NewServerRelativeUrl
        {
            get;
            set;
        }
    }
}
