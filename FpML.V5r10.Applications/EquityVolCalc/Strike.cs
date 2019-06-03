using System;
using Orion.Equity.VolatilityCalculator.Helpers;
using Orion.Util.Helpers;

namespace Orion.Equity.VolatilityCalculator
{
    /// <summary>
    /// Defines a stricke (i.e. a paires call and put position)
    /// </summary>
    [Serializable]
    public class Strike
    {
        //Decimal _volatility = 0;
        private Decimal _defaultVolatility;

        private readonly Pair<OptionPosition, OptionPosition> _strikePositions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        public Strike()
        {
            VolatilityHasBeenSet = false;
            StrikePrice = 0;
            Volatility = null;
            Moneyness = 0;
            PriceUnits = Units.Cents;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        /// <param name="strikePrice">The strike price.</param>
        /// <param name="callPosition">The call position.</param>
        /// <param name="putPosition">The put position.</param>
        /// <example>
        ///     <code>
        ///     // Creates a Strike instance
        ///     OptionPosition callOption = new OptionPosition("123", 456, PositionType.Call);
        ///     OptionPosition putOption = new OptionPosition("123", 789, PositionType.Put);
        ///     Strike strike = new Strike(123, callOption, putOption);
        ///     </code>
        /// </example>
        public Strike(Double strikePrice, OptionPosition callPosition, OptionPosition putPosition)
        {
            VolatilityHasBeenSet = false;
            Volatility = null;
            Moneyness = 0;
            PriceUnits = Units.Cents;
            InputValidator.NotZero("StrikePrice", strikePrice, true);
            StrikePrice = strikePrice;
            _strikePositions = new Pair<OptionPosition, OptionPosition>(callPosition, putPosition);         
            InterpModel = new WingInterp();
            //this.InterpModel = new SABRInterp();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        /// <param name="strikePrice">The strike price.</param>
        /// <param name="callPosition">The call position.</param>
        /// <param name="putPosition">The put position.</param>
        /// <param name="units">The units.</param>
        public Strike(Double strikePrice, OptionPosition callPosition, OptionPosition putPosition, Units units)
        {
            VolatilityHasBeenSet = false;
            Volatility = null;
            Moneyness = 0;
            InputValidator.NotZero("StrikePrice", strikePrice, true);
            StrikePrice = strikePrice;        
            _strikePositions = new Pair<OptionPosition, OptionPosition>(callPosition, putPosition);      
           // this.InterpModel = new SABRInterp();
             InterpModel = new WingInterp();
            PriceUnits = units;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Strike"/> class.
        /// </summary>
        /// <param name="moneyness">The moneyness.</param>
        /// <param name="fwdPrice">The FWD price.</param>
        /// <param name="callPosition">The call position.</param>
        /// <param name="putPosition">The put position.</param>
        public Strike(Double moneyness, double fwdPrice, OptionPosition callPosition, OptionPosition putPosition)
        {
            VolatilityHasBeenSet = false;
            Volatility = null;
            Moneyness = 0;
            PriceUnits = Units.Cents;
            InputValidator.NotZero("StrikePrice", moneyness, true);
            StrikePrice = moneyness * fwdPrice;    
            _strikePositions = new Pair<OptionPosition, OptionPosition>(callPosition, putPosition);
            //this.InterpModel = new SABRInterp();
            InterpModel = new WingInterp();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [nodal point].
        /// </summary>
        /// <value><c>true</c> if [nodal point]; otherwise, <c>false</c>.</value>
        public Boolean NodalPoint { get; set; }

        /// <summary>
        /// Gets a value indicating whether [volatility has been set].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [volatility has been set]; otherwise, <c>false</c>.
        /// </value>
        public bool VolatilityHasBeenSet { get; private set; }

        /// <summary>
        /// Gets the strike price.
        /// </summary>
        /// <value>The strike price.</value>
        public double StrikePrice { get; set; }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public IVolatilityPoint Volatility { get; private set; }

        /// <summary>
        /// Gets the call.
        /// </summary>
        /// <value>The call.</value>
        public OptionPosition Call => _strikePositions.First;

        /// <summary>
        /// Gets the put.
        /// </summary>
        /// <value>The put.</value>
        public OptionPosition Put => _strikePositions.Second;


        /// <summary>
        /// [ERROR: Unknown property access] the moneyness.
        /// </summary>
        /// <value>The moneyness.</value>
        public double Moneyness { get; set; }


        /// <summary>
        /// Sets the volatility.
        /// </summary>
        /// <param name="volatility">The volatility.</param>
        /// <example>
        ///     <code>
        ///     // Adding volatility to a strike with a default value
        ///     IVolatilityPoint point = new VolatilityPoint();
        ///     point.SetVolatility(volatility, VolatilityState.Default());
        ///     Strike strike = new Strike(..);
        ///     strike.SetVolatility(point);
        ///     </code>
        /// </example>
        public void SetVolatility(IVolatilityPoint volatility)
        {
            if (!VolatilityHasBeenSet && volatility.State.Status == VolatilityStateType.Default && volatility.Value == 0)
            {
                DefaultVolatility = volatility.Value;
            }
            else if (volatility.State.Status == VolatilityStateType.Failure)
            {
                VolatilityHasBeenSet = false;
            }
            else
            {
                VolatilityHasBeenSet = true;
            }
            Volatility = volatility;
        }

        /// <summary>
        /// [ERROR: Unknown property access] the price units.
        /// </summary>
        /// <value>The price units.</value>
        public Units PriceUnits { get; set; }

        /// <summary>
        /// Gets or sets the tenor interp model.
        /// </summary>
        /// <value>The tenor model.</value>
        public WingInterp InterpModel { get; set; }

        public decimal DefaultVolatility
        {
            get => _defaultVolatility;
            set => _defaultVolatility = value;
        }
    }
}
