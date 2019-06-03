#region Using directives

using System;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using Orion.Analytics.Helpers;
using Orion.Analytics.BusinessCenters;
using Math = System.Math;

#endregion

namespace Orion.CalendarEngine.Helpers
{
    /// <summary>
    /// Useful methods to convert between date types.
    /// </summary>
    public class AdjustedDateHelper
    {
        ///  <summary>
        ///  Converts to a date time.
        ///  </summary>
        /// <param name="cache"></param>
        ///  <param name="unadjustedDate"></param>
        ///  <param name="businessDayAdjustments"></param>
        ///  <param name="offset"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(ICoreCache cache, DateTime unadjustedDate, BusinessDayAdjustments businessDayAdjustments, Offset offset, string nameSpace)
        {
            if (DayTypeEnum.Business != offset.dayType && DayTypeEnum.Calendar != offset.dayType)
            {
                throw new NotSupportedException(
                    $"Only {DayTypeEnum.Business}, {DayTypeEnum.Calendar} day types supported of Offset type.");
            }
            IBusinessCalendar businessCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, businessDayAdjustments.businessCenters, nameSpace);
            int periodMultiplier = Int32.Parse(offset.periodMultiplier);
            // offset using calendar days
            //
            switch (offset.dayType)
            {
                case DayTypeEnum.Business:
                    {
                        switch (offset.period)
                        {
                            case PeriodEnum.D:
                                {
                                    // Advance using given number of business days
                                    //
                                    int periodMultiplierSign = Math.Sign(periodMultiplier);
                                    DateTime offsetedDate = unadjustedDate;
                                    while (periodMultiplier-- > 0)
                                    {
                                        offsetedDate = offsetedDate.AddDays(periodMultiplierSign);
                                        offsetedDate = businessCalendar.Roll(offsetedDate, businessDayAdjustments.businessDayConvention);
                                    }
                                    return offsetedDate;
                                }
                            default:
                                throw new NotSupportedException(
                                    $"{offset.period} not supported in conjunction with '{offset.dayType} day type'");

                        }//~switch(offset.period)
                    }
                case DayTypeEnum.Calendar:
                    {
                        // Convert offset to period.
                        DateTime adjustedDate = offset.Add(unadjustedDate);
                        return adjustedDate;
                    }
                default:
                    {
                        throw new NotSupportedException(
                            $"Only {DayTypeEnum.Business}, {DayTypeEnum.Calendar} day types supported of Offset type.");
                    }
            }
        }

