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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Xml;

#endregion

namespace Highlander.Metadata.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface IXmlTransformer
    {
        void Transform(string inputUri, string outputUri);
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CustomXmlTransformRuleKind
    {
        TagNameTypeChange,
        RemoveDefaultValueNode,
        ContentFormat,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CustomXmlTransformTextFormat
    {
        Unchanged,
        XsdTimeOnly,
        XsdDateTime
    }

    /// <summary>
    /// 
    /// </summary>
    public class CustomXmlTransformRule
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string OldHeadPath;

        /// <summary>
        /// 
        /// </summary>
        public readonly string OldTailPath;

        /// <summary>
        /// 
        /// </summary>
        public readonly string OldNodeName;

        /// <summary>
        /// 
        /// </summary>
        public readonly string OldXsiTypeName;

        /// <summary>
        /// 
        /// </summary>
        public readonly CustomXmlTransformRuleKind Kind;

        /// <summary>
        /// 
        /// </summary>
        public readonly string NewNodeName;

        /// <summary>
        /// 
        /// </summary>
        public readonly string NewXsiTypeName;

        /// <summary>
        /// 
        /// </summary>
        public readonly CustomXmlTransformTextFormat NewTextFormat;

        /// <summary>
        /// 
        /// </summary>
        public readonly string DefaultValueText;

        /// <summary>
        /// 
        /// </summary>
        public string Key => $"{OldHeadPath ?? "..."}/{OldTailPath ?? "..."}<{OldNodeName}[{OldXsiTypeName}]>";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldHeadPath"></param>
        /// <param name="oldTailPath"></param>
        /// <param name="oldNodeName"></param>
        /// <param name="oldXsiType"></param>
        /// <param name="newNodeName"></param>
        /// <param name="newXsiType"></param>
        public CustomXmlTransformRule(string oldHeadPath, string oldTailPath, string oldNodeName, Type oldXsiType, string newNodeName, Type newXsiType)
        {
            Kind = CustomXmlTransformRuleKind.TagNameTypeChange;
            OldHeadPath = oldHeadPath;
            OldTailPath = oldTailPath;
            OldNodeName = oldNodeName;
            OldXsiTypeName = oldXsiType != null ? oldXsiType.Name : null;
            NewNodeName = newNodeName;
            NewXsiTypeName = (newXsiType != null) ? newXsiType.Name : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldHeadPath"></param>
        /// <param name="oldTailPath"></param>
        /// <param name="oldNodeName"></param>
        /// <param name="newTextFormat"></param>
        public CustomXmlTransformRule(string oldHeadPath, string oldTailPath, string oldNodeName, CustomXmlTransformTextFormat newTextFormat)
        {
            Kind = CustomXmlTransformRuleKind.ContentFormat;
            OldHeadPath = oldHeadPath;
            OldTailPath = oldTailPath;
            OldNodeName = oldNodeName;
            NewTextFormat = newTextFormat;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldHeadPath"></param>
        /// <param name="oldTailPath"></param>
        /// <param name="oldNodeName"></param>
        /// <param name="defaultValueText"></param>
        public CustomXmlTransformRule(string oldHeadPath, string oldTailPath, string oldNodeName, string defaultValueText)
        {
            Kind = CustomXmlTransformRuleKind.RemoveDefaultValueNode;
            OldHeadPath = oldHeadPath;
            OldTailPath = oldTailPath;
            OldNodeName = oldNodeName;
            DefaultValueText = defaultValueText;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CustomXmlTransformer : IXmlTransformer
    {
        private readonly OutputDateTimeKind _outputDateTimeKind;
        private readonly TimeSpan? _customOffset;
        private readonly ConcurrentDictionary<string, CustomXmlTransformRule> _mappings = new ConcurrentDictionary<string, CustomXmlTransformRule>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappings"></param>
        /// <param name="outputDateTimeKind"></param>
        /// <param name="customOffset"></param>
        public CustomXmlTransformer(IEnumerable<CustomXmlTransformRule> mappings, OutputDateTimeKind outputDateTimeKind, TimeSpan? customOffset)
        {
            _outputDateTimeKind = outputDateTimeKind;
            _customOffset = customOffset;
            foreach (CustomXmlTransformRule mapping in mappings)
            {
                _mappings[mapping.Key] = mapping;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappings"></param>
        public CustomXmlTransformer(IEnumerable<CustomXmlTransformRule> mappings)
            : this(mappings, OutputDateTimeKind.SameAsInput, null)
        { }

        private XmlElement ConvertXmlElement(string parentPath, XmlDocument outputDoc, XmlElement sourceElement)
        {
            string debugSource = sourceElement.InnerXml;
            // search mapping for remove empty/default node rules
            bool removeEmptyNode = false;
            string defaultValueText = null;
            foreach (CustomXmlTransformRule mapping in _mappings.Values)
            {
                if ((mapping.Kind == CustomXmlTransformRuleKind.RemoveDefaultValueNode)
                    && sourceElement.Name.Equals(mapping.OldNodeName)
                    )
                    //&& ((mapping.OldXsiTypeName == null) || (mapping.OldXsiTypeName.Equals(oldXsiTypeName))))
                {
                    // possible node - check path matches
                    if (mapping.OldHeadPath != null)
                    {
                        removeEmptyNode = mapping.OldTailPath != null ? parentPath.Equals(mapping.OldHeadPath + "/" + mapping.OldTailPath) : parentPath.StartsWith(mapping.OldHeadPath);
                    }
                    else
                    {
                        removeEmptyNode = mapping.OldTailPath == null || parentPath.EndsWith(mapping.OldTailPath);
                    }
                    if (removeEmptyNode)
                    {
                        // match!
                        defaultValueText = mapping.DefaultValueText;
                        break;
                    }
                }
            }
            if (removeEmptyNode)
            {
                if(sourceElement.InnerXml.Equals(defaultValueText, StringComparison.InvariantCultureIgnoreCase))
                    return null;
            }
            // not removed - continue
            // find old xsi type attribute
            string oldXsiTypeName = null;
            foreach (XmlAttribute sourceAttr in sourceElement.Attributes)
            {
                if (sourceAttr.Prefix == "xsi" && sourceAttr.LocalName == "type")
                {
                    oldXsiTypeName = sourceAttr.Value;
                }
            }
            // search mappings for tag name/type change rules
            string newElementName = null;
            string newXsiTypeName = null;
            bool pathMatched = false;
            foreach (CustomXmlTransformRule mapping in _mappings.Values)
            {
                if (mapping.Kind == CustomXmlTransformRuleKind.TagNameTypeChange
                    && sourceElement.Name.Equals(mapping.OldNodeName)
                    && (mapping.OldXsiTypeName == null || (mapping.OldXsiTypeName.Equals(oldXsiTypeName))))
                {
                    // possible node - check path matches
                    if (mapping.OldHeadPath != null)
                    {
                        pathMatched = mapping.OldTailPath != null ? parentPath.Equals(mapping.OldHeadPath + "/" + mapping.OldTailPath) : parentPath.StartsWith(mapping.OldHeadPath);
                    }
                    else
                    {
                        pathMatched = mapping.OldTailPath == null || parentPath.EndsWith(mapping.OldTailPath);
                    }
                    if (pathMatched)
                    {
                        // match!
                        newElementName = mapping.NewNodeName;
                        newXsiTypeName = mapping.NewXsiTypeName;
                        break;
                    }
                }
            }
            if (!pathMatched)
                newXsiTypeName = oldXsiTypeName;
            XmlElement newElement = outputDoc.CreateElement(sourceElement.Prefix, newElementName ?? sourceElement.Name, sourceElement.NamespaceURI);
            // add new attributes
            if (newXsiTypeName != null)
            {
                XmlAttribute attr = outputDoc.CreateAttribute("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance");
                attr.Value = newXsiTypeName;
                newElement.Attributes.Append(attr);
            }
            // append old attributes
            foreach (XmlAttribute sourceAttr in sourceElement.Attributes)
            {
                if (oldXsiTypeName != null && sourceAttr.Prefix == "xsi" && (sourceAttr.LocalName == "type"))
                {
                    // skip old xsi type attribute
                }
                else
                {
                    if (outputDoc.ImportNode(sourceAttr, true) is XmlAttribute attr) newElement.Attributes.Append(attr);
                }
            }
            // append old children
            foreach (XmlNode sourceNode in sourceElement.ChildNodes)
            {
                XmlNode outputNode = ConvertXmlNode(parentPath + "/" + sourceElement.Name, outputDoc, sourceNode);
                if (outputNode != null)
                    newElement.AppendChild(outputNode);
            }
            string debugOutput = newElement.InnerXml;
            if (debugOutput != debugSource)
                return newElement;
            return newElement;
        }

        private XmlNode ConvertXmlNode(string parentPath, XmlDocument outputDoc, XmlNode sourceNode)
        {
            switch (sourceNode.NodeType)
            {
            case XmlNodeType.Element:
                {
                    var sourceElement = (XmlElement)sourceNode;
                    return ConvertXmlElement(parentPath, outputDoc, sourceElement);
                }
            case XmlNodeType.Text:
                {
                    var newTextFormat = CustomXmlTransformTextFormat.Unchanged;
                    bool pathMatched = false;
                    foreach (CustomXmlTransformRule mapping in _mappings.Values)
                    {
                        // search mappings for output formatting rules
                        if ((mapping.Kind == CustomXmlTransformRuleKind.ContentFormat)
                            && sourceNode.Name.Equals(mapping.OldNodeName)
                            )
                            //&& ((mapping.OldXsiTypeName == null) || (mapping.OldXsiTypeName.Equals(oldXsiTypeName))))
                        {
                            // possible node - check path matches
                            if (mapping.OldHeadPath != null)
                            {
                                pathMatched = mapping.OldTailPath != null ? parentPath.Equals(mapping.OldHeadPath + "/" + mapping.OldTailPath) : parentPath.StartsWith(mapping.OldHeadPath);
                            }
                            else
                            {
                                pathMatched = mapping.OldTailPath == null || parentPath.EndsWith(mapping.OldTailPath);
                            }
                            if (pathMatched)
                            {
                                // match!
                                newTextFormat = mapping.NewTextFormat;
                                break;
                            }
                        }
                    }
                    if (pathMatched)
                    {
                        string newTextValue = sourceNode.Value;
                        switch (newTextFormat)
                        {
                        case CustomXmlTransformTextFormat.Unchanged:
                            break;
                        case CustomXmlTransformTextFormat.XsdTimeOnly:
                            {
                                var dtz = new DateTimeZoneParser(newTextValue, false);
                                if (!dtz.Faulted)
                                    newTextValue = dtz.ToString(_outputDateTimeKind, _customOffset, false, true, false);
                            }
                            break;
                        case CustomXmlTransformTextFormat.XsdDateTime:
                            {
                                var dtz = new DateTimeZoneParser(newTextValue, false);
                                if (!dtz.Faulted)
                                    newTextValue = dtz.ToString(_outputDateTimeKind, _customOffset, true, true, null);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                        }
                        return outputDoc.CreateTextNode(newTextValue);
                    }
                    if (DateTime.TryParse(sourceNode.Value, out var oldDateTimeValue))
                    {
                        var dtz = new DateTimeZoneParser(sourceNode.Value, false);
                        if (dtz.Faulted)
                            return outputDoc.ImportNode(sourceNode, true);
                        return outputDoc.CreateTextNode(dtz.ToString(_outputDateTimeKind, _customOffset));
                    }
                    return outputDoc.ImportNode(sourceNode, true);
                }
            default:
                return outputDoc.ImportNode(sourceNode, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputUri"></param>
        /// <param name="outputUri"></param>
        public void Transform(string inputUri, string outputUri)
        {
            var sourceDoc = new XmlDocument();
            using (XmlReader xr = XmlReader.Create(inputUri))
            {
                sourceDoc.Load(xr);
            }
            var outputDoc = new XmlDocument();
            foreach (XmlNode sourceNode in sourceDoc.ChildNodes)
            {
                XmlNode outputNode = ConvertXmlNode("$", outputDoc, sourceNode);
                if (outputNode != null)
                    outputDoc.AppendChild(outputNode);
            }
            var xws = new XmlWriterSettings {Indent = true, Encoding = Encoding.UTF8};
            using (XmlWriter xw = XmlWriter.Create(outputUri, xws))
            {
                outputDoc.Save(xw);
            }
        }
    }
}
