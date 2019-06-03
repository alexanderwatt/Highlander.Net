using System;

namespace National.QRSC.Analytics.Options
{
    /// <summary>
    /// The ORC wing vol model.
    /// </summary>
      public class OrcWingVol
      {
        #region Private Members

          #endregion

        #region Accessors

          /// <summary>
          /// The current vol.
          /// </summary>
          public double currentVol { get; set; }

          /// <summary>
          /// Te slope.
          /// </summary>
          public double slopeRef { get; set; }

          /// <summary>
          /// The put curve.
          /// </summary>
          public double putCurve { get; set; }

          /// <summary>
          /// The call curve.
          /// </summary>
          public double callCurve { get; set; }

          /// <summary>
          /// Lower cut off.
          /// </summary>
          public double dnCutoff { get; set; }

          /// <summary>
          /// The upper cut off.
          /// </summary>
          public double upCutoff { get; set; }

          /// <summary>
          /// The reference forward.
          /// </summary>
          public double refFwd { get; set; }

          /// <summary>
          /// The reference vol.
          /// </summary>
          public double refVol { get; set; }

          /// <summary>
          /// The vcr.
          /// </summary>
          public double vcr { get; set; }

          /// <summary>
          /// The scr.
          /// </summary>
          public double scr { get; set; }

          /// <summary>
          /// The ssr.
          /// </summary>
          public double ssr { get; set; }

          /// <summary>
          /// The dsr.
          /// </summary>
          public double dsr { get; set; }

          /// <summary>
          /// The usr.
          /// </summary>
          public double usr { get; set; }

          /// <summary>
          /// The time to maturity.
          /// </summary>
          public double timeToMaturity { get; set; }

          /// <summary>
          /// The dsc.
          /// </summary>
          public double _dsc { get; set; }

          /// <summary>
          /// Te usc.
          /// </summary>
          public double _usc { get; set; }

          #endregion

        #region Public Methods

          /// <summary>
          /// The ORC vol.
          /// </summary>
          /// <param name="atFwd"></param>
          /// <param name="k"></param>
          /// <returns></returns>
        public double Orcvol(double atFwd, double k)
        {
          var vol = 0.0;

          var F = Math.Pow(atFwd, ssr / 100.0) * Math.Pow(refFwd, 1.0 - ssr / 100.0);

          var aux = Math.Log(k / F);

          _usc = F * Math.Exp(upCutoff);
          _dsc = F * Math.Exp(dnCutoff);
          
          var sc = (slopeRef - scr*ssr*(atFwd-refFwd)/refFwd);

          ///f1;     
          var f1 = 1.0;

          ///f2;
          var f2 = aux;

          ///f3;
          var f3=0.0;
          if (aux <=0.0)      
            f3 = aux*aux;
          
          ///f4;
          var f4 = 0.0;
          if (aux>0.0)
             f4 = aux * aux;

          ///Volatility surface calculations;      

          if (aux <= (1.0 + dsr) * dnCutoff)
          {
            vol = currentVol + dnCutoff*(2.0+dsr)*(sc/2.0) + (1.0 + dsr)*putCurve*dnCutoff*dnCutoff;
                          
          }
          else if (((1.0+dsr)*dnCutoff < aux) & (aux <= dnCutoff))
          {
            vol = (-putCurve/dsr - sc/(2.0*dnCutoff*dsr))*aux*aux
                          + (1.0+dsr)/dsr*(2*putCurve*dnCutoff+sc)*aux
                          - putCurve*dnCutoff*dnCutoff*(1.0+1.0/dsr)
                          - sc*dnCutoff/(2.0*dsr)
                          + currentVol;
          }
          else if ( (upCutoff < aux) & (aux <= (1.0+ usr)*upCutoff) )
          {
            vol = (-callCurve / usr - sc / (2.0 * upCutoff * usr)) * aux * aux
                  + (1.0 + usr) / usr * (2.0 * callCurve * upCutoff + sc) * aux
                  - callCurve * upCutoff * upCutoff * (1.0 + 1.0 / usr)
                  - sc * upCutoff / (2.0 *usr)
                  + currentVol;
          }
          else if (aux > (1.0 + usr) * upCutoff)
          {
            vol = currentVol 
                  + upCutoff * (2.0 + usr) * (sc / 2.0) 
                  + (1.0 + usr) * callCurve * upCutoff * upCutoff;
          }
          else if ((scr == 0.0) && (vcr == 0.0))
          {
            vol = currentVol * f1 + sc * f2 + putCurve * f3 + callCurve * f4;
          }
          return vol;

        }


    #endregion
  }
}
