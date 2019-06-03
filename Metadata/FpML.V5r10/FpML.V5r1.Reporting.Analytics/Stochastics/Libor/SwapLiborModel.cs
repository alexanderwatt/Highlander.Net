//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Orion.Numerics.LinearAlgebra;
//using Orion.Numerics.Maths;
//using Orion.Numerics.FiniteDifferences;

//namespace Orion.Analytics.Stochastics.Libor
//{
//    public enum MODEL { LIBOR = 0, SWAPRATE, LIBOR_X, LIBOR_Y, SWAPNORMAL , BDT};
//    public enum PRODUCT { BERMUDAN_SWAPTION }
//    public class SwapLiborModel
//    {
		
//        public delegate void target_model(int i);
//        public delegate void target_function(int i, double[] array );


//        #region Private Data Members

//        private const double nominal = 1000000.0;
//        private const double dfshock = 0.0000001;
//        private const double mushock = 0.0000001;
//        private const double volshock = 0.000005;

//        private const int maxT = 100;
//        private const int maxPay = 2;
//        private const int MAXMAT = 50;

//        private const double MagicSeven = 7.0;
//        private const double maxsig = 3.0;

//        private int T;
//        private int Ts;
//        private int Tfv;

//        private int halfindex;
//        private int maxlgrid;

//        private int[] lgrid;
//        private int[] lygrid;

//        private double[] sigma;
//        private double[] truesigma;
//        private double[] L;
//        private double[] t;
//        private double[] mu;
//        private double[] SR;
//        private double[] trueL;
//        private double[] trueSR;

//        private double[] alpha;
//        private double[] truealpha;
//        private double[] capvol;
//        private double[] swapvol;

//        private double[] ddt;
//        private double[] dt;
//        private double[] fix_ddt;
//        private double[] flt_ddt;
//        private double[] nullgrid;
//        private double[] xroots;

//        private double[] discfac;
//        private double[] varsum;
//        private double[] betapoint;
//        private double[] gammaratio;

//        private double[] fxvol;
//        private double[] fxfwd;
//        private double[] fxmu;
//        private double[] fxoffset;

//        private double fxstart;
//        private double fxrho;
//        private double fxrhobar;
//        private double fxavevol;


//        private int[] cflags;
//        private int[] aflags;
//        private int[] bflags;

//        private double[] extradiscfac;

//        private Matrix grid;
//        private Matrix vgrid;
//        private Matrix backupgrid;

//        private Matrix3d fvgrid;
//        private Matrix3d cgridpd;
//        private Matrix3d clgridpd;
//        private Matrix3d crgridpd;

//        private Matrix igrid;
//        private Matrix cgrid;
//        private Matrix clgrid;
//        private Matrix crgrid;
//        private Matrix pengrid;
//        private Matrix tmpvgrid;
//        private Matrix libgrid;
//        private Matrix numgrid;
//        private Matrix disgrid;
//        private Matrix offgrid;
//        private Matrix vargrid;
//        private Matrix projgrid;
//        private Matrix projgridy;
//        private Matrix bpgrid;

//        private double[] tmpgrid;


//        private Matrix ygrid;
//        private Matrix xroots2d;
//        private Matrix yroots2d;

//        private Matrix3d vgrid2d;
//        private Matrix3d cgrid2d;
//        private Matrix3d clgrid2d;
//        private Matrix3d crgrid2d;
//        private Matrix3d igrid2d;
//        private Matrix3d rhogrid;
//        private Matrix3d pengrid2d;
//        private Matrix tmpgrid2d;
//        private Matrix nullgrid2d;

//        private Matrix3d wmatrix = new Matrix3d(MAXMAT, MAXMAT, MAXMAT);
//        private Matrix3d jacob = new Matrix3d(50, 2, 2);
//        private Matrix4d offgrid2d = new Matrix4d(MAXMAT, MAXMAT, MAXMAT, 3);
//        private Matrix4d vargrid2d = new Matrix4d(MAXMAT, MAXMAT, MAXMAT, 3);

//        private int cal_start;

//        private double[] resetgrid;
//        private double[] saferesetgrid;

//        private bool twodimflag;

//        //private bool mrflag;
//        private bool swap_flag;
//        private bool log_normal;

//        private bool calibrated;

//        private Parameters param;
//        private Parameters temparam;
//        private double[] temppaysets = new double[maxPay];

//        private target_model model_setup;

//        #endregion

//        /// <summary>
//        /// set up persistent arrays
//        /// </summary>
//        void swapmodel_setup()
//        {
//            mu = new double[maxT];          // process drift
//            sigma = new double[maxT];       // process volatility
//            alpha = new double[maxT];       // mean reversion parameter
//            truesigma = new double[maxT];   // original value of volatility (pre-calibration)
//            truealpha = new double[maxT];   // original value of mean-reversion (pre-calibration)
//        }

//        //double c_SwapModel(int p_TS, 
//        //                   double[] p_serial_date,
//        //                   double[] p_discount_factor,
//        //                   double[] p_volatility,
//        //                   double[] p_zerodate,
//        //                   double[] p_libor_accfac,
//        //                   double[] p_float_accfac,
//        //                   double[] p_fix_accfix,
//        //                   double[] p_local_vol, 
//        //                   double p_meanrevert,
//        //                   int p_start_date,
//        //                   int p_end_date, 
//        //                   MODEL p_model, 
//        //                   bool p_calib,
//        //                   bool p_dyingtradefalg, 
//        //                   int p_numpaysets)

//        public double c_SwapModel (Parameters parms)
//        {

//            int i;
//            int[] serial_date;
//            bool mrflag;
//            double[] discount_factor;
//            double[] volatility;
//            double[] dummy_weight;

//            param = parms;                  // globalise parameters           

//            Ts = parms.TS;                  // # of rows
//            T = Ts - 1;                     // time horizon
//            Tfv = Math.Max(T, 5);           // # of slices in fvgrid[][][]


//            serial_date = new int[parms.discount_factor.Length];
//            Array.Copy(parms.discount_factor, serial_date, parms.discount_factor.Length);

//            discount_factor = new double[parms.discount_factor.Length];
//            Array.Copy(parms.discount_factor, discount_factor, parms.discount_factor.Length);

//            volatility = new double[parms.volatility.Length];
//            Array.Copy(parms.volatility, volatility, parms.volatility.Length);


//            if ((parms.numdetpays < 0) || (parms.numdetpays > 2) || (parms.numdetpays > parms.numpaysets))
//            {
//                throw new Exception("SwapModel: numdetpays not in believable range");
//            }

//            temparam = parms;                   // set up dummy Parameters: ATTENTION clear any resetgrid info
//            temparam.numpaysets = 0;
//            temparam.numdetpays = 0;
//            temparam.paysets = temppaysets;
//            temparam.float_accfac = parms.libor_accfac;     // don't use funny floating side accfacs for calibratons


//            dummy_weight = new double[Ts];
//            for (i = 0; i < Ts; ++i)
//            {
//                dummy_weight[i] = 1.0;
//            }
//            temparam.weight = dummy_weight;


//            // check for maximum # of time points
//            if (Ts > maxT)
//            {
//                throw new Exception("SwapModel: Time horizon too big");
//            }

//            if (parms.start_date < 0)
//            {
//                throw new Exception("SwapModel: StartTime should be non-negative");
//            }

//            if (parms.end_date > T)
//            {
//                throw new Exception("SwapModel: EndTime should be no more than Ts-1");
//            }

//            #region Initialising Private Data Member

//            xroots = new double[Ts];            // location of break points for discts coupons
//            L = new double[Ts];                 // initial LIBOR rate values
//            SR = new double[Ts];                // initial swap rate values
//            trueL = new double[Ts];             // (fixed) initial LIBOR rate values
//            trueSR = new double[Ts];            // (fixed) initial swap rate values

//            discfac = new double[Ts];           // discount factors
//            capvol = new double[Ts];            // cap(let) volatilities for calibration
//            swapvol = new double[Ts];           // swaption vols for calibration

//            t = new double[Ts];                 // time schedule (act/365)
//            dt = new double[Ts];                // day counts (act/365)

//            varsum = new double[Ts];            // term variance by time t[j] ( mean-reverting models )
//            gammaratio = new double[Ts];        // sqrt( t[i]/varusm[i]/betapoint[i] (MR models)

//            aflags = new int[Ts];            // flags for analytic integral of coupon
//            bflags = new int[Ts];            // flags for break point (discty) of coupon
//            cflags = new int[Ts];            // callability flags

//            if (twodimflag)
//            {
//                fxvol = new double[Ts];
//                fxfwd = new double[Ts];
//                fxmu = new double[Ts];
//                fxoffset = new double[Ts];
//            }

//            //historic LIBOR settings and their values
//            if (parms.numpaysets > 0)
//            {
//                resetgrid = new double[parms.numpaysets];
//                saferesetgrid = new double[parms.numpaysets];
//            }
			

//            #endregion

//            #region Read In Variables

//            mrflag = false;
//            switch (parms.model)
//            {
//                //non-mean reverting models
//                case MODEL.LIBOR:
//                case MODEL.SWAPRATE:
//                case MODEL.SWAPNORMAL:
//                    {
//                        mrflag = false;
//                        break;
//                    }

