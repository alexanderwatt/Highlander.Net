﻿/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using System.Windows.Forms;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.NamedValues;

namespace Highlander.Reporting.UI.V5r3
{
    public partial class CreateTradeForm : Form
    {
        private readonly PricingCache _pricingCache;

        public CreateTradeForm(PricingCache pricingCache)
        {
            InitializeComponent();
            _pricingCache = pricingCache;
            StartUp();
        }

        private void StartUp()
        {
            //Instantiate the data grid
            //
            propertyDataGridView.Columns.Add("PropertyName", "Name");
            propertyDataGridView.Columns.Add("PropertyValue", "Value");
            object[] row1 = { EnvironmentProp.NameSpace, EnvironmentProp.DefaultNameSpace };
            propertyDataGridView.Rows.Add(row1);
            object[] row2 = { EnvironmentProp.SourceSystem, TradeSourceType.Internal };
            propertyDataGridView.Rows.Add(row2);
            tradeTypeBox.SelectedIndex = 0;
            propertyTypeListBox.SelectedIndex = 0;
        }

        private void CreateTradeButtonClick(object sender, EventArgs e)
        {
            //Populate the properties
            var properties = new NamedValueSet();
            if (propertyDataGridView.Columns.Count > 1)
            {
                for (var i = 0; i < propertyDataGridView.Rows.Count; i++)
                {
                    var columnName1 = propertyDataGridView.Columns[0].Name;
                    var columnName2 = propertyDataGridView.Columns[1].Name;
                    if (propertyDataGridView[columnName1, i].Value == null || propertyDataGridView[columnName2, i].Value == null) continue;
                    var namedValue = new NamedValue(propertyDataGridView[columnName1, i].Value.ToString(), propertyDataGridView[columnName2, i].Value);
                    properties.Set(namedValue);
                }
            }
            //Create the transaction
            var transactionType = (ProductTypeSimpleEnum)Enum.Parse(typeof(ProductTypeSimpleEnum), tradeTypeBox.Text, true);
            switch (transactionType)
            {
                case ProductTypeSimpleEnum.LeaseTransaction:
                    // _pricingCache.CreateLeaseTrade();
                    break;
                case ProductTypeSimpleEnum.PropertyTransaction:
                    _pricingCache.CreatePropertyTradeWithProperties(tradeIdentifierTxtBox.Text, isParty1BuyerCheckBox.Checked, Party1TextBox.Text, 
                        Party2TextBox.Text, startDateTimePicker.Value, effectiveDateTimePicker.Value, Convert.ToDecimal(purchaseAmountTextBox.Text),
                        paymentDateTimePicker.Value, null, currencyListBox.Text, propertyAssetIdentifierTextBox.Text, accountingBookTextBox.Text, properties);
                    break;
            }
        }

        private void CreatePropertyAssetClick(object sender, EventArgs e)
        {
            //Load the form
            //
            var frm = new PropertyAssetForm(_pricingCache);
            frm.ShowDialog();
        }

        private void FindPropertyAssetButtonClick(object sender, EventArgs e)
        {
            var propertyType = EnumHelper.Parse<PropertyType>(propertyTypeListBox.Text);
            var instrument = _pricingCache.GetPropertyAsset(propertyType, cityTextBox.Text, shortNameTextBox.Text,
                postcodeTextBox.Text, propertyIdentifierTextBox.Text);
            if (instrument is null)
            {
                propertyAssetIdentifierTextBox.Text = "Non-existent Property!";
                return;
            }

            var properties = instrument.AppProps;
            var id = properties.GetString(PropertyProp.UniqueIdentifier, false);
            if (id != null)
            {
                propertyAssetIdentifierTextBox.Text = id;
            }

            if (!(instrument.Data is PropertyNodeStruct propertyAsset)) return;
            propertyTypeListBox.Text = propertyAsset.Property?.propertyTaxonomyType;
        }
    }
}
