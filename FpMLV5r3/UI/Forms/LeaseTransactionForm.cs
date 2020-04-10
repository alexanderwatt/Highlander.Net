using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Highlander.Core.Interface.V5r3;
using Highlander.Codes.V5r3;
using Highlander.Utilities.NamedValues;
using Highlander.Constants;
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.UI.V5r3
{
    public partial class LeaseTransactionForm : Form
    {
        private readonly PricingCache _pricingCache;

        private readonly NamedValueSet _properties;

        public LeaseTransactionForm(PricingCache pricingCache, NamedValueSet properties)
        {
            InitializeComponent();
            _pricingCache = pricingCache;
            _properties = properties;
            currencyListBox.SelectedIndex = 0;
        }

        private void CreateLeaseTradeButton_Click(object sender, EventArgs e)
        {
            //Create the transaction

            leaseTradeIdentifierTextBox.Text = _pricingCache.CreateLeaseTradeWithProperties(leaseIdentifierTxtBox.Text, isParty1TenantCheckBox.Checked, Party1TextBox.Text,
                     Party2TextBox.Text, tradeDateTimePicker.Value, startDateTimePicker.Value, currencyListBox.Text, portfolioTextBox.Text, Convert.ToDecimal(purchaseAmountTextBox.Text),
                     leaseIdentifierTxtBox.Text, expiryDateTimePicker.Value, leaseAssetIdentifierTextBox.Text, descriptionTextBox.Text, _properties);
        }

        private void findLeaseAssetButton_Click(object sender, EventArgs e)
        {
            var intrument = _pricingCache.GetAssetConfigurationData(currencyListBox.Text, AssetTypesEnum.Lease.ToString(), leaseTypeListBox.Text);
            if(intrument != null && intrument.InstrumentNodeItem is LeaseNodeStruct leaseStruct)
            {
                var lease = leaseStruct.Lease;
                FrequencyListBox.Text = lease.paymentFrequency.ToFrequency().ToString();
                leaseAssetIdentifierTextBox.Text = lease.id;
                //TODO set other data
            }
        }
    }
}
