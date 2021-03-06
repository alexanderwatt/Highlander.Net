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

#region Using directives



#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    public static class ResetFrequencyHelper
    {

        /// <summary>
        /// Parses the specified interval as string.
        /// </summary>
        /// <param name="intervalAsString"><example>3M,3m,14d,6Y</example></param>
        /// <returns></returns>
        public static ResetFrequency Parse(string intervalAsString)
        {
            var result = new ResetFrequency();
            Period interval = PeriodHelper.Parse(intervalAsString);
            result.periodMultiplier = interval.periodMultiplier;
            result.period = interval.period.ToString();        
            return result;
        }
    }
}