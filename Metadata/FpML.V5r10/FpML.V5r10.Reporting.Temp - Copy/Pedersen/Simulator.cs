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
using System.Globalization;
using Highlander.Numerics.Pedersen;
using Matrix = Highlander.Numerics.LinearAlgebra.Matrix;
using DenseVector = MathNet.Numerics.LinearAlgebra.Double.DenseVector;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Pedersen
{
    public enum Derivative { Swaption, Caplet, Custom }

    public class Simulator
    {
        #region Declarations

        public Derivative Derivative { get; set; }

        public int Iterate { get; set; }

        public int Exp { get; set; }

        public int Ten { get; set; } = 4;

        private readonly Parameters _param;
        private readonly Economy _economy;
        private double[][] _discount;
        private double[][] _v;
        private Matrix[] _xi;
        private double[] _shift;

        public double Notional { get; set; } = 1;

        public string PayoffFunction { get; set; }

        public PayoffParser Payoff { get; set; }

        #endregion

        public Simulator(Economy eco, Parameters p)
        {
            Iterate = 1000;
            Derivative = Derivative.Caplet;
            Exp = 1;
            _economy = eco;
            _param = p;
        }

        private double SetStrike(int[] p)
        {
            //ATM
            double result = 0;
            if (Derivative == Derivative.Caplet)
            {
                result = _economy.CashRate(0, p[0]);
            }
            if (Derivative == Derivative.Swaption)
            {
                result = _economy.SwapRate(0, p[0], p[1]);
            }
            //if (Derivative == Derivative.custom)
            //{
            //    result = economy.CashRate(0, p[0]);
            //}
            return result;
        }

        private double FindPayoff(int[] p, double k)
        {
            double result = 0;
            if (Derivative == Derivative.Caplet)
            {
                result = _economy.CapletPayoff(p[0], k);
            }
            if (Derivative == Derivative.Swaption)
            {
                result = _economy.SwaptionPayoff(p[0], p[1], k);
            }
            if (Derivative == Derivative.Custom)
            {
                result = Payoff.Evaluate();
            }
            return result;
        }

        private double FindTheoretical(int[] p, double k)
        {
            double result = 0;
            if (Derivative == Derivative.Caplet)
            {
                result = _economy.CapletBlackScholes(p[0], k);
            }
            if (Derivative == Derivative.Swaption)
            {
                result = _economy.SwaptionBlackScholes(p[0], p[1], k);
            }
            if (Derivative == Derivative.Custom)
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
            SetV(_param.UnderlyingExpiry, _param.UnderlyingTenor);
            if (Derivative == Derivative.Custom)
            {
                Payoff = new PayoffParser(_economy, PayoffFunction);
            }
            double temp = 0;
            double temp2 = 0;
            var p = new[] { Exp, Ten };
            double k = SetStrike(p);
            for (int i = 0; i < Iterate; i++)
            {
                if (i % 100 == 0)
                {
                    Pedersen.WriteRange($"Iteration # {i}");
                }
                Simulate(_param.UnderlyingExpiry, _param.UnderlyingTenor);
                double c = FindPayoff(p, k);
                temp += c;
                temp2 += c * c;
            }
            double actual = Notional * FindTheoretical(p, k);
            double mean = Notional * temp / Iterate;
            double sd = Notional * Math.Sqrt((temp2 / Iterate - (temp / Iterate) * (temp / Iterate)) / (Iterate - 1.0));
            double error = (mean - actual) / sd;
            if (Derivative == Derivative.Caplet)
            {
                Pedersen.Write($"Caplet: {Exp}, ", "sim");
            }
            else if (Derivative == Derivative.Swaption)
            {
                Pedersen.Write($"Swaption: ({Exp}, {Ten}), ", "sim");
            }
            if (actual == 0)
            {
                Pedersen.Write($"Notional: {Notional}, Iterations: {Iterate}\n", "sim");
                Pedersen.Write("Theoretical: N/A\n", "sim");
                Pedersen.Write($"Sim Average: {mean}\n", "sim");
                Pedersen.Write($"Sim Std.Dev: {sd}\n", "sim");
                Pedersen.Write("Error / S.D: N/A\n", "sim");
            }
            else
            {
                Pedersen.Write($"Notional: {Notional}, Iterations: {Iterate}\n", "sim");
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
            var accumulateSum = new DenseVector[tenor + 1];
            for (int i = 0; i < tenor + 1; i++)
            {
                accumulateSum[i] = new DenseVector(_param.NumberOfFactors);
            }
            for (int i = 0; i < expiry; i++)
            {
                var bm = BrownianMotion.BMStep(0.25, _param.NumberOfFactors);
                accumulateSum[i].Clear();
                for (int j = i + 1; j < tenor + 1; j++)
                {
                    double temp = Math.Max(0, Math.Min(1, _v[i][j] / _discount[i][j]));
                    accumulateSum[j] = accumulateSum[j - 1] + temp * _xi[i].RowD(j - i - 1);//accumulateSum[j - 1] + temp * _xi[i][j - i - 1, Range.All];
                }
                for (int j = tenor; j > i; j--)
                {
                    var vol = _xi[i].RowD(j - i - 1)  - accumulateSum[j];//_xi[i][j - i - 1, Range.All] - accumulateSum[j];
                    _v[i + 1][j] = _v[i][j] * Math.Exp(vol * bm - 0.5 * (vol * vol) * 0.25);
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
