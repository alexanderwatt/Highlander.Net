using System;
using Orion.Numerics.Utilities;

namespace Orion.Analytics.NabLib
{
  ///<summary>
  ///</summary>
  public static class SmileCalculation
  {
    ///<summary>
    ///</summary>
    ///<param name="x"></param>
    ///<param name="rass"></param>
    ///<param name="tenor"></param>
    ///<param name="type"></param>
    ///<param name="spot"></param>
    ///<param name="rnum"></param>
    ///<param name="vol"></param>
    ///<param name="delta"></param>
    public delegate double SimpleFN(double x, double rass, double tenor, double type, 
                                    double spot, double rnum, double vol, double delta);

    public const int RHS_FENICS_DELTA = 1;
    public const int RHS_FENICS_FWD_DELTA = 2;
    public const int LHS_FENICS_DELTA = -1;
    public const int LHS_FENICS_FWD_DELTA = -2;

    public const int deltaPutFlag = -1;
    public const int deltaCallFlag = 1;
    public const double footscray_EPS = 0.00001;
    public const int footscray_IMAX = 100;
    public const double DAYS_IN_YEAR = 365.25;

    public const int OPT_PUT = 0;
    public const int OPT_CALL = 1;

    public const int NAB_FLAT	= 0;		        /* flat both ends */
    public const int NAB_EXTRAPOLATE = 1;		  /* extrapolate both ends */
    public const int NAB_S_FLAT_E_EXTRAP = 2;	/* flat before first point, extrapolate after last point */
    public const int NAB_S_EXTRAP_E_FLAT = 3;	/* extrapolate before first point, flat after last point */

    public const int FIT_FIRST_TWO = 0;
    public const int FIT_SECOND_TWO = 1;

    public const int SUCCESS = 0;
    public const int FAILURE = -1;

    public const double DBL_THRESHHOLD = 1.0e-13;	/* good to 12 decimal places */

