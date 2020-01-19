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

#region Usings

using System;
using Highlander.Utilities.NamedValues;

#endregion

namespace Highlander.Utilities.Expressions
{
    /// <summary>
    /// The expression interface.
    /// </summary>
    public interface IExpression
    {
        /// <summary>
        /// Matches the property set to this query expression.
        /// </summary>
        /// <param name="propSet">The prop set.</param>
        /// <returns></returns>
        bool MatchesProperties(NamedValueSet propSet);
        bool MatchesProperties(NamedValueSet propSet, string itemName, DateTimeOffset itemCreated, DateTimeOffset itemExpires, DateTimeOffset dtoNow);
        bool MatchesProperties(IExprContext exprContext);
        bool MatchesProperties(IExprContext exprContext, DateTimeOffset dtoNow);
        /// <summary>
        /// Evaluates this expression in the given data context.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <returns></returns>
        object Evaluate(NamedValueSet dataContext);
        object Evaluate(NamedValueSet dataContext, DateTimeOffset asAtTime);
        object Evaluate(NamedValueSet dataContext, string itemName, DateTimeOffset itemCreated, DateTimeOffset itemExpires, DateTimeOffset asAtTime);
        object Evaluate(IExprContext exprContext, DateTimeOffset asAtTime);
        /// <summary>
        /// Returns the expression in a readable form. Useful for debugging and displays.
        /// </summary>
        /// <returns></returns>
        string DisplayString();
        /// <summary>
        /// Returns true if the expression contains errors and cannot be evaluated.
        /// </summary>
        /// <returns></returns>
        bool HasErrors();
        /// <summary>
        /// Serialises the expression for storage and transmission.
        /// </summary>
        /// <returns></returns>
        string Serialise();
        /// <summary>
        /// Converts this expression to a WCF data contract.
        /// </summary>
        /// <returns></returns>
        V1QueryExpr ToV1QueryExpr();
    }

}
