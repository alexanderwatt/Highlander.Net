using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Highlander.Constants;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.NamedValues;

namespace Highlander.Reporting.UI.V5r3
{
    public partial class PropertyAssetForm : Form
    {
        private readonly PricingCache _pricingCache;

        private readonly NamedValueSet _properties;

        public PropertyAssetForm(PricingCache pricingCache, NamedValueSet properties)
        {
            InitializeComponent();
            _pricingCache = pricingCache;
            _properties = properties;
        }

        private void CreatePropertyAssetClick(object sender, EventArgs e)
        {
            var propertyType = _properties.GetValue<PropertyType>(PropertyProp.PropertyType, true);
            var city = _properties.GetString(PropertyProp.City, false);
            var shortName = _properties.GetString(PropertyProp.ShortName, false);
            var postcode = _properties.GetString(PropertyProp.PostCode, false);
            var propertyIdentifier = _properties.GetString(PropertyProp.PropertyIdentifier, false);
            propertyAssetIdentifierTextBox.Text = _pricingCache.CreatePropertyAsset(propertyIdentifier, propertyType, shortName, streetIdentifierTextBox.Text, streetNameTextBox.Text,
               suburbTextBox.Text, city, postcode, stateTextBox.Text, countryTextBox.Text, bedroomsTextBox.Text, bathroomsTextBox.Text,
                parkingTextBox.Text, currencyTextBox.Text, descriptionTextBox.Text, _properties);
        }
    }
}
