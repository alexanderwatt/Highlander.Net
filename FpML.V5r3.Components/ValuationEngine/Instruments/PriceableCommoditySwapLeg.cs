/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Instruments.Commodities;
using Orion.Models.Commodities.CommoditySwapLeg;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public abstract class PriceableCommoditySwapLeg : InstrumentControllerBase, IPriceableCommoditySwapLeg<ICommoditySwapLegParameters, ICommoditySwapLegInstrumentResults>
    {
        #region Member Fields

        protected const string CModelIdentifier = "CommoditySwapLeg";

        #endregion

        #region Public Fields

        // Analytics
        public IModelAnalytic<ICommoditySwapLegParameters, CommoditySwapLegInstrumentMetrics> AnalyticsModel { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public ICommoditySwapLegParameters AnalyticModelParameters { get; protected set; }  

        // Requirements for pricing
        /// <summary>
        /// The first currency.
        /// </summary>
        public Currency Currency { get; set; }

        /// <summary>
        /// THe discount curve.
        /// </summary>
        public string DiscountCurveName { get; set; }

        /// <summary>
        /// THe payment.
        /// </summary>
        public InstrumentControllerBase PriceablePayment { get; set; }

        /// <summary>
        /// The underlying payments for a delivered fx transaction.
        /// </summary>
        public IList<PriceablePayment> Payments { get; set; }

        /// <summary>
        /// Gets the last calculation results.
        /// </summary>
        /// <value>The last results.</value>
        public ICommoditySwapLegInstrumentResults CalculationResults { get; protected set; }

        /// <summary>
        /// The HybridValuation flag.
        /// </summary>
        public bool HybridValuation { get; protected set; }

        /// <summary>
        /// The IsNonDeliverableForward flag.
        /// </summary>
        public bool IsDeliverable { get; set; }

        /// <summary>
        /// The settlement currency.
        /// </summary>
        public Currency SettlementCurrency { get; set; }

        #endregion

        #region Constructors

        protected PriceableCommoditySwapLeg()
        {
            Multiplier = 1.0m;
            AnalyticsModel = new CommoditySwapLegAnalytic();
        }

        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public abstract CommoditySwapLeg Build();
       
        #endregion

        #region Static Helpers

        internal static IList<InstrumentControllerBase> MapPayments(IList<PriceablePayment> payments)
        {
            return payments.Cast<InstrumentControllerBase>().ToList();
        }

        #endregion

        #region Overrides of InstrumentControllerBase

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return MapPayments(Payments);
        }

        #endregion
    }
}