    public static double CalculateVol(int deltaType,              // FXGetDeltaType returns 2 values, use the first if the option expiry <= 1 yr , otherwise pass in the second
                                      int callOrPut,              // call or put
                                      double forward,             // forward
                                      double deltaOrStrike,       // delta or strike required for the volatility
                                      long actualDaysToExpiry,    // actual number of days until expiry
                                      double volatility,          // On input the ATM volatility
                                      double numeraireDf,         // numeraire discount factor
                                      double assetDf,             // asset discount factor
                                      double[] deltaArray,        // array[5] of deltas we solve for. eg 0.1, 0.25, 0, 0.25, 0.1
                                      int[] deltaPutCallFlags,    // array[5] of put/call flags for deltas eg 1, 1, 0, -1, -1.
                                      double[] deltaVols,         // array[5] of vols for deltas in "deltaPutCallFlags".
                                      int typeSupplied            // type of interpolation 1 for delta, 0 for strike
                                      )
    {
      double ksi, d1, rhs_delta, old_vol;
      int iters;
      double old_jump, new_jump, omega;

      int m = deltaArray.Length - 1;
      int icode = 0;

      switch (typeSupplied) 
      {
        case 1: // delta 

          switch (deltaType)
          {
            case RHS_FENICS_DELTA:
            case RHS_FENICS_FWD_DELTA:
              // first convert each element of delta array into 
              // an asset call RHS_FENICS_DELTA */
              for (int j = 0; j <= m; j++)
              {
                deltaArray[j] = convert_to_rhs_call_delta(deltaType, deltaPutCallFlags[j],
                                                          actualDaysToExpiry, forward,
                                                          deltaVols[j], deltaArray[j],
                                                          numeraireDf, assetDf, out icode);
              }
              /* convert input from asset put to asset call delta */
              /* note if call_put[i]==0 i.e. ATMF then we are assuming that */
              /* the initial guess, vol[i], was already the ATMF vol */
              deltaOrStrike = convert_to_rhs_call_delta(deltaType, callOrPut, actualDaysToExpiry, forward,
                                                        volatility, deltaOrStrike, numeraireDf, assetDf, 
                                                        out icode);

              /* then cubic interp the vol */
              volatility = NabCubicInterpDD(m + 1, deltaOrStrike, 1, deltaArray, deltaVols);

              break;
            case LHS_FENICS_DELTA:
            case LHS_FENICS_FWD_DELTA:
              /* first convert each element of delta array into */
              /* an asset call RHS_FENICS_DELTA */
              for (int j = 0; j <= m; j++)
              {
                deltaArray[j] = convert_to_rhs_call_delta(deltaType, deltaPutCallFlags[j], 
                                                          actualDaysToExpiry, forward,
                                                          deltaVols[j], deltaArray[j], 
                                                          numeraireDf, assetDf, out icode);
              }

              /* now interpolate for each input[i] */
              iters = 0;
              old_jump = new_jump = 0.0;
              old_vol = volatility;

              do
              {
                old_jump = new_jump;
                old_vol = volatility;
                iters++;

                rhs_delta = convert_to_rhs_call_delta(deltaType, callOrPut, actualDaysToExpiry,
                                                      forward, volatility, deltaOrStrike, numeraireDf,
                                                      assetDf, out icode);

                /* then cubic interp the vol */
                volatility = NabCubicInterpDD(m + 1, rhs_delta, 1, deltaArray, deltaVols);

                new_jump = volatility - old_vol;

                if ((new_jump < 0.0 && old_jump > 0.0) || (new_jump > 0.0 && old_jump < 0.0))
                {
                  /* we often overshoot, so half the jump to improve convergence */
                  omega = 0.75;
                }
                else if ((new_jump > 0.0 && old_jump > 0.0) || (new_jump < 0.0 && old_jump < 0.0))
                {
                  omega = 1.5;
                }
                else
                {
                  omega = 1.0;
                }

                volatility = Math.Max((volatility - old_vol) * omega + old_vol, footscray_EPS);

              }
              while (Math.Abs(volatility - old_vol) > footscray_EPS && iters < footscray_IMAX);
              break;
          }

          break;

        case 0: /* strike */

          /* first convert each element of delta array into */
          /* an asset call RHS_FENICS_DELTA */
          for (int j = 0; j <= m; j++)
          {
            deltaArray[j] = convert_to_rhs_call_delta(deltaType, deltaPutCallFlags[j], actualDaysToExpiry,
                            forward, deltaVols[j], deltaArray[j], numeraireDf, assetDf, out icode);
          }

          iters = 0;
          old_jump = new_jump = 0.0;
          old_vol = volatility;
          do
          {
            old_jump = new_jump;
            old_vol = volatility;
            iters++;

            /* find rhs asset call delta */
            ksi = volatility * Math.Sqrt(actualDaysToExpiry / 365.0);
            d1 = (Math.Log(forward / deltaOrStrike) + 0.5 * ksi * ksi) / ksi;
            rhs_delta = NabLibUtilities.CND(d1);

            /* then cubic interp the vol */
            volatility = NabCubicInterpDD(m + 1, rhs_delta, 1, deltaArray, deltaVols);

            new_jump = volatility - old_vol;

            if ((new_jump < 0.0 && old_jump > 0.0) || (new_jump > 0.0 && old_jump < 0.0))
            {
              /* we often overshoot, so half the jump to improve convergence */
              omega = 0.75;
            }
            else if ((new_jump > 0.0 && old_jump > 0.0) || (new_jump < 0.0 && old_jump < 0.0))
            {
              omega = 1.5;
            }
            else
            {
              omega = 1.0;
            }

            volatility = Math.Max((volatility - old_vol) * omega + old_vol, footscray_EPS);

          }
          while (Math.Abs(volatility - old_vol) > footscray_EPS && iters < footscray_IMAX);

          break;
      }

      return volatility;
    }