//                // mean-reverting models
//                case MODEL.LIBOR_X:
//                case MODEL.LIBOR_Y:
//                    {
//                        mrflag = true;
//                        break;
//                    }
//            }

//            switch (parms.model)
//            {
//                case MODEL.LIBOR:       // LIBOR/short rate models
//                case MODEL.LIBOR_X:
//                case MODEL.LIBOR_Y:
//                    {
//                        swap_flag = false;
//                        break;
//                    }

//                case MODEL.SWAPRATE:
//                    {
//                        swap_flag = true;
//                        break;
//                    }
//            }

//            switch (parms.model)
//            {
//                case MODEL.LIBOR:      // log-normal models
//                case MODEL.LIBOR_X:
//                    {
//                        log_normal = true;
//                        break;
//                    }

//                case MODEL.LIBOR_Y:   // normal models
//                case MODEL.SWAPNORMAL:
//                    {
//                        log_normal = false;
//                        break;
//                    }
//            }     
//            #endregion

//            #region Read In Variables

//            varsum[0] = 0.0;
//            betapoint[0] = 1.0;
//            t[0] = 0.0;
//            dt[0] = 0.0;
//            dt[T] = 0.0;                // dummy setting, so that BoundsChecker stays happy

//            for (i = 1; i <= T - 1; ++i)
//            {
//                t[i] = (serial_date[i] - parms.zero_date[i]) / 365.0;
//                dt[i] = t[i] - t[i - 1];
//            }

//            discfac[0] = 1.0;
//            discfac[T] = discount_factor[T];

//            extradiscfac = new double[maxPay];
//            for (i = 0; i < maxPay; ++i)
//            {
//                extradiscfac[i] = discount_factor[i - 1];
//            }

//            //accfacsetup(parms.libor_accfac, parms.float_accfac, parms.fixed_accfac);
//            accfacsetup(parms);

//            if (mrflag)
//            {
//                truealpha = meanversion(parms.meanrevert, parms.volatility, parms.local_vol);
//            }
//            else
//            {
//                for (i = 1; i <= T - 1; ++i)
//                {
//                    truealpha[i] = 1.0;
//                }

//                for (i = 1; i <= T - 1; ++i)
//                {
//                    if (!calibrated || parms.calib)
//                    {
//                        truesigma[i] = volatility[i];
//                        capvol[i] = volatility[i];
//                        swapvol[i] = volatility[i];
//                        alpha[i] = truealpha[i];
//                        mu[i] = 0.0;
//                    }
//                    discfac[i] = discount_factor[i];
//                    varsum[i] = Math.Pow(alpha[i],2) * varsum[i - 1] + dt[i];
//                    betapoint[i] = betapoint[i - 1] / alpha[i];

//                    if (varsum[i] > 0.0)
//                    {
//                        gammaratio[i] = Math.Sqrt(t[i] / varsum[i]) / betapoint[i];
//                    }
//                    else
//                    {
//                        gammaratio[i] = 1.0 / betapoint[i];
//                    }
//                }
				
//              }
//              spotratecalc();

//              for (i = 1; i < T; ++i)
//              {
//                    trueL[i] = L[i];
//                    trueSR[i] = SR[i];
//              }

			  
//            #endregion

//            #region Grid Setup Routine

//              if (!parms.dyingtradeflag)
//              {
//                  grid_setup(param.index);
//              }

//            #endregion

//              #region  Call Calibration and Model Setup Routine

//              if (!parms.dyingtradeflag)
//              {
//                  calib_setup(parms);
//              }

//              #endregion

//              return 0.0;

//        }

//        #region Product Setup
//        private void productsetup(Parameters param)
//        {
//            switch (param.product)
//            {
//                case PRODUCT.BERMUDAN_SWAPTION:
//                    {
//                        bermudan_swaption(param);
//                    } break;
//            }
//        }
//        #endregion


//        void bermudan_swaption(Parameters param)
//        {
//            int j;
 
//            swap(param);
//            for (j = 1; j < T; j++)
//            {
//                if ((j >= param.start_date) && (j < param.end_date))
//                        cflags[j] = param.incflags[j];
//            }

		   
//        }

//        void swap(Parameters param)
//        {
//            int i, j;
//            int payrxflag = param.maxcaps;
//            double payrx;
 
//             clear(param);
//             if (param.start_date == 0)
//             {
//                 throw new Exception("swapmodel: swap has bad start_date");
//             }

//             if (payrxflag == 0) /* payrxflag = 0 : payer's swaption, = 1 : receiver's */
//             {
//                 payrx = +1.0;
//             }
//             else
//             {
//                 payrx = -1.0;
//             }

//             for (j = 1; j <= T; j++)
//             {
//                 if ((j > param.start_date) && (j <= param.end_date))
//                 {
//                     for (i = 1; i <= lgrid[j]; i++)
//                     {
//                         cgrid[j,i] = payrx * (flt_ddt[j] * (libgrid[j,i] + param.spread[j - 1])
//                                - fix_ddt[j] * param.strike_rate[j - 1]) * nominal * param.weight[j - 1];
//                     }
//                 }
//             }

//             for (i = 0; i < param.numpaysets; i++)
//             {
//                 resetgrid[i] = payrx * (flt_ddt[1 - i] * (param.paysets[i] + param.spread[-i])
//                     - fix_ddt[1 - i] * param.strike_rate[-i]) * nominal * param.weight[-i];
//             }

//    }

 

//        #region Accrual Factor Setup
//        /// <summary>
//        /// Accrual factor setup
//        /// </summary>
//        /// <param name="?"></param>
//        private void accfacsetup(Parameters parms)
//        {
//            // offset by one so array range is [2..T]
//            ddt = new double[parms.libor_accfac.Length - 1];
//            Array.Copy(parms.libor_accfac, 1, ddt, 0, parms.libor_accfac.Length - 1);

//            flt_ddt = new double[parms.float_accfac.Length - 1];
//            Array.Copy(parms.float_accfac, 1, flt_ddt, 0, parms.float_accfac.Length - 1);

//            fix_ddt = new double[parms.fixed_accfac.Length - 1];
//            Array.Copy(parms.fixed_accfac, 1, fix_ddt, 0, parms.fixed_accfac.Length - 1);
//        }
//        #endregion

//        #region Mean Reversion
//        /// <summary>
//        /// Calculating Alpha for mean reversion
//        /// </summary>
//        /// <param name="mrparam"></param>
//        /// <param name="vol"></param>
//        /// <param name="localvol"></param>
//        /// <returns></returns>
//        private double[] meanversion(double mrparam,
//                                 double[] vol,
//                                 double[] localvol
//                                 )
//        {
//            int i;
//            double beta;
//            double sumbeta;
//            double tempvar;
//            double tempvol;
//            double[] alphas;

//            alphas =  new double[T];

//            if (localvol[1] > 0.0)
//            {
//                alphas[1] = 1.0;
//                beta = 1.0;
//                sumbeta = Math.Pow(beta, 2) * dt[1];

//                for (i = 2; i < T - 1; ++i)
//                {
//                    if (sumbeta > 0.0)
//                    {
//                        tempvar = (Math.Pow(vol[i], 2) * t[i] - Math.Pow(localvol[i], 2) * dt[i]) / sumbeta;
//                    }
//                    else
//                    {
//                        tempvar = Math.Pow(vol[i], 2);
//                    }

//                    if (tempvar > 0.0)
//                    {
//                        tempvol = Math.Pow(tempvar,2);
//                    }
//                    else
//                    {
//                        tempvol = vol[i] * Math.Sqrt(t[i - 1] / sumbeta);
//                    }

//                    alphas[i] = beta * tempvol / localvol[i];
//                    beta = localvol[i]/ tempvol;
//                    sumbeta += Math.Pow(beta * dt[i], 2);
//                } 
//            } 
//            else
//            {
//                alphas[1] = 1.0;

//                for (i = 2; i <= T - 1; ++i)
//                {
//                    alphas[i] = 1.0 - mrparam;
//                }
//            }
//            return alphas;
//        }
//        #endregion

//        #region Spot Rate Calculation
//        /// <summary>
//        /// Calculate the swap rate
//        /// </summary>
//        private void  spotratecalc()
//        {
//            int i, j;
//            double strip;

//            for (i = 1; i <= T; ++i)
//            {
//                L[i] = (discfac[i] / discfac[i + 1] - 1.0) / ddt[i + 1];
//            }

//            for (i = 1; i <= T; ++i)
//            {
//                // swap rate calculation
//                strip = 0.0;
//                for (j = i+1; j <= T; ++j)
//                {
//                    strip += fix_ddt[j] * discfac[j];
//                }
//                SR[i] = (discfac[i] - discfac[T]) / strip;
//            }
//        }
//        #endregion

//        #region Nearest Good Point
//        /// <summary>
//        /// Find the nearest good point
//        /// </summary>
//        /// <param name="index"></param>
//        /// <param name="xgrid"></param>
//        /// <param name="point"></param>
//        /// <returns></returns>
//        private int nearestgoodpoint( int index, double[] xgrid, double point )
//        { 
//            int halfindex, xlen;
//            double hx;

//            halfindex = (index-1)/2;
//            xlen = (int)xgrid[0];
//            hx = (xgrid[xlen] - xgrid[1])/(double)(xlen-1);
//            return Math.Min(xlen-halfindex,Math.Max(1+halfindex,1+BasicMath.irint((point-xgrid[1])/hx)));
//        } 
//        #endregion

