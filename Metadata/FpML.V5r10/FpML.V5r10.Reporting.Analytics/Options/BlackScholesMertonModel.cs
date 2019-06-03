using System;
using System.Collections.Generic;
using Orion.Analytics.Distributions;

namespace Orion.Analytics.Options
{
    /// <summary>
    /// Black model functions.
    /// </summary>
    public class BlackScholesMertonModel
    {
        // Constant version of 1 / Math.Sqrt(2 * Math.PI)
        private const double OneOverRootTwoPi = 0.3989422804014327;

        ///<summary>
        /// The black-scholes option value.
        ///</summary>
        public double Value { get; set; }

        ///<summary>
        /// The black-scholes spot option delta.
        ///</summary>
        public double SpotDelta { get; set; }

        ///<summary>
        /// The black-scholes forward option delta.
        ///</summary>
        public double ForwardDelta { get; set; }

        ///<summary>
        /// Mathematically the same as the black-scholes vanna.
        ///</summary>
        public double DDeltaDVol { get; set; }

        ///<summary>
        /// The black-scholes DVannaDVol.
        ///</summary>
        public double DVannaDVol { get; set; }

        ///<summary>
        /// The black-scholes Charm.
        ///</summary>
        public double DDeltaDTime { get; set; }

        ///<summary>
        /// The black-scholes option gamma.
        ///</summary>
        public double Gamma { get; set; }

        ///<summary>
        /// The black-scholes option gamma percentage.
        ///</summary>
        public double GammaP { get; set; }

        ///<summary>
        /// The black-scholes option zomma.
        ///</summary>
        public double DGammaDVol { get; set; }

        ///<summary>
        /// The black-scholes option speed.
        ///</summary>
        public double DGammaDSpot { get; set; }

        ///<summary>
        /// The black-scholes option color.
        ///</summary>
        public double DGammaDTime { get; set; }

        ///<summary>
        /// The black-scholes option Vega.
        ///</summary>
        public double Vega { get; set; }

        ///<summary>
        /// The black-scholes option Vega percentage.
        ///</summary>
        public double VegaP { get; set; }

        ///<summary>
        /// The black-scholes option Vega elasticity.
        ///</summary>
        public double VegaElasticity { get; set; }

        ///<summary>
        /// The black-scholes option Vega local maximum wrt the asset price.
        ///</summary>
        public double VegaLocalMax { get; set; }

        ///<summary>
        /// The black-scholes option Vega maximum wrt time.
        ///</summary>
        public double VegaMaxTime { get; set; }

        ///<summary>
        /// The black-scholes option Vega global maximum wrt the asset price.
        ///</summary>
        public double VegaGlobalMax { get; set; }

        ///<summary>
        /// The black-scholes option vomma.
        ///</summary>
        public double DVegaDVol { get; set; }

        ///<summary>
        /// The black-scholes option ultima.
        ///</summary>
        public double DVommaDVol { get; set; }

        ///<summary>
        /// The black-scholes option DVegaDTime.
        ///</summary>
        public double DVegaDTime { get; set; }

        ///<summary>
        /// The black-scholes option variance vega.
        ///</summary>
        public double VarianceVega { get; set; }

        ///<summary>
        /// The black-scholes option delta change wrt the variance.
        ///</summary>
        public double DDeltaDVar { get; set; }

        ///<summary>
        /// The black-scholes option VarianceVomma.
        ///</summary>
        public double VarianceVomma { get; set; }

        ///<summary>
        /// The black-scholes option VarianceUltima.
        ///</summary>
        public double VarianceUltima { get; set; }

        ///<summary>
        /// The black-scholes option theta.
        ///</summary>
        public double Theta { get; set; }

        ///<summary>
        /// The black-scholes option DriftlessTheta.
        ///</summary>
        public double DriftlessTheta { get; set; }

        ///<summary>
        /// The black-scholes option rho.
        ///</summary>
        public double Rho { get; set; }

        ///<summary>
        /// The black-scholes option rho2.
        ///</summary>
        public double Phi { get; set; }

