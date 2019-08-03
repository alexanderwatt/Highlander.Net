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
using System.Globalization;
using System.Xml;

#endregion

namespace Metadata.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class XmlDefaultAttribute
    {
        public string Prefix = "";
        public string LocalName;
        public string Value;
    }

    /// <summary>
    /// 
    /// </summary>
    public class XmlComparisonRules
    {
        private readonly HashSet<string> _pathsToIgnore = new HashSet<string>();
        private readonly HashSet<string> _optionalAttributes = new HashSet<string>();
        private readonly Dictionary<string, XmlDefaultAttribute> _defaultAttributes = new Dictionary<string, XmlDefaultAttribute>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsIgnoredPath(string path)
        {
            return _pathsToIgnore.Contains(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void AddPathToIgnore(string path)
        {
            _pathsToIgnore.Add(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="attrKey"></param>
        public void AddOptionalAttribute(string path, string attrKey)
        {
            _optionalAttributes.Add(path + attrKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="attrKey"></param>
        /// <returns></returns>
        public bool IsOptionalAttribute(string path, string attrKey)
        {
            bool found = _optionalAttributes.Contains(attrKey);
            if (!found)
                found = _optionalAttributes.Contains(path + attrKey);
            return found;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="attrKey"></param>
        /// <param name="value"></param>
        public void AddDefaultAttribute(string path, string attrKey, string value)
        {
            // note null path is ok
            _defaultAttributes.Add(path + attrKey, new XmlDefaultAttribute() { LocalName = attrKey, Value = value });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="attrKey"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        public bool TryGetDefaultAttribute(string path, string attrKey, out XmlDefaultAttribute attr)
        {
            // lookup without path 1st
            if (_defaultAttributes.TryGetValue(attrKey, out attr))
                return true;
            // else lookup with path
            return _defaultAttributes.TryGetValue(path + attrKey, out attr);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class XmlCompare
    {
        private static void CompareXmlAttribute(XmlComparisonRules rules, string path, XmlAttribute a, XmlAttribute b)
        {
            if (rules.IsIgnoredPath(path))
                return;
            if (a.Prefix != b.Prefix)
                throw new Exception($"[{path}] Attr Prefix '{a.Prefix}' != '{b.Prefix}'");
            if (a.LocalName != b.LocalName)
                throw new Exception($"[{path}] Attr LocalName '{a.LocalName}' != '{b.LocalName}'");
            if (a.Value != b.Value)
                throw new Exception($"[{path}] Attr Value '{a.Value}' != '{b.Value}'");
        }

        private static void CompareXmlAttribute(XmlComparisonRules rules, string path, XmlAttribute a, XmlDefaultAttribute b)
        {
            if (rules.IsIgnoredPath(path))
                return;
            if (a.Prefix != b.Prefix)
                throw new Exception($"[{path}] Attr Prefix '{a.Prefix}' != '{b.Prefix}'");
            if (a.LocalName != b.LocalName)
                throw new Exception($"[{path}] Attr LocalName '{a.LocalName}' != '{b.LocalName}'");
            if (a.Value != b.Value)
                throw new Exception($"[{path}] Attr Value '{a.Value}' != '{b.Value}'");
        }

        private static string AttrSortKey(XmlAttribute attr)
        {
            return attr.Name;
        }

        private static SortedDictionary<string, XmlAttribute> FilterAndSortAttrColl(string path, XmlAttributeCollection list)
        {
            // removes
            // - comment Attrs
            SortedDictionary<string, XmlAttribute> results = new SortedDictionary<string, XmlAttribute>();
            if (list != null)
            {
                int ordinal = 0;
                foreach (XmlAttribute attr in list)
                {
                    ordinal++;
                    string attrKey = AttrSortKey(attr);
                    if (attr.GetType() == typeof(XmlComment))
                        continue;
                    // allowed Attr
                    XmlAttribute oldAttr;
                    if (results.TryGetValue(attrKey, out oldAttr))
                        throw new Exception($"[{path}] Attr '{attrKey}' already exists!");
                    results.Add(attrKey, attr);
                }
            }
            return results;
        }

        private static void CompareXmlAttrColls(XmlComparisonRules rules, string path, XmlAttributeCollection a, XmlAttributeCollection b)
        {
            if (rules.IsIgnoredPath(path))
                return;
            if (a == null && b == null)
                return;
            SortedDictionary<string, XmlAttribute> listA = FilterAndSortAttrColl(path, a);
            SortedDictionary<string, XmlAttribute> listB = FilterAndSortAttrColl(path, b);
            // get all keys
            SortedSet<string> attrKeys = new SortedSet<string>();
            foreach (var key in listA.Keys) attrKeys.Add(key);
            foreach (var key in listB.Keys) attrKeys.Add(key);
            // compare attrs
            foreach (string attrKey in attrKeys)
            {
                if (!listA.TryGetValue(attrKey, out var attrA))
                    continue;
                if (listB.TryGetValue(attrKey, out var attrB))
                {
                    // both exist - compare
                    CompareXmlAttribute(rules, path + AttrSortKey(attrA) + "/", attrA, attrB);
                }
                else
                {
                    // attrB is missing - might be optional or have a default value
                    if (rules.IsOptionalAttribute(path, attrKey))
                    {
                        // allowed to be missing
                        continue;
                    }
                    if (rules.TryGetDefaultAttribute(path, attrKey, out var defaultAttribute))
                    {
                        CompareXmlAttribute(rules, path + AttrSortKey(attrA) + "/", attrA, defaultAttribute);
                    }
                    else
                    {
                        // missing and no default value - oops!
                        throw new Exception($"[{path}] Emitted Attribute '{attrKey}' missing!");
                    }
                }
            }
        }

        private static string NodeName(XmlNode node)
        {
            // returns node name without prefix
            // appends id attribute if found
            string nodeName = node.Name;
            if (!String.IsNullOrWhiteSpace(node.Prefix))
            {
                nodeName = nodeName.Substring(node.Prefix.Length + 1);
            }
            return nodeName;
        }

        private static string NodeSortKeyOld(XmlNode node, int? ordinal)
        {
            string nodeName = NodeName(node);
            return NodeSortKeyNew(nodeName, node, ordinal);
        }

        private static void CompareXmlNodeNames(string path, XmlNode a, XmlNode b)
        {
            if (NodeSortKeyOld(a, null) != NodeSortKeyOld(b, null))
                throw new Exception($"[{path}] Name '{a.Name}' != '{b.Name}'");
        }

        private static void CompareXmlNodeTypes(string path, XmlNode a, XmlNode b)
        {
            if (a.GetType() != b.GetType())
                throw new Exception($"[{path}] Type '{a.GetType().Name}' != '{b.GetType().Name}'");
        }

        private static void CompareXmlTexts(XmlComparisonRules rules, string path, XmlText a, XmlText b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (rules.IsIgnoredPath(path))
                return;

            if (a.InnerXml != b.InnerXml)
                throw new Exception("[" + path + "] a.InnerXml != b.InnerXml");
        }

        private static bool BothAreDateTimes(string valueA, string valueB)
        {
            DateTime tempA;
            DateTime tempB;
            return DateTime.TryParse(valueA, out tempA) && DateTime.TryParse(valueB, out tempB);
        }


        private static bool CompareWithDateTimeZoneParser(string valueA, string valueB, bool ignoreTimeZone)
        {
            try
            {
                DateTimeZoneParser tempA = new DateTimeZoneParser(valueA, true);
                DateTimeZoneParser tempB = new DateTimeZoneParser(valueB, true);
                if (tempA.InputAsDateTimeOffset == tempB.InputAsDateTimeOffset)
                    return true;

                if (tempA.ToString(OutputDateTimeKind.ConvertToUniversal, null) == tempB.ToString(OutputDateTimeKind.ConvertToUniversal, null))
                    return true;

                if (ignoreTimeZone)
                {
                    if ((tempA.DatePart == tempB.DatePart) && (tempA.TimePart == tempB.TimePart))
                        return true;
                }
            }
            catch
            {
            }
            return false;
        }
        private static bool CompareDateTimes(DateTimeStyles dateTimeStyle, string valueA, string valueB)
        {
            DateTime tempA;
            DateTime.TryParse(valueA, null, dateTimeStyle, out tempA);
            DateTime tempB;
            DateTime.TryParse(valueB, null, dateTimeStyle, out tempB);
            if (tempA == tempB)
                return true;
            return false;
        }

        private static void CompareXmlNodes(XmlComparisonRules rules, string pathA, XmlNode nodeA, string pathB, XmlNode nodeB)
        {
            if (rules.IsIgnoredPath(pathA))
                return;
            if (rules.IsIgnoredPath(pathB))
                return;
            if (nodeA == null && nodeB == null)
                return;
            if (nodeA == null || nodeB == null)
                throw new Exception("[" + pathA + "] (nodeA == null) || (nodeB == null)");
            if (nodeA.GetType() != nodeB.GetType())
                throw new Exception("[" + pathA + "] nodeA.GetType() != nodeB.GetType()");
            // generic node comparison
            CompareXmlNodeNames(pathA, nodeA, nodeB);
            CompareXmlNodeTypes(pathA, nodeA, nodeB);
            CompareXmlAttrColls(rules, pathA + "attr:", nodeA.Attributes, nodeB.Attributes);
            if (nodeA.HasChildNodes || nodeB.HasChildNodes)
            {
                CompareXmlNodeLists(rules, pathA, nodeA.ChildNodes, pathB, nodeB.ChildNodes);
            }
            else
            {
                // no children - just compare value
                if (String.Compare(nodeA.Value, nodeB.Value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    // values are textually different but could be numerically equal
                    if (BothAreDateTimes(nodeA.Value, nodeB.Value))
                    { 
                        if(!CompareDateTimes(DateTimeStyles.None, nodeA.Value, nodeB.Value)
                            && !CompareDateTimes(DateTimeStyles.AssumeLocal, nodeA.Value, nodeB.Value)
                            && !CompareDateTimes(DateTimeStyles.AssumeUniversal, nodeA.Value, nodeB.Value)
                            && !CompareDateTimes(DateTimeStyles.NoCurrentDateDefault, nodeA.Value, nodeB.Value)
                            && !CompareWithDateTimeZoneParser(nodeA.Value, nodeB.Value, false)
                            && !CompareWithDateTimeZoneParser(nodeA.Value, nodeB.Value, true)
                            )
                        {
                            // date/times are not equal!
                            throw new Exception($"[{pathA}] NodeValue (as DateTime) '{nodeA.Value}' != '{nodeB.Value}'");
                        }
                    }
                    else if (DateTimeOffset.TryParse(nodeA.Value, out var dtoA) && DateTimeOffset.TryParse(nodeB.Value, out var dtoB))
                    {
                        if (dtoA != dtoB)
                            throw new Exception($"[{pathA}] NodeValue (as DateTimeOffset) '{dtoA}' != '{dtoB}'");
                    }
                    else if (Int32.TryParse(nodeA.Value, out var intA) && Int32.TryParse(nodeB.Value, out var intB))
                    {
                        if (intA != intB)
                            throw new Exception($"[{pathA}] NodeValue (as Int32) '{intA}' != '{intB}'");
                    }
                    else if (Decimal.TryParse(nodeA.Value, out var numA) && Decimal.TryParse(nodeB.Value, out var numB))
                    {
                        if (Math.Round(numA, 12) != Math.Round(numB, 12))
                            throw new Exception($"[{pathA}] NodeValue (as Decimal) '{numA}' != '{numB}'");
                    }
                    else
                        throw new Exception($"[{pathA}] NodeValue '{nodeA.Value}' != '{nodeB.Value}'");
                }
            }
            // specific node type comparisons
            if (nodeA.GetType() == typeof(XmlElement))
            {
                //CompareXmlElements(ignorePaths, path + "elt:" + nodeA.Name, nodeA as XmlElement, nodeB as XmlElement);
            }
            else if (nodeA.GetType() == typeof(XmlDeclaration))
            {
                //CompareXmlDeclarations(ignorePaths, path + "decl:" + nodeA.Name, nodeA as XmlDeclaration, nodeB as XmlDeclaration);
            }
            else if (nodeA.GetType() == typeof(XmlText))
            {
                CompareXmlTexts(rules, pathA + "text:" + nodeA.Name, nodeA as XmlText, nodeB as XmlText);
            }
            else
                throw new NotImplementedException("[" + pathA + "] NodeType: " + nodeA.GetType().Name);
        }

        private static string AppendAttrValue(XmlNode node, string attrName)
        {
            XmlAttribute attr = node.Attributes?[attrName];
            return attr != null ? $"[{attrName}={attr.Value}]" : null;
        }

        private static string AppendOrdinal(int? ordinal)
        {
            return ordinal.HasValue ? $"[{ordinal.Value}]" : null;
        }

        private static string AppendNodeInnerText(XmlNode node)
        {
            string nodeValue = node.InnerText;
            return $"[value={nodeValue}]";
        }

        private static string NodeSortKeyNew(string nodeName, XmlNode node, int? ordinal)
        {
            string result = nodeName;
            // add default identities
            if (node.Attributes != null)
            {
                result += AppendAttrValue(node, "id");
                result += AppendAttrValue(node, "idref");
            }
            // add custom identity attributes
            switch (nodeName)
            {
            case "alt":
                result += AppendAttrValue(node, "domain");
                break;
            case "dealer":
                result += AppendAttrValue(node, "context");
                break;
            case "tradeId":
                result += AppendAttrValue(node, "context");
                result += AppendNodeInnerText(node);
                break;
            // ordered lists
            //case "paymentCalculationPeriod":
            //case "rateObservation":
            //case "additionalPartyPayment":
            //case "principalExchange":
            //case "businessCenter":
            //case "adminProperty":
            //case "relatedTrade":
            //case "SubGroup_CalculationInstance":
            //case "swaptionSwapStream":
            //case "exerciseDate":
            //case "SubGroup_ExerciseDate":
            //// cds
            //case "creditEventType":
            //case "offsetContract":
            //case "premiumCalculationPeriod":
            //// capfloor
            //case "optionalPaymentCalculationPeriod":
            //case "optionPremiumAmount":
            //case "SubGroup_OptionPremium":
            //    result += AppendOrdinal(ordinal);
            //    break;
            default:
                result += AppendOrdinal(ordinal);
                break;
            }
            return result;
        }

        private static SortedDictionary<string, XmlNode> FilterAndSortNodeList(string pathA, string pathB, XmlNodeList list)
        {
            // removes
            // - comment nodes
            SortedDictionary<string, XmlNode> results = new SortedDictionary<string, XmlNode>();
            if (list != null)
            {
                ConcurrentDictionary<string, int> nodeNameCount = new ConcurrentDictionary<string, int>();
                //int ordinal = 0;
                foreach (XmlNode node in list)
                {
                    string nodeName = NodeName(node);
                    int ordinal = nodeNameCount.AddOrUpdate(nodeName, 0, (key, oldValue) => oldValue + 1);
                    string nodeKey = NodeSortKeyNew(nodeName, node, ordinal);
                    if (node.GetType() == typeof(XmlComment))
                        continue;
                    // allowed node
                    if (results.TryGetValue(nodeKey, out var oldNode))
                    {
                        // ignore duplicates
                        if (oldNode.InnerXml != node.InnerXml)
                            throw new Exception($"[{pathA}] Node '{nodeKey}' already exists!");
                    }
                    else
                    {
                        results.Add(nodeKey, node);
                    }
                }
                //results.Sort((a, b) => { return SortComparer(a, b); });
            }
            return results;
        }

        private static void CompareXmlNodeLists(XmlComparisonRules rules, string pathA, XmlNodeList nodeListA, string pathB, XmlNodeList nodeListB)
        {
            if (rules.IsIgnoredPath(pathA))
                return;
            if (nodeListA == null && nodeListB == null)
                return;
            SortedDictionary<string, XmlNode> listA = FilterAndSortNodeList(pathA, pathB, nodeListA);
            SortedDictionary<string, XmlNode> listB = FilterAndSortNodeList(pathA, pathB, nodeListB);
            // get all keys
            SortedSet<string> nodeKeys = new SortedSet<string>();
            foreach (var key in listA.Keys) nodeKeys.Add(key);
            foreach (var key in listB.Keys) nodeKeys.Add(key);
            // compare nodes
            foreach (string nodeKey in nodeKeys)
            {
                if (!listA.TryGetValue(nodeKey, out var nodeA))
                    continue;
                if (!listB.TryGetValue(nodeKey, out var nodeB))
                    throw new Exception($"[{pathA}] Emitted node '{nodeKey}' missing!");
                // both exist - compare
                CompareXmlNodes(rules, pathA + NodeSortKeyOld(nodeA, null) + "/", nodeA, pathB + NodeSortKeyOld(nodeB, null) + "/", nodeB);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="docA"></param>
        /// <param name="docB"></param>
        public static void CompareXmlDocs(XmlDocument docA, XmlDocument docB)
        {
            XmlComparisonRules rules = new XmlComparisonRules();
            // optional output attributes
            //rules.AddOptionalAttribute(null, "xmlns"); // any node
            rules.AddOptionalAttribute(null, "xsi:schemaLocation");
            //rules.AddOptionalAttribute("requestAllocation/attr:", "xsi:schemaLocation");
            //rules.AddOptionalAttribute("requestAllocationRetracted/attr:", "xsi:schemaLocation");
            // add default values
            rules.AddDefaultAttribute(null, "entityIdScheme", "http://www.fpml.org/spec/2003/entity-id-RED-1-0");
            rules.AddDefaultAttribute(null, "contractualSupplementScheme", "http://www.fpml.org/coding-scheme/contractual-supplement");
            rules.AddDefaultAttribute(null, "currencyScheme", "http://www.fpml.org/ext/iso4217-2001-08-15");
            rules.AddDefaultAttribute(null, "exchangeIdScheme", "http://www.fpml.org/spec/2002/exchange-id-MIC-1-0");
            rules.AddDefaultAttribute(null, "partyIdScheme", "http://www.fpml.org/ext/iso9362");
            rules.AddDefaultAttribute(null, "facilityTypeScheme", "http://www.fpml.org/coding-scheme/facility-type");
            rules.AddDefaultAttribute(null, "dayCountFractionScheme", "http://www.fpml.org/coding-scheme/day-count-fraction");
            rules.AddDefaultAttribute(null, "routingIdCodeScheme", "http://www.fpml.org/ext/iso9362");
            rules.AddDefaultAttribute(null, "floatingRateIndexScheme", "http://www.fpml.org/coding-scheme/floating-rate-index");
            rules.AddDefaultAttribute(null, "interpolationMethodScheme", "http://www.fpml.org/coding-scheme/interpolation-method");
            rules.AddDefaultAttribute(null, "businessCenterScheme", "http://www.fpml.org/coding-scheme/business-center");
            rules.AddDefaultAttribute(null, "creditSupportAgreementTypeScheme", "http://www.fpml.org/coding-scheme/credit-support-agreement-type");
            rules.AddDefaultAttribute(null, "collateralMarginCallResponseReasonScheme", "http://www.fpml.org/coding-scheme/collateral-margin-call-response-reason");
            rules.AddDefaultAttribute(null, "categoryScheme", "http://www.fpml.org/coding-scheme/org-type-category");
            // nodes (trees) to ignore
            rules.AddPathToIgnore("xml/"); // XML declaration
            //rules.AddPathToIgnore("GWML/Signature/");
            //rules.AddPathToIgnore("GWML/trade/dateAdjustmentMethods/");
            //rules.AddPathToIgnore("GWML/trade/tradeHeader/partyTradeIdentifier/tradeId");
            CompareXmlAttrColls(rules, "attr:", docA.Attributes, docB.Attributes);
            CompareXmlNodeLists(rules, "", docA.ChildNodes, "", docB.ChildNodes);
        }
    }
}
