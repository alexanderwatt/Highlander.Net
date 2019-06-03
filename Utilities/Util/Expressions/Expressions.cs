/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;

#endregion

namespace Orion.Util.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IExprContext
    {
        NamedValueSet DataContext { get; }
        string ItemName { get; }
        DateTimeOffset ItemCreated { get; }
        DateTimeOffset ItemExpires { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ExprContext : IExprContext
    {
        public NamedValueSet DataContext { get; }
        public string ItemName { get; }
        public DateTimeOffset ItemCreated { get; }
        public DateTimeOffset ItemExpires { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="itemName"></param>
        /// <param name="itemCreated"></param>
        /// <param name="itemExpires"></param>
        public ExprContext(NamedValueSet dataContext, string itemName, DateTimeOffset itemCreated, DateTimeOffset itemExpires)
        {
            DataContext = dataContext;
            ItemName = itemName;
            ItemCreated = itemCreated;
            ItemExpires = itemExpires;
        }
    }

    internal enum QueryNodeType
    {
        // note: never change these ordinal names or values after release to test/prod
        // - they are externally transmitted and stored by name
        // - extend by adding new values only
        _NONE,
        CONST,
        FIELD,
        EXPR,
        ERROR
    }

    internal enum QueryOpCode
    {
        // note: never change these ordinal names or values after release to test/prod
        // - they are externally transmitted and stored by name
        // - extend by adding new values only
        _NONE,
        // logical
        NOT,        // !x
        OR,         // x || y
        AND,        // x && y
        XOR,        // x ^ y
        IFF,        // (x ? y : z)
        ISNULL,     // (x == null)
        ISNOTNULL,  // (x != null)
        IFNULL,     // (x ?? y)
        // relational
        EQU,    // x == y
        NEQ,    // x != y
        GTR,    // x >  y
        GEQ,    // x >= y
        LSS,    // x <  y
        LEQ,    // x <= y
        EQZ,    // x == 0
        NEZ,    // x != 0
        GTZ,    // x >  0
        GEZ,    // x >= 0
        LTZ,    // x <  0
        LEZ,    // x <= 0
        // numeric
        NEG,    // negate
        ABS,
        ADD,    // x + y
        SUB,    // x - y
        MUL,    // x * y
        DIV,    // x / y 
        MOD,    // x mod y
        IDIV,   // x div y (integer division)
        // bitwise
        SHL,    // x << y
        SHR,    // x >> y
        BOR,    // |
        BAND,   // &
        BNOT,   // ^
        // string
        STARTS,     // x.StartsWith(y)
        ENDS,       // x.EndsWith(y)
        CONTAINS,   // x.Contains(y)
        LOWER,      // x.ToLowerInvariant()
        UPPER,      // x.ToUpperInvariant()
        // functions
        NOW,        // current date/time [DateTimeOffset]
        DATE,       // DateTimeOffset.Now.Date [DateTimeOffset]
        TIME,       // DateTimeOffset.Now.TimeOfDay [TimeSpan]
        DATEPART,   // x.Date [DateTimeOffset]
        TIMEPART,   // x.TimeOfDay [TimeSpan]
        DOW,        // x.DayOfWeek() [DayOfWeek]
        // generic compare
        COMP,       // Compare(x,y)
        // end of list
        ZZZ
    }

    /// <summary>
    /// A class for defining, communicating and evaluating expressions
    /// </summary>
    public class Expr : IExpression, IEquatable<Expr>
    {
        #region Constants
        public const string SysPropItemName = "$ItemName";
        public const string SysPropItemCreated = "$ItemCreated";
        public const string SysPropItemExpires = "$ItemExpires";

        #endregion

        #region Private state
        private readonly QueryNodeType _nodeType;
        private readonly string _propName;
        private readonly object _constValue;
        private readonly QueryOpCode _operator;
        private readonly IExpression[] _operands;
        private object[] _results;
        #endregion

        #region Static Constructors

        /// <summary>
        /// Constructs a query expression from a Version 1.x QuerySpec
        /// </summary>
        /// <param name="queryXmlStr">The serialised QuerySpec (XML).</param>
        /// <returns></returns>
        public static IExpression Create(string queryXmlStr)
        {
            QuerySpec querySpec = XmlSerializerHelper.DeserializeFromString<QuerySpec>(queryXmlStr);
            if (querySpec == null)
                throw new ArgumentNullException("querySpec");
            if (querySpec.version != 1)
                throw new NotSupportedException("QuerySpec version: " + querySpec.version);
            if (querySpec.v1QueryExpr == null)
                throw new ArgumentNullException("querySpec.v1QueryExpr");
            return new Expr(querySpec.v1QueryExpr);
        }

        /// <summary>
        /// Constructs an expression that evaluates to a constant.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static IExpression Const(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            // create a constant query node
            return new Expr(QueryNodeType.CONST, null, value);
        }

        /// <summary>
        /// Constructs an expression that contains an exception.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static IExpression Error(Exception value)
        {
            // create a constant query node
            return new Expr(QueryNodeType.ERROR, null, value);
        }

        /// <summary>
        /// Constructs an expression that refers to a named value in the evaluation data context.
        /// </summary>
        /// <param name="propName">Name of the prop.</param>
        /// <returns></returns>
        public static IExpression Prop(string propName)
        {
            // create a property reference query node
            return new Expr(QueryNodeType.FIELD, propName, null);
        }

        /// <summary>
        /// Builds an EQU (equal to) expression from 2 same type arguments.
        /// When evaluated, the expression returns: (arg1 == arg2)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression IsEQU(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.EQU, arg1, arg2);
        }

        public static IExpression IsEQU(string propName, object value)
        {
            return new Expr(QueryOpCode.EQU, Expr.Prop(propName), Expr.Const(value));
        }

        /// <summary>
        /// Builds a NEQ (not equal to) expression from 2 same type arguments.
        /// When evaluated, the expression returns: (arg1 != arg2)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression IsNEQ(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.NEQ, arg1, arg2);
        }

        public static IExpression IsNEQ(string propName, object value)
        {
            return new Expr(QueryOpCode.NEQ, Expr.Prop(propName), Expr.Const(value));
        }

        /// <summary>
        /// Builds a GEQ (greater than or equal to) expression from 2 same type arguments.
        /// When evaluated, the expression returns: (arg1 == arg2)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression IsGEQ(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.GEQ, arg1, arg2);
        }

        public static IExpression IsGEQ(string propName, object value)
        {
            return new Expr(QueryOpCode.GEQ, Expr.Prop(propName), Expr.Const(value));
        }

        /// <summary>
        /// Builds a GTR (greater than) expression from 2 same type arguments.
        /// When evaluated, the expression returns: (arg1 == arg2)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression IsGTR(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.GTR, arg1, arg2);
        }

        public static IExpression IsGTR(string propName, object value)
        {
            return new Expr(QueryOpCode.GTR, Expr.Prop(propName), Expr.Const(value));
        }

        /// <summary>
        /// Builds a LEQ (less than or equal to) expression from 2 same type arguments.
        /// When evaluated, the expression returns: (arg1 == arg2)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression IsLEQ(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.LEQ, arg1, arg2);
        }

        public static IExpression IsLEQ(string propName, object value)
        {
            return new Expr(QueryOpCode.LEQ, Expr.Prop(propName), Expr.Const(value));
        }

        /// <summary>
        /// Builds a comparison expression from 2 same type arguments.
        /// When evaluated, the expression returns: 0 when arg1 == arg2, -1 when arg1 is less than arg2, or +1 when arg1 is greeater than arg2.
        /// </summary>
        /// <param name="arg1">An expression</param>
        /// <param name="arg2">An expression</param>
        /// <returns></returns>
        public static IExpression Compare(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.COMP, arg1, arg2);
        }

        /// <summary>
        /// Builds a LSS (less than) expression from 2 same type arguments.
        /// When evaluated, the expression returns: (arg1 == arg2)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression IsLSS(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.LSS, arg1, arg2);
        }

        public static IExpression IsLSS(string propName, object value)
        {
            return new Expr(QueryOpCode.LSS, Expr.Prop(propName), Expr.Const(value));
        }

        /// <summary>
        /// Builds a NOW (get current date/time) expression.
        /// </summary>
        /// <returns>An expression</returns>
        public static IExpression FuncNow()
        {
            return new Expr(QueryOpCode.NOW);
        }

        public static IExpression FuncTimeOfDay()
        {
            return new Expr(QueryOpCode.TIME);
        }

        public static IExpression FuncTimePart(IExpression arg1)
        {
            return new Expr(QueryOpCode.TIMEPART, arg1);
        }

        /// <summary>
        /// Builds a TODAY (get current date) expression.
        /// </summary>
        /// <returns>An expression</returns>
        public static IExpression FuncToday()
        {
            return new Expr(QueryOpCode.DATE);
        }

        public static IExpression FuncDatePart(IExpression arg1)
        {
            return new Expr(QueryOpCode.DATEPART, arg1);
        }

        /// <summary>
        /// Builds an AND expression from 2 boolean arguments.
        /// When evaluated, the expression returns: (arg1 && arg2)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression BoolAND(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.AND, arg1, arg2);
        }

        /// <summary>
        /// Builds an AND expression from 3 boolean arguments.
        /// When evaluated, the expression returns: (arg1 && arg2 && arg3)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <param name="arg3">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression BoolAND(IExpression arg1, IExpression arg2, IExpression arg3)
        {
            return new Expr(QueryOpCode.AND, arg1, arg2, arg3);
        }

        /// <summary>
        /// Builds an AND expression from 4 boolean arguments.
        /// When evaluated, the expression returns: (arg1 && arg2 && arg3 && arg4)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <param name="arg3">A boolean expression</param>
        /// <param name="arg4">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression BoolAND(IExpression arg1, IExpression arg2, IExpression arg3, IExpression arg4)
        {
            return new Expr(QueryOpCode.AND, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Builds an AND expression from N boolean arguments.
        /// When evaluated, the expression returns: (arg1 && arg2 && ... argN)
        /// </summary>
        /// <param name="args">The boolean arguments.</param>
        /// <returns>An expression</returns>
        public static IExpression BoolAND(params IExpression[] args)
        {
            return new Expr(QueryOpCode.AND, args);
        }

        /// <summary>
        /// Builds an AND expression from N boolean arguments.
        /// When evaluated, the expression returns: (arg1 && arg2 && ... argN)
        /// </summary>
        /// <param name="args">The boolean arguments.</param>
        /// <returns>An expression</returns>
        public static IExpression BoolAND(IEnumerable<IExpression> args)
        {
            return new Expr(QueryOpCode.AND, args);
        }

        /// <summary>
        /// Builds an AND-joined expression from a set of named values.
        /// When evaluated, the expression returns: ((prop1 == value1) && (prop2 == value2) && ...)
        /// </summary>
        /// <param name="propSet">The property set.</param>
        /// <returns>An expression</returns>
        public static IExpression BoolAND(NamedValueSet propSet)
        {
            // create a multi-arg AND expression
            return new Expr(QueryOpCode.AND, propSet);
        }

        /// <summary>
        /// Builds an OR expression from 2 boolean arguments.
        /// When evaluated, the expression returns: (arg1 || arg2)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression BoolOR(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.OR, arg1, arg2);
        }

        /// <summary>
        /// Builds an OR expression from 3 boolean arguments.
        /// When evaluated, the expression returns: (arg1 || arg2 || arg3)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <param name="arg3">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression BoolOR(IExpression arg1, IExpression arg2, IExpression arg3)
        {
            return new Expr(QueryOpCode.OR, arg1, arg2, arg3);
        }

        /// <summary>
        /// Builds an OR expression from 4 boolean arguments.
        /// When evaluated, the expression returns: (arg1 || arg2 || arg3 || arg4)
        /// </summary>
        /// <param name="arg1">A boolean expression</param>
        /// <param name="arg2">A boolean expression</param>
        /// <param name="arg3">A boolean expression</param>
        /// <param name="arg4">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression BoolOR(IExpression arg1, IExpression arg2, IExpression arg3, IExpression arg4)
        {
            return new Expr(QueryOpCode.OR, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// Builds an OR expression from N boolean arguments.
        /// When evaluated, the expression returns: (arg1 OR arg2 OR ... argN)
        /// </summary>
        /// <param name="args">The boolean arguments.</param>
        /// <returns>An expression</returns>
        public static IExpression BoolOR(params IExpression[] args)
        {
            return new Expr(QueryOpCode.OR, args);
        }

        /// <summary>
        /// Builds an OR expression from N boolean arguments.
        /// When evaluated, the expression returns: (arg1 OR arg2 OR ... argN)
        /// </summary>
        /// <param name="args">The boolean arguments.</param>
        /// <returns>An expression</returns>
        public static IExpression BoolOR(IEnumerable<IExpression> args)
        {
            return new Expr(QueryOpCode.OR, args);
        }

        /// <summary>
        /// Builds an OR-joined expression from a set of named values.
        /// When evaluated, the expression returns: ((prop1 == value1) || (prop2 == value2) || ...)
        /// </summary>
        /// <param name="propSet">The prop set.</param>
        /// <returns>An expression</returns>
        public static IExpression BoolOR(NamedValueSet propSet)
        {
            return new Expr(QueryOpCode.OR, propSet);
        }

        /// <summary>
        /// Builds a NOT expression from 1 boolean argument.
        /// When evaluated, the expression returns: (!arg1)
        /// </summary>
        /// <param name="arg">A boolean expression</param>
        /// <returns>An expression</returns>
        public static IExpression BoolNOT(IExpression arg)
        {
            // create a unary NOT expression
            return new Expr(QueryOpCode.NOT, arg);
        }

        /// <summary>
        /// Builds an ISNULL expression from 1 argument.
        /// When evaluated, the expression returns: (arg1 == null)
        /// </summary>
        /// <param name="arg1">An expression</param>
        /// <returns>An expression</returns>
        public static IExpression IsNull(IExpression arg1)
        {
            return new Expr(QueryOpCode.ISNULL, arg1);
        }
        /// <summary>
        /// Builds an ISNULL expression using a property name.
        /// When evaluated, the expression returns: (Get(propName) == null)
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        /// <returns>An expression</returns>
        public static IExpression IsNull(string propName)
        {
            return new Expr(QueryOpCode.ISNULL, Prop(propName));
        }

        /// <summary>
        /// Builds an ISNOTNULL expression from 1 argument.
        /// When evaluated, the expression returns: (arg1 != null)
        /// </summary>
        /// <param name="arg1">An expression</param>
        /// <returns>An expression</returns>
        public static IExpression IsNotNull(IExpression arg1)
        {
            return new Expr(QueryOpCode.ISNOTNULL, arg1);
        }
        /// <summary>
        /// Builds an ISNOTNULL expression using a property name.
        /// When evaluated, the expression returns: (Get(propName) != null)
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        /// <returns>An expression</returns>
        public static IExpression IsNotNull(string propName)
        {
            return new Expr(QueryOpCode.ISNOTNULL, Prop(propName));
        }

        /// <summary>
        /// Builds a StartsWith expression from 2 string arguments.
        /// When evaluated, the expression returns: (String)arg1.StartsWith((String)arg2)
        /// </summary>
        /// <param name="arg1">A string expression</param>
        /// <param name="arg2">An expression string expression</param>
        /// <returns>An expression</returns>
        public static IExpression StartsWith(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.STARTS, arg1, arg2);
        }
        /// <summary>
        /// Builds a StartsWith expression from a property name and a string search value.
        /// When evaluated, the expression returns: ((String)Get(propName)).StartsWith(value)
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        /// <param name="value">The search string.</param>
        /// <returns>An expression</returns>
        public static IExpression StartsWith(string propName, string value)
        {
            return new Expr(QueryOpCode.STARTS, Prop(propName), Const(value));
        }

        /// <summary>
        /// Builds a EndsWith expression from 2 string arguments.
        /// When evaluated, the expression returns: (String)arg1.EndsWith((String)arg2)
        /// </summary>
        /// <param name="arg1">A string expression</param>
        /// <param name="arg2">An expression string expression</param>
        /// <returns>An expression</returns>
        public static IExpression EndsWith(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.ENDS, arg1, arg2);
        }
        /// <summary>
        /// Builds a EndsWith expression from a property name and a string search value.
        /// When evaluated, the expression returns: ((String)Get(propName)).EndsWith(value)
        /// </summary>
        /// <param name="propName">Name of the propery.</param>
        /// <param name="value">The search string.</param>
        /// <returns>An expression</returns>
        public static IExpression EndsWith(string propName, string value)
        {
            return new Expr(QueryOpCode.ENDS, Prop(propName), Const(value));
        }

        /// <summary>
        /// Builds a Contains expression from 2 string arguments.
        /// When evaluated, the expression returns: (String)arg1.Constains((String)arg2)
        /// </summary>
        /// <param name="arg1">A string expression</param>
        /// <param name="arg2">An expression string expression</param>
        /// <returns>An expression</returns>
        public static IExpression Contains(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.CONTAINS, arg1, arg2);
        }
        /// <summary>
        /// Builds a Contains expression from a property name and a string search value.
        /// When evaluated, the expression returns: ((String)Get(propName)).Contains(value)
        /// </summary>
        /// <param name="propName">Name of the propery.</param>
        /// <param name="value">The search string.</param>
        /// <returns>An expression</returns>
        public static IExpression Contains(string propName, string value)
        {
            return new Expr(QueryOpCode.CONTAINS, Prop(propName), Const(value));
        }

        /// <summary>
        /// Builds a LOWER expression from 1 string argument.
        /// When evaluated, the expression returns: (String)arg1.ToLowerInvariant()
        /// </summary>
        /// <param name="arg1">A string expression</param>
        /// <returns>An expression</returns>
        public static IExpression ToLower(IExpression arg1)
        {
            return new Expr(QueryOpCode.LOWER, arg1);
        }

        /// <summary>
        /// Builds a UPPER expression from 1 string argument.
        /// When evaluated, the expression returns: (String)arg1.ToUpperInvariant()
        /// </summary>
        /// <param name="arg1">A string expression</param>
        /// <returns>An expression</returns>
        public static IExpression ToUpper(IExpression arg1)
        {
            return new Expr(QueryOpCode.UPPER, arg1);
        }

        /// <summary>
        /// Builds a DayOfWeek expression from 1 date/time argument.
        /// When evaluated, the expression returns: arg1.DayOfWeek()
        /// </summary>
        /// <param name="arg1">A date/time expression</param>
        /// <returns>An expression</returns>
        public static IExpression DayOfWeek(IExpression arg1)
        {
            return new Expr(QueryOpCode.DOW, arg1);
        }

        /// <summary>
        /// Builds an ADDition expression from 2 numeric arguments.
        /// When evaluated, the expression returns: (arg1 + arg2)
        /// </summary>
        /// <param name="arg1">A numeric expression</param>
        /// <param name="arg2">A numeric expression</param>
        /// <returns>An expression</returns>
        public static IExpression NumADD(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.ADD, arg1, arg2);
        }

        /// <summary>
        /// Builds a MULtiplication expression from 2 numeric arguments.
        /// When evaluated, the expression returns: (arg1 * arg2)
        /// </summary>
        /// <param name="arg1">A numeric expression</param>
        /// <param name="arg2">A numeric expression</param>
        /// <returns>An expression</returns>
        public static IExpression NumMUL(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.MUL, arg1, arg2);
        }

        /// <summary>
        /// Builds a SUBtraction expression from 2 numeric arguments.
        /// When evaluated, the expression returns: (arg1 - arg2)
        /// </summary>
        /// <param name="arg1">A numeric expression</param>
        /// <param name="arg2">A numeric expression</param>
        /// <returns>An expression</returns>
        public static IExpression NumSUB(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.SUB, arg1, arg2);
        }

        /// <summary>
        /// Builds a DIVision expression from 2 numeric arguments.
        /// When evaluated, the expression returns: (arg1 / arg2)
        /// </summary>
        /// <param name="arg1">A numeric expression</param>
        /// <param name="arg2">A numeric expression</param>
        /// <returns>An expression</returns>
        public static IExpression NumDIV(IExpression arg1, IExpression arg2)
        {
            return new Expr(QueryOpCode.DIV, arg1, arg2);
        }

        #endregion
        
        #region Private Static Helpers

        private static QueryNodeType QueryNodeTypeFromString(string value)
        {
            string[] names = Enum.GetNames(typeof(QueryNodeType));
            for (int i = 0; i < names.Length; i++)
            {
                if (String.Compare(value, names[i], StringComparison.OrdinalIgnoreCase) == 0)
                    return (QueryNodeType)i;
            }
            // unknown
            return QueryNodeType._NONE;
        }

        private static QueryOpCode QueryOpCodeFromString(string value)
        {
            return EnumHelper.Parse(value, true, QueryOpCode._NONE);
        }

        #endregion
        
        #region Public static helpers

        public static IExpression ALL => Const(true);

        public static string Display(IExpression expr)
        {
            return expr != null ? expr.DisplayString() : "";
        }

        public static string Serialise(IExpression expr)
        {
            return expr?.Serialise();
        }

        public static bool TryDeserialise(string text, out IExpression result, out Exception failReason)
        {
            bool success = false;
            result = null;
            failReason = null;
            try
            {
                result = Create(text);
                success = true;
            }
            catch (Exception excp)
            {
                failReason = excp;
            }
            return success;
        }

        public static IExpression Deserialise(string text)
        {
            IExpression result;
            Exception excp;
            TryDeserialise(text, out result, out excp);
            return result;
        }

        public static T CastTo<T>(object value, T defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is T)
                return (T)value;
            // not exact type - attempt cast
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception excp)
            {
                Trace.WriteLine($"Expr.CastTo<{typeof(T).Name}>() failed: {excp}");
                return defaultValue;
            }
        }

        #endregion
        
        #region Private Constructors

        private Expr(Exception excp)
        {
            // construct an exception constant
            // - used for trapping query deserialisation errors
            _nodeType = QueryNodeType.ERROR;
            _constValue = excp;
            _operator = QueryOpCode._NONE;
        }

        /// <summary>
        /// Initializes a new instance of the <see>
        ///                                     <cref>QueryExpr</cref>
        ///                                   </see> class from a V1QueryExpr.
        /// </summary>
        /// <param name="queryExpr">The query expr.</param>
        public Expr(V1QueryExpr queryExpr)
        {
            // construct from deserialised xml message
            if (queryExpr == null)
                throw new ArgumentNullException(nameof(queryExpr));
            // get node type: eg. const, propref, expr, etc.
            QueryNodeType nodeType = QueryNodeTypeFromString(queryExpr.node);
            _nodeType = nodeType;
            _operator = QueryOpCode._NONE;
            try
            {
                switch (nodeType)
                {
                    case QueryNodeType.CONST:
                        var nv = new NamedValue("value/" + queryExpr.name + "=" + queryExpr.value);
                        _constValue = nv.Value;
                        break;
                    case QueryNodeType.ERROR:
                        _constValue = queryExpr.value;
                        break;
                    case QueryNodeType.FIELD:
                        _propName = queryExpr.name;
                        break;
                    case QueryNodeType.EXPR:
                        _operator = QueryOpCodeFromString(queryExpr.name);
                        _operands = queryExpr.args != null ? new IExpression[queryExpr.args.Length] : new IExpression[0];
                        // recursively load the expression arguments
                        for (int i = 0; i < _operands.Length; i++)
                        {
                            try
                            {
                                _operands[i] = new Expr(queryExpr.args[i]);
                            }
                            catch (Exception excp)
                            {
                                _operands[i] = new Expr(excp);
                            }
                        }
                        break;
                    default:
                        // unknown node type
                        _nodeType = QueryNodeType.ERROR;
                        _constValue = (new ArgumentException("Unknown queryExpr.type", queryExpr.node)).ToString();
                        break;
                }
            }
            catch (Exception excp)
            {
                // exception creating expression node
                // - convert node to exception constant
                _nodeType = QueryNodeType.ERROR;
                _constValue = excp.ToString();
            }
        }
        private Expr(QueryNodeType nodeType, string propName, object constValue)
        {
            // constant/propref constructor
            _nodeType = nodeType;
            _propName = propName;
            _constValue = constValue;
            _operator = QueryOpCode._NONE;
        }
        private Expr(QueryOpCode opCode)
        {
            // unary expression constructor
            _nodeType = QueryNodeType.EXPR;
            _operator = opCode;
            _operands = new IExpression[0];
        }
        private Expr(QueryOpCode opCode, IExpression arg1)
        {
            // unary expression constructor
            _nodeType = QueryNodeType.EXPR;
            _operator = opCode;
            _operands = new [] { arg1 };
        }
        private Expr(QueryOpCode opCode, IExpression arg1, IExpression arg2)
            : this(opCode, new List<IExpression>() { arg1, arg2 })
        { }
        private Expr(QueryOpCode opCode, IExpression arg1, IExpression arg2, IExpression arg3)
            : this(opCode, new List<IExpression>() { arg1, arg2, arg3 })
        { }
        private Expr(QueryOpCode opCode, IExpression arg1, IExpression arg2, IExpression arg3, IExpression arg4)
            : this(opCode, new List<IExpression>() { arg1, arg2, arg3, arg4 })
        { }
        private Expr(QueryOpCode opCode, params IExpression[] args)
            : this(opCode, args.ToList())
        { }
        private Expr(QueryOpCode opCode, IEnumerable<IExpression> args)
        {
            // n-ary expression constructor
            _nodeType = QueryNodeType.EXPR;
            _operator = opCode;
            _operands = args.Where(a => a != null).ToArray();
        }
        private Expr(QueryOpCode opCode, NamedValueSet propSet)
        {
            // binary expression constructor
            _nodeType = QueryNodeType.EXPR;
            _operator = opCode;
            NamedValue[] nvArray = propSet.ToArray();
            _operands = new IExpression[nvArray.Length];
            int i = 0;
            foreach (NamedValue nv in nvArray)
            {
                // assume named value is simple value type
                // except: arrays get converted to Boolean-OR sub-expressions
                object value = nv.Value;
                if (value.GetType().IsArray)
                {
                    // named value is an array - convert to OR sub-expression (or SQL 'IN' expression)
                    var subValues = (Array)value;
                    var subOperands = new IExpression[subValues.Length];
                    int j = 0;
                    foreach (var subValue in subValues)
                    {
                        subOperands[j] = new Expr(QueryOpCode.EQU, Prop(nv.Name), Const(subValue));
                        j++;
                    }
                    _operands[i] = BoolOR(subOperands);
                }
                else
                {
                    // not an array
                    _operands[i] = new Expr(QueryOpCode.EQU, Prop(nv.Name), Const(value));
                }
                i++;
            }
        }
        #endregion
       
        #region Serialisers
        
        /// <summary>
        /// Serialises the expression for storage and transmission.
        /// </summary>
        /// <returns></returns>
        public string Serialise()
        {
            QuerySpec qs = new QuerySpec
            {
                version = 1,
                v1QueryExpr = ToV1QueryExpr()
            };
            return XmlSerializerHelper.SerializeToString(qs);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return DisplayString();
        }

        /// <summary>
        /// Converts this expression to a WCF data contract.
        /// </summary>
        /// <returns></returns>
        public V1QueryExpr ToV1QueryExpr()
        {
            V1QueryExpr result = new V1QueryExpr {node = _nodeType.ToString()};
            switch (_nodeType)
            {
                case QueryNodeType.CONST:
                    result.name = _constValue.GetType().Name;
                    result.value = new NamedValue("value", _constValue).ValueString;
                    break;
                case QueryNodeType.ERROR:
                    result.name = typeof(string).Name;
                    result.value = _constValue.ToString();
                    break;
                case QueryNodeType.FIELD:
                    result.name = _propName;
                    break;
                case QueryNodeType.EXPR:
                    result.name = _operator.ToString();
                    result.args = new V1QueryExpr[_operands.Length];
                    // recursively serialise expression arguments
                    for (int i = 0; i < _operands.Length; i++)
                    {
                        result.args[i] = null;
                        if (_operands[i] != null)
                            result.args[i] = _operands[i].ToV1QueryExpr();
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unknown query node type '" + _nodeType + "'!");
            }
            return result;
        }

        #endregion
        
        #region Expression Evaluator

        private void CheckArgCount(int minCount)
        {
            if (_results.Length < minCount)
                throw new ArgumentException(
                    "Operator '" + _operator + "' requires at least " + minCount + " argument(s)!");
        }

        private void CheckArgCount(int minCount, int maxCount)
        {
            if (_results.Length < minCount || _results.Length > maxCount)
            {
                if (minCount == maxCount)
                    throw new ArgumentException("Operator '" + _operator + "' requires exactly " + minCount + " argument(s)!");
                else
                    throw new ArgumentException("Operator '" + _operator + "' requires between " +
                        minCount + " and " + maxCount + " argument(s)!");
            }
        }

        private void CheckArgType(int index, Type type, bool isRequired)
        {
            if (index < 0)
                throw new ArgumentException("Invalid value: " + index, nameof(index));
            if (_results.Length < (index + 1))
                throw new ArgumentException("Result[" + index + "] is missing!");
            if (isRequired && (_results[index] == null))
                throw new ArgumentNullException("Result[" + index + "]");
            if (_results[index] == null)
                return;
            if (_results[index].GetType() != type)
                throw new ArgumentException(
                    "Result[" + index + "] '" + _results[index] +
                    "' (" + _results[index].GetType().Name + ") is not the correct type (" + type.Name + ")!");
        }

        private void CheckArgTypeIsNumeric(int index, bool isRequired)
        {
            if (index < 0)
                throw new ArgumentException("Invalid value: " + index, nameof(index));
            if (_results.Length < (index + 1))
                throw new ArgumentException("Result[" + index + "] is missing!");
            if (isRequired && (_results[index] == null))
                throw new ArgumentNullException("Result[" + index + "]");
            if (_results[index] == null)
                return;
            if (!TypeIsNumeric(_results[index].GetType()))
                throw new ArgumentException(
                    "Result[" + index + "] '" + _results[index] +
                    "' (" + _results[index].GetType().Name + ") is not numeric!");
        }

        private void CheckArgTypeIsDateTime(int index, bool isRequired)
        {
            if (index < 0)
                throw new ArgumentException("Invalid value: " + index, nameof(index));
            if (_results.Length < (index + 1))
                throw new ArgumentException("Result[" + index + "] is missing!");
            if (isRequired && _results[index] == null)
                throw new ArgumentNullException("Result[" + index + "]");
            if (_results[index] == null)
                return;
            if (!TypeIsDateTime(_results[index].GetType()))
                throw new ArgumentException(
                    "Result[" + index + "] '" + _results[index] +
                    "' (" + _results[index].GetType().Name + ") is not a date/time!");
        }

        private void CheckMultiNumericOp()
        {
            // checks:
            // - there are 1 or more args
            // - all args are not null
            // - all args are numeric
            CheckArgCount(0);
            for (int i = 0; i < _results.Length; i++)
            {
                if (_results[i] == null)
                    throw new ArgumentNullException("Result[" + i + "]");
                if (!TypeIsNumeric(_results[i].GetType()))
                    throw new ArgumentException(
                        "Result[" + i + "] '" + _results[i] +
                        "' (" + _results[i].GetType().Name + ") is not numeric!");
            }
        }

        private static bool TypeIsNumeric(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }
        private static bool TypeIsDateTime(Type type)
        {
            if ((type == typeof(DateTimeOffset)) || (type == typeof(DateTime)))
                return true;

            return false;
        }
        private static int CompareValues(object arg1, object arg2)
        {
            // null handling
            // - null is equal to null
            // - null is less than anything
            // - anything is greater than null
            if (arg1 == arg2)
                return 0;
            if (arg1 == null)
                return 1;
            if (arg2 == null)
                return -1;
            // otherwise do type specific comparison
            Type type1 = arg1.GetType();
            Type type2 = arg2.GetType();
            if (TypeIsNumeric(type1) && TypeIsNumeric(type2))
            {
                // numeric compare - as doubles
                Double value1 = Convert.ToDouble(arg1);
                Double value2 = Convert.ToDouble(arg2);
                if (value1 == value2)
                    return 0;
                if (value1 > value2)
                    return 1;
                return -1;
            }
            // date/time comparisons
            if ((type1 == typeof(DateTime) || type1 == typeof(DateTimeOffset))
                && (type2 == typeof(DateTime) || type2 == typeof(DateTimeOffset)))
            {
                DateTimeOffset value1;
                if (type1 == typeof(DateTime))
                    value1 = new DateTimeOffset((DateTime)(arg1));
                else
                    value1 = (DateTimeOffset)(arg1);
                DateTimeOffset value2;
                if (type2 == typeof(DateTime))
                    value2 = new DateTimeOffset((DateTime)(arg2));
                else
                    value2 = (DateTimeOffset)(arg2);
                return value1.CompareTo(value2);
            }
            // non-numeric compare - types must match
            if (type1 != type2)
                throw new ArgumentException(
                    "Cannot compare values '" + arg1 + "' (" + type1.Name + ") and '" +
                    arg2 + "' (" + type2.Name + ") of different types!");
            // strings - case-insensitive
            if (type1 == typeof(string))
            {
                var value1 = (string)(arg1);
                var value2 = (string)(arg2);
                return String.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
            }
            // TimeSpans
            if (type1 == typeof(TimeSpan))
            {
                var value1 = (TimeSpan)(arg1);
                var value2 = (TimeSpan)(arg2);
                return value1.CompareTo(value2);
            }
            // guids
            if (type1 == typeof(Guid))
            {
                var value1 = (Guid)(arg1);
                var value2 = (Guid)(arg2);
                return value1.CompareTo(value2);
            }
            // bool
            if (type1 == typeof(bool))
            {
                var value1 = (bool)(arg1);
                var value2 = (bool)(arg2);
                return value1.CompareTo(value2);
            }
            // other types
            throw new NotSupportedException(
                "Cannot compare values '" + arg1 + "' (" + type1.Name +
                ") and '" + arg2 + "' (" + type2.Name + ")!");
        }

        private static object DoOpAddValues(object arg1, object arg2)
        {
            Type type1 = arg1.GetType();
            Type type2 = arg2.GetType();
            if (TypeIsNumeric(type1) && TypeIsNumeric(type2))
            {
                // numeric Add - as doubles
                return (Convert.ToDouble(arg1) + Convert.ToDouble(arg2));
            }
            // datetime/timespan addition
            if (((type1 == typeof(DateTime)) || (type1 == typeof(DateTimeOffset))) && (type2 == typeof(TimeSpan)))
            {
                if (type1 == typeof(DateTime))
                    return ((DateTime)arg1).Add((TimeSpan)arg2);
                return ((DateTimeOffset)arg1).Add((TimeSpan)arg2);
            }
            // non-numeric Add - types must match
            if (type1 != type2)
                throw new ArgumentException(
                    "Cannot Add values '" + arg1 + "' (" + type1.Name + ") and '" +
                    arg2 + "' (" + type2.Name + ") of different types!");
            // TimeSpans
            if (type1 == typeof(TimeSpan))
            {
                var value1 = (TimeSpan)(arg1);
                var value2 = (TimeSpan)(arg2);
                return (value1 + value2);
            }
            // other types
            throw new NotSupportedException(
                "Cannot Add values '" + arg1 + "' (" + type1.Name +
                ") and '" + arg2 + "' (" + type2.Name + ")!");
        }

        private static object DoOpSubtractValues(object arg1, object arg2)
        {
            Type type1 = arg1.GetType();
            Type type2 = arg2.GetType();
            if (TypeIsNumeric(type1) && TypeIsNumeric(type2))
            {
                // numeric Subtract - as doubles
                return (Convert.ToDouble(arg1) - Convert.ToDouble(arg2));
            }
            // date/time subtraction
            if (((type1 == typeof(DateTime)) || (type1 == typeof(DateTimeOffset)))
                && ((type2 == typeof(DateTime)) || (type2 == typeof(DateTimeOffset))))
            {
                DateTimeOffset value1;
                if (type1 == typeof(DateTime))
                    value1 = new DateTimeOffset((DateTime)(arg1));
                else
                    value1 = (DateTimeOffset)(arg1);
                DateTimeOffset value2;
                if (type2 == typeof(DateTime))
                    value2 = new DateTimeOffset((DateTime)(arg2));
                else
                    value2 = (DateTimeOffset)(arg2);
                return (value1 - value2);
            }
            // non-numeric Subtract - types must match
            if (type1 != type2)
                throw new ArgumentException(
                    "Cannot Subtract values '" + arg1 + "' (" + type1.Name + ") and '" +
                    arg2 + "' (" + type2.Name + ") of different types!");
            // TimeSpans
            if (type1 == typeof(TimeSpan))
            {
                TimeSpan value1 = (TimeSpan)(arg1);
                TimeSpan value2 = (TimeSpan)(arg2);
                return (value1 - value2);
            }
            // other types
            throw new NotSupportedException(
                "Cannot Subtract values '" + arg1 + "' (" + type1.Name +
                ") and '" + arg2 + "' (" + type2.Name + ")!");
        }

        private static DayOfWeek DoOpDayOfWeek(object arg1)
        {
            Type type1 = arg1.GetType();
            if (type1 == typeof(DateTime))
                return (((DateTime)arg1).DayOfWeek);
            if (type1 == typeof(DateTimeOffset))
                return (((DateTimeOffset)arg1).DayOfWeek);
            throw new NotSupportedException(
                "Cannot get DayOfWeek of value '" + arg1 + "' (" + type1.Name + ")!");
        }

        private static DateTimeOffset DoOpDatePart(object arg1)
        {
            Type type1 = arg1.GetType();
            if (type1 == typeof(DateTime))
                return (((DateTime)arg1).Date);
            if (type1 == typeof(DateTimeOffset))
                return (((DateTimeOffset)arg1).Date);
            throw new NotSupportedException(
                "Cannot get DatePart of value '" + arg1 + "' (" + type1.Name + ")!");
        }

        private static TimeSpan DoOpTimePart(object arg1)
        {
            Type type1 = arg1.GetType();
            if (type1 == typeof(DateTime))
                return (((DateTime)arg1).TimeOfDay);
            if (type1 == typeof(DateTimeOffset))
                return (((DateTimeOffset)arg1).TimeOfDay);
            throw new NotSupportedException(
                "Cannot get TimePart of value '" + arg1 + "' (" + type1.Name + ")!");
        }

        private object EvalExpr(IExprContext exprContext, DateTimeOffset dtoNow)
        {
            // evaluate all the arguments - todo optimisations
            _results = new object[_operands.Length];
            for (int i = 0; i < _operands.Length; i++)
            {
                if (_operands[i] == null)
                    throw new ArgumentNullException("Argument[" + i + "]");
                _results[i] = _operands[i].Evaluate(exprContext, dtoNow);
            }
            switch (_operator)
            {
                case QueryOpCode.AND:
                    {
                        // check all results are boolean, and true
                        for (int i = 0; i < _results.Length; i++)
                        {
                            // skip null values
                            if (_results[i] == null)
                                continue;
                            // non-null values must be boolean
                            if (_results[i].GetType() != typeof(bool))
                                throw new ArgumentException(
                                    "Result[" + i + "] '" + _results[i] +
                                    "' (" + _results[i].GetType().Name + ") is not boolean!");
                            if (!((bool)(_results[i])))
                                return false; // early exit
                        }
                        return true;
                    }
                case QueryOpCode.OR:
                    {
                        // check all results are boolean, and 1 is true
                        for (int i = 0; i < _results.Length; i++)
                        {
                            // skip null values
                            if (_results[i] == null)
                                continue;
                            // non-null values must be boolean
                            if (_results[i].GetType() != typeof(bool))
                                throw new ArgumentException(
                                    "Result[" + i + "] '" + _results[i] +
                                    "' (" + _results[i].GetType().Name + ") is not boolean!");
                            if (((bool)(_results[i])))
                                return true; // early exit
                        }
                        return false;
                    }
                case QueryOpCode.MUL:
                    {
                        CheckMultiNumericOp();
                        // check all results are boolean, and 1 is true
                        return _results.Aggregate(1.0, (current, t) => current*Convert.ToDouble(t));
                    }
                case QueryOpCode.ADD:
                    {
                        CheckArgCount(2, 2);
                        // - add values
                        return (DoOpAddValues(_results[0], _results[1]));
                    }
                case QueryOpCode.SUB:
                    {
                        CheckArgCount(2, 2);
                        // - subtract values
                        return (DoOpSubtractValues(_results[0],_results[1]));
                    }
                case QueryOpCode.DIV:
                    {
                        CheckArgCount(2, 2);
                        CheckArgTypeIsNumeric(0, true);
                        CheckArgTypeIsNumeric(1, true);
                        // - subtract values
                        return (Convert.ToDouble(_results[0]) / Convert.ToDouble(_results[1]));
                    }
                case QueryOpCode.COMP:
                    {
                        CheckArgCount(2, 2);
                        // - compare values
                        return CompareValues(_results[0], _results[1]);
                    }
                case QueryOpCode.EQU:
                    {
                        CheckArgCount(2, 2);
                        // - compare values
                        return (CompareValues(_results[0], _results[1]) == 0);
                    }
                case QueryOpCode.NEQ:
                    {
                        CheckArgCount(2, 2);
                        // - compare values
                        return (CompareValues(_results[0], _results[1]) != 0);
                    }
                case QueryOpCode.GEQ:
                    {
                        CheckArgCount(2, 2);
                        // - compare values
                        return (CompareValues(_results[0], _results[1]) >= 0);
                    }
                case QueryOpCode.GTR:
                    {
                        CheckArgCount(2, 2);
                        // - compare values
                        return (CompareValues(_results[0], _results[1]) > 0);
                    }
                case QueryOpCode.LEQ:
                    {
                        CheckArgCount(2, 2);
                        // - compare values
                        return (CompareValues(_results[0], _results[1]) <= 0);
                    }
                case QueryOpCode.LSS:
                    {
                        CheckArgCount(2, 2);
                        // - compare values
                        return (CompareValues(_results[0], _results[1]) < 0);
                    }
                case QueryOpCode.ISNULL:
                    {
                        CheckArgCount(1, 1);
                        return (_results[0] == null);
                    }
                case QueryOpCode.ISNOTNULL:
                    {
                        CheckArgCount(1, 1);
                        return (_results[0] != null);
                    }
                case QueryOpCode.STARTS:
                    {
                        CheckArgCount(2, 2);
                        CheckArgType(0, typeof(String), false);
                        CheckArgType(1, typeof(String), true);
                        if (_results[0] == null)
                            return false;
                        return (((String)_results[0]).ToUpperInvariant().StartsWith(((String)(_results[1])).ToUpperInvariant()));
                    }
                case QueryOpCode.ENDS:
                    {
                        CheckArgCount(2, 2);
                        CheckArgType(0, typeof(String), false);
                        CheckArgType(1, typeof(String), true);
                        if (_results[0] == null)
                            return false;
                        return (((String)_results[0]).ToUpperInvariant().EndsWith(((String)(_results[1])).ToUpperInvariant()));
                    }
                case QueryOpCode.CONTAINS:
                    {
                        CheckArgCount(2, 2);
                        CheckArgType(0, typeof(String), false);
                        CheckArgType(1, typeof(String), true);
                        if (_results[0] == null)
                            return false;
                        return (((String)_results[0]).ToUpperInvariant().Contains(((String)(_results[1])).ToUpperInvariant()));
                    }
                case QueryOpCode.LOWER:
                    {
                        CheckArgCount(1, 1);
                        CheckArgType(0, typeof(String), false);
                        return ((string) _results[0])?.ToLowerInvariant();
                    }
                case QueryOpCode.UPPER:
                    {
                        CheckArgCount(1, 1);
                        CheckArgType(0, typeof(String), false);
                        return ((string) _results[0])?.ToUpperInvariant();
                    }
                case QueryOpCode.NOW:
                    {
                        CheckArgCount(0, 0);
                        return dtoNow;
                    }
                case QueryOpCode.DATE:
                    {
                        CheckArgCount(0, 0);
                        return DoOpDatePart(dtoNow);
                    }
                case QueryOpCode.DATEPART:
                    {
                        CheckArgCount(1, 1);
                        CheckArgTypeIsDateTime(0, true);
                        return DoOpDatePart(_results[0]);
                    }
                case QueryOpCode.TIME:
                    {
                        CheckArgCount(0, 0);
                        return dtoNow.TimeOfDay;
                    }
                case QueryOpCode.TIMEPART:
                    {
                        CheckArgCount(1, 1);
                        CheckArgTypeIsDateTime(0, true);
                        return DoOpTimePart(_results[0]);
                    }
                case QueryOpCode.DOW:
                    {
                        CheckArgCount(1, 1);
                        CheckArgTypeIsDateTime(0, true);
                        return DoOpDayOfWeek(_results[0]);
                    }
                default:
                    throw new NotImplementedException("Operator '" + _operator + "'");
            }
        }

        private static string OperatorDisplayStr(QueryOpCode opCode)
        {
            switch (opCode)
            {
                case QueryOpCode.EQU: return "==";
                case QueryOpCode.NEQ: return "!=";
                case QueryOpCode.GTR: return ">";
                case QueryOpCode.GEQ: return ">=";
                case QueryOpCode.LSS: return "<";
                case QueryOpCode.LEQ: return "<=";
                case QueryOpCode.ADD: return "+";
                case QueryOpCode.SUB: return "-";
                case QueryOpCode.MUL: return "*";
                case QueryOpCode.DIV: return "/";
                case QueryOpCode.SHL: return "<<";
                case QueryOpCode.SHR: return ">>";
                case QueryOpCode.UPPER: return "ToUpper";
                case QueryOpCode.LOWER: return "ToLower";
                case QueryOpCode.DOW: return "DayOfWeek";
                case QueryOpCode.COMP: return "Compare";
                case QueryOpCode.ISNULL: return "IsNull";
                case QueryOpCode.ISNOTNULL: return "IsNotNull";
                default: return opCode.ToString();
            }
        }

        private string DisplayExprStr()
        {
            var displayStrings = new string[_operands.Length];
            for (int i = 0; i < _operands.Length; i++)
            {
                displayStrings[i] = null;
                if (_operands[i] != null)
                    displayStrings[i] = _operands[i].DisplayString();
            }
            switch (_operator)
            {
                // infix operators
                case QueryOpCode.ADD:
                case QueryOpCode.MUL:
                case QueryOpCode.SUB:
                case QueryOpCode.DIV:
                case QueryOpCode.OR:
                case QueryOpCode.AND:
                case QueryOpCode.EQU:
                case QueryOpCode.NEQ:
                case QueryOpCode.GEQ:
                case QueryOpCode.GTR:
                case QueryOpCode.LEQ:
                case QueryOpCode.LSS:
                case QueryOpCode.STARTS:
                case QueryOpCode.ENDS:
                case QueryOpCode.CONTAINS:
                    {
                        bool printedArg = false;
                        var result = new StringBuilder();
                        for (int i = 0; i < displayStrings.Length; i++)
                        {
                            if (i == 0)
                                result.Append("(");
                            if (displayStrings[i] != null)
                            {
                                if (i > 0 && printedArg)
                                {
                                    result.Append(" " + OperatorDisplayStr(_operator) + " ");
                                    //printedArg = false;
                                }
                                result.Append(displayStrings[i]);
                                printedArg = true;
                            }
                            if (i == (displayStrings.Length - 1))
                                result.Append(")");
                        }
                        return result.ToString();
                    }
                // special unary operators
                case QueryOpCode.LEZ: return "(" + displayStrings[0] + " <= 0)";
                case QueryOpCode.LTZ: return "(" + displayStrings[0] + " < 0)";
                case QueryOpCode.GEZ: return "(" + displayStrings[0] + " >= 0)";
                case QueryOpCode.GTZ: return "(" + displayStrings[0] + " > 0)";
                case QueryOpCode.EQZ: return "(" + displayStrings[0] + " == 0)";
                case QueryOpCode.NEZ: return "(" + displayStrings[0] + " != 0)";
                // non-infix binary operators
                case QueryOpCode.COMP:
                    return OperatorDisplayStr(_operator) + "(" + displayStrings[0] + "," + displayStrings[1] + ")";
                // unary operators
                case QueryOpCode.UPPER:
                case QueryOpCode.DOW:
                case QueryOpCode.LOWER:
                case QueryOpCode.ISNULL:
                case QueryOpCode.ISNOTNULL:
                    return OperatorDisplayStr(_operator) + "(" + displayStrings[0] + ")";
                // scalar operators
                case QueryOpCode.NOW:
                    return "Now()";
                case QueryOpCode.DATE:
                    return "Date()";
                case QueryOpCode.TIME:
                    return "Time()";
                case QueryOpCode.DATEPART:
                    return "DatePart(" + displayStrings[0] + ")";
                case QueryOpCode.TIMEPART:
                    return "TimePart(" + displayStrings[0] + ")";
                default:
                    return "UnknownOP(" + _operator + ")";
            }
        }
        #endregion
        
        #region IExpression implementation

        /// <summary>
        /// Matches the property set to this query expression.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public bool MatchesProperties(NamedValueSet dataContext)
        {
            return MatchesProperties(new ExprContext(dataContext, null, DateTime.MinValue, DateTime.MinValue), DateTimeOffset.Now);
        }

        public bool MatchesProperties(NamedValueSet dataContext, string itemName, DateTimeOffset itemCreated, DateTimeOffset itemExpires, DateTimeOffset dtoNow)
        {
            return MatchesProperties(new ExprContext(dataContext, itemName, itemCreated, itemExpires), dtoNow);
        }

        public bool MatchesProperties(IExprContext exprContext)
        {
            return MatchesProperties(exprContext, DateTimeOffset.Now);
        }

        public bool MatchesProperties(IExprContext exprContext, DateTimeOffset dtoNow)
        {
            object result = Evaluate(exprContext, dtoNow);
            if (result == null)
                throw new InvalidOperationException("Evaluate returned null!");
            if (result.GetType() != typeof(bool))
                throw new InvalidOperationException(
                    "Evaluate returned non-boolean result: '" + result + "' (" + result.GetType().Name + ")");
            return (bool)result;
        }

        /// <summary>
        /// Converts a value to a readable string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ConstToString(object value)
        {
            // special types
            if (value == null)
                return "(null)";
            Type type = value.GetType();
            if (type == typeof(String))
                return "'" + value + "'";
            return "{" + value + "}";
        }

        /// <summary>
        /// Returns the expression in a readable form. Useful for debugging and displays.
        /// </summary>
        /// <returns></returns>
        public string DisplayString()
        {
            switch (_nodeType)
            {
                case QueryNodeType.CONST:
                    return ConstToString(_constValue);
                case QueryNodeType.ERROR:
                    return "ERROR('" + _constValue + "')";
                case QueryNodeType.FIELD:
                    return "[" + _propName + "]";
                case QueryNodeType.EXPR:
                    return DisplayExprStr();
                default:
                    throw new InvalidOperationException("Unknown query node type '" + _nodeType + "'!");
            }
        }

        public bool HasErrors()
        {
            switch (_nodeType)
            {
                case QueryNodeType.CONST:
                    return (_constValue is Exception);
                case QueryNodeType.ERROR:
                    return true;
                case QueryNodeType.FIELD:
                    return false;
                case QueryNodeType.EXPR:
                    {
                        return _operands.Any(operand => operand.HasErrors());
                    }
                default:
                    throw new InvalidOperationException("Unknown query node type '" + _nodeType + "'!");
            }
        }

        /// <summary>
        /// Evaluates this expression in the given data context.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        public object Evaluate(NamedValueSet dataContext)
        {
            return Evaluate(new ExprContext(dataContext, null, DateTimeOffset.MinValue, DateTimeOffset.MaxValue), DateTimeOffset.Now);
        }

        public object Evaluate(NamedValueSet dataContext, DateTimeOffset asAtTime)
        {
            return Evaluate(new ExprContext(dataContext, null, DateTimeOffset.MinValue, DateTimeOffset.MaxValue), asAtTime);
        }

        public object Evaluate(NamedValueSet dataContext, string itemName, DateTimeOffset itemCreated, DateTimeOffset itemExpires, DateTimeOffset asAtTime)
        {
            return Evaluate(new ExprContext(dataContext, itemName, itemCreated, itemExpires), asAtTime);
        }

        public object Evaluate(IExprContext exprContext, DateTimeOffset asAtTime)
        {
            switch (_nodeType)
            {
                case QueryNodeType.CONST:
                    return _constValue;
                case QueryNodeType.ERROR:
                    throw new InvalidOperationException("Cannot evaluate expression containing ERROR!");
                case QueryNodeType.FIELD:
                    if (exprContext == null)
                        throw new ArgumentNullException("ExprContext");
                    if (exprContext.DataContext == null)
                        throw new ArgumentException("exprContext.DataContext is null");
                    NamedValue nv = exprContext.DataContext.Get(_propName);
                    if (nv != null)
                        return nv.Value;

                    // check for system properties
                    if (_propName.Equals(SysPropItemName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (exprContext.ItemName == null)
                            throw new ArgumentException("exprContext.ItemName is null");
                        return exprContext.ItemName;
                    }

                    if (_propName.Equals(SysPropItemCreated, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return exprContext.ItemCreated;
                    }

                    if (_propName.Equals(SysPropItemExpires, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return exprContext.ItemExpires;
                    }
                    return null;
                case QueryNodeType.EXPR:
                    return EvalExpr(exprContext, asAtTime);
                default:
                    throw new InvalidOperationException("Unknown expression node type '" + _nodeType + "'!");
            }
        }

        #endregion

        #region IEquatable<Expr> Members

        private int CalcHash()
        {
            // hash is calculated from operator and operands
            // - determinsitic operators
            int result = _operator.GetHashCode();
            for (int i = 0; i < _operands.Length; i++)
            {
                if (_operands[i] == null)
                    throw new ArgumentNullException("Argument[" + i + "]");
                result ^= _operands[i].GetHashCode();
            }
            // - special case non-determinsitic operators (eg. time-dependent scalar methods)
            switch (_operator)
            {
                case QueryOpCode.NOW:
                case QueryOpCode.DATE:
                case QueryOpCode.TIME:
                    return result ^ DateTimeOffset.Now.GetHashCode();
                default:
                    return result;
            }
        }

        public override int GetHashCode()
        {
            int nodeTypeHash = _nodeType.GetHashCode();
            switch (_nodeType)
            {
                case QueryNodeType.CONST:
                case QueryNodeType.ERROR:
                    return nodeTypeHash ^ (_constValue == null ? 0 : _constValue.GetHashCode());
                case QueryNodeType.FIELD:
                    return nodeTypeHash ^ (_propName == null ? 0 : _propName.ToLower().GetHashCode());
                case QueryNodeType.EXPR:
                    return nodeTypeHash ^ CalcHash();
                default:
                    return nodeTypeHash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals((Expr)obj);
        }

        public bool Equals(Expr other)
        {
            return other != null && (GetHashCode() == other.GetHashCode());
        }

        #endregion
    }
}