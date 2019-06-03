#region Using Directives

using System;
using System.Collections.Generic;
using Orion.Analytics.Utilities;

#endregion

namespace Orion.Analytics.Options
{
    /// <summary>
    /// Class that encapsulates a Black Caplet/Flooret.
    /// The principal method offered to clients of the class is the
    /// functionality to price a Black Caplet/Flooret by an appropriate
    /// modification of the Black-Scholes formula, as described in the
    /// document "Caplet Bootstrap and Interpolation Methodologies" by George
    /// Scoufis (Reference Number: GS-14022008/1).
    /// </summary>
    public class BlackCapletFlooret
    {
        #region Constants and Enums

        /// <summary>
        /// Minimum expiry, which corresponds to one day in ACT/365 day count.
        /// </summary>
        private const decimal MinimumOptionExpiry = 2.73973E-03m;

        /// <summary>
        /// Enumeration for the option types supported by the class.
        /// </summary>
        public enum OptionType
        {
            /// <summary>
            /// Call option on the forward rate.
            /// </summary>
            Caplet,
            /// <summary>
            /// Put option on the forward rate.
            /// </summary>
            Flooret
        }

        #endregion 

        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="BlackCapletFlooret"/>.
        /// </summary>
        /// <param name="notional">Caplet/Flooret face value.
        /// Precondition: notional must be positive.</param>
        /// <param name="optionExpiry">Option expiry expressed in years as
        /// an ACT/365 day count.
        /// Example: 0.24658 for a 90 day option expiry.
        /// Precondition: option expiry must be greater than or equal to zero.
        /// </param>
        /// <param name="optionType">Identifier for the option type.</param>
        /// <param name="strike">Option strike expressed as a decimal.
        /// Example: 0.09 for a strike of 9%.
        /// Precondition: strike must be positive.</param>
        /// <param name="tau">Year fraction for the Caplet/Flooret in the
        /// appropriate day count convention.
        /// Precondition: year fraction must be positive.</param>
        /// <param name="validateArguments">if set to <c>true</c>
        /// validate constructor arguments, otherwise do not validate.</param>
        public BlackCapletFlooret(decimal notional, decimal optionExpiry, OptionType optionType,
                                  decimal strike, decimal tau, bool validateArguments)
        {
            if (validateArguments)
            {
                ValidateConstructorArguments(notional, optionExpiry, strike, tau);
            }

            InitialisePrivateFields(notional, optionExpiry, optionType, strike, tau);
        }

        /// <summary>
        /// Constructor for the class <see cref="BlackCapletFlooret"/>.
        /// </summary>
        /// <param name="notional">Caplet/Flooret face value.
        /// Precondition: notional must be positive.</param>
        /// <param name="optionExpiry">Option expiry expressed in years as
        /// an ACT/365 day count.
        /// Example: 0.24658 for a 90 day option expiry.
        /// Precondition: option expiry must be greater than or equal to zero.
        /// </param>
        /// <param name="optionType">Identifier for the option type.</param>
        /// <param name="strike">Option strike expressed as a decimal.
        /// Example: 0.09 for a strike of 9%.
        /// Precondition: strike must be positive.</param>
        /// <param name="tau">Year fraction for the Caplet/Flooret in the
        /// appropriate day count convention.
        /// Precondition: year fraction must be positive.</param>
        /// <param name="validateArguments">if set to <c>true</c>
        /// validate constructor arguments, otherwise do not validate.</param>
        public BlackCapletFlooret(double notional, double optionExpiry, OptionType optionType,
                                  double strike, double tau, bool validateArguments)
        {
            if (validateArguments)
            {
                ValidateConstructorArguments(notional, optionExpiry, strike, tau);
            }

            InitialisePrivateFields(notional, optionExpiry, optionType, strike, tau);
        }

        #endregion

        #region Public Business Logic Methods

