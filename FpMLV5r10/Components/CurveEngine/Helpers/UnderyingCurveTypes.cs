namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// Underlying curve types.
    /// </summary>
    public enum UnderyingCurveTypes
    {
        /// <summary>
        /// A discount factor curve.
        /// </summary>
        DiscountFactorCurve,

        /// <summary>
        /// A zero curve.
        /// </summary>
        ZeroCurve,

        /// <summary>
        /// A zero curve.
        /// </summary>
        ZeroSpreadCurve,

        /// <summary>
        /// A forward curve.
        /// </summary>
        ForwardCurve,

        /// <summary>
        /// A volatility curve.
        /// </summary>
        VolatilityCurve
    }
}