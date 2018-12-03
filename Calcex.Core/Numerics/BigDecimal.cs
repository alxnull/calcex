using System;
using System.Numerics;

namespace Bluegrams.Calcex.Numerics
{
    /// <summary>
    /// Represents an arbitrarily precisioned decimal.
    /// All operations are exact, except for division. Division never determines more digits than the given precision.
    /// Based on https://gist.github.com/JcBernack/0b4eef59ca97ee931a2f45542b9ff06d.
    /// WARNING: Experimental.
    /// </summary>
    public struct BigDecimal : IComparable, IComparable<BigDecimal>, IEquatable<BigDecimal>
    {

        /// <summary>
        /// Sets the maximum precision of division operations.
        /// </summary>
        public static int Precision = 50;

        public BigInteger Mantissa { get; set; }
        public int Exponent { get; set; }

        public BigDecimal(BigInteger mantissa, int exponent)
            : this()
        {
            Mantissa = mantissa;
            Exponent = exponent;
            Normalize();
        }

        /// <summary>
        /// Removes trailing zeros on the mantissa.
        /// </summary>
        public void Normalize()
        {
            if (Mantissa.IsZero)
            {
                Exponent = 0;
            }
            else
            {
                BigInteger remainder = 0;
                while (remainder == 0)
                {
                    var shortened = BigInteger.DivRem(Mantissa, 10, out remainder);
                    if (remainder == 0)
                    {
                        Mantissa = shortened;
                        Exponent++;
                    }
                }
            }
        }

        /// <summary>
        /// Truncate the number to the given precision by removing the least significant digits.
        /// </summary>
        /// <returns>The truncated number</returns>
        public BigDecimal Truncate(int precision)
        {
            // copy this instance (remember it's a struct)
            var shortened = this;
            // save some time because the number of digits is not needed to remove trailing zeros
            shortened.Normalize();
            // remove the least significant digits, as long as the number of digits is higher than the given Precision
            while (NumberOfDigits(shortened.Mantissa) > precision)
            {
                shortened.Mantissa /= 10;
                shortened.Exponent++;
            }
            // normalize again to make sure there are no trailing zeros left
            shortened.Normalize();
            return shortened;
        }

        public BigDecimal Truncate()
        {
            return Truncate(Precision);
        }

        public BigDecimal Floor()
        {
            return Truncate(BigDecimal.NumberOfDigits(Mantissa) + Exponent);
        }

        public static int NumberOfDigits(BigInteger value)
        {
            return (int)Math.Floor(BigInteger.Log10(value * value.Sign)) + 1;
        }

