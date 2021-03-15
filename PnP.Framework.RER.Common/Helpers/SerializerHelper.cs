using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace PnP.Framework.RER.Common.Helpers
{
    public static class SerializerHelper
    {
        public static T Deserialize<T>(string xml)
        {
            using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                var serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
        }

        public static string Serialize<T>(T data)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(stream, data);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
