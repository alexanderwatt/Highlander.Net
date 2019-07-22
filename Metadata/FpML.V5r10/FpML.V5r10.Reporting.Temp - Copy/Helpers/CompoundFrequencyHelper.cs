/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using FpML.V5r3.Codes;

#endregion

namespace Orion.Analytics.Helpers
{
    /// <summary>
    /// Compound frequency helper.
    /// </summary>
    public static class CompoundFrequencyHelper
    {
        private static double GetCompoundingPeriod(CompoundingFrequencyEnum compoundingFrequencyEnum)
        {
            double frequency;
            switch (compoundingFrequencyEnum)
            {
                case CompoundingFrequencyEnum.Continuous:
                    frequency = 0;
                    break;
                case CompoundingFrequencyEnum.Daily:
                    frequency = 1 / 365d;
                    break;
                case CompoundingFrequencyEnum.Weekly:
                    frequency = 1 / 52d;
                    break;
                case CompoundingFrequencyEnum.Monthly:
                    frequency = 1 / 12d;
                    break;
                case CompoundingFrequencyEnum.Quarterly:
                    frequency = 0.25;
                    break;
                case CompoundingFrequencyEnum.SemiAnnual:
                    frequency = 0.5;
                    break;
                case CompoundingFrequencyEnum.Annual:
                    frequency = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compoundingFrequencyEnum), "CompoundingFrequency is of an invalid type " + compoundingFrequencyEnum);
            }
            return frequency;
        }
    }
}
