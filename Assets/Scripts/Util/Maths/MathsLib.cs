
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Util.Maths
{
	public static class MathsLib
	{

		#region Random
		/// <summary>
		/// Returns a index pick based on a (positive) probabiltiy distribution.
		/// </summary>
		/// <param name="prob"></param>
		/// <returns></returns>
		public static int WeightedRandomPick(List<int> prob)
		{
			int t = 0;
			for (int i = 0; i < prob.Count; ++i)
			{
				t += prob[i];
			}

			int r = UnityEngine.Random.Range(0, t);
			t = 0;
			for (int i = 0; i < prob.Count; ++i)
			{
				t += prob[i];
				if (r < t) return t;
			}
			return -1;
		}

		public static List<int> RandomPickFromN(int n, int m)
		{
			List<int> vec = new List<int>(n);
			for (int i = 0; i < n; ++i)
			{
				vec.Add(i);
			}

			while (vec.Count > m)
			{
				vec.RemoveAt(UnityEngine.Random.Range(0, vec.Count));
			}

			return vec;
		}

		public static List<int> RandomWeightedPickFromN(int n, List<int> v)
		{
			List<int> vec = new List<int>(n);
			List<int> weights = new List<int>(v);
			int r = 0;

			for (int i = 0; i < weights.Count; ++i)
			{
				r = WeightedRandomPick(weights);
				vec.Add(weights[r]);
				weights.RemoveAt(r);
			}

			return vec;
		}

		public static bool RandomLessThan(double bar)
		{
			return UnityEngine.Random.Range(0.0f, 1.0f) < bar;
		}

		#endregion Random

		#region Game Space

		public static void PointTo(GameObject source, GameObject target, float speed = 0.01f)
		{
			PointTo(source, target.transform.position, speed);
		}
		public static void PointTo(GameObject source, Vector3 target, float speed = 0.01f)
		{
			float angle = Mathf.Atan2(target.y - source.transform.position.y, target.x - source.transform.position.x) * Mathf.Rad2Deg;
			Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
			source.transform.rotation = Quaternion.Slerp(source.transform.rotation, targetRotation, speed);
		}
		public static float AngleBetween(Vector2 v, Vector2 w)
		{
			return Vector2.Angle(v, w);
		}

		#endregion Game Space

		#region Operations

		/// <summary>
		/// e^x, computed directly in ddouble's mantissa/exponent form rather than through
		/// System.Math.Exp(x) - that overflows to double.PositiveInfinity once x passes ~709, which
		/// defeats the entire point of ddouble (representing numbers far beyond what a double can hold).
		/// Anything stat-related uses ddouble specifically to avoid that ceiling, so exponential scaling
		/// (e.g. EntityManager's spawn-time wave-progression multiplier) needs an Exp that doesn't
		/// reintroduce it.
		///
		/// Works by converting to base 10 instead of e (e^x = 10^(x/ln 10)), then splitting that into an
		/// integer exponent and a fractional mantissa in [1, 10) - the same normalized shape ddouble's
		/// own double-&gt;ddouble implicit conversion already produces. The intermediate double math
		/// (math.pow(10, frac)) never exceeds single digits regardless of how large x is, so it can't
		/// overflow - only the final ddouble's Exponent field grows, and that's a double itself, with a
		/// vastly larger usable range than the number it would have represented directly.
		/// </summary>
		public static ddouble Exp(double x)
		{
			double y = x / math.log(10.0); // e^x = 10^y
			double exponent = math.floor(y);
			double mantissa = math.pow(10.0, y - exponent); // in [1, 10)
			return new ddouble(mantissa, exponent);
		}

		/// <summary>
		/// log10(d), computed directly from d's mantissa/exponent - log10(mantissa * 10^exp) =
		/// log10(mantissa) + exp - rather than converting the whole ddouble to a double first (which
		/// overflows for a large-exponent ddouble, since ddouble's own implicit conversion to double
		/// computes Mantissa * 10^Exponent directly and that pow can itself overflow). The result is
		/// always a normal-sized number - taking a log shrinks scale, it doesn't need ddouble's range -
		/// but returns ddouble anyway for API consistency with everything else here.
		/// </summary>
		public static ddouble Log10(ddouble d) => math.log10(d.Mantissa) + d.Exponent;

		/// <summary>Natural log - ln(x) = log10(x) * ln(10). Same safety reasoning as Log10.</summary>
		public static ddouble Ln(ddouble d) => Log10(d) * math.log(10.0);

		/// <summary>Log to an arbitrary base - log_b(x) = log10(x) / log10(b). Same safety reasoning as Log10.</summary>
		public static ddouble Log(ddouble d, double newBase) => Log10(d) / math.log10(newBase);

		/// <summary>
		/// sin/cos/tan of a ddouble angle - thin wrappers through the existing ddouble&lt;-&gt;double
		/// conversion, not reimplemented in mantissa/exponent form like Exp/Log10 are. Deliberate: trig
		/// functions are periodic and only meaningful for normal-sized arguments in the first place -
		/// nothing should ever be calling Sin on a googol-scale ddouble angle, that's a bug at the call
		/// site, not a case worth engineering around. Not safe for arbitrarily large input the way Exp/
		/// Log10/Ln/Log are; that's a non-goal here, not an oversight.
		/// </summary>
		public static ddouble Sin(ddouble angle) => math.sin((double)angle);
		public static ddouble Cos(ddouble angle) => math.cos((double)angle);
		public static ddouble Tan(ddouble angle) => math.tan((double)angle);

		public static double Operate(double a, double b, MathOperation op)
		{
			switch (op)
			{
				case MathOperation.Multiply:
					return a * b;
				case MathOperation.Add:
					return a + b;
				case MathOperation.Exponent:
					return Mathf.Pow((float)a, (float)b);
				case MathOperation.Logarithm:
					return Mathf.Log((float)a, (float)b);
				case MathOperation.Max:
					return Mathf.Max((float)a, (float)b);
				case MathOperation.Min:
					return Mathf.Min((float)a, (float)b);
				case MathOperation.Equal:
					return a == b ? 1 : 0;
				case MathOperation.NotEqual:
					return a != b ? 1 : 0;
				case MathOperation.Greater:
					return a > b ? 1 : 0;
				case MathOperation.Lesser:
					return a < b ? 1 : 0;
				case MathOperation.Nothing:
					return a; // b does nothing happens to a.
				default:
					throw new System.Exception("Invalid operation");
			}
		}

		public static double Mean(List<double> values, MathJoin join)
		{
			double sum = 0;

			switch (join)
			{
				case MathJoin.Arithmetic:
					values.ForEach(x => sum += x);
					return sum / values.Count;
				case MathJoin.Product:
					sum = 1;
					values.ForEach(x => sum *= x);
					return sum;
				case MathJoin.Geometric:
					sum = 1;
					values.ForEach(x => sum *= x);
					return Mathf.Pow((float)sum, 1.0f / (float)values.Count);
				case MathJoin.Harmonic:
					sum = 0;
					values.ForEach(x => sum += 1.0f / x);
					return values.Count / sum;
				case MathJoin.Power:
					sum = 0;
					foreach (var (x, i) in values.Select((x, i) => (x, i)))
					{
						sum += Mathf.Pow((float)x, i);
					}
					return sum / values.Count;
				case MathJoin.Alternating:
					sum = 0;
					foreach (var (x, i) in values.Select((x, i) => (x, i)))
					{
						sum += (i % 2 == 0 ? x : -x);
					}

					return sum;
				case MathJoin.AlternatingPower:
					sum = 0;
					foreach (var (x, i) in values.Select((x, i) => (x, i)))
					{
						sum += (i % 2 == 0 ? x : -x) * Mathf.Pow((float)x, i);
					}
					return sum;
				default:
					throw new System.Exception("Invalid operation");
			}
		}

		public static bool Compare(double a, double b, MathOperation comparative)
		{
			switch (comparative)
			{
				case MathOperation.Equal:
					return a == b;
				case MathOperation.Greater:
					return a > b;
				case MathOperation.Lesser:
					return a < b;
				case MathOperation.Geq:
					return a >= b;
				case MathOperation.Leq:
					return a <= b;
				case MathOperation.NotEqual:
					return a != b;
				case MathOperation.AbsGreater:
					return math.abs(a) > math.abs(b); // Maths.Abs(b) >= b
				case MathOperation.AbsLesser:
					return math.abs(a) < math.abs(b);
				case MathOperation.Ageq:
					return math.abs(a) >= math.abs(b);
				case MathOperation.Aleq:
					return math.abs(a) <= math.abs(b);
				default:
					throw new System.Exception("Invalid operation");
			}
		}

		public static bool Compare(ddouble a, ddouble b, MathOperation comparative)
		{
			switch (comparative)
			{
				case MathOperation.Equal:
					return a == b;
				case MathOperation.Greater:
					return a > b;
				case MathOperation.Lesser:
					return a < b;
				case MathOperation.Geq:
					return a >= b;
				case MathOperation.Leq:
					return a <= b;
				case MathOperation.NotEqual:
					return a != b;
				case MathOperation.AbsGreater:
					return math.abs(a) > math.abs(b); // Maths.Abs(b) >= b
				case MathOperation.AbsLesser:
					return math.abs(a) < math.abs(b);
				case MathOperation.Ageq:
					return math.abs(a) >= math.abs(b);
				case MathOperation.Aleq:
					return math.abs(a) <= math.abs(b);
				default:
					throw new System.Exception("Invalid operation");
			}
		}

		public static bool Compare(float a, float b, MathOperation comparative)
		{
			return Compare((double)a, (double)b, comparative);
		}

		public static bool Compare(int a, int b, MathOperation comparative)
		{
			switch (comparative)
			{
				case MathOperation.Equal:
					return a == b;
				case MathOperation.Greater:
					return a > b;
				case MathOperation.Lesser:
					return a < b;
				case MathOperation.Geq:
					return a >= b;
				case MathOperation.Leq:
					return a <= b;
				case MathOperation.NotEqual:
					return a != b;
				case MathOperation.AbsGreater:
					return math.abs(a) > math.abs(b);
				case MathOperation.AbsLesser:
					return math.abs(a) < math.abs(b);
				case MathOperation.Ageq:
					return math.abs(a) >= math.abs(b);
				case MathOperation.Aleq:
					return math.abs(a) <= math.abs(b);
				default:
					throw new System.Exception("Invalid operation");
			}
		}

		#endregion Operations

		#region Util

		public static bool IsPositive(MathOperation op)
		{
			if (op == MathOperation.Add) return true;
			if (op == MathOperation.Exponent) return true;
			if (op == MathOperation.Multiply) return true;

			return false;
		}

		#endregion Util
	}

	#region Objects and Interfaces
	// ===== Objects =====
	[System.Serializable]
	public struct ddouble
	{
		// ===== Fields =====
		public double Mantissa;
		public double /*BigInteger*/ Exponent;
		public int Precision;

		// ===== Constructors =====
		public ddouble(double mantissa = 0, double exponent = 0, int precision = 10)
		{
			Mantissa = mantissa;
			Exponent = exponent;
			Precision = precision;
		}
		public ddouble(int value)
		{
			Exponent = math.log10(value);
			Mantissa = value / math.pow(10, Exponent);
			Precision = 10; // Default precision
		}
		// Implicit
		public override string ToString()
		{
			return $"{Mantissa:F3}e{Exponent}";
		}
		public static implicit operator ddouble(double d)
		{
			double Exponent = math.log10(d);
			return new ddouble(d / math.pow(10, Exponent), Exponent);
		}
		public static implicit operator ddouble(int i)
		{
			double Exponent = math.log10(i);
			return new ddouble(i / math.pow(10, Exponent), Exponent);
		}
		public static implicit operator double(ddouble d)
		{
			double exponent = math.log10(d.Mantissa) + d.Exponent;
			if (exponent < 0) return d.Mantissa * math.pow(10, exponent);
			else return d.Mantissa * math.pow(10, exponent);
		}
		public static implicit operator string(ddouble d)
		{
			return d.ToString();
		}
		public static bool operator ==(ddouble a, ddouble b)
		{
			return a.Exponent == b.Exponent
			&& math.abs(a.Mantissa - b.Mantissa) < math.pow(0.1f, math.max(a.Precision, b.Precision));
		}
		public static bool operator !=(ddouble a, ddouble b)
		{
			return !(a == b);
		}
		public override bool Equals(object obj)
		{
			if (!(obj is ddouble)) return false;
			return this == (ddouble)obj;
		}
		public override int GetHashCode()
		{
			return (Mantissa.GetHashCode() * 397) ^ Exponent.GetHashCode();
		}

		// ===== Operators =====
		public static ddouble operator *(ddouble a, ddouble b)
		{
			if (a.Mantissa * b.Mantissa > 10) { a.Mantissa /= 10; a.Exponent += 1; }
			else if (a.Mantissa * b.Mantissa < 1) { a.Mantissa *= 10; a.Exponent -= 1; }
			return new ddouble(a.Mantissa * b.Mantissa, a.Exponent + b.Exponent);
		}
		public static ddouble operator /(ddouble a, ddouble b)
		{
			if (a.Mantissa / b.Mantissa < 10) { a.Mantissa *= 10; a.Exponent -= 1; }
			return new ddouble(a.Mantissa / b.Mantissa, a.Exponent - b.Exponent);
		}
		public static ddouble operator -(ddouble a)
		{
			return new ddouble(-a.Mantissa, a.Exponent);
		}
		public static ddouble operator *(ddouble a, double b)
		{
			ddouble _b = b;
			return a * _b;
		}

		public static ddouble operator +(ddouble a, ddouble b)
		{
			if (a.Exponent == b.Exponent)
			{
				return new ddouble(a.Mantissa + b.Mantissa, a.Exponent);
			}
			else if (a.Exponent > b.Exponent)
			{
				double scale = math.pow(10, b.Exponent - a.Exponent);
				// Optimisation
				if (scale > math.max(a.Precision, b.Precision)) return b;
				return new ddouble(a.Mantissa + b.Mantissa * scale, a.Exponent);
			}
			else
			{
				double scale = math.pow(10, a.Exponent - b.Exponent);
				// Optimisation
				if (scale > math.max(a.Precision, b.Precision)) return a;
				return new ddouble(a.Mantissa * scale + b.Mantissa, b.Exponent);
			}
		}
		public static ddouble operator -(ddouble a, ddouble b)
		{
			if (a.Exponent == b.Exponent)
			{
				return new ddouble(a.Mantissa - b.Mantissa, a.Exponent);
			}
			else if (a.Exponent > b.Exponent)
			{
				double scale = math.pow(10, b.Exponent - a.Exponent);
				// Optimisation
				if (scale > math.max(a.Precision, b.Precision)) { return -b; }
				return new ddouble(a.Mantissa - b.Mantissa * scale, a.Exponent);
			}
			else
			{
				double scale = math.pow(10, a.Exponent - b.Exponent);
				// Optimisation
				if (scale > math.max(a.Precision, b.Precision)) return a;
				return new ddouble(a.Mantissa * scale - b.Mantissa, b.Exponent);
			}
		}
		public static ddouble operator %(ddouble a, ddouble b) // Modulus
		{
			double scale = math.pow(10, a.Exponent - b.Exponent);
			double mantissaMod = a.Mantissa * scale % b.Mantissa;
			return new ddouble(mantissaMod / scale, b.Exponent);
		}
		public static ddouble operator &(ddouble a, ddouble b) // Quotient (integer division)
		{
			double scale = math.pow(10, a.Exponent - b.Exponent);
			double mantissaQuotient = math.floor(a.Mantissa * scale / b.Mantissa);
			return new ddouble(mantissaQuotient / scale, b.Exponent);
		}

		/// <summary>
		/// a^b, using b's *full* value (Mantissa AND Exponent) - the previous implementation used only
		/// b.Mantissa (always in [1, 10)), silently discarding b.Exponent, so raising to any power of 10
		/// or more (25, 100, ...) gave the wrong answer even though nothing was astronomically large.
		///
		/// Computed via logs so a huge base (a up to ~1e100+) never overflows: log10(a^b) = b * log10(a).
		/// log10(a.Mantissa) + a.Exponent is always a normal-sized double even for a huge a (see
		/// MathsLib.Log10's own reasoning), and b converted to a plain double is safe too since a stat
		/// mod's own exponent argument realistically never needs to be astronomically large itself (only
		/// the base/result do) - same tradeoff MathsLib.Sin/Cos/Tan make deliberately. The result's
		/// exponent (b * log10(a)) can itself be large (e.g. base 1e100 squared -> 200) - that's still
		/// just a plain double, comfortably within range, and splitting it into mantissa+exponent the
		/// same way Exp does keeps the final ddouble's own Exponent field as the only thing that grows.
		/// </summary>
		public static ddouble operator ^(ddouble a, ddouble b)
		{
			double log10A = math.log10(a.Mantissa) + a.Exponent;
			double resultLog10 = log10A * (double)b;

			double newExponent = math.floor(resultLog10);
			double newMantissa = math.pow(10.0, resultLog10 - newExponent); // in [1, 10)
			return new ddouble(newMantissa, newExponent);
		}
		public static ddouble operator ^(ddouble a, double b)
		{
			ddouble _b = b;
			return a ^ _b;
		}
		public static ddouble operator ~(ddouble a) // Log
		{
			double newMantissa = math.log10(a.Mantissa);
			double newExponent = a.Exponent - 1;
			if (newMantissa >= 10)
			{
				newMantissa /= 10;
				newExponent += 1;
			}
			else if (newMantissa < 1)
			{
				newMantissa *= 10;
				newExponent -= 1;
			}
			return new ddouble(newMantissa, newExponent);
		}
		public static ddouble operator |(ddouble a, ddouble b) // Root
		{
			double newMantissa = math.pow(a.Mantissa, 1.0 / b.Mantissa);
			double newExponent = a.Exponent / b.Mantissa;
			if (newMantissa >= 10)
			{
				newMantissa /= 10;
				newExponent += 1;
			}
			else if (newMantissa < 1)
			{
				newMantissa *= 10;
				newExponent -= 1;
			}
			return new ddouble(newMantissa, newExponent);
		}

		// Static Identities
		public static ddouble ZERO = new();

		/// <summary>
		/// Human-facing display string for game-balance numbers - plain with thousands separators below
		/// a million, abbreviated (M/B/T/Qa/...) above. Distinct from ToString() (mantissa-e-exponent,
		/// meant for huge incremental-currency numbers, e.g. "1.000e12") - use this anywhere a player
		/// reads a normal number: damage, DPS, stat values, description placeholders.
		/// </summary>
		public string PrettyPrint()
		{
			double value = (double)this;
			double abs = math.abs(value);
			string sign = value < 0 ? "-" : "";

			(double threshold, string suffix)[] tiers =
			{
				(1e15, "Qa"), (1e12, "T"), (1e9, "B"), (1e6, "M"),
			};
			foreach (var (threshold, suffix) in tiers)
			{
				if (abs >= threshold) return sign + (abs / threshold).ToString("0.##") + suffix;
			}
			return sign + abs.ToString("#,##0.##");
		}
	}

	// ===== Interfaces =====

	// ===== Enum =====
	public enum MathOperation
	{
		// The following are ordered by precedence
		Add,
		Exponent, // Inverse exponent is the Root. Take a^(1/n) instead of a^n
		Multiply, // For Divide and Deduct, simply use the inverse values aka n^-1 and -n; same for the rest.
		Logarithm,

		// Binary
		Or,
		And,
		Xor,

		// Comparative
		Equal,
		NotEqual,
		Greater,
		Lesser,
		Geq, // Greater or equal
		Leq,
		Max,
		Min,
		AbsGreater, // Absolutely greater
		AbsLesser,
		Ageq, // Absolutely greater or equal
		Aleq,

		// Mono
		Nothing,
	}

	public enum MathJoin
	{
		Arithmetic,
		Geometric,
		Harmonic,
		Power,
		Alternating,
		Product,
		AlternatingPower,
	}

	#endregion Objects and Interfaces
}