    public static double convert_to_rhs_call_delta(int delta_type,
                                                   int call_put,
                                                   long days,
                                                   double forward,
                                                   double vol,
                                                   double delta,
                                                   double num_df,
                                                   double ass_df,
                                                   out int icode)
    /* function converts any inputs to rhs asset call fwd delta */
    {

      icode = 0;

      double strike, ksi, d1, ret_val = 0;

      if (call_put == 0) /* zero delta straddle strike */
      {
        switch (delta_type)
        {
          case LHS_FENICS_DELTA:
          case LHS_FENICS_FWD_DELTA:
            ksi = vol * Math.Sqrt(days / 365.0);
            strike = forward * Math.Exp(-0.5 * ksi * ksi);
            d1 = (Math.Log(forward / strike) + 0.5 * ksi * ksi) / ksi;
            ret_val = NabLibUtilities.CND(d1);
            break;

          case RHS_FENICS_DELTA:
          case RHS_FENICS_FWD_DELTA:
            ret_val = 0.5;
            break;
        }
      }
      else
      {
        switch (delta_type)
        {
          case LHS_FENICS_DELTA:

            strike = find_strike_given_delta(call_put, 0, days, forward, 0.0, 0.0, vol, delta / ass_df,
              delta_type, footscray_EPS, 100, out icode);

            ksi = vol * Math.Sqrt(days / 365.0);
            d1 = (Math.Log(forward / strike) + 0.5 * ksi * ksi) / ksi;
            ret_val = NabLibUtilities.CND(d1);

            break;

          case LHS_FENICS_FWD_DELTA:

            strike = find_strike_given_delta(call_put, 0, days, forward, 0.0, 0.0, vol, delta,
              delta_type, footscray_EPS, 100, out icode);

            ksi = vol * Math.Sqrt(days / 365.0);
            d1 = (Math.Log(forward / strike) + 0.5 * ksi * ksi) / ksi;
            ret_val = NabLibUtilities.CND(d1);

            break;

          case RHS_FENICS_DELTA:

            if (call_put == deltaPutFlag)
            {
              ret_val = 1.0 - delta / ass_df;
            }
            else
            {
              ret_val = delta / ass_df;
            }

            break;

          case RHS_FENICS_FWD_DELTA:

            if (call_put == deltaPutFlag)
            {
              ret_val = 1.0 - delta;
            }
            else
            {
              ret_val = delta;
            }

            break;
        }
      }
      return ret_val;
    }

