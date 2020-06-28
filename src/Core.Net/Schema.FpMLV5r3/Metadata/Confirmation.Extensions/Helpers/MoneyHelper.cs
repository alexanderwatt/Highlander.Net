using System;
using System.Collections.Generic;
using System.Linq;


namespace nab.QDS.FpML.V47
{
    public class MoneyHelper
    {
        public static   string ToString(Money money)
        {
            string result = String.Format("{0} {1}", money.amount, money.currency.Value);
            return result;
        }

        public static Money Clone(Money original)
        {
            var cloned = new Money {amount = original.amount};
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
            return cloned;
        }

        public static List<Money> Clone(List<Money> originalList)
        {
            return originalList.Select(Clone).ToList();
        }

        public static Money Clone(Money original, Currency currencyToClone)
        {
            var amount = GetZeroAmount(currencyToClone.Value);
            //var cloned = new Money { amount = original.amount};
            if (null != original.currency)
            {
                if (original.currency.Value == currencyToClone.Value)
                {
                    amount.amount = original.amount;
                }
            }
            amount.id = original.id;
            return amount;
        }

        public static Money Normalise(Money originalMoney, bool isPayerBase)
        {
            var money = new Money {currency = originalMoney.currency};
            money.amount = System.Math.Abs(money.amount);
            if (isPayerBase)
            {
                money.amount = -1*money.amount;
            }
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
            return money;
        }

        public static List<Money> Clone(List<Money> originalList, Currency currencyToClone)
        {
            return originalList.Select(original => Clone(original, currencyToClone)).ToList();
        }

        public static Money Sum(List<Money> listOfMoney, Currency currencyToAdd)
        {
            if (0 == listOfMoney.Count)
            {
                //throw new ArgumentOutOfRangeException("listOfMoney", "","listOfMoney cannot be empty list");
                return GetZeroAmount(currencyToAdd);
            }
            if (1 == listOfMoney.Count)
            {
                Money firstElement = Clone(listOfMoney[0], currencyToAdd);
                return firstElement;
            }
            else//two or more elements
            {
                // clone collection internally - just to keep invariant of the method.
                //  
                List<Money> clonedCollection = Clone(listOfMoney, currencyToAdd);
                Money firstElement = clonedCollection[0];
                clonedCollection.RemoveAt(0);
                Money sumOfTheTail = Sum(clonedCollection, currencyToAdd);
                return Add(firstElement, sumOfTheTail, currencyToAdd);
            }
        }

        public static Money Sum(List<Money> listOfMoney)
        {
            if (0 == listOfMoney.Count)
            {
                //throw new ArgumentOutOfRangeException("listOfMoney", "","listOfMoney cannot be empty list");
                return GetZeroAmount();
            }
            if (1 == listOfMoney.Count)
            {
                Money firstElement = Clone(listOfMoney[0]);

                return firstElement;
            }
            else//two or more elements
            {
                // clone collection internally - just to keep invariant of the method.
                //  
                List<Money> clonedCollection = Clone(listOfMoney);
                Money firstElement = clonedCollection[0];
                clonedCollection.RemoveAt(0);
                Money sumOfTheTail = Sum(clonedCollection);
                return Add(firstElement, sumOfTheTail);               
            }
        }

        public static Money Neg(Money money)
        {
            var amount = Convert.ToDecimal(money.amount);
            var temp = -1 * amount;
            var result = new Money { currency = money.currency, amount = temp };
            return result;
        }

        public static Money Mul(Money money, double d)
        {
            var result = new Money {currency = money.currency, amount = money.amount*(decimal) d};
            return result;
        }

        public static Money Mul(Money money, params double[] doubles)
        {
            var result = new Money {currency = money.currency, amount = money.amount};
            return doubles.Aggregate(result, Mul);
        }

        public static Money Mul(Money money, decimal d)
        {
            var result = new Money {currency = money.currency, amount = money.amount*d};
            return result;
        }

        public static Money Mul(Money money, bool payerIsBase)
        {
            var multiplier = 1;
            if(payerIsBase)
            {
                multiplier = -1;
            }
            var result = new Money { currency = money.currency, amount = money.amount * multiplier };
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
            var result = new Money { currency = ccy, amount = amount * multiplier };
            return result;
        }

        public static Money Div(Money money, double d)
        {
            var result = new Money {currency = money.currency, amount = money.amount/(decimal) d};
            return result;
        }

        public static Money Add(Money money1, Money money2, Currency currencyToAdd)
        {
            if (currencyToAdd.Value == null)
            {
                throw new ArgumentException(String.Format("Error: currencies value is not supplied. Currency 1 : {0}", currencyToAdd.Value));
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
                (!String.IsNullOrEmpty(money1.currency.Value)) &
                (!String.IsNullOrEmpty(money2.currency.Value))
                )
            {
                if (money1.currency.Value != money2.currency.Value)
                {
                    throw new ArgumentException(String.Format("Error: currencies are not the same. Currency 1 : {0}, Currency 2: {1}", money1.currency.Value, money2.currency.Value));
                }
            }
            result.currency = money1.currency;
            result.amount = money1.amount + money2.amount;
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

        private const string _currencyNotSpecified = "CURRENCY NOT SPECIFIED";

        public  static  Money   GetAmount(double amount)
        {
            return GetAmount(amount, _currencyNotSpecified);
        }

        public  static  Money   GetAmount(decimal amount)
        {
            return GetAmount(amount, _currencyNotSpecified);
        }

        public static Money GetAmount(double amount, string currencyAsString)
        {
            var currency = new Currency {Value = currencyAsString};
            var money = new Money {currency = currency, amount = (decimal) amount};
            return money;
        }

        public static Money GetAmount(double amount, Currency currency)
        {
            var money = new Money { currency = currency, amount = (decimal)amount };
            return money;
        }

        public static Money GetAmount(decimal amount, string currencyAsString)
        {
            var currency = new Currency {Value = currencyAsString};
            var money = new Money {currency = currency, amount = amount};
            return money;
        }


        public static Money GetAmount(decimal amount, Currency currency)
        {
            var money = new Money {currency = currency, amount = amount};
            return money;
        }
    }

}
