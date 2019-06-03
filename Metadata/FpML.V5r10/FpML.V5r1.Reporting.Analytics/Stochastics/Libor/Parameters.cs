//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Orion.Analytics.Stochastics.Libor
//{
//    public class Parameters
//    {
//        private int _TS;
//        private int _start_date;
//        private int _end_date;

//        private int _index;

//        private int _numpaysets;
//        private int _numdetpays;

//        private double[] _paysets;

//        private bool _dyingtradeflag;
//        private bool _calib;
//        private bool _prec;
//        private bool _dd_flag;

//        private double _meanrevert;

//        private MODEL _model;
//        private PRODUCT _product;

//        private double[] _libor_accfac;
//        private double[] _float_accfac;
//        private double[] _fixed_accfac;
//        private double[] _weight;
//        private double[] _strike_rate;

//        private double[] _serial_date;
//        private double[] _discount_factor;
//        private double[] _volatility;
//        private double[] _zero_date;
//        private double[] _local_vol;


//        private double _fx_start;
//        private double _fx_rho;

//        private double[] _fxvol;
//        private double[] _fxfwd;

//        private int[] _incflags;
//        private double[] _spread;

//        private int _maxcaps;


//        public Parameters(  int ts, int startDate, int endDate, 
//                            int numPaySets, int numDetPays,
//                            int theIndex, double[] paySets,
//                            double[] liborAccfac, double[] floatAccfac,
//                            double[] fixedAccfac, double[] theWeight,
//                            double[] strikeRates, double[] serialDate,
//                            double[] discountFactor, double[] theVolatility,
//                            double[] zeroDate,  double[] localVol, 
//                            double meanRevert,  bool theCalib, bool thePrec,
//                            bool dyingTradeFlag ,  bool theDD_flag, MODEL theModel, 
//                            double fxStart, double fxRho,
//                            double[] fxVol, double[] fxFwd, 
//                            PRODUCT theProdcut, int[] incFlags,
//                            int maxCaps, double[] theSpread
//                            )
//        {
//            _TS = ts;
//            _start_date = startDate;
//            _end_date = endDate;

//            _numpaysets = numPaySets;
//            _numdetpays = numDetPays;

//            _index = theIndex;
//            _paysets = paySets;

//            _libor_accfac = liborAccfac;
//            _float_accfac = floatAccfac;
//            _fixed_accfac = fixedAccfac;
//            _weight = theWeight;
//            _strike_rate = strikeRates;

//            _serial_date = serial_date;
//            _discount_factor = discountFactor;
//            _volatility = theVolatility;
//            _zero_date = zeroDate;
//            _local_vol = localVol;
//            _meanrevert = meanRevert;
//            _calib = theCalib;
//            _prec = thePrec;
//            _dyingtradeflag = dyingTradeFlag;
//            _model = theModel;
//            _dd_flag = theDD_flag;

//            _fx_start = fx_start;
//            _fx_rho = fxRho;

//            _fxvol = fxVol;
//            _fxfwd = fxFwd;

//            _product = theProdcut;
//            _incflags = incFlags;

//            _maxcaps = maxCaps;
//            _spread = theSpread;
            
//        }

//        public int TS
//        {
//            get { return _TS; }
//            set { _TS = value; }
//        }

//        public int start_date
//        {
//            get{   return _start_date; }
//            set{  _start_date = value;  }
//        }

//        public int end_date
//        {
//            get{ return _end_date; }
//            set{ _end_date = value;}
//        }

//        public int index
//        {
//            get { return _index; }
//            set { _index = value; }
//        } 

//        public int numpaysets
//        {
//            get { return _numpaysets; }
//            set { _numpaysets = value; }
//        }

//        public int numdetpays
//        {
//            get { return _numdetpays; }
//            set { _numdetpays = value; }
//        }

//        public double[] paysets
//        {
//            get { return _paysets; }
//            set { _paysets = value; }
//        }

//        public double meanrevert
//        { 
//            get{ return _meanrevert; }
//            set { _meanrevert = value; }
//        }

//        public bool calib
//        {
//            get { return _calib; }
//            set { _calib = value; }
//        }

//        public bool prec
//        {
//            get { return _prec; }
//            set { _prec = value; }
//        } 

//        public bool dyingtradeflag
//        {
//            get { return _dyingtradeflag; }
//            set { _dyingtradeflag = value; }
//        }

//        public bool dd_flag
//        {
//            get { return _dd_flag; }
//            set { _dd_flag = value; }
//        }

//        public MODEL model
//        {
//            get { return _model; }
//            set { _model = value; }
//        } 


//        public double[] libor_accfac
//        {
//            get{ return _libor_accfac; }
//            set { _libor_accfac = value; }
//        }

//        public double[] float_accfac
//        {
//            get { return _float_accfac; }
//            set { _float_accfac = value; }
//        }

//        public double[] fixed_accfac
//        {
//            get { return _fixed_accfac; }
//            set { _fixed_accfac = value; }
//        }

//        public double[] weight
//        { 
//            get{ return _weight; }
//            set{ _weight = value;}
//        }

//        public double[] strike_rate
//        {
//            get { return _strike_rate; }
//            set { _strike_rate = value; }
//        }

//        public double[] serial_date
//        { 
//            get{ return _serial_date; }
//            set { _serial_date = value; }
//        }

//        public double[] discount_factor
//        {
//            get { return _discount_factor; }
//            set { _discount_factor = value; }
//        }

//        public double[] volatility
//        { 
//            get { return _volatility; }
//            set { _volatility = value; }
//        } 

//        public double[] zero_date
//        { 
//            get{ return _zero_date; }
//            set{ _zero_date = value; }
//        }

//        public double[] local_vol
//        {
//            get { return _local_vol; }
//            set { _local_vol = value; }
//        }

//        public double fx_start
//        { 
//            get{ return _fx_start;  }
//            set { _fx_start = value; } 
//        } 

//        public double fx_rho
//        { 
//            get{ return _fx_rho; }
//            set{ _fx_rho = value; }
//        }

//        public double[] fxvol
//        {
//            get { return _fxvol; }
//            set { _fxvol = value; }
//        }

//        public double[] fxfwd
//        {
//            get { return _fxfwd; }
//            set { _fxfwd = value; }
//        }

//        public PRODUCT product
//        {
//            get { return _product; }
//            set { _product = value; }
//        }

//        public int[] incflags
//        { 
//            get{ return _incflags; }
//            set { _incflags = value; }
//        }

//        public int maxcaps
//        {
//            get { return _maxcaps; }
//            set { _maxcaps = value; }
//        }

//        public double[] spread
//        { 
//            get{ return _spread; }
//            set { _spread = value; }
//        } 
        
//    }
//}
