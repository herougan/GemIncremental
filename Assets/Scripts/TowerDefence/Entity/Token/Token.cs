using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace TowerDefence.Entity.Token
{
	// === Token ===
	public interface IToken
	{
		// Properties
		int Number { get; }
		TokenType Type { get; }
		public event Action<IToken, int> OnTokenChanged;

		// Methods
		void Add(int number);
		void Remove(int number);
		bool Enough(int number);
		void SetType(TokenType type);
	}

	public class Token : IToken
	{
		// Properties
		public int Number { get; private set; }
		public TokenType Type { get; private set; }
		public event Action<IToken, int> OnTokenChanged = delegate { };

		// Constructor
		public Token(TokenType type, int number = 0)
		{
			Type = type;
			Number = number;
		}

		// Method
		public void Add(int number)
		{
			Number += number;
			OnTokenChanged?.Invoke(this, number);
		}

		public void Remove(int number)
		{
			Number -= number;
			OnTokenChanged?.Invoke(this, -number);
		}

		public bool Enough(int number)
		{
			return (Number >= number);
		}

		public void SetType(TokenType type)
		{
			Type = type;
		}
	}

	public enum TokenType
	{
		Spores,
	}

	// === Inventory ===

	public interface IEntityInventory<T>
	{
		List<T> Things { get; }

		public void Init(List<T> things);
		public T Get(T _);
		public void Add(T _);
		public void Remove(T _);
		public void Sort();
	}

	public class TokenInventory : IEntityInventory<IToken>
	{
		#region Preamble
		public List<IToken> Things { get; } = new List<IToken>();
		public event Action<IToken, int> OnTokenChanged;
		public event Action<IToken, IToken, int> OnTokenTransmute; // From, to
		public event Action<IToken, IToken> OnTokenExchange; // From, to
		public event Action<IToken> OnNewToken;

		public void Init(List<IToken> things)
		{
			// Clear and re-add
			Things.Clear();
			Things.AddRange(things);
			// Wire events
			foreach (var token in Things)
			{
				token.OnTokenChanged += (t, n) => OnTokenChanged?.Invoke(t, n);
			}
		}

		public void RegisterCallbacks()
		{
			// Reset
			OnTokenChanged = null;
			OnTokenTransmute = null;
			OnTokenExchange = null;
			OnNewToken = null;

			// Add
			foreach (var token in Things)
			{
				token.OnTokenChanged += (t, n) => OnTokenChanged?.Invoke(t, n);
			}
		}

		#endregion Preamble

		#region Method

		// Methods

		public void Sort() { }

		public IToken Get(IToken token)
		{
			return Things.Find(t => t.Type == token.Type);
		}

		public IToken Get(TokenType type)
		{
			return Things.Find(t => t.Type == type);
		}

		public IToken Get(int index)
		{
			return Things[index];
		}

		public void Add(IToken token)
		{
			var existing = Get(token);

			// If not exist, add new
			if (existing == null)
			{
				Things.Add(new Token(token.Type, token.Number));
				OnNewToken?.Invoke(token);
				return;
			}
			// Else, add number
			Get(token).Add(token.Number);
		}

		public void Remove(IToken token)
		{
			var existing = Get(token);
			existing?.Remove(token.Number);
		}

		public bool Enough(IToken token)
		{
			return Enough(token.Type, token.Number);
		}

		public bool Enough(TokenType type, int number)
		{
			return Get(type).Enough(number);
		}
		#endregion Method

		#region Transmute / Exchange
		public void Transmute(IToken from, IToken to)
		{
			Get(to).Add(from.Number);
			OnTokenTransmute?.Invoke(from, to, from.Number);
			Remove(from);
		}

		public void Exchange(IToken from, IToken to)
		{
			TokenType _type = from.Type;

			// Swap numbers
			from.SetType(to.Type);
			to.SetType(_type);

			OnTokenExchange?.Invoke(from, to);
		}

		#endregion Transmute / Exchange

	}
}