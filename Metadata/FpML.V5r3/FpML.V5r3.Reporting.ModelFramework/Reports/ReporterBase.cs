#region Using directives

using System.Collections.Generic;
using Orion.ModelFramework.Instruments;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Reports
{
    public abstract class ReporterBase
    {
        public abstract object DoReport(InstrumentControllerBase priceable);

        public abstract object[,] DoXLReport(InstrumentControllerBase pricer);

        public abstract object[,] DoReport(Product product, NamedValueSet properties);

        public abstract List<object[]> DoExpectedCashflowReport(InstrumentControllerBase pricer);

    }
}
