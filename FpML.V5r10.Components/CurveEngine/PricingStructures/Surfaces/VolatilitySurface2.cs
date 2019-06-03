#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Util.Helpers;
using Orion.CurveEngine.PricingStructures.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Surfaces
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class VolatilitySurface2 : PricingStructureBase, IVolatilitySurface2
    {
        ///<summary>
        ///</summary>
        ///<param name="fpmlData"></param>
        public VolatilitySurface2(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            // unpack data and create interpolator around it
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
        }


        ///<summary>
        ///</summary>
        ///<param name="pt"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public override IValue GetValue(IPoint pt)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="pt"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public override IList<IValue> GetClosestValues(IPoint pt)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="dimension1"></param>
        ///<param name="dimension2"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public double GetValue(double dimension1, double dimension2)
        {
            var dataPoints = GetDataPoints();
            return VolatilitySurfaceHelper.GetValue(dataPoints, dimension1, dimension2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object[,] GetSurface()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expirationAsDate"></param>
        /// <param name="strike"></param>
        /// <returns></returns>
        public double GetValueByExpirationAndStrike(DateTime expirationAsDate, double strike)
        {
            var dataPoints = GetDataPoints();
            var baseDate = PricingStructureValuation.baseDate.Value;
            double dimension1 = (expirationAsDate - baseDate).TotalDays / 365.0;          
            return VolatilitySurfaceHelper.GetValue(dataPoints, dimension1, strike);
        }

        private IEnumerable<PricingStructurePoint> GetDataPoints()
        {
            return ((VolatilityMatrix)PricingStructureValuation).dataPoints.point;
        }


        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public override string GetAlgorithm()
        {
            throw new NotImplementedException();
        }
    }
}