//        #region Gaussj FixedSize
//        private void gaussj_fixedsize(Matrix a, int n)
//        { 
//            int i, j ;
//            Matrix

//            aa = new Matrix(n + 1, n + 1);
//            for( i = 1;  i <= n; ++i)
//            { 
//                for( j =1; j <=n; ++j )
//                { 
//                    aa[i, j] = a[i, j];
//                } 
//            }

//            BasicMath.gaussj(aa, n);
//            for( i=1; i <=n; ++i )
//            { 
//                for( j = 1; j<=n ; ++j )
//                { 
//                    a[i, j] = aa[i, j];
//                }
//            }
//            return;
//        }
//        #endregion

//        #region Setup Inverse Matrices
//        private void setupinversematrices( int maxindex , Matrix3d wmatx)
//        {
//            int i, j, k, msize;
//            double x;

//            for ( j=1; j<=(maxindex-1)/2 ;j++ )
//            {
//                msize = 2*j+1;
//                for (i=1 ; i<=msize ;i++)
//                {
//                    x = (double)(i-1-j);
//                    wmatx[msize,1,i] = 1.0;
//                    for (k=2 ; k<=msize ; k++)
//                    { 
//                        wmatx[msize, k, i] = wmatx[msize, k-1, i] * x;
//                    }
//                 }

//                gaussj_fixedsize(wmatx[msize], msize);
//            }
//            return;
//        }
//        #endregion

//        #region quickzc
//        //private void quickzc(int p_enddate, 
//        //                     int p_numpaysets, 
//        //                     bool dyingtradeflag,
//        //                     double[] p_libor_accfac,
//        //                     double[] p_float_accfac,
//        //                     double[] p_fixed_accfac)	

//        private void quickzc( int enddate )
//        {
//            temparam.end_date = enddate;
//            accfacsetup(temparam);
//            zero_coupon(temparam);
			
//            return;
//        }
//        #endregion

//        #region zero_coupon
//        //private void zero_coupon(int p_enddate, int p_numpaysets, bool dyingtradeflag)
//        private void zero_coupon(Parameters parms)
//        {
//            int i, j;

//            clear(parms);
//            for (j = 1; j <= T; j++)
//            {
//                for (i = 1; i <= lgrid[j]; i++)
//                {
//                    if (j == parms.end_date)
//                    {
//                        cgrid[j, i] = nominal;
//                    }
//                }
//            }
//        }
//        #endregion

//        #region Grid Size
//        private double gridsize(int index)
//        {
//            double gsize;

//            switch (index)
//            {
//                case 3:
//                    {
//                        gsize = Math.Sqrt(3);
//                        break;
//                    }
//                case 5:
//                    {
//                        gsize = Math.Sqrt(30) / 4.0;
//                        break;
//                    }
//                case 7:
//                    {
//                        gsize = MagicSeven;
//                        break;
//                    }
//                default:
//                    {
//                        throw new Exception("SwapModel: Index not equal to 3, 5 or 7");
//                    }
//            }
//            return gsize;
//        } 
//        #endregion

//        #region Grid Setup
//        /// <summary>
//        /// Grid setup
//        /// </summary>
//        /// <param name="index"></param>
//        private void grid_setup(int index)
//        {
//            int i;
//            int j;
//            int mgl;
//            int tmp;

//            double gdx;
//            double pvar;
//            double current_dx;

//            // Grid setup


//            lgrid = new int[Ts];             // length of each grid[.]
//            gdx = gridsize(index);              // standard grid spacing
//            halfindex = (index + 1) / 2;        // position of midpoint of index-size block
//            tmp = 1;                            // running maximum of grid sizes
//            lgrid[1] = 1;                       // first grid is trivial

//            for (j = 2; j <= T; ++j)
//            {
//                pvar = varsum[j - 1];           // term variance depends on mean reversion
//                if (pvar > 0.0)
//                {
//                    // half size of grid
//                    mgl = (int)Math.Max(halfindex - 1, Math.Ceiling(maxsig * Math.Sqrt(pvar / dt[j - 1] / gdx)));
//                }
//                else
//                {
//                    mgl = 0;
//                    lgrid[j] = 2 * mgl + 1;             // actual size of grid
//                    tmp = (int)Math.Max(tmp, lgrid[j]);      // update running maximum
//                }
//            }
//            maxlgrid = tmp;                     // set global grid maximum size

//            #region Array Declaration

//            grid = new Matrix(T + 1, maxlgrid + 1);          // grid of base point
//            vgrid = new Matrix(T + 1, maxlgrid + 1);         // option values vgrid[j][i] = worth of option at t_j given t_{j-1} info
//            backupgrid = new Matrix(T + 1, maxlgrid + 1);    // back up option values

//                fvgrid = new Matrix3d(Tfv, T, maxlgrid);                // flexicap values
//                cgridpd = new Matrix3d(T, maxlgrid, maxlgrid);          // path dependent coupon
//                clgridpd = new Matrix3d(T, maxlgrid, maxlgrid);         // path dependent coupon
//                crgridpd = new Matrix3d(T, maxlgrid, maxlgrid);         // path dependent coupon

//                igrid = new Matrix(T + 1, maxlgrid + 1);         // analytic integrals of coupons
//                cgrid = new Matrix(T + 1, maxlgrid + 1);         // coupons

//                clgrid = new Matrix(T + 1, maxlgrid + 1);        // left-hand-side of (discts) coupons
//                crgrid = new Matrix(T + 1, maxlgrid + 1);        // right-hand-side of (discts) coupons

//                pengrid = new Matrix(T + 1, maxlgrid + 1);       // grid of penalties (Bermudan Swaption)
//                tmpvgrid = new Matrix(T + 1, maxlgrid + 1);      // temporary grid ( flexicaps only )

//                libgrid = new Matrix(T + 1, maxlgrid + 1);       // LIBOR values ( of rate being set now ) libgrid[j][i] = L_{j-1}(t_{j-1})
//                numgrid = new Matrix(T + 1, maxlgrid + 1);       // numeraires	vgrid_j = disgrid_j * int( numgrid_{j+1} * vgrid_{j+1} )
//                disgrid = new Matrix(T + 1, maxlgrid + 1);       // discounts
//                offgrid = new Matrix(T + 1, maxlgrid + 1);       // offsets ( process means)
//                vargrid = new Matrix(T + 1, maxlgrid + 1);       // variances ( process square vols )

//                projgrid = new Matrix(T + 1, maxlgrid + 1);      // midpoint on grid of next level
//                projgridy = new Matrix(T + 1, maxlgrid + 1);    // midpoint on  grid of next level (2nd dim)

//                tmpgrid = new double[maxlgrid + 1];                    // temporary grid (crf and *recurs)
//                nullgrid = new double[maxlgrid + 1];                    // grid of zeros (for recurs)

//                bpgrid = new Matrix(T + 1, maxlgrid + 1);        // bond price values ( for vol surfaces calculations)

//                if (twodimflag)
//                {
//                    ygrid = new Matrix(T + 1, maxlgrid + 1);
//                    lygrid = new int[Ts];

//                    xroots2d = new Matrix(T + 1, maxlgrid + 1);
//                    yroots2d = new Matrix(T + 1, maxlgrid + 1);

//                    vgrid2d = new Matrix3d(T + 1, maxlgrid + 1, maxlgrid + 1);
//                    cgrid2d = new Matrix3d(T + 1, maxlgrid + 1, maxlgrid + 1);
//                    clgrid2d = new Matrix3d(T + 1, maxlgrid + 1, maxlgrid + 1);
//                    igrid2d = new Matrix3d(T + 1, maxlgrid + 1, maxlgrid + 1);
//                    rhogrid = new Matrix3d(T + 1, maxlgrid + 1, maxlgrid + 1);
//                    pengrid2d = new Matrix3d(T + 1, maxlgrid + 1, maxlgrid + 1);

//                    tmpgrid2d = new Matrix(maxlgrid + 1, maxlgrid + 1);
//                    nullgrid2d = new Matrix(maxlgrid + 1, maxlgrid + 1);

//                    for (i = 1; i <= maxlgrid; ++i)
//                    {
//                        for (int k = 1; k <= maxlgrid; ++k)
//                        {
//                            nullgrid2d[j, k] = 0.0;
//                        }
//                    }

//                    for (i = 1; i <= maxlgrid; ++i)
//                    {
//                        nullgrid[i] = 0.0;
//                    }

//                }
//                #endregion

//            #region Grid Creation
//                grid[1, 1] = 0.0;                  // first gird is trivial
//                grid[1, 0] = 1.0;                  // 0th entry used to record size of grid ( = lgrid[1] )

//                for (j = 2; j <= T; ++j)
//                {
//                    mgl = (int)(lgrid[j] - 1) / 2;         // grid half-size
//                    current_dx = gdx * Math.Sqrt(dt[j - 1]);

//                    for (i = -mgl; i <= mgl; ++i)
//                    {
//                        grid[j, i + mgl + 1] = current_dx * (double)i;         // grid construction
//                    }
//                    grid[j, 0] = (double)lgrid[j];                         // length of grid
//                }

