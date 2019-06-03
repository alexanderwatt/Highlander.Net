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
using System.Globalization;

#endregion

namespace Orion.Analytics.Pedersen
{
    public enum Derivative { Swaption, Caplet, Custom }

    public class Simulator
    {
        #region declarations

        public Derivative Deriv { get; set; }

        public int Iter { get; set; }

        public int Exp { get; set; }

        public int Ten { get; set; } = 4;

        private readonly Parameters _param;
        private readonly Economy _economy;
        private double[][] _discount;
        private double[][] _v;
        private GeneralMatrix[] _xi;
        private double[] _shift;

        public double Notional { get; set; } = 1;

        public string PayoffFunction { get; set; }

        public PayoffParser Payoff { get; set; }

        #endregion

        public Simulator(Economy eco, Parameters p)
        {
            Iter = 1000;
            Deriv = Derivative.Caplet;
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
                Payoff = new PayoffParser(_economy, PayoffFunction);
            }

            double temp = 0;
            double temp2 = 0;
            var p = new[] { Exp, Ten };
            double k = SetStrike(p);

            for (int i = 0; i < Iter; i++)
            {
                if (i % 100 == 0)
                {
                    Pedersen.WriteRange($"Iteration # {i}");
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
                Pedersen.Write($"Caplet: {Exp}, ", "sim");
            }
            else if (Deriv == Derivative.Swaption)
            {
                Pedersen.Write($"Swaption: ({Exp}, {Ten}), ", "sim");
            }
            if (actual == 0)
            {
                Pedersen.Write($"Notional: {Notional}, Iterations: {Iter}\n", "sim");
                Pedersen.Write("Theoretical: N/A\n", "sim");
                Pedersen.Write($"Sim Average: {mean}\n", "sim");
                Pedersen.Write($"Sim Std.Dev: {sd}\n", "sim");
                Pedersen.Write("Error / S.D: N/A\n", "sim");
            }
            else
            {
                Pedersen.Write($"Notional: {Notional}, Iterations: {Iter}\n", "sim");
                Pedersen.Write($"Theoretical: {actual}\n", "sim");
                Pedersen.Write($"Sim Average: {mean}\n", "sim");
                Pedersen.Write($"Sim Std.Dev: {sd}\n", "sim");
                Pedersen.Write($"Error / S.D: {error}\n", "sim");
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
                Pedersen.Debug(_discount[0][i].ToString(CultureInfo.InvariantCulture));
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
