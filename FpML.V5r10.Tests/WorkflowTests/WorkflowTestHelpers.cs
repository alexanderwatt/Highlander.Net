using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Orion.Workflow;

namespace Orion.Workflow.Tests
{
    public struct T1 { public int a;}
    public struct T2 { public int b;}
    public struct T3 { public int c;}
    public class SequentialSubstep1 : WorkstepBase<T1, T2>
    {
        public SequentialSubstep1() { }
        protected override T2 OnExecute(T1 input)
        {
            T2 result;
            result.b = input.a;
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
            result.c = input.b;
            Thread.Sleep(100);
            return result;
        }
    }
    public class BranchSubstep : WorkstepBase<T1, T2>
    {
        private int _Op;
        public BranchSubstep(int op) { _Op = op; }
        protected override T2 OnExecute(T1 input)
        {
            T2 result;
            result.b = input.a * _Op;
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
            return input.a;
        }
        public BranchingStep(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddSubstep(new BranchSubstep(i));
            }
        }
    }
    public struct P1 { public int from; public int to; public int slice;}
    public struct P2 { public int p; public int q; }
    public struct P8 { public Int64 subtotal; }
    public struct P9 { public Int64 total; }
    public class ParallelSubstep : WorkstepBase<P2, P8>
    {
        public ParallelSubstep() { }
        protected override P8 OnExecute(P2 input)
        {
            P8 result;
            result.subtotal = 0;
            for (int i = input.p; i <= input.q; i++)
            {
                result.subtotal += i;
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
            int lower = input.from;
            int upper = input.from + input.slice - 1;
            while (upper < input.to)
            {
                P2 p2 = new P2();
                p2.p = lower;
                p2.q = upper;
                result.Add(p2);
                lower = upper + 1;
                upper += input.slice;
            }
            P2 pLast = new P2();
            pLast.p = lower;
            pLast.q = input.to;
            result.Add(pLast);
            return result.ToArray();
        }
        protected override P9 OnParCombine(P8[] outputs)
        {
            P9 result = new P9();
            result.total = 0;
            foreach (var subOutput in outputs)
            {
                result.total += subOutput.subtotal;
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
        public int begin;
        public int end;
    }
    public struct LoopData
    {
        public int index;
        public int end;
        public int total;
    }
    public struct LoopResult
    {
        public int total;
    }
    public class LoopingSubstep : WorkstepBase<LoopData, LoopData>
    {
        public LoopingSubstep() { }
        protected override LoopData OnExecute(LoopData input)
        {
            input.total += input.index;
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
            result.index = input.begin;
            result.end = input.end;
            result.total = 0;
            return result;
        }
        protected override bool OnCondition(LoopData loopData)
        {
            return (loopData.index <= loopData.end);
        }
        protected override void OnLoopNext(ref LoopData loopData)
        {
            loopData.index++;
        }
        protected override LoopResult OnLoopDone(LoopData loopData)
        {
            LoopResult result;
            result.total = loopData.total;
            return result;
        }
    }
}