//                for (j = 1; j < T; ++j)
//                {
//                    for (i = 1; i <= lgrid[j]; ++i)
//                    {
//                        projgrid[j, i] = nearestgoodpoint(index, grid.Row(j + 1).Data.ToArray<double>(), grid[j, i]);
//                    }
//                }

//                if (twodimflag)
//                {
//                    for (j = 1; j <= T; ++j)
//                    {
//                        lygrid[j] = lgrid[j];
//                        for (int k = 0; k <= lygrid[j]; ++k)
//                        {
//                            ygrid[j, k] = grid[j, k];
//                        }
//                    }

//                    for (j = 1; j < T; ++j)
//                    {
//                        for (i = 1; i <= lygrid[j]; ++i)
//                        {
//                            projgridy[j, i] = nearestgoodpoint(index, ygrid.Row(j + 1).Data.ToArray<double>(), ygrid[j, i]);
//                        }
//                    }
//                }
//                setupinversematrices(9, wmatrix);

//                #endregion

//        }
//        #endregion

//        #region Libor Libor
//        /// <summary>
//        /// returns the value of L_k(t_j; W(t-j)=grid[j+1,i]). NB does NOT obey (j,i) convention
//        /// Compare with libgrid[j][i] = liborlibor(j-1, j-1, i) = L_{ j-1 }(t_{ j-1} , does obey convention
//        /// </summary>
//        /// <param name="k"></param>
//        /// <param name="j"></param>
//        /// <param name="i"></param>
//        /// <returns></returns>
//        private double liborlibor(int k, int j, int i)
//        {
//            if (k <= 0)
//            {
//                throw new Exception("swapswap: L[0] requested for liborlibor\n");
//            }

//            if (j == 0)
//            {
//                return L[k];
//            }

//            return (L[k] * Math.Exp(sigma[k] * grid[j + 1,i] + (mu[k] - Math.Pow(sigma[k], 2) / 2) * t[j]));
//        }

//        #endregion

//        #region Libor Setup
//        private void libor_setup(int l)
//        { 
//            int i, j, low, hi;

//            if ( l > 0 )
//            {
//                low = l;
//                hi = l;
//            }
//            else
//            { 
//                low = l;
//                hi = T - 1;
			  
//                for( j = 1; j <= T; ++j )
//                {
//                    for( i=1; i <= lgrid[j]; ++i )
//                    { 
//                        numgrid[j, i] = 1.0;
//                        vgrid[j, i] = dt[j];
//                    } 
//                }
//            }

//            for ( j = low; j <= hi; ++j )
//            { 
//                for( i = 1; i <= lgrid[j+1]; ++i )
//                {
//                    libgrid[j + 1, i] = liborlibor(j, j, i);
//                }
//            } 

//            if( param.dd_flag )
//            { 
//                for( i = 1; i <= lgrid[j]; ++i )
//                { 
//                    disgrid[j,i] = 1.0/(1.0 + ddt[j+1] * L[j]);
//                    offgrid[j,i] = grid[j,i] - mu[j]/sigma[j] * dt[j];
//                } 

//            } 
//            else
//            { 
//                for( i=1; i <= lgrid[j]; ++i )
//                { 
//                    disgrid[j, i] = 1.0/(1.0 + ddt[j+1]* liborlibor(j, j-1, i));
//                    offgrid[j, i] = grid[j, i] - mu[j]/sigma[j]*dt[j];
//                }
//            }
//        }
//        #endregion

//        #region Libor Precall
//        private void libor_precall()
//        {
//            int j;
//            double sum;

//            sum = 0.0;
//            for (j = 1; j < T; ++j)
//            {
//                 sum += sigma[j]*(ddt[j+1]*L[j]/(1+ddt[j+1]*L[j]));
//                 mu[j] = sum*sigma[j];
//            }
//        }
//        #endregion

//        private void cap(Parameters parm)
//        {

//            int i, j;

//            clear(parm);
//            if (parm.start_date == 0)
//            {
//                throw new Exception("swapmodel: cap has bad start date");
//            }

//            for (j = 1; j <= T; ++j)
//            {
//                if ((j <= parm.end_date) && (j > parm.start_date))
//                {
//                    for (i = 1; i <= lgrid[j]; i++)
//                    {
//                        clgrid[j, i] = 0.0;
//                        crgrid[j, i] = flt_ddt[j] * nominal * param.weight[j - 1] * (libgrid[j, i] - parm.strike_rate[j - 1]);
//                        cgrid[j, i] = Math.Max(clgrid[j, i], crgrid[j, i]);
//                    }
//                }
//            }

//            if (parm.model == MODEL.LIBOR)
//            {
//                for (j = parm.start_date; j <= param.end_date - 1; ++j)
//                {
//                    aflags[j] = 1;
//                    for (i = 1; i <= lgrid[j]; i++)
//                    {
//                        igrid[j, i] = flt_ddt[j + 1] * nominal * parm.weight[j] * disgrid[j, i] *
//                                    BasicMath.blackscholes(liborlibor(j, j - 1, i), parm.strike_rate[j], dt[j] * Math.Pow(sigma[j], 2));
//                    }
//                    xroots[j + 1] = -(Math.Log(L[j] / parm.strike_rate[j]) + (mu[j] - 0.5 * Math.Pow(sigma[j], 2)) * t[j]) / sigma[j];
//                }
//            }
//            else
//            {
//                for (j = parm.start_date + 1; j <= parm.end_date; ++j)
//                {
//                    xroots[j] = rootfind(crgrid.Column(j).Data.ToArray<double>(), grid.Column(j).Data.ToArray<double>(), parm.index);
//                }
//            }

//            for (j = parm.start_date; j <= parm.end_date - 1; ++j)
//            {
//                bflags[j] = 1;
//            }
//        }
			


//        private void caplet(double rate, int Tend)
//        {
//            double[] strk = new double[Ts];

//            for (int i = 0; i <= T; ++i)
//            {
//                strk[i] = 0.0;
//            }

//            strk[Tend - 1] = rate;
//            temparam.start_date = Tend - 1;
//            temparam.end_date = Tend;
//            temparam.strike_rate = strk;

//            cap(temparam);

//        }

//        #region True Caplet
//        private double truecaplet(double rate, int j)
//        {
//            double mcap, vcap;

//            if (j < 2)
//            {
//                throw new Exception("swapmodel: bad truecaplet requested");
//            }

//            mcap = Math.Log(L[j - 1] / rate);
//            vcap = capvol[j - 1] * Math.Sqrt(t[j - 1]);

//            return ddt[j] * discfac[j] * (L[j - 1] * BasicMath.ndist((mcap + Math.Pow(vcap, 2) / 2) / vcap) -
//                       rate * BasicMath.ndist((mcap - Math.Pow(vcap,2) / 2) / vcap));

//        } 
//        #endregion
						  


//        private void libor_loop(int fc, double[] arr)
//        {
//            double zcobs, zcact, capobs, capact;

//            quickzc(fc + 1);
//            zcobs = fastcalc(fc + 1);
//            caplet(trueL[fc], fc + 1);
//            capobs = fastcalc(fc + 1);
//            zcact = nominal * discfac[fc + 1];
//            capact = nominal * truecaplet(trueL[fc], fc + 1);
//            arr[0] = zcobs - zcact;
//            arr[1] = capobs - capact;
//            arr[2] = zcobs;
//            arr[3] = capobs;
			
//        }



//        private void calib_setup(Parameters param)
//        {
//            cal_start = 1;
//            while (dt[cal_start] < 0.1)	// finds the first period at least 0.1Y long
//            {					// to avoid calibrating over very short periods
//                cal_start++;
//            }

//            if (param.calib || !calibrated)
//            {
//                calibrate();
//            }
//            else
//            {
//                switch (param.model)
//                {
//                    case MODEL.LIBOR:           //log-normal LIBOR model
//                        {
//                            libor_setup(0);
//                            break;
//                        }
//                }
//            }

//            if (twodimflag)
//            {
//                twofacFXsetup(param.fxvol, param.fxfwd, param.fx_start, param.fx_rho);
//            }

//            return;

//        }

//        public void calibrate()
//        {
//            int i, numloops, base_cal;
//            target_function target_loop;

//            base_cal = 2;

//            while ((base_cal - 1 <= T - 1) && (dt[base_cal - 1] == 0.0))
//            {
//                base_cal++;
//            }

//            for (i = 1; i < T; i++)
//            {
//                sigma[i] = truesigma[i];
//                mu[i] = 0.0;
//            }

//            switch (param.model)
//            {
//                case MODEL.LIBOR:
//                    {
//                        if (param.dd_flag)
//                        {
//                            libor_setup(0);
//                        }
//                        else
//                        {
//                            libor_precall();
//                            libor_setup(0);
//                        }
//                        break;
//                    }
//            }

//            switch (param.model)
//            {
//                case MODEL.LIBOR:
//                    {
//                        model_setup = new target_model(libor_setup);
//                        target_loop =new target_function(libor_loop);
//                        numloops = 1;
//                        cal_start = Math.Max(cal_start, base_cal);
//                        break;
//                    }
//            }

			

			





//        }

//        private void fullcal(target_model setup, int fullcalstart, int numloops, double mushock,
//                              double relvolshock, target_function target_loop)
//        {

//            double chmu, chvol, det, volshock;
//            double oldmu, oldsigma, jxx, jxy, jyx, jyy;
//            int sc, dlp, errflag, firstcal, overcal, calinc, loop_cnt;

