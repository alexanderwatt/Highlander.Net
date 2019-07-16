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

using FpML.V5r11.Codes;

namespace FpML.V5r11.Reporting
{
    public partial class CompoundingFrequency
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CompoundingFrequencyEnum ToEnum()
        {
            return EnumParse.ToCompoundingFrequencyEnum(Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static CompoundingFrequency Parse(string frequency)
        {
            var result = new CompoundingFrequency { Value = frequency };

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static CompoundingFrequency Create(CompoundingFrequencyEnum frequency)
        {
            var result
                = new CompoundingFrequency
                {
                    Value = CompoundingFrequencyScheme.GetEnumString(frequency)
                };

            return result;
        }
    }
}
