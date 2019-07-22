#region Using directives

using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;

#endregion

namespace Orion.Analytics.Helpers
{
    /// <summary>
    /// Useful methods to convert between date types.
    /// </summary>
    public static class DateTypesHelper
    {
        /// <summary>
        /// Converts to a required date type.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static RequiredIdentifierDate ToRequiredIdentifierDate(DateTime dateTime)
        {
            var result = new RequiredIdentifierDate {Value = dateTime};
            return result;
        }

        /// <summary>
        /// Converts to an identified date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <returns></returns>
        public static IdentifiedDate ToUnadjustedIdentifiedDate(DateTime unadjustedDate)
        {
            var result = new IdentifiedDate {Value = unadjustedDate, id = ItemsChoiceType1.unadjustedDate.ToString()};
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="adjustedDate"></param>
        /// <returns></returns>
        public static AdjustableOrAdjustedDate ToAdjustableOrAdjustedDate(DateTime adjustedDate)
        {
            var result = AdjustableOrAdjustedDateHelper.CreateAdjustedDate(adjustedDate);
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayConventionAsString"></param>
        /// <param name="businessCentersAsString"></param>
        /// <returns></returns>
        public static AdjustableOrAdjustedDate ToAdjustableOrAdjustedDate(DateTime unadjustedDate, string businessDayConventionAsString, string businessCentersAsString)
        {
            var result = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(unadjustedDate, businessDayConventionAsString, businessCentersAsString);
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayConvention"></param>
        /// <param name="businessCentersAsString"></param>
        /// <returns></returns>
        public static AdjustableOrAdjustedDate ToAdjustableOrAdjustedDate(DateTime unadjustedDate, BusinessDayConventionEnum businessDayConvention, string businessCentersAsString)
        {
            var result = AdjustableOrAdjustedDateHelper.CreateUnadjustedDate(unadjustedDate, businessDayConvention.ToString(), businessCentersAsString);
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayConventionAsString"></param>
        /// <param name="businessCentersAsString"></param>
        /// <returns></returns>
        public static AdjustableDate ToAdjustableDate(DateTime unadjustedDate, string businessDayConventionAsString, string businessCentersAsString)
        {
            AdjustableDate result = ToAdjustableDate(unadjustedDate, businessDayConventionAsString);
            result.dateAdjustments.businessCenters = BusinessCentersHelper.Parse(businessCentersAsString);
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayConvention"></param>
        /// <param name="businessCentersAsString"></param>
        /// <returns></returns>
        public static AdjustableDate ToAdjustableDate(DateTime unadjustedDate, BusinessDayConventionEnum businessDayConvention, string businessCentersAsString)
        {
            AdjustableDate result = ToAdjustableDate(unadjustedDate, businessDayConvention);
            result.dateAdjustments.businessCenters = BusinessCentersHelper.Parse(businessCentersAsString);
            return result;
        }

        /// <summary>
        /// Converts t an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayConventionAsString"></param>
        /// <returns></returns>
        public static AdjustableDate ToAdjustableDate(DateTime unadjustedDate, string businessDayConventionAsString)
        {
            var result = new AdjustableDate { unadjustedDate = ToUnadjustedIdentifiedDate(unadjustedDate) };
            var businessDayAdjustments = new BusinessDayAdjustments
                                                                {
                                                                    businessDayConvention =
                                                                        BusinessDayConventionHelper.Parse(
                                                                        businessDayConventionAsString),
                                                                        businessDayConventionSpecified = true
                                                                };
            result.dateAdjustments = businessDayAdjustments;
            return result;
        }

        /// <summary>
        /// Converts t an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <param name="businessDayConvention"></param>
        /// <returns></returns>
        public static AdjustableDate ToAdjustableDate(DateTime unadjustedDate, BusinessDayConventionEnum businessDayConvention)
        {
            var result = new AdjustableDate { unadjustedDate = ToUnadjustedIdentifiedDate(unadjustedDate) };
            var businessDayAdjustments = new BusinessDayAdjustments
            {
                businessDayConvention = businessDayConvention,
                businessDayConventionSpecified = true
            };
            result.dateAdjustments = businessDayAdjustments;
            return result;
        }

        /// <summary>
        /// Converts to an adjustable date type.
        /// </summary>
        /// <param name="unadjustedDate"></param>
        /// <returns></returns>
        public static AdjustableDate ToAdjustableDate(DateTime unadjustedDate)
        {
            return ToAdjustableDate(unadjustedDate, "NONE");
        }

        /// <summary>
        /// Gets the payment relative to.
        /// </summary>
        /// <param name="discountingType">Type of the discounting.</param>
        /// <returns></returns>
        public static PayRelativeToEnum GetPaymentRelativeTo(DiscountingTypeEnum discountingType)
        {
            var payRelativeTo = PayRelativeToEnum.CalculationPeriodEndDate;
            switch (discountingType)
            {
                case DiscountingTypeEnum.FRA:
                    payRelativeTo = PayRelativeToEnum.CalculationPeriodStartDate;
                    break;
                case DiscountingTypeEnum.Standard:
                    payRelativeTo = PayRelativeToEnum.CalculationPeriodEndDate;
                    break;
            }
            return payRelativeTo;
        }

        /// <summary>
        /// Gets the unadjusted payment date.
        /// </summary>
        /// <param name="unadjustedStartDate">The unadjusted start date.</param>
        /// <param name="unadjustedEndDate">The unadjusted end date.</param>
        /// <param name="payRelativeTo">The pay relative to.</param>
        /// <returns></returns>
        public static DateTime GetUnadjustedPaymentDate(DateTime unadjustedStartDate, DateTime unadjustedEndDate, PayRelativeToEnum payRelativeTo)
        {
            DateTime unadjustedPaymentDate;
            switch (payRelativeTo)
            {
                case PayRelativeToEnum.CalculationPeriodStartDate:
                    unadjustedPaymentDate = unadjustedStartDate;
                    break;
                case PayRelativeToEnum.CalculationPeriodEndDate:
                    unadjustedPaymentDate = unadjustedEndDate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{payRelativeTo} Payment relative not supported!");
            }
            return unadjustedPaymentDate;
        }

        ///<summary>
        ///</summary>
        ///<param name="unadjustedStartDate"></param>
        ///<param name="unadjustedEndDate"></param>
        ///<param name="discountingType"></param>
        ///<param name="dateAdjustmentConvention"></param>
        ///<param name="businessCenters"></param>
        ///<returns></returns>
        public static AdjustableDate ToAdjustableDate(DateTime unadjustedStartDate, DateTime unadjustedEndDate, 
            DiscountingTypeEnum discountingType, string dateAdjustmentConvention, string businessCenters)
        {
        return ToAdjustableDate(GetUnadjustedPaymentDate(unadjustedStartDate, unadjustedEndDate,
                                                 GetPaymentRelativeTo(discountingType)),
        dateAdjustmentConvention, businessCenters);
        }
    }
}