//            int[] jflag = new int[50];
//            Matrix pts = new Matrix(3, 4);
//            double tolerance = 5.0;

//            errflag = 0;
//            if (numloops > 0)
//            {
//                firstcal = fullcalstart;
//                overcal = T;
//                calinc = +1;
//            }
//            else
//            {
//                firstcal = T - 1;
//                overcal = fullcalstart - 1;
//                calinc = -1;
//            }


//            for (sc = fullcalstart; sc < T; sc++)
//            {
//                jflag[sc] = 0;
//            }

//            for (dlp = 1; dlp <= Math.Abs(numloops); dlp++)
//            {
//                for (sc = firstcal; (sc != overcal) && (errflag != 1); sc += calinc)
//                {
//                    oldmu = mu[sc];
//                    oldsigma = sigma[sc];
//                    volshock = relvolshock * oldsigma;
//                    target_loop(sc, pts.Column(0).Data.ToArray<double>());

//                    if (jflag[sc] != 1)
//                    {
//                        mu[sc] += mushock;
//                        setup(sc);
//                        target_loop(sc, pts.Column(1).Data.ToArray<double>());


//                        mu[sc] = oldmu;
//                        sigma[sc] += volshock;

//                        setup(sc);
//                        target_loop(sc, pts.Column(2).Data.ToArray<double>());


//                        jxx = (pts[1, 0] - pts[0, 0]) / mushock;
//                        jxy = (pts[2, 0] - pts[0, 0]) / volshock;
//                        jyx = (pts[1, 1] - pts[0, 1]) / mushock;
//                        jyy = (pts[2, 1] - pts[0, 1]) / volshock;
//                        det = jxx * jyy - jxy * jyx;

//                        if (det != 0.0)
//                        {
//                            jacob[sc, 0, 0] = jyy / det;
//                            jacob[sc, 0, 1] = -jxy / det;
//                            jacob[sc, 1, 0] = -jyx / det;
//                            jacob[sc, 1, 1] = jxx / det;
//                        }
//                        else   /* det=0 indicates no mu dependence so we work only with sigma and caplet */
//                        {
//                            jacob[sc, 0, 0] = 0.0;
//                            jacob[sc, 0, 1] = 0.0;
//                            jacob[sc, 1, 0] = 0.0;
//                            jacob[sc, 1, 1] = 1.0 / jyy;
//                        }
//                        jflag[sc] = 1;
//                    }
//                    chmu = jacob[sc,0,0] * pts[0,0] + jacob[sc,0,1] * pts[0,1];
//                    chvol = jacob[sc,1,0] * pts[0,0] + jacob[sc,1,1] * pts[0,1];
//                    mu[sc] = oldmu - chmu;
//                    sigma[sc] = oldsigma - chvol;
//                  }

//                  setup(sc);
//                  target_loop(sc, pts.Column(0).Data.ToArray<double>());
//                  loop_cnt = 100;

//                  while ((Math.Max(Math.Abs(pts[0, 0]), Math.Abs(pts[0, 1])) > tolerance) && (errflag != 1))
//                  {

//                      chmu = jacob[sc, 0, 0] * pts[0, 0] + jacob[sc, 0, 1] * pts[0, 1];
//                      chvol = jacob[sc, 1, 0] * pts[0, 0] + jacob[sc, 1, 1] * pts[0, 1];
//                      mu[sc] += -chmu;
//                      sigma[sc] += -chvol;

//                      setup(sc);
//                      target_loop(sc, pts.Column(0).Data.ToArray<double>());

//                      if ((sigma[sc] < 0.0) || (sigma[sc] > 1.0) || ((Math.Abs(mu[sc]) > 1.0) && (param.model != MODEL.BDT)))
//                      {
//                          errflag = 1;
//                      }

//                      if (--loop_cnt < 0)
//                      {
//                          errflag = 1;
//                      }
//                  }
//            }
//            if (errflag == 1)
//            {
//                throw new Exception("SwapModel: Calibration failed");
//            }
//        }

//        #region twofacFXsetup
//        private void twofacFXsetup(double[] infxvol, double[] infxfwd, double infxstart, double infxrho)
//        {
//            double fxvolfac;
//            double fxadjvol;
//            double fxadjrho;
//            double fxrhofac;
//            double fxpifac;

//            int i, j, k;


//            fxstart = infxstart;
//            fxrho = infxrho;
//            fxrhobar = Math.Sqrt(1 - Math.Pow(fxrho,2));

//            for (i = 0; i <= T; ++i)
//            {
//                fxvol[i] = infxvol[i];
//                fxfwd[i] = infxfwd[i];
//            }

//            fxfwd[0] = infxstart;
//            fxavevol = 0.0;

//            for (i = 1; i <= T - 1; ++i)
//            {
//                fxavevol += fxvol[i];
//            }

//            fxavevol = fxavevol / (double)(T - 1);
//            fxoffset[0] = 0.0;

//            for (j = 1; j <= T - 1; ++j)
//            {
//                fxmu[j] = Math.Log(fxfwd[j] / fxfwd[j - 1] / dt[j]);
//                fxoffset[j] = fxoffset[j - 1] + (fxmu[j] - 0.5 * Math.Pow(fxvol[j],2)) * dt[j];
//                fxvolfac = fxvol[j] / fxavevol;

//                fxrhofac = fxrho / fxrhobar;
//                fxpifac = fxrhofac * (fxvolfac - 1.0);

//                fxadjvol = dt[j] * (Math.Pow(fxvolfac, 2) + Math.Pow(fxpifac, 2));
//                fxadjrho = fxpifac / Math.Sqrt(fxadjvol / dt[j]);

//                for (i = 1; i <= lgrid[j]; i++)
//                {
//                    for (k = 1; k <= lygrid[j]; k++)
//                    {
//                        offgrid2d[j, i, k, 1] = offgrid[j, i];
//                        offgrid2d[j, i, k, 2] = ygrid[j, k];
//                        vargrid2d[j, i, k, 1] = vargrid[j, i];
//                        vargrid2d[j, i, k, 2] = fxadjvol;
//                        rhogrid[j, i, k] = fxadjrho;
//                    }
//                }
//            }

//            for (j = 1; j <= T - 1; ++j)
//            {
//                //exp_function(j + 1, fxrho * fxavevol);
//               // offseterror = Math.Log( discfac[j+1]/fastcalc

//            }

//        }
//        #endregion

//        #region recurs
//        private double recurs(int j)//, bool prec, int index)
//        {
//            int i;
//            double discount;

//            if (dt[j] > 0.0)
//            {
//                for (i = 1; i <= lgrid[j + 1]; ++i)
//                {
//                    tmpgrid[i] = vgrid[j + 1, i] + cgrid[j + 1, i];
//                }

//                if (cflags[j] == 1)
//                {
//                    generalmaxint(tmpgrid, pengrid.Column(j + 1).Data.ToArray<double>(), j, bflags[j],
//                                        vgrid.Column(j).Data.ToArray<double>());
//                }
//                else
//                {
//                    generalint(tmpgrid, j, aflags[j], bflags[j], vgrid.Column(j).Data.ToArray<double>());
//                }
//            }
//            else
//            {
//                discount = discfac[j + 1] / discfac[j];
//                if ((cflags[j] == 1) && (j > 0))
//                {
//                    vgrid[j, 1] = discount * Math.Max(vgrid[j + 1, 1] + cgrid[j + 1, 1], pengrid[j + 1, 1]);
//                }
//                else
//                {
//                    vgrid[j, 1] = discount * (vgrid[j + 1, 1] + cgrid[j + 1, 1]);
//                }
//            }
//            return vgrid[j,1];
//        }
//        #endregion




//        #region fastcalc
//        private double fastcalc(int j)// bool prec, int index)
//        {
//            int k;

//            for (k = j - 1; k > 0; --k)
//            {
//                recurs(j);
//            }

//            return recurs(0); ;
//        }
//        #endregion

//        #region exp_function
//        private void exp_function(int Tend, 
//                                  double alfac)
								 
//        {
//            int i, j;

//            clear(param);

//            for (j = 1; j <= T; ++j)
//            {
//                for (i = 1; i <= lgrid[j]; ++i)
//                {
//                    if( j == Tend )
//                    { 
//                        cgrid[j, i] = Math.Exp(alfac * ( grid[j,i] + mu[j-1]/sigma[j-1]*t[j-1] -
//                                        0.5* Math.Pow(alfac, 2) * t[j-1] ));
//                    }
//                } 
//            } 
//        }
//        #endregion

//        #region Clear
//        /// <summary>
//        /// initialises variables and flags
//        /// </summary>
//        /// <param name="numpaysets"></param>
//        /// <param name="dyingtradeflag"></param>
//        //private void clear(int numpaysets, bool dyingtradeflag)
//        private void clear(Parameters prams)
//        {
//            int i, j, k;

//            for (i = 0; i < prams.numpaysets; ++i)
//            {
//                resetgrid[i] = 0.0;
//            }

//            if (prams.dyingtradeflag)
//                return;

//            for (i = 0; i <= T; i++)
//            {
//                cflags[i] = 0;
//                aflags[i] = 0;
//                bflags[i] = 0;
//                xroots[i] = -100.0; //????? -DBL_ LOG_MAX
//            }