    /* Solve for strike given vol and delta */
    public static double find_strike_given_delta(
                        int call_or_put,
                        long t,
                        long end_t,
                        double spot,
                        double r_num,
                        double r_ass,
                        double vol,
                        double delta,
                        int delta_type,
                        double eps,
                        int imax,
                        out int ierr)
    {

      ierr = 0;
      double strike = 0;
      double spot_min, spot_max;

      double x_delta = delta;
      double x_type = call_or_put;
      double x_tenor = (end_t - t) / DAYS_IN_YEAR;
      double x_spot = spot;
      double x_rnum = r_num;
      double x_rass = r_ass;
      double x_vol = vol;

      if (x_tenor > 0.0)
      {
        switch (delta_type)
        {
          case RHS_FENICS_DELTA:
            spot_min = spot * Math.Exp(-4 * vol * Math.Sqrt(x_tenor));
            spot_max = spot * Math.Exp(4 * vol * Math.Sqrt(x_tenor));

            roots_bis(fenics_rhs_delta_eqn, spot_min, spot_max, eps, imax, out strike, out ierr,
                      x_delta, x_type, x_tenor, x_spot, x_rnum, x_rass, x_vol);

            break;
          case LHS_FENICS_DELTA:
            /* first find spot_min and spot_max allowing for the fact that there's */
            /* a stationary maximum point for LHS delta */
            if (call_or_put == OPT_CALL)
            {
              spot_min = spot * Math.Exp(-4 * vol * Math.Sqrt(x_tenor));
              spot_max = spot * Math.Exp(4 * vol * Math.Sqrt(x_tenor));
              roots_bis(deriv_lhs_delta_wrt_K, spot_min, spot_max, eps, imax, out spot_min, out ierr,
                      x_delta, x_type, x_tenor, x_spot, x_rnum, x_rass, x_vol);
            }
            else
            {
              spot_min = spot * Math.Exp(-4 * vol * Math.Sqrt(x_tenor));
              spot_max = spot * Math.Exp(4 * vol * Math.Sqrt(x_tenor));
              roots_bis(deriv_lhs_delta_wrt_K, spot_min, spot_max, eps, imax, out spot_max, out ierr,
                      x_delta, x_type, x_tenor, x_spot, x_rnum, x_rass, x_vol);
              // Since in the call above spot_max has been passed in as both an input and an output 
              // parameter. And since C# requires one to initialise the output parameter when it 
              // enters the routine, this parameter would have been set to zero. In the case that roots_bis
              // exits because (f1 * f2 > 0) then spot_max must be restored to what it was.
              if (ierr == -1)
                spot_max = spot * Math.Exp(4 * vol * Math.Sqrt(x_tenor));
            }
            roots_bis(fenics_lhs_delta_eqn, spot_min, spot_max, eps, imax, out strike, out ierr,
                      x_delta, x_type, x_tenor, x_spot, x_rnum, x_rass, x_vol);
            break;
          case RHS_FENICS_FWD_DELTA:

            spot_min = spot * Math.Exp(-4 * vol * Math.Sqrt(x_tenor));
            spot_max = spot * Math.Exp(4 * vol * Math.Sqrt(x_tenor));
            roots_bis(fenics_rhs_fwd_delta_eqn, spot_min, spot_max, eps, imax, out strike, out ierr,
                      x_delta, x_type, x_tenor, x_spot, x_rnum, x_rass, x_vol);
            break;
          case LHS_FENICS_FWD_DELTA:
            /* first find spot_min and spot_max allowing for the fact that there's */
            /* a stationary maximum point for LHS delta */
            if (call_or_put == OPT_CALL)
            {
              spot_min = spot * Math.Exp(-4 * vol * Math.Sqrt(x_tenor));
              spot_max = spot * Math.Exp(4 * vol * Math.Sqrt(x_tenor));
              roots_bis(deriv_lhs_delta_wrt_K, spot_min, spot_max, eps, imax, out spot_min, out ierr,
                      x_delta, x_type, x_tenor, x_spot, x_rnum, x_rass, x_vol);
            }
            else
            {
              spot_min = spot * Math.Exp(-4 * vol * Math.Sqrt(x_tenor));
              spot_max = spot * Math.Exp(4 * vol * Math.Sqrt(x_tenor));
              roots_bis(deriv_lhs_delta_wrt_K, spot_min, spot_max, eps, imax, out spot_max, out ierr,
                      x_delta, x_type, x_tenor, x_spot, x_rnum, x_rass, x_vol);
              // Since in the call above spot_max has been passed in as both an input and an output 
              // parameter. And since C# requires one to initialise the output parameter when it 
              // enters the routine, this parameter would have been set to zero. In the case that roots_bis
              // exits because (f1 * f2 > 0) then spot_max must be restored to what it was.
              if (ierr == -1)
                spot_max = spot * Math.Exp(4 * vol * Math.Sqrt(x_tenor));

            }
            roots_bis(fenics_lhs_fwd_delta_eqn, spot_min, spot_max, eps, imax, out strike, out ierr,
                      x_delta, x_type, x_tenor, x_spot, x_rnum, x_rass, x_vol);
            break;
        }
        return strike;
      }
      else
        return spot;
    }

    public static double NormalDensity(double x)
    {
      return 1.0 / Math.Sqrt(2 * Math.PI) * Math.Exp(-(x * x) / 2.0);
    }

    // fenics_rhs_delta_eqn(x, x_rass, x_tenor, x_type, x_spot, x_rnum, x_vol, x_delta)
    public static double fenics_rhs_delta_eqn(double x, double rass, double tenor, double type, 
                                              double spot, double rnum, double vol, double delta) 
    {
        return Math.Exp(-rass * tenor) * NabLibUtilities.CND(type * ((Math.Log(spot / x) + 
            (rnum - rass + 0.5 * vol * vol) * tenor) / (vol * Math.Sqrt(tenor)))) -  delta; 
    }

    public static double deriv_lhs_delta_wrt_K(double strike, double rass, double tenor, double type,
                                               double spot, double rnum, double vol, double delta)
    {
      double fwd, volsqrt, d2;

      fwd = spot * Math.Exp((rnum - rass) * tenor);
      volsqrt = vol * Math.Sqrt(tenor);
      d2 = (Math.Log(fwd / strike) - 0.5 * volsqrt * volsqrt) / volsqrt;
      /* have missed out the pre-scaler (numeraire discount factor / spot) */
      return type * NabLibUtilities.CND(type * d2) - NormalDensity(d2) / volsqrt;
    }

    public static double fenics_lhs_delta_eqn(double x, double rass, double tenor, double type,
                                              double spot, double rnum, double vol, double delta)
    {
      return Math.Exp(-rnum * tenor) * (x / spot) *
             NabLibUtilities.CND(type * ((Math.Log(spot / x) + (rnum - rass - 0.5 * vol * vol) * tenor) 
             / (vol * Math.Sqrt(tenor)))) - delta;
    }

