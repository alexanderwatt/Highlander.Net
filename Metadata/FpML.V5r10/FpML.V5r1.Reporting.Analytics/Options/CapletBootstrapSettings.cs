#region Using Directives

using National.QRSC.Analytics.Utilities;
using nabCap.QR.Schemas.FpML;
using System;
using System.Collections.Generic;
using National.QRSC.Business.Calendars;
using National.QRSC.ModelFramework;

#endregion

namespace National.QRSC.Analytics.Options
{
    /// <summary>
    /// Enum for the Cap frequency (number of Caplets per year).
    /// </summary>
    public enum CapFrequency
    {
        /// <summary>
        /// 1 Caplet per year
        /// </summary>
        Yearly = 1,

        /// <summary>
        /// 4 Caplets per year
        /// </summary>
        Quarterly = 4,

        /// <summary>
        ///  2 Caplets per year
        /// </summary>
        SemiAnnually = 2,

        /// <summary>
        /// 12 Caplets per year 
        /// </summary>
        Monthly = 12       
    }

    /// <summary>
    /// Enum for the par volatility interpolation that will be used
    /// in the Caplet bootstrap.
    /// </summary>
    public enum ParVolatilityInterpolationType
    {
        /// <summary>
        /// Cubic Hermite Spline interpolation of par volatilities.
        /// </summary>
        CubicHermiteSpline,
        /// <summary>
        /// Linear interpolation of par volatilities.
        /// </summary>
        Linear
    }

    /// <summary>
    /// Class that encapsulates the settings used to bootstrap Caplet 
    /// volatilities from ETO volatilities and (par) Cap volatilities.
    /// Note: the comments below for Cap and Caplets also apply to Floor and
    /// Floorets.
    /// </summary>
    public class CapletBootstrapSettings
    {
        #region Constructor

        /// <summary>
        /// Constructor for the class <see cref="CapletBootstrapSettings"/>.
        /// </summary>
        /// <param name="calculationDate">Reference date to which all cashflows
        /// are discounted.</param>
        /// <param name="capFrequency">Number of Caplets per year.
        /// Example: Value of 4 corresponds to a Cap with quarterly Caplets, so
        /// that reference interest rate is the 3M index in the appropriate
        /// currency.</param>
        /// <param name="capStartLag">Number of days after the Calculation Date
        /// for the commencement (first caplet expiry) of the Interest Rate 
        /// Caps with quoted market Par volatilities.
        /// Precondition: cap start lag cannot be negative.</param>
        /// <param name="currency">Three letter letter currency code.
        /// Example: AUD.</param>
        /// <param name="handle">Name that identifies the object that stores
        /// the settings for the Caplet bootstrap procedure.
        /// Precondition: handle cannot be an empty string.</param>
        /// <param name="parVolatilityInterpolation">One dimensional
        /// interpolation methodology for par volatilities.</param>
        /// <param name="rollConvention">Swap roll convention that will be used
        /// to generate the schedule of Caplet expiries.</param>
        /// <param name="validateArguments">if set to <c>true</c> validate
        /// constructor arguments, otherwise do not validate.</param>
        public CapletBootstrapSettings
            (DateTime calculationDate,
             CapFrequency capFrequency,
             int capStartLag,
             string currency,
             string handle,
             ParVolatilityInterpolationType parVolatilityInterpolation,
             BusinessDayConventionEnum rollConvention,
             bool validateArguments)
        {
            if(validateArguments)
            {
                ValidateConstructorArguments(capStartLag, handle);   
            }

            InitialisePrivateFields
                (calculationDate,
                 capFrequency,
                 capStartLag,
                 currency,
                 handle,
                 parVolatilityInterpolation,
                 rollConvention);
        }

        #endregion

        #region Accessor Methods

        /// <summary>
        /// Accessor  method for the Business Calendar.
        /// </summary>
        /// <value>Four letter code for the Business Calendar.
        /// Example: AUSY</value>
        public string BusinessCalendar { get; private set; }

        /// <summary>
        /// Accessor method of the Calculation Date.
        /// </summary>
        /// <value>The Calculation Date.</value>
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// Accessor method for the Cap Frequency.
        /// </summary>
        /// <value>The Cap Frequency.</value>
        public CapFrequency CapFrequency { get; private set; }

        /// <summary>
        /// Accessor method for the Cap Start Date.
        /// </summary>
        /// <value>The Cap Start Date.</value>
        public DateTime CapStartDate { get; private set; }

        /// <summary>
        /// Accessor method for the currency.
        /// </summary>
        /// <value>Three letter code for the Currency.
        /// Example: AUD</value>
        public string Currency { get; private set; }

        /// <summary>
        /// Accessor method for the day count.
        /// </summary>
        /// <value>Day count convention.</value>
        public string DayCount { get; private set; }

        /// <summary>
        /// Accessor method for the par volatility interpolation.
        /// </summary>
        /// <value>Par volatility interpolation method.</value>
        public ParVolatilityInterpolationType ParVolatilityInterpolation
        {
            get { return _parVolatilityInterpolation; }
        }

        /// <summary>
        /// Accessor method for the roll convention.
        /// </summary>
        /// <value>Enum for the roll convention.</value>
        public BusinessDayConventionEnum RollConvention { get; private set; }

        #endregion

        #region Private Data Initialisation and Validation Methods

