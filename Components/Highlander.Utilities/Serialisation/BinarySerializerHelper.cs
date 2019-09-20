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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace Highlander.Utilities.Serialisation
{
    /// <summary>
    /// Helper class for Binary serialization
    /// </summary>
    public static class BinarySerializerHelper
    {
        /// <summary>
        /// Clones the specified object to clone.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToClone">The object to clone.</param>
        /// <returns></returns>
        public static T Clone<T>(T objectToClone)
        {
            var binaryFormatter = new BinaryFormatter();
            using(var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, objectToClone);
                memoryStream.Position = 0;
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static string SerializeToString(object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, obj);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialized"></param>
        /// <returns></returns>
        public static object DeserializeFromString(string serialized)
        {
            using (var stringReader = new StringReader(serialized))
            {
                byte[] bytes = Convert.FromBase64String(stringReader.ReadToEnd());
                {
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(new MemoryStream(bytes));
                }
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
            using (var stringReader = new StringReader(serialized))
            {
                var bytes = Convert.FromBase64String(stringReader.ReadToEnd());
                object deserializedUntyped = new BinaryFormatter().Deserialize(new MemoryStream(bytes));
                return (T)deserializedUntyped;
            }
        }

        /// <summary>
        /// Serializes to byte[].
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static byte[] SerializeToByteArray(object obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialized"></param>
        /// <returns></returns>
        public static object DeserializeFromByteArray(byte[] serialized)
        {
            using (var memoryStream = new MemoryStream(serialized))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Are equal.
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
