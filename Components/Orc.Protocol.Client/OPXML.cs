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
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Highlander.Orc.Messages;

namespace Highlander.Orc.Client
{
    public class OPXML
    {
        #region Declarations

        private MessageTypes _messageType;

        public const string XMLRootNodeName = "orc_message";

        public const string XMLInfoNodeName = "message_info";

        public const string XMLTypeNodeName = "message_type";

        public const string XMLReplyNodeName = "reply_to";

        #endregion

        public static string RootNodeXml { get; } = string.Concat("<", XMLRootNodeName, "></", XMLRootNodeName,
            ">");

        //public Data.MessageTypes Type
        //{
        //    get { return _messageType; }
        //    set
        //    {
        //        if (_OPXMLDoc == null)
        //        {
        //            _messageType = value;
        //            BuildBaseXML();
        //        }
        //        else
        //            throw new InvalidOperationException("Type can only be set for a new, un-typed Orc XML instance.");
        //    }
        //}

        public XmlDocument Document { get; private set; }

        public XPathNavigator Navigator
        {
            get
            {
                XPathDocument xpathDoc = new XPathDocument(new XmlNodeReader(Document));
                return xpathDoc.CreateNavigator();
            }
        }

        //Orc XML Message Structure - client to server.
        //
        //<orc_message>								<-- Orc XML Wrapper (root node)
        //	<message_info>							\
        //		<message_type>login</message_type>	 >- Orc XML Message Header (can have additional nodes).
        //		<...>								|
        //	</message_info>							/
        //	<...>									<-- Place holder for the Orc XML Message Content.
        //</orc_message>

        //Orc XML Message Structure - server replies.
        //
        //<orc_message>								<-- Orc XML Wrapper (root node)
        //	<reply_to>								\
        //		<message_type>login</message_type>	 >- Orc XML Message Header (can have additional nodes).
        //		<...>								|
        //	</reply_to>								/
        //	<...>									<-- Place holder for the Orc XML Message Content.
        //	<error>...</error>
        //	<error_description>...</error_description>
        //</orc_message>

        //Object XML Serialisation Structure.
        //
        //<?xml version="1.0" encoding="utf-8"?> 
        //<ClassName>			<-- Object class name, relates to Orc Message Type, but is not exactly the same in all cases.
        //	<...>				<-- Actual object XML data, is the same as the Orc Message Content
        //</ClassName>

        #region Constructors and initialise

        public OPXML()
        {
        }

        //public OPXML(Data.MessageTypes MessageType)
        //{
        //    _messageType = MessageType;
        //    BuildBaseXML();
        //}

        private void BuildBaseXML()
        {
            Document = new XmlDocument();
            Document.LoadXml(RootNodeXml);
            //message_info
            AddNode(XMLInfoNodeName, null, XMLRootNodeName);
            //message_type = xxx
            AddNode(XMLTypeNodeName, _messageType.ToString(), "//" + XMLInfoNodeName);
        }

        #endregion

        /// <summary>
        /// Explicitly add a node into the Orc XML.
        /// This will append the node as a child of the specified parent node.
        /// </summary>
        /// <param name="name">Name of node to add</param>
        /// <param name="value">Inner text value of the node (may be null)</param>
        /// <param name="parentXPath">XPath to the parent of the node being add</param>
        public void AddNode(string name, string value, string parentXPath)
        {
            XmlNode xNode = Document.CreateNode(XmlNodeType.Element, name, null);
            if (value != null)
                xNode.InnerText = value;
            Document.SelectSingleNode(parentXPath)?.AppendChild(xNode);
        }

        #region Receive Message Methods

        public void AddReceiveMessage(XPathNavigator businessData)
        {
            //Get Orc Message Header XML (base xml created when first instantiated).
            string sHeaderXML = Document.FirstChild.InnerXml;

            //First child - to select the xml declaration,
            //Next sibling - to get the class name xml node,
            //Inner xml - to get the xml data contained within the class name node.
            businessData.MoveToFirstChild();
            businessData.MoveToNext();
            string sContentXML = businessData.InnerXml;
            //Rebuild XML
            StringBuilder sb = new StringBuilder();
            sb.Append('<');
            sb.Append(XMLRootNodeName);
            sb.Append('>');
            sb.Append(sHeaderXML);
            sb.Append(sContentXML);
            sb.Append("</");
            sb.Append(XMLRootNodeName);
            sb.Append('>');
            Document.LoadXml(sb.ToString());
        }

