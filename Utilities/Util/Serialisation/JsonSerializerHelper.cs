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
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;

#endregion

namespace Orion.Util.Serialisation
{
    /// <summary>
    /// Helper class for Soap serialization
    /// </summary>
    public static class JsonSerializerHelper
    {
        /// <summary>
        /// Clones the specified object to clone.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToClone">The object to clone.</param>
        /// <returns></returns>
        public static T Clone<T>(T objectToClone)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, objectToClone);
                var deserializedObject = (T)serializer.ReadObject(memoryStream);
                memoryStream.Close();
                return deserializedObject;
            }
        }

        /// <summary>
        /// Deserializes from string.
        /// </summary>
        /// <param name="asRootType">Type of root.</param>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public static object DeserializeFromString(Type asRootType, string serializedObject)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serializedObject)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(asRootType);
                var deserializedObject = serializer.ReadObject(memoryStream);
                memoryStream.Close();
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
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serializedObject)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                var deserializedObject = (T)serializer.ReadObject(memoryStream);
                memoryStream.Close();
                return deserializedObject;
            }
        }

        /// <summary>
        /// Serializes an object to an Json string.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns></returns>
        public static string SerializeToString(object objectToSerialize)
        {
            return SerializeToString(objectToSerialize.GetType(), objectToSerialize);
        }

        /// <summary>
        /// Serializes an object to an Json string, specifying the root type.
        /// </summary>
        /// <typeparam name="T">The root type.</typeparam>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns></returns>
        public static string SerializeToString<T>(T objectToSerialize)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Serializer the User object to the stream.  
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(memoryStream, objectToSerialize);
                byte[] json = memoryStream.ToArray();
                memoryStream.Close();
                return Encoding.UTF8.GetString(json, 0, json.Length);
            }
        }

        /// <summary>
        /// Serializes an object to a Json string, specifying the root type.
        /// </summary>
        /// <param name="asRootType">Type of root.</param>
        /// <param name="objectToSerialize">The object to serialize.</param>
        /// <returns></returns>
        public static string SerializeToString(Type asRootType, object objectToSerialize)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Serializer the User object to the stream.  
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(asRootType);
                serializer.WriteObject(memoryStream, objectToSerialize);
                byte[] json = memoryStream.ToArray();
                memoryStream.Close();
                return Encoding.UTF8.GetString(json, 0, json.Length);
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
}