        /// <summary>
        /// Converts a string representation of a number to a BigDecimal.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <returns>A BigInteger value that is equivalent to the given string.</returns>
        public static BigDecimal Parse(string value)
        {
            return Parse(value, System.Globalization.NumberFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// Converts a string representation of a number to a BigDecimal.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="formatProvider">An object that provides formatting information about value.</param>
        /// <returns>A BigInteger value that is equivalent to the given string.</returns>
        public static BigDecimal Parse(string value, IFormatProvider formatProvider)
        {
            string p = System.Globalization.NumberFormatInfo.GetInstance(formatProvider).NumberDecimalSeparator;
            int exponent = value.IndexOf(p) < 0 ? 0 : value.IndexOf(p) - value.Replace(p, "").Length;
            return new BigDecimal(BigInteger.Parse(value.Replace(p, "")), exponent);
        }

        /// <summary>
        /// Returns the mantissa of value, aligned to the exponent of reference.
        /// Assumes the exponent of value is larger than of reference.
        /// </summary>
        private static BigInteger AlignExponent(BigDecimal value, BigDecimal reference)
        {
            return value.Mantissa * BigInteger.Pow(10, value.Exponent - reference.Exponent);
        }

        #region Conversions

        public static implicit operator BigDecimal(int value) => new BigDecimal(value, 0);

        public static implicit operator BigDecimal(long value) => new BigDecimal(value, 0);

        public static implicit operator BigDecimal(double value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            double scaleFactor = 1;
            while (Math.Abs(value * scaleFactor - (double)mantissa) > 0)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static implicit operator BigDecimal(float value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            float scaleFactor = 1;
            while (Math.Abs(value * scaleFactor - (float)mantissa) > 0)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static implicit operator BigDecimal(decimal value)
        {
            var mantissa = (BigInteger)value;
            var exponent = 0;
            decimal scaleFactor = 1;
            while ((decimal)mantissa != value * scaleFactor)
            {
                exponent -= 1;
                scaleFactor *= 10;
                mantissa = (BigInteger)(value * scaleFactor);
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static explicit operator double(BigDecimal value)
        {
            return (double)value.Mantissa * Math.Pow(10, value.Exponent);
        }

        public static explicit operator float(BigDecimal value) => (float)((double)value);

        public static explicit operator decimal(BigDecimal value)
        {
            return (decimal)value.Mantissa * (decimal)Math.Pow(10, value.Exponent);
        }

        public static explicit operator int(BigDecimal value) => (int)((double)value);

        public static explicit operator long(BigDecimal value) => (long)((double)value);

        #endregion

        #region Operators

        public static BigDecimal operator +(BigDecimal value)
        {
            return value;
        }

        public static BigDecimal operator -(BigDecimal value)
        {
            value.Mantissa *= -1;
            return value;
        }

        public static BigDecimal operator ++(BigDecimal value)
        {
            return value + 1;
        }

        public static BigDecimal operator --(BigDecimal value)
        {
            return value - 1;
        }

        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            return Add(left, right);
        }

        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            return Add(left, -right);
        }

        public static BigDecimal Add(BigDecimal left, BigDecimal right)
        {
            return left.Exponent > right.Exponent
                ? new BigDecimal(AlignExponent(left, right) + right.Mantissa, right.Exponent)
                : new BigDecimal(AlignExponent(right, left) + left.Mantissa, left.Exponent);
        }

        public static BigDecimal operator *(BigDecimal left, BigDecimal right) => Multiply(left, right);

        public static BigDecimal Multiply(BigDecimal left, BigDecimal right)
        {
            return new BigDecimal(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
        }

        public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
        {
            var exponentChange = Precision - (NumberOfDigits(dividend.Mantissa) - NumberOfDigits(divisor.Mantissa));
            if (exponentChange < 0)
            {
                exponentChange = 0;
            }
            dividend.Mantissa *= BigInteger.Pow(10, exponentChange);
            return truncateToPrecision(dividend.Mantissa / divisor.Mantissa, dividend.Exponent - divisor.Exponent - exponentChange);
        }

        private static BigDecimal truncateToPrecision(BigInteger mantissa, int exponent)
        {
            while ((int)BigInteger.Log10(mantissa) + 1 > Precision)
            {
                mantissa = mantissa / 10;
                exponent += 1;
            }
            return new BigDecimal(mantissa, exponent);
        }

        public static BigDecimal operator %(BigDecimal left, BigDecimal right)
        {
            return left - right * (left / right).Floor();
        }
        #endregion

        #region Comparison
        public static bool operator ==(BigDecimal left, BigDecimal right) => left.Equals(right);

        public static bool operator !=(BigDecimal left, BigDecimal right) => !left.Equals(right);

        public static bool operator <(BigDecimal left, BigDecimal right) => left.CompareTo(right) < 0;

        public static bool operator >(BigDecimal left, BigDecimal right) => left.CompareTo(right) > 0;

        public static bool operator <=(BigDecimal left, BigDecimal right) => left.CompareTo(right) <= 0;

        public static bool operator >=(BigDecimal left, BigDecimal right) => left.CompareTo(right) >= 0;

        public bool Equals(BigDecimal other)
        {
            return other.Mantissa.Equals(Mantissa) && other.Exponent == Exponent;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is BigDecimal && Equals((BigDecimal)obj);
        }

        public int CompareTo(BigDecimal other)
        {
            return this.Exponent > other.Exponent ? AlignExponent(this, other).CompareTo(other.Mantissa) 
                : this.Mantissa.CompareTo(AlignExponent(other, this));
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj is BigDecimal))
            {
                throw new ArgumentException();
            }
            return CompareTo((BigDecimal)obj);
        }
        #endregion

        // Currently not used, unchanged from original implementation.
        #region Additional mathematical functions

        public static BigDecimal Exp(double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Exp(diff);
                exponent -= diff;
            }
            return tmp * Math.Exp(exponent);
        }

        public static BigDecimal Pow(double basis, double exponent)
        {
            var tmp = (BigDecimal)1;
            while (Math.Abs(exponent) > 100)
            {
                var diff = exponent > 0 ? 100 : -100;
                tmp *= Math.Pow(basis, diff);
                exponent -= diff;
            }
            return tmp * Math.Pow(basis, exponent);
        }

        #endregion

        public override string ToString()
        {
            return ToString(System.Globalization.NumberFormatInfo.CurrentInfo);
        }

        public string ToString(IFormatProvider provider)
        {
            string s = Mantissa.ToString(provider);
            if (Exponent > 0) return string.Concat(s, "E", Exponent);
            string p = System.Globalization.NumberFormatInfo.GetInstance(provider).NumberDecimalSeparator;
            if (Exponent == 0) return s;
            int decimalPoint = s.Length + Exponent;
            if (decimalPoint <= 0) return String.Format("0{0}{1}{2}", p, new string('0', -decimalPoint), s);
            else return s.Insert(decimalPoint, p);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mantissa.GetHashCode() * 397) ^ Exponent;
            }
        }
    }
}
