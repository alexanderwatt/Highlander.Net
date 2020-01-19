using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;

namespace Highlander.Utilities.Serialisation
{
    /// <summary>
    /// Helper class for Soap serialization
    /// </summary>
    public static class SoapSerializerHelper
    {
        /// <summary>
        /// Clones the specified object to clone.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToClone">The object to clone.</param>
        /// <returns></returns>
        public static T Clone<T>(T objectToClone)
        {
            var soapFormatter = new SoapFormatter();
            using (var memoryStream = new MemoryStream())
            {
                soapFormatter.Serialize(memoryStream, objectToClone);
                memoryStream.Position = 0;
                var clonedObject = (T)soapFormatter.Deserialize(memoryStream);
                return clonedObject;
            }
        }

        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static string SerializeToString(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                SoapFormatter formatter = new SoapFormatter();
                formatter.Serialize(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialized"></param>
        /// <returns></returns>
        public static object DeserializeFromString(string serialized)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(serialized)))
            {
                SoapFormatter formatter = new SoapFormatter();
                return formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serialized"></param>
        /// <returns></returns>
        public static T DeserializeFromString<T>(string serialized)
        {
            return (T)DeserializeFromString(serialized);
        }

        /// <summary>
        /// Ares the equal.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns></returns>
        public static bool AreEqual(object obj1, object obj2)
        {
            return SerializeToString(obj1) == SerializeToString(obj2);
        }
    }
}
