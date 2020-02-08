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
using System.Collections.Generic;
using System.Linq;
using Highlander.Reporting.V5r3;


namespace Highlander.Reporting.Helpers.V5r3
{
    public class MoneyHelper
    {
        public static string ToString(Money money)
        {
            string result = $"{money.amount} {money.currency.Value}";
            return result;
        }

        public static Money CopyFromNonNegativeToMoney(NonNegativeMoney original)
        {
            var result = CopyToMoney(original);
            return result;
        }

        public static Money CopyToMoney(MoneyBase original)
        {
            var cloned = new Money();
            if (original is Money money)
            {
                cloned.amount = money.amount;
            }
            else if (original is NonNegativeMoney negativeMoney)
            {
                cloned.amount = negativeMoney.amount;
            }
            else if (original is PositiveMoney positiveMoney)
            {
                cloned.amount = positiveMoney.amount;
            }
            if (null != original.currency)
            {
                cloned.currency = new Currency();
                if (null != original.currency.currencyScheme)
                {
                    cloned.currency.currencyScheme = original.currency.currencyScheme;
                }
                if (null != original.currency.Value)
                {
                    cloned.currency.Value = original.currency.Value;
                }
            }
            cloned.id = original.id;
            cloned.amountSpecified = true;
            return cloned;
        }

        public static List<Money> CopyToMoney(List<MoneyBase> originalList)
        {
            return originalList.Select(CopyToMoney).ToList();
        }

        /// <summary>
        /// THis will only clone: PositiveMoney, NonNegativeMoney and Money types.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="currencyToClone"></param>
        /// <returns></returns>
        public static Money CopyToMoney(MoneyBase original, Currency currencyToClone)
        {
            var amount = GetZeroAmount(currencyToClone.Value);
            // || original as PositiveMoney != null || original as NonNegativeMoney != null
            if (null != original.currency)
            {
                if (original.currency.Value == currencyToClone.Value)
                {
                    if (original is Money money)
                    {
                        amount.amount = money.amount;
                    }
                    else if (original is NonNegativeMoney negativeMoney)
                    {
                        amount.amount = negativeMoney.amount;
                    }
                    else if (original is PositiveMoney positiveMoney)
                    {
                        amount.amount = positiveMoney.amount;
                    }
                }
            }
            amount.id = original.id;
            amount.amountSpecified = true;
            return amount;
        }

        public static Money Normalise(MoneyBase originalMoney, bool isPayerBase)
        {
            var money = CopyToMoney(originalMoney);//TODO this does nothing
            //money.amount = System.Math.Abs(money.amount);
            if (isPayerBase)
            {
                money.amount = -1*money.amount;
            }
            money.amountSpecified = true;
            return money;
        }

        public static Money Normalise(string currency, decimal amount)
        {
            return Normalise(currency, amount, false);
        }

        public static Money Normalise(string currency, decimal amount, bool isPayerBase)
        {
            var money = new Money { currency = CurrencyHelper.Parse(currency) };
            money.amount = System.Math.Abs(money.amount);
            if (isPayerBase)
            {
                money.amount = -1 * money.amount;
            }
            money.amountSpecified = true;
            return money;
        }

        public static List<Money> CopyToMoney(List<MoneyBase> originalList, Currency currencyToClone)
        {
            return originalList.Select(original => CopyToMoney(original, currencyToClone)).ToList();
        }

        public static Money Sum(List<Money> listOfMoney, Currency currencyToAdd)
        {
            var moneyBaseList = listOfMoney.Cast<MoneyBase>().ToList();
            return Sum(moneyBaseList, currencyToAdd);
        }

        public static Money Sum(List<MoneyBase> listOfMoney, Currency currencyToAdd)
        {
            if (0 == listOfMoney.Count)
            {
                //throw new ArgumentOutOfRangeException("listOfMoney", "","listOfMoney cannot be empty list");
                return GetZeroAmount(currencyToAdd);
            }
            if (1 == listOfMoney.Count)
            {
                Money firstElement = CopyToMoney(listOfMoney[0], currencyToAdd);
                return firstElement;
            }
            else//two or more elements
            {
                // clone collection internally - just to keep invariant of the method.
                //  
                var clonedCollection = CopyToMoney(listOfMoney, currencyToAdd);
                var firstElement = clonedCollection[0];
                clonedCollection.RemoveAt(0);
                var sumOfTheTail = Sum(clonedCollection, currencyToAdd);
                return Add(firstElement, sumOfTheTail, currencyToAdd);
            }
        }

        public static Money Sum(List<Money> listOfMoney)
        {
            var moneyBaseList = listOfMoney.Cast<MoneyBase>().ToList();
            return Sum(moneyBaseList);
        }

