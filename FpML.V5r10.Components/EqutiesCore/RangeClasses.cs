using System;

namespace Orion.EquitiesCore
{

    /// <summary>
    /// Zero Curve
    /// </summary>
    [Serializable]
    public class ZeroCurveRange
    {
        /// <summary>
        /// rate date
        /// </summary>
        public DateTime RateDate;
        /// <summary>
        /// Rate in %
        /// </summary>
        public double Rate;
    }

    /// <summary>
    /// Dividend Range
    /// </summary>
    [Serializable]
    public class DividendRange
    {
        /// <summary>
        /// Dividend date
        /// </summary>
        public DateTime DivDate;
        /// <summary>
        /// Dividend amount
        /// </summary>
        public double DivAmt;
    }

    /// <summary>
    /// Wing Curve Parameters
    /// </summary>
    [Serializable]
    public class WingParamsRange
    {
        /// <summary>
        /// Current vol
        /// </summary>
        public double CurrentVolatility;
        /// <summary>
        /// Slope reference
        /// </summary>
        public double SlopeReference;
        /// <summary>
        /// Put curvature
        /// </summary>
        public double PutCurvature;
        /// <summary>
        /// Call curvature
        /// </summary>
        public double CallCurvature;
        /// <summary>
        /// Down cut off
        /// </summary>
        public double DownCutOff;
        /// <summary>
        /// Up cut off
        /// </summary>
        public double UpCutOff;
        /// <summary>
        /// Volatility change rate
        /// </summary>
        public double VolChangeRate;
        /// <summary>
        /// Slope change rate
        /// </summary>
        public double SlopeChangeRate;
        /// <summary>
        /// Swimmingness
        /// </summary>
        public double SkewSwimmingnessRate;
        /// <summary>
        /// Down smoothing range
        /// </summary>
        public double DownSmoothingRange;
        /// <summary>
        /// Up smoothing range
        /// </summary>
        public double UpSmoothingRange;
        /// <summary>
        /// Ref forward
        /// </summary>
        public double ReferenceForward;
    }   
}
