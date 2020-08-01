using System;
using System.ComponentModel.DataAnnotations;

namespace Highlander.Web.API.V5r3.Models
{
    public class LeaseTradeViewModel
    {
        public string Id { get; set; }
        public string Tenant { get; set; }
        public string Owner { get; set; }
        public string RollConvention { get; set; }
        public string BusinessCalendar { get; set; }
        public string BusinessDayAdjustments { get; set; }
        public decimal? UpfrontAmount { get; set; }
        public string LeaseType { get; set; }
        public decimal? LeaseArea { get; set; }
        public string LeaseAreaUnits { get; set; }
        public string ReviewFrequency { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime LeaseStartDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public DateTime LeaseExpiryDate { get; set; }
        public decimal? ReviewAmountUpDown { get; set; }
        public string Currency { get; set; }
        public string Portfolio { get; set; }
        public decimal StartGrossAmount { get; set; }
        public string PaymentFrequency { get; set; }
        public string LeaseId { get; set; }
        public string ReferencePropertyIdentifier { get; set; }
        public string Description { get; set; }
    }
}