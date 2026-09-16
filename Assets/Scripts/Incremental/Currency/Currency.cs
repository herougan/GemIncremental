using System;
using Util.Maths;

namespace Incremental.Currency
{
	/// <summary>
	/// Currency is a fungible resource used for transactions. Amount is ddouble, not double - see
	/// CLAUDE.md/MathsLib.cs: gold is intended to scale enormously (copper -> silver -> gold at 1e6 each,
	/// eventually toward 1e10,000), exactly the range ddouble exists for. Barely used anywhere yet
	/// (Item.Cost is the only other reference) before this, so converting from the original double was
	/// low-risk - this is what actually wires a Currency into real gameplay for the first time (Player.
	/// Gold, awarded on Monster death - see GameManager.HandleEntityEvent).
	/// </summary>
	public class Currency
	{
		public string Name { get; private set; }
		public string Symbol { get; private set; }
		public ddouble Amount { get; private set; }

		public Currency(string name, string symbol, ddouble amount = default(ddouble))
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Currency name cannot be null or empty", nameof(name));
			}
			if (string.IsNullOrEmpty(symbol))
			{
				throw new ArgumentException("Currency symbol cannot be null or empty", nameof(symbol));
			}
			if ((double)amount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative");
			}
			Name = name;
			Symbol = symbol;
			Amount = amount;
		}

		public void Add(ddouble amount)
		{
			if ((double)amount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount to add cannot be negative");
			}
			Amount += amount;
		}

		public bool Subtract(ddouble amount)
		{
			if ((double)amount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount to subtract cannot be negative");
			}
			if ((double)Amount >= (double)amount)
			{
				Amount -= amount;
				return true;
			}
			return false; // Not enough currency
		}

		/// <summary>Human-facing display - see ddouble.PrettyPrint (plain+commas below a million, K/M/B/T/... above).</summary>
		public string PrettyPrint() => Amount.PrettyPrint();

		public override string ToString()
		{
			return $"{Amount} {Symbol} ({Name})";
		}
	}
}
