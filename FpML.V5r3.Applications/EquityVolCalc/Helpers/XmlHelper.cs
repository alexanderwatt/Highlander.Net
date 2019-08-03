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

namespace Orion.Equity.VolatilityCalculator.Helpers
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
        internal static string GetNodeValueString(string nodeName, XmlNode fromNode)
        {
            string value = string.Empty;
            XmlNode node = fromNode.SelectSingleNode(nodeName);
            if (node != null)
            {
                value = node.InnerText;
            }
            return value;
        }

        internal static Boolean GetNodeValueBool(string nodeName, XmlNode fromNode)
        {
            Boolean value = false;
            string strval = GetNodeValueString(nodeName, fromNode);
            if (strval != "")
                value = Boolean.Parse(strval);            
            return value;
        }

        /// <summary>
        /// Gets the node value date.
        /// </summary>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="fromNode">From node.</param>
        /// <returns></returns>
        internal static DateTime GetNodeValueDate(string nodeName, XmlNode fromNode)
        {
            DateTime.TryParse(GetNodeValueString(nodeName, fromNode), out var value);
            return value;
        }

        internal static Double GetNodeValueDouble(string nodeName, XmlNode fromNode)
        {
            Double.TryParse(GetNodeValueString(nodeName, fromNode), out var value);
            return value;
        }

        internal static Decimal GetNodeValueDecimal(string nodeName, XmlNode fromNode)
        {
            Decimal.TryParse(GetNodeValueString(nodeName, fromNode), out var value);
            return value;
        }

        internal static XmlElement AddDocumentElementToDom(XmlDocument document, string name)
        {
            XmlElement element = document.CreateElement(name);
            document.AppendChild(element);
            return element;
        }

        internal static XmlElement AddElementToDom(XmlDocument document, XmlElement parentElement, string name)
        {
            XmlElement element = document.CreateElement(name);
            parentElement.AppendChild(element);
            return element;
        }

        internal static XmlElement AddNodeToDom(XmlDocument document, XmlElement parentElement, string name, object value)
        {
            XmlElement node = document.CreateElement(name);
            node.InnerText = value.ToString();
            parentElement.AppendChild(node);

            return node;
        }

        internal static XmlAttribute AddAttribute(XmlDocument document, XmlElement element, string name, object value)
        {
            XmlAttribute attrib = document.CreateAttribute(name);
            attrib.InnerText = value.ToString();
            element.Attributes.Append(attrib);
            return attrib;
        }
    }
}
