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
using System.Globalization;

#endregion

namespace Orion.Analytics.Pedersen
{
    #region abstract classes
    public abstract class NodeOperation
    {
        public char Symbol { get; set; }

        protected virtual double EvaluateOnce(double x, double y)
        {
            return 0;
        }
        public virtual double Evaluate(Node[] n)
        {
            double result = n[0].Evaluate();
            for (int i = 1; i < n.Length; i++)
            {
                result = EvaluateOnce(result, n[i].Evaluate());
            }
            return result;
        }
        public virtual Node[] Parse(string s, NodeOperation[] nodeOps)
        {
            string[] splitstr = ParseHelper.SpecialSplit(s, Symbol);
            if (splitstr.Length > 1)
            {
                return Node.CreateNodes(splitstr, nodeOps);
            }
            return null;
        }
    }
    public abstract class NodeFunction : NodeOperation
    {
        public string Name { get; set; }

        public override Node[] Parse(string s, NodeOperation[] nodeOps)
        {
            if (s.IndexOf(Name + "(", StringComparison.CurrentCultureIgnoreCase) == 0)
            {
                s = s.Substring(Name.Length);
                s = ParseHelper.RemoveOuterBracket(s);
                string[] splitstr = Symbol != 0 ? ParseHelper.SpecialSplit(s, Symbol) : new[] { s };
                return Node.CreateNodes(splitstr, nodeOps);
            }
            return null;
        }
    }
    abstract class NodeSeriesFunction : NodeFunction
    {
        public CounterCollection Cc { get; set; }

        public override Node[] Parse(string s, NodeOperation[] nodeOps)
        {
            if (Cc != null)
            {
                if (s.IndexOf(Name + "{", StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    s = s.Substring(Name.Length);
                    var splitstr = new string[4];
                    int temp = ParseHelper.MatchBracket(s, 0);
                    splitstr[3] = s.Substring(temp + 1);
                    s = s.Substring(1, temp - 1);
                    temp = s.IndexOf('=');
                    string argname = s.Substring(0, temp);
                    s = s.Substring(temp + 1);
                    temp = s.IndexOf(':');
                    splitstr[1] = s.Substring(0, temp);
                    splitstr[2] = s.Substring(temp + 1);
                    Cc.CreateCounter(argname);
                    splitstr[0] = Cc.IndexOf(argname).ToString(CultureInfo.InvariantCulture);
                    return Node.CreateNodes(splitstr, nodeOps);
                }
                return null;
            }
            return base.Parse(s, nodeOps);
        }

        public override double Evaluate(Node[] n)
        {
            if (Cc != null)
            {
                var counterid = (int)n[0].Evaluate();
                var start = (int)n[1].Evaluate();
                var finish = (int)n[2].Evaluate();
                Cc.Set(counterid, start);
                double result = n[3].Evaluate();

                for (int i = start + 1; i <= finish; i++)
                {
                    Cc.Set(counterid, i);
                    result = EvaluateOnce(result, n[3].Evaluate());
                }
                return result;
            }
            return base.Evaluate(n);
        }
    }
    #endregion

    #region NodeOperations
    class Add : NodeOperation
    {
        public Add()
        {
            Symbol = '+';
        }
        protected override double EvaluateOnce(double x, double y)
        {
            return x + y;
        }
    }
    class Subtract : NodeOperation
    {
        public Subtract()
        {
            Symbol = '-';
        }
        protected override double EvaluateOnce(double x, double y)
        {
            return x - y;
        }
    }
    class Multiply : NodeOperation
    {
        public Multiply()
        {
            Symbol = '*';
        }
        protected override double EvaluateOnce(double x, double y)
        {
            return x * y;
        }
    }
    class Divide : NodeOperation
    {
        public Divide()
        {
            Symbol = '/';
        }
        protected override double EvaluateOnce(double x, double y)
        {
            return x / y;
        }
    }
    class Power : NodeOperation
    {
        public Power()
        {
            Symbol = '^';
        }
        protected override double EvaluateOnce(double x, double y)
        {
            return Math.Pow(x, y);
        }

    }

    class Counter : NodeOperation
    {
        readonly CounterCollection _cc;
        public Counter(CounterCollection c)
        {
            _cc = c;
        }
        public override Node[] Parse(string s, NodeOperation[] nodeOps)
        {
            int i = _cc.IndexOf(s);
            if (i > -1)
            {
                var splitstr = new[] { i.ToString(CultureInfo.InvariantCulture) };
                return Node.CreateNodes(splitstr, nodeOps);
            }
            return null;
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            return _cc.Get((int)n0);
        }
    }
    #endregion

