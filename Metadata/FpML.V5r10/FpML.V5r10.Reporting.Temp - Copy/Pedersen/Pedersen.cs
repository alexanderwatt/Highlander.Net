/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using Highlander.Numerics.Pedersen;
using MathNet.Numerics.LinearAlgebra.Double;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Pedersen
{
    public class Pedersen
    {
        #region Declarations

        private readonly Economy _usd;
        private readonly Parameters _parameters;
        private readonly Recycle _recycle;
        private readonly ObjectiveFunction _objective;
        private readonly Calibrator _pedersen;
        private readonly Cascade _cascade;
        private readonly Simulator _glasserman;
        private static string _calSummaryString = "";
        private static string _simSummaryString = "";
        public static string Debugger { get; private set; }

        /// <summary>
        /// The current range.
        /// </summary>
        public static object[,] CurrentRange = new object[1,1];

        #endregion

        #region Helpers

        public static void Debug(string s)
        {
            Debugger = s;
        }

        public object DebugOutput()
        {
            var res = new object[3, 1];
            res[0, 0] = _parameters.NumberOfCaplets;
            res[1, 0] = _parameters.CapletTenors;
            res[2, 0] = _parameters.NumberOfSwaptions;
            return res;
        }

        public static void Clear(string target)
        {
            if (target == "cal")
            {
                _calSummaryString = "";
            }
            if (target == "sim")
            {
                _simSummaryString = "";
            }
        }

        public static void Write(string s, string target)
        {
            if (target == "cal")
            {
                _calSummaryString = _calSummaryString + s;
            }
            if (target == "sim")
            {
                _simSummaryString = _simSummaryString + s;
            }
        }

        public static void WriteRange(object o)
        {
            CurrentRange[0,0] = o;
        }

        public static void WriteRange(int i, int j, object o)
        {
            if (i > 0 && j > 0)
            {
                CurrentRange[i, j] = o;
            }
        }

        #endregion

        #region Constructor

        public Pedersen()
        {
            _parameters = new Parameters();
            _recycle = new Recycle(_parameters);
            _usd = new Economy(_parameters, _recycle);
            _objective = new ObjectiveFunction(_usd, _recycle, _parameters);
            _pedersen = new Calibrator(_objective, _parameters);
            _cascade = new Cascade(_usd, _parameters);
            _glasserman = new Simulator(_usd, _parameters);
        }

        #endregion

        #region Data

        public object[,] ShowCorrelation()
        {
            var result = new object[1, 1];
            result[0, 0] = "Correlation Unsuccessfull";
            try
            {
                var res = new object[_parameters.NumberOfTenors + 1, _parameters.NumberOfTenors + 1];
                res[0, 0] = "Correlation";
                for (int i = 1; i <= _parameters.NumberOfTenors; i++)
                {
                    res[0, i] = _parameters.Tenor[i];
                    res[i, 0] = _parameters.Tenor[i];
                }
                for (int i = 1; i <= _parameters.NumberOfTenors; i++)
                {
                    for (int j = 1; j <= _parameters.NumberOfTenors; j++)
                    {
                        res[i, j] = _usd.Correlation[i - 1, j - 1];
                    }
                }
                return res;
            }
            catch (Exception e)
            {                
                result[0, 0] = e.ToString();
            }
            return result;
        }

        /// <summary>
        /// Setting discount function from a range in the spreadsheet
        /// Requires actual data.
        /// </summary>
        /// <param name="discountFactorArray"></param>
        /// <returns></returns>
        public string SetDiscountFactors(List<double> discountFactorArray)
        {
            const int length = 123;
            string temp;
            try
            {
                var discount = new double[length];
                int upper = Math.Min(length, discountFactorArray.Count);
                if (upper < length)
                {
                    double ratio = discount[upper - 1] / discount[upper - 2];
                    for (int i = upper; i < length; i++)
                    {
                        discount[i] = ratio * discount[i - 1];
                    }
                }
                _usd.CurDiscount = discount;
                _usd.DiscountStatus = "User Defined";
                temp = "Discount factors were set successfully.";
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        /// <summary>
        /// Set discount from a yield curve ID
        /// </summary>
        /// <param name="rateCurve"></param>
        /// <returns></returns>
        public string SetDiscountFactors(IRateCurve rateCurve)
        {
            const int length = 123;
            string temp;
            try
            {
                var discount = new double[length];
                DateTime today = DateTime.Now;
                for (int i = 0; i < length; i++)
                {
                    discount[i] = rateCurve.GetDiscountFactor(today, today.AddMonths(i * 3));
                }
                _usd.CurDiscount = discount;
                _usd.DiscountStatus = "User Defined";
                temp = String.Format("Discount factors were set.");
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        public string SetCapletImpliedVolatility(List<double> cpltVolArray)
        {
            string temp;
            try
            {
                var iVol = new double[120];
                for (int i = cpltVolArray.Count; i <= cpltVolArray.Count; i++)
                {
                    try
                    {
                        double vol = cpltVolArray[i];
                        if (vol > 2)
                        {
                            iVol[i - 1] = vol / 100;
                        }
                        else
                        {
                            iVol[i - 1] = vol;
                        }
                    }
                    catch
                    {
                        iVol[i - 1] = 0;
                    }
                }
                _usd.RawCapletImpliedVolatility = iVol;
                _usd.CapletImpliedVolatilityStatus = "User Defined";
                temp = string.Format("Caplet Implied Vols were set successfully.");
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        public string SetCapletImpliedVolatility(double strike, IStrikeVolatilitySurface volSurface)
        {
            const int length = 120;
            try
            {
                var temp = volSurface;
                if (temp!= null)
                {
                    var result = new object[length + 1, 1];
                    //DateTime today = DateTime.Now;
                    var iVol = new double[length];
                    for (int i = 1; i <= length; i++)
                    {
                        var term = i*3 + "M";
                        iVol[i - 1] = volSurface.GetValueByExpiryTermAndStrike(term, strike);
                        result[i, 0] = iVol[i - 1];
                    }
                    _usd.RawCapletImpliedVolatility = iVol;
                    _usd.CapletImpliedVolatilityStatus = "User Defined";
                    return string.Format("Caplet Implied Vols were set successfully.");                
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return string.Format("Caplet Implied Vols not set successfully.");
        }

        public string SetSwaptionImpliedVolatility(object[,] objects)
        {
            string temp;
            try
            {
                var iVol = new double[objects.GetUpperBound(0), objects.GetUpperBound(1)];
                for (int i = 1; i <= objects.GetUpperBound(0); i++)
                {
                    for (int j = 1; j <= objects.GetUpperBound(1); j++)
                    {
                        try
                        {
                            double vol = double.Parse(objects[i, j].ToString());
                            if (vol > 2)
                            {
                                iVol[i - 1, j - 1] = vol / 100;
                            }
                            else
                            {
                                iVol[i - 1, j - 1] = vol;
                            }
                        }
                        catch
                        {
                            iVol[i - 1, j - 1] = 0;
                        }
                    }
                }
                _usd.RawSwaptionImpliedVolatility = iVol;
                _usd.SwaptionImpliedVolatilityStatus = "User Defined";
                temp = "Swaption Implied Vols were set successfully.";
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        public object SetCorrelation(object[,] objects)
        {
            object temp;
            try
            {
                int size = Math.Min(objects.GetUpperBound(0), objects.GetUpperBound(1));
                var cor = new DenseMatrix(size);
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        cor[i, j] = double.Parse(objects[i + 1, j + 1].ToString());
                    }
                }
                _usd.CorrelationData = cor;
                temp = "Correlations were set successfully.";
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;

        }

        #endregion

        #region Calibration

        /*
         * Performs the actual calibration.
         */
        public object Calibration(object[,] r1)
        {
            object temp;
            Clear("cal");
            try
            {
                CalSetInput(r1);

                #region Initialise Objects

                //order is important, Param must be initialised first
                _parameters.Initialise();
                _recycle.Initialise();
                _usd.Initialise();
                _objective.Initialise();

                #endregion

                var t = new Thread(CalTStart);
                t.Start();
                temp = "Running...";
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        private void CalTStart()
        {
            Thread.Sleep(200);
            try
            {
                var result = new object[2, 1];
                result[0, 0] = "Running...";
                result[1, 0] =
                    $"(Factors: {_parameters.NumberOfFactors}, Shift: {_usd.ConstShift}, ExpForm: {_objective.ExpForm}, Iterate: {_pedersen.Iteration}, Caplet Bound: {_cascade.CpltBound}, Swaption Bound: {_cascade.SwpnBound}, NSWPN: {_parameters.NumberOfSwaptions}, NCPLT: {_parameters.NumberOfCaplets})";
                WriteRange(result);
                Write("Pedersen Calibration:\n", "cal");
                _pedersen.Go();
                Write("Cascade Algorithm:\n", "cal");
                _cascade.Go();
                _usd.OutputStatus();
                WriteRange(1, 1, "Calibration Successful!");
            }
            catch (Exception e)
            {
                WriteRange(e.ToString());
            }
        }

        private void CalSetInput(object[,] objects)
        {
            CalSetDefault();
            for (int i = objects.GetLowerBound(0); i <= objects.GetUpperBound(0); i++)
            {
                for (int j = objects.GetLowerBound(1); j < objects.GetUpperBound(1); j++)
                {
                    try
                    {
                        switch (objects[i, j].ToString().ToLower())
                        {
                            case "factors":
                                _parameters.NumberOfFactors = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "shift":
                                _usd.ConstShift = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "exp form":
                                _objective.ExpForm = bool.Parse(objects[i, j + 1].ToString());
                                break;
                            case "iterations":
                                _pedersen.Iteration = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "expiry":
                                _parameters.UnderlyingExpiry = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "tenor":
                                _parameters.UnderlyingTenor = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "caplet bound":
                                _cascade.CpltBound = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "swaption bound":
                                _cascade.SwpnBound = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "use caplet":
                                _parameters.CapletOn = bool.Parse(objects[i, j + 1].ToString());
                                break;
                            case "use swaption":
                                _parameters.SwaptionOn = bool.Parse(objects[i, j + 1].ToString());
                                break;
                            case "threads":
                                _objective.NThreads = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "horizontal weight":
                                _objective.HWeight = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "vertical weight":
                                _objective.VWeight = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "caplet weight":
                                _objective.CWeight = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "swaption weight":
                                _objective.SWeight = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "discretisation":
                                string[] st = objects[i, j + 1].ToString().Split(',');
                                _parameters.Timeframe = new int[st.Length];
                                for (int k = 0; k < st.Length; k++)
                                {
                                    _parameters.Timeframe[k] = int.Parse(st[k].Trim());
                                }
                                break;
                        }
                    }
                    catch
                    { }
                }
            }
        }

        private void CalSetDefault()
        {
            _parameters.NumberOfFactors = 3;
            _usd.ConstShift = 0;
            _objective.ExpForm = true;
            _pedersen.Iteration = 50;
            _parameters.UnderlyingExpiry = 60;
            _parameters.UnderlyingTenor = 60;
            _cascade.CpltBound = 1.20;
            _cascade.SwpnBound = 1.10;
            _parameters.CapletOn = true;
            _parameters.SwaptionOn = true;
            _objective.NThreads = 2;
            _objective.HWeight = 0.001;
            _objective.VWeight = 0.001;
            _objective.CWeight = 1;
            _objective.SWeight = 1;
            _parameters.Timeframe = new[] { 0, 1, 2, 3, 4, 6, 8, 10, 12, 16, 20, 24, 28, 34, 40, 50, 60, 80, 100, 120 };
        }

        /// <summary>
        /// Displays post-Calibration result summary.
        /// </summary>
        /// <returns></returns>
        public object CalSummary()
        {
            object temp;
            try
            {
                string[] str = _calSummaryString.Split('\n');
                var result = new object[str.Length, 1];
                for (int i = 0; i < str.Length; i++)
                {
                    result[i, 0] = str[i];
                }
                temp = result;
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        /// <summary>
        /// Displays post-Calibration vol surface (multi-factored)
        /// </summary>
        /// <returns></returns>
        public object CalVol()
        {
            object temp;
            try
            {
                var result = new object[_parameters.NumberOfFactors*(_parameters.UnderlyingExpiry + 2) - 1, _parameters.UnderlyingTenor + 1];
                for (int i = result.GetLowerBound(0); i < result.GetUpperBound(0); i++)
                {
                    for (int j = result.GetLowerBound(1); j < result.GetUpperBound(1); j++)
                    {
                        result[i, j] = "";
                    }
                }
                for (int j = 0; j < _parameters.NumberOfFactors; j++)
                {
                    result[j * (_parameters.UnderlyingExpiry + 2), 0] = $"Vol Fac {j + 1}";
                    for (int i = 0; i < _parameters.UnderlyingExpiry; i++)
                    {
                        result[j * (_parameters.UnderlyingExpiry + 2) + i + 1, 0] = i + 1;
                    }
                    for (int i = 0; i < _parameters.UnderlyingTenor; i++)
                    {
                        result[j * (_parameters.UnderlyingExpiry + 2), i + 1] = i + 1;
                    }
                    for (int i = 0; i < _parameters.UnderlyingExpiry; i++)
                    {
                        for (int k = 0; k < _parameters.UnderlyingTenor; k++)
                        {
                            double vol = _usd.Xi[i][k, j];
                            result[j * (_parameters.UnderlyingExpiry + 2) + i + 1, k + 1] = vol;
                        }
                    }
                }
                temp = result;
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        /// <summary>
        /// Displays post-Calibration vol surface (vol sizes)
        /// </summary>
        /// <returns></returns>
        public object CalVolNorm()
        {
            object temp;
            try
            {
                var result = new object[_parameters.UnderlyingExpiry + 1, _parameters.UnderlyingTenor + 1];
                result[0, 0] = "Volatility";
                for (int i = 0; i < _parameters.UnderlyingExpiry; i++)
                {
                    result[i + 1, 0] = i + 1;
                }
                for (int i = 0; i < _parameters.UnderlyingTenor; i++)
                {
                    result[0, i + 1] = i + 1;
                }
                for (int i = 0; i < _parameters.UnderlyingExpiry; i++)
                {
                    for (int j = 0; j < _parameters.UnderlyingTenor; j++)
                    {
                        double vol = _usd.XiNorm(i, j);
                        result[i + 1, j + 1] = vol;
                    }
                }
                temp = result;
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        #endregion

        #region Simulation

        public object Simulation(object[,] r1)
        {
            object result;
            Clear("sim");
            try
            {
                SimSetInput(r1);
                var t = new Thread(SimTStart);
                t.Start();
                result = "Running...";
            }
            catch (Exception e)
            {
                result = e.ToString();
            }
            return result;
        }

        private void SimTStart()
        {
            Thread.Sleep(200);
            try
            {
                double price = _glasserman.Go();
                WriteRange(price);
            }
            catch (Exception e)
            {
                WriteRange(e.ToString());
            }
        }

        private void SimSetInput(object[,] objects)
        {
            for (int i = objects.GetLowerBound(0); i <= objects.GetUpperBound(0); i++)
            {
                for (int j = objects.GetLowerBound(1); j < objects.GetUpperBound(1); j++)
                {
                    try
                    {
                        switch (objects[i, j].ToString().ToLower())
                        {
                            case "derivative":
                                _glasserman.Derivative = (Derivative) Enum.Parse(typeof(Derivative), objects[i, j + 1].ToString().ToLower());
                                break;
                            case "iterations":
                                _glasserman.Iterate = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "expiry":
                                _glasserman.Exp = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "tenor":
                                _glasserman.Ten = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "notional":
                                _glasserman.Notional = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "payoff":
                                _glasserman.PayoffFunction = objects[i, j + 1].ToString();
                                break;
                            
                        }
                    }
                    catch { }
                }
            }
        }

        public object SimSummary()
        {
            object temp;
            try
            {
                string[] str = _simSummaryString.Split('\n');
                var result = new object[str.Length, 1];
                for (int i = 0; i < str.Length; i++)
                {
                    result[i, 0] = str[i];
                }
                temp = result;
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }
        #endregion       
    }
}
