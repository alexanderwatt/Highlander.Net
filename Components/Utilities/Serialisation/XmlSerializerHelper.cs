/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)
 Copyright (C) 2019 Simon Dudley

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

#endregion

namespace Highlander.Utilities.Serialisation
{
    /// <summary>
    /// Xml Serialization Helper
    /// </summary>
    public static class XmlSerializerHelper
    {
        /// <summary>
        /// Clones the specified object to clone.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToClone">The object to clone.</param>
        /// <returns></returns>
        public static T Clone<T>(T objectToClone)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, objectToClone);
                memoryStream.Position = 0;
                var deserializedObject = (T)serializer.Deserialize(memoryStream);
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
            using (var stringReader = new StringReader(node.OuterXml))
            {
                var serializer = new XmlSerializer(typeof(T));
                var deserializedObject = (T)serializer.Deserialize(stringReader);
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
            if (serializedObject == null)
                return default(T);
            using (var stringReader = new StringReader(serializedObject))
            {
                var serializer = new XmlSerializer(asRootType);
                var deserializedObject = (T)serializer.Deserialize(stringReader);
                return deserializedObject;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asRootType"></param>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        public static object DeserializeFromString(Type asRootType, string serializedObject)
        {
            if (serializedObject == null)
                return null;
            using (var stringReader = new StringReader(serializedObject))
            {
                var serializer = new XmlSerializer(asRootType);
                return serializer.Deserialize(stringReader);
            }
        }

        /// <summary>
        /// Serializes an object to an XML string.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns></returns>
        public static string SerializeToString(object objectToSerialize)
        {
            return SerializeToString(objectToSerialize.GetType(), objectToSerialize);
        }

        /// <summary>
        /// Serializes an object to an XML string, specifying the root type.
        /// </summary>
        /// <typeparam name="T">The root type.</typeparam>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns></returns>
        public static string SerializeToString<T>(T objectToSerialize)
        {
            return SerializeToString(typeof(T), objectToSerialize);
        }

        /// <summary>
        /// Serializes an object to an XML string, specifying the root type.
        /// </summary>
        /// <param name="asRootType">Type of as root.</param>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns></returns>
        public static string SerializeToString(Type asRootType, object objectToSerialize)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    var xmlSerializer = new XmlSerializer(asRootType);
                    xmlSerializer.Serialize(xmlTextWriter, objectToSerialize);
                    memoryStream.Position = 0;
                    using (var reader = new StreamReader(memoryStream))
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
        /// <param name="fileName">The file name.</param>
        public static void SerializeToFile<T>(Type asRootType, T objectToSerialize, string fileName)
        {
            using (var streamWriter = new StreamWriter(fileName))
            {
                streamWriter.Write(SerializeToString(asRootType, objectToSerialize));
            }
        }

        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <param name="fileName">The file name.</param>
        public static void SerializeToFile<T>(T objectToSerialize, string fileName)
        {
            SerializeToFile(typeof(T), objectToSerialize, fileName);
        }

        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName">The file name.</param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string fileName)
        {
            return DeserializeFromFile<T>(typeof (T), fileName);
        }

        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asRootType">Type of as root.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(Type asRootType, string fileName)
        {
            using (var streamReader = new StreamReader(fileName))
            {
                var serializer = new XmlSerializer(asRootType);
                var t = (T)serializer.Deserialize(streamReader);
                return t;
            }
        }

        /// <summary>
        /// Ares the equal.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns></returns>
        public static bool AreEqual(object obj1, object obj2)
        {
            return SerializeToString(obj1.GetType(), obj1) == SerializeToString(obj2.GetType(), obj2);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DynamicXml : DynamicObject
    {
        readonly XElement _root;

        private DynamicXml(XElement root)
        {
            _root = root;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static DynamicXml Parse(string xmlString)
        {
            return new DynamicXml(XDocument.Parse(xmlString).Root);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static DynamicXml Load(string filename)
        {
            return new DynamicXml(XDocument.Load(filename).Root);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            var att = _root.Attribute(binder.Name);
            if (att != null)
            {
                result = att.Value;
                return true;
            }
            var nodes = _root.Elements(binder.Name);
            var xElements = nodes as XElement[] ?? nodes.ToArray();
            if (xElements.Length > 1)
            {
                result = xElements.Select(n => n.HasElements ? (object)new DynamicXml(n) : n.Value).ToList();
                return true;
            }
            var node = _root.Element(binder.Name);
            if (node != null)
            {
                result = node.HasElements || node.HasAttributes ? (object)new DynamicXml(node) : node.Value;
                return true;
            }
            return true;
        }
    }
}