//            for (j = 0; j <= T; ++i)
//            {
//                for (i = 1; i <= lgrid[j]; ++i)
//                {
//                    vgrid[j, i] = 0.0;
//                    cgrid[j, i] = 0.0;
//                    igrid[j, i] = 0.0;
//                    pengrid[j, i] = 0.0;

//                    for (k = 0; k <= Tfv; ++k)
//                    {
//                        fvgrid[k, j, i] = 0.0;
//                    }
//                }
//            }

//            for (i = 0; i < prams.numpaysets; ++i)
//            {
//                resetgrid[i] = 0.0;
//            }
//        }
//        #endregion

//        #region Clear2d
//        //private void clear2d(int numpaysets, bool dyingTradeflag)
//        private void clear2d(Parameters parms)
//        {
//            int i, j, k;

//            clear(parms);

//            if (parms.dyingtradeflag)
//                return;

//            for (j = 1; j <= T; ++j)
//            {
//                for (i = 1; i <= lgrid[j]; ++i)
//                {
//                    for (k = 1; k <= lygrid[j]; ++k)
//                    {
//                        vgrid2d[j, i, k] = 0.0;
//                        cgrid2d[j, i, k] = 0.0;
//                        clgrid2d[j, i, k] = 0.0;
//                        crgrid2d[j, i, k] = 0.0;
//                        pengrid2d[j, i, k] = 0.0;
//                    }
//                    xroots2d[j, i] = -100.0;
//                }

//                for (k = 1; k <= lygrid[j]; ++k)
//                {
//                    yroots2d[j, k] = -100.0;
//                }
//            }
//        }
//        #endregion

//        #region generalint
//        //param.index
//        //param.prec
//        private void generalint(double[] ingrid, int j, int aflag, int bflag, double[] outgrid) 
//                                //bool prec, int index)
//        {
//            double[] bbgrid;
//            double[] fn1;
//            double[] fn2;

//            int i, glen, olen;

//            if (aflag == 2)		// path dependent coupons
//            {
//                pdcint(ingrid, j, bflag, outgrid);
//                return;
//            }

//            glen = lgrid[j + 1];
//            olen = lgrid[j];
//            bbgrid = new double[glen + 1];
//            fn1 = new double[glen + 1];
//            fn2 = new double[glen + 1];

//            for (i = 1; i <= glen; i++)
//            {
//                bbgrid[i] = ingrid[i];
//                if ((aflag == 1 || bflag ==1) && (param.prec))
//                    bbgrid[i] -= cgrid[j + 1,i];
//            }

//            if ((bflag == 1)&& (aflag != 1) && param.prec)
//            {
//                for (i = 1; i <= glen; i++)
//                {
//                    fn1[i] = bbgrid[i] + clgrid[j + 1,i];
//                    fn2[i] = bbgrid[i] + crgrid[j + 1,i];
//                }

//                //numgrid[j + 1]
//                array_multiply(fn1, numgrid.Column(j + 1).Data.ToArray<double>(), glen, fn1);
//                array_multiply(fn2, numgrid.Column(j + 1).Data.ToArray<double>(), glen, fn2);
//                for (i = 1; i <= olen; i++)
//                {
//                    outgrid[i] = disgrid[j, i] * twowayint(param.index, grid.Column(j + 1).Data.ToArray<double>(), fn1, xroots[j + 1], fn2, offgrid[j, i],
//                        (int) projgrid[j,i], vargrid[j,i], wmatrix[param.index]);
//                }
//            }
//            else
//            {
//                array_multiply(bbgrid, numgrid.Column(j + 1).Data.ToArray<double>(), glen, bbgrid);
//                for (i = 1; i <= olen; i++)
//                {
//                    outgrid[i] = disgrid[j, i] * numint(param.index, grid.Column(j + 1).Data.ToArray<double>(), bbgrid, offgrid[j, i], (int) projgrid[j, i],
//                            vargrid[j,i], wmatrix[param.index]);
//                }
//            }

//            if (aflag == 1 && param.prec)
//            {
//                for (i = 1; i <= olen; i++)
//                    outgrid[i] += igrid[j,i];
//            }
//            return;
//        }
//        #endregion

//        #region generalmaxint
//        //param.prec
//        //param.index
//        public void generalmaxint(double[] onegrid, double[] twogrid,
//                                  int j, int bflag, double[] outgrid)
//                                  //bool prec, int index)
//        {
//            double[] bbgrid;
//            double[] ccgrid;
//            double[] ddgrid;
//            double[] f1grid;
//            double[] f2grid;
//            double[] f3grid;

//            double bk1;
//            double bk2;
//            double mroot;

//            double[] outvec = new double[4];
//            double[] numr;
//            int i, ilen, olen, eflag, part, modecode;

//            ilen = lgrid[j + 1];
//            olen = lgrid[j];

//            bbgrid = new double[ilen + 1];
//            ccgrid = new double[ilen + 1];
//            ddgrid = new double[ilen + 1];
//            f1grid = new double[ilen + 1];
//            f2grid = new double[ilen + 1];
//            f3grid = new double[ilen + 1];


//            for (i = 1; i <= ilen; i++)
//            {
//                bbgrid[i] = onegrid[i];
//                ccgrid[i] = twogrid[i];
//                ddgrid[i] = onegrid[i] - twogrid[i];
//            }

//            if (param.prec && (bflag > 0))
//            {
//                for (i = 1; i <= ilen; i++)
//                {
//                    f1grid[i] = ddgrid[i] - cgrid[j + 1,i] + clgrid[j + 1,i];
//                    f2grid[i] = ddgrid[i] - cgrid[j + 1,i] + crgrid[j + 1,i];
//                }
//            }

//            part = 0;
//            bk1 = -100.0; bk2 = -100.0;
//            if (j == 7)
//            { part = 0; }

//            int flag;
//            if ((bflag == 1) && param.prec)
//            {
//                flag = 1;
//            }
//            else
//            {
//                flag = 0;
//            }
//            eflag = 0;

//            mroot = maxrootfind(grid.Column(j + 1).Data.ToArray<double>(), ddgrid, f1grid, xroots[j + 1], f2grid, flag, eflag);

//            numr = numgrid.Column(j+1).Data.ToArray<double>();
//            if (param.prec && (bflag == 1) && (eflag > 0))
//            {
//                for (i = 1; i <= ilen; ++i)
//                {
//                    bbgrid[i] -= cgrid[j + 1,i];
//                }

//                if (eflag == 1)
//                {
//                    for (i = 1; i <= ilen; i++)
//                    {
//                        f1grid[i] = bbgrid[i] + clgrid[j + 1,i];
//                        f2grid[i] = bbgrid[i] + crgrid[j + 1,i];
//                        f3grid[i] = ccgrid[i];
//                    }
//                    bk1 = xroots[j + 1];
//                    bk2 = mroot;

//                    if (bk1 < bk2)
//                    {
//                        part = 1;
//                        modecode = 1;
//                    }
//                    else
//                    {
//                        part = 3;
//                        modecode = 2;
//                    }
//                }
//                else
//                {
//                    for (i = 1; i <= ilen; i++)
//                    {
//                        f1grid[i] = ccgrid[i];
//                        f2grid[i] = bbgrid[i] + clgrid[j + 1,i];
//                        f3grid[i] = bbgrid[i] + crgrid[j + 1,i];
//                    }
//                    bk1 = mroot;
//                    bk2 = xroots[j + 1];

//                    if (bk1 < bk2)
//                    {
//                        part = 1;
//                        modecode = 3;
//                    }
//                    else
//                    {
//                        part = 2;
//                        modecode = 4;
//                    }
//                }

//                array_multiply(f1grid, numr, ilen, f1grid);
//                array_multiply(f2grid, numr, ilen, f2grid);
//                array_multiply(f3grid, numr, ilen, f3grid);

//                for (i = 1; i <= olen; i++)
//                {
//                    threewayint(param.index, grid.Column(j + 1).Data.ToArray<double>(), f1grid, bk1, f2grid, bk2, f3grid, offgrid[j, i], (int)projgrid[j, i],
//                    vargrid[j, i], wmatrix[param.index], outvec);
//                    outgrid[i] = outvec[part];
//                }
//            }
//            else
//            {
//                if (eflag > 0)
//                {
//                    if (eflag == 1)
//                    {
//                        array_multiply(bbgrid, numr, ilen, f1grid);
//                        array_multiply(ccgrid, numr, ilen, f2grid);
//                        modecode = 5;
//                    }
//                    else
//                    {
//                        array_multiply(ccgrid, numr, ilen, f1grid);
//                        array_multiply(bbgrid, numr, ilen, f2grid);
//                        modecode = 6;
//                    }

//                    for (i = 1; i <= olen; i++)
//                    {
//                        outgrid[i] = twowayint(param.index, grid.Column(j + 1).Data.ToArray<double>(), f1grid, mroot, f2grid,
//                                        offgrid[j, i], (int) projgrid[j, i], vargrid[j, i], wmatrix[param.index]);
//                    }
//                }

//                else
//                {
//                    for (i = 1; i <= ilen; ++i)
//                    {
//                        f1grid[i] = Math.Max(bbgrid[i], ccgrid[i]);
//                    }

//                    array_multiply(f1grid, numr, ilen, f1grid);
//                    modecode = 7;

