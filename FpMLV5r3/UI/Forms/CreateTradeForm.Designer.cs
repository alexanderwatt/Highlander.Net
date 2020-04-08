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
            this.checkPropertyAssetButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.FindPropertyAssetButton = new System.Windows.Forms.Button();
            this.shortNameTextBox = new System.Windows.Forms.TextBox();
            this.shortNameLabel = new System.Windows.Forms.Label();
            this.propertyTypeListBox = new System.Windows.Forms.ListBox();
            this.label10 = new System.Windows.Forms.Label();
            this.propertyIdentifierTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.propertyAssetIdentifierTextBox = new System.Windows.Forms.TextBox();
            this.accountingBookTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.currencyListBox = new System.Windows.Forms.ListBox();
            this.paymentDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.label24 = new System.Windows.Forms.Label();
            this.effectiveDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.isParty1BuyerCheckBox = new System.Windows.Forms.CheckBox();
            this.startDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.Party2TextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.createTradeButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.Party1TextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.purchaseAmountTextBox = new System.Windows.Forms.TextBox();
            this.tradeIdentifierTxtBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.cityTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.postcodeTextBox = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.propertyDataGridView)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.transactionTypeLabel);
            this.splitContainer1.Panel1.Controls.Add(this.propertiesLabel);
            this.splitContainer1.Panel1.Controls.Add(this.propertyDataGridView);
            this.splitContainer1.Panel1.Controls.Add(this.tradeTypeBox);
            this.splitContainer1.Panel1.Controls.Add(this.checkPropertyAssetButton);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label7);
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Size = new System.Drawing.Size(848, 335);
            this.splitContainer1.SplitterDistance = 286;
            this.splitContainer1.TabIndex = 0;
            // 
            // transactionTypeLabel
            // 
            this.transactionTypeLabel.AutoSize = true;
            this.transactionTypeLabel.Location = new System.Drawing.Point(12, 3);
            this.transactionTypeLabel.Name = "transactionTypeLabel";
            this.transactionTypeLabel.Size = new System.Drawing.Size(90, 13);
            this.transactionTypeLabel.TabIndex = 3;
            this.transactionTypeLabel.Text = "Transaction Type";
            // 
            // propertiesLabel
            // 
            this.propertiesLabel.AutoSize = true;
            this.propertiesLabel.Location = new System.Drawing.Point(12, 142);
            this.propertiesLabel.Name = "propertiesLabel";
            this.propertiesLabel.Size = new System.Drawing.Size(54, 13);
            this.propertiesLabel.TabIndex = 2;
            this.propertiesLabel.Text = "Properties";
            // 
            // propertyDataGridView
            // 
            this.propertyDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.propertyDataGridView.Location = new System.Drawing.Point(12, 158);
            this.propertyDataGridView.Name = "propertyDataGridView";
            this.propertyDataGridView.Size = new System.Drawing.Size(250, 168);
            this.propertyDataGridView.TabIndex = 0;
            // 
            // tradeTypeBox
            // 
            this.tradeTypeBox.FormattingEnabled = true;
            this.tradeTypeBox.Items.AddRange(new object[] {
            "PropertyTransaction",
            "LeaseTransaction"});
            this.tradeTypeBox.Location = new System.Drawing.Point(138, 12);
            this.tradeTypeBox.Name = "tradeTypeBox";
            this.tradeTypeBox.Size = new System.Drawing.Size(122, 108);
            this.tradeTypeBox.TabIndex = 0;
            // 
            // checkPropertyAssetButton
            // 
            this.checkPropertyAssetButton.Location = new System.Drawing.Point(138, 130);
            this.checkPropertyAssetButton.Name = "checkPropertyAssetButton";
            this.checkPropertyAssetButton.Size = new System.Drawing.Size(122, 22);
            this.checkPropertyAssetButton.TabIndex = 1;
            this.checkPropertyAssetButton.Text = "Check Underlying Asset";
            this.checkPropertyAssetButton.UseVisualStyleBackColor = true;
            this.checkPropertyAssetButton.Click += new System.EventHandler(this.CreatePropertyAssetClick);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 30;
            this.label7.Text = "Transaction";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.postcodeTextBox);
            this.panel2.Controls.Add(this.label17);
            this.panel2.Controls.Add(this.cityTextBox);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.FindPropertyAssetButton);
            this.panel2.Controls.Add(this.shortNameTextBox);
            this.panel2.Controls.Add(this.shortNameLabel);
            this.panel2.Controls.Add(this.propertyTypeListBox);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.propertyIdentifierTextBox);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.propertyAssetIdentifierTextBox);
            this.panel2.Controls.Add(this.accountingBookTextBox);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.currencyListBox);
            this.panel2.Controls.Add(this.paymentDateTimePicker);
            this.panel2.Controls.Add(this.label24);
            this.panel2.Controls.Add(this.effectiveDateTimePicker);
            this.panel2.Controls.Add(this.isParty1BuyerCheckBox);
            this.panel2.Controls.Add(this.startDateTimePicker);
            this.panel2.Controls.Add(this.Party2TextBox);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.createTradeButton);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.Party1TextBox);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.purchaseAmountTextBox);
            this.panel2.Controls.Add(this.tradeIdentifierTxtBox);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Location = new System.Drawing.Point(8, 19);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(540, 307);
            this.panel2.TabIndex = 32;
            // 
            // FindPropertyAssetButton
            // 
            this.FindPropertyAssetButton.Location = new System.Drawing.Point(19, 277);
            this.FindPropertyAssetButton.Name = "FindPropertyAssetButton";
            this.FindPropertyAssetButton.Size = new System.Drawing.Size(78, 22);
            this.FindPropertyAssetButton.TabIndex = 65;
            this.FindPropertyAssetButton.Text = "Find Property";
            this.FindPropertyAssetButton.UseVisualStyleBackColor = true;
            this.FindPropertyAssetButton.Click += new System.EventHandler(this.FindPropertyAssetButtonClick);
            // 
            // shortNameTextBox
            // 
            this.shortNameTextBox.Location = new System.Drawing.Point(112, 91);
            this.shortNameTextBox.Name = "shortNameTextBox";
            this.shortNameTextBox.Size = new System.Drawing.Size(71, 20);
            this.shortNameTextBox.TabIndex = 64;
            // 
            // shortNameLabel
            // 
            this.shortNameLabel.AutoSize = true;
            this.shortNameLabel.Location = new System.Drawing.Point(20, 95);
            this.shortNameLabel.Name = "shortNameLabel";
            this.shortNameLabel.Size = new System.Drawing.Size(66, 13);
            this.shortNameLabel.TabIndex = 63;
            this.shortNameLabel.Text = "Short Name:";
            // 
            // propertyTypeListBox
            // 
            this.propertyTypeListBox.FormattingEnabled = true;
            this.propertyTypeListBox.Items.AddRange(new object[] {
            "Commercial",
            "Residential",
            "Other"});
            this.propertyTypeListBox.Location = new System.Drawing.Point(112, 11);
            this.propertyTypeListBox.Name = "propertyTypeListBox";
            this.propertyTypeListBox.Size = new System.Drawing.Size(71, 43);
            this.propertyTypeListBox.TabIndex = 62;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 12);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(76, 13);
            this.label10.TabIndex = 60;
            this.label10.Text = "Property Type:";
            // 
            // propertyIdentifierTextBox
            // 
            this.propertyIdentifierTextBox.Location = new System.Drawing.Point(111, 149);
            this.propertyIdentifierTextBox.Name = "propertyIdentifierTextBox";
            this.propertyIdentifierTextBox.Size = new System.Drawing.Size(71, 20);
            this.propertyIdentifierTextBox.TabIndex = 61;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 152);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 13);
            this.label6.TabIndex = 59;
            this.label6.Text = "Property Identifier:";
            // 
            // propertyAssetIdentifierTextBox
            // 
            this.propertyAssetIdentifierTextBox.Location = new System.Drawing.Point(19, 251);
            this.propertyAssetIdentifierTextBox.Name = "propertyAssetIdentifierTextBox";
            this.propertyAssetIdentifierTextBox.ReadOnly = true;
            this.propertyAssetIdentifierTextBox.Size = new System.Drawing.Size(300, 20);
            this.propertyAssetIdentifierTextBox.TabIndex = 28;
            // 
            // accountingBookTextBox
            // 
            this.accountingBookTextBox.Location = new System.Drawing.Point(384, 230);
            this.accountingBookTextBox.Name = "accountingBookTextBox";
            this.accountingBookTextBox.Size = new System.Drawing.Size(87, 20);
            this.accountingBookTextBox.TabIndex = 27;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(289, 234);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(92, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "Accounting Book:";
            // 
            // currencyListBox
            // 
            this.currencyListBox.FormattingEnabled = true;
            this.currencyListBox.Items.AddRange(new object[] {
            "AUD"});
            this.currencyListBox.Location = new System.Drawing.Point(384, 207);
            this.currencyListBox.Name = "currencyListBox";
            this.currencyListBox.Size = new System.Drawing.Size(42, 17);
            this.currencyListBox.TabIndex = 29;
            // 
            // paymentDateTimePicker
            // 
            this.paymentDateTimePicker.CustomFormat = "dd/MM/yyyy";
            this.paymentDateTimePicker.Location = new System.Drawing.Point(385, 178);
            this.paymentDateTimePicker.Name = "paymentDateTimePicker";
            this.paymentDateTimePicker.Size = new System.Drawing.Size(148, 20);
            this.paymentDateTimePicker.TabIndex = 18;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(19, 235);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(145, 13);
            this.label24.TabIndex = 27;
            this.label24.Text = "Reference Property Identifier:";
            // 
            // effectiveDateTimePicker
            // 
            this.effectiveDateTimePicker.CustomFormat = "dd/MM/yyyy";
            this.effectiveDateTimePicker.Location = new System.Drawing.Point(385, 152);
            this.effectiveDateTimePicker.Name = "effectiveDateTimePicker";
            this.effectiveDateTimePicker.Size = new System.Drawing.Size(148, 20);
            this.effectiveDateTimePicker.TabIndex = 17;
            // 
            // isParty1BuyerCheckBox
            // 
            this.isParty1BuyerCheckBox.AutoSize = true;
            this.isParty1BuyerCheckBox.Location = new System.Drawing.Point(289, 89);
            this.isParty1BuyerCheckBox.Name = "isParty1BuyerCheckBox";
            this.isParty1BuyerCheckBox.Size = new System.Drawing.Size(90, 17);
            this.isParty1BuyerCheckBox.TabIndex = 20;
            this.isParty1BuyerCheckBox.Text = "isParty1Buyer";
            this.isParty1BuyerCheckBox.UseVisualStyleBackColor = true;
            // 
            // startDateTimePicker
            // 
            this.startDateTimePicker.CustomFormat = "dd/MM/yyyy";
            this.startDateTimePicker.Location = new System.Drawing.Point(385, 123);
            this.startDateTimePicker.Name = "startDateTimePicker";
            this.startDateTimePicker.Size = new System.Drawing.Size(148, 20);
            this.startDateTimePicker.TabIndex = 16;
            // 
            // Party2TextBox
            // 
            this.Party2TextBox.Location = new System.Drawing.Point(385, 64);
            this.Party2TextBox.Name = "Party2TextBox";
            this.Party2TextBox.Size = new System.Drawing.Size(86, 20);
            this.Party2TextBox.TabIndex = 22;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(287, 124);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Start Date:";
            // 
            // createTradeButton
            // 
            this.createTradeButton.Location = new System.Drawing.Point(384, 277);
            this.createTradeButton.Name = "createTradeButton";
            this.createTradeButton.Size = new System.Drawing.Size(149, 22);
            this.createTradeButton.TabIndex = 0;
            this.createTradeButton.Text = "Create Property Transaction";
            this.createTradeButton.UseVisualStyleBackColor = true;
            this.createTradeButton.Click += new System.EventHandler(this.CreateTradeButtonClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(287, 152);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Effective Date:";
            // 
            // Party1TextBox
            // 
            this.Party1TextBox.Location = new System.Drawing.Point(385, 36);
            this.Party1TextBox.Name = "Party1TextBox";
            this.Party1TextBox.Size = new System.Drawing.Size(86, 20);
            this.Party1TextBox.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(287, 182);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Payment Date:";
            // 
            // purchaseAmountTextBox
            // 
            this.purchaseAmountTextBox.Location = new System.Drawing.Point(434, 204);
            this.purchaseAmountTextBox.Name = "purchaseAmountTextBox";
            this.purchaseAmountTextBox.Size = new System.Drawing.Size(99, 20);
            this.purchaseAmountTextBox.TabIndex = 19;
            this.purchaseAmountTextBox.Text = "10,000,000";
            // 
            // tradeIdentifierTxtBox
            // 
            this.tradeIdentifierTxtBox.Location = new System.Drawing.Point(384, 9);
            this.tradeIdentifierTxtBox.Name = "tradeIdentifierTxtBox";
            this.tradeIdentifierTxtBox.Size = new System.Drawing.Size(87, 20);
            this.tradeIdentifierTxtBox.TabIndex = 23;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(275, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Trade Identifier:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(286, 207);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Purchase Amount:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(313, 36);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "Party 1:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(313, 64);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Party 2:";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(257, 215);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(88, 20);
            this.textBox3.TabIndex = 6;
            // 
            // cityTextBox
            // 
            this.cityTextBox.Location = new System.Drawing.Point(111, 63);
            this.cityTextBox.Name = "cityTextBox";
            this.cityTextBox.Size = new System.Drawing.Size(71, 20);
            this.cityTextBox.TabIndex = 67;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(19, 67);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(27, 13);
            this.label11.TabIndex = 66;
            this.label11.Text = "City:";
            // 
            // postcodeTextBox
            // 
            this.postcodeTextBox.Location = new System.Drawing.Point(111, 120);
            this.postcodeTextBox.Name = "postcodeTextBox";
            this.postcodeTextBox.Size = new System.Drawing.Size(39, 20);
            this.postcodeTextBox.TabIndex = 69;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(43, 123);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(55, 13);
            this.label17.TabIndex = 68;
            this.label17.Text = "Postcode:";
            // 
            // CreateTradeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 335);
            this.Controls.Add(this.splitContainer1);
            this.Name = "CreateTradeForm";
            this.Text = "CreateTradeForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.propertyDataGridView)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox tradeTypeBox;
        private System.Windows.Forms.DataGridView propertyDataGridView;
        private System.Windows.Forms.Label propertiesLabel;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button createTradeButton;
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
        private System.Windows.Forms.TextBox tradeIdentifierTxtBox;
        private System.Windows.Forms.TextBox Party2TextBox;
        private System.Windows.Forms.TextBox Party1TextBox;
        private System.Windows.Forms.CheckBox isParty1BuyerCheckBox;
        private System.Windows.Forms.TextBox purchaseAmountTextBox;
        private System.Windows.Forms.DateTimePicker paymentDateTimePicker;
        private System.Windows.Forms.DateTimePicker effectiveDateTimePicker;
        private System.Windows.Forms.DateTimePicker startDateTimePicker;
        private System.Windows.Forms.ListBox currencyListBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button checkPropertyAssetButton;
        private System.Windows.Forms.TextBox propertyAssetIdentifierTextBox;
        private System.Windows.Forms.Label label24;
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
    }
}