        ///<summary>
        /// The black-scholes option carry rho.
        ///</summary>
        public double CarryRho { get; set; }

        ///<summary>
        /// The black-scholes option InTheMoneyProbability.
        ///</summary>
        public double InTheMoneyProbability { get; set; }

        ///<summary>
        /// The black-scholes option RiskNeutralProbabilityDensity.
        ///</summary>
        public double RiskNeutralProbabilityDensity { get; set; }

        ///<summary>
        /// The black-scholes option zeta sensitvity to vol.
        ///</summary>
        public double DZetaDVol { get; set; }

        ///<summary>
        /// The black-scholes option zeta sensitvity to time.
        ///</summary>
        public double DZetaDTime { get; set; }

        /// <summary>
        /// Our parameterless constructor.
        /// </summary>
        public BlackScholesMertonModel()
        {}

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// </summary>
        /// <param name="callFlag">The call/put flag.</param>
        /// <param name="fwdPrice">Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">Exercise price of option</param>
        /// <param name="vol">Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="t">Time in years to the maturity of the option.</param>
        /// <returns>An array of reuslts for Black Scholes.</returns>
        public BlackScholesMertonModel(Boolean callFlag, double fwdPrice, double strike, double vol, double t)
        {
            Calculate(callFlag, fwdPrice, strike, vol, t);
        }

        /// <summary>
        /// Gets the swaption value.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="strikeRate"></param>
        /// <param name="volatility"></param>
        /// <param name="timeToExpiry"></param>
        /// <returns></returns>
        public static double GetSwaptionValue(double rate, double strikeRate, double volatility, double timeToExpiry)
        {
            var model = new BlackScholesMertonModel(true, rate, strikeRate, volatility, timeToExpiry);
            return model.Value;
        }

        /// <summary>
        /// Gets the call option value.
        /// </summary>
        /// <param name="floatRate"></param>
        /// <param name="strikeRate"></param>
        /// <param name="volatility"></param>
        /// <param name="timeToExpiry"></param>
        /// <returns></returns>]
        public static Decimal GetCallOptionValue(Decimal floatRate, Decimal strikeRate, Decimal volatility, Decimal timeToExpiry)
        {
            var dFloatRate = Convert.ToDouble(floatRate);
            var dStrikeRate = Convert.ToDouble(strikeRate);
            var dVolatility = Convert.ToDouble(volatility);
            var dTimeToExpiry = Convert.ToDouble(timeToExpiry);
            var model = new BlackScholesMertonModel(true, dFloatRate, dStrikeRate, dVolatility, dTimeToExpiry);
            return Convert.ToDecimal(model.Value);
        }

