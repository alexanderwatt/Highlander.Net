/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Highlander.Utilities.Threading;

#endregion

namespace Highlander.Utilities.NamedValues
{
    public delegate void LogStringDelegate(string text);

    /// <summary>
    /// A set of NameValue objects.
    /// </summary>
    [Serializable]
    public class NamedValueSet : ISerializable 
    {
        private readonly Guarded<Dictionary<string, NamedValue>> _dict = 
            new Guarded<Dictionary<string,NamedValue>>(new Dictionary<string, NamedValue>());
        private bool _frozen;

        public static string Serialise(NamedValueSet nvs)
        {
            return nvs?.Serialise();
        }

        /// <summary>
        /// Replaces the tokens.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public string ReplaceTokens(string input)
        {
            string lastResult = null;
            string result = input;
            while (lastResult != result)
            {
                NamedValue[] values = ToArray();
                lastResult = result;
                foreach (NamedValue nv in values)
                {
                    string pattern = "{" + nv.Name + "}";
                    string replacement = nv.ValueString;
                    result = Regex.Replace(result, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
            } // while
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="NamedValueSet"/> is frozen.
        /// </summary>
        /// <value><c>true</c> if frozen; otherwise, <c>false</c>.</value>
        public bool Frozen => _frozen;

        /// <summary>
        /// Gets a number of members in this <see cref="NamedValueSet"/>.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                int result = 0;
                _dict.Locked(dict => result = dict.Count);
                return result;
            }
        }
        /// <summary>
        /// Constructs an empty, modifiable <see cref="NamedValueSet"/>.
        /// </summary>
        public NamedValueSet()
        {
        }

        /// <summary>
        /// Constructs a modifiable <see cref="NamedValueSet"/> by copying another.
        /// </summary>
        /// <param name="nvs">The NVS.</param>
        public NamedValueSet(NamedValueSet nvs)
        {
            // constructs by cloning another
            if (nvs != null)
            {
                _dict.Locked(dict =>
                {
                    foreach (NamedValue nv in nvs.ToColl())
                        dict[nv.Name.ToLowerInvariant()] = nv;
                });
            }
        }

        /// <summary>
        /// Constructs a modifiable <see cref="NamedValueSet"/> by copying 2 others.
        /// </summary>
        /// <param name="nvs1">The NVS.</param>
        /// <param name="nvs2"> </param>
        public NamedValueSet(NamedValueSet nvs1, NamedValueSet nvs2)
        {
            // constructs by cloning another
            _dict.Locked(dict =>
            {
                if (nvs1 != null)
                {
                    foreach (NamedValue nv in nvs1.ToColl())
                        dict[nv.Name.ToLowerInvariant()] = nv;
                }
                if (nvs2 != null)
                {
                    foreach (NamedValue nv in nvs2.ToColl())
                        dict[nv.Name.ToLowerInvariant()] = nv;
                }
            });
        }

        /// <summary>
        /// Constructs a modifiable <see cref="NamedValueSet"/> with a single named value.
        /// </summary>
        /// <param name="nv">The NVS.</param>
        public NamedValueSet(NamedValue nv)
        {
            if (nv != null)
            {
                _dict.Locked(dict =>
                {
                    dict[nv.Name.ToLowerInvariant()] = nv;
                });
            }
        }

        /// <summary>
        /// Constructs a modifiable <see cref="NamedValueSet"/> with a collection of named values.
        /// </summary>
        /// <param name="nvs">The NVS.</param>
        public NamedValueSet(IEnumerable<NamedValue> nvs)
        {
            if (nvs != null)
            {
                _dict.Locked(dict =>
                {
                    foreach (NamedValue nv in nvs)
                    {
                        dict[nv.Name.ToLowerInvariant()] = nv;
                    }
                });
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="names">The names array.</param>
        /// <param name="values">The values array.</param>
        public NamedValueSet(string[] names, object[] values)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (names.Length != values.Length)
                throw new ArgumentException("names.Length != values.Length");
            _dict.Locked(dict =>
            {
                for (int i = 0; i < names.Length; i++)
                {
                    var nv = new NamedValue(names[i], values[i]);
                    dict[nv.Name.ToLowerInvariant()] = nv;
                }
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public NamedValueSet(IDictionary dictionary)
        {
            if (dictionary != null)
            {
                _dict.Locked(dict =>
                {
                    foreach (DictionaryEntry dictionaryEntry in dictionary)
                    {
                        var nv = new NamedValue(dictionaryEntry.Key.ToString(), dictionaryEntry.Value);
                        dict[nv.Name.ToLowerInvariant()] = nv;
                    }
                });
            }
        }

        public NamedValueSet(object[,] properties)
        {
            if (properties != null)
            {
                _dict.Locked(dict =>
                {
                    for (int i = 0; i < properties.GetLength(0); i++)
                    {
                        if (properties[i, 0] != null)
                        {
                            var nv = new NamedValue(properties[i, 0].ToString(), properties[i, 1]);
                            dict[nv.Name.ToLowerInvariant()] = nv;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public NamedValueSet(IEnumerable<KeyValuePair<string, string>> dictionary)
        {
            if (dictionary != null)
            {
                _dict.Locked(dict =>
                {
                    foreach (KeyValuePair<string, string> kvp in dictionary)
                    {
                        var nv = new NamedValue(kvp.Key, kvp.Value);
                        dict[nv.Name.ToLowerInvariant()] = nv;
                    }
                });
            }
        }

        /// <summary>
        /// Constructs a modifiable <see cref="NamedValueSet"/> by deserializing.
        /// </summary>
        /// <param name="text">The text.</param>
        public NamedValueSet(string text)
        {
            Deserialise(text);
        }

        private void Deserialise(string text)
        {
            // construct from serialised text
            if (text != null)
            {
                string delims = NameConst.sepList + Environment.NewLine;
                string[] textParts = text.Split(delims.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                _dict.Locked(dict =>
                {
                    foreach (string textPart in textParts)
                    {
                        if (!string.IsNullOrEmpty(textPart.Trim()))
                        {
                            var nv = new NamedValue(textPart);
                            dict[nv.Name.ToLowerInvariant()] = nv;
                        }
                    }
                });
            }
        }

        public static NamedValueSet DeserializeFromFile(string fileName)
        {
            using (var streamReader = new StreamReader(fileName))
            {
                string text = streamReader.ReadToEnd();
                return new NamedValueSet(text);
            }
        }

        /// <summary>
        /// Serializes this <see cref="NamedValueSet"/>.
        /// </summary>
        /// <returns></returns>
        public string Serialise()
        {
            var sb = new StringBuilder();
            NamedValue[] namedValues = ToArray();
            for (int i = 0; i < namedValues.Length; i++)
            {
                if (i > 0)
                    sb.Append(NameConst.sepList);
                sb.Append(namedValues[i].Serialise());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            NamedValue[] namedValues = ToArray();
            for (int i = 0; i < namedValues.Length; i++)
            {
                if (i > 0)
                    sb.Append(NameConst.sepList);
                sb.Append(namedValues[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Logs this <see cref="NamedValueSet"/>.
        /// </summary>
        public void LogValues(LogStringDelegate logger)
        {
            NamedValue[] namedValues = ToArray();
            foreach (NamedValue t in namedValues)
                t.LogValue(logger);
        }

        /// <summary>
        /// Converts this <see cref="NamedValueSet"/> to a collection of NameValue.
        /// </summary>
        /// <returns></returns>
        public ICollection<NamedValue> ToColl()
        {
            ICollection<NamedValue> result = null;
            _dict.Locked(dict =>
            {
                result = dict.Values;
            });
            return result;
        }

        /// <summary>
        /// Converts this <see cref="NamedValueSet"/> to a Dictionary 0f <see cref="string"/> and <see cref="object"/>.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>();
            _dict.Locked(dict =>
            {
                foreach (NamedValue nv in dict.Values)
                    result.Add(nv.Name, nv.Value);
            });
            return result;
        }

        /// <summary>
        /// Converts this <see cref="NamedValueSet"/> to an array of NameValue.
        /// </summary>
        /// <returns></returns>
        public NamedValue[] ToArray()
        {
            NamedValue[] result = null;
            _dict.Locked(dict =>
            {
                result = dict.Values.ToArray();
            });
            return result;
        }
        /// <summary>
        /// Finds the NameValue object with the given name. Throws an exception if a mandatory name is not found.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isMandatory">if set to <c>true</c> [is mandatory].</param>
        /// <returns></returns>
        public NamedValue Get(string name, bool isMandatory)
        {
            NamedValue result = null;
            string key = name.ToLowerInvariant();
            _dict.Locked(dict => dict.TryGetValue(key, out result));
            // mandatory value - fail if not found
            if (isMandatory && (result == null))
                throw new ArgumentException("Mandatory value not set!", name);
            return result;
        }

        /// <summary>
        /// Finds the NameValue object with the given name. 
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public NamedValue Get(string name)
        {
            return Get(name, false);
        }

        /// <summary>
        /// Finds the string value with the given name. Throws an exception if a mandatory name is not found.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="isMandatory">The isMandatory flag. </param>
        /// <returns></returns>
        public string GetString(string name, bool isMandatory)
        {
            NamedValue nv = Get(name, isMandatory);
            if (nv == null)
                return "";
            return nv.AsValue<string>();
        }

        public string GetString(string name, string defaultValue)
        {
            NamedValue nv = Get(name, false);
            if (nv == null)
                return defaultValue;
            return nv.AsValue<string>();
        }

        /// <summary>
        /// Gets the value with the given name, or the type's default value if not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public T GetValue<T>(string name)
        {
            NamedValue nv = Get(name, false);
            if (nv == null)
                return default;
            return nv.AsValue<T>();
        }
        /// <summary>
        /// Gets the value with the given name, or the given default value if not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T GetValue<T>(string name, T defaultValue)
        {
            NamedValue nv = Get(name, false);
            if (nv == null)
                return defaultValue;
            return nv.AsValue<T>();
        }

        /// <summary>
        /// Gets the nullable typed value with the given name, or null if not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public T? GetNullable<T>(string name) where T : struct
        {
            T? result = null;
            NamedValue nv = Get(name, false);
            if (nv != null)
                result = nv.AsValue<T>();
            return result;
        }

        /// <summary>
        /// Gets the value with the given name. Throws an exception if a mandatory name is not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="isMandatory">if set to <c>true</c> [is mandatory].</param>
        /// <returns></returns>
        public T GetValue<T>(string name, bool isMandatory)
        {
            NamedValue nv = Get(name, isMandatory);
            if (nv == null)
                return default;
            return nv.AsValue<T>();
        }

        /// <summary>
        /// Finds the array value with the given name, or returns an empty array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name of the array.</param>
        /// <returns></returns>
        public T[] GetArray<T>(string name)
        {
            NamedValue nv = Get(name, false);
            return nv?.AsArray<T>();
        }

        /// <summary>
        /// Finds the array value with the given name, or returns the default provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name of the array.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T[] GetArray<T>(string name, T[] defaultValue)
        {
            NamedValue nv = Get(name, false);
            if (nv == null)
                return defaultValue;
            return nv.AsArray<T>();
        }

        /// <summary>
        /// Sets the specified new named value.
        /// </summary>
        /// <param name="name">The new named value.</param>
        /// <param name="value">The value </param>
        /// <returns></returns>
        public void Set(string name, object value)
        {
            if (_frozen)
                throw new InvalidOperationException("Cannot modify frozen object");
            string key = name.ToLowerInvariant();
            if (value != null)
            {
                _dict.Locked(dict =>
                {
                    var nv = new NamedValue(name, value);
                    dict[key] = nv;
                });
            }
            else
            {
                // null - remove named value
                _dict.Locked(dict => { dict.Remove(key); });
            }
        }
        public void Set(NamedValue nv)
        {
            if (null == nv)
                return;
            if (_frozen)
                throw new InvalidOperationException("Cannot modify frozen object");
            _dict.Locked(dict =>
            {
                dict[nv.Name.ToLowerInvariant()] = nv;
            });
        }
        public void Add(NamedValueSet nvs)
        {
            if (nvs != null)
            {
                _dict.Locked(dict =>
                {
                    foreach (NamedValue nv in nvs.ToColl())
                    {
                        dict[nv.Name.ToLowerInvariant()] = nv;
                    }
                });
            }
        }

        public void Add<T>(IDictionary<string, T> newValues)
        {
            _dict.Locked(dict =>
            {
                foreach (string name in newValues.Keys)
                {
                    var nv = new NamedValue(name, newValues[name]);
                    dict[nv.Name.ToLowerInvariant()] = nv;
                }
            });
        }

        /// <summary>
        /// Clears all names.
        /// </summary>
        /// <returns></returns>
        public void Clear()
        {
            if (_frozen)
                throw new InvalidOperationException("Cannot modify frozen object");
            _dict.Locked(dict => dict.Clear());
        }

        /// <summary>
        /// Freezes this instance.
        /// </summary>
        /// <returns></returns>
        public NamedValueSet Freeze()
        {
            // freezes this set (makes it readonly)
            _dict.Locked(dict => _frozen = true);
            return this;
        }

        /// <summary>
        /// Clones (deep copies) this instance.
        /// </summary>
        /// <returns></returns>
        public NamedValueSet Clone()
        {
            return new NamedValueSet(this);
        }

        protected NamedValueSet(SerializationInfo info, StreamingContext context)
        {
            var serializedState = (string)info.GetValue("serializedState", typeof(string));
            Deserialise(serializedState);
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("serializedState", Serialise());
        }

    }
}