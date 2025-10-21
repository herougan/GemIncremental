using System;

namespace Incremental.Currency
{
	/// <summary>
	/// Currency is a fungible resource used for transactions.
	/// </summary>
	public class Currency
	{
		public string Name { get; private set; }
		public string Symbol { get; private set; }
		public double Amount { get; private set; }

		public Currency(string name, string symbol, double amount = 0)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Currency name cannot be null or empty", nameof(name));
			}
			if (string.IsNullOrEmpty(symbol))
			{
				throw new ArgumentException("Currency symbol cannot be null or empty", nameof(symbol));
			}
			if (amount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative");
			}
			Name = name;
			Symbol = symbol;
			Amount = amount;
		}

		public void Add(double amount)
		{
			if (amount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount to add cannot be negative");
			}
			Amount += amount;
		}

		public bool Subtract(double amount)
		{
			if (amount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount to subtract cannot be negative");
			}
			if (Amount >= amount)
			{
				Amount -= amount;
				return true;
			}
			return false; // Not enough currency
		}

		public override string ToString()
		{
			return $"{Amount} {Symbol} ({Name})";
		}
	}
}