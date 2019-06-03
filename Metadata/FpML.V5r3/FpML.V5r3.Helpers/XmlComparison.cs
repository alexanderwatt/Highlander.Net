using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;

namespace FpML.V5r3.Helpers
{
    public class XmlDefaultAttribute
    {
        public string Prefix = "";
        public string LocalName;
        public string Value;
    }
    public class XmlComparisonRules
    {
        private readonly HashSet<string> _PathsToIgnore = new HashSet<string>();
        private readonly HashSet<string> _OptionalAttributes = new HashSet<string>();
        private readonly Dictionary<string, XmlDefaultAttribute> _DefaultAttributes = new Dictionary<string, XmlDefaultAttribute>();
        public bool IsIgnoredPath(string path)
        {
            return _PathsToIgnore.Contains(path);
        }
        public void AddPathToIgnore(string path)
        {
            _PathsToIgnore.Add(path);
        }
        public void AddOptionalAttribute(string path, string attrKey)
        {
            _OptionalAttributes.Add(path + attrKey);
        }
        public bool IsOptionalAttribute(string path, string attrKey)
        {
            bool found = _OptionalAttributes.Contains(attrKey);
            if (!found)
                found = _OptionalAttributes.Contains(path + attrKey);
            return found;
        }
        public void AddDefaultAttribute(string path, string attrKey, string value)
        {
            // note null path is ok
            _DefaultAttributes.Add(path + attrKey, new XmlDefaultAttribute() { LocalName = attrKey, Value = value });
        }
        public bool TryGetDefaultAttribute(string path, string attrKey, out XmlDefaultAttribute attr)
        {
            // lookup without path 1st
            if (_DefaultAttributes.TryGetValue(attrKey, out attr))
                return true;
            // else lookup with path
            return _DefaultAttributes.TryGetValue(path + attrKey, out attr);
        }
    }

    public static class XmlCompare
    {
        private static void CompareXmlAttribute(XmlComparisonRules rules, string path, XmlAttribute a, XmlAttribute b)
        {
            if (rules.IsIgnoredPath(path))
                return;

            if (a.Prefix != b.Prefix)
                throw new Exception(String.Format("[{0}] Attr Prefix '{1}' != '{2}'", path, a.Prefix, b.Prefix));

            if (a.LocalName != b.LocalName)
                throw new Exception(String.Format("[{0}] Attr LocalName '{1}' != '{2}'", path, a.LocalName, b.LocalName));

            if (a.Value != b.Value)
                throw new Exception(String.Format("[{0}] Attr Value '{1}' != '{2}'", path, a.Value, b.Value));
        }