//                    for (i = 1; i <= olen; ++i)
//                    {
//                        outgrid[i] = numint(param.index, grid.Column(j + 1).Data.ToArray<double>(), f1grid, offgrid[j, i], (int) projgrid[j, i],
//                               vargrid[j, i], wmatrix[param.index]);
//                    }
//                }
//            }
//                array_multiply(outgrid,disgrid.Column(j).Data.ToArray<double>(),olen,outgrid);
//                return;

//        }
//        #endregion

//        #region maxrootfind
//        private double maxrootfind(double[] xgrid, double[] fgrid, double[] flgrid,
//                                   double bkpt, double[] frgrid, int bflag, int eflag)
//                                   //int index)
//        {
//            int lsign, rsign, glen;
//            bool good1, good2, twoflag;
//            double mroot1, mroot2, mroot;

//            eflag = 0;
//            glen = (int)xgrid[0];
//            twoflag = false;

//            good1 = false; 
//            good2 = false;

//            mroot = 0.0; 
//            mroot1 = 0.0; 
//            mroot2 = 0.0;


//            if (bflag == 1)
//            {
//                mroot1 = rootfind(flgrid, xgrid, param.index);
//                good1 = ((mroot1 > -99) && (mroot1 >= xgrid[1] - 2) && (mroot1 <= xgrid[glen] + 2)
//                        && (mroot1 <= bkpt));
//                mroot2 = rootfind(frgrid, xgrid, param.index);
//                good2 = ((mroot2 > -99) && (mroot2 >= xgrid[1] - 2) && (mroot2 <= xgrid[glen] + 2)
//                        && (mroot2 >= bkpt));

//                if (good1 && (!good2))
//                {
//                    mroot = mroot1;
//                    if (roughsign(flgrid[1] - flgrid[glen]) > 0)
//                    {
//                        eflag = 1;
//                    }

//                    if (roughsign(flgrid[1] - flgrid[glen]) < 0)
//                    {
//                        eflag = 2;
//                    }
//                    twoflag = true;
//                }

//                if (good2 && (!good1))
//                {
//                    mroot = mroot2;
//                    if (roughsign(frgrid[1] - frgrid[glen]) > 0)
//                    {
//                        eflag = 1;
//                    }

//                    if (roughsign(frgrid[1] - frgrid[glen]) < 0)
//                    {
//                        eflag = 2;
//                    }
//                    twoflag = true;
//                }
//            }
//                if (!twoflag)
//                {
//                    mroot=rootfind(fgrid,xgrid,param.index);
//                    lsign=roughsign(fgrid[1]);
//                    rsign=roughsign(fgrid[glen]);

//                    if (mroot>-99)
//                    {
//                        if ((lsign>=0) && (rsign<=0))
//                        { 
//                                eflag = 1;
//                        }

//                        if ((lsign<=0) && (rsign>=0))
//                        {
//                                eflag = 2;
//                        }

//                        if (mroot<xgrid[1])
//                        {
//                                mroot = xgrid[1]-7.0;
//                                if ((lsign>=0) && (rsign>=0))
//                                {
//                                        eflag = 2;
//                                }

//                                if ((lsign<=0) && (rsign<=0))
//                                {
//                                        eflag = 1;
//                                }
//                        }
//                    }
//                 }
				
//                return mroot;
//        }
//        #endregion

//        #region roughsign
//        private int roughsign(double x)
//        {
//            if (x>0.01)
//                return +1;
//            else
//            {
//                if (x<-0.01)
//                    return -1;
//                else
//                    return 0;
//            }
//        }
//        #endregion


//        #region pdcint
//        public void pdcint(double[] ingrid, int j, int bflag, double[] outgrid)
//        {
//            double[] bbgrid;
//            double[] dtab;
//            double[] ntab;
//            double[] fn1;
//            double[] fn2;
//            double xroot;

//            int i, glen, olen, k;

//            glen = lgrid[j + 1];
//            olen = lgrid[j];

//            bbgrid =new double[glen + 1];
//            ntab = new double[glen + 1];
//            fn1 = new double[glen + 1];
//            fn2 = new double[glen + 1];

//            for (i = 1; i <= glen; i++)
//            {
//                bbgrid[i] = ingrid[i];
//            }

//            dtab = disgrid.Column(j).Data.ToArray<double>();
//            if (bflag==1 && param.prec)
//            {
//                for (i=1; i <= olen; i++)
//                {
//                    for (k=1; k<=glen; k++)
//                    {
//                        fn1[k] = (bbgrid[k] + clgridpd[j+1,k, i])*numgrid[j+1, k];
//                        fn2[k] = (bbgrid[k] + crgridpd[j+1,k, i])*numgrid[j+1, k];
//                        tmpgrid[k] = clgridpd[j+1,k,i]-crgridpd[j+1, k, i];
//                    }
			
//                    xroot = rootfind(tmpgrid, grid.Column(j+1).Data.ToArray<double>(), param.index);
//                    ntab[i] = twowayint(param.index, grid.Column(j + 1).Data.ToArray<double>(), fn1, xroot, fn2, offgrid[j, i], (int) projgrid[j, i],
//                                vargrid[j,i],wmatrix[param.index]);
//                }
//            }
//            else
//            {
//                for (i=1; i <= olen; i++)
//                {
//                        for (k=1; k<=glen ;k++)
//                        {
//                                fn1[k] = (bbgrid[k] + cgridpd[j+1, k, i])*numgrid[j+1, k];
//                        }
//                        ntab[i] = numint(param.index, grid.Column(j + 1).Data.ToArray<double>(), fn1, offgrid[j, i], (int)projgrid[j, i],
//                                vargrid[j,i],wmatrix[param.index]);
//                }
//             }
	
//            array_multiply(ntab,dtab,olen,outgrid);
//            return;


//        }
//        #endregion

//        // Quadrature Integration: 3 functions with tow preset break points.
//        //                        returns three values ( three-way; two-way; two-way on bk2 )

//        #region Three Way Integration
//        private double threewayint( int index, 
//                                    double[] xgrid,
//                                    double[] fn1,
//                                    double  bkpt1,
//                                    double[] fn2,
//                                    double bkpt2,
//                                    double[] fn3,
//                                    double mean,
//                                    int inmid,
//                                    double var,
//                                    Matrix wmatx,
//                                    double[] outvec )
//    {
//         int midpt, leftpt, i, j, xlen;
//         double truemean, truevar, truebk1, truebk2;
//         double hx;

//         double[] moms = new double[MAXMAT];
//         double[] conmoms = new double[MAXMAT];
//         double[] conconmoms = new double[MAXMAT];

//         double weight, conweight, conconweight, lrsum, mainsum, rsum;
 
//         midpt = inmid;		
//         leftpt = midpt - (index+1)/2;
//         xlen = (int)xgrid[0];
//         hx = (xgrid[xlen] - xgrid[1])/(double)(xlen-1);
//         truemean = (mean-xgrid[midpt])/hx;
//         truevar = var/(hx*hx);
//         truebk1 = (bkpt1 - xgrid[midpt])/hx;
//         truebk2 = (bkpt2 - xgrid[midpt])/hx;
		
//         moms = Quadrature.genmoments(index,truemean,truevar);
//         conmoms = Quadrature.genconmoms(index, truemean, truevar, truebk1, -1);
//         conconmoms = Quadrature.genconconmoms(index, truemean, truevar, truebk1, truebk2);

//         lrsum = 0.0;
//         mainsum = 0.0;
//         rsum = 0.0;

//        for (i=1; i<=index; i++)
//        {
//            weight = 0.0;
//            conweight = 0.0;
//            conconweight = 0.0;
//            for (j=1; j<=index; j++)
//            {
//                weight += wmatx[i,j]*moms[j];
//                conweight += wmatx[i,j]*conmoms[j];
//                conconweight += wmatx[i,j]*conconmoms[j];
//            }
//            lrsum += conweight*(fn1[leftpt+i] - fn3[leftpt+i]) + weight*fn3[leftpt+i];
//            mainsum += conconweight*(fn2[leftpt+i]-fn3[leftpt+i]);
//            rsum += conconweight*(fn1[leftpt+i]-fn3[leftpt+i]);
//        }
//            outvec[1] = lrsum + mainsum;
//            outvec[2] = lrsum;
//            outvec[3] = lrsum + rsum;

//            return outvec[1];


//    }
//        #endregion

//        #region Two Way Integration
//        private double twowayint(int index, 
//                                  double[] xgrid,
//                                  double[] fn1, 
//                                  double bkpt,
//                                  double[] fn2,
//                                  double mean,
//                                  int inmid,
//                                  double var,
//                                  Matrix wmatx)
//        { 
//            int midpt, leftpt, i, j, xlen;
//            double truemean, truevar, truebreak;
//            double hx,weight, conweight, sum;

//            double[] moms = new double[MAXMAT];
//            double[] conmoms = new double[MAXMAT];

//            midpt = inmid;		
//            leftpt = midpt - (index+1)/2;
//            xlen = (int)xgrid[0];
//            hx = (xgrid[xlen] - xgrid[1])/(double)(xlen-1);
//            truemean = (mean-xgrid[midpt])/hx;
//            truevar = var/(hx*hx);
//            truebreak = (bkpt - xgrid[midpt])/hx;

//            moms = Quadrature.genmoments(index, truemean, truevar);
//            conmoms = Quadrature.genconmoms(index, truemean, truevar, truebreak, -1);

