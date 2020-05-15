using Highlander.Constants;

namespace Highlander.Web.API.V5r3.Models
{
    public class CurveUpdateInputsViewModel
    {
        public string PropertyId { get; set; }
        public PropertyType PropertyType { get; set; }
        public string ShortName { get; set; }
        public string StreetIdentifier { get; set; }
        public string StreetName { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public int? NumBedrooms { get; set; } = 0;
        public int? NumBathrooms { get; set; } = 0;
        public int? NumParking { get; set; } = 0;
        public string Currency { get; set; }
        public string Description { get; set; }
    }
}