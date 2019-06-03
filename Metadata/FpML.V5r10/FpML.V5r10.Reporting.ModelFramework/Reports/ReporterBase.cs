#region Using directives

using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using Orion.Util.NamedValues;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.Reports
{
    public abstract class ReporterBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priceable"></param>
        /// <returns></returns>
        public abstract object DoReport(InstrumentControllerBase priceable);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pricer"></param>
        /// <returns></returns>
        public abstract object[,] DoXLReport(InstrumentControllerBase pricer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public abstract object[,] DoReport(Product product, NamedValueSet properties);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pricer"></param>
        /// <returns></returns>
        public abstract List<object[]> DoExpectedCashflowReport(InstrumentControllerBase pricer);
    }
}
