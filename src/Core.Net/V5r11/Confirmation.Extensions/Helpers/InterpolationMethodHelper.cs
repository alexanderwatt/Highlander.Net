#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
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
