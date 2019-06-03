#region Using directives

using System;

#endregion

namespace Orion.CurveEngine.Extensions
{
    ///<summary>
    ///</summary>
    public static class ExceptionExtension
    {
        ///<summary>
        ///</summary>
        ///<param name="exception"></param>
        ///<returns></returns>
        public static string GetExceptionInfo(this Exception exception)
        {
            return exception.ToString();
        }
    }
}