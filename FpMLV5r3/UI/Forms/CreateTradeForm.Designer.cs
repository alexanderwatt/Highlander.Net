namespace Highlander.Reporting.UI.V5r3
{
    partial class CreateTradeForm
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
            this.transactionTypeLabel = new System.Windows.Forms.Label();
            this.propertiesLabel = new System.Windows.Forms.Label();
            this.propertyDataGridView = new System.Windows.Forms.DataGridView();
            this.tradeTypeBox = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.transactionIdentifierTextBox = new System.Windows.Forms.TextBox();
            this.CreateLeaseTransactionButton = new System.Windows.Forms.Button();
            this.tradeIdentifierTextBox = new System.Windows.Forms.TextBox();
            this.accountingBookTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.currencyListBox = new System.Windows.Forms.ListBox();
            this.paymentDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.effectiveDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.isParty1BuyerCheckBox = new System.Windows.Forms.CheckBox();
            this.startDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.Party2TextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.CreatePropertyTradeButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.Party1TextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.purchaseAmountTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.postcodeTextBox = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.FindPropertyAssetButton = new System.Windows.Forms.Button();
            this.propertyAssetIdentifierTextBox = new System.Windows.Forms.TextBox();
            this.cityTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.shortNameTextBox = new System.Windows.Forms.TextBox();
            this.shortNameLabel = new System.Windows.Forms.Label();
            this.propertyTypeListBox = new System.Windows.Forms.ListBox();
            this.label10 = new System.Windows.Forms.Label();
            this.propertyIdentifierTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.propertyDataGridView)).BeginInit();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.transactionTypeLabel);
            this.splitContainer1.Panel1.Controls.Add(this.propertiesLabel);
            this.splitContainer1.Panel1.Controls.Add(this.propertyDataGridView);
            this.splitContainer1.Panel1.Controls.Add(this.tradeTypeBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Size = new System.Drawing.Size(1186, 406);
            this.splitContainer1.SplitterDistance = 364;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // transactionTypeLabel
            // 
            this.transactionTypeLabel.AutoSize = true;
            this.transactionTypeLabel.Location = new System.Drawing.Point(13, 16);
            this.transactionTypeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.transactionTypeLabel.Name = "transactionTypeLabel";
            this.transactionTypeLabel.Size = new System.Drawing.Size(119, 17);
            this.transactionTypeLabel.TabIndex = 3;
            this.transactionTypeLabel.Text = "Transaction Type";
            // 
            // propertiesLabel
            // 
            this.propertiesLabel.AutoSize = true;
            this.propertiesLabel.Location = new System.Drawing.Point(16, 154);
            this.propertiesLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.propertiesLabel.Name = "propertiesLabel";
            this.propertiesLabel.Size = new System.Drawing.Size(73, 17);
            this.propertiesLabel.TabIndex = 2;
            this.propertiesLabel.Text = "Properties";
            // 
            // propertyDataGridView
            // 
            this.propertyDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.propertyDataGridView.Location = new System.Drawing.Point(20, 176);
            this.propertyDataGridView.Margin = new System.Windows.Forms.Padding(4);
            this.propertyDataGridView.Name = "propertyDataGridView";
            this.propertyDataGridView.RowHeadersWidth = 62;
            this.propertyDataGridView.Size = new System.Drawing.Size(339, 219);
            this.propertyDataGridView.TabIndex = 0;
            // 
            // tradeTypeBox
            // 
            this.tradeTypeBox.FormattingEnabled = true;
            this.tradeTypeBox.ItemHeight = 16;
            this.tradeTypeBox.Items.AddRange(new object[] {
            "PropertyTransaction",
            "LeaseTransaction"});
            this.tradeTypeBox.Location = new System.Drawing.Point(191, 13);
            this.tradeTypeBox.Margin = new System.Windows.Forms.Padding(4);
            this.tradeTypeBox.Name = "tradeTypeBox";
            this.tradeTypeBox.Size = new System.Drawing.Size(168, 132);
            this.tradeTypeBox.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.transactionIdentifierTextBox);
            this.panel2.Controls.Add(this.CreateLeaseTransactionButton);
            this.panel2.Controls.Add(this.tradeIdentifierTextBox);
            this.panel2.Controls.Add(this.accountingBookTextBox);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.currencyListBox);
            this.panel2.Controls.Add(this.paymentDateTimePicker);
            this.panel2.Controls.Add(this.effectiveDateTimePicker);
            this.panel2.Controls.Add(this.isParty1BuyerCheckBox);
            this.panel2.Controls.Add(this.startDateTimePicker);
            this.panel2.Controls.Add(this.Party2TextBox);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.CreatePropertyTradeButton);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.Party1TextBox);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.purchaseAmountTextBox);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Location = new System.Drawing.Point(27, 4);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(777, 391);
            this.panel2.TabIndex = 32;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(658, 303);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 17);
            this.label7.TabIndex = 74;
            this.label7.Text = "Unique Identifier:";
            // 
            // transactionIdentifierTextBox
            // 
            this.transactionIdentifierTextBox.Location = new System.Drawing.Point(380, 323);
            this.transactionIdentifierTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.transactionIdentifierTextBox.Name = "transactionIdentifierTextBox";
            this.transactionIdentifierTextBox.Size = new System.Drawing.Size(393, 22);
            this.transactionIdentifierTextBox.TabIndex = 73;
            // 
            // CreateLeaseTransactionButton
            // 
            this.CreateLeaseTransactionButton.Location = new System.Drawing.Point(15, 354);
            this.CreateLeaseTransactionButton.Margin = new System.Windows.Forms.Padding(4);
            this.CreateLeaseTransactionButton.Name = "CreateLeaseTransactionButton";
            this.CreateLeaseTransactionButton.Size = new System.Drawing.Size(199, 27);
            this.CreateLeaseTransactionButton.TabIndex = 72;
            this.CreateLeaseTransactionButton.Text = "Create Lease Transaction";
            this.CreateLeaseTransactionButton.UseVisualStyleBackColor = true;
            this.CreateLeaseTransactionButton.Click += new System.EventHandler(this.CreateLeaseTransactionButton_Click);
            // 
            // tradeIdentifierTextBox
            // 
            this.tradeIdentifierTextBox.Location = new System.Drawing.Point(581, 9);
            this.tradeIdentifierTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.tradeIdentifierTextBox.Name = "tradeIdentifierTextBox";
            this.tradeIdentifierTextBox.Size = new System.Drawing.Size(113, 22);
            this.tradeIdentifierTextBox.TabIndex = 71;
            // 
            // accountingBookTextBox
            // 
            this.accountingBookTextBox.Location = new System.Drawing.Point(497, 272);
            this.accountingBookTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.accountingBookTextBox.Name = "accountingBookTextBox";
            this.accountingBookTextBox.Size = new System.Drawing.Size(114, 22);
            this.accountingBookTextBox.TabIndex = 27;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(449, 275);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(44, 17);
            this.label12.TabIndex = 15;
            this.label12.Text = "Book:";
            // 
            // currencyListBox
            // 
            this.currencyListBox.FormattingEnabled = true;
            this.currencyListBox.ItemHeight = 16;
            this.currencyListBox.Items.AddRange(new object[] {
            "AUD"});
            this.currencyListBox.Location = new System.Drawing.Point(500, 240);
            this.currencyListBox.Margin = new System.Windows.Forms.Padding(4);
            this.currencyListBox.Name = "currencyListBox";
            this.currencyListBox.Size = new System.Drawing.Size(55, 20);
            this.currencyListBox.TabIndex = 29;
            // 
            // paymentDateTimePicker
            // 
            this.paymentDateTimePicker.CustomFormat = "dd/MM/yyyy";
            this.paymentDateTimePicker.Location = new System.Drawing.Point(497, 206);
            this.paymentDateTimePicker.Margin = new System.Windows.Forms.Padding(4);
            this.paymentDateTimePicker.Name = "paymentDateTimePicker";
            this.paymentDateTimePicker.Size = new System.Drawing.Size(196, 22);
            this.paymentDateTimePicker.TabIndex = 18;
            // 
            // effectiveDateTimePicker
            // 
            this.effectiveDateTimePicker.CustomFormat = "dd/MM/yyyy";
            this.effectiveDateTimePicker.Location = new System.Drawing.Point(497, 174);
            this.effectiveDateTimePicker.Margin = new System.Windows.Forms.Padding(4);
            this.effectiveDateTimePicker.Name = "effectiveDateTimePicker";
            this.effectiveDateTimePicker.Size = new System.Drawing.Size(196, 22);
            this.effectiveDateTimePicker.TabIndex = 17;
            // 
            // isParty1BuyerCheckBox
            // 
            this.isParty1BuyerCheckBox.AutoSize = true;
            this.isParty1BuyerCheckBox.Location = new System.Drawing.Point(576, 101);
            this.isParty1BuyerCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.isParty1BuyerCheckBox.Name = "isParty1BuyerCheckBox";
            this.isParty1BuyerCheckBox.Size = new System.Drawing.Size(118, 21);
            this.isParty1BuyerCheckBox.TabIndex = 20;
            this.isParty1BuyerCheckBox.Text = "isParty1Buyer";
            this.isParty1BuyerCheckBox.UseVisualStyleBackColor = true;
            // 
            // startDateTimePicker
            // 
            this.startDateTimePicker.CustomFormat = "dd/MM/yyyy";
            this.startDateTimePicker.Location = new System.Drawing.Point(497, 138);
            this.startDateTimePicker.Margin = new System.Windows.Forms.Padding(4);
            this.startDateTimePicker.Name = "startDateTimePicker";
            this.startDateTimePicker.Size = new System.Drawing.Size(196, 22);
            this.startDateTimePicker.TabIndex = 16;
            // 
            // Party2TextBox
            // 
            this.Party2TextBox.Location = new System.Drawing.Point(581, 71);
            this.Party2TextBox.Margin = new System.Windows.Forms.Padding(4);
            this.Party2TextBox.Name = "Party2TextBox";
            this.Party2TextBox.Size = new System.Drawing.Size(113, 22);
            this.Party2TextBox.TabIndex = 22;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(412, 140);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Start Date:";
            // 
            // CreatePropertyTradeButton
            // 
            this.CreatePropertyTradeButton.Location = new System.Drawing.Point(380, 354);
            this.CreatePropertyTradeButton.Margin = new System.Windows.Forms.Padding(4);
            this.CreatePropertyTradeButton.Name = "CreatePropertyTradeButton";
            this.CreatePropertyTradeButton.Size = new System.Drawing.Size(199, 27);
            this.CreatePropertyTradeButton.TabIndex = 0;
            this.CreatePropertyTradeButton.Text = "Create Property Transaction";
            this.CreatePropertyTradeButton.UseVisualStyleBackColor = true;
            this.CreatePropertyTradeButton.Click += new System.EventHandler(this.CreatePropertyTradeButtonClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(392, 174);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Effective Date:";
            // 
            // Party1TextBox
            // 
            this.Party1TextBox.Location = new System.Drawing.Point(581, 39);
            this.Party1TextBox.Margin = new System.Windows.Forms.Padding(4);
            this.Party1TextBox.Name = "Party1TextBox";
            this.Party1TextBox.Size = new System.Drawing.Size(113, 22);
            this.Party1TextBox.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(392, 209);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Payment Date:";
            // 
            // purchaseAmountTextBox
            // 
            this.purchaseAmountTextBox.Location = new System.Drawing.Point(562, 238);
            this.purchaseAmountTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.purchaseAmountTextBox.Name = "purchaseAmountTextBox";
            this.purchaseAmountTextBox.Size = new System.Drawing.Size(132, 22);
            this.purchaseAmountTextBox.TabIndex = 19;
            this.purchaseAmountTextBox.Text = "10,000,000";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(465, 12);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "Trade Identifier:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(432, 242);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Amount:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(516, 39);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(57, 17);
            this.label9.TabIndex = 12;
            this.label9.Text = "Party 1:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(516, 74);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(57, 17);
            this.label8.TabIndex = 11;
            this.label8.Text = "Party 2:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.postcodeTextBox);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.FindPropertyAssetButton);
            this.groupBox1.Controls.Add(this.propertyAssetIdentifierTextBox);
            this.groupBox1.Controls.Add(this.cityTextBox);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.shortNameTextBox);
            this.groupBox1.Controls.Add(this.shortNameLabel);
            this.groupBox1.Controls.Add(this.propertyTypeListBox);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.propertyIdentifierTextBox);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(4, 7);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(381, 313);
            this.groupBox1.TabIndex = 70;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Property Asset Information";
            // 
            // postcodeTextBox
            // 
            this.postcodeTextBox.Location = new System.Drawing.Point(132, 206);
            this.postcodeTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.postcodeTextBox.Name = "postcodeTextBox";
            this.postcodeTextBox.Size = new System.Drawing.Size(50, 22);
            this.postcodeTextBox.TabIndex = 69;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(7, 206);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(71, 17);
            this.label17.TabIndex = 68;
            this.label17.Text = "Postcode:";
            // 
            // FindPropertyAssetButton
            // 
            this.FindPropertyAssetButton.Location = new System.Drawing.Point(11, 280);
            this.FindPropertyAssetButton.Margin = new System.Windows.Forms.Padding(4);
            this.FindPropertyAssetButton.Name = "FindPropertyAssetButton";
            this.FindPropertyAssetButton.Size = new System.Drawing.Size(143, 27);
            this.FindPropertyAssetButton.TabIndex = 65;
            this.FindPropertyAssetButton.Text = "Find Property Asset";
            this.FindPropertyAssetButton.UseVisualStyleBackColor = true;
            this.FindPropertyAssetButton.Click += new System.EventHandler(this.FindPropertyAssetButtonClick);
            // 
            // propertyAssetIdentifierTextBox
            // 
            this.propertyAssetIdentifierTextBox.Location = new System.Drawing.Point(5, 23);
            this.propertyAssetIdentifierTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.propertyAssetIdentifierTextBox.Name = "propertyAssetIdentifierTextBox";
            this.propertyAssetIdentifierTextBox.ReadOnly = true;
            this.propertyAssetIdentifierTextBox.Size = new System.Drawing.Size(371, 22);
            this.propertyAssetIdentifierTextBox.TabIndex = 28;
            // 
            // cityTextBox
            // 
            this.cityTextBox.Location = new System.Drawing.Point(132, 129);
            this.cityTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.cityTextBox.Name = "cityTextBox";
            this.cityTextBox.Size = new System.Drawing.Size(93, 22);
            this.cityTextBox.TabIndex = 67;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 129);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 17);
            this.label11.TabIndex = 66;
            this.label11.Text = "City:";
            // 
            // shortNameTextBox
            // 
            this.shortNameTextBox.Location = new System.Drawing.Point(132, 163);
            this.shortNameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.shortNameTextBox.Name = "shortNameTextBox";
            this.shortNameTextBox.Size = new System.Drawing.Size(93, 22);
            this.shortNameTextBox.TabIndex = 64;
            // 
            // shortNameLabel
            // 
            this.shortNameLabel.AutoSize = true;
            this.shortNameLabel.Location = new System.Drawing.Point(7, 168);
            this.shortNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.shortNameLabel.Name = "shortNameLabel";
            this.shortNameLabel.Size = new System.Drawing.Size(87, 17);
            this.shortNameLabel.TabIndex = 63;
            this.shortNameLabel.Text = "Short Name:";
            // 
            // propertyTypeListBox
            // 
            this.propertyTypeListBox.FormattingEnabled = true;
            this.propertyTypeListBox.ItemHeight = 16;
            this.propertyTypeListBox.Items.AddRange(new object[] {
            "Commercial",
            "Residential",
            "Other"});
            this.propertyTypeListBox.Location = new System.Drawing.Point(132, 63);
            this.propertyTypeListBox.Margin = new System.Windows.Forms.Padding(4);
            this.propertyTypeListBox.Name = "propertyTypeListBox";
            this.propertyTypeListBox.Size = new System.Drawing.Size(93, 52);
            this.propertyTypeListBox.TabIndex = 62;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 63);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(102, 17);
            this.label10.TabIndex = 60;
            this.label10.Text = "Property Type:";
            // 
            // propertyIdentifierTextBox
            // 
            this.propertyIdentifierTextBox.Location = new System.Drawing.Point(132, 242);
            this.propertyIdentifierTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.propertyIdentifierTextBox.Name = "propertyIdentifierTextBox";
            this.propertyIdentifierTextBox.Size = new System.Drawing.Size(93, 22);
            this.propertyIdentifierTextBox.TabIndex = 61;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 242);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(124, 17);
            this.label6.TabIndex = 59;
            this.label6.Text = "Property Identifier:";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(257, 215);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(88, 22);
            this.textBox3.TabIndex = 6;
            // 
            // CreateTradeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1186, 406);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CreateTradeForm";
            this.Text = "CreateTradeForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.propertyDataGridView)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox tradeTypeBox;
        private System.Windows.Forms.DataGridView propertyDataGridView;
        private System.Windows.Forms.Label propertiesLabel;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button CreatePropertyTradeButton;
        private System.Windows.Forms.Label transactionTypeLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox accountingBookTextBox;
        private System.Windows.Forms.TextBox Party2TextBox;
        private System.Windows.Forms.TextBox Party1TextBox;
        private System.Windows.Forms.CheckBox isParty1BuyerCheckBox;
        private System.Windows.Forms.TextBox purchaseAmountTextBox;
        private System.Windows.Forms.DateTimePicker paymentDateTimePicker;
        private System.Windows.Forms.DateTimePicker effectiveDateTimePicker;
        private System.Windows.Forms.DateTimePicker startDateTimePicker;
        private System.Windows.Forms.ListBox currencyListBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox propertyAssetIdentifierTextBox;
        private System.Windows.Forms.Button FindPropertyAssetButton;
        private System.Windows.Forms.TextBox shortNameTextBox;
        private System.Windows.Forms.Label shortNameLabel;
        private System.Windows.Forms.ListBox propertyTypeListBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox propertyIdentifierTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox cityTextBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox postcodeTextBox;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tradeIdentifierTextBox;
        private System.Windows.Forms.Button CreateLeaseTransactionButton;
        private System.Windows.Forms.TextBox transactionIdentifierTextBox;
        private System.Windows.Forms.Label label7;
    }
}