        /// <summary>
        /// Computes the price of a Black Caplet/Flooret.
        /// </summary>
        /// <param name="discountFactor">Discount factor from the 
        /// Calculation Date to the end (maturity) of the Caplet/Flooret.
        /// Precondition: discount factor must be positive.</param>
        /// <param name="forwardRate">Simple forward rate over the 
        /// Caplet/Flooret period as a decimal in the appropriate
        /// day count convention.
        /// Example: 0.07469 for 7.469%.
        /// Precondition: forward rate must be positive.</param>
        /// <param name="sigma">The volatility expressed as a decimal
        /// from the Calculation Date to the start (expiry) of the
        /// Caplet/Flooret.
        /// Example: 0.0876 for a volatility of 8.76%.
        /// Precondition: sigma must be positive.</param>
        /// <param name="validateArguments">If set to <c>true</c> all
        /// function arguments are validated against the preconditions.</param>
        /// <returns>Caplet/Flooret price.</returns>
        public decimal ComputePrice(decimal discountFactor, decimal forwardRate, decimal sigma, bool validateArguments)
        {
            string key = string.Format("{0},{1},{2}", discountFactor, forwardRate, sigma);
            if (Prices.ContainsKey(key))
            {
                return Prices[key];
            }

            if (validateArguments)
            {
                ValidateComputePriceArguments(discountFactor, forwardRate, sigma);
            }

            // Compute Caplet/Flooret price.
            decimal price;

            if (_optionExpiry == 0.0m)
            {
                // Caplet/Flooret has expired: zero value.
                price = 0.0m;
            }
            else if (_optionExpiry < MinimumOptionExpiry)
            {
                // Caplet/Flooret price corresponds to its intrinsic value.
                var omega = (_optionType == OptionType.Caplet ? 1d : -1d);
                price = _notional * _tau * discountFactor *
                        (decimal)Math.Max(omega * (double)(forwardRate - _strike), 0d);
            }
            else
            {
                // Caplet/Flooret price is computed from Black's formula.
                // Map the arguments into Blacks's formula.

                var discountFactorAsDouble = decimal.ToDouble(discountFactor);
                var forwardRateAsDouble = decimal.ToDouble(forwardRate);
                var sigmaAsDouble = decimal.ToDouble(sigma);

                double temp = pricer.PriceBlackVanillaSwaption(forwardRateAsDouble, discountFactorAsDouble, sigmaAsDouble);
                price = _notional * _tau * (decimal)temp;
            }

            Prices.Add(key, price);

            return price;
        }

        Dictionary<string, decimal> Prices = new Dictionary<string, decimal>();

        /// <summary>
        /// Computes the price of a Black Caplet/Flooret.
        /// </summary>
        /// <param name="discountFactor">Discount factor from the 
        /// Calculation Date to the end (maturity) of the Caplet/Flooret.
        /// Precondition: discount factor must be positive.</param>
        /// <param name="forwardRate">Simple forward rate over the 
        /// Caplet/Flooret period as a decimal in the appropriate
        /// day count convention.
        /// Example: 0.07469 for 7.469%.
        /// Precondition: forward rate must be positive.</param>
        /// <param name="sigma">The volatility expressed as a decimal
        /// from the Calculation Date to the start (expiry) of the
        /// Caplet/Flooret.
        /// Example: 0.0876 for a volatility of 8.76%.
        /// Precondition: sigma must be positive.</param>
        /// <param name="validateArguments">If set to <c>true</c> all
        /// function arguments are validated against the preconditions.</param>
        /// <returns>Caplet/Flooret price.</returns>
        public double ComputePrice(double discountFactor,
                                    double forwardRate,
                                    double sigma,
                                    bool validateArguments)
        {
            if(validateArguments)
            {
                ValidateComputePriceArguments
                    (discountFactor, forwardRate, sigma);
            }

            // Compute Caplet/Flooret price.
            double price;

            if (_optionExpiry == 0.0m)
            {
                // Caplet/Flooret has expired: zero value.
                price = 0.0d;
            }
            else if (_optionExpiry < MinimumOptionExpiry)
            {
                var omega = (_optionType == OptionType.Caplet ? 1d : -1d);
                // Caplet/Flooret price corresponds to its intrinsic value.
                price = decimal.ToDouble(_notional * _tau)* discountFactor *
                        Math.Max(omega * (forwardRate - decimal.ToDouble(_strike)), 0d);
            }
            else
            {
                // Caplet/Flooret price is computed from Black's formula.
                // Map the arguments into Blacks's formula.
                var temp = pricer.PriceBlackVanillaSwaption(forwardRate, discountFactor, sigma);
                price = decimal.ToDouble(_notional*_tau)*temp;
            }

            return price;
        }