    public static double fenics_rhs_fwd_delta_eqn(double x, double rass, double tenor, double type,
                                                  double spot, double rnum, double vol, double delta)
    {
        return NabLibUtilities.CND(type * ((Math.Log(spot / x) + (rnum - rass + 0.5 * vol * vol) * tenor) /
             (vol * Math.Sqrt(tenor)))) - delta;
    }

    public static double fenics_lhs_fwd_delta_eqn(double x, double rass, double tenor, double type,
                                                  double spot, double rnum, double vol, double delta)
    {
      return Math.Exp((rass - rnum) * tenor) * (x / spot) *
            NabLibUtilities.CND(type * ((Math.Log(spot / x) + (rnum - rass - 0.5 * vol * vol) * tenor) /
            (vol * Math.Sqrt(tenor)))) - delta;
    }

    public static void roots_bis(
     SimpleFN f,
     double xstart,
     double xend,
     double eps,
     int imax,
     out double soln,
     out int icode,
     double x_delta, 
     double x_type, 
     double x_tenor, 
     double x_spot, 
     double x_rnum, 
     double x_rass, 
     double x_vol)
    {
      double f1, f2, x1, x2;
      double fmid, xmid;
      int niters = 0;
      soln = 0;

      x1 = xstart;
      x2 = xend;

      f1 = f(x1, x_rass, x_tenor, x_type, x_spot, x_rnum, x_vol, x_delta);
      f2 = f(x2, x_rass, x_tenor, x_type, x_spot, x_rnum, x_vol, x_delta);

      if (f1 == 0.0)
      {
        icode = 0;
        soln = x1;
      }
      else if (f2 == 0.0)
      {
        icode = 0;
        soln = x2;
      }
      else if (f1 * f2 > 0)
        icode = -1;
      else
      {
        /* tjo - correct for case x1 = 0 */
        while ((niters < imax) && (Math.Abs(x2 - x1) >= Math.Abs(eps * (xstart - xend))))
        {
          xmid = (x2 + x1) / 2;
          fmid = f(xmid, x_rass, x_tenor, x_type, x_spot, x_rnum, x_vol, x_delta);

          if (f1 * fmid < 0)
            x2 = xmid;
          else
            x1 = xmid;

          niters++;
        }
        if ((Math.Abs(x2 - x1) < Math.Abs(eps * (xstart - xend))))
        {
          icode = 0;
          soln = (x1 + x2) / 2;
        }
        else
        { /* exceeded imax */
          icode = -2;
          soln = (x1 + x2) / 2;
        }
      }
    }

