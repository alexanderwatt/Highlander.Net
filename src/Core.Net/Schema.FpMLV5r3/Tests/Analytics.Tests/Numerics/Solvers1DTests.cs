/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using Highlander.Reporting.Analytics.V5r3.Solvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Numerics
{
    /// <summary>
    /// </summary>
    [TestClass]
    public class SolverIDTests
    {

        // declare private members here used for test setup
        private readonly ISolver1D[] _solvers = { 
                                                    new Brent(), 
                                                    new Bisection(), 
                                                    new Newton(),
                                                    new NewtonSafe()
                                                };

        private readonly double[] _accuracies = { 1e-4, 1e-6, 1e-8 };

        private class TestFunc : ObjectiveFunction
        {
            public override double Value(double x)
            {
                return x * x - 1.0;
            }

            public override double Derivative(double x)
            {
                return 2.0 * x;
            }
        }

        [TestMethod]
        public void TestSolvers()
        {
            IObjectiveFunction objFun = new TestFunc();
            foreach (ISolver1D solver in _solvers)
            {
                foreach (double accuracy in _accuracies)
                {
                    string testMsg = $"{solver}, accuracy={accuracy:e1}";
                    double root = solver.Solve(objFun, accuracy, 1.09, 0.1);
                    Assert.AreEqual(root, 1.0, accuracy, testMsg);
                    root = solver./*Bracketed*/Solve(
                        objFun, accuracy, 0.9, 0.0, 1.0);
                    Assert.AreEqual(root, 1.0, accuracy, testMsg + " (bracketed)");
                }
            }
        }

        [TestMethod]
        public void BrentMultipleRoots()
        {
            // Roots at -2, 2
            double F1(double x) => x * x - 4;
            Assert.AreEqual(0, F1(Brent.FindRoot(F1, 1, 5, 1e-14)));
            Assert.AreEqual(0, F1(Brent.FindRootExpand(F1, 3, 5, 1e-14)));
            Assert.AreEqual(-2, Brent.FindRoot(F1, -5, -1, 1e-14));
            Assert.AreEqual(2, Brent.FindRoot(F1, 1, 4, 1e-14));
            Assert.AreEqual(0, F1(Brent.FindRoot(x => -F1(x), 1, 5, 1e-14)));
            Assert.AreEqual(-2, Brent.FindRoot(x => -F1(x), -5, -1, 1e-14));
            Assert.AreEqual(2, Brent.FindRoot(x => -F1(x), 1, 4, 1e-14));
            // Roots at 3, 4
            double F2(double x) => (x - 3) * (x - 4);
            Assert.AreEqual(0, F2(Brent.FindRoot(F2, -5, 3.5, 1e-14)));
            Assert.AreEqual(3, Brent.FindRoot(F2, -5, 3.5, 1e-14));
            Assert.AreEqual(4, Brent.FindRoot(F2, 3.2, 5, 1e-14));
            Assert.AreEqual(3, Brent.FindRoot(F2, 2.1, 3.9, 0.001, 50), 0.001);
            Assert.AreEqual(3, Brent.FindRoot(F2, 2.1, 3.4, 0.001, 50), 0.001);
        }
    }
}