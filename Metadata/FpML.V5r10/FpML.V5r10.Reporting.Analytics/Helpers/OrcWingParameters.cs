using System;

namespace Orion.Analytics.Helpers
{
    [Serializable]
    public struct OrcWingParameters
    {
        #region Parameters

        /// <summary>
        /// The atm forward.
        /// </summary>
        public double AtmForward { get; set; }

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
        public double Vcr { get; set; }

        /// <summary>
        /// The scr.
        /// </summary>
        public double Scr { get; set; }

        /// <summary>
        /// The ssr.
        /// </summary>
        public double Ssr { get; set; }

        /// <summary>
        /// The dsr.
        /// </summary>
        public double Dsr { get; set; }

        /// <summary>
        /// The usr.
        /// </summary>
        public double Usr { get; set; }

        /// <summary>
        /// The time to maturity.
        /// </summary>
        public double TimeToMaturity { get; set; }

        ///// <summary>
        ///// The dsc.
        ///// </summary>
        //public double Dsc { get; set; }

        ///// <summary>
        ///// Te usc.
        ///// </summary>
        //public double Usc { get; set; }

        #endregion
    }
}