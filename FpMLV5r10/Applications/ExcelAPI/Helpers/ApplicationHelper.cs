#region Usings

using System;
using System.Reflection;

#endregion

namespace HLV5r3.Helpers
{
    ///<summary>
    ///</summary>
    public static class ApplicationHelper
    {
        #region Diagnostics

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public static object[,] Diagnostics()
        {
            var info = new AssemblyInfo(Assembly.GetExecutingAssembly());
            return info.ToArray();
        }

        ///<summary>
        ///</summary>
        ///<param name="itemName"></param>
        ///<returns></returns>
        public static string Diagnostics(string itemName)
        {
            var info = new AssemblyInfo(Assembly.GetExecutingAssembly());
            return info[itemName];
        }

        #endregion

        #region Com Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subKeyName"></param>
        /// <returns></returns>
        public static string GetSubKeyName(Type type, string subKeyName)
        {
            var s = new System.Text.StringBuilder();
            s.Append(@"CLSID\{");
            s.Append(type.GUID.ToString().ToUpper());
            s.Append(@"}\");
            s.Append(subKeyName);
            return s.ToString();
        }

        #endregion
    }
}