    public static double NabCubicInterpDD(
                            long numOfElts,			/* (I) number of elements in arrays */
                            double xval,				/* (I) x value to search for */
                            int extrapMethod,		/* (I) extrapolation method */
                            double[] x,					/* (I) array of double x values */
                            double[] y)					/* (I) array of double y values */
    /*-----------------------------------------------------------------------------
    ** FUNCTION:	NabCubicInterpDD.
    **
    ** AUTHOR:		Steven Marshall, January 5th, 1994.
    **
    ** DESCRIPTION:	Line cubic interpolation with double x values and double y values.
    **				Adapted from NabTools original by Olivier Deloire.  The first 
    **				and last periods, and any extrapolation fits to a quadratic.
    **
    ** RETURNS:		The interpolated y value.  In the case of errors, the error
    **				is logged and 0.0 is returned.
    **---------------------------------------------------------------------------*/
    {
      double yval;			/* return y value */
      double a = 0, b = 0, c = 0, d = 0;		/* polynomial coefficients */
      int i;				/* loop search index variable */
      int rc;				/* return code */

      if (numOfElts == 2)
      {
        /* use linear interpolation if only two points */
        yval = (double)linear_interp((double)xval,
                                     (double)x[0],
                                     (double)x[1],
                                     (double)y[0],
                                     (double)y[1]);
      }
      else
      {
        /* use a linear search at the moment */
        for (i = 0; i < numOfElts; i++)
          if (x[i] >= xval)
            break;

        if (i <= 1)		/* before second point */
        {
          if ((i == 0) && (extrapMethod == NAB_FLAT || extrapMethod == NAB_S_FLAT_E_EXTRAP))
            yval = y[0];	/* flat at start */
          else
          {
            /* solve quadratic for first three points */
            rc = NabSolveQuadratic(x, y, FIT_FIRST_TWO, out a, out b, out c);
            /* Note that we can ignore the return code as we know that all
            ** our x values will be distinct from calling routine.
            */
            yval = c + xval * (b + a * xval);
          }
        }
        else if (i >= numOfElts - 1)		/* after second to last point */
        {
          if ((i == numOfElts) && (extrapMethod == NAB_FLAT || extrapMethod == NAB_S_EXTRAP_E_FLAT))
            yval = y[i - 1];	/* flat at end */
          else
          {
            /* solve quadratic for last three points */
            double[] lessX = new double[3];
            double[] lessY = new double[3];
            lessX[0] = x[numOfElts - 3]; lessX[1] = x[numOfElts - 2]; lessX[2] = x[numOfElts - 1];
            lessY[0] = y[numOfElts - 3]; lessY[1] = y[numOfElts - 2]; lessY[2] = y[numOfElts - 1];
            rc = NabSolveQuadratic(lessX, lessY, FIT_SECOND_TWO, out a, out b, out c);
            yval = c + xval * (b + a * xval);
          }
        }
        else		/* covered by array */
        {
          /* solve cubic for four points */
          double[] lessX = new double[4];
          double[] lessY = new double[4];
          Array.Copy(x, i - 2, lessX, 0, 4);
          Array.Copy(y, i - 2, lessY, 0, 4);
          rc = NabSolveCubic(lessX, lessY, out a, out b, out c, out d);
          yval = d + xval * (c + xval * (b + a * xval));
        }
      }

      return yval;
    }

    public static int NabSolveCubic(
	                          double[]		x,		/* (I) x values to solve for */
	                          double[]		y,		/* (I) f(x) values to solve for */
	                          out double  a,			/* (O) cubic coefficient */
	                          out double	b,			/* (O) quadratic coefficient */
	                          out double	c,			/* (O) linear coefficient */
	                          out double	d)			/* (O) constant coefficient */
    /*-----------------------------------------------------------------------------
    ** FUNCTION:	NabSolveCubic.
    **
    ** AUTHOR:		Steven Marshall, January 5th, 1994.
    **
    ** DESCRIPTION:	Solves for a, b, c and d in the equation f(x)=a*x^3+b*x^2+c*x+d 
    **				given four pairs of x and y=f(x).  Solves so that the curve passes
    **				through the second and third points, and keeps the derivatives at
    **				these points constant.  Solution provided by Mathematica.
    **
    ** RETURNS:		SUCCESS and the values a, b, c and d.  If there was a problem
    **				then FAILURE and the error is logged.
    **---------------------------------------------------------------------------*/
    {                                                              
	    double	P1, P2;                           
	    double	tx, ty;
	    double	x1, x2, y1, y2;
	    double	x1s, x1c;
	    double	x2s, x2c;
	    double	txc;

      a = 0;
      b = 0;
      c = 0;
      d = 0;

	    x1 = x[1];           
	    x2 = x[2];
	    y1 = y[1];
	    y2 = y[2];
	    tx = x2-x1;
	    ty = y2-y1;

	    /* check so we don't get a divide by zero */
      if ((Math.Abs(x[0] - x1) < DBL_THRESHHOLD) || (Math.Abs(tx - 0.0) < DBL_THRESHHOLD)
          || (Math.Abs(x[3] - x2) < DBL_THRESHHOLD))

	    {
    /*		DrError("Could not solve for cubic as two x values are the same.\n");
		    DrError("Four supplied values are;\n%f\n%f\n%f\n%f\n", x[0], x1, x2, x[3]);*/
		    return FAILURE;
	    }
    			
        P1 = ( (y1-y[0])/(x1-x[0]) + ty/tx )/2.0;	/* derivative at point 1 */
        P2 = ( ty/tx + (y[3]-y2)/(x[3]-x2) )/2.0;	/* derivative at point 2 */                 
        x1s = x1 * x1;								/* x[1] squared */
        x1c = x1s * x1;								/* x[1] cubed */
        x2s = x2 * x2;								/* x[2] squared */
        x2c = x2s * x2;								/* x[2] cubed */                                      
        txc = tx * tx * tx;							/* (x[2]-x[1]) cubed */
        
        d = (P2*x1c*x2 + (P1-P2)*x1s*x2s - P1*x1*x2c - 3.0*x1*x2s*y1 + x2c*y1 - x1c*y2 + 3.0*x1s*x2*y2)/txc;
    	
	      c = (-P2*x1c - 2.0*P1*x1s*x2 - P2*x1s*x2 + P1*x1*x2s + 2.0*P2*x1*x2s + P1*x2c + 6.0*x1*x2*(y1-y2))/txc; 

        b = (P1*x1s + 2.0*P2*x1s + (P1-P2)*x1*x2 - 2.0*P1*x2s - P2*x2s + 3.0*(x2*y2+x1*y2-x2*y1-x1*y1))/txc;
        
        a = -((x1-x2)*(P1+P2) + 2.0*(y2-y1))/txc;

        return SUCCESS;
    }   

