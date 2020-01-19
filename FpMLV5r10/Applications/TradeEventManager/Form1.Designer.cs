namespace Orion.TradeEventManger
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dtpMarketEndDate = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.cbClientNameSpace = new System.Windows.Forms.ComboBox();
            this.comboBoxBaseParty = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkRunAtServer = new System.Windows.Forms.CheckBox();
            this.lblDateRange = new System.Windows.Forms.Label();
            this.dtpBaseDate = new System.Windows.Forms.DateTimePicker();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpTradeAll = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lvTradeAll = new System.Windows.Forms.ListView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnTradeAddAll = new System.Windows.Forms.Button();
            this.btnTradeAddSelected = new System.Windows.Forms.Button();
            this.pnlTradeAll = new System.Windows.Forms.Panel();
            this.tpTradeSel = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lvTradeSel = new System.Windows.Forms.ListView();
            this.panel5 = new System.Windows.Forms.Panel();
            this.btnTradeClearAll = new System.Windows.Forms.Button();
            this.btnTradeRemoveSelected = new System.Windows.Forms.Button();
            this.pnlTradeSel = new System.Windows.Forms.Panel();
            this.tpValueRaw = new System.Windows.Forms.TabPage();
            this.lvValueRaw = new System.Windows.Forms.ListView();
            this.panelValueRaw = new System.Windows.Forms.Panel();
            this.tpPortfolios = new System.Windows.Forms.TabPage();
            this.lvPortfolio = new System.Windows.Forms.ListView();
            this.panelPortfolio = new System.Windows.Forms.Panel();
            this.tpRequests = new System.Windows.Forms.TabPage();
            this.lvProgress = new System.Windows.Forms.ListView();
            this.panelProgress = new System.Windows.Forms.Panel();
            this.tpDebugLog = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpTradeAll.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tpTradeSel.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tpValueRaw.SuspendLayout();
            this.tpPortfolios.SuspendLayout();
            this.tpRequests.SuspendLayout();
            this.tpDebugLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1047, 66);
            this.panel1.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.dtpMarketEndDate);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cbClientNameSpace);
            this.groupBox1.Controls.Add(this.comboBoxBaseParty);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.chkRunAtServer);
            this.groupBox1.Controls.Add(this.lblDateRange);
            this.groupBox1.Controls.Add(this.dtpBaseDate);
            this.groupBox1.Location = new System.Drawing.Point(10, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1027, 57);
            this.groupBox1.TabIndex = 45;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Trade Valuation";
            // 
            // dtpMarketEndDate
            // 
            this.dtpMarketEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpMarketEndDate.Location = new System.Drawing.Point(216, 28);
            this.dtpMarketEndDate.Name = "dtpMarketEndDate";
            this.dtpMarketEndDate.Size = new System.Drawing.Size(78, 20);
            this.dtpMarketEndDate.TabIndex = 61;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(313, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 57;
            this.label5.Text = "Base Party:";
            // 
            // cbClientNameSpace
            // 
            this.cbClientNameSpace.FormattingEnabled = true;
            this.cbClientNameSpace.Items.AddRange(new object[] {
            "Orion.V5r3",
            "Orion",
            "FpML47"});
            this.cbClientNameSpace.Location = new System.Drawing.Point(570, 28);
            this.cbClientNameSpace.Name = "cbClientNameSpace";
            this.cbClientNameSpace.Size = new System.Drawing.Size(90, 21);
            this.cbClientNameSpace.TabIndex = 56;
            this.cbClientNameSpace.Text = "Orion.V5r3";
            // 
            // comboBoxBaseParty
            // 
            this.comboBoxBaseParty.FormattingEnabled = true;
            this.comboBoxBaseParty.Items.AddRange(new object[] {
            "CBA",
            "Party1",
            "Party2"});
            this.comboBoxBaseParty.Location = new System.Drawing.Point(380, 27);
            this.comboBoxBaseParty.Name = "comboBoxBaseParty";
            this.comboBoxBaseParty.Size = new System.Drawing.Size(64, 21);
            this.comboBoxBaseParty.TabIndex = 55;
            this.comboBoxBaseParty.Text = "Party1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(467, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 13);
            this.label4.TabIndex = 50;
            this.label4.Text = "Client Name Space:";
            // 
            // chkRunAtServer
            // 
            this.chkRunAtServer.AutoSize = true;
            this.chkRunAtServer.Location = new System.Drawing.Point(705, 28);
            this.chkRunAtServer.Name = "chkRunAtServer";
            this.chkRunAtServer.Size = new System.Drawing.Size(171, 17);
            this.chkRunAtServer.TabIndex = 49;
            this.chkRunAtServer.Text = "Process request on server/grid";
            this.chkRunAtServer.UseVisualStyleBackColor = true;
            // 
            // lblDateRange
            // 
            this.lblDateRange.AutoSize = true;
            this.lblDateRange.Location = new System.Drawing.Point(21, 32);
            this.lblDateRange.Name = "lblDateRange";
            this.lblDateRange.Size = new System.Drawing.Size(68, 13);
            this.lblDateRange.TabIndex = 38;
            this.lblDateRange.Text = "Date Range:";
            // 
            // dtpBaseDate
            // 
            this.dtpBaseDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpBaseDate.Location = new System.Drawing.Point(124, 28);
            this.dtpBaseDate.Name = "dtpBaseDate";
            this.dtpBaseDate.Size = new System.Drawing.Size(82, 20);
            this.dtpBaseDate.TabIndex = 39;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpTradeAll);
            this.tabControl1.Controls.Add(this.tpTradeSel);
            this.tabControl1.Controls.Add(this.tpValueRaw);
            this.tabControl1.Controls.Add(this.tpPortfolios);
            this.tabControl1.Controls.Add(this.tpRequests);
            this.tabControl1.Controls.Add(this.tpDebugLog);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 66);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1047, 729);
            this.tabControl1.TabIndex = 2;
            // 
            // tpTradeAll
            // 
            this.tpTradeAll.Controls.Add(this.groupBox4);
            this.tpTradeAll.Location = new System.Drawing.Point(4, 22);
            this.tpTradeAll.Name = "tpTradeAll";
            this.tpTradeAll.Padding = new System.Windows.Forms.Padding(3);
            this.tpTradeAll.Size = new System.Drawing.Size(1039, 703);
            this.tpTradeAll.TabIndex = 5;
            this.tpTradeAll.Text = "Available Trades";
            this.tpTradeAll.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lvTradeAll);
            this.groupBox4.Controls.Add(this.panel4);
            this.groupBox4.Controls.Add(this.pnlTradeAll);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1033, 697);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Available Trades";
            // 
            // lvTradeAll
            // 
            this.lvTradeAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvTradeAll.FullRowSelect = true;
            this.lvTradeAll.Location = new System.Drawing.Point(3, 48);
            this.lvTradeAll.Name = "lvTradeAll";
            this.lvTradeAll.Size = new System.Drawing.Size(1027, 614);
            this.lvTradeAll.TabIndex = 10;
            this.lvTradeAll.UseCompatibleStateImageBehavior = false;
            this.lvTradeAll.DoubleClick += new System.EventHandler(this.LvTradeAllDoubleClick1);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnTradeAddAll);
            this.panel4.Controls.Add(this.btnTradeAddSelected);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(3, 662);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1027, 32);
            this.panel4.TabIndex = 9;
            // 
            // btnTradeAddAll
            // 
            this.btnTradeAddAll.Location = new System.Drawing.Point(3, 6);
            this.btnTradeAddAll.Name = "btnTradeAddAll";
            this.btnTradeAddAll.Size = new System.Drawing.Size(170, 23);
            this.btnTradeAddAll.TabIndex = 1;
            this.btnTradeAddAll.Text = "Add all to portfolio";
            this.btnTradeAddAll.UseVisualStyleBackColor = true;
            this.btnTradeAddAll.Click += new System.EventHandler(this.BtnTradeAddAllClick1);
            // 
            // btnTradeAddSelected
            // 
            this.btnTradeAddSelected.Location = new System.Drawing.Point(179, 6);
            this.btnTradeAddSelected.Name = "btnTradeAddSelected";
            this.btnTradeAddSelected.Size = new System.Drawing.Size(170, 23);
            this.btnTradeAddSelected.TabIndex = 0;
            this.btnTradeAddSelected.Text = "Add selected to portfolio";
            this.btnTradeAddSelected.UseVisualStyleBackColor = true;
            this.btnTradeAddSelected.Click += new System.EventHandler(this.BtnTradeAddSelectedClick1);
            // 
            // pnlTradeAll
            // 
            this.pnlTradeAll.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTradeAll.Location = new System.Drawing.Point(3, 16);
            this.pnlTradeAll.Name = "pnlTradeAll";
            this.pnlTradeAll.Size = new System.Drawing.Size(1027, 32);
            this.pnlTradeAll.TabIndex = 6;
            // 
            // tpTradeSel
            // 
            this.tpTradeSel.Controls.Add(this.groupBox5);
            this.tpTradeSel.Location = new System.Drawing.Point(4, 22);
            this.tpTradeSel.Name = "tpTradeSel";
            this.tpTradeSel.Padding = new System.Windows.Forms.Padding(3);
            this.tpTradeSel.Size = new System.Drawing.Size(1039, 633);
            this.tpTradeSel.TabIndex = 6;
            this.tpTradeSel.Text = "Portfolio trades";
            this.tpTradeSel.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.lvTradeSel);
            this.groupBox5.Controls.Add(this.panel5);
            this.groupBox5.Controls.Add(this.pnlTradeSel);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(3, 3);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(1033, 627);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Selected Trades";
            // 
            // lvTradeSel
            // 
            this.lvTradeSel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvTradeSel.FullRowSelect = true;
            this.lvTradeSel.Location = new System.Drawing.Point(3, 48);
            this.lvTradeSel.Name = "lvTradeSel";
            this.lvTradeSel.Size = new System.Drawing.Size(1027, 544);
            this.lvTradeSel.TabIndex = 10;
            this.lvTradeSel.UseCompatibleStateImageBehavior = false;
            this.lvTradeSel.DoubleClick += new System.EventHandler(this.LvTradeSelDoubleClick);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.btnTradeClearAll);
            this.panel5.Controls.Add(this.btnTradeRemoveSelected);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(3, 592);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1027, 32);
            this.panel5.TabIndex = 9;
            // 
            // btnTradeClearAll
            // 
            this.btnTradeClearAll.Location = new System.Drawing.Point(179, 6);
            this.btnTradeClearAll.Name = "btnTradeClearAll";
            this.btnTradeClearAll.Size = new System.Drawing.Size(170, 23);
            this.btnTradeClearAll.TabIndex = 2;
            this.btnTradeClearAll.Text = "Remove all trades";
            this.btnTradeClearAll.UseVisualStyleBackColor = true;
            this.btnTradeClearAll.Click += new System.EventHandler(this.BtnTradeClearAllClick1);
            // 
            // btnTradeRemoveSelected
            // 
            this.btnTradeRemoveSelected.Location = new System.Drawing.Point(3, 6);
            this.btnTradeRemoveSelected.Name = "btnTradeRemoveSelected";
            this.btnTradeRemoveSelected.Size = new System.Drawing.Size(170, 23);
            this.btnTradeRemoveSelected.TabIndex = 1;
            this.btnTradeRemoveSelected.Text = "Remove selected trades";
            this.btnTradeRemoveSelected.UseVisualStyleBackColor = true;
            this.btnTradeRemoveSelected.Click += new System.EventHandler(this.BtnTradeRemoveSelectedClick1);
            // 
            // pnlTradeSel
            // 
            this.pnlTradeSel.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTradeSel.Location = new System.Drawing.Point(3, 16);
            this.pnlTradeSel.Name = "pnlTradeSel";
            this.pnlTradeSel.Size = new System.Drawing.Size(1027, 32);
            this.pnlTradeSel.TabIndex = 6;
            // 
            // tpValueRaw
            // 
            this.tpValueRaw.Controls.Add(this.lvValueRaw);
            this.tpValueRaw.Controls.Add(this.panelValueRaw);
            this.tpValueRaw.Location = new System.Drawing.Point(4, 22);
            this.tpValueRaw.Name = "tpValueRaw";
            this.tpValueRaw.Padding = new System.Windows.Forms.Padding(3);
            this.tpValueRaw.Size = new System.Drawing.Size(1039, 633);
            this.tpValueRaw.TabIndex = 4;
            this.tpValueRaw.Text = "Valuations";
            this.tpValueRaw.UseVisualStyleBackColor = true;
            // 
            // lvValueRaw
            // 
            this.lvValueRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvValueRaw.Location = new System.Drawing.Point(3, 33);
            this.lvValueRaw.Name = "lvValueRaw";
            this.lvValueRaw.Size = new System.Drawing.Size(1033, 597);
            this.lvValueRaw.TabIndex = 9;
            this.lvValueRaw.UseCompatibleStateImageBehavior = false;
            // 
            // panelValueRaw
            // 
            this.panelValueRaw.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelValueRaw.Location = new System.Drawing.Point(3, 3);
            this.panelValueRaw.Name = "panelValueRaw";
            this.panelValueRaw.Size = new System.Drawing.Size(1033, 30);
            this.panelValueRaw.TabIndex = 8;
            // 
            // tpPortfolios
            // 
            this.tpPortfolios.Controls.Add(this.lvPortfolio);
            this.tpPortfolios.Controls.Add(this.panelPortfolio);
            this.tpPortfolios.Location = new System.Drawing.Point(4, 22);
            this.tpPortfolios.Name = "tpPortfolios";
            this.tpPortfolios.Padding = new System.Windows.Forms.Padding(3);
            this.tpPortfolios.Size = new System.Drawing.Size(1039, 633);
            this.tpPortfolios.TabIndex = 9;
            this.tpPortfolios.Text = "Portfolios";
            this.tpPortfolios.UseVisualStyleBackColor = true;
            // 
            // lvPortfolio
            // 
            this.lvPortfolio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPortfolio.Location = new System.Drawing.Point(3, 33);
            this.lvPortfolio.Name = "lvPortfolio";
            this.lvPortfolio.Size = new System.Drawing.Size(1033, 597);
            this.lvPortfolio.TabIndex = 15;
            this.lvPortfolio.UseCompatibleStateImageBehavior = false;
            // 
            // panelPortfolio
            // 
            this.panelPortfolio.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelPortfolio.Location = new System.Drawing.Point(3, 3);
            this.panelPortfolio.Name = "panelPortfolio";
            this.panelPortfolio.Size = new System.Drawing.Size(1033, 30);
            this.panelPortfolio.TabIndex = 14;
            // 
            // tpRequests
            // 
            this.tpRequests.Controls.Add(this.lvProgress);
            this.tpRequests.Controls.Add(this.panelProgress);
            this.tpRequests.Location = new System.Drawing.Point(4, 22);
            this.tpRequests.Name = "tpRequests";
            this.tpRequests.Padding = new System.Windows.Forms.Padding(3);
            this.tpRequests.Size = new System.Drawing.Size(1039, 633);
            this.tpRequests.TabIndex = 8;
            this.tpRequests.Text = "Requests";
            this.tpRequests.UseVisualStyleBackColor = true;
            // 
            // lvProgress
            // 
            this.lvProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvProgress.Location = new System.Drawing.Point(3, 33);
            this.lvProgress.Name = "lvProgress";
            this.lvProgress.Size = new System.Drawing.Size(1033, 597);
            this.lvProgress.TabIndex = 13;
            this.lvProgress.UseCompatibleStateImageBehavior = false;
            // 
            // panelProgress
            // 
            this.panelProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProgress.Location = new System.Drawing.Point(3, 3);
            this.panelProgress.Name = "panelProgress";
            this.panelProgress.Size = new System.Drawing.Size(1033, 30);
            this.panelProgress.TabIndex = 12;
            // 
            // tpDebugLog
            // 
            this.tpDebugLog.Controls.Add(this.txtLog);
            this.tpDebugLog.Location = new System.Drawing.Point(4, 22);
            this.tpDebugLog.Name = "tpDebugLog";
            this.tpDebugLog.Padding = new System.Windows.Forms.Padding(3);
            this.tpDebugLog.Size = new System.Drawing.Size(1039, 633);
            this.tpDebugLog.TabIndex = 2;
            this.tpDebugLog.Text = "Log";
            this.tpDebugLog.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(3, 3);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(1033, 627);
            this.txtLog.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1047, 795);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Trade Event Manager Test Harness";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1FormClosing);
            this.Load += new System.EventHandler(this.Form1Load);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tpTradeAll.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.tpTradeSel.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.tpValueRaw.ResumeLayout(false);
            this.tpPortfolios.ResumeLayout(false);
            this.tpRequests.ResumeLayout(false);
            this.tpDebugLog.ResumeLayout(false);
            this.tpDebugLog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DateTimePicker dtpBaseDate;
        private System.Windows.Forms.Label lblDateRange;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpValueRaw;
        private System.Windows.Forms.ListView lvValueRaw;
        private System.Windows.Forms.Panel panelValueRaw;
        private System.Windows.Forms.TabPage tpDebugLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TabPage tpTradeAll;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListView lvTradeAll;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnTradeAddAll;
        private System.Windows.Forms.Button btnTradeAddSelected;
        private System.Windows.Forms.Panel pnlTradeAll;
        private System.Windows.Forms.TabPage tpTradeSel;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ListView lvTradeSel;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button btnTradeClearAll;
        private System.Windows.Forms.Button btnTradeRemoveSelected;
        private System.Windows.Forms.Panel pnlTradeSel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkRunAtServer;
        private System.Windows.Forms.TabPage tpRequests;
        private System.Windows.Forms.ListView lvProgress;
        private System.Windows.Forms.Panel panelProgress;
        private System.Windows.Forms.TabPage tpPortfolios;
        private System.Windows.Forms.ListView lvPortfolio;
        private System.Windows.Forms.Panel panelPortfolio;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxBaseParty;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbClientNameSpace;
        private System.Windows.Forms.DateTimePicker dtpMarketEndDate;
    }
}