        /// <summary>
        /// Computes the dollar Vega of a Black Caplet/Flooret.
        /// </summary>
        /// <param name="discountFactor">Discount factor from the 
        /// Calculation Date to the end (maturity) of the Caplet/Flooret.
        /// Precondition: discount factor must be positive.</param>
        /// <param name="forwardRate">Simple forward rate over the 
        /// Caplet/Flooret period as a decimal in the appropriate
        /// day count convention.
        /// Example: 0.07469 for 7.469%.
        /// Precondition: forward rate must be positive.</param>
        /// <param name="sigma">The volatility expressed as a decimal
        /// from the Calculation Date to the start (expiry) of the
        /// Caplet/Flooret.
        /// Example: 0.0876 for a volatility of 8.76%.
        /// Precondition: sigma must be positive.</param>
        /// <param name="validateArguments">If set to <c>true</c> all
        /// function arguments are validated against the preconditions.</param>
        /// <returns>Vega as a dollar amount.</returns>
        public decimal ComputeVega(decimal discountFactor,
                                   decimal forwardRate,
                                   decimal sigma,
                                   bool validateArguments)
        {
            if (validateArguments)
            {
                ValidateComputeVegaArguments
                    (discountFactor, forwardRate, sigma);
            }

            // Compute the Vega: initialise the return variable with the
            // Vega in the case of a small expiry.
            var vega = 0.0m;

            if (_optionExpiry >= MinimumOptionExpiry)
            {
                // Compute the Vega by a closed-form formula.
                var logMoneyness = (decimal)Math.Log(decimal.ToDouble(forwardRate / _strike));
                var var = sigma * sigma * _optionExpiry;
                var d1 =
                    (logMoneyness + 0.5m * var) / (decimal)Math.Sqrt(decimal.ToDouble(var));
                var phid1 = (decimal)(Math.Exp(-0.5d * decimal.ToDouble(d1 * d1)) / Math.Sqrt(2 * Math.PI));

                vega = discountFactor * _tau * forwardRate * _notional *
                       (decimal)Math.Sqrt(decimal.ToDouble(_optionExpiry)) * phid1;

                // Convert vega to a dollar amount.
                vega = vega/100.0m;                
            }

            return vega;
        }

        /// <summary>
        /// Computes the dollar Vega of a Black Caplet/Flooret.
        /// </summary>
        /// <param name="discountFactor">Discount factor from the 
        /// Calculation Date to the end (maturity) of the Caplet/Flooret.
        /// Precondition: discount factor must be positive.</param>
        /// <param name="forwardRate">Simple forward rate over the 
        /// Caplet/Flooret period as a decimal in the appropriate
        /// day count convention.
        /// Example: 0.07469 for 7.469%.
        /// Precondition: forward rate must be positive.</param>
        /// <param name="sigma">The volatility expressed as a decimal
        /// from the Calculation Date to the start (expiry) of the
        /// Caplet/Flooret.
        /// Example: 0.0876 for a volatility of 8.76%.
        /// Precondition: sigma must be positive.</param>
        /// <param name="validateArguments">If set to <c>true</c> all
        /// function arguments are validated against the preconditions.</param>
        /// <returns>Vega as a dollar amount.</returns>
        public double ComputeVega(double discountFactor,
                                   double forwardRate,
                                   double sigma,
                                   bool validateArguments)
        {
            if (validateArguments)
            {
                ValidateComputeVegaArguments
                    (discountFactor, forwardRate, sigma);
            }

            // Compute the Vega: initialise the return variable with the
            // Vega in the case of a small expiry.
            var vega = 0.0d;

            if (_optionExpiry >= MinimumOptionExpiry)
            {
                // Compute the Vega by a closed-form formula.
                var logMoneyness = Math.Log(forwardRate / decimal.ToDouble(_strike));
                var var = sigma * sigma * decimal.ToDouble(_optionExpiry);
                var d1 =
                    (logMoneyness + 0.5d * var) / Math.Sqrt(var);
                var phid1 = Math.Exp(-0.5d * d1 * d1) / Math.Sqrt(2 * Math.PI);

                vega = discountFactor * decimal.ToDouble(_tau) * forwardRate * decimal.ToDouble(_notional) *
                       Math.Sqrt(decimal.ToDouble(_optionExpiry)) * phid1;

                // Convert vega to a dollar amount.
                vega = vega / 100.0d;
            }

            return vega;
        }

