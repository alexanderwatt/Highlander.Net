/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

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
using System.Xml;

namespace Highlander.Utilities.Helpers
{
    /// <summary>
    /// Xml Helper class
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Gets the node value.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="fromNode">From node.</param>
        /// <returns></returns>
        public static string GetNodeValueString(string nodeName, XmlNode fromNode)
        {
            string value = string.Empty;
            XmlNode node = fromNode.SelectSingleNode(nodeName);
            if (node != null)
            {
                value = node.InnerText;
            }
            return value;
        }

        public static bool GetNodeValueBool(string nodeName, XmlNode fromNode)
        {
            bool value = false;
            string strVal = GetNodeValueString(nodeName, fromNode);
            if (strVal != "")
                value = bool.Parse(strVal);            
            return value;
        }

        /// <summary>
        /// Gets the node value date.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="fromNode">From node.</param>
        /// <returns></returns>
        public static DateTime GetNodeValueDate(string nodeName, XmlNode fromNode)
        {
            DateTime.TryParse(GetNodeValueString(nodeName, fromNode), out var value);
            return value;
        }

        public static double GetNodeValueDouble(string nodeName, XmlNode fromNode)
        {
            double.TryParse(GetNodeValueString(nodeName, fromNode), out var value);
            return value;
        }

        public static decimal GetNodeValueDecimal(string nodeName, XmlNode fromNode)
        {
            decimal.TryParse(GetNodeValueString(nodeName, fromNode), out var value);
            return value;
        }

        public static XmlElement AddDocumentElementToDom(XmlDocument document, string name)
        {
            XmlElement element = document.CreateElement(name);
            document.AppendChild(element);
            return element;
        }

        public static XmlElement AddElementToDom(XmlDocument document, XmlElement parentElement, string name)
        {
            XmlElement element = document.CreateElement(name);
            parentElement.AppendChild(element);
            return element;
        }

        public static XmlElement AddNodeToDom(XmlDocument document, XmlElement parentElement, string name, object value)
        {
            XmlElement node = document.CreateElement(name);
            node.InnerText = value.ToString();
            parentElement.AppendChild(node);

            return node;
        }

        public static XmlAttribute AddAttribute(XmlDocument document, XmlElement element, string name, object value)
        {
            XmlAttribute attribute = document.CreateAttribute(name);
            attribute.InnerText = value.ToString();
            element.Attributes.Append(attribute);
            return attribute;
        }
    }
}
