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
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Instruments;
using Orion.Models.Commodities.CommoditySwapLeg;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableCommodityFixedLeg : PriceableCommoditySwapLeg
    {

        #region Public Fields

        public NonPeriodicFixedPriceLeg FixedLeg { get; set; }

        #endregion

        #region Constructors

        protected PriceableCommodityFixedLeg(NonPeriodicFixedPriceLeg fixedLeg)
        {
            Multiplier = 1.0m;
            AnalyticsModel = new CommoditySwapLegAnalytic();
            FixedLeg = fixedLeg;
        }

        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public override CommoditySwapLeg Build()
        {
            return new NonPeriodicFixedPriceLeg();
        }
       
        #endregion

        #region Static Helpers


        #endregion

        #region Overrides of InstrumentControllerBase

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketing.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketingInterval)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            throw new NotImplementedException();
        }

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