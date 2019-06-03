using System;
using System.Collections.Generic;


namespace Orion.Analytics.Rates
{
    ///<summary>
    ///</summary>
    public class CrossCurrencyIRSwap
    {
        #region Private Data Member

        private PaymentStream _payerPaymentStream;
        private PaymentStream _receiverPaymentStream;
        private decimal _spotExchangeRate;
        private PaymentStream.Direction _spotExchangeRateBase;

        #endregion

        #region Constructors


        public CrossCurrencyIRSwap(PaymentStream payerPaymentStream, PaymentStream receiverPaymentStream,
                                   decimal spotExchangeRate, PaymentStream.Direction direction)
        {
            _payerPaymentStream = payerPaymentStream;
            _receiverPaymentStream = receiverPaymentStream;
            _spotExchangeRate = spotExchangeRate;
            _spotExchangeRateBase = direction;
        }



        #endregion

        #region Public Methods

        public decimal SpotExchangeRate(PaymentStream.Direction conversionBase)
        {
            if (conversionBase == _payerPaymentStream.Dir)
            {
                return _receiverPaymentStream.PrincipleExchange / _payerPaymentStream.PrincipleExchange; 
            }

            return _payerPaymentStream.PrincipleExchange / _receiverPaymentStream.PrincipleExchange;

        }

        public decimal getSpotExchangeRate(PaymentStream.Direction direction)
        {
            if (direction == _spotExchangeRateBase)
                return _spotExchangeRate;

            return 1.0m / _spotExchangeRate;

        }

        public decimal BreakEvenRate(PaymentStream.Direction direction)
        {
            
            if (direction == _payerPaymentStream.Dir)
            {
                if (_payerPaymentStream.GetType() == typeof(FixedPaymentStream))
                {

                    return (((_receiverPaymentStream.NPV() / getSpotExchangeRate(direction)) - _payerPaymentStream.PrincipleExchangePV()) / _payerPaymentStream.AccruedNPVOfNotional()) * -1.0m;
                }
                //else
                //{
                //      find the spread over floating rate
                //} 

            }
            else
            {
                if (_receiverPaymentStream.GetType() == typeof(FixedPaymentStream))
                {
                    return (((_payerPaymentStream.NPV() / getSpotExchangeRate(direction)) - _receiverPaymentStream.PrincipleExchangePV()) / _receiverPaymentStream.AccruedNPVOfNotional()) * -1.0m;   

                }
                //else
                //{
                //      find the spread over floating rate
                //} 


            } 
            return 0.0m;
        }



        #endregion

    }
}
