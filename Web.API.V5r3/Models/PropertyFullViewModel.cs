using System.Collections.Generic;

namespace Highlander.Web.API.V5r3.Models
{
    public class PropertyFullViewModel : PropertyAssetViewModel
    {
        public PropertyTradeViewModel Trade { get; set; }
        public IEnumerable<LeaseTradeViewModel> Leases { get; set; }
    }
}