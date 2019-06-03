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
            QueryParameterOperatorEnum result;
            EnumHelper.TryParse(queryValue, true, out result);
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