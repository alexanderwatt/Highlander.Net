using System;
using System.Globalization;
using Extreme.Mathematics.LinearAlgebra;
using PedersenHost.Utilities;

namespace PedersenHost
{
    public enum Derivative { Swaption, Caplet, Custom }

    class Simulator
    {
        #region declarations

        private Derivative _deriv = Derivative.Caplet;
        public Derivative Deriv
        {
            get { return _deriv; }
            set { _deriv = value; }
        }
        private int _iter = 1000;
        public int Iter
        {
            get { return _iter; }
            set { _iter = value; }
        }

        public int Exp { get; set; }

        private int _ten = 4;
        public int Ten
        {
            get { return _ten; }
            set { _ten = value; }
        }

        private readonly Parameters _param;
        private readonly Economy _economy;
        private double[][] _discount;
        private double[][] _v;
        private GeneralMatrix[] _xi;
        private double[] _shift;
        private double _notional = 1;
        public double Notional
        {
            get { return _notional; }
            set { _notional = value; }
        }

        public string PayoffFunction { get; set; }

        public PayoffParser.PayoffParser Payoff { get; set; }

        #endregion

        public Simulator(Economy eco, Parameters p)
        {
            Exp = 1;
            _economy = eco;
            _param = p;
        }

        private double SetStrike(int[] p)
        {
            //ATM
            double result = 0;
            if (Deriv == Derivative.Caplet)
            {
                result = _economy.CashRate(0, p[0]);
            }
            if (Deriv == Derivative.Swaption)
            {
                result = _economy.SwapRate(0, p[0], p[1]);
            }
            //if (Deriv == Derivative.custom)
            //{
            //    result = economy.CashRate(0, p[0]);
            //}
            return result;
        }

        private double FindPayoff(int[] p, double k)
        {
            double result = 0;
            if (Deriv == Derivative.Caplet)
            {
                result = _economy.CpltPayoff(p[0], k);
            }
            if (Deriv == Derivative.Swaption)
            {
                result = _economy.SwpnPayoff(p[0], p[1], k);
            }
            if (Deriv == Derivative.Custom)
            {
                result = Payoff.Evaluate();
            }
            return result;
        }

        private double FindTheoretical(int[] p, double k)
        {
            double result = 0;
            if (Deriv == Derivative.Caplet)
            {
                result = _economy.CpltBS(p[0], k);
            }
            if (Deriv == Derivative.Swaption)
            {
                result = _economy.SwpnBS(p[0], p[1], k);
            }
            if (Deriv == Derivative.Custom)
            {
                result = 0;
            }
            return result;
        }

        public double Go()
        {
            _discount = _economy.Discount;
            _xi = _economy.Xi;
            if (_xi == null)
            {
                throw new Exception("Run Calibrator first!");
            }
            _shift = _economy.Shift;
            SetV(_param.Uexpiry, _param.Utenor);
            if (Deriv == Derivative.Custom)
            {
                Payoff = new PayoffParser.PayoffParser(_economy, PayoffFunction);
            }

            double temp = 0;
            double temp2 = 0;
            var p = new[] { Exp, Ten };
            double k = SetStrike(p);

            for (int i = 0; i < Iter; i++)
            {
                if (i % 100 == 0)
                {
                    Program.WriteRange(String.Format("Iteration # {0}", i));
                }
                Simulate(_param.Uexpiry, _param.Utenor);
                double c = FindPayoff(p, k);
                temp += c;
                temp2 += c * c;
            }
            double actual = Notional * FindTheoretical(p, k);
            double mean = Notional * temp / Iter;
            double sd = Notional * Math.Sqrt((temp2 / Iter - (temp / Iter) * (temp / Iter)) / (Iter - 1.0));
            double error = (mean - actual) / sd;
            if (Deriv == Derivative.Caplet)
            {
                Program.Write(String.Format("Caplet: {0}, ", Exp), "sim");
            }
            else if (Deriv == Derivative.Swaption)
            {
                Program.Write(String.Format("Swaption: ({0}, {1}), ", Exp, Ten), "sim");
            }
            if (actual == 0)
            {
                Program.Write(String.Format("Notional: {0}, Iterations: {1}\n", Notional, Iter), "sim");
                Program.Write("Theoretical: N/A\n", "sim");
                Program.Write(String.Format("Sim Average: {0}\n", mean), "sim");
                Program.Write(String.Format("Sim Std.Dev: {0}\n", sd), "sim");
                Program.Write("Error / S.D: N/A\n", "sim");
            }
            else
            {
                Program.Write(String.Format("Notional: {0}, Iterations: {1}\n", Notional, Iter), "sim");
                Program.Write(String.Format("Theoretical: {0}\n", actual), "sim");
                Program.Write(String.Format("Sim Average: {0}\n", mean), "sim");
                Program.Write(String.Format("Sim Std.Dev: {0}\n", sd), "sim");
                Program.Write(String.Format("Error / S.D: {0}\n", error), "sim");
            }

            return mean;

        }

        private void SetV(int expiry, int tenor)
        {
            _v = new double[expiry + 1][];
            _v[0] = new double[tenor + 1];
            for (int i = 0; i < expiry; i++)
            {
                _v[i + 1] = new double[tenor + 1];
            }
            for (int i = 0; i < tenor; i++)
            {
                Program.Debug(_discount[0][i].ToString(CultureInfo.InvariantCulture));
                _v[0][i] = _discount[0][i] - (1 - 0.25 * _shift[i]) * _discount[0][i + 1];
            }
            _v[0][tenor] = _discount[0][tenor];
        }

        private void Simulate(int expiry, int tenor)
        {
            var accsum = new GeneralVector[tenor + 1];
            for (int i = 0; i < tenor + 1; i++)
            {
                accsum[i] = new GeneralVector(_param.NFAC);
            }

            for (int i = 0; i < expiry; i++)
            {
                GeneralVector bm = BrownianMotion.BMStep(0.25, _param.NFAC);
                accsum[i].SetToZero();
                for (int j = i + 1; j < tenor + 1; j++)
                {
                    double temp = Math.Max(0, Math.Min(1, _v[i][j] / _discount[i][j]));
                    accsum[j] = (GeneralVector)Vector.Add(accsum[j - 1], Vector.Multiply(temp, _xi[i][j - i - 1, Range.All]));
                }
                for (int j = tenor; j > i; j--)
                {
                    var vol = (GeneralVector)Vector.Subtract(_xi[i][j - i - 1, Range.All], accsum[j]);
                    _v[i + 1][j] = _v[i][j] * Math.Exp(Vector.DotProduct(vol, bm) - 0.5 * Vector.DotProduct(vol, vol) * 0.25);
                    if (j < tenor)
                    {
                        _discount[i + 1][j] = _discount[i + 1][j + 1] * (1 - 0.25 * _shift[j]) + _v[i + 1][j];
                    }
                    else
                    {
                        _discount[i + 1][j] = _v[i + 1][j];
                    }
                }
            }
        }
    }
}
