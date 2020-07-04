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
using Highlander.Utilities.Expressions;
using Highlander.Utilities.NamedValues;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V1.Tests
{
    /// <summary>
    /// Summary description for ExpressionTests
    /// </summary>
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public void TestSerialisationFailures()
        {
            var props = new NamedValueSet();
            props.Set("prop1", 123);
            props.Set("prop2", 456);
            IExpression expr1 = Expr.BoolAND(props);
            Assert.AreEqual<bool>(false, expr1.HasErrors());
            string text1 = expr1.Serialise();
            Assert.AreEqual<string>(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<QuerySpec xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://tempuri.org/ExprFormat.xsd\">\r\n" +
                "  <version>1</version>\r\n" +
                "  <v1QueryExpr node=\"EXPR\" name=\"AND\">\r\n" +
                "    <args node=\"EXPR\" name=\"EQU\">\r\n" +
                "      <args node=\"FIELD\" name=\"prop1\" />\r\n" +
                "      <args node=\"CONST\" name=\"Int32\" value=\"123\" />\r\n" +
                "    </args>\r\n" +
                "    <args node=\"EXPR\" name=\"EQU\">\r\n" +
                "      <args node=\"FIELD\" name=\"prop2\" />\r\n" +
                "      <args node=\"CONST\" name=\"Int32\" value=\"456\" />\r\n" +
                "    </args>\r\n" +
                "  </v1QueryExpr>\r\n" +
                "</QuerySpec>", text1);
            string text2a =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<QuerySpec xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://tempuri.org/ExprFormat.xsd\">\r\n" +
                "  <version>1</version>\r\n" +
                "  <v1QueryExpr node=\"EXPR\" name=\"AND\">\r\n" +
                "    <args node=\"EXPR\" name=\"EQU\">\r\n" +
                "      <args node=\"FIELD\" name=\"prop1\" />\r\n" +
                "      <args node=\"CONSTqq\" name=\"Int32\" value=\"123\" />\r\n" +
                "    </args>\r\n" +
                "    <args node=\"EXPR\" name=\"EQU\">\r\n" +
                "      <args node=\"FIELD\" name=\"prop2\" />\r\n" +
                "      <args node=\"CONST\" name=\"Int32\" value=\"456\" />\r\n" +
                "    </args>\r\n" +
                "  </v1QueryExpr>\r\n" +
                "</QuerySpec>";
            // deserialisation should succeed, but with errors
            IExpression expr2 = Expr.Create(text2a);
            Assert.AreEqual<bool>(true, expr2.HasErrors());

            // evaluation should fail
            UnitTestHelper.AssertThrows<InvalidOperationException>(
                                        () => expr2.Evaluate(props));

        }

        [TestMethod]
        public void TestNamedValueSetConstructors()
        {
            NamedValueSet props = new NamedValueSet();
            props.Set("prop1", 123);
            props.Set("prop2", 234);
            int[] choiceSet = new int[] { 456, 567, 678 };
            props.Set("prop3", choiceSet);
            IExpression expr1 = Expr.BoolAND(props);
            Assert.AreEqual<bool>(false, expr1.HasErrors());
            string text1 = expr1.Serialise();
            string disp1 = expr1.DisplayString();
        }

        [TestMethod]
        public void TestDateFunctions()
        {
            DateTimeOffset dtoNow = DateTimeOffset.Now;
            DateTime dtNow = dtoNow.DateTime;
            DateTime dtToday = dtNow.Date;
            NamedValueSet props = new NamedValueSet();
            props.Set("dtoNow", dtoNow);
            props.Set("dtNow", dtNow);
            props.Set("dtToday", dtToday);
            // check DayOfWeek()
            {
                IExpression expr = Expr.Create(Expr.BoolAND(
                                                   Expr.IsGEQ(Expr.DayOfWeek(Expr.Prop("dtoNow")), Expr.Const(DayOfWeek.Sunday)),
                                                   Expr.IsLEQ(Expr.DayOfWeek(Expr.Prop("dtoNow")), Expr.Const(DayOfWeek.Saturday))
                                                   ).Serialise());

                string disp = expr.DisplayString();
                object result = expr.Evaluate(props);
                Assert.IsNotNull(result);
                Assert.AreEqual<Type>(typeof(bool), result.GetType());
                Assert.AreEqual<bool>(true, (bool)result);
            }

            // check FuncTODAY
            {
                IExpression expr = Expr.IsEQU(Expr.Prop("dtToday"), Expr.FuncToday());
                object result = expr.Evaluate(props);
                Assert.IsNotNull(result);
                Assert.AreEqual<Type>(typeof(bool), result.GetType());
                Assert.AreEqual<bool>(true, (bool)result);
            }

            // test DateTimeOffset/DateTime comparisons
            {
                IExpression expr = Expr.IsEQU(Expr.Prop("dtNow"), Expr.Prop("dtoNow"));
                object result = expr.Evaluate(props);
                Assert.IsNotNull(result);
                Assert.AreEqual<Type>(typeof(bool), result.GetType());
                Assert.AreEqual<bool>(true, (bool)result);
            }
            {
                IExpression expr = Expr.IsLSS(Expr.Prop("dtToday"), Expr.Prop("dtNow"));
                object result = expr.Evaluate(props);
                Assert.IsNotNull(result);
                Assert.AreEqual<Type>(typeof(bool), result.GetType());
                Assert.AreEqual<bool>(true, (bool)result);
            }
            {
                IExpression expr = Expr.IsLSS(Expr.Prop("dtToday"), Expr.Prop("dtoNow"));
                object result = expr.Evaluate(props);
                Assert.IsNotNull(result);
                Assert.AreEqual<Type>(typeof(bool), result.GetType());
                Assert.AreEqual<bool>(true, (bool)result);
            }
        }

        [TestMethod]
        public void IsNotNullTest()
        {
            // Set up conditions that Name must exist and be not empty
            IExpression expression = Expr.IsNEQ("Name", "");
            Assert.IsNotNull(expression);
            IExpression conditions = expression;
            expression = Expr.IsNotNull("Name");
            Assert.IsNotNull(expression);
            conditions = Expr.BoolAND(expression, conditions);
            Assert.IsNotNull(conditions);
            // Test that if there is Name that it passes
            NamedValueSet props = new NamedValueSet();
            props.Set("Name", "Test");
            object result = conditions.Evaluate(props);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);
            // And if there isn't it fails
            props = new NamedValueSet();
            props.Set("NoName", "Test");
            result = conditions.Evaluate(props);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsFalse((bool)result);
            // Test serialization
            string serialized = conditions.Serialise();
            Assert.IsTrue(!string.IsNullOrEmpty(serialized));
            // And deserialization
            IExpression deserializedConditions = Expr.Deserialise(serialized);
            // Test that if there is Name that it passes
            props = new NamedValueSet();
            props.Set("Name", "Test");
            result = deserializedConditions.Evaluate(props);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);
            // And if there isn't it fails
            props = new NamedValueSet();
            props.Set("NoName", "Test");
            result = deserializedConditions.Evaluate(props);
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsFalse((bool)result);
        }
    }
}