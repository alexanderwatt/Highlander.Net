#region Usings

using System;
using Orion.Models.ForeignExchange.FxLeg;

#endregion

namespace Orion.Models.ForeignExchange.FxOptionLeg
{
    public enum FxOptionLegInstrumentMetrics
    {
        BaseCurrencyNPV
        , ForeignCurrencyNPV
    }

    public interface IFxOptionLegInstrumentResults : IFxLegInstrumentResults
    {
        ///// <summary>
        ///// Gets the NPV in reporting currency.
        ///// </summary>
        ///// <value>The NPV.</value>
        //Decimal NPV { get; }

        ///// <summary>
        ///// Gets the base currency NPV.
        ///// </summary>
        ///// <value>The base xcurrency NPV.</value>
        //Decimal BaseCurrencyNPV { get; }

        ///// <summary>
        ///// Gets the foreign currency NPV.
        ///// </summary>
        ///// <value>The foreign currency NPV.</value>
        //Decimal ForeignCurrencyNPV { get; }

    }
}