        private static void CompareXmlAttribute(XmlComparisonRules rules, string path, XmlAttribute a, XmlDefaultAttribute b)
        {
            if (rules.IsIgnoredPath(path))
                return;

            if (a.Prefix != b.Prefix)
                throw new Exception(String.Format("[{0}] Attr Prefix '{1}' != '{2}'", path, a.Prefix, b.Prefix));

            if (a.LocalName != b.LocalName)
                throw new Exception(String.Format("[{0}] Attr LocalName '{1}' != '{2}'", path, a.LocalName, b.LocalName));

            if (a.Value != b.Value)
                throw new Exception(String.Format("[{0}] Attr Value '{1}' != '{2}'", path, a.Value, b.Value));
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
                foreach (XmlAttribute Attr in list)
                {
                    ordinal++;
                    string attrKey = AttrSortKey(Attr);
                    if (Attr.GetType() == typeof(XmlComment))
                        continue;
                    // allowed Attr
                    XmlAttribute oldAttr;
                    if (results.TryGetValue(attrKey, out oldAttr))
                        throw new Exception(String.Format("[{0}] Attr '{1}' already exists!", path, attrKey));
                    results.Add(attrKey, Attr);
                }
            }
            return results;
        }
        private static void CompareXmlAttrColls(XmlComparisonRules rules, string path, XmlAttributeCollection a, XmlAttributeCollection b)
        {
            if (rules.IsIgnoredPath(path))
                return;

            if ((a == null) && (b == null))
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
                XmlAttribute attrA;
                if (!listA.TryGetValue(attrKey, out attrA))
                    continue;

                XmlAttribute attrB;
                if (listB.TryGetValue(attrKey, out attrB))
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
                    else
                    {
                        XmlDefaultAttribute defaultAttribute;
                        if (rules.TryGetDefaultAttribute(path, attrKey, out defaultAttribute))
                        {
                            CompareXmlAttribute(rules, path + AttrSortKey(attrA) + "/", attrA, defaultAttribute);
                        }
                        else
                        {
                            // missing and no default value - oops!
                            throw new Exception(String.Format("[{0}] Emitted Attribute '{1}' missing!", path, attrKey));
                        }
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
                throw new Exception(String.Format("[{0}] Name '{1}' != '{2}'", path, a.Name, b.Name));
        }

        private static void CompareXmlNodeTypes(string path, XmlNode a, XmlNode b)
        {
            if (a.GetType() != b.GetType())
                throw new Exception(String.Format("[{0}] Type '{1}' != '{2}'", path, a.GetType().Name, b.GetType().Name));
        }

        private static void CompareXmlTexts(XmlComparisonRules rules, string path, XmlText a, XmlText b)
        {
            if (rules.IsIgnoredPath(path))
                return;

            if (a.InnerXml != b.InnerXml)
                throw new Exception("[" + path + "] a.InnerXml != b.InnerXml");
        }

        private static bool BothAreDateTimes(string valueA, string valueB)
        {
            DateTime tempA;
            DateTime tempB;
            if (DateTime.TryParse(valueA, out tempA) && DateTime.TryParse(valueB, out tempB))
                return true;
            else
                return false;
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
            else
                return false;
        }

        private static void CompareXmlNodes(XmlComparisonRules rules, string pathA, XmlNode nodeA, string pathB, XmlNode nodeB)
        {
            if (rules.IsIgnoredPath(pathA))
                return;

            if (rules.IsIgnoredPath(pathB))
                return;

            if ((nodeA == null) && (nodeB == null))
                return;
            if ((nodeA == null) || (nodeB == null))
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
                if (String.Compare(nodeA.Value, nodeB.Value, true) != 0)
                {
                    // values are textually different but could be numerically equal
                    DateTimeOffset dtoA;
                    DateTimeOffset dtoB;
                    Int32 intA;
                    Int32 intB;
                    Decimal numA;
                    Decimal numB;
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
                            throw new Exception(String.Format("[{0}] NodeValue (as DateTime) '{1}' != '{2}'", pathA, nodeA.Value, nodeB.Value));
                        }
                    }
                    else if (DateTimeOffset.TryParse(nodeA.Value, out dtoA) && DateTimeOffset.TryParse(nodeB.Value, out dtoB))
                    {
                        if (dtoA != dtoB)
                            throw new Exception(String.Format("[{0}] NodeValue (as DateTimeOffset) '{1}' != '{2}'", pathA, dtoA, dtoB));
                    }
                    else if (Int32.TryParse(nodeA.Value, out intA) && Int32.TryParse(nodeB.Value, out intB))
                    {
                        if (intA != intB)
                            throw new Exception(String.Format("[{0}] NodeValue (as Int32) '{1}' != '{2}'", pathA, intA, intB));
                    }
                    else if (Decimal.TryParse(nodeA.Value, out numA) && Decimal.TryParse(nodeB.Value, out numB))
                    {
                        if (Math.Round(numA, 12) != Math.Round(numB, 12))
                            throw new Exception(String.Format("[{0}] NodeValue (as Decimal) '{1}' != '{2}'", pathA, numA, numB));
                    }
                    else
                        throw new Exception(String.Format("[{0}] NodeValue '{1}' != '{2}'", pathA, nodeA.Value, nodeB.Value));
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
            XmlAttribute attr = node.Attributes[attrName];
            if (attr != null)
                return String.Format("[{0}={1}]", attrName, attr.Value);
            else
                return null;
        }

        private static string AppendOrdinal(int? ordinal)
        {
            if (ordinal.HasValue)
                return String.Format("[{0}]", ordinal.Value);
            else
                return null;
        }

        private static string AppendNodeInnerText(XmlNode node)
        {
            string nodeValue = node.InnerText;
            if (nodeValue != null)
                return String.Format("[value={0}]", nodeValue);
            else
                return null;
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
                    int ordinal = nodeNameCount.AddOrUpdate(nodeName, 0, (key, oldValue) => { return oldValue + 1; });
                    string nodeKey = NodeSortKeyNew(nodeName, node, ordinal);
                    if (node.GetType() == typeof(XmlComment))
                        continue;
                    // allowed node
                    XmlNode oldNode;
                    if (results.TryGetValue(nodeKey, out oldNode))
                    {
                        // ignore duplicates
                        if (oldNode.InnerXml != node.InnerXml)
                            throw new Exception(String.Format("[{0}] Node '{1}' already exists!", pathA, nodeKey));
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

            if ((nodeListA == null) && (nodeListB == null))
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
                XmlNode nodeA;
                if (!listA.TryGetValue(nodeKey, out nodeA))
                    continue;

                XmlNode nodeB;
                if (!listB.TryGetValue(nodeKey, out nodeB))
                    throw new Exception(String.Format("[{0}] Emitted node '{1}' missing!", pathA, nodeKey));

                // both exist - compare
                CompareXmlNodes(rules, pathA + NodeSortKeyOld(nodeA, null) + "/", nodeA, pathB + NodeSortKeyOld(nodeB, null) + "/", nodeB);
            }
        }

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
