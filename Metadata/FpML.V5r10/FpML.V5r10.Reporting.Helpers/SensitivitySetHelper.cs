#region Using directives

using System.Collections.Generic;
using System.Linq;

#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    public static class SensitivitySetHelper
    {
        public static SensitivitySet Copy(SensitivitySet baseSensitivitySet)
        {
            SensitivitySet result = null;
            if (baseSensitivitySet!=null)
            {
                result = new SensitivitySet();
                if (baseSensitivitySet.sensitivity!=null)
                {
                    result.sensitivity = SensitivityHelper.Copy(baseSensitivitySet.sensitivity);
                }
                if (baseSensitivitySet.definitionReference!=null)
                {
                    result.definitionReference = new SensitivitySetDefinitionReference
                                                     {href = baseSensitivitySet.definitionReference.href};
                }               
                result.id = baseSensitivitySet.id;
                result.name = baseSensitivitySet.name;
            }
            return result; 
        }

        public static SensitivitySet[] Copy(SensitivitySet[] baseSensitivitySet)
        {
            return baseSensitivitySet?.Select(Copy).ToArray();
        }
    }

    public static class SensitivityHelper
    {
        public static Sensitivity Copy(Sensitivity baseSensitivity)
        {
            Sensitivity result = null;
            if (baseSensitivity != null)
            {
                result = new Sensitivity
                             {
                                 Value = baseSensitivity.Value,
                                 name = baseSensitivity.name,
                                 definitionRef = baseSensitivity.definitionRef
                             };
            }
            return result;
        }

        public static Sensitivity[] Copy(Sensitivity[] baseSensitivities)
        {
            List<Sensitivity> result = baseSensitivities?.Select(Copy).ToList();
            return result?.ToArray();
        }
    }
}