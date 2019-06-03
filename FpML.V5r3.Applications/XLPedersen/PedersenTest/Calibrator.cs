using System;
using Extreme.Mathematics;
using Extreme.Mathematics.Optimization;
using Extreme.Mathematics.LinearAlgebra;

namespace PedersenHost
{

    class Calibrator
    {
        #region declarations
        private readonly MultivariateRealFunction _f;
        private readonly FastMultivariateVectorFunction _g;
        private GeneralVector _initialGuess;
        private readonly ObjectiveFunction _objective;
        private readonly Parameters _p;
        public int Iter { get; set; }

        #endregion

        public Calibrator(ObjectiveFunction obj, Parameters par)
        {
            _objective = obj;
            _f = _objective.ObjFun;
            _g = _objective.ObjGrad;
            _p = par;
        }

        public void Go()
        {
            _initialGuess = new GeneralVector(_p.NEXPIRY * _p.Ntenor);
            double iniguess = _p.AvgSwpnIvol;
            if (iniguess == 0)
            {
                iniguess = _p.AvgCpltIvol;
            }
            if (iniguess == 0)
            {
                throw new Exception("The selected time range does not include any calibration targets.");
            }
            if (_objective.ExpForm)
            {
                _initialGuess.SetValue(Math.Log(iniguess) / 20);
            }
            else
            {
                _initialGuess.SetValue(iniguess);
            }
            var bfgs =
                new QuasiNewtonOptimizer(QuasiNewtonMethod.Bfgs);
            Optimize(bfgs);
            //ConjugateGradientOptimizer cg =
            //    new ConjugateGradientOptimizer(ConjugateGradientMethod.PositivePolakRibiere);
            //Optimize(cg);
            //PowellOptimizer pw = new PowellOptimizer();
            //Optimize(pw);
        }

        private void Optimize(MultidimensionalOptimizer opti)
        {
            var sw = new StopWatch();
            opti.InitialGuess = _initialGuess;
            opti.ExtremumType = ExtremumType.Minimum;
            //opti.ConvergenceCriterion = ConvergenceCriterion.WithinAnyTolerance;
            //opti.AbsoluteTolerance = 0.1;
            //opti.RelativeTolerance = 0.1;
            opti.MaxIterations = Iter;
            opti.MaxEvaluations = (int)(2 * opti.MaxIterations * 1.2);
            // Set the ObjectiveFunction:
            opti.ObjectiveFunction = _f;
            // Set either the GradientFunction or FastGradientFunction:
            opti.FastGradientFunction = _g;
            // The FindExtremum method does all the hard work:
            sw.Start();
            _objective.StartOtherThreads();
            opti.FindExtremum();
            _objective.Finished = true;
            sw.Stop();
            TimeSpan ts = sw.GetElapsedTime();
            Program.Write(String.Format("  Value at extremum: {0}\n", opti.ValueAtExtremum), "cal");
            _objective.OutputResult(opti.Extremum);
            Program.Write(String.Format("  Process Time: {0:d2}:{1:d2}:{2:d2}:{3:d3}\n", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds), "cal");
        }
    }
}
