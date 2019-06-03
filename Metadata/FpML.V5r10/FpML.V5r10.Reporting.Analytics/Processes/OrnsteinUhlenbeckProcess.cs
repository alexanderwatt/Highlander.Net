using System;
// COM interop attributes
// some useful attributes

namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Ornstein-Uhlenbeck process class.
    /// </summary>
    /// <remarks>
    /// This class describes the Ornstein-Uhlenbeck process governed by 
    /// \f[
    /// 	dx = -a x_t dt + \sigma dW_t.
    /// \f]
    /// </remarks>
    public class OrnsteinUhlenbeckProcess : DiffusionProcess 
    {
        ///<summary>
        ///</summary>
        ///<param name="speed"></param>
        ///<param name="volatility"></param>
        public OrnsteinUhlenbeckProcess(double speed, double volatility)
            : this(speed, volatility, 0.0)
        {}
        ///<summary>
        ///</summary>
        ///<param name="speed"></param>
        ///<param name="volatility"></param>
        ///<param name="x0"></param>
        public OrnsteinUhlenbeckProcess(double speed, double volatility, 
                                        double x0) : base(x0)
        {
            _speed = speed;
            _volatility = volatility;
        }

        private readonly double _speed;
        private readonly double _volatility;

        public override double Drift(double time, double x)
        {
            return - _speed*x;
        }
        public override double Diffusion(double time, double x)
        {
            return _volatility;
        }

        public override double Expectation(double t0, double x0, double dt)
        {
            return x0*Math.Exp(-_speed*dt);
        }

        public override double Variance(double t0, double x0, double dt)
        {
            return 0.5 * _volatility*_volatility / _speed *
                   ( 1.0 - Math.Exp(-2.0*_speed*dt) );
        }
    };
}