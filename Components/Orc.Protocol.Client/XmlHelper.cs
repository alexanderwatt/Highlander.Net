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
using System;
using System.IO;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;


namespace Highlander.Orc.Client
{
    public sealed class XmlHelper
    {
        private readonly string _prefix;
        private readonly Uri _namespace;

        public XmlHelper()
        {
            // Default public constructor
        }

        public XmlHelper(string prefix, Uri uri)
        {
            _prefix = prefix;
            _namespace = uri;
        }

        public bool SelectBoolean(XPathNavigator navigator, string xpath)
        {
            string value = SelectString(navigator, xpath, null);
            return (0 == string.Compare(value.ToUpper(), "TRUE", true, CultureInfo.InvariantCulture));
        }

        public bool SelectBoolean(XPathNavigator navigator, string xpath, bool defaultValue)
        {
            string value = SelectString(navigator, xpath, string.Empty);
            if (value.Trim().Length == 0)
            {
                return defaultValue;
            }

            return (0 == string.Compare(value.ToUpper(), "TRUE", true, CultureInfo.InvariantCulture));
        }

        public DateTime SelectDate(XPathNavigator navigator, string xpath)
        {
            string value = SelectString(navigator, xpath, null);
            return Convert.ToDateTime(value);
        }

        public DateTime SelectDate(XPathNavigator navigator, string xpath, DateTime defaultValue)
        {
            string value = SelectString(navigator, xpath, string.Empty);
            if (value.Trim().Length == 0)
            {
                return defaultValue;
            }

            return Convert.ToDateTime(value);
        }

        public decimal SelectDecimal(XPathNavigator navigator, string xpath)
        {
            string value = SelectString(navigator, xpath, null);
            decimal toDecimal = Convert.ToDecimal(value);
            return toDecimal;
        }

        public decimal SelectDecimal(XPathNavigator navigator, string xpath, decimal defaultValue)
        {
            string value = SelectString(navigator, xpath, string.Empty);
            if (value.Trim().Length == 0)
            {
                return defaultValue;
            }
            else
            {
                return Convert.ToDecimal(value);
            }
        }

        public string SelectString(XPathNavigator navigator, string xpath)
        {
            return SelectString(navigator, xpath, null);
        }

        public string SelectString(XPathNavigator navigator, string xpath, string defaultValue)
        {
            XPathExpression expression = navigator.Compile(xpath);
            expression.SetContext(GetNamespaceManager(navigator));
            XPathNodeIterator iterator = navigator.Select(expression);
            if ((iterator.Count < 1) && (defaultValue != null))
            {
                return defaultValue;
            }
            if ((iterator.Count < 1) && (defaultValue == null))
            {
                throw new ApplicationException("XmlNode Is Missing '" + xpath + "'");
            }
            if (iterator.Count > 1)
            {
                throw new ApplicationException("XmlNode Is Multiple '" + xpath + "'");
            }
            iterator.MoveNext();
            return iterator.Current.Value;
        }

        public XPathNavigator @Select(XPathNavigator navigator, string xpath)
        {
            XPathExpression expression = navigator.Compile(xpath);
            expression.SetContext(GetNamespaceManager(navigator));
            XPathNodeIterator iterator = navigator.Select(expression);
            iterator.MoveNext();
            return iterator.Current;
        }

        public object Evaluate(XPathNavigator navigator, string xpath)
        {
            XPathExpression expression = navigator.Compile(xpath);
            expression.SetContext(GetNamespaceManager(navigator));
            return navigator.Evaluate(expression);
        }

