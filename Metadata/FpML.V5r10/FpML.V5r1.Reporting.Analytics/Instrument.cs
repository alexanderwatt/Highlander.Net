
namespace FpML.V5r10.Reporting.Analytics
{
    /// <summary>
    /// Abstract instrument class.
    /// </summary>
    /// <remarks>
    /// This class is purely abstract and defines the interface of 
    /// concrete instruments which will be derived from this one.
    /// </remarks>
    public abstract class Instrument : IInstrument
    {
        ///<summary>
        ///</summary>
        protected Instrument() : this("", "") 
        {}

        protected Instrument(string isinCode, string description)
        {
            _isinCode = isinCode;
            _description = description;
        }

        protected double _npv;
		
        protected bool _isExpired;

        private readonly string _isinCode;
        private readonly string _description;
        private bool _calculated;

        /// <summary>
        /// A stringified representation of this instrument.
        /// </summary>
        /// <returns>A String representing the object.</returns>
        // string ToString();

        //protected override void Notify(object sender, System.EventArgs e)
        //{
        //    _calculated = false;

        //    base.Notify(sender, e);
        //}

        /// <summary>
        /// This method performs all needed calculations by calling
        /// the <i><b>performCalculations</b></i> method.
        /// </summary>
        /// <remarks>
        /// Instruments cache the results of the previous calculation. 
        /// Such results will be returned upon later invocations of 
        /// <i><b>calculate</b></i>. When the results depend on 
        /// arguments such as term structures which could change
        /// between invocations, the instrument must register itself 
        /// as observer of such objects for the calculations to be
        /// performed again when they change.
        /// </remarks>
        protected void Calculate()
        {
            if (!_calculated)
            {
                PerformCalculations();

                _calculated = true;
            }
        }

        /// <summary>
        /// This method must implement any calculations which must be
        /// (re)done in order to calculate the NPV of the instrument.
        /// </summary>
        abstract protected void PerformCalculations();

        #region Implementation of IInstrument

        /// <summary>
        /// Net present value of the instrument.
        /// </summary>
        /// <returns>Returns the net present value.</returns>
        public double NPV
        {
            get 
            {
                Calculate();
				
                //return (_isExpired ? 0.0 : _npv);

                return (_isExpired ? 0 : _npv);
            }
        }

        /// <summary>
        /// This method force the recalculation of the instrument value 
        /// and other results which would otherwise be cached.
        /// </summary>
        /// <remarks>
        /// Explicit invocation of this method is <b>not</b> necessary 
        /// if the instrument registered itself as observer with the 
        /// structures on which such results depend. 
        /// It is strongly advised to follow this policy when possible.
        /// </remarks>
        public void Recalculate()
        {
            PerformCalculations();

            _calculated = true;

//			OnChanged();
        }

        /// <summary>
        /// ISIN code of the instrument, when given.
        /// </summary>
        public string IsinCode
        {
            get	{ return _isinCode; }
        }

        /// <summary>
        /// A brief textual _description of the instrument.
        /// </summary>
        public string Description
        {
            get { return _description; }
        }

        /// <summary>
        /// Returns whether the instrument is still tradable.
        /// </summary>
        public bool IsExpired
        {
            get	{ Calculate(); return _isExpired; }
        }
        #endregion

    }
}