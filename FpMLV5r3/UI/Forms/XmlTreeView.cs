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

#region Usings

using System;
using System.Windows.Forms;
using System.Xml;
using Highlander.Core.Common;
using Highlander.Metadata.Common;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Serialisation;
using Exception = System.Exception;

#endregion

namespace Highlander.Reporting.UI.V5r3
{
    public partial class XmlTreeDisplay : Form
    {
        public ICoreItem CoreItem { get; set; }

        //public NamedValueSet NewItemProperties { get; set; }

        public ICoreClient Client { get; set; }

        public XmlTreeDisplay(ICoreClient client, ICoreItem coreItem)
        {
            Client = client;
            CoreItem = coreItem;
            InitializeComponent();
            LoadData();
        }

        private void XmlTreeDisplayLoad(object sender, EventArgs e)
        {
            txtXmlFile.Text = CoreItem.Name;//UniqueName
        }

        private void LoadData()
        {
            // Clear the tree.
            treeXml.Nodes.Clear();
            // Load the XML Document
            var doc = new XmlDocument();
            try
            {
                var xml = CoreItem.Text;
                doc.LoadXml(xml);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                return;
            }
            // Populate the TreeView.
            ConvertXmlNodeToTreeNode(doc, treeXml.Nodes);
            // Expand all nodes.
            treeXml.Nodes[0].ExpandAll();
        }

        private void ConvertXmlNodeToTreeNode(XmlNode xmlNode, TreeNodeCollection treeNodes)
        {
            // Add a TreeNode node that represents this XmlNode.
            TreeNode newTreeNode = treeNodes.Add(xmlNode.Name);
            // Customize the TreeNode text based on the XmlNode
            // type and content.
            switch (xmlNode.NodeType)
            {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.XmlDeclaration:
                    newTreeNode.Text = "<?" + xmlNode.Name + " " + xmlNode.Value + "?>";
                    break;
                case XmlNodeType.Element:
                    newTreeNode.Text = "<" + xmlNode.Name + ">";
                    break;
                case XmlNodeType.Attribute:
                    newTreeNode.Text = "ATTRIBUTE: " + xmlNode.Name;
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    newTreeNode.Text = xmlNode.Value;
                    break;
                case XmlNodeType.Comment:
                    newTreeNode.Text = "<!--" + xmlNode.Value + "-->";
                    break;
            }
            // Call this routine recursively for each attribute.
            // (XmlAttribute is a subclass of XmlNode.)
            if (xmlNode.Attributes != null)
            {
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    ConvertXmlNodeToTreeNode(attribute, newTreeNode.Nodes);
                }
            }
            // Call this routine recursively for each child node.
            // Typically, this child node represents a nested element,
            // or element content.
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                ConvertXmlNodeToTreeNode(childNode, newTreeNode.Nodes);
            }
        }

        private void BtnCollapseClick(object sender, EventArgs e)
        {
            treeXml.Nodes[0].Collapse();
        }

        private void BtnExpandNodesClick(object sender, EventArgs e)
        {
            treeXml.Nodes[0].ExpandAll();
        }

        private void BtnCloneObjectClick(object sender, EventArgs e)
        {
            txtNewXmlName.Text = $"{txtXmlFile.Text}.Copy";
        }

        private void BtnSaveObjectClick(object sender, EventArgs e)
        {
            //TODO End editing.
            //treeXml.Nodes[0].EndEdit(true);
            var xml = CoreItem.Text; //.AppProps.Serialise();
            var dataType = CoreItem.DataTypeName;
            //TODO Add other types for cloning.
            if (dataType == "Highlander.Reporting.V5r3.Market")//typeof(Market)
            {
                var market = XmlSerializerHelper.DeserializeFromString<Market>(xml);
                Client.SaveObject(market, txtNewXmlName.Text, CoreItem.AppProps);
            }
            if (dataType == "Highlander.Confirmation.V5r3.Trade")
            {
                var confirmationTrade = XmlSerializerHelper.DeserializeFromString<Confirmation.V5r3.Trade>(xml);
                Client.SaveObject(confirmationTrade, txtNewXmlName.Text, CoreItem.AppProps);
            }
            if (dataType == "Highlander.Reporting.V5r3.Trade")
            {
                var reportingTrade = XmlSerializerHelper.DeserializeFromString<Trade>(xml);
                Client.SaveObject(reportingTrade, txtNewXmlName.Text, CoreItem.AppProps);
            }
            if (dataType == "Highlander.Reporting.V5r3.ValuationReport")
            {
                var valuation = XmlSerializerHelper.DeserializeFromString<ValuationReport>(xml);
                Client.SaveObject(valuation, txtNewXmlName.Text, CoreItem.AppProps);
            }
            if (dataType == "Highlander.Reporting.V5r3.Instrument")
            {
                var instrument = XmlSerializerHelper.DeserializeFromString<Instrument>(xml);
                Client.SaveObject(instrument, txtNewXmlName.Text, CoreItem.AppProps);
            }
            if (dataType == "Highlander.Reporting.V5r3.QuotedAssetSet")
            {
                var quoteSet = XmlSerializerHelper.DeserializeFromString<QuotedAssetSet>(xml);
                Client.SaveObject(quoteSet, txtNewXmlName.Text, CoreItem.AppProps);
            }
            if (dataType == "Highlander.Configuration.Data.V5r3.Algorithm")
            {
                var quoteSet = XmlSerializerHelper.DeserializeFromString<Algorithm>(xml);
                Client.SaveObject(quoteSet, txtNewXmlName.Text, CoreItem.AppProps);
            }
        }

        private void BtnEditObjectClick(object sender, EventArgs e)
        {
            treeXml.LabelEdit = true;
            //treeXml.Nodes[0].BeginEdit();
        }
    }
}