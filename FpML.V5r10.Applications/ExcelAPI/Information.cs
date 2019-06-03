#region Using directives

using System;
using System.IO;
using System.Reflection;

#endregion

namespace HLV5r3
{
    ///<summary>
    /// Information about the CurveGenerator assembly.
    ///</summary>
    public static class Information
    {
        ///<summary>
        /// Gets the version of the assembly
        ///</summary>
        ///<returns></returns>
        public  static string GetVersionInfo()
        {
            var uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var fileInfo = new FileInfo(uri.LocalPath);
            DateTime buildTime = fileInfo.LastWriteTime;
            string formattedInfo =
                $"Version: FpML.V5r3 v1.0 Built on : {buildTime.ToShortDateString()}, {buildTime.ToShortTimeString()}";
            return formattedInfo;
        }
    }
}