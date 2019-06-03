

namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Black-Scholes diffusion process class.
    /// </summary>
    /// <remarks>
    /// This class describes the stochastic process governed by 
    /// \f[
    /// 	dS = (r - \frac{\sigma^2}{2}) dt + \sigma dW_t.
    /// \f]
    /// </remarks>
    public class BlackScholesProcess : DiffusionProcess 
    {
        ///<summary>
        ///</summary>
        ///<param name="rate"></param>
        ///<param name="volatility"></param>
        public BlackScholesProcess(double rate, double volatility)
            : this(rate, volatility, 0.0)
        {}

        ///<summary>
        ///</summary>
        ///<param name="rate"></param>
        ///<param name="volatility"></param>
        ///<param name="s0"></param>
        public BlackScholesProcess(double rate, double volatility, double s0)
            : base(s0)
        {
            _rate = rate;
            _sigma = volatility;
        }

        private readonly double _rate;
        private readonly double _sigma;

        public override double Drift(double time, double x)
        {
            return _rate - 0.5*_sigma*_sigma;
        }
        public override double Diffusion(double time, double x)
        {
            return _sigma;
        }
    }
}