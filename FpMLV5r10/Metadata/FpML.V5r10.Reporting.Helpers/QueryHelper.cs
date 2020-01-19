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

using System;
using FpML.V5r10.Codes;
using Orion.Util.Helpers;

namespace FpML.V5r10.Reporting.Helpers
{
    public static class QueryHelper
    {
        //public static QueryParameter Create(String queryId,
        //    List<Pair<String queryOperator, String queryValue>>)
        //{
        //    var query = new QueryParameter();

        //    return query;
        //}

        public static QueryParameter Create(String queryId,
                                            String queryOperator, String queryValue)
        {
            EnumHelper.TryParse(queryValue, true, out QueryParameterOperatorEnum _);
            var query = new QueryParameter
                {
                    queryParameterId = new QueryParameterId {Value = queryId},
                    queryParameterOperator = new QueryParameterOperator {Value = queryOperator},
                    queryParameterValue = queryValue
                };
            return query;
        }
    }
}