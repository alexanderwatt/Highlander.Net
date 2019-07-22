/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/


namespace Orion.Analytics.Rates
{
    ///<summary>
    ///</summary>
    public class CrossCurrencyIRSwap
    {
        #region Private Data Member

        private readonly PaymentStream _payerPaymentStream;
        private readonly PaymentStream _receiverPaymentStream;
        private readonly decimal _spotExchangeRate;
        private readonly PaymentStream.Direction _spotExchangeRateBase;

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
