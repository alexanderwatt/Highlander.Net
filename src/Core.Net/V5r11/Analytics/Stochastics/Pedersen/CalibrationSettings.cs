/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Highlander.Reporting.Analytics.V5r3.Stochastics.Pedersen
{
    /// <summary>
    /// Contains calibration related parameters.
    /// </summary>
    public class CalibrationSettings
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public CalibrationSettings()
        {
            CascadeParam = new CascadeParameters();
            HSmoothWeight = 0.001;
            VSmoothWeight = 0.001;
            SwaptionWeight = 1;
            CapletWeight = 1;
            ExponentialForm = true;
            ThreadCount = 2;
            MaxIteration = 100;
        }

        /// <summary>
        /// Custom constructor
        /// </summary>
        /// <param name="hWeight">horizontal weight</param>
        /// <param name="vWeight">vertical weight</param>
        /// <param name="sWeight">swaption weight</param>
        /// <param name="cWeight">caplet weight</param>
        /// <param name="expForm">whether to use exponential form for input vector</param>
        /// <param name="threads">number of threads used</param>
        /// <param name="maxIterations">maximum number of iterations</param>
        /// <param name="cascadeParam">cascade specifications</param>
        public CalibrationSettings(double hWeight, double vWeight, double sWeight, double cWeight, bool expForm, int threads, int maxIterations, CascadeParameters cascadeParam)
        {
            CascadeParam = cascadeParam;
            HSmoothWeight = hWeight;
            VSmoothWeight = vWeight;
            SwaptionWeight = sWeight;
            CapletWeight = cWeight;
            ExponentialForm = expForm;
            ThreadCount = threads;
            MaxIteration = maxIterations;
        }

        #endregion

        #region Private Fields

        ///<summary>
        ///</summary>
        public CascadeParameters CascadeParam { get; }

        ///<summary>
        ///</summary>
        public double HSmoothWeight { get; }

        ///<summary>
        ///</summary>
        public double VSmoothWeight { get; }

        ///<summary>
        ///</summary>
        public double SwaptionWeight { get; }

        ///<summary>
        ///</summary>
        public double CapletWeight { get; }

        ///<summary>
        ///</summary>
        public bool ExponentialForm { get; }

        ///<summary>
        ///</summary>
        public int ThreadCount { get; }

        ///<summary>
        ///</summary>
        public int MaxIteration { get; }

        #endregion
    }
}