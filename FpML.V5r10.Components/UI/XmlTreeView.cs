#region Usings

using System;
using System.Windows.Forms;
using System.Xml;
using Core.Common;

#endregion

namespace Orion.UI
{
    public partial class XmlTreeDisplay : Form
    {
        public ICoreItem TradeItem { get; set; }

        public XmlTreeDisplay(ICoreItem tradeItem)
        {
            TradeItem = tradeItem;
            InitializeComponent();
            LoadData();
        }

        private void XmlTreeDisplayLoad(object sender, EventArgs e)
        {
            txtXmlFile.Text = TradeItem.UniqueName;
        }

        private void LoadData()
        {
            // Clear the tree.
            treeXml.Nodes.Clear();
            // Load the XML Document
            var doc = new XmlDocument();
            try
            {
                var xml = TradeItem.Text;
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
    }
}