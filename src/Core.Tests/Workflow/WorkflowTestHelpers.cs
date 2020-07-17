/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Threading;

namespace Highlander.Core.Workflow.Tests
{
    public struct T1 { public int A;}
    public struct T2 { public int B;}
    public struct T3 { public int C;}
    public class SequentialSubstep1 : WorkstepBase<T1, T2>
    {
        public SequentialSubstep1() { }
        protected override T2 OnExecute(T1 input)
        {
            T2 result;
            result.B = input.A;
            Thread.Sleep(100);
            return result;
        }
    }
    public class SequentialSubstep2 : WorkstepBase<T2, T3>
    {
        public SequentialSubstep2() { }
        protected override T3 OnExecute(T2 input)
        {
            T3 result;
            result.C = input.B;
            Thread.Sleep(100);
            return result;
        }
    }
    public class BranchSubstep : WorkstepBase<T1, T2>
    {
        private int _op;
        public BranchSubstep(int op) { _op = op; }
        protected override T2 OnExecute(T1 input)
        {
            T2 result;
            result.B = input.A * _op;
            return result;
        }
    }
    public class SequentialStep : WorkstepBaseSeq<T1, T3>
    {
        public SequentialStep()
        {
            AddSubstep(new SequentialSubstep1());
            AddSubstep(new SequentialSubstep2());
        }
    }
    public class BranchingStep : WorkstepBaseAlt<T1, T2>
    {
        protected override int OnAltChoose(T1 input)
        {
            return input.A;
        }
        public BranchingStep(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddSubstep(new BranchSubstep(i));
            }
        }
    }
    public struct P1 { public int From; public int To; public int Slice;}
    public struct P2 { public int P; public int Q; }
    public struct P8 { public long Subtotal; }
    public struct P9 { public long Total; }
    public class ParallelSubstep : WorkstepBase<P2, P8>
    {
        public ParallelSubstep() { }
        protected override P8 OnExecute(P2 input)
        {
            P8 result;
            result.Subtotal = 0;
            for (int i = input.P; i <= input.Q; i++)
            {
                result.Subtotal += i;
            }
            Random rg = new Random();
            Thread.Sleep(rg.Next(5000));
            return result;
        }
    }
    public class ParallelStep : WorkstepBasePar<P1, P2, P8, P9>
    {
        protected override P2[] OnParSplit(P1 input)
        {
            List<P2> result = new List<P2>();
            int lower = input.From;
            int upper = input.From + input.Slice - 1;
            while (upper < input.To)
            {
                P2 p2 = new P2 {P = lower, Q = upper};
                result.Add(p2);
                lower = upper + 1;
                upper += input.Slice;
            }

            P2 pLast = new P2 {P = lower, Q = input.To};
            result.Add(pLast);
            return result.ToArray();
        }
        protected override P9 OnParCombine(P8[] outputs)
        {
            P9 result = new P9 {Total = 0};
            foreach (var subOutput in outputs)
            {
                result.Total += subOutput.Subtotal;
            }
            return result;
        }
        public ParallelStep()
        {
            AddSubstep(new ParallelSubstep());
        }
    }
    public struct LoopInput
    {
        public int Begin;
        public int End;
    }
    public struct LoopData
    {
        public int Index;
        public int End;
        public int Total;
    }
    public struct LoopResult
    {
        public int Total;
    }
    public class LoopingSubstep : WorkstepBase<LoopData, LoopData>
    {
        public LoopingSubstep() { }
        protected override LoopData OnExecute(LoopData input)
        {
            input.Total += input.Index;
            //Random rg = new Random();
            //Thread.Sleep(rg.Next(2000));
            return input;
        }
    }
    public class LoopingStep : WorkstepBaseLoop<LoopInput, LoopData, LoopResult>
    {
        public LoopingStep()
        {
            AddSubstep(new LoopingSubstep());
        }
        protected override LoopData OnLoopInit(LoopInput input)
        {
            LoopData result;
            result.Index = input.Begin;
            result.End = input.End;
            result.Total = 0;
            return result;
        }
        protected override bool OnCondition(LoopData loopData)
        {
            return (loopData.Index <= loopData.End);
        }
        protected override void OnLoopNext(ref LoopData loopData)
        {
            loopData.Index++;
        }
        protected override LoopResult OnLoopDone(LoopData loopData)
        {
            LoopResult result;
            result.Total = loopData.Total;
            return result;
        }
    }
}