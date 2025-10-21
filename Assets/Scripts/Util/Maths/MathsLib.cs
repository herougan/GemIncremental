
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

		public static ddouble operator ^(ddouble a, ddouble b) // Power
		{
			double newMantissa = math.pow(a.Mantissa, b.Mantissa);
			double newExponent = a.Exponent * b.Mantissa;
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