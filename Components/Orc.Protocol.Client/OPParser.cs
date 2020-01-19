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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Highlander.Orc.Messages;

namespace Highlander.Orc.Client
{

    public class OPParser
    {
        #region Declarations

        private MessageBase _dataObject;
        private int _lengthOp;
        private StringBuilder _sb;
        private string _stringOp;
        private XmlDocument _xDoc = new XmlDocument();

        public bool DebugMessages { get; set; }

        private enum TokenType
        {
            Assign = 0, // expect assignment operator
            Value = 1, // expect a value or a sub dictionary
            Separator = 2, // expect a separator "|" or termination "}"
            Name = 3 // name or empty dictionary '}'
        }

        #endregion

        /// <summary>
        /// Convert an OP message into an Orc XML Document.
        /// </summary>
        /// <param name="op">OP Message</param>
        /// <returns>Orc XML Document</returns>
        public XPathNavigator OPtoXML(string op)
        {
            //Capture OP message and details
            _lengthOp = op.Length;
            _stringOp = op;
            //Create new XML document to clear any previous transforms.
            _xDoc = new XmlDocument();
            if (_lengthOp > 0)
            {
                //Very first token of the OP is always "{" since this represents the root element of the message
                int iPos = 0;
                if (_stringOp[iPos] == '{')
                    iPos++;
                else
                    throw new Exception("Can not find initial message element");
                //Parse the OP message.
                _xDoc.LoadXml(OPXML.RootNodeXml);
                XmlNode xNode = _xDoc.LastChild;
                if (!ParseOP(ref iPos, ref xNode))
                    throw new Exception("Can not convert OP to XML");
            }
            XPathDocument xpathDoc = new XPathDocument(new XmlNodeReader(_xDoc));
            return xpathDoc.CreateNavigator();
        }

        /// <summary>
        /// Convert an Orc XML Document into an OP message.
        /// </summary>
        /// <param name="xmlIn">Orc XML Document</param>
        /// <returns>OP Message</returns>
        public string XMLtoOP(XPathNavigator xmlIn)
        {
            //Capture XML Message.
            _xDoc.LoadXml(xmlIn.InnerXml);
            //create new string builder to clear previous transforms.
            _sb = new StringBuilder();
            if (_xDoc.HasChildNodes)
            {
                if (DebugMessages)
                {
                    if (_xDoc.FirstChild.HasChildNodes)
                    {
                        XmlNode debugNode = _xDoc.CreateNode(XmlNodeType.Element, "debug", "");
                        debugNode.InnerText = "true";
                        _xDoc.FirstChild.InsertBefore(debugNode, _xDoc.FirstChild.FirstChild);
                    }
                }
                //Firts node is always message wrapper.
                XmlNodeList xNode = _xDoc.SelectSingleNode(OPXML.XMLRootNodeName)?.ChildNodes;
                //Parse the XML message.
                int counter = 0;
                if (!ParseXML(ref xNode, ref counter))
                    throw new Exception("Can not convert XML to OP");
                //if (DebugMessages)
                //{
                //    Logger.LogMessage("OrcProtocolParser - Output", _sb.ToString());
                //}
            }
            return _sb.ToString();
        }

        ///// <summary>
        ///// Convert an Orc XML Document into an OP message.
        ///// </summary>
        ///// <param name="Data">Orc Data Object</param>
        ///// <returns>OP Message</returns>
        public string XMLtoOP(MessageBase data)
        {
            //Capture Xml.
            _dataObject = data;
            string sXML = XmlHelper.Serialize(data, data.GetType(), Encoding.Unicode);
            _xDoc.LoadXml(sXML);
            //Create new string builder to clear previous transforms.
            _sb = new StringBuilder();
            if (_xDoc.HasChildNodes)
            {
                if (DebugMessages)
                {
                    if (_xDoc.DocumentElement != null)
                    {
                        XmlNode debugNode = _xDoc.CreateNode(XmlNodeType.Element, "debug", "");
                        debugNode.InnerText = "true";
                        _xDoc.DocumentElement?.InsertBefore(debugNode, _xDoc.DocumentElement.FirstChild);
                    }
                }
                //Firts node is always message wrapper.
                XmlNodeList xNode = _xDoc.SelectSingleNode("/orc_message")?.ChildNodes;
                //Parse the XML message.
                int counter = 0;
                if (!ParseXML(ref xNode, ref counter, false))
                    throw new Exception("Can not convert XML to OP");
                //if (DebugMessages)
                //{
                //    Logger.LogMessage("OrcProtocolParser - Output", _sb.ToString());
                //}
            }
            //return the OP message.
            return _sb.ToString();
        }