    public static int NabSolveQuadratic(
	                        double[]  x,		/* (I) x values to solve for */
	                        double[]	y,		/* (I) f(x) values to solve for */
	                        int	fitFlag,	  /* (I) specifies which two points we fit too */
	                        out double a,		/* (O) quadratic coefficient */
	                        out double b,		/* (O) linear coefficient */
	                        out double c)		/* (O) constant coefficient */
  /*-----------------------------------------------------------------------------
  ** FUNCTION:	NabSolveQuadratic.
  **
  ** AUTHOR:		Steven Marshall, January 5th, 1994.
  **
  ** DESCRIPTION:	Solves for a, b and c in the equation f(x)=a*x^2+b*x+c given
  **				three pairs of x and y=f(x) for values near to x[1], keeping
  **				a constant derivative at x[1].
  **				The user may specify whether to fit the first two points or
  **				the second two points exactly.
  **
  ** RETURNS:		SUCCESS and the values a, b and c.  If there was a problem then
  **				FAILURE and the error is logged.
  **
  ** AMENDMENTS:
  ** 6 June 1995	Fixed a bug which caused an error in cubic interpolation for
  **				the last period, as we were fitting the first two points, not
  **				the last two.  SM & OD. Fixed for release 2.10.
  **---------------------------------------------------------------------------*/
  {                                                              
	  double	deriv; 
	  double	x1, y1;				/* x[1] and y[1] */
	  double	tx, ty, tc;			/* temporary variables for performance */
	  double	fx, fy;				/* variables to fit curve to */

    a = 0;
    b = 0;
    c = 0;
	  x1 = x[1];	y1 = y[1];

	  if (fitFlag == FIT_FIRST_TWO)
	  {
		  fx = x[0];
		  fy = y[0];
	  }		   
	  else
	  {
		  fx = x[2];
		  fy = y[2];
	  }

	  /* check so we don't get a divide by zero */
    if ((Math.Abs(x1 - x[0]) < DBL_THRESHHOLD) || (Math.Abs(x[2] - x1) < DBL_THRESHHOLD))
	  {
  /*		DrError("Could not solve for quadratic as two x values are the same.\n");
		  DrError("Three supplied values are;\n%f\n%f\n%f\n", x[0], x1, x[2]);*/
		  return FAILURE;
	  }

	  /* Calculate the derivative for the centre point */
	  deriv = ((y1-y[0])/(x1-x[0]) + (y[2]-y1)/(x[2]-x1)) * 0.5;
	  tx = x1-fx;
	  ty = y1-fy;

	  tc = (deriv - ty/tx) / tx;
	  a = tc;
	  b = (deriv - 2.0*tc*x1 + ty/tx - tc*(x1+fx)) * 0.5;
	  c = (fy - fx*((b)+tc*fx) + y1 - x1*((b)+tc*x1)) * 0.5;
      
      return SUCCESS;
  }
    public static double linear_interp(double x,
                                       double x1,
                                       double x2,
                                       double f1,
                                       double f2)
    {
      if (x <= x1)
      {
        return f1;
      }
      else if (x >= x2)
      {
        return f2;
      }
      else
      {
        return f1 + (f2 - f1) * (x - x1) / (x2 - x1);
      }
    }
  }
}
