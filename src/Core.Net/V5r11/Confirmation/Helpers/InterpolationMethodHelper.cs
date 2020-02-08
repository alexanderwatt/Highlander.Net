#region Using directives



#endregion

namespace FpML.V5r3.Confirmation
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
