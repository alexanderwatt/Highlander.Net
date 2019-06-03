using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;

namespace Orion.ValuationEngine.Helpers
{
    public class CachedSummary
    {
        public readonly string UniqueId;
        public readonly NamedValueSet Properties;
        public readonly ValuationReport Summary;
        public CachedSummary(string uniqueId, NamedValueSet properties, ValuationReport summary)
        {
            UniqueId = uniqueId;
            Properties = properties;
            Summary = summary;
        }
    }
}
