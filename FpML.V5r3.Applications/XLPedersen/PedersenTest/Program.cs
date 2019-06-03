using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Extreme.Mathematics.LinearAlgebra;
using System.Threading;
using Orion.CurveEngine.PricingStructures.Curves;
using PedersenHost.Utilities;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace PedersenHost
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]

    /* 
     * Handles the interface from Excel
     */

    public class Program
    {
        #region declarations

        private readonly Economy _usd;
        private readonly Parameters _param;
        private readonly Recycle _rec;
        private readonly ObjectiveFunction _objective;
        private readonly Calibrator _pedersen;
        private readonly Cascade _cas;
        private readonly Simulator _glasserman;
        private static string _debugger = "";
        private static string _calsummarystring = "";
        private static string _simsummarystring = "";

        public static Range CurrentRange { get; set; }


        public static void WriteRange(object o)
        {
            CurrentRange.Value2 = o;
        }

        public static void WriteRange(int i, int j, object o)
        {
            if (i > 0 && j > 0)
            {
                CurrentRange.Cells[i, j] = o;
            }
        }

        #endregion

        //public object aaa(object o)
        //{
        //    try
        //    {
        //        Thread t = new Thread(new ThreadStart(ts));
        //        CurrentRange = (Microsoft.Office.Interop.Excel.Range)o;
        //        CurrentRange = (Microsoft.Office.Interop.Excel.Range)CurrentRange.Application.Selection;
        //        t.Start();


        //        return "Done";
        //    }
        //    catch (Exception e)
        //    {
        //        return e.Message;
        //    }
        //}

        //public void ts()
        //{
        //    try
        //    {

        //        Thread.Sleep(TimeSpan.FromMilliseconds(200));

        //        CurrentRange.Value2 = (object)"abc";
        //    }
        //    catch
        //    {
        //    }
        //}

        public Program()
        {
            _param = new Parameters();
            _rec = new Recycle(_param);
            _usd = new Economy(_param, _rec);
            _objective = new ObjectiveFunction(_usd, _rec, _param);
            _pedersen = new Calibrator(_objective, _param);
            _cas = new Cascade(_usd, _param);
            _glasserman = new Simulator(_usd, _param);
        }

        #region Data
        public object ShowCorrelation()
        {
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
                return e.ToString();
            }
        }

        /*
         * Setting discount function from a range in the spreadsheet
         * Requires actual data
         */
        public object SetDiscount(object r1)
        {
            const int length = 123;
            object temp;
            try
            {
                var objects = ConvertRangeTo2DArray(r1);
                var result = new object[length + 1, 1];
                var discount = new double[length];
                int upper = Math.Min(length, objects.GetUpperBound(0));
                for (int i = 1; i <= upper; i++)
                {
                    discount[i - 1] = double.Parse(objects[i, 1].ToString());
                }
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
                result[0, 0] = "Discount factors were set successfully.";
                for (int i = 1; i <= length; i++)
                {
                    result[i, 0] = _usd.CurDiscount[i - 1];
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
         * Set discount from a yieldcurve ID
         */
        public object SetCurrentDiscount(Range range)
        {
            const int length = 123;
            object temp;
            try
            {
                //var range = (Range)r1;
                string s = range.Value2.ToString();
                var result = new object[length + 1, 1];
                var discount = new double[length];
                var rc = new RateCurve();
                DateTime today = DateTime.Now;
                for (int i = 0; i < length; i++)
                {
                    discount[i] = rc.GetDiscountFactor(s, today.AddMonths(i * 3));
                }
                _usd.CurDiscount = discount;
                _usd.DiscountStatus = "User Defined";
                result[0, 0] = String.Format("Discount factors were set to ({0})." , s) ;
                for (var i = 1; i <= length; i++)
                {
                    result[i, 0] = _usd.CurDiscount[i - 1];
                }
                temp = result;
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        public object SetCpltIvol(object r1)
        {
            object temp;
            try
            {
                var objects = ConvertRangeTo2DArray(r1);
                var ivol = new double[120];
                if (objects.GetLength(1) == 1)
                {
                    for (int i = objects.GetLowerBound(0); i <= objects.GetUpperBound(0); i++)
                    {
                        try
                        {
                            double vol = double.Parse(objects[i, objects.GetLowerBound(1)].ToString());
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
                else if (objects.GetLength(1) == 2)
                {
                    for (int i = objects.GetLowerBound(0); i <= objects.GetUpperBound(0); i++)
                    {
                        try
                        {
                            int j = ToQuarters.Convert(objects[i, objects.GetLowerBound(1)].ToString());
                            if (j > 0)
                            {
                                double vol = double.Parse(objects[i, objects.GetLowerBound(1) + 1].ToString());
                                if (vol > 2)
                                {
                                    ivol[j - 1] = vol / 100;
                                }
                                else
                                {
                                    ivol[j - 1] = vol;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    _usd.RawCpltIvol = ivol;
                    _usd.CpltIvolStatus = "User Defined";
                    temp = string.Format("Caplet Implied Vols were set successfully.");
                }
                else
                {
                    temp = "Invalid Input.";
                }
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        public object SetCurrentCpltIvol(Range range)
        {
            const int length = 123;
            object temp;
            try
            {
                //var range = (Range)r1;
                string s = range.Value2.ToString();
                temp = nabCap.QR.PricingStructureAPI.VolatilitySurfaceAPI.GetValue(s);
                if (temp.GetType() == typeof(Object[,]))
                {
                    var result = new object[length + 1, 1];
                    var objects = (object[,])temp;
                    var ivol = new double[length];
                    for (int i = objects.GetLowerBound(0); i <= objects.GetUpperBound(0); i++)
                    {
                        try
                        {
                            int j = ToQuarters.Convert(objects[i, objects.GetLowerBound(1)].ToString());
                            if (j > 0)
                            {
                                double vol = double.Parse(objects[i, objects.GetLowerBound(1) + 1].ToString());
                                if (vol>2)
                                {
                                    ivol[j - 1] = vol / 100;
                                }
                                else
                                {
                                    ivol[j - 1] = vol;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                    _usd.RawCpltIvol = ivol;
                    _usd.CpltIvolStatus = "User Defined";
                    for (int i = 1; i <= length; i++)
                    {
                        result[i, 0] = ivol[i - 1];
                    }
                    result[0, 0] = string.Format("Caplet Implied Vols were set to ({0}).", s);
                    temp = result;
                }
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        public object SetSwpnIvol(object r1)
        {
            object temp;
            try
            {
                var objects = ConvertRangeTo2DArray(r1);
                var ivol = new double[objects.GetUpperBound(0), objects.GetUpperBound(1)];
                for (int i = 1; i <= objects.GetUpperBound(0); i++)
                {
                    for (int j = 1; j <= objects.GetUpperBound(1); j++)
                    {
                        try
                        {
                            var vol = double.Parse(objects[i, j].ToString());
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

        public object SetCurrentSwpnIvol(Range range)
        {
            object temp;
            try
            {
                //var range = (Range)r1;
                string s = range.Value2.ToString();
                temp = nabCap.QR.PricingStructureAPI.VolatilitySurfaceAPI.GetValue(s);
                if (temp.GetType() == typeof(Object[,]))
                {
                    var objects = (object[,])temp;
                    var ivol = new double[_param.SwpnExp.Length, _param.SwpnTen.Length];
                    var result = new object[_param.SwpnExp.Length + 2, _param.SwpnTen.Length + 1];
                    for (int i = _param.SwpnExp.GetLowerBound(0); i <= _param.SwpnExp.GetUpperBound(0); i++)
                    {
                        result[i + 2, 0] = _param.SwpnExp[i] + 1;
                    }
                    for (int i = _param.SwpnTen.GetLowerBound(0); i <= _param.SwpnTen.GetUpperBound(0); i++)
                    {
                        result[1, i + 1] = _param.SwpnTen[i] + 1;
                    }
                    result[1,0] = "Exp\\Ten (quarters)";
                    for (int i = objects.GetLowerBound(0); i <= objects.GetUpperBound(0); i++)
                    {
                        try
                        {
                            int exp = ToQuarters.Convert(objects[i, objects.GetLowerBound(1)].ToString());
                            int ten = ToQuarters.Convert(objects[i, objects.GetLowerBound(1) + 1].ToString());
                            int j = Parameters.ExactPositionOf(exp - 1, _param.SwpnExp);
                            int k = Parameters.ExactPositionOf(ten - 1, _param.SwpnTen);
                            if (j > -1 && k > -1)
                            {
                                double vol = double.Parse(objects[i, objects.GetLowerBound(1) + 2].ToString());
                                if (vol > 2)
                                {
                                    ivol[j, k] = vol / 100;
                                }
                                else
                                {
                                    ivol[j, k] = vol;
                                }
                                result[j + 2, k + 1] = ivol[j, k];
                            }
                        }
                        catch
                        {
                        }
                    }
                    _usd.RawSwpnIvol = ivol;
                    _usd.SwpnIvolStatus = "User Defined";
                    result[0, 0] = string.Format("Swaption Implied Vols were set to ({0}).", s);
                    temp = result;
                }
            }
            catch (Exception e)
            {
                temp = e.ToString();
            }
            return temp;
        }

        public object SetCor(object r1)
        {
            object temp;
            try
            {
                object[,] objects = ConvertRangeTo2DArray(r1);
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
        public object Calibration(object r1)
        {
            object temp;
            Clear("cal");
            CurrentRange = (Range)((Range)r1).Application.Selection;
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
                result[1, 0] = String.Format("(Factors: {0}, Shift: {1}, ExpForm: {2}, Iter: {3}, Cplt Bound: {4}, Swpn Bound: {5}, NSWPN: {6}, NCPLT: {7})", 
                                             _param.NFAC, _usd.ConstShift, _objective.ExpForm, _pedersen.Iter, _cas.CpltBound, _cas.SwpnBound, _param.NSWPN, _param.NCPLT);
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

        private void CalSetInput(object obj)
        {
            CalSetDefault();
            object[,] objects = ConvertRangeTo2DArray(obj);
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
                    result[j * (_param.Uexpiry + 2), 0] = String.Format("Vol Fac {0}", (j + 1));
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
        public object Simulation(object r1)
        {
            object result;
            Clear("sim");
            CurrentRange = (Range)((Range)r1).Application.Selection;
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

        private void SimSetInput(object obj)
        {
            object[,] objects = ConvertRangeTo2DArray(obj);
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

        public static void Debug(string s)
        {
            _debugger = s;
        }

        public object DebugOutput()
        {
            var res = new object[3, 1];
            res[0, 0] = _param.NCPLT;
            res[1, 0] = _param.TCPLT;
            res[2, 0] = _param.NSWPN;
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

        //public static void InitialiseOutputArray(object[,] arr)
        //{
        //    for (int i = arr.GetLowerBound(0); i <= arr.GetUpperBound(0); i++)
        //    {
        //        arr[i]
        //    }
        //}

        #region Range Parsing

        public static object[,] ConvertRangeTo2DArray(object rangeAsObject)
        {
            var range = (Range)rangeAsObject;
            var valueArray = (object[,])range.Value[Type.Missing];
            var formulaArray = (object[,])range.Formula;
            if (
                valueArray.GetUpperBound(0) != formulaArray.GetUpperBound(0) ||
                valueArray.GetUpperBound(1) != formulaArray.GetUpperBound(1) ||
                valueArray.GetLowerBound(0) != formulaArray.GetLowerBound(0) ||
                valueArray.GetLowerBound(1) != formulaArray.GetLowerBound(1)
                )
            {
                throw new Exception("RangeHelper.ConvertRangeTo2DArray method failed. Dimensions of the value and formula arrays must be the same.");
            }
            for (int i = valueArray.GetLowerBound(0); i <= valueArray.GetUpperBound(0); ++i)
            {
                for (int j = valueArray.GetLowerBound(1); j <= valueArray.GetUpperBound(1); ++j)
                {
                    var formulaCellValue = (string)formulaArray[i, j];
                    // if a correspondent formula value has indicated that there's no value in it - put the NULL into value array.
                    //
                    if (IsNAString(formulaCellValue))
                    {
                        valueArray[i, j] = null;
                    }
                }
            }
            return valueArray;
        }

        /// list of possible N/A strings in Excel
        /// 
        /// #NULL! 0x800a07d0
        /// #DIV/0! 0x800a07d7
        /// #VALUE! 0x800a07df
        /// #REF! 0x800a07e7
        /// #NAME? 0x800a07ed
        /// #NUM! 0x800a07f4
        /// #N/A 0x800a07fa
        private static readonly List<string> NAValues = new List<string>(new[] { "#NULL!", "#DIV/0!", "#VALUE!", "#REF!", "#NAME?", "#NUM!", "#N/A" });

        public static bool IsNAString(string value)
        {
            return -1 != NAValues.IndexOf(value);
        }

        #endregion

        #region Com

        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            Microsoft.Win32.Registry.ClassesRoot.CreateSubKey
                ("CLSID\\{" + t.GUID.ToString().ToUpper() + "}\\Programmable");
        }

        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            Microsoft.Win32.Registry.ClassesRoot.DeleteSubKey
                ("CLSID\\{" + t.GUID.ToString().ToUpper() + "}\\Programmable");
        }

        #endregion
    }
}