        //public void AddReceiveMessage(Data.MessageBase BusinessData)
        //{
        //    //Build the Orc Message Content XML.
        //    XPathNavigator xdContent = Serialise(BusinessData, BusinessData.GetType());

        //    //Call build by xml method.
        //    AddReceiveMessage(xdContent);
        //}

        /// <summary>
        /// Deserialise given xml in to a business object.
        /// </summary>
        /// <param name="xml">Orc XML Message</param>
        /// <param name="dataObjectType"></param>
        /// <param name="addDictionaryElement"></param>
        /// <returns>Business object holding the xml data</returns>
        public static object DeserialiseReceiveXML(XPathNavigator xml, Type dataObjectType, bool addDictionaryElement)
        {
            //Get message content from xml passed in.
            string sMessageContent = GetData(xml, dataObjectType, addDictionaryElement);
            XmlSerializer xSerial = new XmlSerializer(dataObjectType);
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(sMessageContent.ToCharArray()));
            return xSerial.Deserialize(stream);
        }

        public static object DeserialiseReceiveXML(XPathNavigator xml, Type dataObjectType)
        {
            //Get message content from xml passed in.
            string sMessageContent = xml.OuterXml;
            XmlSerializer xSerial = new XmlSerializer(dataObjectType);
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(sMessageContent.ToCharArray()));
            return xSerial.Deserialize(stream);
        }

        #endregion

        #region Send Message Methods

        public void AddSendMessage(XPathNavigator businessData)
        {
            //Get Orc Message Header XML (base xml created when first instantiated).
            string sHeaderXML = Document.FirstChild.InnerXml;
            //First child - to select the xml declaration,
            //Inner xml - to get the xml data contained within the class name node.
            //BusinessData.MoveToFirstChild();
            string sContentXML = businessData.InnerXml;
            //Rebuild XML
            StringBuilder sb = new StringBuilder();
            sb.Append('<');
            sb.Append(XMLRootNodeName);
            sb.Append('>');
            sb.Append(sHeaderXML);
            sb.Append(sContentXML);
            sb.Append("</");
            sb.Append(XMLRootNodeName);
            sb.Append('>');
            Document.LoadXml(sb.ToString());
        }

        //public void AddSendMessage(Data.MessageBase BusinessData)
        //{
        //    //Build the Orc Message Content XML.
        //    XPathNavigator xdContent = Serialise(BusinessData, BusinessData.GetType());

        //    //Call build by xml method.
        //    AddSendMessage(xdContent);
        //}

        /// <summary>
        /// Deserialise given xml in to a business object.
        /// </summary>
        /// <param name="xml">Orc XML Message</param>      
        /// <param name="dataObjectType"></param>
        /// <param name="addDictionaryElement"></param>
        /// <returns>Business object holding the xml data</returns>
        public static object DeserialiseSendXML(XPathNavigator xml, Type dataObjectType, bool addDictionaryElement)
        {
            //Get message content from xml passed in.
            string sMessageContent = GetData(xml, dataObjectType, addDictionaryElement);
            XmlSerializer xSerial = new XmlSerializer(dataObjectType);
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(sMessageContent.ToCharArray()));
            return xSerial.Deserialize(stream);
        }

        #endregion

        private static string GetData(XPathNavigator xml, Type dataObjectType, bool addDictionaryElement)
        {
            string sContent;
            //xml.MoveToFirstChild();
            //Dictionary object already contain their parent node.
            if (addDictionaryElement)
            {
                string sName = XmlHelper.GetRootElementName(dataObjectType);
                sContent = "<" + sName + ">" + xml.InnerXml + "</" + sName + ">";
            }
            else
                sContent = xml.OuterXml;
            return sContent;
        }
    }
}