        /// <summary>
        /// Gets the put option value.
        /// </summary>
        /// <param name="floatRate"></param>
        /// <param name="strikeRate"></param>
        /// <param name="volatility"></param>
        /// <param name="timeToExpiry"></param>
        /// <returns></returns>
        public static Decimal GetPutOptionValue(Decimal floatRate, Decimal strikeRate, Decimal volatility, Decimal timeToExpiry)
        {
            var dFloatRate = Convert.ToDouble(floatRate);
            var dStrikeRate = Convert.ToDouble(strikeRate);
            var dVolatility = Convert.ToDouble(volatility);
            var dTimeToExpiry = Convert.ToDouble(timeToExpiry);
            var model = new BlackScholesMertonModel(false, dFloatRate, dStrikeRate, dVolatility, dTimeToExpiry);
            return Convert.ToDecimal(model.Value);
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// </summary>
        /// <param name="callFlag">The call/put flag.</param>
        /// <param name="fwdPrice">Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">Exercise price of option</param>
        /// <param name="vol">Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="t">Time in years to the maturity of the option.</param>
        /// <returns>An array of reuslts for Black Scholes.</returns>
        public static object[,] Greeks(Boolean callFlag, double fwdPrice, double strike, double vol, double t)
        {
            BlackScholesMertonModel model = new BlackScholesMertonModel(callFlag, fwdPrice, strike, vol, t);
            object[,] result = new object[2,6];
            result[0, 0] = "Value";
            result[1, 0] = model.Value;
            result[0, 1] = "Delta";
            result[1, 1] = model.SpotDelta;
            result[0, 2] = "Gamma";
            result[1, 2] = model.Gamma;
            result[0, 3] = "Vega";
            result[1, 3] = model.Vega;
            result[0, 4] = "Theta";
            result[1, 4] = model.Theta;
            result[0, 5] = "Rho";
            result[1, 5] = model.Rho;
            return result;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// Different combinations in inputs to the generalised model instantiate different models:
        /// b=r (the cost of carry rate = the risk free rate). Black Scholes 1973 stock option model.
        /// b=r-q, where q is the continuous dividend yield. Merton 1973 stock option model.
        /// b=0. The Black 1976 futures option model.
        /// b=0 and r=0. Assay 1982 margined futures option model.
        /// b=r - rf, rf being the foreign rate. Garman Kohlhagen 1983 currency option model.
        /// </summary>
        /// <param name="callFlag">The call/put flag.</param>
        /// <param name="price">The stock price S. Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">The strike price K. Exercise price of option</param>
        /// <param name="rate">The risk free rate.</param>
        /// <param name="costOfCarry">The cost of carry rate.</param>
        /// <param name="vol">Volatility of the relative price change of the underlting assset S. 
        /// Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="t">Time in years to the maturity of the option.</param>
        /// <returns>An array of reuslts for Black Scholes.</returns>
        public static object BSMGeneralisedWithGreeks(Boolean callFlag, double price, double strike, 
                                                      double rate, double costOfCarry, double vol, double t)
        {
            var names = new[] { "Value", "Delta", "Gamma", "Vega", "Theta", "Rho" };
            var greeks = CalculateGeneralisedModel(callFlag, price, strike, t, rate, costOfCarry, vol);
            var result = new object[2, greeks.Length];
            var index = 0;
            foreach (var element in greeks)
            {
                result[0, index] = names[index];
                result[1, index] = element;
                index++;
            }
            return result;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// </summary>
        /// <param name="callFlag">The call/put flag.</param>
        /// <param name="fwdPrice">Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">Exercise price of option</param>
        /// <param name="vol">Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="t">Time in years to the maturity of the option.</param>
        /// <returns>An array of reuslts for Black Scholes.</returns>
        private void Calculate(Boolean callFlag, double fwdPrice, double strike, double vol, double t)
        {
            if (fwdPrice < 0) throw new ArgumentOutOfRangeException(nameof(fwdPrice), "Invalid parameter in option evaluation");
            if (vol < 0) throw new ArgumentOutOfRangeException(nameof(vol), "Invalid parameter in option evaluation");
            if (t < 0) throw new ArgumentOutOfRangeException(nameof(t), "Invalid parameter in option evaluation");
            string key = $"{fwdPrice},{strike},{vol},{t}";
            if (!_values.ContainsKey(key))
            {
                _values.Add(key, 0);
            }
            if (fwdPrice == 0)
            {
                Value = callFlag ? Math.Max(-1 * strike, 0) : Math.Max(strike, 0);
                Rho = -t * Value;
            }
            else if (strike <= 0)
            {
                if (!callFlag)
                {
                    throw new ArgumentOutOfRangeException(nameof(callFlag), "If strike is less than 0 then option must be a put.");
                }
                Value = fwdPrice - strike;
                SpotDelta = 1d;
                Rho = -t * Value;
            }
            else if (t == 0 || vol == 0)
            {
                var df = callFlag ? 1 : -1;
                Value = callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(-1 * (fwdPrice - strike), 0);
                SpotDelta = Value > 0 ? df : 0;
                Rho = -t * Value;
            }
            else
            {
                double sqrtT = Math.Sqrt(t);
                double srt = vol * sqrtT;
                double h = Math.Log(fwdPrice / strike) / srt + srt / 2;
                NormalDistribution normalDistribution = new NormalDistribution();
                double v = OneOverRootTwoPi * Math.Exp(-h * h / 2);
                Vega = v * fwdPrice * sqrtT;
                if (callFlag)
                {
                    SpotDelta = normalDistribution.CumulativeDistribution(h);
                    Value = fwdPrice*SpotDelta - strike*normalDistribution.CumulativeDistribution(h - srt);
                }
                else
                {
                    SpotDelta = -normalDistribution.CumulativeDistribution(-h);
                    Value = fwdPrice*SpotDelta + strike*normalDistribution.CumulativeDistribution(srt - h);
                }
                Gamma = v / (fwdPrice * srt);
                Theta = Vega*vol/(2*t);
                Rho = -t * Value;
            }
        }

        readonly Dictionary<string, double> _values = new Dictionary<string, double>();
        
        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// Different combinations in inputs to the generalised model instantiate different models:
        /// b=r (the cost of carry rate = the risk free rate). Black Scholes 1973 stock option model.
        /// b=r-q, where q is the continuous dividend yield. Merton 1973 stock option model.
        /// b=0. The Black 1976 futures option model.
        /// b=0 and r=0. Assay 1982 margined futures option model.
        /// b=r - rf, rf being the foreign rate. Garman Kohlhagen 1983 currency option model.
        /// The supersymmetry relation between calls and puts is: c(S,K,T,r,b,v) = p(-S,-K,T,r,b,-v)
        /// </summary>
        /// <param name="callFlag">The call/put flag.</param>
        /// <param name="price">The stock price S. Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">The strike price K. Exercise price of option</param>
        /// <param name="rate">The risk free rate.</param>
        /// <param name="costOfCarry">The cost of carry rate.</param>
        /// <param name="vol">Volatility of the relative price change of the underlting assset S. 
        /// Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="timeToExpity">Time in years to the maturity of the option.</param>
        /// <returns>An array of reuslts for Black Scholes.</returns>
        public static double[] CalculateGeneralisedModel(Boolean callFlag, double price, double strike, double timeToExpity, 
                                                         double rate, double costOfCarry, double vol)
        {
            var result = new double[6];
            var discount1 = callFlag ? Math.Exp((costOfCarry - rate) * timeToExpity) : Math.Exp(-rate * timeToExpity);
            var discount2 = callFlag ? Math.Exp(-rate * timeToExpity) : Math.Exp((costOfCarry - rate) * timeToExpity);
            if (price != 0 && vol != 0 && timeToExpity != 0)
            {
                var srt = vol * Math.Sqrt(timeToExpity);

                var d1 = (Math.Log(price / strike) + (costOfCarry + Math.Pow(vol, 2) / 2) * timeToExpity) / srt;
                var d2 = d1 - srt;
                var call = price * discount1 * new NormalDistribution().CumulativeDistribution(d1) -
                           strike * discount2 * new NormalDistribution().CumulativeDistribution(d2);
                var put = strike * discount1 * new NormalDistribution().CumulativeDistribution(-d2) -
                          price * discount2 * new NormalDistribution().CumulativeDistribution(-d1);

                var prem = callFlag ? call : put;
                result[0] = prem;
            }
            if (price == 0)
            {
                result[0] = callFlag ? discount2 * Math.Max(-1 * strike, 0) : discount2 * Math.Max(strike, 0);
            }
            if (strike == 0)
            {
                if (!callFlag)
                {
                    return result;
                }
                result[0] = discount1 * price;
            }
            if (timeToExpity == 0 || vol == 0)
            {
                result[0] = callFlag ? Math.Max((price - strike), 0) : Math.Max(-1 * (price - strike), 0);

            }
            return result;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// Different combinations in inputs to the generalised model instantiate different models:
        /// b=r (the cost of carry rate = the risk free rate). Black Scholes 1973 stock option model.
        /// b=r-q, where q is the continuous dividend yield. Merton 1973 stock option model.
        /// b=0. The Black 1976 futures option model.
        /// b=0 and r=0. Assay 1982 margined futures option model.
        /// b=r - rf, rf being the foreign rate. Garman Kohlhagen 1983 currency option model.
        /// The supersymmetry relation between calls and puts is: c(S,K,T,r,b,v) = p(-S,-K,T,r,b,-v)
        /// </summary>
        /// <param name="price">The stock price S. Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">The strike price K. Exercise price of option</param>
        /// <param name="rate">The risk free rate.</param>
        /// <param name="costOfCarry">The cost of carry rate.</param>
        /// <param name="vol">Volatility of the relative price change of the underlting assset S. 
        /// Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="timeToExpity">Time in years to the maturity of the option.</param>
        /// <returns>A value for the Black Scholes option.</returns>
        public static double CalculateBSMCallValue(double price, double strike, double timeToExpity,
                                                   double rate, double costOfCarry, double vol)
        {
            var discount1 = Math.Exp((costOfCarry - rate) * timeToExpity);
            var discount2 = Math.Exp(-rate * timeToExpity);
            var srt = vol * Math.Sqrt(timeToExpity);
            var d1 = (Math.Log(price / strike) + (costOfCarry + Math.Pow(vol, 2) / 2) * timeToExpity) / srt;
            var d2 = d1 - srt;
            var result = price * discount1 * new NormalDistribution().CumulativeDistribution(d1) -
                         strike * discount2 * new NormalDistribution().CumulativeDistribution(d2);          
            return result;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// Different combinations in inputs to the generalised model instantiate different models:
        /// b=r (the cost of carry rate = the risk free rate). Black Scholes 1973 stock option model.
        /// b=r-q, where q is the continuous dividend yield. Merton 1973 stock option model.
        /// b=0. The Black 1976 futures option model.
        /// b=0 and r=0. Assay 1982 margined futures option model.
        /// b=r - rf, rf being the foreign rate. Garman Kohlhagen 1983 currency option model.
        /// The supersymmetry relation between calls and puts is: c(S,K,T,r,b,v) = p(-S,-K,T,r,b,-v)
        /// </summary>
        /// <param name="price">The stock price S. Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">The strike price K. Exercise price of option</param>
        /// <param name="rate">The risk free rate.</param>
        /// <param name="costOfCarry">The cost of carry rate.</param>
        /// <param name="vol">Volatility of the relative price change of the underlting assset S. 
        /// Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="timeToExpity">Time in years to the maturity of the option.</param>
        /// <returns>A value for the Black Scholes option.</returns>
        public static double CalculateBSMPutValue(double price, double strike, double timeToExpity,
                                                  double rate, double costOfCarry, double vol)
        {
            var result = CalculateBSMCallValue(-price, -strike, timeToExpity, rate, costOfCarry, -vol);
            return result;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// Different combinations in inputs to the generalised model instantiate different models:
        /// b=r (the cost of carry rate = the risk free rate). Black Scholes 1973 stock option model.
        /// b=r-q, where q is the continuous dividend yield. Merton 1973 stock option model.
        /// b=0. The Black 1976 futures option model.
        /// b=0 and r=0. Assay 1982 margined futures option model.
        /// b=r - rf, rf being the foreign rate. Garman Kohlhagen 1983 currency option model.
        /// The supersymmetry relation between calls and puts is: c(S,K,T,r,b,v) = p(-S,-K,T,r,b,-v)
        /// </summary>
        /// <param name="price">The stock price S. Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">The strike price K. Exercise price of option</param>
        /// <param name="rate">The risk free rate.</param>
        /// <param name="costOfCarry">The cost of carry rate.</param>
        /// <param name="vol">Volatility of the relative price change of the underlting assset S. 
        /// Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="timeToExpity">Time in years to the maturity of the option.</param>
        /// <returns>A delta for the Black Scholes call option.</returns>
        public static double CalculateBSMSpotCallDelta(double price, double strike, double timeToExpity,
                                                       double rate, double costOfCarry, double vol)
        {
            var discount1 = Math.Exp((costOfCarry - rate) * timeToExpity);
            var srt = vol * Math.Sqrt(timeToExpity);
            var d1 = (Math.Log(price / strike) + (costOfCarry + Math.Pow(vol, 2) / 2) * timeToExpity) / srt;
            var result = discount1 * new NormalDistribution().CumulativeDistribution(d1);
            return result;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// Different combinations in inputs to the generalised model instantiate different models:
        /// b=r (the cost of carry rate = the risk free rate). Black Scholes 1973 stock option model.
        /// b=r-q, where q is the continuous dividend yield. Merton 1973 stock option model.
        /// b=0. The Black 1976 futures option model.
        /// b=0 and r=0. Assay 1982 margined futures option model.
        /// b=r - rf, rf being the foreign rate. Garman Kohlhagen 1983 currency option model.
        /// The supersymmetry relation between calls and puts is: c(S,K,T,r,b,v) = p(-S,-K,T,r,b,-v)
        /// </summary>
        /// <param name="price">The stock price S. Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">The strike price K. Exercise price of option</param>
        /// <param name="rate">The risk free rate.</param>
        /// <param name="costOfCarry">The cost of carry rate.</param>
        /// <param name="vol">Volatility of the relative price change of the underlting assset S. 
        /// Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="timeToExpity">Time in years to the maturity of the option.</param>
        /// <returns>A delta for the Black Scholes put option.</returns>
        public static double CalculateBSMSpotPutDelta(double price, double strike, double timeToExpity,
                                                      double rate, double costOfCarry, double vol)
        {
            var result = CalculateBSMSpotCallDelta(-price, -strike, timeToExpity, rate, costOfCarry, -vol);
            return result;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// Different combinations in inputs to the generalised model instantiate different models:
        /// b=r (the cost of carry rate = the risk free rate). Black Scholes 1973 stock option model.
        /// b=r-q, where q is the continuous dividend yield. Merton 1973 stock option model.
        /// b=0. The Black 1976 futures option model.
        /// b=0 and r=0. Assay 1982 margined futures option model.
        /// b=r - rf, rf being the foreign rate. Garman Kohlhagen 1983 currency option model.
        /// The supersymmetry relation between calls and puts is: c(S,K,T,r,b,v) = p(-S,-K,T,r,b,-v)
        /// </summary>
        /// <param name="price">The stock price S. Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">The strike price K. Exercise price of option</param>
        /// <param name="rate">The risk free rate.</param>
        /// <param name="costOfCarry">The cost of carry rate.</param>
        /// <param name="vol">Volatility of the relative price change of the underlting assset S. 
        /// Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="timeToExpity">Time in years to the maturity of the option.</param>
        /// <returns>A future delta for the Black Scholes call option.</returns>
        public static double CalculateBSMFutureCallDelta(double price, double strike, double timeToExpity,
                                                         double rate, double costOfCarry, double vol)
        {
            var result = CalculateBSMSpotCallDelta(price, strike, timeToExpity, rate, costOfCarry, vol);
            return result * Math.Exp(-costOfCarry * timeToExpity);
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// Different combinations in inputs to the generalised model instantiate different models:
        /// b=r (the cost of carry rate = the risk free rate). Black Scholes 1973 stock option model.
        /// b=r-q, where q is the continuous dividend yield. Merton 1973 stock option model.
        /// b=0. The Black 1976 futures option model.
        /// b=0 and r=0. Assay 1982 margined futures option model.
        /// b=r - rf, rf being the foreign rate. Garman Kohlhagen 1983 currency option model.
        /// The supersymmetry relation between calls and puts is: c(S,K,T,r,b,v) = p(-S,-K,T,r,b,-v)
        /// </summary>
        /// <param name="price">The stock price S. Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">The strike price K. Exercise price of option</param>
        /// <param name="rate">The risk free rate.</param>
        /// <param name="costOfCarry">The cost of carry rate.</param>
        /// <param name="vol">Volatility of the relative price change of the underlting assset S. 
        /// Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="timeToExpity">Time in years to the maturity of the option.</param>
        /// <returns>A future delta for the Black Scholes put option.</returns>
        public static double CalculateBSMFuturePutDelta(double price, double strike, double timeToExpity,
                                                        double rate, double costOfCarry, double vol)
        {
            var result = CalculateBSMSpotPutDelta(price, strike, timeToExpity, rate, costOfCarry, vol);
            return result * Math.Exp(-costOfCarry * timeToExpity);
        }
    }
}