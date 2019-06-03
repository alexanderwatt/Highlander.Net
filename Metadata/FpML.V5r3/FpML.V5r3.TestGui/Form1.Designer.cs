namespace FpML.V5r3.TestGui
{
    partial class Form1
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.clbTests = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.rbExternal = new System.Windows.Forms.RadioButton();
            this.btnRunTests = new System.Windows.Forms.Button();
            this.btnSelectWorkPath = new System.Windows.Forms.Button();
            this.txtWorkPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSelectSourcePath = new System.Windows.Forms.Button();
            this.txtSourcePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpLog = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.tpExternalSource = new System.Windows.Forms.TabPage();
            this.txtExternalXml = new System.Windows.Forms.TextBox();
            this.tpImported = new System.Windows.Forms.TabPage();
            this.txtImportedXml = new System.Windows.Forms.TextBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtInternalXml = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtExportedXml = new System.Windows.Forms.TextBox();
            this.ofdSelectSourcePath = new System.Windows.Forms.OpenFileDialog();
            this.fbdSelectWorkPath = new System.Windows.Forms.FolderBrowserDialog();
            this.rbInternal = new System.Windows.Forms.RadioButton();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.btnSetupTests = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpLog.SuspendLayout();
            this.tpExternalSource.SuspendLayout();
            this.tpImported.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1.Controls.Add(this.btnSelectWorkPath);
            this.splitContainer1.Panel1.Controls.Add(this.txtWorkPath);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.btnSelectSourcePath);
            this.splitContainer1.Panel1.Controls.Add(this.txtSourcePath);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(802, 407);
            this.splitContainer1.SplitterDistance = 239;
            this.splitContainer1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.splitContainer2);
            this.panel1.Location = new System.Drawing.Point(4, 69);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(794, 160);
            this.panel1.TabIndex = 6;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.clbTests);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer2.Size = new System.Drawing.Size(794, 160);
            this.splitContainer2.SplitterDistance = 512;
            this.splitContainer2.TabIndex = 0;
            // 
            // clbTests
            // 
            this.clbTests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clbTests.FormattingEnabled = true;
            this.clbTests.Location = new System.Drawing.Point(0, 0);
            this.clbTests.Name = "clbTests";
            this.clbTests.Size = new System.Drawing.Size(512, 160);
            this.clbTests.TabIndex = 9;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSetupTests);
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Controls.Add(this.rbInternal);
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Controls.Add(this.rbExternal);
            this.groupBox2.Controls.Add(this.btnRunTests);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(278, 160);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Run tests";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(26, 19);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(244, 43);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = "External - source file will be validated against FpML.org schema prior to importi" +
    "ng for internal deserialisation/serialisation with C# classes.";
            // 
            // rbExternal
            // 
            this.rbExternal.AutoSize = true;
            this.rbExternal.Checked = true;
            this.rbExternal.Location = new System.Drawing.Point(6, 19);
            this.rbExternal.Name = "rbExternal";
            this.rbExternal.Size = new System.Drawing.Size(14, 13);
            this.rbExternal.TabIndex = 3;
            this.rbExternal.TabStop = true;
            this.rbExternal.UseVisualStyleBackColor = true;
            // 
            // btnRunTests
            // 
            this.btnRunTests.Location = new System.Drawing.Point(128, 119);
            this.btnRunTests.Name = "btnRunTests";
            this.btnRunTests.Size = new System.Drawing.Size(116, 23);
            this.btnRunTests.TabIndex = 2;
            this.btnRunTests.Text = "Run tests";
            this.btnRunTests.UseVisualStyleBackColor = true;
            this.btnRunTests.Click += new System.EventHandler(this.BtnRunTestsClick);
            // 
            // btnSelectWorkPath
            // 
            this.btnSelectWorkPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectWorkPath.Location = new System.Drawing.Point(763, 41);
            this.btnSelectWorkPath.Name = "btnSelectWorkPath";
            this.btnSelectWorkPath.Size = new System.Drawing.Size(27, 23);
            this.btnSelectWorkPath.TabIndex = 5;
            this.btnSelectWorkPath.Text = "...";
            this.btnSelectWorkPath.UseVisualStyleBackColor = true;
            this.btnSelectWorkPath.Click += new System.EventHandler(this.BtnSelectWorkPathClick);
            // 
            // txtWorkPath
            // 
            this.txtWorkPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWorkPath.Location = new System.Drawing.Point(78, 43);
            this.txtWorkPath.Name = "txtWorkPath";
            this.txtWorkPath.Size = new System.Drawing.Size(679, 20);
            this.txtWorkPath.TabIndex = 4;
            this.txtWorkPath.Text = "D:\\temp\\FpMLTest";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Work path:";
            // 
            // btnSelectSourcePath
            // 
            this.btnSelectSourcePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectSourcePath.Location = new System.Drawing.Point(763, 12);
            this.btnSelectSourcePath.Name = "btnSelectSourcePath";
            this.btnSelectSourcePath.Size = new System.Drawing.Size(27, 23);
            this.btnSelectSourcePath.TabIndex = 2;
            this.btnSelectSourcePath.Text = "...";
            this.btnSelectSourcePath.UseVisualStyleBackColor = true;
            this.btnSelectSourcePath.Click += new System.EventHandler(this.BtnSelectSourcePathClick);
            // 
            // txtSourcePath
            // 
            this.txtSourcePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourcePath.Location = new System.Drawing.Point(86, 14);
            this.txtSourcePath.Name = "txtSourcePath";
            this.txtSourcePath.Size = new System.Drawing.Size(671, 20);
            this.txtSourcePath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source path:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpLog);
            this.tabControl1.Controls.Add(this.tpExternalSource);
            this.tabControl1.Controls.Add(this.tpImported);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(802, 164);
            this.tabControl1.TabIndex = 0;
            // 
            // tpLog
            // 
            this.tpLog.Controls.Add(this.txtLog);
            this.tpLog.Location = new System.Drawing.Point(4, 22);
            this.tpLog.Name = "tpLog";
            this.tpLog.Padding = new System.Windows.Forms.Padding(3);
            this.tpLog.Size = new System.Drawing.Size(794, 138);
            this.tpLog.TabIndex = 0;
            this.tpLog.Text = "Log";
            this.tpLog.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(3, 3);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(788, 132);
            this.txtLog.TabIndex = 1;
            // 
            // tpExternalSource
            // 
            this.tpExternalSource.Controls.Add(this.txtExternalXml);
            this.tpExternalSource.Location = new System.Drawing.Point(4, 22);
            this.tpExternalSource.Name = "tpExternalSource";
            this.tpExternalSource.Padding = new System.Windows.Forms.Padding(3);
            this.tpExternalSource.Size = new System.Drawing.Size(794, 138);
            this.tpExternalSource.TabIndex = 1;
            this.tpExternalSource.Text = "External Source";
            this.tpExternalSource.UseVisualStyleBackColor = true;
            // 
            // txtExternalXml
            // 
            this.txtExternalXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtExternalXml.Location = new System.Drawing.Point(3, 3);
            this.txtExternalXml.Multiline = true;
            this.txtExternalXml.Name = "txtExternalXml";
            this.txtExternalXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtExternalXml.Size = new System.Drawing.Size(788, 132);
            this.txtExternalXml.TabIndex = 3;
            // 
            // tpImported
            // 
            this.tpImported.Controls.Add(this.txtImportedXml);
            this.tpImported.Location = new System.Drawing.Point(4, 22);
            this.tpImported.Name = "tpImported";
            this.tpImported.Padding = new System.Windows.Forms.Padding(3);
            this.tpImported.Size = new System.Drawing.Size(794, 138);
            this.tpImported.TabIndex = 2;
            this.tpImported.Text = "Imported (internal)";
            this.tpImported.UseVisualStyleBackColor = true;
            // 
            // txtImportedXml
            // 
            this.txtImportedXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtImportedXml.Location = new System.Drawing.Point(3, 3);
            this.txtImportedXml.Multiline = true;
            this.txtImportedXml.Name = "txtImportedXml";
            this.txtImportedXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtImportedXml.Size = new System.Drawing.Size(788, 132);
            this.txtImportedXml.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtInternalXml);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(794, 138);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "Serialised (internal)";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtInternalXml
            // 
            this.txtInternalXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInternalXml.Location = new System.Drawing.Point(3, 3);
            this.txtInternalXml.Multiline = true;
            this.txtInternalXml.Name = "txtInternalXml";
            this.txtInternalXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInternalXml.Size = new System.Drawing.Size(788, 132);
            this.txtInternalXml.TabIndex = 3;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.txtExportedXml);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(794, 138);
            this.tabPage2.TabIndex = 4;
            this.tabPage2.Text = "Exported (external)";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtExportedXml
            // 
            this.txtExportedXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtExportedXml.Location = new System.Drawing.Point(3, 3);
            this.txtExportedXml.Multiline = true;
            this.txtExportedXml.Name = "txtExportedXml";
            this.txtExportedXml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtExportedXml.Size = new System.Drawing.Size(788, 132);
            this.txtExportedXml.TabIndex = 4;
            // 
            // ofdSelectSourcePath
            // 
            this.ofdSelectSourcePath.FileOk += new System.ComponentModel.CancelEventHandler(this.OfdSelectSourcePathFileOk);
            // 
            // rbInternal
            // 
            this.rbInternal.AutoSize = true;
            this.rbInternal.Location = new System.Drawing.Point(6, 68);
            this.rbInternal.Name = "rbInternal";
            this.rbInternal.Size = new System.Drawing.Size(14, 13);
            this.rbInternal.TabIndex = 5;
            this.rbInternal.TabStop = true;
            this.rbInternal.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Location = new System.Drawing.Point(26, 68);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(244, 45);
            this.textBox2.TabIndex = 6;
            this.textBox2.Text = "Internal - source file will be deserialised, re-serialised, and exported prior to" +
    " validation against FpML.org schema.";
            // 
            // btnSetupTests
            // 
            this.btnSetupTests.Location = new System.Drawing.Point(6, 119);
            this.btnSetupTests.Name = "btnSetupTests";
            this.btnSetupTests.Size = new System.Drawing.Size(116, 23);
            this.btnSetupTests.TabIndex = 7;
            this.btnSetupTests.Text = "Setup tests";
            this.btnSetupTests.UseVisualStyleBackColor = true;
            this.btnSetupTests.Click += new System.EventHandler(this.BtnSetupTestsClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(802, 407);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "FpML V5r3 Test Harness (Confirmations)";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tpLog.ResumeLayout(false);
            this.tpLog.PerformLayout();
            this.tpExternalSource.ResumeLayout(false);
            this.tpExternalSource.PerformLayout();
            this.tpImported.ResumeLayout(false);
            this.tpImported.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnSelectWorkPath;
        private System.Windows.Forms.TextBox txtWorkPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSelectSourcePath;
        private System.Windows.Forms.TextBox txtSourcePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpLog;
        private System.Windows.Forms.TabPage tpExternalSource;
        private System.Windows.Forms.OpenFileDialog ofdSelectSourcePath;
        private System.Windows.Forms.FolderBrowserDialog fbdSelectWorkPath;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.CheckedListBox clbTests;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnRunTests;
        private System.Windows.Forms.TabPage tpImported;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TextBox txtImportedXml;
        private System.Windows.Forms.TextBox txtExternalXml;
        private System.Windows.Forms.TextBox txtInternalXml;
        private System.Windows.Forms.TextBox txtExportedXml;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.RadioButton rbExternal;
        private System.Windows.Forms.Button btnSetupTests;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.RadioButton rbInternal;
    }
}

