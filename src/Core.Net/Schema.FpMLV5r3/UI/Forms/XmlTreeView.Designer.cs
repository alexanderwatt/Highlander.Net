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

namespace Highlander.Reporting.UI.V5r3
{
    partial class XmlTreeDisplay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtXmlFile = new System.Windows.Forms.TextBox();
            this.treeXml = new System.Windows.Forms.TreeView();
            this.btnExpand = new System.Windows.Forms.Button();
            this.btnExpandNodes = new System.Windows.Forms.Button();
            this.btnEditObject = new System.Windows.Forms.Button();
            this.btnSaveObject = new System.Windows.Forms.Button();
            this.btnCloneObject = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNewXmlName = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 18);
            this.label1.TabIndex = 7;
            this.label1.Text = "Original Identifier:";
            // 
            // txtXmlFile
            // 
            this.txtXmlFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtXmlFile.Location = new System.Drawing.Point(101, 85);
            this.txtXmlFile.Name = "txtXmlFile";
            this.txtXmlFile.Size = new System.Drawing.Size(486, 21);
            this.txtXmlFile.TabIndex = 5;
            // 
            // treeXml
            // 
            this.treeXml.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeXml.Location = new System.Drawing.Point(4, 133);
            this.treeXml.Name = "treeXml";
            this.treeXml.Size = new System.Drawing.Size(583, 678);
            this.treeXml.TabIndex = 4;
            // 
            // btnExpand
            // 
            this.btnExpand.Location = new System.Drawing.Point(3, 3);
            this.btnExpand.Name = "btnExpand";
            this.btnExpand.Size = new System.Drawing.Size(93, 67);
            this.btnExpand.TabIndex = 8;
            this.btnExpand.Text = "Collapse Nodes";
            this.btnExpand.UseVisualStyleBackColor = true;
            this.btnExpand.Click += new System.EventHandler(this.BtnCollapseClick);
            // 
            // btnExpandNodes
            // 
            this.btnExpandNodes.Location = new System.Drawing.Point(102, 3);
            this.btnExpandNodes.Name = "btnExpandNodes";
            this.btnExpandNodes.Size = new System.Drawing.Size(93, 67);
            this.btnExpandNodes.TabIndex = 9;
            this.btnExpandNodes.Text = "Expand Nodes";
            this.btnExpandNodes.UseVisualStyleBackColor = true;
            this.btnExpandNodes.Click += new System.EventHandler(this.BtnExpandNodesClick);
            // 
            // btnEditObject
            // 
            this.btnEditObject.Location = new System.Drawing.Point(106, 5);
            this.btnEditObject.Name = "btnEditObject";
            this.btnEditObject.Size = new System.Drawing.Size(93, 63);
            this.btnEditObject.TabIndex = 10;
            this.btnEditObject.Text = "Edit Object";
            this.btnEditObject.UseVisualStyleBackColor = true;
            this.btnEditObject.Click += new System.EventHandler(this.BtnEditObjectClick);
            // 
            // btnSaveObject
            // 
            this.btnSaveObject.Location = new System.Drawing.Point(206, 6);
            this.btnSaveObject.Name = "btnSaveObject";
            this.btnSaveObject.Size = new System.Drawing.Size(93, 63);
            this.btnSaveObject.TabIndex = 11;
            this.btnSaveObject.Text = "Save Object";
            this.btnSaveObject.UseVisualStyleBackColor = true;
            this.btnSaveObject.Click += new System.EventHandler(this.BtnSaveObjectClick);
            // 
            // btnCloneObject
            // 
            this.btnCloneObject.Location = new System.Drawing.Point(6, 5);
            this.btnCloneObject.Name = "btnCloneObject";
            this.btnCloneObject.Size = new System.Drawing.Size(93, 63);
            this.btnCloneObject.TabIndex = 12;
            this.btnCloneObject.Text = "Clone Object";
            this.btnCloneObject.UseVisualStyleBackColor = true;
            this.btnCloneObject.Click += new System.EventHandler(this.BtnCloneObjectClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCloneObject);
            this.panel1.Controls.Add(this.btnSaveObject);
            this.panel1.Controls.Add(this.btnEditObject);
            this.panel1.Location = new System.Drawing.Point(4, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(305, 73);
            this.panel1.TabIndex = 13;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnExpand);
            this.panel2.Controls.Add(this.btnExpandNodes);
            this.panel2.Location = new System.Drawing.Point(388, 6);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(198, 73);
            this.panel2.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(5, 112);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 18);
            this.label2.TabIndex = 16;
            this.label2.Text = "New Identifier:";
            // 
            // txtNewXmlName
            // 
            this.txtNewXmlName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNewXmlName.Location = new System.Drawing.Point(101, 109);
            this.txtNewXmlName.Name = "txtNewXmlName";
            this.txtNewXmlName.Size = new System.Drawing.Size(486, 21);
            this.txtNewXmlName.TabIndex = 15;
            // 
            // XmlTreeDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(593, 795);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtNewXmlName);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtXmlFile);
            this.Controls.Add(this.treeXml);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "XmlTreeDisplay";
            this.Text = "XmlTreeDisplay";
            this.Load += new System.EventHandler(this.XmlTreeDisplayLoad);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtXmlFile;
        private System.Windows.Forms.TreeView treeXml;
        private System.Windows.Forms.Button btnExpand;
        private System.Windows.Forms.Button btnExpandNodes;
        private System.Windows.Forms.Button btnEditObject;
        private System.Windows.Forms.Button btnSaveObject;
        private System.Windows.Forms.Button btnCloneObject;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNewXmlName;
    }
}