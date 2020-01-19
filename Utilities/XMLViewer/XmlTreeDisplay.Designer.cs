namespace Highlander.XmlViewer
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
            this.cmdLoad = new System.Windows.Forms.Button();
            this.txtXmlFile = new System.Windows.Forms.TextBox();
            this.treeXml = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "File:";
            // 
            // cmdLoad
            // 
            this.cmdLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdLoad.Location = new System.Drawing.Point(524, 5);
            this.cmdLoad.Name = "cmdLoad";
            this.cmdLoad.Size = new System.Drawing.Size(56, 24);
            this.cmdLoad.TabIndex = 6;
            this.cmdLoad.Text = "Load";
            this.cmdLoad.Click += new System.EventHandler(this.CmdLoadClick);
            // 
            // txtXmlFile
            // 
            this.txtXmlFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtXmlFile.Location = new System.Drawing.Point(41, 8);
            this.txtXmlFile.Name = "txtXmlFile";
            this.txtXmlFile.Size = new System.Drawing.Size(478, 21);
            this.txtXmlFile.TabIndex = 5;
            // 
            // treeXml
            // 
            this.treeXml.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeXml.Location = new System.Drawing.Point(4, 35);
            this.treeXml.Name = "treeXml";
            this.treeXml.Size = new System.Drawing.Size(577, 509);
            this.treeXml.TabIndex = 4;
            // 
            // XmlTreeDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(593, 528);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdLoad);
            this.Controls.Add(this.txtXmlFile);
            this.Controls.Add(this.treeXml);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "XmlTreeDisplay";
            this.Text = "XmlTreeDisplay";
            this.Load += new System.EventHandler(this.XmlTreeDisplayLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdLoad;
        private System.Windows.Forms.TextBox txtXmlFile;
        private System.Windows.Forms.TreeView treeXml;
    }
}