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

using System;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;

namespace Orion.Identifiers
{
    public class PriceableAssetProperties
    {
        public PriceableAssetProperties(string instrumentId)
        {
            string[] parts = instrumentId.Split('-');
            if (parts.Length == 1)
            {
                Term = parts[0];
                _termTenorString = parts[0];
                AssetType = AssetTypesEnum.Period;
                return;
            }
            if (parts.Length == 2)
            {
                throw new ArgumentException(
                    $"Instrument name '{instrumentId}' is invalid, it must be in the format 'a-b-c'");
            }
            // Extract assetType
            string assetName = parts[1].Split('/')[0];
            if (!EnumHelper.TryParse(assetName, true, out AssetTypesEnum assetType))
            {
                throw new NotSupportedException($"Asset type '{assetName}' is not supported");
            }
            AssetType = assetType;
            // Extract currency
            if (parts[0].Length == 3)
            {
                Currency = parts[0].ToUpper();
            }
            else if ((assetType == AssetTypesEnum.FxSpot
                      || assetType == AssetTypesEnum.FxForward
                      || assetType == AssetTypesEnum.FxFuture) && parts[0].Length == 6)
            {
                Currency = parts[0].Substring(0, 3).ToUpper();
            }
            else
            {
                throw new ArgumentException(
                    $"Instrument name '{instrumentId}' is invalid, it must be in the format 'a-b-c' where 'a' is the 3 letter currency");
            }
            // Extract variant
            string[] assetNames = parts[1].Split('/');
            if (assetNames.Length > 1)
            {
                Variant = assetNames[1];
            }
            // Extract term
            if (assetType == AssetTypesEnum.IRFuture)//TODO Added later to handle null exception.
            {
                Term = parts[2];
                _termTenorString = parts[2];
                if (parts.Length > 3)
                {
                    Term = Term + parts[3];
                    _termTenorString = Term;
                }               
            }
            if (assetType == AssetTypesEnum.IRFutureOption)//TODO Added later to handle null exception.
            {
                Term = parts[2];
                _termTenorString = parts[2];
                if (parts.Length > 3)
                {
                    Term = Term + parts[3];
                    _termTenorString = Term;
                }
                if (parts.Length > 4)
                {
                    if (decimal.TryParse(parts[4], out var strike))
                    {
                        Strike = strike;
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"Instrument name '{instrumentId}' is invalid, it must be in the format 'a-b-c-d-e' where 'e' is the strike as a decimal");
                    }
                }
            }
            if (assetType != AssetTypesEnum.IRFuture)
            {
                if (assetType == AssetTypesEnum.Bond)
                {
                    var bondCode =parts[2].Split('.');
                    if (bondCode.Length > 3)
                    {
                        Term = bondCode[4];
                        _termTenorString = bondCode[4];
                    }
                }
                else
                {
                    Term = parts[2];
                    _termTenorString = parts[2];
                }               
            }
            // Extract specific parts for certain class types
            switch (assetType)
            {
                case AssetTypesEnum.Caplet:
                case AssetTypesEnum.Floorlet:
                case AssetTypesEnum.BillCaplet:
                case AssetTypesEnum.BillFloorlet:
                    // Extract Forward Index
                    if (parts.Length > 3)
                    {
                        ForwardIndex = PeriodHelper.Parse(parts[3]);
                    }
                    // Extract Strike
                    if (parts.Length > 4)
                    {
                        if (decimal.TryParse(parts[4], out var strike))
                        {
                            Strike = strike;
                        }
                        else
                        {
                            throw new ArgumentException(
                                $"Instrument name '{instrumentId}' is invalid, it must be in the format 'a-b-c-d-e' where 'e' is the strike as a decimal");
                        }
                    }                   
                    break;
                case AssetTypesEnum.Fra:
                case AssetTypesEnum.BillFra:
                case AssetTypesEnum.SpreadFra:
                case AssetTypesEnum.SimpleFra:
                case AssetTypesEnum.BasisSwap:
                case AssetTypesEnum.Swaption:
                    // Extract Forward Index
                    if (parts.Length > 3)
                    {
                        ForwardIndex = PeriodHelper.Parse(parts[3]);
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"Instrument name '{instrumentId}' is invalid, it must be in the format 'a-b-c-d' where 'd' is the index interval");
                    }
                    break;
                case AssetTypesEnum.IRCap:
                case AssetTypesEnum.IRFloor:
                    // Extract Forward Index if it is there.
                    if (parts.Length > 3)
                    {
                        try
                        {
                            if (decimal.TryParse(parts[3], out var strike))
                            {
                                Strike = strike;
                            }
                            else
                            {
                                ForwardIndex = PeriodHelper.Parse(parts[3]);
                            }
                        }
                        catch (System.Exception e)
                        {
                            throw new ArgumentException(
                                $"Instrument name '{instrumentId}' is invalid, it must be in the format 'a-b-c-d' where 'd' is the strike as a decimal OR an interval and it is '{e}'");
                        }                                            
                    }
                    //if (parts.Length > 3)
                    //{
                    //    if (decimal.TryParse(parts[3], out var strike))
                    //    {
                    //        Strike = strike;
                    //    }
                    //    else
                    //    {
                    //        throw new ArgumentException(
                    //            $"Instrument name '{instrumentId}' is invalid, it must be in the format 'a-b-c-d-e' where 'e' is the strike as a decimal");
                    //    }
                    //}
                    break;
                case AssetTypesEnum.CommodityForward:
                case AssetTypesEnum.CommodityFuture:
                case AssetTypesEnum.CommoditySpread:
                case AssetTypesEnum.CommodityFutureSpread:
                case AssetTypesEnum.CommodityAverageForward:
                    if (parts.Length > 3)
                    {
                        _termTenorString = parts[3];
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"Instrument name '{instrumentId}' is invalid, it must be in the format 'a-b-c-d' where 'd' is the tenor");
                    }
                    break;
            }
        }

        private readonly string _termTenorString;

        public string Currency { get; }

        public AssetTypesEnum AssetType { get; }

        public string Variant { get; }

        public string Term { get; }

        public Period TermTenor => _termTenorString != null ? PeriodHelper.Parse(_termTenorString) : null;

        public decimal Strike { get; }

        public string Commodity => Term;

        public Period ForwardIndex { get; }
    }
}