using System;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace Highlander.XmlViewer
{
    public partial class XmlTreeDisplay : Form
    {
        public XmlTreeDisplay()
        {
            InitializeComponent();
        }

        private void XmlTreeDisplayLoad(object sender, EventArgs e)
        {
            txtXmlFile.Text = Path.Combine(Application.StartupPath,
               @"..\..\ProductCatalog.xml");
        }

        private void CmdLoadClick(object sender, EventArgs e)
        {
            // Clear the tree.
            treeXml.Nodes.Clear();
            var fileOpen = new OpenFileDialog
                               {
                                   InitialDirectory = ".\\",
                                   Filter = "XML files (*.xml)|*.xml|" +
                                            "All files (*.*)|*.*",
                                   FilterIndex = 1,
                                   RestoreDirectory = true
                               };
            if (fileOpen.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            txtXmlFile.Text = fileOpen.FileName;
            // Load the XML Document
            var doc = new XmlDocument();
            try
            {
                doc.Load(txtXmlFile.Text);
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