        public static Money Sum(List<MoneyBase> listOfMoney)
        {
            if (0 == listOfMoney.Count)
            {
                //throw new ArgumentOutOfRangeException("listOfMoney", "","listOfMoney cannot be empty list");
                return GetZeroAmount();
            }
            if (1 == listOfMoney.Count)
            {
                Money firstElement = CopyToMoney(listOfMoney[0]);
                return firstElement;
            }
            else//two or more elements
            {
                // clone collection internally - just to keep invariant of the method.
                //  
                var clonedCollection = CopyToMoney(listOfMoney);
                var firstElement = clonedCollection[0];
                clonedCollection.RemoveAt(0);
                var sumOfTheTail = Sum(clonedCollection);
                return Add(firstElement, sumOfTheTail);               
            }
        }

        public static Money Neg(Money money)
        {
            var amount = Convert.ToDecimal(money.amount);
            var temp = -1 * amount;
            var result = new Money { currency = money.currency, amount = temp, amountSpecified = true };
            return result;
        }

        public static Money Neg(PositiveMoney money)
        {
            var amount = Convert.ToDecimal(money.amount);
            var temp = -1 * amount;
            var result = new Money { currency = money.currency, amount = temp, amountSpecified = true };
            return result;
        }

        public static Money Neg(NonNegativeMoney money)
        {
            var amount = Convert.ToDecimal(money.amount);
            var temp = -1 * amount;
            var result = new Money { currency = money.currency, amount = temp, amountSpecified = true };
            return result;
        }

        public static Money Mul(Money money, double d)
        {
            var result = new Money { currency = money.currency, amount = money.amount * (decimal)d, amountSpecified = true };
            return result;
        }

        public static Money Mul(Money money, params double[] doubles)
        {
            var result = new Money { currency = money.currency, amount = money.amount, amountSpecified = true };
            return doubles.Aggregate(result, Mul);
        }

        public static Money Mul(Money money, decimal d)
        {
            var result = new Money {currency = money.currency, amount = money.amount*d, amountSpecified = true};
            return result;
        }

        public static Money Mul(NonNegativeMoney money, decimal d)
        {
            var result = new Money {currency = money.currency, amount = money.amount*d, amountSpecified = true};
            return result;
        }

        public static Money Mul(Money money, bool payerIsBase)
        {
            var multiplier = 1;
            if(payerIsBase)
            {
                multiplier = -1;
            }
            var result = new Money {currency = money.currency, amount = money.amount*multiplier, amountSpecified = true};
            return result;
        }

        public static Money Mul(decimal amount, string  currency, bool payerIsBase)
        {
            var multiplier = 1;
            if (payerIsBase)
            {
                multiplier = -1;
            }
            var ccy = new Currency {Value = currency};
            var result = new Money {currency = ccy, amount = amount*multiplier, amountSpecified = true};
            return result;
        }

        public static Money Div(Money money, double d)
        {
            var result = new Money
                {
                    currency = money.currency,
                    amount = money.amount/(decimal) d,
                    amountSpecified = true
                };
            return result;
        }

        public static Money Add(Money money1, Money money2, Currency currencyToAdd)
        {
            if (currencyToAdd.Value == null)
            {
                throw new ArgumentException(
                    $"Error: currencies value is not supplied. Currency 1 : {currencyToAdd.Value}");
            }
            var result = new Money {currency = currencyToAdd};
            decimal amount = 0.0m;
            //  If currencies are REALLY different (not just empty strings) - that should be considered as the error.
            //
            if (money1.currency.Value == currencyToAdd.Value)
            {
                if (money1.currency.Value == currencyToAdd.Value)
                {
                    amount = money1.amount;
                }
            }
            if (money2.currency.Value == currencyToAdd.Value)
            {
                if (money2.currency.Value == currencyToAdd.Value)
                {
                    amount += money1.amount;
                }
            }
            result.amount = amount;
            result.amountSpecified = true;
            return result;
        }


        public static Money Add(Money money1, Money money2)
        {
            var result = new Money();
            if (money1.amount == 0.0m && money2.amount == 0.0m)
            {
                return GetZeroAmount();
            }
            if (money1.amount == 0.0m)
            {
                return money2;
            }
            if (money2.amount == 0.0m)
            {
                return money1;
            }
            //  If currencies are REALLY different (not just empty strings) - that should be considered as the error.
            //
            if
                (
                !String.IsNullOrEmpty(money1.currency.Value) &
                !String.IsNullOrEmpty(money2.currency.Value)
                )
            {
                if (money1.currency.Value != money2.currency.Value)
                {
                    throw new ArgumentException(
                        $"Error: currencies are not the same. Currency 1 : {money1.currency.Value}, Currency 2: {money2.currency.Value}");
                }
            }
            result.currency = money1.currency;
            result.amount = money1.amount + money2.amount;
            result.amountSpecified = true;
            return result;
        }

