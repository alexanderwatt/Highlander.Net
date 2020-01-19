namespace RequestTestHarness
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
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.btnCurveGenRequest = new System.Windows.Forms.Button();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.chkAllFXStresses = new System.Windows.Forms.CheckBox();
            this.chkAllIRStresses = new System.Windows.Forms.CheckBox();
            this.cbCounterParty = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbMarketName = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnPVRWoolworths = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.nudPingDelay = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.chkPingFault = new System.Windows.Forms.CheckBox();
            this.btnPingHandler = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnExecute = new System.Windows.Forms.Button();
            this.txtWorkerInstance = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRequestId = new System.Windows.Forms.TextBox();
            this.txtWorkerComputer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rbExecSendToManager = new System.Windows.Forms.RadioButton();
            this.rbExecAssignToWorker = new System.Windows.Forms.RadioButton();
            this.rbExecInprocess = new System.Windows.Forms.RadioButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpLog = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.tpRequests = new System.Windows.Forms.TabPage();
            this.lvProgress = new System.Windows.Forms.ListView();
            this.panelProgress = new System.Windows.Forms.Panel();
            this.tpWorker = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtWorkerALog = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnStopWorkerA = new System.Windows.Forms.Button();
            this.btnStartWorkerA = new System.Windows.Forms.Button();
            this.tpWorkerB = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtWorkerBLog = new System.Windows.Forms.TextBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnStopWorkerB = new System.Windows.Forms.Button();
            this.btnStartWorkerB = new System.Windows.Forms.Button();
            this.tpManager = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtManagerLog = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnStopManager = new System.Windows.Forms.Button();
            this.btnStartManager = new System.Windows.Forms.Button();
            this.tpAvailability = new System.Windows.Forms.TabPage();
            this.lvAvailability = new System.Windows.Forms.ListView();
            this.panelAvailability = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPingDelay)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpLog.SuspendLayout();
            this.tpRequests.SuspendLayout();
            this.tpWorker.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tpWorkerB.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tpManager.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tpAvailability.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox7);
            this.panel1.Controls.Add(this.groupBox6);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1177, 183);
            this.panel1.TabIndex = 0;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.btnCurveGenRequest);
            this.groupBox7.Location = new System.Drawing.Point(352, 12);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(265, 165);
            this.groupBox7.TabIndex = 6;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Curve Generation";
            // 
            // btnCurveGenRequest
            // 
            this.btnCurveGenRequest.Location = new System.Drawing.Point(6, 120);
            this.btnCurveGenRequest.Name = "btnCurveGenRequest";
            this.btnCurveGenRequest.Size = new System.Drawing.Size(154, 36);
            this.btnCurveGenRequest.TabIndex = 5;
            this.btnCurveGenRequest.Text = "Create Request";
            this.btnCurveGenRequest.UseVisualStyleBackColor = true;
            this.btnCurveGenRequest.Click += new System.EventHandler(this.btnCurveGenRequest_Click);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.chkAllFXStresses);
            this.groupBox6.Controls.Add(this.chkAllIRStresses);
            this.groupBox6.Controls.Add(this.cbCounterParty);
            this.groupBox6.Controls.Add(this.label7);
            this.groupBox6.Controls.Add(this.cbMarketName);
            this.groupBox6.Controls.Add(this.label6);
            this.groupBox6.Controls.Add(this.btnPVRWoolworths);
            this.groupBox6.Location = new System.Drawing.Point(173, 12);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(173, 165);
            this.groupBox6.TabIndex = 5;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Portfolio Valuation";
            // 
            // chkAllFXStresses
            // 
            this.chkAllFXStresses.AutoSize = true;
            this.chkAllFXStresses.Location = new System.Drawing.Point(9, 96);
            this.chkAllFXStresses.Name = "chkAllFXStresses";
            this.chkAllFXStresses.Size = new System.Drawing.Size(94, 17);
            this.chkAllFXStresses.TabIndex = 10;
            this.chkAllFXStresses.Text = "All FX stresses";
            this.chkAllFXStresses.UseVisualStyleBackColor = true;
            // 
            // chkAllIRStresses
            // 
            this.chkAllIRStresses.AutoSize = true;
            this.chkAllIRStresses.Location = new System.Drawing.Point(9, 73);
            this.chkAllIRStresses.Name = "chkAllIRStresses";
            this.chkAllIRStresses.Size = new System.Drawing.Size(92, 17);
            this.chkAllIRStresses.TabIndex = 9;
            this.chkAllIRStresses.Text = "All IR stresses";
            this.chkAllIRStresses.UseVisualStyleBackColor = true;
            // 
            // cbCounterParty
            // 
            this.cbCounterParty.FormattingEnabled = true;
            this.cbCounterParty.Location = new System.Drawing.Point(55, 46);
            this.cbCounterParty.Name = "cbCounterParty";
            this.cbCounterParty.Size = new System.Drawing.Size(105, 21);
            this.cbCounterParty.TabIndex = 8;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 49);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "CPty:";
            // 
            // cbMarketName
            // 
            this.cbMarketName.FormattingEnabled = true;
            this.cbMarketName.Location = new System.Drawing.Point(55, 19);
            this.cbMarketName.Name = "cbMarketName";
            this.cbMarketName.Size = new System.Drawing.Size(105, 21);
            this.cbMarketName.TabIndex = 6;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Market:";
            // 
            // btnPVRWoolworths
            // 
            this.btnPVRWoolworths.Location = new System.Drawing.Point(9, 120);
            this.btnPVRWoolworths.Name = "btnPVRWoolworths";
            this.btnPVRWoolworths.Size = new System.Drawing.Size(154, 36);
            this.btnPVRWoolworths.TabIndex = 4;
            this.btnPVRWoolworths.Text = "Create Request";
            this.btnPVRWoolworths.UseVisualStyleBackColor = true;
            this.btnPVRWoolworths.Click += new System.EventHandler(this.btnPVRWoolworths_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.nudPingDelay);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.chkPingFault);
            this.groupBox3.Controls.Add(this.btnPingHandler);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(155, 165);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Test Request";
            // 
            // nudPingDelay
            // 
            this.nudPingDelay.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudPingDelay.Location = new System.Drawing.Point(95, 42);
            this.nudPingDelay.Name = "nudPingDelay";
            this.nudPingDelay.Size = new System.Drawing.Size(42, 20);
            this.nudPingDelay.TabIndex = 8;
            this.nudPingDelay.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Delay (seconds):";
            // 
            // chkPingFault
            // 
            this.chkPingFault.AutoSize = true;
            this.chkPingFault.Location = new System.Drawing.Point(6, 20);
            this.chkPingFault.Name = "chkPingFault";
            this.chkPingFault.Size = new System.Drawing.Size(105, 17);
            this.chkPingFault.TabIndex = 6;
            this.chkPingFault.Text = "Throw exception";
            this.chkPingFault.UseVisualStyleBackColor = true;
            // 
            // btnPingHandler
            // 
            this.btnPingHandler.Location = new System.Drawing.Point(6, 120);
            this.btnPingHandler.Name = "btnPingHandler";
            this.btnPingHandler.Size = new System.Drawing.Size(134, 36);
            this.btnPingHandler.TabIndex = 3;
            this.btnPingHandler.Text = "Create Request";
            this.btnPingHandler.UseVisualStyleBackColor = true;
            this.btnPingHandler.Click += new System.EventHandler(this.btnGenPingRequest_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnExecute);
            this.groupBox1.Controls.Add(this.txtWorkerInstance);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtRequestId);
            this.groupBox1.Controls.Add(this.txtWorkerComputer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.rbExecSendToManager);
            this.groupBox1.Controls.Add(this.rbExecAssignToWorker);
            this.groupBox1.Controls.Add(this.rbExecInprocess);
            this.groupBox1.Location = new System.Drawing.Point(755, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 165);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Execution Options";
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(300, 72);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(98, 36);
            this.btnExecute.TabIndex = 7;
            this.btnExecute.Text = "Execute!";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // txtWorkerInstance
            // 
            this.txtWorkerInstance.Location = new System.Drawing.Point(357, 41);
            this.txtWorkerInstance.Name = "txtWorkerInstance";
            this.txtWorkerInstance.Size = new System.Drawing.Size(41, 20);
            this.txtWorkerInstance.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(300, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Instance:";
            // 
            // txtRequestId
            // 
            this.txtRequestId.Location = new System.Drawing.Point(74, 88);
            this.txtRequestId.Name = "txtRequestId";
            this.txtRequestId.Size = new System.Drawing.Size(220, 20);
            this.txtRequestId.TabIndex = 1;
            // 
            // txtWorkerComputer
            // 
            this.txtWorkerComputer.Location = new System.Drawing.Point(204, 41);
            this.txtWorkerComputer.Name = "txtWorkerComputer";
            this.txtWorkerComputer.Size = new System.Drawing.Size(90, 20);
            this.txtWorkerComputer.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Request Id:";
            // 
            // rbExecSendToManager
            // 
            this.rbExecSendToManager.AutoSize = true;
            this.rbExecSendToManager.Location = new System.Drawing.Point(6, 65);
            this.rbExecSendToManager.Name = "rbExecSendToManager";
            this.rbExecSendToManager.Size = new System.Drawing.Size(164, 17);
            this.rbExecSendToManager.TabIndex = 3;
            this.rbExecSendToManager.Text = "Send request to grid manager";
            this.rbExecSendToManager.UseVisualStyleBackColor = true;
            // 
            // rbExecAssignToWorker
            // 
            this.rbExecAssignToWorker.AutoSize = true;
            this.rbExecAssignToWorker.Location = new System.Drawing.Point(6, 42);
            this.rbExecAssignToWorker.Name = "rbExecAssignToWorker";
            this.rbExecAssignToWorker.Size = new System.Drawing.Size(192, 17);
            this.rbExecAssignToWorker.TabIndex = 2;
            this.rbExecAssignToWorker.Text = "Assign request to grid worker. Host:";
            this.rbExecAssignToWorker.UseVisualStyleBackColor = true;
            // 
            // rbExecInprocess
            // 
            this.rbExecInprocess.AutoSize = true;
            this.rbExecInprocess.Checked = true;
            this.rbExecInprocess.Location = new System.Drawing.Point(6, 19);
            this.rbExecInprocess.Name = "rbExecInprocess";
            this.rbExecInprocess.Size = new System.Drawing.Size(285, 17);
            this.rbExecInprocess.TabIndex = 0;
            this.rbExecInprocess.TabStop = true;
            this.rbExecInprocess.Text = "Execue handler directly (in-process background thread)";
            this.rbExecInprocess.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpLog);
            this.tabControl1.Controls.Add(this.tpRequests);
            this.tabControl1.Controls.Add(this.tpWorker);
            this.tabControl1.Controls.Add(this.tpWorkerB);
            this.tabControl1.Controls.Add(this.tpManager);
            this.tabControl1.Controls.Add(this.tpAvailability);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 183);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1177, 272);
            this.tabControl1.TabIndex = 1;
            // 
            // tpLog
            // 
            this.tpLog.Controls.Add(this.txtLog);
            this.tpLog.Location = new System.Drawing.Point(4, 22);
            this.tpLog.Name = "tpLog";
            this.tpLog.Padding = new System.Windows.Forms.Padding(3);
            this.tpLog.Size = new System.Drawing.Size(1169, 246);
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
            this.txtLog.Size = new System.Drawing.Size(1163, 240);
            this.txtLog.TabIndex = 0;
            // 
            // tpRequests
            // 
            this.tpRequests.Controls.Add(this.lvProgress);
            this.tpRequests.Controls.Add(this.panelProgress);
            this.tpRequests.Location = new System.Drawing.Point(4, 22);
            this.tpRequests.Name = "tpRequests";
            this.tpRequests.Padding = new System.Windows.Forms.Padding(3);
            this.tpRequests.Size = new System.Drawing.Size(1169, 246);
            this.tpRequests.TabIndex = 1;
            this.tpRequests.Text = "Requests";
            this.tpRequests.UseVisualStyleBackColor = true;
            // 
            // lvProgress
            // 
            this.lvProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvProgress.Location = new System.Drawing.Point(3, 35);
            this.lvProgress.Name = "lvProgress";
            this.lvProgress.Size = new System.Drawing.Size(1163, 208);
            this.lvProgress.TabIndex = 1;
            this.lvProgress.UseCompatibleStateImageBehavior = false;
            // 
            // panelProgress
            // 
            this.panelProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProgress.Location = new System.Drawing.Point(3, 3);
            this.panelProgress.Name = "panelProgress";
            this.panelProgress.Size = new System.Drawing.Size(1163, 32);
            this.panelProgress.TabIndex = 0;
            // 
            // tpWorker
            // 
            this.tpWorker.Controls.Add(this.groupBox2);
            this.tpWorker.Controls.Add(this.panel2);
            this.tpWorker.Location = new System.Drawing.Point(4, 22);
            this.tpWorker.Name = "tpWorker";
            this.tpWorker.Padding = new System.Windows.Forms.Padding(3);
            this.tpWorker.Size = new System.Drawing.Size(1169, 246);
            this.tpWorker.TabIndex = 2;
            this.tpWorker.Text = "Worker A";
            this.tpWorker.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtWorkerALog);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 35);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1163, 208);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Log";
            // 
            // txtWorkerALog
            // 
            this.txtWorkerALog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtWorkerALog.Location = new System.Drawing.Point(3, 16);
            this.txtWorkerALog.Multiline = true;
            this.txtWorkerALog.Name = "txtWorkerALog";
            this.txtWorkerALog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtWorkerALog.Size = new System.Drawing.Size(1157, 189);
            this.txtWorkerALog.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnStopWorkerA);
            this.panel2.Controls.Add(this.btnStartWorkerA);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1163, 32);
            this.panel2.TabIndex = 0;
            // 
            // btnStopWorkerA
            // 
            this.btnStopWorkerA.Location = new System.Drawing.Point(86, 3);
            this.btnStopWorkerA.Name = "btnStopWorkerA";
            this.btnStopWorkerA.Size = new System.Drawing.Size(75, 23);
            this.btnStopWorkerA.TabIndex = 1;
            this.btnStopWorkerA.Text = "Stop";
            this.btnStopWorkerA.UseVisualStyleBackColor = true;
            this.btnStopWorkerA.Click += new System.EventHandler(this.btnStopWorker_Click);
            // 
            // btnStartWorkerA
            // 
            this.btnStartWorkerA.Location = new System.Drawing.Point(5, 3);
            this.btnStartWorkerA.Name = "btnStartWorkerA";
            this.btnStartWorkerA.Size = new System.Drawing.Size(75, 23);
            this.btnStartWorkerA.TabIndex = 0;
            this.btnStartWorkerA.Text = "Start";
            this.btnStartWorkerA.UseVisualStyleBackColor = true;
            this.btnStartWorkerA.Click += new System.EventHandler(this.btnStartWorker_Click);
            // 
            // tpWorkerB
            // 
            this.tpWorkerB.Controls.Add(this.groupBox5);
            this.tpWorkerB.Controls.Add(this.panel4);
            this.tpWorkerB.Location = new System.Drawing.Point(4, 22);
            this.tpWorkerB.Name = "tpWorkerB";
            this.tpWorkerB.Padding = new System.Windows.Forms.Padding(3);
            this.tpWorkerB.Size = new System.Drawing.Size(1169, 246);
            this.tpWorkerB.TabIndex = 4;
            this.tpWorkerB.Text = "Worker B";
            this.tpWorkerB.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtWorkerBLog);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(3, 35);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(1163, 208);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Log";
            // 
            // txtWorkerBLog
            // 
            this.txtWorkerBLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtWorkerBLog.Location = new System.Drawing.Point(3, 16);
            this.txtWorkerBLog.Multiline = true;
            this.txtWorkerBLog.Name = "txtWorkerBLog";
            this.txtWorkerBLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtWorkerBLog.Size = new System.Drawing.Size(1157, 189);
            this.txtWorkerBLog.TabIndex = 0;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnStopWorkerB);
            this.panel4.Controls.Add(this.btnStartWorkerB);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(3, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1163, 32);
            this.panel4.TabIndex = 2;
            // 
            // btnStopWorkerB
            // 
            this.btnStopWorkerB.Location = new System.Drawing.Point(86, 3);
            this.btnStopWorkerB.Name = "btnStopWorkerB";
            this.btnStopWorkerB.Size = new System.Drawing.Size(75, 23);
            this.btnStopWorkerB.TabIndex = 1;
            this.btnStopWorkerB.Text = "Stop";
            this.btnStopWorkerB.UseVisualStyleBackColor = true;
            this.btnStopWorkerB.Click += new System.EventHandler(this.btnStopWorkerB_Click);
            // 
            // btnStartWorkerB
            // 
            this.btnStartWorkerB.Location = new System.Drawing.Point(5, 3);
            this.btnStartWorkerB.Name = "btnStartWorkerB";
            this.btnStartWorkerB.Size = new System.Drawing.Size(75, 23);
            this.btnStartWorkerB.TabIndex = 0;
            this.btnStartWorkerB.Text = "Start";
            this.btnStartWorkerB.UseVisualStyleBackColor = true;
            this.btnStartWorkerB.Click += new System.EventHandler(this.btnStartWorkerB_Click);
            // 
            // tpManager
            // 
            this.tpManager.Controls.Add(this.groupBox4);
            this.tpManager.Controls.Add(this.panel3);
            this.tpManager.Location = new System.Drawing.Point(4, 22);
            this.tpManager.Name = "tpManager";
            this.tpManager.Padding = new System.Windows.Forms.Padding(3);
            this.tpManager.Size = new System.Drawing.Size(1169, 246);
            this.tpManager.TabIndex = 3;
            this.tpManager.Text = "Manager";
            this.tpManager.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtManagerLog);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(3, 35);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1163, 208);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Log";
            // 
            // txtManagerLog
            // 
            this.txtManagerLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtManagerLog.Location = new System.Drawing.Point(3, 16);
            this.txtManagerLog.Multiline = true;
            this.txtManagerLog.Name = "txtManagerLog";
            this.txtManagerLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtManagerLog.Size = new System.Drawing.Size(1157, 189);
            this.txtManagerLog.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnStopManager);
            this.panel3.Controls.Add(this.btnStartManager);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1163, 32);
            this.panel3.TabIndex = 2;
            // 
            // btnStopManager
            // 
            this.btnStopManager.Location = new System.Drawing.Point(86, 3);
            this.btnStopManager.Name = "btnStopManager";
            this.btnStopManager.Size = new System.Drawing.Size(75, 23);
            this.btnStopManager.TabIndex = 1;
            this.btnStopManager.Text = "Stop";
            this.btnStopManager.UseVisualStyleBackColor = true;
            this.btnStopManager.Click += new System.EventHandler(this.btnStopManager_Click);
            // 
            // btnStartManager
            // 
            this.btnStartManager.Location = new System.Drawing.Point(5, 3);
            this.btnStartManager.Name = "btnStartManager";
            this.btnStartManager.Size = new System.Drawing.Size(75, 23);
            this.btnStartManager.TabIndex = 0;
            this.btnStartManager.Text = "Start";
            this.btnStartManager.UseVisualStyleBackColor = true;
            this.btnStartManager.Click += new System.EventHandler(this.btnStartManager_Click);
            // 
            // tpAvailability
            // 
            this.tpAvailability.Controls.Add(this.lvAvailability);
            this.tpAvailability.Controls.Add(this.panelAvailability);
            this.tpAvailability.Location = new System.Drawing.Point(4, 22);
            this.tpAvailability.Name = "tpAvailability";
            this.tpAvailability.Padding = new System.Windows.Forms.Padding(3);
            this.tpAvailability.Size = new System.Drawing.Size(1169, 246);
            this.tpAvailability.TabIndex = 5;
            this.tpAvailability.Text = "Availability";
            this.tpAvailability.UseVisualStyleBackColor = true;
            // 
            // lvAvailability
            // 
            this.lvAvailability.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvAvailability.Location = new System.Drawing.Point(3, 35);
            this.lvAvailability.Name = "lvAvailability";
            this.lvAvailability.Size = new System.Drawing.Size(1163, 208);
            this.lvAvailability.TabIndex = 3;
            this.lvAvailability.UseCompatibleStateImageBehavior = false;
            // 
            // panelAvailability
            // 
            this.panelAvailability.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAvailability.Location = new System.Drawing.Point(3, 3);
            this.panelAvailability.Name = "panelAvailability";
            this.panelAvailability.Size = new System.Drawing.Size(1163, 32);
            this.panelAvailability.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1177, 455);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Grid Test Harness";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPingDelay)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tpLog.ResumeLayout(false);
            this.tpLog.PerformLayout();
            this.tpRequests.ResumeLayout(false);
            this.tpWorker.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tpWorkerB.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.tpManager.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.tpAvailability.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtWorkerInstance;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtWorkerComputer;
        private System.Windows.Forms.RadioButton rbExecSendToManager;
        private System.Windows.Forms.RadioButton rbExecAssignToWorker;
        private System.Windows.Forms.RadioButton rbExecInprocess;
        private System.Windows.Forms.TextBox txtRequestId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TabPage tpRequests;
        private System.Windows.Forms.Button btnPingHandler;
        private System.Windows.Forms.ListView lvProgress;
        private System.Windows.Forms.Panel panelProgress;
        private System.Windows.Forms.TabPage tpWorker;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtWorkerALog;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnStopWorkerA;
        private System.Windows.Forms.Button btnStartWorkerA;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnPVRWoolworths;
        private System.Windows.Forms.TabPage tpManager;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtManagerLog;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnStopManager;
        private System.Windows.Forms.Button btnStartManager;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkPingFault;
        private System.Windows.Forms.NumericUpDown nudPingDelay;
        private System.Windows.Forms.TabPage tpWorkerB;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox txtWorkerBLog;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnStopWorkerB;
        private System.Windows.Forms.Button btnStartWorkerB;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.ComboBox cbMarketName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cbCounterParty;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkAllFXStresses;
        private System.Windows.Forms.CheckBox chkAllIRStresses;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button btnCurveGenRequest;
        private System.Windows.Forms.TabPage tpAvailability;
        private System.Windows.Forms.ListView lvAvailability;
        private System.Windows.Forms.Panel panelAvailability;
    }
}

