/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System.Collections.Generic;
using System.Linq;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.Reporting.Helpers.V5r3
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