﻿using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.ModelFramework.Calibrators.Bootstrappers;
using FpML.V5r10.Reporting.ModelFramework.Identifiers;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// Base Model Controller class from which all Instrument controllers/models should be extended
    /// </summary>
    public abstract class PricingStructureBase : IPricingStructure
    {
        /// <summary>
        /// The pricing structure.
        /// </summary>
        public PricingStructure PricingStructure { get; set; }

        /// <summary>
        /// The pricing structure id.
        /// </summary>
        ///<returns>IIdentifier</returns>
        public IPricingStructureIdentifier PricingStructureIdentifier { get; set; }

        /// <summary>
        /// The pricing structure Valuation
        /// </summary>
        public PricingStructureValuation PricingStructureValuation { get; set; }

        ///<summary>
        /// The type of curve evolution to use. The defualt is ForwardToSpot
        ///</summary>
        public PricingStructureEvolutionType PricingStructureEvolutionType { get; set; }

        ///<summary>
        /// The curve data.
        ///</summary>
        public PricingStructureData PricingStructureData { get; set; }

        /// <summary>
        /// The interpolator
        /// </summary>
        public IInterpolatedSpace Interpolator { get; set; }

        protected IBootstrapController<IBootstrapControllerData, IPricingStructure> Bootstrapper { get; set; }

        protected PricingStructureBase()
        {
            PricingStructureEvolutionType = PricingStructureEvolutionType.ForwardToSpot;
        }

        /// <summary>
        /// The base class for pricing structures.
        /// </summary>
        /// <param name="pricingStructure"></param>
        protected PricingStructureBase(Pair<PricingStructure, PricingStructureValuation> pricingStructure)
        {
            if (pricingStructure == null) return;
            PricingStructure = pricingStructure.First;
            PricingStructureValuation = pricingStructure.Second;
            PricingStructureEvolutionType = PricingStructureEvolutionType.ForwardToSpot;
        }

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>The <c>double</c> function value at the point</returns>
        public virtual double Value(IPoint point)
        {
            return Interpolator.Value(point);
        }

        /// <summary>
        /// This returns the point function used to add continuity to the discrete space.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in
        /// mathematical functions applied to curves and surfaces.
        /// </summary>
        /// <returns>
        /// 	<c>int</c> The number of <c>int</c> points.
        /// </returns>
        public IInterpolation GetInterpolatingFunction()
        {
            return Interpolator.GetInterpolatingFunction();
        }

        /// <summary>
        /// Gets the nunderlying discrete space that is used for interpolation.
        /// </summary>
        /// <returns></returns>
        public IDiscreteSpace GetDiscreteSpace()
        {
            return Interpolator.GetDiscreteSpace();
        }

        /// <summary>
        /// Is extrapolation allowed?
        /// </summary>
        /// <returns></returns>
        public bool AllowExtrapolation()
        {
            return Interpolator.AllowExtrapolation();
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <returns></returns>
        public abstract IValue GetValue(IPoint pt);

        /// <summary>
        /// Gets the base date of the pricing structure..
        /// </summary>
        /// <returns></returns>
        public DateTime GetBaseDate()
        {
            return PricingStructureValuation != null ? PricingStructureValuation.baseDate.Value : DateTime.MinValue;
        }

        /// <summary>
        /// Gets the spat date of the pricing structure..
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetSpotDate()
        {
            return PricingStructureValuation != null ? PricingStructureValuation.spotDate.Value : DateTime.MinValue;
        }

        /// <summary>
        /// The pricing structure id.
        /// </summary>
        ///<returns>IIdentifier</returns>
        public IIdentifier GetPricingStructureId()
        {
            return PricingStructureIdentifier;
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public abstract IList<IValue> GetClosestValues(IPoint pt);

        /// <summary>
        /// The interpolated space
        /// </summary>
        public IInterpolatedSpace GetInterpolatedSpace()
        {
            return Interpolator;
        }

        /// <summary>
        /// The pricing strucutre properties.
        /// </summary>
        public NamedValueSet GetProperties()
        {
            return PricingStructureIdentifier.Properties;
        }

        /// <summary>
        /// Gets the fpML data.
        /// </summary>
        /// <returns></returns>
        public Pair<PricingStructure, PricingStructureValuation> GetFpMLData()
        {
            return new Pair<PricingStructure, PricingStructureValuation>(PricingStructure, PricingStructureValuation);
        }

        /// <summary>
        /// Gets the fpML data.
        /// </summary>
        /// <returns></returns>
        public Market GetMarket()
        {
            var market = new Market
            {
                id = PricingStructureIdentifier.UniqueIdentifier,
                Items = new PricingStructure[1],
                Items1 = new PricingStructureValuation[1]
            };
            market.Items[0] = PricingStructure;
            market.Items1[0] = PricingStructureValuation;

            return market;
        }

        ///<summary>
        /// Gets the algorithm.
        ///</summary>
        ///<returns></returns>
        public abstract string GetAlgorithm();

        /// <summary>
        /// Creates a perturbed copy of the curve.
        /// </summary>
        /// <returns></returns>
        public Pair<NamedValueSet, Market> GetPerturbedCopy(Decimal[] values)
        {
            throw new NotImplementedException();
        }
    }
}