        public static Money Sub(Money money1, Money money2)
        {
            var result = new Money();
            if (money1.amount == 0.0m && money2.amount == 0.0m)
            {
                return GetZeroAmount();
            }
            if (money1.amount == 0.0m)
            {
                return money2;
            }
            if (money2.amount == 0.0m)
            {
                return money1;
            }
            if (money1.currency.Value != money2.currency.Value)
            {
                throw new ArgumentException("Error: currencies are not the same");
            }
            result.currency = money1.currency;
            result.amount = money1.amount - money2.amount;
            result.amountSpecified = true;
            return result;
        }

        public static decimal Div(Money money1, Money money2)
        {
            if (money1.currency.Value != money2.currency.Value)
            {
                throw new ArgumentException("Error: currencies are not the same");
            }
            return money1.amount / money2.amount;
        }

        public static double ToDouble(Money money)
        {
            return (double)money.amount;            
        }

        public static decimal ToDecimal(Money money)
        {
            return money.amount;            
        }

        public  static  Money   GetZeroAmount()
        {
            return GetAmount(0.0);
        }

        public static Money GetZeroAmount(Currency currency)
        {
            return GetAmount(0.0, currency);
        }

        public static Money GetZeroAmount(string currency)
        {
            return GetAmount(0.0, currency);
        }

        private const string CurrencyNotSpecified = "CURRENCY NOT SPECIFIED";

        public static NonNegativeMoney GetNonNegativeAmount(double amount)
        {
            return GetNonNegativeAmount(amount, CurrencyNotSpecified);
        }

        public static NonNegativeMoney GetNonNegativeAmount(double amount, string currencyAsString)
        {
            var currency = new Currency { Value = currencyAsString };
            var money = new NonNegativeMoney { currency = currency, amount = System.Math.Abs((decimal)amount), amountSpecified = true };
            return money;
        }

        public static Money GetMoney(PositiveMoney positiveMoney)
        {
            var currency = new Currency { Value = positiveMoney.currency.Value };
            var money = new Money { currency = currency, amount = positiveMoney.amount, amountSpecified = true };
            return money;
        }

        public static PositiveMoney GetPositiveMoney(Money amount)
        {
            var currency = new Currency { Value = amount.currency.Value };
            var money = new PositiveMoney { currency = currency, amount = System.Math.Abs(amount.amount), amountSpecified = true };
            return money;
        }

        public static PositiveMoney GetPositiveMoney(decimal amount, string currencyAsString)
        {
            var currency = new Currency { Value = currencyAsString };
            var money = new PositiveMoney { currency = currency, amount = System.Math.Abs(amount), amountSpecified = true};
            return money;
        }

        public static NonNegativeMoney GetNonNegativeAmount(Premium premium)
        {
            return GetNonNegativeAmount(premium.pricePerOption);
        }

        public static NonNegativeMoney GetNonNegativeAmount(PositiveMoney positiveMoney)
        {
            return GetNonNegativeAmount(positiveMoney.amount, positiveMoney.currency.Value);
        }

        public static NonNegativeMoney GetNonNegativeAmount(decimal amount)
        {
            return GetNonNegativeAmount(amount, CurrencyNotSpecified);
        }

        public static NonNegativeMoney GetNonNegativeAmount(decimal amount, string currencyAsString)
        {
            var currency = new Currency { Value = currencyAsString };
            var money = new NonNegativeMoney { currency = currency, amount = System.Math.Abs(amount), amountSpecified = true };
            return money;
        }

        public static NonNegativeMoney GetNonNegativeAmount(Money generalMoney)
        {
            var currency = new Currency { Value = generalMoney.currency.Value };
            var money = new NonNegativeMoney { currency = currency, amount = System.Math.Abs(generalMoney.amount), amountSpecified = true };
            return money;
        }

        public  static  Money   GetAmount(double amount)
        {
            return GetAmount(amount, CurrencyNotSpecified);
        }

        public  static  Money   GetAmount(decimal amount)
        {
            return GetAmount(amount, CurrencyNotSpecified);
        }

        public static Money GetAmount(double amount, string currencyAsString)
        {
            var currency = new Currency {Value = currencyAsString};
            var money = new Money { currency = currency, amount = (decimal)amount, amountSpecified = true };
            return money;
        }

        public static Money GetAmount(double amount, Currency currency)
        {
            var money = new Money { currency = currency, amount = (decimal)amount, amountSpecified = true };
            return money;
        }

        public static Money GetAmount(decimal amount, string currencyAsString)
        {
            var currency = new Currency {Value = currencyAsString};
            var money = new Money { currency = currency, amount = amount, amountSpecified = true };
            return money;
        }

        public static Money GetAmount(decimal amount, Currency currency)
        {
            var money = new Money { currency = currency, amount = amount, amountSpecified = true };
            return money;
        }
    }
}