    #region NodeFunctions
    class Sqrt : NodeFunction
    {
        public Sqrt()
        {
            Name = "Sqrt";
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            return Math.Sqrt(n0);
        }
    }
    class Exp : NodeFunction
    {
        public Exp()
        {
            Name = "Exp";
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            return Math.Exp(n0);
        }
    }
    class Log : NodeFunction
    {
        public Log()
        {
            Symbol = ',';
            Name = "Log";
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            if (n.Length == 1)
            {
                return Math.Log10(n0);
            }
            double n1 = n[1].Evaluate();
            return Math.Log(n0, n1);
        }
    }
    class Ln : NodeFunction
    {
        public Ln()
        {
            Name = "Ln";
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            return Math.Log(n0);
        }
    }
    class Abs : NodeFunction
    {
        public Abs()
        {
            Name = "Abs";
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            return Math.Abs(n0);
        }
    }
    class Int : NodeFunction
    {
        public Int()
        {
            Name = "Int";
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            return Math.Floor(n0);
        }
    }
    class Pos : NodeFunction
    {
        public Pos()
        {
            Name = "Pos";
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            return Math.Max(n0, 0);
        }
    }

    #region rate related
    class Rate : NodeFunction //aka libor
    {
        readonly Economy _eco;
        public Rate(Economy e)
        {
            Symbol = ',';
            Name = "Rate";
            _eco = e;
        }

        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            if (n.Length == 1)
            {
                return _eco.CashRate((int)n0, (int)n0);
            }
            double n1 = n[1].Evaluate();
            return _eco.CashRate((int)n0, (int)n1);
        }
    }
    class ATM : NodeFunction
    {
        readonly Economy _eco;
        public ATM(Economy e)
        {
            Name = "ATM";
            _eco = e;
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            return _eco.CashRate(0, (int)n0);
        }
    }
    class SwapRate : NodeFunction
    {
        readonly Economy _eco;
        public SwapRate(Economy e)
        {
            Symbol = ',';
            Name = "SwapRate";
            _eco = e;
        }
        public override double Evaluate(Node[] n)
        {
            if (n.Length < 2)
            {
                throw new Exception("!! Swap rate requires an expiry and a tenor.");
            }
            double n0 = n[0].Evaluate();
            double n1 = n[1].Evaluate();
            if (n.Length == 2)
            {
                return _eco.SwapRate((int)n0, (int)n0, (int)n1);
            }
            double n2 = n[2].Evaluate();
            return _eco.SwapRate((int)n0, (int)n1, (int)n2);
        }
    }
    class Discount : NodeFunction
    {
        readonly Economy _eco;
        public Discount(Economy e)
        {
            Symbol = ',';
            Name = "Discount";
            _eco = e;
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            if (n.Length == 1)
            {
                return _eco.Discount[(int)n0][(int)n0];
            }
            double n1 = n[1].Evaluate();
            return _eco.Discount[(int)n0][(int)n1];
        }
    }
    class PV : NodeFunction
    {
        readonly Economy _eco;
        public PV(Economy e)
        {
            Name = "PV";
            _eco = e;
        }
        public override double Evaluate(Node[] n)
        {
            double n0 = n[0].Evaluate();
            return _eco.Discount[(int)n0][(int)n0];
        }
    }
    #endregion

    #endregion

    #region NodeSeriesFunctions
    class Max : NodeSeriesFunction
    {
        public Max()
        {
            Symbol = ',';
            Name = "Max";
        }
        public Max(CounterCollection c)
        {
            Name = "Max";
            Cc = c;
        }
        protected override double EvaluateOnce(double x, double y)
        {
            return Math.Max(x, y);
        }
    }
    class Min : NodeSeriesFunction
    {
        public Min()
        {
            Symbol = ',';
            Name = "Min";
        }
        public Min(CounterCollection c)
        {
            Name = "Min";
            Cc = c;
        }
        protected override double EvaluateOnce(double x, double y)
        {
            return Math.Min(x, y);
        }
    }
    class Sum : NodeSeriesFunction
    {
        public Sum()
        {
            Symbol = ',';
            Name = "Sum";
        }
        public Sum(CounterCollection c)
        {
            Name = "Sum";
            Cc = c;
        }
        protected override double EvaluateOnce(double x, double y)
        {
            return x + y;
        }
    }
    #endregion

    class CounterCollection
    {
        readonly List<int> _counters = new List<int>();
        readonly List<string> _names = new List<string>();

        public void CreateCounter(string s)
        {
            _names.Add(s);
            _counters.Add(0);
        }

        public int Get(string s)
        {
            int i = _names.IndexOf(s);
            return _counters[i];
        }

        public int Get(int i)
        {
            return _counters[i];
        }

        public void Set(string s, int x)
        {
            int i = _names.IndexOf(s);
            _counters[i] = x;
        }

        public void Set(int i, int x)
        {
            _counters[i] = x;
        }

        public void Step(string s)
        {
            int i = _names.IndexOf(s);
            _counters[i]++;
        }

        public int IndexOf(string s)
        {
            return _names.IndexOf(s);
        }
    }
}
