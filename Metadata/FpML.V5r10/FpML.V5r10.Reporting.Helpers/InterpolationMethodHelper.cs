#region Using directives



#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    public class InterpolationMethodHelper
    {
        public static InterpolationMethod Parse(string methodName)
        {
            InterpolationMethod interpolationMethod = new InterpolationMethod();
            interpolationMethod.Value = methodName;
            return interpolationMethod;
        }
   }
}