        #endregion

        #region Private Data Initialisation and Validation Methods

        /// <summary>
        /// Helper function used to initialise all private fields. All function
        /// arguments are mapped to the appropriate private field.
        /// No data validation on the function arguments is performed.
        /// </summary>
        /// <param name="notional">Face value.</param>
        /// <param name="optionExpiry">Option expiry.</param>
        /// <param name="optionType">Option type.</param>
        /// <param name="strike">Option strike.</param>
        /// <param name="tau">Year fraction for the caplet/flooret</param>        
        private void InitialisePrivateFields(decimal notional, decimal optionExpiry, OptionType optionType,
             decimal strike, decimal tau)
        {
            // Map each argument to its appropriate private field.
            _notional = notional;
            _optionExpiry = optionExpiry;
            _optionType = optionType;
            _strike = strike;
            _tau = tau;

            var callPutFlag = optionType == OptionType.Caplet ? BlackVanillaSwaption.SwaptionType.Payer : BlackVanillaSwaption.SwaptionType.Receiver;
            pricer = new BlackVanillaSwaption(callPutFlag, (double)strike, (double)optionExpiry);
        }

        /// <summary>
        /// Helper function used to initialise all private fields. All function
        /// arguments are mapped to the appropriate private field.
        /// No data validation on the function arguments is performed.
        /// </summary>
        /// <param name="notional">Face value.</param>
        /// <param name="optionExpiry">Option expiry.</param>
        /// <param name="optionType">Option type.</param>
        /// <param name="strike">Option strike.</param>
        /// <param name="tau">Year fraction for the caplet/flooret</param>        
        private void InitialisePrivateFields
            (double notional,
             double optionExpiry,
             OptionType optionType,
             double strike,
             double tau)
        {
            InitialisePrivateFields((decimal) notional, (decimal) optionExpiry, optionType, 
                (decimal) strike, (decimal) tau);
        }

        /// <summary>
        /// Helper function used to validate the arguments in the function
        /// used to compute the Caplet/Flooret price.
        /// </summary>
        /// <param name="discountFactor">Discount factor.</param>
        /// <param name="forwardRate">Forward rate.</param>
        /// <param name="sigma">Volatility.</param>
        private static void ValidateComputePriceArguments
            (decimal discountFactor,
             decimal forwardRate,
             decimal sigma)
        {
            const bool ThrowError = true;

            // Validate discount factor.
            const string DiscountFactorErrorMessage =
                "Discount factor used to price a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(discountFactor,
                                                  DiscountFactorErrorMessage,
                                                  ThrowError);

            // Validate forward rate.
            const string ForwardRateErrorMessage =
                "Forward rate used to price a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(forwardRate,
                                                  ForwardRateErrorMessage,
                                                  ThrowError);