        // this is the recursive descent parsing function. This function is entered
        // when the opening "{" has already been read and consumed. It exits when
        // the closing "}" is encountered and consumed by this function.
        // "{" starts a new lower level dictionary, or XML node
        // "=" ends the name of the name/value pair
        // "|" starts a new sibling level dictionary, or XML node
        // "}" 
        private bool ParseOP(ref int iPos, ref XmlNode xmlObj)
        {
            string sTok = "";
            //this loop allows all siblings to be read at the same recursion level
            //this loop is ended with a break, or a return statement.
            while (true)
            {
                if (!GetNextToken(ref iPos, TokenType.Name, ref sTok))
                    return false;
                if (IsValidDictName(sTok))
                {
                    //Remove the sequence number if found on the end.
                    sTok = RemoveSequenceNumber(sTok);
                    XmlNode xmlTemp = _xDoc.CreateElement(sTok);
                    // now we expect the next token to be "="
                    if (!GetNextToken(ref iPos, TokenType.Assign, ref sTok))
                        return false;
                    // now either expect "{", or a value string terminated by "|" or "}"
                    if (GetNextToken(ref iPos, TokenType.Separator, ref sTok))
                    {
                        // child object encountered, call self
                        xmlObj.AppendChild(xmlTemp);
                        if (!ParseOP(ref iPos, ref xmlTemp))
                            return false;
                    }
                    else
                    {
                        //Build Value terminated by "|" or "}".
                        if (GetNextToken(ref iPos, TokenType.Value, ref sTok))
                        {
                            XmlNode xmlValue = _xDoc.CreateTextNode(sTok);
                            xmlTemp.AppendChild(xmlValue);
                        }
                        else
                            return false;
                    }
                    // this node is now completed;
                    xmlObj.AppendChild(xmlTemp);
                    // check the next token to decide what to do. Can only be "|" or "}"
                    if (!GetNextToken(ref iPos, TokenType.Separator, ref sTok))
                        return false;
                    // unless we have siblings, we have completed this level
                    if (sTok != "|")
                        break;
                }
            }
            // current object closed, so we must see a closing "}"
            if (sTok != "}")
                return false;
            return true;
        }

        // this is the recursive descent object walker that creates the OP from XML
        private bool ParseXML(ref XmlNodeList xmlObj, ref int counter)
        {
            XmlNode thisNode = xmlObj[0];
            string thisNodeName = thisNode.Name;
            //Check if we are at the value (text) node of the element.
            if (thisNodeName == "#text")
            {
                //Add the node as a value, '=' delimited.
                _sb.Append("=");
                _sb.Append(thisNode.Value);
            }
            else
            {
                //Entered a new sub-level, (The first sublevel has no '=').
                _sb.Append(_sb.Length == 0 ? "{" : "={");
                //Check if this node needs a sequence number.				
                if (NeedsSequenceNumber(thisNode))
                {
                    counter++;
                    thisNodeName = String.Concat(thisNodeName.Substring(0, thisNodeName.Length), counter.ToString());
                }
                _sb.Append(thisNodeName);
                //Add child nodes to output
                if (thisNode.HasChildNodes)
                {
                    //recursive call for child nodes
                    XmlNodeList xList = thisNode.ChildNodes;
                    int childCounter = 0;
                    if (!ParseXML(ref xList, ref childCounter))
                        return false;
                }
                //Add node siblings to output
                thisNode = thisNode.NextSibling;
                while (thisNode != null)
                {
                    thisNodeName = thisNode.Name;
                    //Add child nodes of this sibling to output
                    if (thisNode.HasChildNodes)
                    {
                        //Add the node as a sibling, '|' delimited.
                        _sb.Append("|");
                        //Check if this node needs a sequence number.                        
                        if (NeedsSequenceNumber(thisNode))
                        {
                            counter++;
                            thisNodeName =
                                string.Concat(thisNodeName.Substring(0, thisNodeName.Length), counter.ToString());
                        }
                        _sb.Append(thisNodeName);
                        //recursive call for child nodes
                        int newChildCounter = 0;
                        XmlNodeList xList = thisNode.ChildNodes;
                        if (!ParseXML(ref xList, ref newChildCounter))
                            return false;
                    }
                    //get next sibling for parseing
                    thisNode = thisNode.NextSibling;
                }
                //Finished this level of siblings and children so...
                //Close the current node list, NOTE: this also closes the end of the message.
                _sb.Append("}");
            }
            return true;
        }

