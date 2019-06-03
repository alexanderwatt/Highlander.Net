
#region Using directives

using System;
using System.Data;
using System.Diagnostics;
using Orion.Analytics.PricingEngines;
using Math=System.Math;

#endregion

namespace FpML.V5r10.Reporting.Analytics
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Option : Instrument 
    {
        ///<summary>
        ///</summary>
        public enum Type
        {
            ///<summary>
            ///</summary>
            Call,
 
            ///<summary>
            ///</summary>
            Put,
 
            ///<summary>
            ///</summary>
            Straddle
        };

        ///<summary>
        ///</summary>
        ///<param name="optionType"></param>
        ///<param name="price"></param>
        ///<param name="strike"></param>
        ///<returns></returns>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        public static double ExercisePayoff(Type optionType, double price, double strike) 
        {
            switch(optionType)
            {
                case Type.Call:
                    {
                        return Math.Max(price - strike, 0.0);
                    }
                case Type.Put:
                    {
                        return Math.Max(strike-price, 0.0);
                    }
                case Type.Straddle:
                    {
                        return Math.Abs(strike - price);
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(optionType), optionType, "TODO: Unknown option type");
                    }
            }
        }

        protected Option(IPricingEngine engine, DataRow args) 
            : this (engine, args, "", "")
        {}

        protected Option(IPricingEngine engine, DataRow args, 
                         string isinCode, string description) : base(isinCode, description)
        {
            _engine = engine;
            _args = args;
            var temp = new DataTable("Results");			
            temp.Columns.AddRange(OptionValue.Columns);        
            _results = temp.NewRow();
        }

        private readonly IPricingEngine _engine;
        private readonly DataRow _args;
        private readonly DataRow _results;

        ///<summary>
        ///</summary>
        ///<exception cref="NullReferenceException"></exception>
        public IPricingEngine PricingEngine
        {
            get => _engine;
            set
            {
                if (value == null) 
                    throw new NullReferenceException(
                        "TODO: null pricing _engine not allowed");
                
                //IS
                //this._engine = _engine;

                // this will trigger recalculation and notify observers
//				Update();
                SetupEngine();
            }
        }

        protected abstract void SetupEngine();

        /*! \warning this method simply launches the _engine and copies the 
				returned value into NPV_. It does <b>not</b> set _isExpired. 
				This should be taken care of by redefining this method in
				derived classes and calling this implementation after 
				checking for validity and only if the check succeeded.
		*/
        protected override void PerformCalculations()
        {
            SetupEngine();
            // assume 0 is the "value" column
            Debug.Assert( _results.Table.Columns.IndexOf("value") == 0);
            _results[0] = DBNull.Value;
            _engine.Validate(_args);
            _engine.Calculate(_args, _results);
            if( _results.IsNull(0) ) 
                throw new ApplicationException(
                    "no _results returned from option pricer");

            _npv = (double)_results[0];
        }
    }
}