#region Using directives

using System;
using System.Collections.Generic;

#endregion

namespace Orion.CurveEngine.Extensions
{
    ///<summary>
    ///</summary>
    public static class DictionaryExtension
    {
        ///<summary>
        /// Property item
        ///</summary>
        public class ExpressionItem
        {
            /// <summary>
            /// Name of the property
            /// </summary>
            public string Name;

            /// <summary>
            /// Value of the property
            /// </summary>
            public object Value;
        }

        ///<summary>
        ///</summary>
        ///<param name="dictionary"></param>
        ///<returns></returns>
        public static string GetAsString(this IDictionary<string, object> dictionary)
        {
            var result = Environment.NewLine;
            result += "Size: " + dictionary.Count + Environment.NewLine;
            foreach (var key in dictionary.Keys)
            {
                result += String.Format("['{0}'] = '{1}'" + Environment.NewLine, key, dictionary[key]);
            }
            return result;
        }


        ///<summary>
        ///</summary>
        ///<param name="dictionary"></param>
        ///<returns></returns>
        public static string GetAsString(this IDictionary<string, string> dictionary)
        {
            var result = Environment.NewLine;
            result += "Size: " + dictionary.Count + Environment.NewLine;
            foreach (var key in dictionary.Keys)
            {
                result += String.Format("['{0}'] = '{1}'" + Environment.NewLine, key, dictionary[key]);
            }
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="query"></param>
        ///<returns></returns>
        public static IDictionary<string, object> ToQuery(List<ExpressionItem> query)
        {
            var result = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            query.ForEach(expressionItem => result.Add(expressionItem.Name, expressionItem.Value));
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="query"></param>
        ///<returns></returns>
        public static IDictionary<string, string> ToStringStringQuery(List<ExpressionItem> query)
        {
            var result = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            query.ForEach(expressionItem => result.Add(expressionItem.Name, expressionItem.Value.ToString()));
            return result;
        }
    }
}