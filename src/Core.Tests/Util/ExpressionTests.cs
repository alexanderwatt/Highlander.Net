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

using System.Threading;
using Highlander.Utilities.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests
{
    /// <summary>
    /// Summary description for ExpressionTests
    /// </summary>
    [TestClass]
    public class ExpressionTests
    {
        public ExpressionTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestExpressionEquality()
        {
            // expression equality tests
            // constant expressions
            {
                IExpression expr1 = Expr.Const(false);
                IExpression expr2 = Expr.Const(false);
                Assert.IsTrue(expr1.Equals(expr2));
            }
            {
                IExpression expr1 = Expr.Const(false);
                IExpression expr2 = Expr.Const(true);
                Assert.IsFalse(expr1.Equals(expr2));
            }
            {
                IExpression expr1 = Expr.Const("Value1");
                IExpression expr2 = Expr.Const("Value1");
                Assert.IsTrue(expr1.Equals(expr2));
            }
            {
                IExpression expr1 = Expr.Const("Value1");
                IExpression expr2 = Expr.Const("value1");
                Assert.IsFalse(expr1.Equals(expr2));
            }
            {
                IExpression expr1 = Expr.Const("Value1");
                IExpression expr2 = Expr.Const("Value2");
                Assert.IsFalse(expr1.Equals(expr2));
            }
            // incompatibale types
            {
                IExpression expr1 = Expr.Const("Value1");
                IExpression expr2 = Expr.Const(true);
                Assert.IsFalse(expr1.Equals(expr2));
            }
            // property names are case insensitive
            {
                IExpression expr1 = Expr.IsEQU("Name", "Value");
                IExpression expr2 = Expr.IsEQU("Name", "Value");
                Assert.IsTrue(expr1.Equals(expr2));
            }
            {
                IExpression expr1 = Expr.IsEQU("Name", "Value");
                IExpression expr2 = Expr.IsEQU("name", "Value");
                Assert.IsTrue(expr1.Equals(expr2));
            }
            // order of evaluation is irrelevant
            {
                IExpression expr1 = Expr.BoolAND(Expr.IsEQU("Name1", "Value1"), Expr.IsEQU("Name2", "Value2"));
                IExpression expr2 = Expr.BoolAND(Expr.IsEQU("Name2", "Value2"), Expr.IsEQU("Name1", "Value1"));
                Assert.IsTrue(expr1.Equals(expr2));
            }
            {
                IExpression expr1 = Expr.BoolOR(Expr.IsEQU("Name1", "Value1"), Expr.IsEQU("Name2", "Value2"));
                IExpression expr2 = Expr.BoolOR(Expr.IsEQU("Name2", "Value2"), Expr.IsEQU("Name1", "Value1"));
                Assert.IsTrue(expr1.Equals(expr2));
            }
            // operator is important
            {
                IExpression expr1 = Expr.BoolOR(Expr.IsEQU("Name1", "Value1"), Expr.IsEQU("Name2", "Value2"));
                IExpression expr2 = Expr.BoolAND(Expr.IsEQU("Name1", "Value1"), Expr.IsEQU("Name2", "Value2"));
                Assert.IsFalse(expr1.Equals(expr2));
            }
            // non-deterministic expressions are never equal
            {
                IExpression expr1 = Expr.FuncNow();
                IExpression expr2 = Expr.FuncNow();
                int hash1a = expr1.GetHashCode();
                int hash2a = expr2.GetHashCode();
                // these are usually equal but not always
                //Assert.AreEqual<int>(hash1a, hash2a);
                Thread.Sleep(30); // sleep at least 2 Windows time quanta (not ticks)
                int hash1b = expr1.GetHashCode();
                int hash2b = expr2.GetHashCode();
                Assert.AreNotEqual<int>(hash1a, hash1b);
                Assert.AreNotEqual<int>(hash1a, hash2b);
            }
        }

        [TestMethod]
        public void TestMultipleArgumentBooleanExpressions()
        {
            // expression tests
            // - empty multi-AND = true
            {
                IExpression[] args = { };
                Assert.IsTrue(Expr.CastTo<bool>(Expr.BoolAND(args).Evaluate(null), false));
            }
            // - multi-AND true
            {
                IExpression[] args = { Expr.Const(true), null, null, Expr.Const(true) };
                Assert.IsTrue(Expr.CastTo<bool>(Expr.BoolAND(args).Evaluate(null), false));
            }
            // multi-AND false
            {
                IExpression[] args = { Expr.Const(true), null, null, Expr.Const(false) };
                Assert.IsFalse(Expr.CastTo<bool>(Expr.BoolAND(args).Evaluate(null), false));
            }
            // - empty multi-OR = false
            {
                IExpression[] args = { };
                Assert.IsFalse(Expr.CastTo<bool>(Expr.BoolOR(args).Evaluate(null), false));
            }
            // - multi-OR false
            {
                IExpression[] args = { Expr.Const(false), null, null, Expr.Const(false) };
                Assert.IsFalse(Expr.CastTo<bool>(Expr.BoolOR(args).Evaluate(null), false));
            }
            // multi-OR true
            {
                IExpression[] args = { Expr.Const(false), null, null, Expr.Const(true) };
                Assert.IsTrue(Expr.CastTo<bool>(Expr.BoolOR(args).Evaluate(null), false));
            }
        }
    }
}
