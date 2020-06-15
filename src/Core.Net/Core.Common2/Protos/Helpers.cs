using Highlander.Utilities.Logging;
using System;

namespace Highlander.Core.Common.Protos
{
    public partial class V131Helpers
    {
        /// <summary>
        /// Checks the required file version of a candidate against required. Version numbers must be in the 4-part
        /// dotted "Major.Minor.BuildDate.Revision" format. The "Major" parts must be equal. For the "Minor" and "BuildDate"
        /// parts, the candidate must be greater than or equal to the required. The "Revision" parts are ignored.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="requiredVersion">The required version.</param>
        /// <param name="candidateVersion">The candidate version.</param>
        /// <returns></returns>
        public static bool CheckRequiredFileVersion(ILogger logger, string requiredVersion, string candidateVersion)
        {
            bool result = false;
            try
            {
                ServiceHelper.ParseBuildLabel(requiredVersion, out var requiredMajorVersion, out var requiredMinorVersion, out var requiredBuildDate);
                ServiceHelper.ParseBuildLabel(candidateVersion, out var candidateMajorVersion, out var candidateMinorVersion, out var candidateBuildDate);
                result =
                    candidateMajorVersion == requiredMajorVersion &&
                    candidateMinorVersion >= requiredMinorVersion &&
                    candidateBuildDate >= requiredBuildDate;
            }
            catch (FormatException e)
            {
                logger.Log(e);
            }
            return result;
        }
    }
}
