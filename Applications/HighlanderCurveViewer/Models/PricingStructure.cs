using System;
using System.ComponentModel.DataAnnotations;

namespace Highlander.CurveViewer.Models
{
    public class PricingStructure
    {
        public string UniqueIdentifier { get; set; }

        public string PricingStructureType { get; set; }

        [DataType(DataType.Date)]
        public DateTime BuildDate { get; set; }

        public string Market { get; set; }

        public string IndexName { get; set; }
    }
}