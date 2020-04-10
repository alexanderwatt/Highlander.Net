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

namespace Highlander.Reporting.UI.V5r3
{
    public partial class PropertyAssetForm : Form
    {
        private readonly PricingCache _pricingCache;

        public PropertyAssetForm(PricingCache pricingCache)
        {
            InitializeComponent();
            _pricingCache = pricingCache;
            propertyTypeListBox.SelectedIndex = 0;
        }

        private void FindPropertyAssetButtonClick(object sender, EventArgs e)
        {
            var propertyType = EnumHelper.Parse<PropertyType>(propertyTypeListBox.Text);
            var instrument = _pricingCache.GetPropertyAsset(propertyType, cityTextBox.Text, shortNameTextBox.Text,
                postcodeTextBox.Text, propertyIdentifierTextBox.Text);
            if (instrument is null)
            {
                uniqueIdentifierTextBox.Text = "Non-existent Property!"; 
                return;
            }

            var properties = instrument.AppProps;
            var id = properties.GetString(PropertyProp.UniqueIdentifier, false);
            if (id != null)
            {
                uniqueIdentifierTextBox.Text = id;
            }

            if (!(instrument.Data is PropertyNodeStruct propertyAsset)) return;
            propertyTypeListBox.Text = propertyAsset.Property?.propertyTaxonomyType;
            var address = propertyAsset.Property?.propertyAddress?.streetAddress;
            if (address != null)
            {
                var length = address.Length;

                if (length > 0 && string.IsNullOrEmpty(address[0]))
                {
                    streetIdentifierTextBox.Text = address[0];
                }

                if (length > 1 && string.IsNullOrEmpty(address[1]))
                {
                    streetNameTextBox.Text = address[1];
                }

                if (length > 2 && string.IsNullOrEmpty(address[2]))
                {
                    suburbTextBox.Text = address[2];
                }
            }
            cityTextBox.Text = propertyAsset.Property?.propertyAddress?.city;
            postcodeTextBox.Text = propertyAsset.Property?.propertyAddress?.postalCode;
            stateTextBox.Text = propertyAsset.Property?.propertyAddress?.state;
            countryTextBox.Text = propertyAsset.Property?.propertyAddress?.country.Value;
            bedroomsTextBox.Text = propertyAsset.Property?.bedrooms;
            bathroomsTextBox.Text = propertyAsset.Property?.bathrooms;
            parkingTextBox.Text = propertyAsset.Property?.parking;
            descriptionTextBox.Text = propertyAsset.Property?.description;
        } 

        private void CreatePropertyAssetClick(object sender, EventArgs e)
        {
            var propertyType = EnumHelper.Parse<PropertyType>(propertyTypeListBox.Text);
            uniqueIdentifierTextBox.Text = _pricingCache.CreatePropertyAsset(propertyIdentifierTextBox.Text, propertyType, shortNameTextBox.Text, streetIdentifierTextBox.Text, streetNameTextBox.Text,
               suburbTextBox.Text, cityTextBox.Text, postcodeTextBox.Text, stateTextBox.Text, countryTextBox.Text, bedroomsTextBox.Text, bathroomsTextBox.Text,
                parkingTextBox.Text, currencyTextBox.Text, descriptionTextBox.Text, null);
        }
    }
}
