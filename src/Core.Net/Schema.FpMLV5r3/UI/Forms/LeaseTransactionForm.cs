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

        private Lease _lease;

        public LeaseTransactionForm(PricingCache pricingCache, NamedValueSet properties)
        {
            InitializeComponent();
            _pricingCache = pricingCache;
            _properties = properties;
            currencyListBox.SelectedIndex = 0;
            ReviewFrequencyListBox.SelectedIndex = 0;
            rollConventionTextBox.Text = @"EOM";
            businessCalendarListBox.SelectedIndex = 0;
            businessDayAdjustmentsListBox.SelectedIndex = 0;
            propertyIdentifierTextBox.Text = _properties?.GetString(LeaseProp.ReferencePropertyIdentifier, true);
        }

        private void CreateLeaseTradeButton_Click(object sender, EventArgs e)
        {
            //Create the transaction
            var properties = new NamedValueSet();
            properties.Set(LeaseProp.RollConvention, rollConventionTextBox.Text);
            properties.Set(LeaseProp.BusinessDayCalendar, businessCalendarListBox.Text);
            properties.Set(LeaseProp.BusinessDayAdjustments, businessDayAdjustmentsListBox.Text);
            properties.Set(LeaseProp.UpfrontAmount, 0.0m);
            properties.Set(LeaseProp.LeaseType, leaseTypeListBox.Text);
            //properties.Set(LeaseProp.Area, 0.0m);
            //properties.Set(LeaseProp.UnitsOfArea, "sqm");
            properties.Set(LeaseProp.ReviewFrequency, ReviewFrequencyListBox.Text);
            properties.Set(LeaseProp.NextReviewDate, startDateTimePicker.Value.AddYears(1));
            properties.Set(LeaseProp.ReviewChange, reviewAmountUpDown.Value);
            leaseTradeIdentifierTextBox.Text = _pricingCache.CreateLeaseTradeWithProperties(leaseIdentifierTxtBox.Text, isParty1TenantCheckBox.Checked, Party1TextBox.Text,
                     Party2TextBox.Text, tradeDateTimePicker.Value, startDateTimePicker.Value, currencyListBox.Text, portfolioTextBox.Text, Convert.ToDecimal(purchaseAmountTextBox.Text),
                     leaseIdentifierTxtBox.Text, expiryDateTimePicker.Value, propertyIdentifierTextBox.Text, descriptionTextBox.Text, _properties);
        }

        private void findLeaseAssetButton_Click(object sender, EventArgs e)
        {
            var instrument = _pricingCache.GetAssetConfigurationData(currencyListBox.Text, AssetTypesEnum.Lease.ToString(), leaseTypeListBox.Text);
            if(instrument != null && instrument.InstrumentNodeItem is LeaseNodeStruct leaseStruct)
            {
                _lease = leaseStruct.Lease;
                FrequencyListBox.Text = _lease.paymentFrequency.ToFrequency().ToString();
                leaseAssetIdentifierTextBox.Text = _lease.leaseIdentifier;
                //TODO set other data
            }
        }
    }
}
