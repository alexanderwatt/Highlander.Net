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

using FpML.V5r10.Codes;

namespace FpML.V5r10.Reporting.Helpers
{
    public class AssetMeasureTypeHelper
    {
        public static AssetMeasureType Parse(string measureTypeAsString)
        {
            // ensures value is valid enum string
            var assetMeasureEnum = AssetMeasureEnum.Undefined;
            if (measureTypeAsString != null)
                assetMeasureEnum = AssetMeasureScheme.ParseEnumString(measureTypeAsString);
            AssetMeasureType assetMeasureType = Create(assetMeasureEnum);
            return assetMeasureType;
        }
        
        public static AssetMeasureType Create(AssetMeasureEnum assetMeasure)
        {
            var assetMeasureType = new AssetMeasureType {Value = assetMeasure.ToString()};
            return assetMeasureType;
        }

        public static AssetMeasureType Copy(AssetMeasureType assetMeasure)
        {
            var assetMeasureType = new AssetMeasureType { Value = assetMeasure.Value};
            return assetMeasureType;
        }
    }
}