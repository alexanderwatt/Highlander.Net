#region Using directives



#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public class InterpolationMethodHelper
    {
        public static InterpolationMethod Parse(string methodName)
        {
            InterpolationMethod interpolationMethod = new InterpolationMethod {Value = methodName};
            return interpolationMethod;
        }
   }
}