            // Validate the forward rate.
            const string SigmaErrroMessage =
                "Volatility used to price a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(sigma,
                                                  SigmaErrroMessage,
                                                  ThrowError);
        }

        /// <summary>
        /// Helper function used to validate the arguments in the function
        /// used to compute the Caplet/Flooret price.
        /// </summary>
        /// <param name="discountFactor">Discount factor.</param>
        /// <param name="forwardRate">Forward rate.</param>
        /// <param name="sigma">Volatility.</param>
        private static void ValidateComputePriceArguments
            (double discountFactor,
             double forwardRate,
             double sigma)
        {
            const bool ThrowError = true;

            // Validate discount factor.
            const string DiscountFactorErrorMessage =
                "Discount factor used to price a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(discountFactor,
                                                  DiscountFactorErrorMessage,
                                                  ThrowError);

            // Validate forward rate.
            const string ForwardRateErrorMessage =
                "Forward rate used to price a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(forwardRate,
                                                  ForwardRateErrorMessage,
                                                  ThrowError);

            // Validate the forward rate.
            const string SigmaErrroMessage =
                "Volatility used to price a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(sigma,
                                                  SigmaErrroMessage,
                                                  ThrowError);
        }

        /// <summary>
        /// Helper function used to validate the arguments in the function
        /// used to compute the Caplet/Flooret Vega.
        /// </summary>
        /// <param name="discountFactor">Discount factor.</param>
        /// <param name="forwardRate">Forward rate.</param>
        /// <param name="sigma">Volatility.</param>
        private static void ValidateComputeVegaArguments
            (decimal discountFactor,
             decimal forwardRate,
             decimal sigma)
        {
            ValidateComputePriceArguments(discountFactor, forwardRate, sigma);
        }

        /// <summary>
        /// Helper function used to validate the arguments in the function
        /// used to compute the Caplet/Flooret Vega.
        /// </summary>
        /// <param name="discountFactor">Discount factor.</param>
        /// <param name="forwardRate">Forward rate.</param>
        /// <param name="sigma">Volatility.</param>
        private static void ValidateComputeVegaArguments
            (double discountFactor,
             double forwardRate,
             double sigma)
        {
            ValidateComputePriceArguments(discountFactor, forwardRate, sigma);
        }


        /// <summary>
        /// Helper function used to validate all constructor arguments.
        /// </summary>
        /// <param name="notional">Caplet/Flooret face value.</param>
        /// <param name="optionExpiry">Option expiry expressed in years
        /// as an ACT/365 day count.</param>
        /// <param name="strike">Option strike expressed as a decimal.</param>
        /// <param name="tau">Year fraction for the Caplet/Flooret.</param>
        private static void ValidateConstructorArguments
            (double notional,
             double optionExpiry,
             double strike,
             double tau)
        {
            // Validate notional.
            const string NotionalErrorMessage =
                "Notional for a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(notional,
                                                  NotionalErrorMessage,
                                                  true);

            // Validate option expiry.
            const string OptionExpiryErrorMessage =
                "Expiry for a Caplet/Flooret cannot be negative";
            DataQualityValidator.ValidateMinimum(optionExpiry,
                                                 0d,
                                                 OptionExpiryErrorMessage,
                                                 true);

            // Validate strike.
            const string StrikeErrorMessage =
                "Strike for a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(strike,
                                                  StrikeErrorMessage,
                                                  true);

            // Validate tau.
            const string TauErrorMessage =
                "Year fraction for a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(tau,
                                                  TauErrorMessage,
                                                  true);
        }

        /// <summary>
        /// Helper function used to validate all constructor arguments.
        /// </summary>
        /// <param name="notional">Caplet/Flooret face value.</param>
        /// <param name="optionExpiry">Option expiry expressed in years
        /// as an ACT/365 day count.</param>
        /// <param name="strike">Option strike expressed as a decimal.</param>
        /// <param name="tau">Year fraction for the Caplet/Flooret.</param>
        private static void ValidateConstructorArguments
            (decimal notional,
             decimal optionExpiry,
             decimal strike,
             decimal tau)
        {
            // Validate notional.
            const string NotionalErrorMessage =
                "Notional for a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(notional,
                                                  NotionalErrorMessage,
                                                  true);

            // Validate option expiry.
            const string OptionExpiryErrorMessage =
                "Expiry for a Caplet/Flooret cannot be negative";
            DataQualityValidator.ValidateMinimum(optionExpiry,
                                                 0m,
                                                 OptionExpiryErrorMessage,
                                                 true);

            // Validate strike.
            const string StrikeErrorMessage =
                "Strike for a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(strike,
                                                  StrikeErrorMessage,
                                                  true);

            // Validate tau.
            const string TauErrorMessage =
                "Year fraction for a Caplet/Flooret must be positive";
            DataQualityValidator.ValidatePositive(tau,
                                                  TauErrorMessage,
                                                  true);
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Caplet/Flooret face value.
        /// </summary>
        private decimal _notional;

        /// <summary>
        /// Option expiry expressed in years as an ACT/365 day count.
        /// Example: 0.24658 for a 90 day option expiry. 
        /// </summary>
        private decimal _optionExpiry;

        /// <summary>
        /// Identifier for the option type.
        /// </summary>
        private OptionType _optionType;

        /// <summary>
        /// Option strike expressed as a decimal.
        /// Example: 0.09 for a strike of 9%.
        /// </summary>
        private decimal _strike;

        /// <summary>
        /// Year fraction for the caplet/flooret in the
        /// appropriate day count convention.
        /// </summary>
        private decimal _tau;

        private BlackVanillaSwaption pricer;

        #endregion 
    }
}