        /// <summary>
        /// Converts to a date time.
        /// </summary>
        ///<param name="businessCalendar"></param>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayAdjustments"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(IBusinessCalendar businessCalendar, DateTime unadjustedDate, BusinessDayAdjustments businessDayAdjustments, Offset offset)
        {
            if (DayTypeEnum.Business != offset.dayType && DayTypeEnum.Calendar != offset.dayType)
            {
                throw new NotSupportedException(
                    $"Only {DayTypeEnum.Business}, {DayTypeEnum.Calendar} day types supported of Offset type.");
            }
            int periodMultiplier = Int32.Parse(offset.periodMultiplier);
            // offset using calendar days
            //
            switch (offset.dayType)
            {
                case DayTypeEnum.Business:
                    {
                        switch (offset.period)
                        {
                            case PeriodEnum.D:
                                {
                                    // Advance using given number of business days
                                    //
                                    int periodMultiplierSign = Math.Sign(periodMultiplier);
                                    DateTime offsetedDate = unadjustedDate;
                                    while (periodMultiplier-- > 0)
                                    {
                                        offsetedDate = offsetedDate.AddDays(periodMultiplierSign);
                                        offsetedDate = businessCalendar.Roll(offsetedDate, businessDayAdjustments.businessDayConvention);
                                    }
                                    return offsetedDate;
                                }
                            default:
                                throw new NotSupportedException(
                                    $"{offset.period} not supported in conjunction with '{offset.dayType} day type'");

                        }//~switch(offset.period)
                    }
                case DayTypeEnum.Calendar:
                    {
                        // Convert offset to period.
                        DateTime adjustedDate = offset.Add(unadjustedDate);
                        return adjustedDate;
                    }
                default:
                    {
                        throw new NotSupportedException(
                            $"Only {DayTypeEnum.Business}, {DayTypeEnum.Calendar} day types supported of Offset type.");
                    }
            }
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="adjustableDate"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(ICoreCache cache, AdjustableDate adjustableDate, string nameSpace)
        {
            if (null == adjustableDate)
                throw new ArgumentNullException(nameof(adjustableDate));
            // handle  BusinessDatConventionEnum is NONE as a special case, since there might be no business centers provided. 
            //
            if ((null == adjustableDate.dateAdjustments) ||
                (BusinessDayConventionEnum.NONE == adjustableDate.dateAdjustments.businessDayConvention))
            {
                return adjustableDate.unadjustedDate.Value;
            }
            IBusinessCalendar businessCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, adjustableDate.dateAdjustments.businessCenters, nameSpace);
            DateTime result = businessCalendar.Roll(adjustableDate.unadjustedDate.Value, adjustableDate.dateAdjustments.businessDayConvention);
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="cache"> </param>
        /// <param name="referenceDate"></param>
        /// <param name="adjustableOrRelativeDate"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(ICoreCache cache, DateTime? referenceDate, AdjustableOrRelativeDate adjustableOrRelativeDate, string nameSpace)
        {            
            if (null == adjustableOrRelativeDate.Item)
                throw new ArgumentNullException(nameof(adjustableOrRelativeDate));
            // handle  BusinessDatConventionEnum is NONE as a special case, since there might be no business centers provided. 
            //
            var adjustableDate = adjustableOrRelativeDate.Item as AdjustableDate;
            var relativeDate = adjustableOrRelativeDate.Item as RelativeDateOffset;
            var result = new DateTime();
            if (adjustableDate !=null )
            {
                var calendar = BusinessCenterHelper.ToBusinessCalendar(cache, adjustableDate.dateAdjustments.businessCenters, nameSpace);
                return ToAdjustedDate(calendar, adjustableDate);
            }
            if (relativeDate != null)
            {
                var calendar = BusinessCenterHelper.ToBusinessCalendar(cache, relativeDate.businessCenters, nameSpace);
                if (referenceDate != null) return ToAdjustedDate(calendar, (DateTime)referenceDate, relativeDate);
            }
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="adjustableDate"></param>
        /// <param name="businessCalendar"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(IBusinessCalendar businessCalendar, AdjustableDate adjustableDate)
        {
            if (null == adjustableDate)
                throw new ArgumentNullException(nameof(adjustableDate));
            // handle  BusinessDatConventionEnum is NONE as a special case, since there might be no business centers provided. 
            //
            if ((null == adjustableDate.dateAdjustments) ||
                (BusinessDayConventionEnum.NONE == adjustableDate.dateAdjustments.businessDayConvention))
            {
                return adjustableDate.unadjustedDate.Value;
            }
            if (businessCalendar == null)
            {
                return adjustableDate.unadjustedDate.Value;
            }
            DateTime result = businessCalendar.Roll(adjustableDate.unadjustedDate.Value, adjustableDate.dateAdjustments.businessDayConvention);
            return result;
        }

        /// <summary>
        /// Converts to an adjusted date.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="referenceDate"></param>
        /// <param name="relativeDateOffset"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(ICoreCache cache, DateTime referenceDate, RelativeDateOffset relativeDateOffset, string nameSpace)
        {
            // handle  BusinessDatConventionEnum is NONE as a special case, since there might be no business centers provided. 
            if (BusinessDayConventionEnum.NONE == relativeDateOffset.businessDayConvention)
            {
                return referenceDate;
            }
            //The default daytype.
            if (relativeDateOffset.dayTypeSpecified == false || relativeDateOffset.businessCenters == null)
            {
                relativeDateOffset.dayType = DayTypeEnum.Calendar;
            }
            IBusinessCalendar businessCalendar = relativeDateOffset.businessCenters == null ? new Hell() : BusinessCenterHelper.ToBusinessCalendar(cache, relativeDateOffset.businessCenters, nameSpace);
            var interval = PeriodHelper.Parse(relativeDateOffset.periodMultiplier + relativeDateOffset.period);
            var offset = OffsetHelper.FromInterval(interval, relativeDateOffset.dayType);
            DateTime result = businessCalendar.Advance(referenceDate, offset, relativeDateOffset.businessDayConvention);
            return result;
        }

        /// <summary>
        /// Converts to an adjusted date.
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <param name="relativeDateOffset"></param>
        /// <param name="businessCalendar"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(IBusinessCalendar businessCalendar, DateTime referenceDate, RelativeDateOffset relativeDateOffset)
        {
            // handle  BusinessDatConventionEnum is NONE as a special case, since there might be no business centers provided. 
            if (BusinessDayConventionEnum.NONE == relativeDateOffset.businessDayConvention)
            {
                return referenceDate;
            }
            //The default daytype.
            if (relativeDateOffset.dayTypeSpecified == false || relativeDateOffset.businessCenters == null)
            {
                relativeDateOffset.dayType = DayTypeEnum.Calendar;
            }
            //IBusinessCalendar businessCalendar = relativeDateOffset.businessCenters == null ? new Hell() : BusinessCenterHelper.ToBusinessCalendar(cache, relativeDateOffset.businessCenters);
            var interval = PeriodHelper.Parse(relativeDateOffset.periodMultiplier + relativeDateOffset.period);
            var offset = OffsetHelper.FromInterval(interval, relativeDateOffset.dayType);
            DateTime result = businessCalendar.Advance(referenceDate, offset, relativeDateOffset.businessDayConvention);
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayConventions"></param>
        /// <param name="businessCentersAsString"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(ICoreCache cache, DateTime unadjustedDate, string businessDayConventions, string businessCentersAsString, string nameSpace)
        {
            AdjustableDate adjustableDate = DateTypesHelper.ToAdjustableDate(unadjustedDate, businessDayConventions, businessCentersAsString);
            DateTime adjustedDate = ToAdjustedDate(cache, adjustableDate, nameSpace);
            return adjustedDate;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayConventions"></param>
        /// <param name="businessCentersAsString"></param>
        /// <param name="businessCalendar"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(IBusinessCalendar businessCalendar, DateTime unadjustedDate, string businessDayConventions, string businessCentersAsString)
        {
            AdjustableDate adjustableDate = DateTypesHelper.ToAdjustableDate(unadjustedDate, businessDayConventions, businessCentersAsString);
            DateTime adjustedDate = ToAdjustedDate(businessCalendar, adjustableDate);
            return adjustedDate;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayAdjustments"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(ICoreCache cache, DateTime unadjustedDate, BusinessDayAdjustments businessDayAdjustments, string nameSpace)
        {
            var adjustableDate = new AdjustableDate
                                     {
                                         unadjustedDate = new IdentifiedDate { Value = unadjustedDate, id = ItemsChoiceType.adjustedDate.ToString() },
                                         dateAdjustments = businessDayAdjustments
                                     };
            return ToAdjustedDate(cache, adjustableDate, nameSpace);
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayAdjustments"></param>
        /// <param name="businessCalendar"></param>
        /// <returns></returns>
        public static DateTime ToAdjustedDate(IBusinessCalendar businessCalendar, DateTime unadjustedDate, BusinessDayAdjustments businessDayAdjustments)
        {
            var adjustableDate = new AdjustableDate
            {
                unadjustedDate = new IdentifiedDate { Value = unadjustedDate, id = ItemsChoiceType.adjustedDate.ToString() },
                dateAdjustments = businessDayAdjustments
            };
            return ToAdjustedDate(businessCalendar, adjustableDate);
        }

        /// <summary>
        /// Derives the adjusted date if not already provided.
        /// </summary>
        /// <param name="adjustableOrAdjustedDate">htis may contain the adjustedDate, an unadjustedDate and businessCentres</param>
        /// <param name="businessCalendar">THe business calendar must be provided, no namespace is required and can be null</param>
        /// <returns></returns>
        public static DateTime? GetAdjustedDate(IBusinessCalendar businessCalendar, AdjustableOrAdjustedDate adjustableOrAdjustedDate)
        {
            object date;
            var result = AdjustableOrAdjustedDateHelper.Contains(adjustableOrAdjustedDate, ItemsChoiceType.adjustedDate, out date);
            if (result)
            {
                return ((IdentifiedDate)date).Value;
            }
            object businessDayAdjustments;
            result = AdjustableOrAdjustedDateHelper.Contains(adjustableOrAdjustedDate, ItemsChoiceType.unadjustedDate, out date);
            var bda = AdjustableOrAdjustedDateHelper.Contains(adjustableOrAdjustedDate, ItemsChoiceType.dateAdjustments, out businessDayAdjustments);
            if (result && date != null)
            {
                DateTime unadjustedDate = ((IdentifiedDate)date).Value;
                if (bda && businessCalendar != null)
                {
                    var adjustments = businessDayAdjustments as BusinessDayAdjustments;
                    if (adjustments != null)
                    {
                        
                        return businessCalendar.Roll(unadjustedDate, adjustments.businessDayConvention);
                    }
                }
                return unadjustedDate;
            }
            return null;
        }

        /// <summary>
        /// Derives the adjusted date if not already provided.
        /// </summary>
        /// <param name="adjustableDate">htis may contain the adjustedDate, an unadjustedDate and businessCentres</param>
        /// <param name="cache">THe cache if the business calendar has not already been calculated.</param>
        /// <param name="nameSpace">The client nameSpce</param>
        /// <param name="businessCalendar">THe business calendar must be provided, no namespace is required and can be null</param>
        /// <returns></returns>
        public static DateTime? GetAdjustedDate(ICoreCache cache, String nameSpace, IBusinessCalendar businessCalendar,
            AdjustableDate adjustableDate)
        {
            if (adjustableDate.adjustedDate != null)
            {
                return adjustableDate.adjustedDate.Value;
            }
            if (businessCalendar == null)
            {
                businessCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, adjustableDate.dateAdjustments.businessCenters, nameSpace);
            }
            return ToAdjustedDate(businessCalendar, adjustableDate.unadjustedDate.Value, adjustableDate.dateAdjustments);
        }

        /// <summary>
        /// Derives the adjusted date if not already provided.
        /// </summary>
        /// <param name="baseDate">The base date is settlement is relative.</param>
        /// <param name="relativeDateOffset">htis may contain the adjustedDate, an unadjustedDate and businessCentres</param>
        /// <param name="cache">THe cache if the business calendar has not already been calculated.</param>
        /// <param name="nameSpace">The client nameSpce</param>
        /// <param name="businessCalendar">THe business calendar must be provided, no namespace is required and can be null</param>
        /// <returns></returns>
        public static DateTime? GetAdjustedDate(ICoreCache cache, String nameSpace, IBusinessCalendar businessCalendar, 
            DateTime? baseDate, RelativeDateOffset relativeDateOffset)
        {
            if (relativeDateOffset.adjustedDate != null)
            {
                return relativeDateOffset.adjustedDate.Value;
            }
            if (baseDate == null) return null;
            if (businessCalendar == null)
            {
                businessCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, relativeDateOffset.businessCenters, nameSpace);
            }
            return ToAdjustedDate(businessCalendar, (DateTime)baseDate, relativeDateOffset);
        }

        /// <summary>
        /// Derives the adjusted date if not already provided.
        /// </summary>
        /// <param name="baseDate">The base date is settlement is relative.</param>
        /// <param name="adjustableOrAdjustedDate">htis may contain the adjustedDate, an unadjustedDate and businessCentres</param>
        /// <param name="cache">THe cache if the business calendar has not already been calculated.</param>
        /// <param name="nameSpace">The client nameSpce</param>
        /// <param name="businessCalendar">THe business calendar must be provided, no namespace is required and can be null</param>
        /// <returns></returns>
        public static DateTime? GetAdjustedDate(ICoreCache cache, String nameSpace, IBusinessCalendar businessCalendar, 
            DateTime? baseDate, AdjustableOrRelativeDate adjustableOrAdjustedDate)
        {
            var date = adjustableOrAdjustedDate.Item as AdjustableDate;
            if (date != null)
            {
                return GetAdjustedDate(cache, nameSpace, businessCalendar, date);
            }
            var relativeDate = adjustableOrAdjustedDate.Item as RelativeDateOffset;
            if (relativeDate != null && baseDate != null)
            {
                return GetAdjustedDate(cache, nameSpace, businessCalendar, baseDate,
                                       relativeDate);
            }
            return null;
        }
    }
}