        public XmlNamespaceManager GetNamespaceManager(XPathNavigator navigator)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(navigator.NameTable);
            if ((_prefix != string.Empty) && (_namespace != null))
            {
                nsManager.AddNamespace(_prefix, _namespace.ToString());
            }
            return nsManager;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Determines whether the specified XPath expression will select a non-empty node set on the provided XPathNavigator.
        /// </summary>
        /// <param name="navigator">The XPathNavigator to use.</param>
        /// <param name="xpath">A string representing an XPath expression.</param>
        /// <returns><see langword="true"/> if the xpath matches a defined constant; otherwise <see langword="false"/>.</returns>
        /// -----------------------------------------------------------------------------
        public bool HasValue(XPathNavigator navigator, string xpath)
        {
            try
            {
                XPathExpression expression = navigator.Compile(xpath);
                expression.SetContext(GetNamespaceManager(navigator));
                XPathNodeIterator iterator = navigator.Select(expression);
                return (iterator.Count > 0);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Creates a new XPathNavigator object for navigating an XML snippet.
        /// </summary>
        /// <param name="input">XML snippet to use.</param>
        /// <returns>An XPathNavigator object.</returns>
        /// -----------------------------------------------------------------------------
        public static XPathNavigator CreateNavigator(string input)
        {
            var document = new XPathDocument(new StringReader(input));
            return document.CreateNavigator();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Serializes the input object to a string.
        /// </summary>
        /// <param name="input">Item to serialize.</param>
        /// <param name="type">Type to be used for serialization.</param>
        /// <returns>Serialized version of the <paramref name="input"/>.</returns>
        /// -----------------------------------------------------------------------------
        public static string Serialize(object input, Type type)
        {
            return Serialize(input, type, Encoding.UTF8);
        }

        public static string Serialize(object input, Type type, Encoding encoding)
        {
            return Serialize(input, type, Encoding.UTF8, string.Empty, string.Empty);
        }
        public static string Serialize(object input, Type type, Encoding encoding, string prefix, string ns)
        {
            StringWriter writer = new EncodingStringWriter(encoding);
            XmlSerializer serializer = new XmlSerializer(type);
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            xsn.Add(prefix, ns);
            serializer.Serialize(writer, input, xsn);
            writer.Flush();
            return writer.ToString();
        }

        public static string Serialize(object input, Type type, Encoding encoding, string prefix, string ns, XmlAttributeOverrides overrides)
        {
            StringWriter writer = new EncodingStringWriter(encoding);
            XmlSerializer serializer = new XmlSerializer(type, overrides);
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            xsn.Add(prefix, ns);
            serializer.Serialize(writer, input, xsn);
            writer.Flush();
            return writer.ToString();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     Deserializes the input string to an object.
        /// </summary>
        /// <param name="input">String to deserialize.</param>
        /// <param name="type">Type to be used for de-serialization.</param>
        /// <returns>De-serialized version of the <paramref name="input"/>.</returns>
        /// -----------------------------------------------------------------------------
        public static object Deserialize(string input, Type type)
        {
            StringReader reader = new StringReader(input);
            XmlSerializer serializer = new XmlSerializer(type);
            return serializer.Deserialize(reader);
        }

        public static object Deserialize(string input, Type type, XmlAttributeOverrides overrides)
        {
            StringReader reader = new StringReader(input);
            XmlSerializer serializer = new XmlSerializer(type, overrides);
            return serializer.Deserialize(reader);
        }

        private class EncodingStringWriter : StringWriter
        {
            public EncodingStringWriter(Encoding encoding)
            {
                Encoding = encoding;
            }

            public override Encoding Encoding { get; }
        }

        /// <summary>
        /// Extract the XmlRootAttribute element name for a serialisable object.
        /// </summary>
        /// <param name="dataType">The typeof for the object being examined</param>
        /// <returns>The ElementName attribute</returns>
        public static string GetRootElementName(Type dataType)
        {
            string sElementName = "";
            System.Reflection.MemberInfo info = dataType;
            object[] attributes = info.GetCustomAttributes(true);
            for (int i = 0; i < attributes.Length; i++)
            {
                object attribute = attributes[i];
                if (attribute.GetType() == typeof(XmlRootAttribute))
                    sElementName = ((XmlRootAttribute)attribute).ElementName;
            }
            return sElementName;
        }
    }
}
