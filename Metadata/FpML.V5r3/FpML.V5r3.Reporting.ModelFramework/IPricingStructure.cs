/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework
{
    ///<summary>
    /// The reference data for the curve.
    ///</summary>
    [Serializable]
    public class PricingStructureData
    {
        ///<summary>
        /// The curve type.
        ///</summary>
        public CurveType CurveType  { get; protected set; }

        ///<summary>
        /// The asset class.
        ///</summary>
        public Constants.AssetClass AssetClass { get; protected set; }

        /// <summary>
        /// The risk curve perturbation type: Parent, Child or Hybrid.
        /// If the Curve type is Parent then this is always Parent.
        /// If the curve type is child, then it can be Parent: perturbs the parent asset inputs;
        /// Child: perturbs the child assets or Hybrid, which is both - meaning the sensitivities may double count.
        /// </summary>
        public PricingStructureRiskSetType PricingStructureRiskSetType { get; set; }

        /// <summary>
        /// Stores all relevant curve data.
        /// </summary>
        /// <param name="curveType">The curve type: Parent or Child</param>
        /// <param name="assetClass">The asset class: fx, rates etc.</param>
        public PricingStructureData(CurveType curveType, Constants.AssetClass assetClass)
        {
            CurveType = curveType;
            AssetClass = assetClass;
            PricingStructureRiskSetType = PricingStructureRiskSetType.Parent;
        }

        /// <summary>
        /// Stores all relevant curve data.
        /// </summary>
        /// <param name="curveType">The curve type: Parent or Child</param>
        /// <param name="assetClass">The asset class: fx, rates etc.</param>
        /// <param name="properties">The properties for that curve. </param>
        public PricingStructureData(CurveType curveType, Constants.AssetClass assetClass, NamedValueSet properties)
        {
            CurveType = curveType;
            AssetClass = assetClass;
            PricingStructureRiskSetType = PricingStructureRiskSetType.Parent;
            try
            {
                var pricingStructureRiskSetType = properties.GetValue<string>("PricingStructureRiskSetType", false);
                if (pricingStructureRiskSetType != null)
                {
                    var result = EnumHelper.Parse<PricingStructureRiskSetType>(pricingStructureRiskSetType);
                    PricingStructureRiskSetType = result;
                }
            }
            catch (System.Exception)
            {
                var exception = new System.Exception("PricingStructureRiskSetType does not exist");
                throw exception;
            }
        }
    }

    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface IPricingStructure : IInterpolatedSpace
    {
        ///<summary>
        /// The type of curve evolution to use. The default is ForwardToSpot
        ///</summary>
        PricingStructureEvolutionType PricingStructureEvolutionType { get; set; }

        ///<summary>
        /// The curve data.
        ///</summary>
        PricingStructureData PricingStructureData { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <returns></returns>
        IValue GetValue(IPoint pt);

        /// <summary>
        /// Gets the base date of the pricing structure..
        /// </summary>
        /// <returns></returns>
        DateTime GetBaseDate();

        /// <summary>
        /// Gets the sp0t date of the pricing structure..
        /// </summary>
        /// <returns></returns>
        DateTime GetSpotDate();

        /// <summary>
        /// The pricing structure id.
        /// </summary>
        ///<returns>IIdentifier</returns>
        IIdentifier GetPricingStructureId();

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        IList<IValue> GetClosestValues(IPoint pt);

        /// <summary>
        /// The interpolated space
        /// </summary>
        IInterpolatedSpace GetInterpolatedSpace();

        /// <summary>
        /// Gets the fp ML data.
        /// </summary>
        /// <returns></returns>
        Pair<PricingStructure, PricingStructureValuation> GetFpMLData();

        ///<summary>
        /// Creates and returns The FpML Market
        ///</summary>
        ///<returns></returns>
        Market GetMarket();

        /// <summary>
        /// Creates a perturbed copy of the curve.
        /// </summary>
        /// <returns></returns>
        Pair<NamedValueSet, Market> GetPerturbedCopy(Decimal[] values);
    }
    


}