        /// <summary>
        /// Helper function used by the master initialisation function to 
        /// map the currency into a format that can be used by the 
        /// Calendar library and set the currency code.
        /// Mappings are:
        /// 1) AUD => AUSY;
        /// 2) USD => USNY;
        /// 3) GBP => GBLO;
        /// 4) EUR => EUEU;
        /// 5) NZD => NZWE.       
        /// </summary>
        /// <param name="currency">Three letter currency code in the format
        /// CCY. The currency code is NOT case sensitive.
        /// If the three letter currency code cannot be found, the default
        /// is to map to the string "AUD" and the business calendar "AUSY".
        /// </param>
        private void InitialiseBusinessCalendarAndCurrency(string currency)
        {
            // Load the currency string converter.            
            var currencyStringConverter =
                new SortedList<string, string>
                    {
                        {"AUD", "AUSY"},
                        {"USD", "USNY"},
                        {"GBP", "GBLO"},
                        {"EUR", "EUEU"},
                        {"NZD", "NZWE"}
                    };

            // Set defaults.
            BusinessCalendar = "AUSY";
            Currency = "AUD";

            string searchString = currency.ToUpperInvariant();
            if (currencyStringConverter.ContainsKey(searchString))
            {
                // Replace the defaults.
                BusinessCalendar = currencyStringConverter[searchString];
                Currency = searchString;
            }
        }

        /// <summary>
        /// Helper function used by the master initialisation function to
        /// initialise the Cap Start Date.
        /// Preconditions: private fields _calculationDate and _currency have
        /// been initialised.
        /// </summary>
        /// <param name="capStartLag">Number of days after the Calculation Date
        /// for the commencement of the Interest Rate Caps with quoted market
        /// Par volatilities.</param>
        private void InitialiseCapStartDate(int capStartLag)
        {
            // Add the lag to the Calculation Date.
            CapStartDate = CalculationDate.AddDays(capStartLag);

            // Apply the FOLLOWING roll convention.
            IBusinessCalendar businessCalendar =
                new BusinessCalendar(BusinessCalendar);

            CapStartDate = businessCalendar.Roll
                (CapStartDate, BusinessDayConventionEnum.FOLLOWING);
        }        

        /// <summary>
        /// Helper function used by the master initialisation function to
        /// initialise the Day Count.
        /// Precondition: InitialiseBusinessCalendarAndCurrency method has
        /// been called.
        /// </summary>
        private void InitialiseDayCount()
        {
            // Load the day count converter.
            var dayCountConverter =
                new SortedList<string, string>
                    {
                        {"AUD", "ACT/365.FIXED"},
                        {"USD", "ACT/360"},
                        {"GBP", "ACT/365.FIXED"},
                        {"EUR", "ACT/360"},
                        {"NZD", "ACT/365.FIXED"}
                    };

            // Set default.
            DayCount = "ACT/365.FIXED";

            if (dayCountConverter.ContainsKey(Currency))
            {
                // Replace default.
                DayCount = dayCountConverter[Currency];
            }
        }

        /// <summary>
        /// Master function used to initialise all private fields.
        /// </summary>
        /// <param name="calculationDate">Reference date to which all cashflows
        /// are discounted.</param>
        /// <param name="capFrequency">Number of Caplets per year.</param>
        /// <param name="capStartLag">Number of days after the Calculation Date
        /// for the commencement of the Interest Rate Caps with quoted market
        /// Par volatilities.
        /// Precondition: cap start lag cannot be negative.</param>
        /// <param name="currency">Three letter currency code.</param>
        /// <param name="handle">Name that identifies the object that stores
        /// the settings for the Caplet bootstrap procedure.
        /// Precondition: handle cannot be an empty string.</param>
        /// <param name="parVolatilityInterpolatioin">One dimensional interpolation
        /// methodology for par volatilities.</param>
        /// <param name="rollConvention">Swap roll convention that will be used
        /// to generate the schedule of Caplet expiries.</param>
        private void InitialisePrivateFields
            (DateTime calculationDate,
             CapFrequency capFrequency,
             int capStartLag,
             string currency,
             string handle,
             ParVolatilityInterpolationType parVolatilityInterpolatioin,
             BusinessDayConventionEnum rollConvention)
        {
            // Map the function arguments to the appropriate private field.
            CalculationDate = calculationDate;
            CapFrequency = capFrequency;
            _handle = handle;
            _parVolatilityInterpolation = parVolatilityInterpolatioin;
            RollConvention = rollConvention;
            InitialiseBusinessCalendarAndCurrency(currency);
            InitialiseCapStartDate(capStartLag);
            InitialiseDayCount();
        }

        /// <summary>
        /// Helper function used to validate the arguments used in the
        /// constructor.
        /// </summary>
        /// <param name="capStartLag">Number of days after the calculation date
        /// in which the Cap commences.
        /// Precondition: cap start lag cannot be negative.</param>
        /// <param name="handle">Name that identifies the object that stores
        /// the settings for the Caplet bootstrap procedure.
        /// Precondition: handle cannot be an empty string.</param>
        private static void ValidateConstructorArguments
            (int capStartLag, string handle)
        {        

            // Validate the Cap start lag.
            const string CapStartLagErrorMessage =
                "Cap start lag cannot be a negative number of days";
            DataQualityValidator.ValidateMinimum
                ((decimal)capStartLag, 0.0m, CapStartLagErrorMessage, true);            

            // Validate the handle.
            const string HandleErrorMessage =
                "Caplet bootstrap settings handle cannot be empty";
            DataQualityValidator.ValidateNonEmptyString
                (handle, HandleErrorMessage, true);
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Unique name that identifies the object that stores the settings 
        /// for the Caplet bootstrap procedure.
        /// </summary>
        private string _handle;

        /// <summary>
        /// One dimensional interpolation methodology for par volatilities.
        /// </summary>
        private ParVolatilityInterpolationType _parVolatilityInterpolation;

        #endregion
    }
}