        // this is the recursive descent object walker that creates the OP from XML
        private bool ParseXML(ref XmlNodeList xmlObj, ref int counter, bool sequenceMe)
        {
            XmlNode thisNode = xmlObj[0];
            string thisNodeName = thisNode.Name;
            //Check if we are at the value (text) node of the element.
            if (thisNodeName == "#text")
            {
                //Add the node as a value, '=' delimited.
                _sb.Append("=");
                _sb.Append(thisNode.Value);
            }
            else
            {
                //Entered a new sub-level, (NOTE: the first sublevel has no '=').
                _sb.Append(_sb.Length == 0 ? "{" : "={");
                //Check if this node needs a sequence number.
                bool sequenceChildren = NeedsSequenceNumber(thisNode);
                if (sequenceMe)
                {
                    counter++;
                    thisNodeName = string.Concat(thisNodeName.Substring(0, thisNodeName.Length), counter.ToString());
                }
                _sb.Append(thisNodeName);
                //Add child nodes to output
                if (thisNode.HasChildNodes)
                {
                    //recursive call for child nodes
                    XmlNodeList xList = thisNode.ChildNodes;
                    int childCounter = 0;
                    if (!ParseXML(ref xList, ref childCounter, sequenceChildren))
                        return false;
                }
                //Add node siblings to output
                thisNode = thisNode.NextSibling;
                while (thisNode != null)
                {
                    thisNodeName = thisNode.Name;
                    //Add child nodes of this sibling to output
                    if (thisNode.HasChildNodes)
                    {
                        //Add the node as a sibling, '|' delimited.
                        _sb.Append("|");

                        //Check if this node needs a sequence number.                        
                        sequenceChildren = NeedsSequenceNumber(thisNode);
                        if (sequenceMe)
                        {
                            counter++;
                            thisNodeName =
                                string.Concat(thisNodeName.Substring(0, thisNodeName.Length), counter.ToString());
                        }
                        _sb.Append(thisNodeName);
                        //recursive call for child nodes
                        int newChildCounter = 0;
                        XmlNodeList xList = thisNode.ChildNodes;
                        if (!ParseXML(ref xList, ref newChildCounter, sequenceChildren))
                            return false;
                    }

                    //get next sibling for parseing
                    thisNode = thisNode.NextSibling;
                }

                //Finished this level of siblings and children so...
                //Close the current node list, NOTE: this also closes the end of the message.
                _sb.Append("}");
            }

            return true;
        }

        //This function recognises if an XML node name needs to have a sequence number suffixed to be complient to the Orc Protocol.
        //And returns the name to be used either with or without the number as required.
        //The sequence position needs to be stored independently from the current node being processed due to the presences of child nodes.
        private bool NeedsSequenceNumber(XmlNode thisNode)
        {
            string thisNodeName = thisNode.Name;
            bool bNeedsSequencing = false;

            //Check for enumerable types.
            if (_dataObject != null)
            {
                bool sequence = false;
                Type type = GetPropertyTypeForElement(_dataObject.GetType(), thisNodeName, ref sequence);
                if (type != null)
                    if (sequence)
                        bNeedsSequencing = true;
            }
            else
            {
                //Check if the next node is one of a sequence.
                XmlNode nodeTest = thisNode.NextSibling;
                if (nodeTest != null)
                {
                    if (nodeTest.Name == thisNodeName)
                        bNeedsSequencing = true;
                }

                if (!bNeedsSequencing)
                {
                    //Check if the previous node is one of a sequence.
                    nodeTest = thisNode.PreviousSibling;
                    if (nodeTest != null)
                    {
                        if (nodeTest.Name == thisNodeName)
                            bNeedsSequencing = true;
                    }
                }
            }
            return bNeedsSequencing;
        }

        public static Type GetPropertyTypeForElement(Type thisType, string elementName, ref bool sequence)
        {
            Type returnType = null;
            PropertyInfo[] propInfos = thisType.GetProperties();

            for (int i = 0; i < propInfos.Length; i++)
            {
                PropertyInfo propI = (PropertyInfo)propInfos.GetValue(i);
                object[] attributes = propI.GetCustomAttributes(true);
                //Look at all the attributes of that property.
                foreach (var attribute in attributes)
                {
                    Type propType = attribute.GetType();
                    if (propType == typeof(XmlElementAttribute))
                    {
                        if (elementName == ((XmlElementAttribute)attribute).ElementName)
                        {
                            returnType = propI.PropertyType;
                            break;
                        }
                    }
                    else if (propType == typeof(XmlArrayAttribute))
                    {
                        if (elementName == ((XmlArrayAttribute)attribute).ElementName)
                        {
                            returnType = propI.PropertyType;
                            sequence = true;
                            break;
                        }
                    }
                    else if (propType == typeof(XmlArrayItemAttribute))
                    {
                        if (elementName == ((XmlArrayItemAttribute)attribute).ElementName)
                        {
                            returnType = propI.PropertyType;
                            break;
                        }
                    }
                }
                //Still not found, try the properties of the property.
                if (returnType == null)
                {
                    //Does the property have properties?
                    Type propType = propI.PropertyType;
                    returnType = GetPropertyTypeForElement(propType, elementName, ref sequence);
                }
                else
                    break;
            }
            return returnType;
        }

