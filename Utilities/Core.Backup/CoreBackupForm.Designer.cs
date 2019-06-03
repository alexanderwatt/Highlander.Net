namespace Core.Backup
{
    partial class CoreBackupForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnReadWrite = new System.Windows.Forms.Button();
            this.btnReadOnly = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtPropItemNameValue = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.chkPropItemName = new System.Windows.Forms.CheckBox();
            this.cbDataTypeValues = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.chkProp1 = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtTgtFilename = new System.Windows.Forms.TextBox();
            this.txtTgtServer = new System.Windows.Forms.TextBox();
            this.rbTgtFilename = new System.Windows.Forms.RadioButton();
            this.rbTgtServer = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtSrcFilename = new System.Windows.Forms.TextBox();
            this.txtSrcServer = new System.Windows.Forms.TextBox();
            this.rbSrcFilename = new System.Windows.Forms.RadioButton();
            this.rbSrcServer = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rbPreset4 = new System.Windows.Forms.RadioButton();
            this.rbPreset3 = new System.Windows.Forms.RadioButton();
            this.rbPreset2 = new System.Windows.Forms.RadioButton();
            this.rbPreset1 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnReadWrite);
            this.panel1.Controls.Add(this.btnReadOnly);
            this.panel1.Controls.Add(this.groupBox5);
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(819, 261);
            this.panel1.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(600, 215);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(207, 35);
            this.btnCancel.TabIndex = 56;
            this.btnCancel.Text = "Cancel running job!";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // btnReadWrite
            // 
            this.btnReadWrite.Location = new System.Drawing.Point(600, 76);
            this.btnReadWrite.Name = "btnReadWrite";
            this.btnReadWrite.Size = new System.Drawing.Size(207, 58);
            this.btnReadWrite.TabIndex = 5;
            this.btnReadWrite.Text = "TRANSFER (read source/write target)";
            this.btnReadWrite.UseVisualStyleBackColor = true;
            this.btnReadWrite.Click += new System.EventHandler(this.BtnReadWriteClick);
            // 
            // btnReadOnly
            // 
            this.btnReadOnly.Location = new System.Drawing.Point(600, 12);
            this.btnReadOnly.Name = "btnReadOnly";
            this.btnReadOnly.Size = new System.Drawing.Size(207, 58);
            this.btnReadOnly.TabIndex = 4;
            this.btnReadOnly.Text = "PREVIEW (read source only)";
            this.btnReadOnly.UseVisualStyleBackColor = true;
            this.btnReadOnly.Click += new System.EventHandler(this.BtnReadOnlyClick);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Controls.Add(this.txtPropItemNameValue);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Controls.Add(this.chkPropItemName);
            this.groupBox5.Controls.Add(this.cbDataTypeValues);
            this.groupBox5.Controls.Add(this.label8);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.chkProp1);
            this.groupBox5.Location = new System.Drawing.Point(3, 167);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(591, 83);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Filter";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(43, 43);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(106, 13);
            this.label11.TabIndex = 55;
            this.label11.Text = "Item name starts with";
            // 
            // txtPropItemNameValue
            // 
            this.txtPropItemNameValue.Location = new System.Drawing.Point(155, 40);
            this.txtPropItemNameValue.Name = "txtPropItemNameValue";
            this.txtPropItemNameValue.Size = new System.Drawing.Size(356, 20);
            this.txtPropItemNameValue.TabIndex = 54;
            this.txtPropItemNameValue.Text = "FpML.Configuration";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(60, 30);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(25, 13);
            this.label12.TabIndex = 53;
            this.label12.Text = "and";
            // 
            // chkPropItemName
            // 
            this.chkPropItemName.AutoSize = true;
            this.chkPropItemName.Checked = true;
            this.chkPropItemName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPropItemName.Location = new System.Drawing.Point(9, 42);
            this.chkPropItemName.Name = "chkPropItemName";
            this.chkPropItemName.Size = new System.Drawing.Size(29, 17);
            this.chkPropItemName.TabIndex = 52;
            this.chkPropItemName.Text = ":";
            this.chkPropItemName.UseVisualStyleBackColor = true;
            this.chkPropItemName.CheckedChanged += new System.EventHandler(this.ChkPropItemNameCheckedChanged);
            // 
            // cbDataTypeValues
            // 
            this.cbDataTypeValues.FormattingEnabled = true;
            this.cbDataTypeValues.Location = new System.Drawing.Point(155, 13);
            this.cbDataTypeValues.Name = "cbDataTypeValues";
            this.cbDataTypeValues.Size = new System.Drawing.Size(356, 21);
            this.cbDataTypeValues.TabIndex = 46;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(43, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(57, 13);
            this.label8.TabIndex = 45;
            this.label8.Text = "Data Type";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(130, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(19, 13);
            this.label5.TabIndex = 44;
            this.label5.Text = "==";
            // 
            // chkProp1
            // 
            this.chkProp1.AutoSize = true;
            this.chkProp1.Location = new System.Drawing.Point(9, 15);
            this.chkProp1.Name = "chkProp1";
            this.chkProp1.Size = new System.Drawing.Size(29, 17);
            this.chkProp1.TabIndex = 43;
            this.chkProp1.Text = ":";
            this.chkProp1.UseVisualStyleBackColor = true;
            this.chkProp1.CheckedChanged += new System.EventHandler(this.ChkProp1CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtTgtFilename);
            this.groupBox4.Controls.Add(this.txtTgtServer);
            this.groupBox4.Controls.Add(this.rbTgtFilename);
            this.groupBox4.Controls.Add(this.rbTgtServer);
            this.groupBox4.Location = new System.Drawing.Point(238, 85);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(356, 76);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Target";
            // 
            // txtTgtFilename
            // 
            this.txtTgtFilename.Location = new System.Drawing.Point(82, 45);
            this.txtTgtFilename.Name = "txtTgtFilename";
            this.txtTgtFilename.Size = new System.Drawing.Size(268, 20);
            this.txtTgtFilename.TabIndex = 6;
            // 
            // txtTgtServer
            // 
            this.txtTgtServer.Location = new System.Drawing.Point(82, 19);
            this.txtTgtServer.Name = "txtTgtServer";
            this.txtTgtServer.Size = new System.Drawing.Size(268, 20);
            this.txtTgtServer.TabIndex = 5;
            // 
            // rbTgtFilename
            // 
            this.rbTgtFilename.AutoSize = true;
            this.rbTgtFilename.Location = new System.Drawing.Point(6, 46);
            this.rbTgtFilename.Name = "rbTgtFilename";
            this.rbTgtFilename.Size = new System.Drawing.Size(68, 17);
            this.rbTgtFilename.TabIndex = 1;
            this.rbTgtFilename.TabStop = true;
            this.rbTgtFilename.Text = "Path/file:";
            this.rbTgtFilename.UseVisualStyleBackColor = true;
            // 
            // rbTgtServer
            // 
            this.rbTgtServer.AutoSize = true;
            this.rbTgtServer.Location = new System.Drawing.Point(6, 20);
            this.rbTgtServer.Name = "rbTgtServer";
            this.rbTgtServer.Size = new System.Drawing.Size(59, 17);
            this.rbTgtServer.TabIndex = 0;
            this.rbTgtServer.TabStop = true;
            this.rbTgtServer.Text = "Server:";
            this.rbTgtServer.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtSrcFilename);
            this.groupBox2.Controls.Add(this.txtSrcServer);
            this.groupBox2.Controls.Add(this.rbSrcFilename);
            this.groupBox2.Controls.Add(this.rbSrcServer);
            this.groupBox2.Location = new System.Drawing.Point(238, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(356, 76);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Source";
            // 
            // txtSrcFilename
            // 
            this.txtSrcFilename.Location = new System.Drawing.Point(82, 45);
            this.txtSrcFilename.Name = "txtSrcFilename";
            this.txtSrcFilename.Size = new System.Drawing.Size(268, 20);
            this.txtSrcFilename.TabIndex = 6;
            // 
            // txtSrcServer
            // 
            this.txtSrcServer.Location = new System.Drawing.Point(82, 19);
            this.txtSrcServer.Name = "txtSrcServer";
            this.txtSrcServer.Size = new System.Drawing.Size(268, 20);
            this.txtSrcServer.TabIndex = 5;
            // 
            // rbSrcFilename
            // 
            this.rbSrcFilename.AutoSize = true;
            this.rbSrcFilename.Location = new System.Drawing.Point(6, 46);
            this.rbSrcFilename.Name = "rbSrcFilename";
            this.rbSrcFilename.Size = new System.Drawing.Size(68, 17);
            this.rbSrcFilename.TabIndex = 1;
            this.rbSrcFilename.TabStop = true;
            this.rbSrcFilename.Text = "Path/file:";
            this.rbSrcFilename.UseVisualStyleBackColor = true;
            // 
            // rbSrcServer
            // 
            this.rbSrcServer.AutoSize = true;
            this.rbSrcServer.Location = new System.Drawing.Point(6, 20);
            this.rbSrcServer.Name = "rbSrcServer";
            this.rbSrcServer.Size = new System.Drawing.Size(59, 17);
            this.rbSrcServer.TabIndex = 0;
            this.rbSrcServer.TabStop = true;
            this.rbSrcServer.Text = "Server:";
            this.rbSrcServer.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rbPreset4);
            this.groupBox3.Controls.Add(this.rbPreset3);
            this.groupBox3.Controls.Add(this.rbPreset2);
            this.groupBox3.Controls.Add(this.rbPreset1);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(229, 158);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Presets";
            // 
            // rbPreset4
            // 
            this.rbPreset4.AutoSize = true;
            this.rbPreset4.Location = new System.Drawing.Point(6, 42);
            this.rbPreset4.Name = "rbPreset4";
            this.rbPreset4.Size = new System.Drawing.Size(161, 17);
            this.rbPreset4.TabIndex = 5;
            this.rbPreset4.TabStop = true;
            this.rbPreset4.Text = "COPY from SERVER to FILE";
            this.rbPreset4.UseVisualStyleBackColor = true;
            this.rbPreset4.CheckedChanged += new System.EventHandler(this.RbPreset4CheckedChanged);
            // 
            // rbPreset3
            // 
            this.rbPreset3.AutoSize = true;
            this.rbPreset3.Location = new System.Drawing.Point(6, 88);
            this.rbPreset3.Name = "rbPreset3";
            this.rbPreset3.Size = new System.Drawing.Size(200, 17);
            this.rbPreset3.TabIndex = 4;
            this.rbPreset3.Text = "RESTORE from FILE to LOCAL store";
            this.rbPreset3.UseVisualStyleBackColor = true;
            this.rbPreset3.CheckedChanged += new System.EventHandler(this.RbPreset3CheckedChanged);
            // 
            // rbPreset2
            // 
            this.rbPreset2.AutoSize = true;
            this.rbPreset2.Location = new System.Drawing.Point(6, 65);
            this.rbPreset2.Name = "rbPreset2";
            this.rbPreset2.Size = new System.Drawing.Size(184, 17);
            this.rbPreset2.TabIndex = 3;
            this.rbPreset2.Text = "BACKUP my LOCAL store to FILE";
            this.rbPreset2.UseVisualStyleBackColor = true;
            this.rbPreset2.CheckedChanged += new System.EventHandler(this.RbPreset2CheckedChanged);
            // 
            // rbPreset1
            // 
            this.rbPreset1.AutoSize = true;
            this.rbPreset1.Location = new System.Drawing.Point(6, 19);
            this.rbPreset1.Name = "rbPreset1";
            this.rbPreset1.Size = new System.Drawing.Size(173, 17);
            this.rbPreset1.TabIndex = 2;
            this.rbPreset1.Text = "COPY from SERVER to LOCAL";
            this.rbPreset1.UseVisualStyleBackColor = true;
            this.rbPreset1.CheckedChanged += new System.EventHandler(this.RbPreset1CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtLog);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 261);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(819, 277);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Log";
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(3, 16);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(813, 258);
            this.txtLog.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(819, 538);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "QDS Core Backup & Restore";
            this.Load += new System.EventHandler(this.Form1Load);
            this.panel1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbSrcFilename;
        private System.Windows.Forms.RadioButton rbSrcServer;
        private System.Windows.Forms.TextBox txtSrcFilename;
        private System.Windows.Forms.TextBox txtSrcServer;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtTgtFilename;
        private System.Windows.Forms.TextBox txtTgtServer;
        private System.Windows.Forms.RadioButton rbTgtFilename;
        private System.Windows.Forms.RadioButton rbTgtServer;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.RadioButton rbPreset1;
        private System.Windows.Forms.RadioButton rbPreset2;
        private System.Windows.Forms.Button btnReadWrite;
        private System.Windows.Forms.Button btnReadOnly;
        private System.Windows.Forms.ComboBox cbDataTypeValues;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkProp1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtPropItemNameValue;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox chkPropItemName;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton rbPreset3;
        private System.Windows.Forms.RadioButton rbPreset4;
    }
}

