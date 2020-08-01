using Highlander.Constants;
using System;

namespace Highlander.Web.API.V5r3.Models
{
    public class PropertyTradeViewModel
    {
        public string TradeId { get; set; }
        public string Purchaser { get; set; }
        public string Seller { get; set; }
        public DateTime TradeTimeUtc { get; set; }
        public DateTime EffectiveTimeUtc { get; set; }
        public decimal PurchaseAmount { get; set; }
        public DateTime PaymentTimeUtc { get; set; }
        public string PropertyType { get; set; }
        public string Currency { get; set; }
        public string PropertyId { get; set; }
        public string TradingBook { get; set; }
    }
}