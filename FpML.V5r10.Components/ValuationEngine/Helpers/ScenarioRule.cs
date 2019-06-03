using Orion.Util.NamedValues;
using FpML.V5r10.Reporting;

namespace Orion.ValuationEngine.Helpers
{
    public class CachedSummary
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string UniqueId;

        /// <summary>
        /// 
        /// </summary>
        public readonly NamedValueSet Properties;

        /// <summary>
        /// 
        /// </summary>
        public readonly ValuationReport Summary;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="properties"></param>
        /// <param name="summary"></param>
        public CachedSummary(string uniqueId, NamedValueSet properties, ValuationReport summary)
        {
            UniqueId = uniqueId;
            Properties = properties;
            Summary = summary;
        }
    }
}
