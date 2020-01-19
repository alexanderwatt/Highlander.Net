namespace FpML.V5r3.AutoRatePublisher
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.trackBarFrequency = new System.Windows.Forms.TrackBar();
            this.lstBoxNameSpace = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSelectCurve = new System.Windows.Forms.Button();
            this.lstBoxCurveName = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonStopPublish = new System.Windows.Forms.Button();
            this.btnAutoCurvePublisher = new System.Windows.Forms.Button();
            this.txtChangePort = new System.Windows.Forms.TextBox();
            this.chkChangePort = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panelQASDef = new System.Windows.Forms.Panel();
            this.lvQASDef = new System.Windows.Forms.ListView();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFrequency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.trackBarFrequency);
            this.groupBox1.Controls.Add(this.lstBoxNameSpace);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnSelectCurve);
            this.groupBox1.Controls.Add(this.lstBoxCurveName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.buttonStopPublish);
            this.groupBox1.Controls.Add(this.btnAutoCurvePublisher);
            this.groupBox1.Controls.Add(this.txtChangePort);
            this.groupBox1.Controls.Add(this.chkChangePort);
            this.groupBox1.Controls.Add(this.splitContainer1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1206, 731);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Log";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(930, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(214, 13);
            this.label3.TabIndex = 54;
            this.label3.Text = "Publisher Frequency:  Highest of 1 minutes";
            // 
            // trackBarFrequency
            // 
            this.trackBarFrequency.Location = new System.Drawing.Point(881, 16);
            this.trackBarFrequency.Maximum = 60;
            this.trackBarFrequency.Name = "trackBarFrequency";
            this.trackBarFrequency.Size = new System.Drawing.Size(310, 45);
            this.trackBarFrequency.TabIndex = 53;
            // 
            // lstBoxNameSpace
            // 
            this.lstBoxNameSpace.FormattingEnabled = true;
            this.lstBoxNameSpace.Items.AddRange(new object[] {
            "Orion.V5r3"});
            this.lstBoxNameSpace.Location = new System.Drawing.Point(103, 48);
            this.lstBoxNameSpace.Name = "lstBoxNameSpace";
            this.lstBoxNameSpace.Size = new System.Drawing.Size(112, 17);
            this.lstBoxNameSpace.TabIndex = 52;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 51;
            this.label2.Text = "NameSpace:";
            // 
            // btnSelectCurve
            // 
            this.btnSelectCurve.Location = new System.Drawing.Point(307, 48);
            this.btnSelectCurve.Name = "btnSelectCurve";
            this.btnSelectCurve.Size = new System.Drawing.Size(112, 27);
            this.btnSelectCurve.TabIndex = 50;
            this.btnSelectCurve.Text = "Select Curve";
            this.btnSelectCurve.UseVisualStyleBackColor = true;
            this.btnSelectCurve.Click += new System.EventHandler(this.BtnSelectCurveClick);
            // 
            // lstBoxCurveName
            // 
            this.lstBoxCurveName.FormattingEnabled = true;
            this.lstBoxCurveName.Items.AddRange(new object[] {
            "QR_LIVE.DiscountCurve.AUD-LIBOR-SENIOR",
            "QR_LIVE.RateBasisCurve.AUD-BBR-BBSW-1M",
            "QR_LIVE.RateCurve.AUD-BBR-BBSW-3M",
            "QR_LIVE.RateBasisCurve.AUD-BBR-BBSW-6M"});
            this.lstBoxCurveName.Location = new System.Drawing.Point(425, 15);
            this.lstBoxCurveName.Name = "lstBoxCurveName";
            this.lstBoxCurveName.Size = new System.Drawing.Size(312, 82);
            this.lstBoxCurveName.TabIndex = 49;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(349, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 48;
            this.label1.Text = "CurveName:";
            // 
            // buttonStopPublish
            // 
            this.buttonStopPublish.Location = new System.Drawing.Point(749, 53);
            this.buttonStopPublish.Name = "buttonStopPublish";
            this.buttonStopPublish.Size = new System.Drawing.Size(125, 27);
            this.buttonStopPublish.TabIndex = 47;
            this.buttonStopPublish.Text = "Stop RateServer";
            this.buttonStopPublish.UseVisualStyleBackColor = true;
            this.buttonStopPublish.Click += new System.EventHandler(this.ButtonStopPublishClick);
            // 
            // btnAutoCurvePublisher
            // 
            this.btnAutoCurvePublisher.Location = new System.Drawing.Point(750, 19);
            this.btnAutoCurvePublisher.Name = "btnAutoCurvePublisher";
            this.btnAutoCurvePublisher.Size = new System.Drawing.Size(125, 27);
            this.btnAutoCurvePublisher.TabIndex = 46;
            this.btnAutoCurvePublisher.Text = "RateServer";
            this.btnAutoCurvePublisher.UseVisualStyleBackColor = true;
            this.btnAutoCurvePublisher.Click += new System.EventHandler(this.BtnAutoCurvePublisherClick);
            // 
            // txtChangePort
            // 
            this.txtChangePort.Location = new System.Drawing.Point(257, 19);
            this.txtChangePort.Name = "txtChangePort";
            this.txtChangePort.Size = new System.Drawing.Size(43, 20);
            this.txtChangePort.TabIndex = 45;
            this.txtChangePort.Text = "9999";
            // 
            // chkChangePort
            // 
            this.chkChangePort.AutoSize = true;
            this.chkChangePort.Location = new System.Drawing.Point(35, 19);
            this.chkChangePort.Name = "chkChangePort";
            this.chkChangePort.Size = new System.Drawing.Size(222, 17);
            this.chkChangePort.TabIndex = 44;
            this.chkChangePort.Text = "Change server port from default (nnnn) to:";
            this.chkChangePort.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(3, 103);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtLog);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelQASDef);
            this.splitContainer1.Panel2.Controls.Add(this.lvQASDef);
            this.splitContainer1.Size = new System.Drawing.Size(1191, 622);
            this.splitContainer1.SplitterDistance = 396;
            this.splitContainer1.TabIndex = 2;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(396, 622);
            this.txtLog.TabIndex = 1;
            // 
            // panelQASDef
            // 
            this.panelQASDef.Location = new System.Drawing.Point(0, 0);
            this.panelQASDef.Name = "panelQASDef";
            this.panelQASDef.Size = new System.Drawing.Size(788, 32);
            this.panelQASDef.TabIndex = 9;
            // 
            // lvQASDef
            // 
            this.lvQASDef.Location = new System.Drawing.Point(0, 38);
            this.lvQASDef.Name = "lvQASDef";
            this.lvQASDef.Size = new System.Drawing.Size(788, 578);
            this.lvQASDef.TabIndex = 0;
            this.lvQASDef.UseCompatibleStateImageBehavior = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1206, 731);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Highlander Live Rates Publisher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1FormClosing);
            this.Load += new System.EventHandler(this.Form1Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFrequency)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lstBoxNameSpace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSelectCurve;
        private System.Windows.Forms.ListBox lstBoxCurveName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonStopPublish;
        private System.Windows.Forms.Button btnAutoCurvePublisher;
        private System.Windows.Forms.TextBox txtChangePort;
        private System.Windows.Forms.CheckBox chkChangePort;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBarFrequency;
        private System.Windows.Forms.ListView lvQASDef;
        private System.Windows.Forms.Panel panelQASDef;
    }
}