//            sum = 0.0;
//            for (i=1; i <= index; i++)
//            {
//                 weight = 0.0;
//                conweight = 0.0;
//                for (j=1; j<=index; j++)
//                {
//                    weight += wmatx[i, j]*moms[j];
//                    conweight += wmatx[i, j]*conmoms[j];
//                }
//                sum += conweight*(fn1[i+leftpt]-fn2[i+leftpt])+ weight*fn2[i+leftpt];
//            }
//            return sum;
//        }
//        #endregion
		
//        #region numint
//        private double numint( int index,
//                               double[] xgrid,
//                               double[] fn, 
//                               double mean, 
//                               int inmid, 
//                               double var,
//                               Matrix wmatx)
//        { 
//             int midpt, leftpt, i, j, xlen;
//             double hx,  weight, sum;
					
//             double[] moms = new double[MAXMAT];

//             midpt = inmid; 		
//             leftpt = midpt - (index+1)/2;
//             xlen = (int) xgrid[0];
//             hx = (xgrid[xlen] - xgrid[1])/(double)(xlen-1);
//             moms = Quadrature.genmoments(index, (mean - xgrid[midpt]) / hx, var / (hx * hx));

//             sum = 0.0;
//            for (i=1; i<=index ;i++)
//            {
//                weight = 0.0;
//                for (j=1; j<=index ;j++)
//                {
//                    weight += wmatx[i,j]*moms[j];
//                }
//                sum += weight*fn[i+leftpt];
//            }
			
//            return sum;
//        }
//        #endregion

//        #region root find
//        private double rootfind(double[] fn, double[] xgrid, int index)
//        {
//            int[] pmf = new int[5];
//            int halfindex, xlen, newlen;

//            int i, firstpos, lastpos, firstneg, lastneg, mid, midlo, midhi;
//            int pmflag, newpmflag;

//            double xroot, hx, slope;
//            double[] newfn;
//            double[] newgrid;

//            xlen = (int)xgrid[0];
//            newfn = new double[xlen + 7];
//            newgrid = new double[xlen + 7];

//            newlen = xlen;
//            hx = xgrid[2] - xgrid[1];

//            pmflag = plusminusfind(fn, xlen, pmf);
//            firstpos = pmf[1];
//            lastpos = pmf[2];
//            firstneg = pmf[3];
//            lastneg = pmf[4];

//            if (pmflag == 0)
//            {
//                for (i = 1; i <= xlen; i++)
//                {
//                    newfn[i] = fn[i];
//                    newgrid[i] = xgrid[i];
//                }
//                newlen = xlen;
//            }

//            if ((pmflag == 1) || (pmflag == 2))
//            {
//                for (i = 1; i <= xlen; ++i)
//                {
//                    newfn[i + 3] = fn[i];
//                    newgrid[i + 3] = xgrid[i];
//                }

//                for (i = 1; i <= 3; i++)
//                {
//                    newfn[4 - i] = finterp(fn, index, (double)(1 - i));
//                    double[] tmp = new double[fn.Length - xlen + index] ;
//                    fn.CopyTo(tmp, xlen -index);
//                    newfn[xlen + 3 + i] = finterp(tmp, index, (double)(index + i));
//                    newgrid[4 - i] = xgrid[1] - hx * i;
//                    newgrid[xlen + 3 + i] = xgrid[xlen] + hx * i;
//                    newlen = xlen + 6;
//                }

//                newpmflag = plusminusfind(newfn, newlen, pmf);
//                if (newpmflag == 0)				// only use extended fn if good root found
//                {
//                    firstpos = pmf[1];
//                    lastpos = pmf[2];
//                    firstneg = pmf[3];
//                    lastneg = pmf[4];
//                    pmflag = newpmflag;
//                }
//            }

//            if (pmflag == 1)					// all points negative
//            {
//                slope = fn[xlen] - fn[1];
//                if (slope < 0.0)
//                    xroot = xgrid[1] - 6.0;
//                else
//                    xroot = xgrid[xlen] + 6.0;

//                return xroot;
//            }

//            if (pmflag == 2)					// all points positive
//            {
//                slope = fn[xlen] - fn[1];
//                if (slope > 0.0)
//                    xroot = xgrid[1] - 6.0;
//                else
//                    xroot = xgrid[xlen] + 6.0;

//                return xroot;
//            }

//             if (pmflag == 3)				// multi-roots pos:neg:pos
//             {
//                 return (-100.0);
//             }

//             if (pmflag == 4)				// multi-roots neg:pos:neg
//             {
//                return (-101.0);
//             }

//             if (firstpos < firstneg)
//             {
//                 midlo = lastpos;
//                 midhi = firstneg;
//             }
//             else
//             {
//                 midlo = lastneg;
//                 midhi = firstpos;
//             }

//             mid = BasicMath.irint((midlo * Math.Abs(newfn[midhi]) + midhi * Math.Abs(newfn[midlo])) /
//                (Math.Abs(newfn[midhi]) + Math.Abs(newfn[midlo])));
//             halfindex = (index + 1) / 2;
//             mid = Math.Min(newlen + 1 - halfindex, Math.Max(halfindex, mid));
//             double[] newnewfn = new double[newfn.Length - mid + halfindex];
//             newfn.CopyTo(newnewfn, mid - halfindex);
//             xroot = rootinterp(newnewfn, index);
//             xroot = (xroot - halfindex) * hx + newgrid[mid];
			 
//             return xroot;

//        }
//        #endregion

//        #region rootinterp
//        private double rootinterp(double[] fn, int nn)
//        {
//            double hi, lo, md;
//            int flo, fhi, fmd;
//            double zerotolerance = 0.00000001;

//            lo = 1.0;
//            hi = (double)nn;
//            flo = Math.Sign(finterp(fn, nn, lo));
//            fhi = Math.Sign(finterp(fn, nn, hi));
//            while ((hi - lo) > zerotolerance)
//            {
//                md = (hi + lo) / 2;
//                fmd = Math.Sign(finterp(fn, nn, md));
//                if (flo == fmd)
//                {
//                    lo = md;
//                }
//                else
//                {
//                    if (fhi == fmd)
//                    {
//                        hi = md;
//                    }
//                    else
//                    {
//                        hi = md;
//                        lo = md;
//                    }
//                }
//            }
//            return (hi + lo) / 2;
//        }
//        #endregion

//        #region finterp
//        private double finterp(double[] fn, int nn, double x)
//        { 
//            Matrix pp;
//            int i, j;

//            pp = new Matrix(nn, nn);

//            for( i=1; i <= nn; ++i )
//            {
//                pp[nn, i] = fn[i];
//            }

//            for ( j=nn-1; j >=1;  --j )
//            { 
//                for( i= 1; i <= j; ++i )
//                { 
//                    pp[j, i] = ((x -(double) (nn+i - j)) * pp[j+1, i] +
//                                ((double)i-x)*pp[j+1,i+1])/(double)(nn-j);
//                }

//            }
//            return pp[1, 1];

//        }
//        #endregion

//        #region plusminusfind
//        private int plusminusfind( double[] fn, int nn, int[] outvec)
//        { 
//            int firstpos, firstneg, lastpos, lastneg, firstbig, lastbig, i;
//            int returncode;
//            double zerotolerance = 0.001;

//            firstbig = -1;
//            lastbig = -1;
//            for (i=1; i<= nn; i++)
//            {
//                if (Math.Abs(fn[i]) > zerotolerance)
//                {
//                    lastbig = i;
//                    if (firstbig < 0)
//                    {
//                        firstbig = i;
//                    }
//                }
//            }

//            firstpos = -1;
//            firstneg = -1;
//            lastpos = -1;
//            lastneg = -1;

//            if( firstbig != -1 )
//            {
//                for (i = firstbig; i <= lastbig; i++)
//                {
//                    if (fn[i] >= 0.0)
//                    {
//                        lastpos = i;
//                        if (firstpos < 0)
//                        {
//                            firstpos = i;
//                        }
//                    }
//                    else
//                    {
//                        lastneg = i;
//                        if (firstneg < 0)
//                        {
//                            firstneg = i;
//                        }
//                    }
//                }
//            }
//            outvec[1] = firstpos;
//            outvec[2] = lastpos;
//            outvec[3] = firstneg;
//            outvec[4] = lastneg;
//            returncode = 0;					// returns 0 if a good root isolated

//            if (firstpos == -1)
//            {
//                returncode = 1;		        // returns 1 if all values non-positive 
//            }

//            if (firstneg == -1)
//            {
//                returncode = 2;		        // returns 2 if all values non-negative
//            }

//            if ((firstpos<firstneg) && (firstneg<lastpos))
//            {
//                returncode = 3;				// returns 3 if multi-roots pos:neg:pos
//            }

//            if ((firstneg<firstpos) && (firstpos<lastneg))
//            {
//                returncode = 4;				// returns 4 if multi-roots neg:pos:neg
//            }

//            return returncode;

			
//        }
//#endregion

//        #region array_multiply
//        private void array_multiply( double[] a, double[] b, int length, double[] c)
//        { 
//            int i; 

//            for( i = 1; i <= length; ++i )
//            {
//                c[i] = a[i] * b[i];
//            } 
//        } 
//        #endregion

		
//    }
//}