        // depending on the state, the token may be terminated by a SP/TAB/CR/LF character
        // or by a separator "|" or "{" or "}". If values are read, escaped characters
        // must be restored.
        // s:       input string, untruncated
        // k:       current parsing read position
        // state:   parsing state, determining expected token and termination
        // sTok:    if successful, the token to be returned
        private bool GetNextToken(ref int iPos, TokenType tokenType, ref string sTok)
        {
            int i = iPos;
            sTok = "";
            // skip leading spaces
            while (i < _lengthOp)
            {
                //Advance current pos to skip white space chars.
                if (_stringOp[i] == ' ' || _stringOp[i] == '\t' || _stringOp[i] == '\r' || _stringOp[i] == '\n' ||
                    _stringOp[i] == '\0')
                    iPos++;
                else
                    break;

                i++;
            }
            if (iPos >= _lengthOp) // nothing left to read
                return false;
            // check the first no space char, and act accordingly, depending on state
            if (tokenType == TokenType.Name)
            {
                // expect either a name or closing '}' (i.e. empty)
                if (_stringOp[i] == '}')
                {
                    // empty object
                    sTok = _stringOp.Substring(i, 1);
                    iPos = i + 1;
                    return true;
                }
                // now read the name, terminated by '=' but trim to clear leading or training white space
                StringBuilder sb = new StringBuilder();
                while (_stringOp[i] != '=')
                {
                    sb.Append(_stringOp[i]);
                    i++;
                    iPos = i;
                }
                //Remove unnecessary white space
                sTok = ClearWhiteSpace(sb.ToString());
                return true;
            }
            if (tokenType == TokenType.Assign)
            {
                // expect assignment
                if (_stringOp[i] == '=')
                {
                    sTok = _stringOp.Substring(i, 1);
                    iPos = i + 1;
                    return true;
                }
            }
            else if (tokenType == TokenType.Separator)
            {
                //expect child or sibling indicator, or and end of children indicator.
                if (_stringOp[i] == '{' || _stringOp[i] == '|' || _stringOp[i] == '}')
                {
                    sTok = _stringOp.Substring(i, 1);
                    iPos = i + 1;
                    return true;
                }
            }
            else if (tokenType == TokenType.Value && i != 0)
            {
                //loop to collect whole value at once.
                while (i < _lengthOp)
                {
                    if (_stringOp[i] == '|' || _stringOp[i] == '}')
                        //found end of value indicator.
                        break;
                    // was it escaped? if not, we have reached the end 
                    if (_stringOp[i - 1] != '\\')
                    {
                    }
                    i++;
                }

                // clean-up the string by unescaping and 
                // also eliminating trailing spaces
                sTok = _stringOp.Substring(iPos, i - iPos);
                sTok = ClearWhiteSpace(sTok);

                //Return the new pos, being an end of value delimitor.
                iPos = i;
                return true;
            }
            //Did not find expected token.
            return false;
        }

        private static string ClearWhiteSpace(string str)
        {
            str = str.Replace("\r", string.Empty);
            str = str.Replace("\n", string.Empty);
            str = str.Replace("\t", string.Empty);
            str = str.Replace("\0", string.Empty);
            str = str.Trim();

            return str;
        }

        // tests whether a name is a valid OP dictionary key name
        private static bool IsValidDictName(string sName)
        {
            //valid key names start with a letter, followed by letter, '_' or digits
            const string pattern = @"[a-zA-Z][a-zA-Z0-9_]*";
            Regex regex = new Regex(pattern);
            bool blnMatch = regex.IsMatch(sName);
            return blnMatch;
        }

        //Simply strip the number from the dictionary name.
        private static string RemoveSequenceNumber(string str)
        {
            str = str.Replace("0", string.Empty);
            str = str.Replace("1", string.Empty);
            str = str.Replace("2", string.Empty);
            str = str.Replace("3", string.Empty);
            str = str.Replace("4", string.Empty);
            str = str.Replace("5", string.Empty);
            str = str.Replace("6", string.Empty);
            str = str.Replace("7", string.Empty);
            str = str.Replace("8", string.Empty);
            str = str.Replace("9", string.Empty);
            return str;
        }
    }
}
