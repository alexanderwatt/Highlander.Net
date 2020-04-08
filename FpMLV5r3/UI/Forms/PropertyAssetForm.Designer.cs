namespace Highlander.Reporting.UI.V5r3
{
    partial class PropertyAssetForm
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
            this.FindPropertyAssetButton = new System.Windows.Forms.Button();
            this.shortNameTextBox = new System.Windows.Forms.TextBox();
            this.shortNameLabel = new System.Windows.Forms.Label();
            this.propertyTypeListBox = new System.Windows.Forms.ListBox();
            this.label10 = new System.Windows.Forms.Label();
            this.propertyIdentifierTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.uniqueIdentifierTextBox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.currencyTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.parkingTextBox = new System.Windows.Forms.TextBox();
            this.bedroomsTextBox = new System.Windows.Forms.TextBox();
            this.bathroomsTextBox = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.streetNameTextBox = new System.Windows.Forms.TextBox();
            this.suburbTextBox = new System.Windows.Forms.TextBox();
            this.streetIdentifierTextBox = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.cityTextBox = new System.Windows.Forms.TextBox();
            this.postcodeTextBox = new System.Windows.Forms.TextBox();
            this.countryTextBox = new System.Windows.Forms.TextBox();
            this.stateTextBox = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FindPropertyAssetButton
            // 
            this.FindPropertyAssetButton.Location = new System.Drawing.Point(12, 170);
            this.FindPropertyAssetButton.Name = "FindPropertyAssetButton";
            this.FindPropertyAssetButton.Size = new System.Drawing.Size(78, 22);
            this.FindPropertyAssetButton.TabIndex = 58;
            this.FindPropertyAssetButton.Text = "Find Property";
            this.FindPropertyAssetButton.UseVisualStyleBackColor = true;
            this.FindPropertyAssetButton.Click += new System.EventHandler(this.FindPropertyAssetButtonClick);
            // 
            // shortNameTextBox
            // 
            this.shortNameTextBox.Location = new System.Drawing.Point(107, 87);
            this.shortNameTextBox.Name = "shortNameTextBox";
            this.shortNameTextBox.Size = new System.Drawing.Size(135, 20);
            this.shortNameTextBox.TabIndex = 57;
            // 
            // shortNameLabel
            // 
            this.shortNameLabel.AutoSize = true;
            this.shortNameLabel.Location = new System.Drawing.Point(28, 91);
            this.shortNameLabel.Name = "shortNameLabel";
            this.shortNameLabel.Size = new System.Drawing.Size(66, 13);
            this.shortNameLabel.TabIndex = 56;
            this.shortNameLabel.Text = "Short Name:";
            // 
            // propertyTypeListBox
            // 
            this.propertyTypeListBox.FormattingEnabled = true;
            this.propertyTypeListBox.Items.AddRange(new object[] {
            "Commercial",
            "Residential",
            "Other"});
            this.propertyTypeListBox.Location = new System.Drawing.Point(107, 10);
            this.propertyTypeListBox.Name = "propertyTypeListBox";
            this.propertyTypeListBox.Size = new System.Drawing.Size(135, 43);
            this.propertyTypeListBox.TabIndex = 55;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(14, 10);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(76, 13);
            this.label10.TabIndex = 53;
            this.label10.Text = "Property Type:";
            // 
            // propertyIdentifierTextBox
            // 
            this.propertyIdentifierTextBox.Location = new System.Drawing.Point(107, 135);
            this.propertyIdentifierTextBox.Name = "propertyIdentifierTextBox";
            this.propertyIdentifierTextBox.Size = new System.Drawing.Size(135, 20);
            this.propertyIdentifierTextBox.TabIndex = 54;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(2, 138);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(92, 13);
            this.label6.TabIndex = 52;
            this.label6.Text = "Property Identifier:";
            // 
            // uniqueIdentifierTextBox
            // 
            this.uniqueIdentifierTextBox.Location = new System.Drawing.Point(146, 171);
            this.uniqueIdentifierTextBox.Name = "uniqueIdentifierTextBox";
            this.uniqueIdentifierTextBox.Size = new System.Drawing.Size(407, 20);
            this.uniqueIdentifierTextBox.TabIndex = 60;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.descriptionTextBox);
            this.panel1.Controls.Add(this.DescriptionLabel);
            this.panel1.Controls.Add(this.currencyTextBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.parkingTextBox);
            this.panel1.Controls.Add(this.bedroomsTextBox);
            this.panel1.Controls.Add(this.bathroomsTextBox);
            this.panel1.Controls.Add(this.label22);
            this.panel1.Controls.Add(this.label21);
            this.panel1.Controls.Add(this.label20);
            this.panel1.Controls.Add(this.streetNameTextBox);
            this.panel1.Controls.Add(this.suburbTextBox);
            this.panel1.Controls.Add(this.streetIdentifierTextBox);
            this.panel1.Controls.Add(this.label15);
            this.panel1.Controls.Add(this.label14);
            this.panel1.Controls.Add(this.label13);
            this.panel1.Controls.Add(this.countryTextBox);
            this.panel1.Controls.Add(this.stateTextBox);
            this.panel1.Controls.Add(this.label19);
            this.panel1.Controls.Add(this.label18);
            this.panel1.Location = new System.Drawing.Point(248, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(465, 158);
            this.panel1.TabIndex = 61;
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Location = new System.Drawing.Point(74, 88);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(231, 20);
            this.descriptionTextBox.TabIndex = 74;
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.AutoSize = true;
            this.DescriptionLabel.Location = new System.Drawing.Point(6, 92);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(63, 13);
            this.DescriptionLabel.TabIndex = 73;
            this.DescriptionLabel.Text = "Description:";
            // 
            // currencyTextBox
            // 
            this.currencyTextBox.Location = new System.Drawing.Point(74, 10);
            this.currencyTextBox.Name = "currencyTextBox";
            this.currencyTextBox.Size = new System.Drawing.Size(31, 20);
            this.currencyTextBox.TabIndex = 72;
            this.currencyTextBox.Text = "AUD";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 71;
            this.label1.Text = "Currency:";
            // 
            // parkingTextBox
            // 
            this.parkingTextBox.Location = new System.Drawing.Point(412, 133);
            this.parkingTextBox.Name = "parkingTextBox";
            this.parkingTextBox.Size = new System.Drawing.Size(39, 20);
            this.parkingTextBox.TabIndex = 69;
            // 
            // bedroomsTextBox
            // 
            this.bedroomsTextBox.Location = new System.Drawing.Point(412, 109);
            this.bedroomsTextBox.Name = "bedroomsTextBox";
            this.bedroomsTextBox.Size = new System.Drawing.Size(39, 20);
            this.bedroomsTextBox.TabIndex = 68;
            // 
            // bathroomsTextBox
            // 
            this.bathroomsTextBox.Location = new System.Drawing.Point(412, 84);
            this.bathroomsTextBox.Name = "bathroomsTextBox";
            this.bathroomsTextBox.Size = new System.Drawing.Size(39, 20);
            this.bathroomsTextBox.TabIndex = 67;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(320, 88);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(60, 13);
            this.label22.TabIndex = 66;
            this.label22.Text = "Bathrooms:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(320, 113);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(57, 13);
            this.label21.TabIndex = 65;
            this.label21.Text = "Bedrooms:";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(320, 137);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(46, 13);
            this.label20.TabIndex = 64;
            this.label20.Text = "Parking:";
            // 
            // streetNameTextBox
            // 
            this.streetNameTextBox.Location = new System.Drawing.Point(316, 32);
            this.streetNameTextBox.Name = "streetNameTextBox";
            this.streetNameTextBox.Size = new System.Drawing.Size(135, 20);
            this.streetNameTextBox.TabIndex = 63;
            // 
            // suburbTextBox
            // 
            this.suburbTextBox.Location = new System.Drawing.Point(316, 58);
            this.suburbTextBox.Name = "suburbTextBox";
            this.suburbTextBox.Size = new System.Drawing.Size(135, 20);
            this.suburbTextBox.TabIndex = 62;
            // 
            // streetIdentifierTextBox
            // 
            this.streetIdentifierTextBox.Location = new System.Drawing.Point(316, 6);
            this.streetIdentifierTextBox.Name = "streetIdentifierTextBox";
            this.streetIdentifierTextBox.Size = new System.Drawing.Size(135, 20);
            this.streetIdentifierTextBox.TabIndex = 61;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(224, 62);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(44, 13);
            this.label15.TabIndex = 60;
            this.label15.Text = "Suburb:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(224, 35);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(69, 13);
            this.label14.TabIndex = 59;
            this.label14.Text = "Street Name:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(224, 10);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(81, 13);
            this.label13.TabIndex = 58;
            this.label13.Text = "Street Identifier:";
            // 
            // cityTextBox
            // 
            this.cityTextBox.Location = new System.Drawing.Point(107, 59);
            this.cityTextBox.Name = "cityTextBox";
            this.cityTextBox.Size = new System.Drawing.Size(135, 20);
            this.cityTextBox.TabIndex = 57;
            this.cityTextBox.Text = "Sydney";
            // 
            // postcodeTextBox
            // 
            this.postcodeTextBox.Location = new System.Drawing.Point(107, 111);
            this.postcodeTextBox.Name = "postcodeTextBox";
            this.postcodeTextBox.Size = new System.Drawing.Size(39, 20);
            this.postcodeTextBox.TabIndex = 56;
            // 
            // countryTextBox
            // 
            this.countryTextBox.Location = new System.Drawing.Point(74, 61);
            this.countryTextBox.Name = "countryTextBox";
            this.countryTextBox.Size = new System.Drawing.Size(135, 20);
            this.countryTextBox.TabIndex = 55;
            this.countryTextBox.Text = "Australia";
            // 
            // stateTextBox
            // 
            this.stateTextBox.Location = new System.Drawing.Point(74, 36);
            this.stateTextBox.Name = "stateTextBox";
            this.stateTextBox.Size = new System.Drawing.Size(77, 20);
            this.stateTextBox.TabIndex = 54;
            this.stateTextBox.Text = "NSW";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(6, 39);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(35, 13);
            this.label19.TabIndex = 53;
            this.label19.Text = "State:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(6, 65);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(46, 13);
            this.label18.TabIndex = 52;
            this.label18.Text = "Country:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(39, 114);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(55, 13);
            this.label17.TabIndex = 51;
            this.label17.Text = "Postcode:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(63, 63);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(27, 13);
            this.label16.TabIndex = 50;
            this.label16.Text = "City:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(564, 168);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(135, 24);
            this.button1.TabIndex = 70;
            this.button1.Text = "Create Propert Asset";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.CreatePropertyAssetClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(152, 155);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 62;
            this.label2.Text = "Unique Identifier:";
            // 
            // PropertyAssetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 197);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.uniqueIdentifierTextBox);
            this.Controls.Add(this.FindPropertyAssetButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.shortNameTextBox);
            this.Controls.Add(this.shortNameLabel);
            this.Controls.Add(this.propertyTypeListBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.propertyIdentifierTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cityTextBox);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.postcodeTextBox);
            this.Controls.Add(this.label17);
            this.Name = "PropertyAssetForm";
            this.Text = "PropertyAssetForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button FindPropertyAssetButton;
        private System.Windows.Forms.TextBox shortNameTextBox;
        private System.Windows.Forms.Label shortNameLabel;
        private System.Windows.Forms.ListBox propertyTypeListBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox propertyIdentifierTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox uniqueIdentifierTextBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox cityTextBox;
        private System.Windows.Forms.TextBox postcodeTextBox;
        private System.Windows.Forms.TextBox countryTextBox;
        private System.Windows.Forms.TextBox stateTextBox;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox streetNameTextBox;
        private System.Windows.Forms.TextBox suburbTextBox;
        private System.Windows.Forms.TextBox streetIdentifierTextBox;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox parkingTextBox;
        private System.Windows.Forms.TextBox bedroomsTextBox;
        private System.Windows.Forms.TextBox bathroomsTextBox;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox currencyTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.Label label2;
    }
}