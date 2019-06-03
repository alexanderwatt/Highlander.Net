using System;

namespace Orion.Analytics.Stochastics.Volatilities
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
        public double CurrentVol { get; set; }

        /// <summary>
        /// Te slope.
        /// </summary>
        public double SlopeRef { get; set; }

        /// <summary>
        /// The put curve.
        /// </summary>
        public double PutCurve { get; set; }

        /// <summary>
        /// The call curve.
        /// </summary>
        public double CallCurve { get; set; }

        /// <summary>
        /// Lower cut off.
        /// </summary>
        public double DnCutoff { get; set; }

        /// <summary>
        /// The upper cut off.
        /// </summary>
        public double UpCutoff { get; set; }

        /// <summary>
        /// The reference forward.
        /// </summary>
        public double RefFwd { get; set; }

        /// <summary>
        /// The reference vol.
        /// </summary>
        public double RefVol { get; set; }

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
        public double TimeToMaturity { get; set; }

        /// <summary>
        /// The dsc.
        /// </summary>
        public double Dsc { get; set; }

        /// <summary>
        /// Te usc.
        /// </summary>
        public double Usc { get; set; }

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

            var F = Math.Pow(atFwd, ssr / 100.0) * Math.Pow(RefFwd, 1.0 - ssr / 100.0);

            var aux = Math.Log(k / F);

            Usc = F * Math.Exp(UpCutoff);
            Dsc = F * Math.Exp(DnCutoff);
          
            var sc = (SlopeRef - scr*ssr*(atFwd-RefFwd)/RefFwd);

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

            if (aux <= (1.0 + dsr) * DnCutoff)
            {
                vol = CurrentVol + DnCutoff*(2.0+dsr)*(sc/2.0) + (1.0 + dsr)*PutCurve*DnCutoff*DnCutoff;
                          
            }
            else if (((1.0+dsr)*DnCutoff < aux) & (aux <= DnCutoff))
            {
                vol = (-PutCurve/dsr - sc/(2.0*DnCutoff*dsr))*aux*aux
                      + (1.0+dsr)/dsr*(2*PutCurve*DnCutoff+sc)*aux
                      - PutCurve*DnCutoff*DnCutoff*(1.0+1.0/dsr)
                      - sc*DnCutoff/(2.0*dsr)
                      + CurrentVol;
            }
            else if ( (UpCutoff < aux) & (aux <= (1.0+ usr)*UpCutoff) )
            {
                vol = (-CallCurve / usr - sc / (2.0 * UpCutoff * usr)) * aux * aux
                      + (1.0 + usr) / usr * (2.0 * CallCurve * UpCutoff + sc) * aux
                      - CallCurve * UpCutoff * UpCutoff * (1.0 + 1.0 / usr)
                      - sc * UpCutoff / (2.0 *usr)
                      + CurrentVol;
            }
            else if (aux > (1.0 + usr) * UpCutoff)
            {
                vol = CurrentVol 
                      + UpCutoff * (2.0 + usr) * (sc / 2.0) 
                      + (1.0 + usr) * CallCurve * UpCutoff * UpCutoff;
            }
            else if ((scr == 0.0) && (vcr == 0.0))
            {
                vol = CurrentVol * f1 + sc * f2 + PutCurve * f3 + CallCurve * f4;
            }
            return vol;

        }


        #endregion
    }
}