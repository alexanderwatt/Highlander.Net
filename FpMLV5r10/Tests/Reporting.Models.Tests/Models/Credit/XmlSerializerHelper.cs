#region Using directives

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

#endregion

namespace National.QRSC.AnalyticModels.Tests.Models.Credit
{
    /// <summary>
    /// Xml Serialization Helper
    /// </summary>
    public static class XmlSerializerHelper
    {
        private static readonly Dictionary<Type, XmlSerializer> _serializersCache = new Dictionary<Type, XmlSerializer>();
        
        public  static XmlSerializer   GetSerializer(Type type)
        {
            if (_serializersCache.ContainsKey(type))
            {
                return _serializersCache[type];
            }

            XmlSerializer serializer = new XmlSerializer(type);

            _serializersCache.Add(type, serializer);

            return serializer;
        }


        /// <summary>
        /// Clones the specified object to clone.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToClone">The object to clone.</param>
        /// <returns></returns>
        public static T Clone<T>(T objectToClone)
        {
            XmlSerializer serializer = GetSerializer(typeof(T));

            using(MemoryStream memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, objectToClone);

                memoryStream.Position = 0;

                T deserializedObject = (T)serializer.Deserialize(memoryStream);

                return deserializedObject;
            }
        }

        /// <summary>
        /// Deserializes the node.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static T DeserializeNode<T>(XmlNode node)
        {
            using(StringReader stringReader = new StringReader(node.OuterXml))
            {
                XmlSerializer serializer = GetSerializer(typeof(T));

                T deserializedObject = (T)serializer.Deserialize(stringReader);

                return deserializedObject;
            }
        }

        /// <summary>
        /// Deserializes from string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public static T DeserializeFromString<T>(string serializedObject)
        {
            return DeserializeFromString<T>(typeof(T), serializedObject);
        }

        /// <summary>
        /// Deserializes from string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asRootType">Type of as root.</param>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public static T DeserializeFromString<T>(Type asRootType, string serializedObject)
        {
            using (StringReader stringReader = new StringReader(serializedObject))
            {
                XmlSerializer serializer = GetSerializer(asRootType);

                T deserializedObject = (T)serializer.Deserialize(stringReader);

                return deserializedObject;
            }
        }

        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns></returns>
        public static string SerializeToString(object objectToSerialize)
        {
            return SerializeToString(objectToSerialize.GetType(), objectToSerialize);
        }

        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <param name="asRootType">Type of as root.</param>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns></returns>
        public static string SerializeToString(Type asRootType, object objectToSerialize)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using(XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    xmlTextWriter.Formatting = Formatting.Indented;

                    XmlSerializer xmlSerializer = GetSerializer(asRootType);
                    xmlSerializer.Serialize(xmlTextWriter, objectToSerialize);

                    memoryStream.Position = 0;

                    using(StreamReader reader = new StreamReader(memoryStream))
                    {
                        string buf = reader.ReadToEnd();

                        return buf;
                    }
                }
            }
        }

        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asRootType">Type of as root.</param>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="filename">The filename.</param>
        public static void SerializeToFile<T>(Type asRootType, T objectToSerialize, string filename)
        {
            using (StreamWriter streamWriter = new StreamWriter(filename))
            {
                streamWriter.Write(SerializeToString(asRootType, objectToSerialize));
            }
        }

        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="filename">The filename.</param>
        public static void SerializeToFile<T>(T objectToSerialize, string filename)
        {
            using (StreamWriter streamWriter = new StreamWriter(filename))
            {
                streamWriter.Write(SerializeToString(objectToSerialize));
            }
        }

        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string filename)
        {
            using (StreamReader streamReader = new StreamReader(filename))
            {
                XmlSerializer serializer = GetSerializer(typeof(T));

                T t = (T)serializer.Deserialize(streamReader);

                return t;
            }
        }

        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static T DeserializeFile<T>(string filename)
        {
            return DeserializeFromFile<T>(filename);
        }


        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asRootType">Type of as root.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(Type asRootType, string filename)
        {
            using (StreamReader streamReader = new StreamReader(filename))
            {
                XmlSerializer serializer = GetSerializer(asRootType);

                T t = (T)serializer.Deserialize(streamReader);

                return t;
            }
        }

        /// <summary>
        /// Ares the equal.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns></returns>
        public static   bool    AreEqual(object obj1, object obj2)
        {
            return SerializeToString(obj1) == SerializeToString(obj2);
        }

    }
}
