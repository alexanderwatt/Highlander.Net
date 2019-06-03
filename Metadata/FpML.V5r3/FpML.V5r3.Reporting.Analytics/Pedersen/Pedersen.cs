/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Analytics.Pedersen
{
    public class Pedersen
    {
        #region Declarations

        private readonly Economy _usd;
        private readonly Parameters _param;
        private readonly Recycle _rec;
        private readonly ObjectiveFunction _objective;
        private readonly Calibrator _pedersen;
        private readonly Cascade _cas;
        private readonly Simulator _glasserman;
        private static string _debugger;
        private static string _calsummarystring = "";
        private static string _simsummarystring = "";
        public static object[,] CurrentRange = new object[1,1];
        //public static Range CurrentRange { get; set; }

        #endregion

        #region Helpers

        public static void Debug(string s)
        {
            _debugger = s;
        }

        public object DebugOutput()
        {
            var res = new object[3, 1];
            res[0, 0] = _param.Ncplt;
            res[1, 0] = _param.Tcplt;
            res[2, 0] = _param.Nswpn;
            return res;
        }

        public static void Clear(string target)
        {
            if (target == "cal")
            {
                _calsummarystring = "";
            }
            if (target == "sim")
            {
                _simsummarystring = "";
            }
        }

        public static void Write(string s, string target)
        {
            if (target == "cal")
            {
                _calsummarystring = _calsummarystring + s;
            }
            if (target == "sim")
            {
                _simsummarystring = _simsummarystring + s;
            }
        }

        public static void WriteRange(object o)
        {
            CurrentRange[0,0] = o;
        }

        public static void WriteRange(int i, int j, object o)
        {
            //var result = CurrentRange as object[,];
            if (i > 0 && j > 0)
            {
                CurrentRange[i, j] = o;
            }
        }

        //public static void WriteRange(object o)
        //{
        //    CurrentRange.Value2 = o;
        //}

        //public static void WriteRange(int i, int j, object o)
        //{
        //    if (i > 0 && j > 0)
        //    {
        //        CurrentRange.Cells[i, j] = o;
        //    }
        //}

        #endregion

        #region Constructor

        public Pedersen()
        {
            _param = new Parameters();
            _rec = new Recycle(_param);
            _usd = new Economy(_param, _rec);
            _objective = new ObjectiveFunction(_usd, _rec, _param);
            _pedersen = new Calibrator(_objective, _param);
            _cas = new Cascade(_usd, _param);
            _glasserman = new Simulator(_usd, _param);
        }

        #endregion

        #region Data

        public object[,] ShowCorrelation()
        {
            var result = new object[1, 1];
            result[0, 0] = "Correlation Unsuccessfull";
            try
            {
                var res = new object[_param.Ntenor + 1, _param.Ntenor + 1];
                res[0, 0] = "Correlation";
                for (int i = 1; i <= _param.Ntenor; i++)
                {
                    res[0, i] = _param.Tenor[i];
                    res[i, 0] = _param.Tenor[i];
                }
                for (int i = 1; i <= _param.Ntenor; i++)
                {
                    for (int j = 1; j <= _param.Ntenor; j++)
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

        /*
         * Setting discount function from a range in the spreadsheet
         * Requires actual data
         */
        public string SetDiscount(List<double> discountFactorArray)
        {
            const int length = 123;
            string temp; //object
            try
            {
                //var objects = ConvertRangeTo2DArray(r1);
                //var result = new object[length + 1, 1];
                var discount = new double[length];
                int upper = Math.Min(length, discountFactorArray.Count);
                //for (int i = 1; i <= upper; i++)
                //{
                //    discount[i - 1] = double.Parse(objects[i, 1].ToString());
                //}
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
                //result[0, 0] = "Discount factors were set successfully.";
                //for (int i = 1; i <= length; i++)
                //{
                //    result[i, 0] = _usd.CurDiscount[i - 1];
                //}
                temp = "Discount factors were set successfully."; //result;
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        /*
         * Set discount from a yieldcurve ID
         */
        public string SetCurrentDiscount(IRateCurve rateCurve)
        {
            const int length = 123;
            string temp; //object
            try
            {
                //var result = new object[length + 1, 1];
                var discount = new double[length];
                DateTime today = DateTime.Now;
                for (int i = 0; i < length; i++)
                {
                    discount[i] = rateCurve.GetDiscountFactor(today, today.AddMonths(i * 3));
                }
                _usd.CurDiscount = discount;
                _usd.DiscountStatus = "User Defined";
                //result[0, 0] = String.Format("Discount factors were set.");
                //for (int i = 1; i <= length; i++)
                //{
                //    result[i, 0] = _usd.CurDiscount[i - 1];
                //}
                temp = String.Format("Discount factors were set.");//result;
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        public string SetCpltIvol(List<double> cpltVolArray)
        {
            string temp;
            try
            {
                var ivol = new double[120];
                for (int i = cpltVolArray.Count; i <= cpltVolArray.Count; i++)
                {
                    try
                    {
                        double vol = cpltVolArray[i];
                        if (vol > 2)
                        {
                            ivol[i - 1] = vol / 100;
                        }
                        else
                        {
                            ivol[i - 1] = vol;
                        }
                    }
                    catch
                    {
                        ivol[i - 1] = 0;
                    }
                }
                _usd.RawCpltIvol = ivol;
                _usd.CpltIvolStatus = "User Defined";
                temp = string.Format("Caplet Implied Vols were set successfully.");
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        public string SetCurrentCpltIvol(double strike, IStrikeVolatilitySurface volSurface)
        {
            const int length = 120;
            try
            {
                var temp = volSurface;
                if (temp!= null)
                {
                    var result = new object[length + 1, 1];
                    //DateTime today = DateTime.Now;
                    var ivol = new double[length];
                    for (int i = 1; i <= length; i++)
                    {
                        var term = i*3 + "M";
                        ivol[i - 1] = volSurface.GetValueByExpiryTermAndStrike(term, strike);
                        result[i, 0] = ivol[i - 1];
                    }
                    _usd.RawCpltIvol = ivol;
                    _usd.CpltIvolStatus = "User Defined";
                    //for (int i = 1; i <= length; i++)
                    //{
                    //    result[i, 0] = ivol[i - 1];
                    //}
                    //result[0, 0] = string.Format("Caplet Implied Vols were set successfully.");
                    return string.Format("Caplet Implied Vols were set successfully."); // result;                   
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return string.Format("Caplet Implied Vols not set successfully.");
        }

        public string SetSwpnIvol(object[,] objects)
        {
            string temp;
            try
            {
                //var objects = ConvertRangeTo2DArray(r1);
                var ivol = new double[objects.GetUpperBound(0), objects.GetUpperBound(1)];
                for (int i = 1; i <= objects.GetUpperBound(0); i++)
                {
                    for (int j = 1; j <= objects.GetUpperBound(1); j++)
                    {
                        try
                        {
                            double vol = double.Parse(objects[i, j].ToString());
                            if (vol > 2)
                            {
                                ivol[i - 1, j - 1] = vol / 100;
                            }
                            else
                            {
                                ivol[i - 1, j - 1] = vol;
                            }
                        }
                        catch
                        {
                            ivol[i - 1, j - 1] = 0;
                        }
                    }
                }
                _usd.RawSwpnIvol = ivol;
                _usd.SwpnIvolStatus = "User Defined";
                temp = "Swaption Implied Vols were set successfully.";
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        //public object SetCurrentSwpnIvol(object[,] volSurface)
        //{
        //    object temp;
        //    try
        //    {
        //        var ivol = new double[_param.SwpnExp.Length, _param.SwpnTen.Length];
        //        var result = new object[_param.SwpnExp.Length + 2, _param.SwpnTen.Length + 1];
        //        for (int i = _param.SwpnExp.GetLowerBound(0); i <= _param.SwpnExp.GetUpperBound(0); i++)
        //        {
        //            result[i + 2, 0] = _param.SwpnExp[i] + 1;
        //        }
        //        for (int i = _param.SwpnTen.GetLowerBound(0); i <= _param.SwpnTen.GetUpperBound(0); i++)
        //        {
        //            result[1, i + 1] = _param.SwpnTen[i] + 1;
        //        }
        //        result[1,0] = "Exp\\Ten (quarters)";
        //        for (int i = volSurface.GetLowerBound(0); i <= volSurface.GetUpperBound(0); i++)
        //        {
        //            try
        //            {
        //                int exp = ToQuarters.Convert(volSurface[i, volSurface.GetLowerBound(1)].ToString());
        //                int ten = ToQuarters.Convert(volSurface[i, volSurface.GetLowerBound(1) + 1].ToString());
        //                int j = Parameters.ExactPositionOf(exp - 1, _param.SwpnExp);
        //                int k = Parameters.ExactPositionOf(ten - 1, _param.SwpnTen);

        //                if (j > -1 && k > -1)
        //                {
        //                    double vol = double.Parse(volSurface[i, volSurface.GetLowerBound(1) + 2].ToString());
        //                    if (vol > 2)
        //                    {
        //                        ivol[j, k] = vol / 100;
        //                    }
        //                    else
        //                    {
        //                        ivol[j, k] = vol;
        //                    }
        //                    result[j + 2, k + 1] = ivol[j, k];
        //                }
        //            }
        //            catch
        //            {
        //            }
        //        }
        //        _usd.RawSwpnIvol = ivol;
        //        _usd.SwpnIvolStatus = "User Defined";
        //        result[0, 0] = string.Format("Swaption Implied Vols were set successfully.");
        //        temp = result;
        //    }
        //    catch (Exception e)
        //    {
        //        temp = e.ToString();
        //    }
        //    return temp;
        //}

        public object SetCor(object[,] objects)
        {
            object temp;
            try
            {
                //object[,] objects = ConvertRangeTo2DArray(r1);
                int size = Math.Min(objects.GetUpperBound(0), objects.GetUpperBound(1));
                var cor = new SymmetricMatrix(size);
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        cor[i, j] = double.Parse(objects[i + 1, j + 1].ToString());
                    }
                }
                _usd.CorData = cor;
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
            //CurrentRange = (Range)((Range)r1).Application.Selection;
            try
            {
                CalSetInput(r1);

                #region initialise objects
                //order is important, Param must be initialised first
                _param.Initialise();
                _rec.Initialise();
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
                //temp = (object)(debugger);

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
                    $"(Factors: {_param.NFAC}, Shift: {_usd.ConstShift}, ExpForm: {_objective.ExpForm}, Iter: {_pedersen.Iter}, Cplt Bound: {_cas.CpltBound}, Swpn Bound: {_cas.SwpnBound}, NSWPN: {_param.Nswpn}, NCPLT: {_param.Ncplt})";
                WriteRange(result);

                Write("Pedersen Calibration:\n", "cal");
                _pedersen.Go();
                Write("Cascade Algorithm:\n", "cal");
                _cas.Go();
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
            //object[,] objects = ConvertRangeTo2DArray(obj);
            for (int i = objects.GetLowerBound(0); i <= objects.GetUpperBound(0); i++)
            {
                for (int j = objects.GetLowerBound(1); j < objects.GetUpperBound(1); j++)
                {
                    try
                    {
                        switch (objects[i, j].ToString().ToLower())
                        {
                            case "factors":
                                _param.NFAC = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "shift":
                                _usd.ConstShift = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "exp form":
                                _objective.ExpForm = bool.Parse(objects[i, j + 1].ToString());
                                break;
                            case "iterations":
                                _pedersen.Iter = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "expiry":
                                _param.Uexpiry = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "tenor":
                                _param.Utenor = int.Parse(objects[i, j + 1].ToString());
                                break;
                            case "cplt bound":
                                _cas.CpltBound = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "swpn bound":
                                _cas.SwpnBound = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "use caplet":
                                _param.CpltOn = bool.Parse(objects[i, j + 1].ToString());
                                break;
                            case "use swaption":
                                _param.SwpnOn = bool.Parse(objects[i, j + 1].ToString());
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
                            case "cplt weight":
                                _objective.CWeight = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "swpn weight":
                                _objective.SWeight = double.Parse(objects[i, j + 1].ToString());
                                break;
                            case "discretisation":
                                string[] st = objects[i, j + 1].ToString().Split(',');
                                _param.Timeframe = new int[st.Length];
                                for (int k = 0; k < st.Length; k++)
                                {
                                    _param.Timeframe[k] = int.Parse(st[k].Trim());
                                }
                                break;
                        }
                    }
                    catch { }
                }
            }
        }

        private void CalSetDefault()
        {
            _param.NFAC = 3;
            _usd.ConstShift = 0;
            _objective.ExpForm = true;
            _pedersen.Iter = 50;
            _param.Uexpiry = 60;
            _param.Utenor = 60;
            _cas.CpltBound = 1.20;
            _cas.SwpnBound = 1.10;
            _param.CpltOn = true;
            _param.SwpnOn = true;
            _objective.NThreads = 2;
            _objective.HWeight = 0.001;
            _objective.VWeight = 0.001;
            _objective.CWeight = 1;
            _objective.SWeight = 1;
            _param.Timeframe = new[] { 0, 1, 2, 3, 4, 6, 8, 10, 12, 16, 20, 24, 28, 34, 40, 50, 60, 80, 100, 120 };
        }

        /*
         * Displays post-Calibration result summary.
         */ 
        public object CalSummary()
        {
            object temp;
            try
            {
                string[] str = _calsummarystring.Split('\n');
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

        /*
         * Displays post-Calibration vol surface (multifactored)
         */ 
        public object CalVol()
        {
            object temp;
            try
            {
                var result = new object[_param.NFAC*(_param.Uexpiry + 2) - 1, _param.Utenor + 1];
                for (int i = result.GetLowerBound(0); i < result.GetUpperBound(0); i++)
                {
                    for (int j = result.GetLowerBound(1); j < result.GetUpperBound(1); j++)
                    {
                        result[i, j] = "";
                    }
                }
                for (int j = 0; j < _param.NFAC; j++)
                {
                    result[j * (_param.Uexpiry + 2), 0] = $"Vol Fac {j + 1}";
                    for (int i = 0; i < _param.Uexpiry; i++)
                    {
                        result[j * (_param.Uexpiry + 2) + i + 1, 0] = i + 1;
                    }
                    for (int i = 0; i < _param.Utenor; i++)
                    {
                        result[j * (_param.Uexpiry + 2), i + 1] = i + 1;
                    }
                    for (int i = 0; i < _param.Uexpiry; i++)
                    {
                        for (int k = 0; k < _param.Utenor; k++)
                        {
                            double vol = _usd.Xi[i][k, j];
                            result[j * (_param.Uexpiry + 2) + i + 1, k + 1] = vol;
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

        /*
         * Displays post-Calibration vol surface (vol sizes)
         */
        public object CalVolNorm()
        {
            object temp;
            try
            {
                var result = new object[_param.Uexpiry + 1, _param.Utenor + 1];
                result[0, 0] = "Volatility";
                for (int i = 0; i < _param.Uexpiry; i++)
                {
                    result[i + 1, 0] = i + 1;
                }
                for (int i = 0; i < _param.Utenor; i++)
                {
                    result[0, i + 1] = i + 1;
                }
                for (int i = 0; i < _param.Uexpiry; i++)
                {
                    for (int j = 0; j < _param.Utenor; j++)
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
            //CurrentRange = (Range)((Range)r1).Application.Selection;
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
                //result = (object)(debugger);
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
                //result = (object)(debugger);
            }
        }

        private void SimSetInput(object[,] objects)
        {
            //object[,] objects = ConvertRangeTo2DArray(obj);
            for (int i = objects.GetLowerBound(0); i <= objects.GetUpperBound(0); i++)
            {
                for (int j = objects.GetLowerBound(1); j < objects.GetUpperBound(1); j++)
                {
                    try
                    {
                        switch (objects[i, j].ToString().ToLower())
                        {
                            case "derivative":
                                _glasserman.Deriv = (Derivative) Enum.Parse(typeof(Derivative), objects[i, j + 1].ToString().ToLower());
                                break;
                            case "iterations":
                                _glasserman.Iter = int.Parse(objects[i, j + 1].ToString());
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
                string[] str = _simsummarystring.Split('\n');
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
