namespace LoadConfigGui
{
    partial class LoadConfigForm
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxHolidayDates = new System.Windows.Forms.CheckBox();
            this.checkBoxFpML = new System.Windows.Forms.CheckBox();
            this.checkBoxMarkets = new System.Windows.Forms.CheckBox();
            this.cBoxNameSpaces = new System.Windows.Forms.ComboBox();
            this.bondDataCheckBox = new System.Windows.Forms.CheckBox();
            this.chkDeleteAllAppSettings = new System.Windows.Forms.CheckBox();
            this.chkDeleteAllStatus = new System.Windows.Forms.CheckBox();
            this.chkDeleteAllConfig = new System.Windows.Forms.CheckBox();
            this.btnUnselectAll = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.chkDateRules = new System.Windows.Forms.CheckBox();
            this.chkStressRules = new System.Windows.Forms.CheckBox();
            this.chkAlgorithmConfig = new System.Windows.Forms.CheckBox();
            this.chkInstrumentsConfig = new System.Windows.Forms.CheckBox();
            this.chkAppSettings = new System.Windows.Forms.CheckBox();
            this.chkTradeImportRules = new System.Windows.Forms.CheckBox();
            this.chkFileImportRules = new System.Windows.Forms.CheckBox();
            this.chkAlertMonitorRules = new System.Windows.Forms.CheckBox();
            this.chkPricingStructureDefs = new System.Windows.Forms.CheckBox();
            this.chkMDSProviderMaps = new System.Windows.Forms.CheckBox();
            this.btnLoadSelected = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(746, 148);
            this.panel1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxHolidayDates);
            this.groupBox2.Controls.Add(this.checkBoxFpML);
            this.groupBox2.Controls.Add(this.checkBoxMarkets);
            this.groupBox2.Controls.Add(this.cBoxNameSpaces);
            this.groupBox2.Controls.Add(this.bondDataCheckBox);
            this.groupBox2.Controls.Add(this.chkDeleteAllAppSettings);
            this.groupBox2.Controls.Add(this.chkDeleteAllStatus);
            this.groupBox2.Controls.Add(this.chkDeleteAllConfig);
            this.groupBox2.Controls.Add(this.btnUnselectAll);
            this.groupBox2.Controls.Add(this.btnSelectAll);
            this.groupBox2.Controls.Add(this.chkDateRules);
            this.groupBox2.Controls.Add(this.chkStressRules);
            this.groupBox2.Controls.Add(this.chkAlgorithmConfig);
            this.groupBox2.Controls.Add(this.chkInstrumentsConfig);
            this.groupBox2.Controls.Add(this.chkAppSettings);
            this.groupBox2.Controls.Add(this.chkTradeImportRules);
            this.groupBox2.Controls.Add(this.chkFileImportRules);
            this.groupBox2.Controls.Add(this.chkAlertMonitorRules);
            this.groupBox2.Controls.Add(this.chkPricingStructureDefs);
            this.groupBox2.Controls.Add(this.chkMDSProviderMaps);
            this.groupBox2.Controls.Add(this.btnLoadSelected);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(746, 148);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Configuration data";
            // 
            // checkBoxHolidayDates
            // 
            this.checkBoxHolidayDates.AutoSize = true;
            this.checkBoxHolidayDates.Location = new System.Drawing.Point(394, 113);
            this.checkBoxHolidayDates.Name = "checkBoxHolidayDates";
            this.checkBoxHolidayDates.Size = new System.Drawing.Size(92, 17);
            this.checkBoxHolidayDates.TabIndex = 22;
            this.checkBoxHolidayDates.Text = "Holiday Dates";
            this.checkBoxHolidayDates.UseVisualStyleBackColor = true;
            // 
            // checkBoxFpML
            // 
            this.checkBoxFpML.AutoSize = true;
            this.checkBoxFpML.Location = new System.Drawing.Point(229, 114);
            this.checkBoxFpML.Name = "checkBoxFpML";
            this.checkBoxFpML.Size = new System.Drawing.Size(72, 17);
            this.checkBoxFpML.TabIndex = 21;
            this.checkBoxFpML.Text = "Fpml data";
            this.checkBoxFpML.UseVisualStyleBackColor = true;
            // 
            // checkBoxMarkets
            // 
            this.checkBoxMarkets.AutoSize = true;
            this.checkBoxMarkets.Location = new System.Drawing.Point(393, 88);
            this.checkBoxMarkets.Name = "checkBoxMarkets";
            this.checkBoxMarkets.Size = new System.Drawing.Size(97, 17);
            this.checkBoxMarkets.TabIndex = 20;
            this.checkBoxMarkets.Text = "Markets Config";
            this.checkBoxMarkets.UseVisualStyleBackColor = true;
            this.checkBoxMarkets.CheckedChanged += new System.EventHandler(this.CheckBoxMarketsCheckedChanged);
            // 
            // cBoxNameSpaces
            // 
            this.cBoxNameSpaces.FormattingEnabled = true;
            this.cBoxNameSpaces.Items.AddRange(new object[] {
            "Alexander",
            "FpML.V4r7",
            "Orion",
            "Orion.V5r3"});
            this.cBoxNameSpaces.Location = new System.Drawing.Point(537, 114);
            this.cBoxNameSpaces.Name = "cBoxNameSpaces";
            this.cBoxNameSpaces.Size = new System.Drawing.Size(115, 21);
            this.cBoxNameSpaces.Sorted = true;
            this.cBoxNameSpaces.TabIndex = 19;
            this.cBoxNameSpaces.Text = "Orion";
            this.cBoxNameSpaces.SelectedIndexChanged += new System.EventHandler(this.ComboBox1SelectedIndexChanged);
            // 
            // bondDataCheckBox
            // 
            this.bondDataCheckBox.AutoSize = true;
            this.bondDataCheckBox.Location = new System.Drawing.Point(229, 89);
            this.bondDataCheckBox.Name = "bondDataCheckBox";
            this.bondDataCheckBox.Size = new System.Drawing.Size(75, 17);
            this.bondDataCheckBox.TabIndex = 18;
            this.bondDataCheckBox.Text = "Bond data";
            this.bondDataCheckBox.UseVisualStyleBackColor = true;
            // 
            // chkDeleteAllAppSettings
            // 
            this.chkDeleteAllAppSettings.AutoSize = true;
            this.chkDeleteAllAppSettings.Location = new System.Drawing.Point(551, 65);
            this.chkDeleteAllAppSettings.Name = "chkDeleteAllAppSettings";
            this.chkDeleteAllAppSettings.Size = new System.Drawing.Size(120, 17);
            this.chkDeleteAllAppSettings.TabIndex = 17;
            this.chkDeleteAllAppSettings.Text = "Delete App Settings";
            this.chkDeleteAllAppSettings.UseVisualStyleBackColor = true;
            // 
            // chkDeleteAllStatus
            // 
            this.chkDeleteAllStatus.AutoSize = true;
            this.chkDeleteAllStatus.Location = new System.Drawing.Point(551, 42);
            this.chkDeleteAllStatus.Name = "chkDeleteAllStatus";
            this.chkDeleteAllStatus.Size = new System.Drawing.Size(97, 17);
            this.chkDeleteAllStatus.TabIndex = 16;
            this.chkDeleteAllStatus.Text = "Delete Status.*";
            this.chkDeleteAllStatus.UseVisualStyleBackColor = true;
            // 
            // chkDeleteAllConfig
            // 
            this.chkDeleteAllConfig.AutoSize = true;
            this.chkDeleteAllConfig.Location = new System.Drawing.Point(551, 19);
            this.chkDeleteAllConfig.Name = "chkDeleteAllConfig";
            this.chkDeleteAllConfig.Size = new System.Drawing.Size(129, 17);
            this.chkDeleteAllConfig.TabIndex = 15;
            this.chkDeleteAllConfig.Text = "Delete Configuration.*";
            this.chkDeleteAllConfig.UseVisualStyleBackColor = true;
            this.chkDeleteAllConfig.CheckedChanged += new System.EventHandler(this.ChkDeleteAllConfigCheckedChanged);
            // 
            // btnUnselectAll
            // 
            this.btnUnselectAll.Location = new System.Drawing.Point(93, 111);
            this.btnUnselectAll.Name = "btnUnselectAll";
            this.btnUnselectAll.Size = new System.Drawing.Size(75, 23);
            this.btnUnselectAll.TabIndex = 14;
            this.btnUnselectAll.Text = "Unselect All";
            this.btnUnselectAll.UseVisualStyleBackColor = true;
            this.btnUnselectAll.Click += new System.EventHandler(this.BtnUnselectAllClick);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(12, 111);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(75, 23);
            this.btnSelectAll.TabIndex = 13;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.BtnSelectAllClick);
            // 
            // chkDateRules
            // 
            this.chkDateRules.AutoSize = true;
            this.chkDateRules.Location = new System.Drawing.Point(393, 19);
            this.chkDateRules.Name = "chkDateRules";
            this.chkDateRules.Size = new System.Drawing.Size(79, 17);
            this.chkDateRules.TabIndex = 12;
            this.chkDateRules.Text = "Date Rules";
            this.chkDateRules.UseVisualStyleBackColor = true;
            // 
            // chkStressRules
            // 
            this.chkStressRules.AutoSize = true;
            this.chkStressRules.Location = new System.Drawing.Point(229, 19);
            this.chkStressRules.Name = "chkStressRules";
            this.chkStressRules.Size = new System.Drawing.Size(125, 17);
            this.chkStressRules.TabIndex = 11;
            this.chkStressRules.Text = "Stress/scenario rules";
            this.chkStressRules.UseVisualStyleBackColor = true;
            // 
            // chkAlgorithmConfig
            // 
            this.chkAlgorithmConfig.AutoSize = true;
            this.chkAlgorithmConfig.Location = new System.Drawing.Point(393, 42);
            this.chkAlgorithmConfig.Name = "chkAlgorithmConfig";
            this.chkAlgorithmConfig.Size = new System.Drawing.Size(102, 17);
            this.chkAlgorithmConfig.TabIndex = 10;
            this.chkAlgorithmConfig.Text = "Algorithm Config";
            this.chkAlgorithmConfig.UseVisualStyleBackColor = true;
            // 
            // chkInstrumentsConfig
            // 
            this.chkInstrumentsConfig.AutoSize = true;
            this.chkInstrumentsConfig.Location = new System.Drawing.Point(393, 65);
            this.chkInstrumentsConfig.Name = "chkInstrumentsConfig";
            this.chkInstrumentsConfig.Size = new System.Drawing.Size(113, 17);
            this.chkInstrumentsConfig.TabIndex = 9;
            this.chkInstrumentsConfig.Text = "Instruments Config";
            this.chkInstrumentsConfig.UseVisualStyleBackColor = true;
            // 
            // chkAppSettings
            // 
            this.chkAppSettings.AutoSize = true;
            this.chkAppSettings.Location = new System.Drawing.Point(12, 88);
            this.chkAppSettings.Name = "chkAppSettings";
            this.chkAppSettings.Size = new System.Drawing.Size(86, 17);
            this.chkAppSettings.TabIndex = 8;
            this.chkAppSettings.Text = "App Settings";
            this.chkAppSettings.UseVisualStyleBackColor = true;
            // 
            // chkTradeImportRules
            // 
            this.chkTradeImportRules.AutoSize = true;
            this.chkTradeImportRules.Location = new System.Drawing.Point(229, 65);
            this.chkTradeImportRules.Name = "chkTradeImportRules";
            this.chkTradeImportRules.Size = new System.Drawing.Size(110, 17);
            this.chkTradeImportRules.TabIndex = 7;
            this.chkTradeImportRules.Text = "Trade import rules";
            this.chkTradeImportRules.UseVisualStyleBackColor = true;
            // 
            // chkFileImportRules
            // 
            this.chkFileImportRules.AutoSize = true;
            this.chkFileImportRules.Location = new System.Drawing.Point(229, 42);
            this.chkFileImportRules.Name = "chkFileImportRules";
            this.chkFileImportRules.Size = new System.Drawing.Size(98, 17);
            this.chkFileImportRules.TabIndex = 6;
            this.chkFileImportRules.Text = "File import rules";
            this.chkFileImportRules.UseVisualStyleBackColor = true;
            // 
            // chkAlertMonitorRules
            // 
            this.chkAlertMonitorRules.AutoSize = true;
            this.chkAlertMonitorRules.Location = new System.Drawing.Point(12, 65);
            this.chkAlertMonitorRules.Name = "chkAlertMonitorRules";
            this.chkAlertMonitorRules.Size = new System.Drawing.Size(123, 17);
            this.chkAlertMonitorRules.TabIndex = 4;
            this.chkAlertMonitorRules.Text = "Alert monitoring rules";
            this.chkAlertMonitorRules.UseVisualStyleBackColor = true;
            // 
            // chkPricingStructureDefs
            // 
            this.chkPricingStructureDefs.AutoSize = true;
            this.chkPricingStructureDefs.Location = new System.Drawing.Point(12, 42);
            this.chkPricingStructureDefs.Name = "chkPricingStructureDefs";
            this.chkPricingStructureDefs.Size = new System.Drawing.Size(199, 17);
            this.chkPricingStructureDefs.TabIndex = 3;
            this.chkPricingStructureDefs.Text = "QR_LIVE pricing structure definitions";
            this.chkPricingStructureDefs.UseVisualStyleBackColor = true;
            // 
            // chkMDSProviderMaps
            // 
            this.chkMDSProviderMaps.AutoSize = true;
            this.chkMDSProviderMaps.Location = new System.Drawing.Point(12, 19);
            this.chkMDSProviderMaps.Name = "chkMDSProviderMaps";
            this.chkMDSProviderMaps.Size = new System.Drawing.Size(189, 17);
            this.chkMDSProviderMaps.TabIndex = 2;
            this.chkMDSProviderMaps.Text = "Market data service provider maps";
            this.chkMDSProviderMaps.UseVisualStyleBackColor = true;
            // 
            // btnLoadSelected
            // 
            this.btnLoadSelected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadSelected.Location = new System.Drawing.Point(658, 89);
            this.btnLoadSelected.Name = "btnLoadSelected";
            this.btnLoadSelected.Size = new System.Drawing.Size(76, 53);
            this.btnLoadSelected.TabIndex = 1;
            this.btnLoadSelected.Text = "LOAD!";
            this.btnLoadSelected.UseVisualStyleBackColor = true;
            this.btnLoadSelected.Click += new System.EventHandler(this.BtnLoadSelectedClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtLog);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 148);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(746, 392);
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
            this.txtLog.Size = new System.Drawing.Size(740, 373);
            this.txtLog.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 540);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Load Config";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1FormClosing);
            this.Load += new System.EventHandler(this.Form1Load);
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkMDSProviderMaps;
        private System.Windows.Forms.Button btnLoadSelected;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.CheckBox chkAlertMonitorRules;
        private System.Windows.Forms.CheckBox chkPricingStructureDefs;
        private System.Windows.Forms.CheckBox chkTradeImportRules;
        private System.Windows.Forms.CheckBox chkFileImportRules;
        private System.Windows.Forms.CheckBox chkAppSettings;
        private System.Windows.Forms.CheckBox chkInstrumentsConfig;
        private System.Windows.Forms.CheckBox chkAlgorithmConfig;
        private System.Windows.Forms.CheckBox chkStressRules;
        private System.Windows.Forms.CheckBox chkDateRules;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnUnselectAll;
        private System.Windows.Forms.CheckBox chkDeleteAllAppSettings;
        private System.Windows.Forms.CheckBox chkDeleteAllStatus;
        private System.Windows.Forms.CheckBox chkDeleteAllConfig;
        private System.Windows.Forms.CheckBox bondDataCheckBox;
        private System.Windows.Forms.ComboBox cBoxNameSpaces;
        private System.Windows.Forms.CheckBox checkBoxMarkets;
        private System.Windows.Forms.CheckBox checkBoxFpML;
        private System.Windows.Forms.CheckBox checkBoxHolidayDates;
    }
}

