using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Calcex.Parsing
{
    /// <summary>
    /// A class containing all valid parser symbols.
    /// </summary>
    public static class ParserSymbols
    {
        // --- Operators ---
        // Arithmetic operators
        [Description("Adds two numbers", "12 + 5", "17")]
        public const string Add = "+";
        [Description("Subtracts two numbers", "9 - 4", "5")]
        public const string Subtract = "-";
        [Description("Multiplies two numbers", "4 * 6", "24")]
        public const string Multiply = "*";
        [Description("Divides two numbers", "35 / 2", "17.5")]
        public const string Divide = "/";
        [Description("Raises a number to the specified power", "4 ^ 3", "64")]
        public const string Power = "^";
        [Description("Calculates the remainder of the division of two numbers", "23 % 4", "3")]
        public const string Modulo = "%";
        // Comparing and boolean operators
        [Description("Checks if two values are equal", "2*4 = 8", "true")]
        public const string Equal = "=";
        [Description("Checks if one value is less than another", "8 < 10", "true")]
        public const string LessThan = "<";
        [Description("Checks if one value is greater than another", "4.5 > -3", "true")]
        public const string GreaterThan = ">";
        [Description("Checks if one value is less or equal compared to another", "5 <= 5", "true")]
        public const string LessEqual = "<=";
        [Description("Checks if one value is greater or equal compared to another", "8 >= 7", "true")]
        public const string GreaterEqual = ">=";
        [Description("Checks if two values are not equal", "12 <> 14", "true")]
        public const string NotEqual = "<>";
        [Description("Performs a boolean and operation between two values", "2 < 3 && 3 < 2", "false")]
        public const string And = "&&";
        [Description("Performs a boolean or operation between two values", "2 < 3 || 3 < 2", "true")]
        public const string Or = "||";
        // Bitwise operators
        [Description("Performs a bitwise and operation between two values", "6 & 11", "2")]
        public const string BitwiseAnd = "&";
        [Description("Performs a bitwise or operation between two values", "6 | 11", "15")]
        public const string BitwiseOr = "|";
        [Description("Performs a bitwise xor operation between two values", "6 ^| 11", "13")]
        public const string BitwiseXor = "^|";
        [Description("Performs an arithmetic left shift", "8 << 2", "32")]
        public const string LeftShift = "<<";
        [Description("Performs an arithmetic right shift", "-24 >> 2", "6")]
        public const string RightShift = ">>";
        [Description("Performs a logical (unsigned) right shift of an integer value")]
        public const string UnsignedRightShift = ">>>";

        // --- Functions ---
        // Roots / Powers
        [Description("Calculates the square root of a number", "sqrt(49)", "7")]
        public const string Sqrt = "sqrt";
        [Description("Calculates the cubic root of a number", "cbrt(27)", "3")]
        public const string Cbrt = "cbrt";
        [Description("Raises e to the specified power.", "exp(2)", "e^2")]
        public const string Exp = "exp";
        // Trigonometry
        [Description("Calculates the sine of an angle", "sin(0)", "0")]
        public const string Sin = "sin";
        [Description("Calculates the cosine of an angle", "cos(pi)", "-1")]
        public const string Cos = "cos";
        [Description("Calculates the tangent of an angle", "tan(0)", "0")]
        public const string Tan = "tan";
        [Description("Calculates the angle whose sine is the given number", "asin(0)", "0")]
        public const string ASin = "asin";
        [Description("Calculates the angle whose cosine is the given number", "acos(-1)", "pi")]
        public const string ACos = "acos";
        [Description("Calculates the angle whose tangent is the given number", "atan(0)", "0")]
        public const string ATan = "atan";
        [Description("Calculates the hyperbolic cosine of a number", "cosh(0)", "1")]
        public const string Cosh = "cosh";
        [Description("Calculates the hyperbolic sine of a number", "sinh(0)", "0")]
        public const string Sinh = "sinh";
        [Description("Calculates the hyperbolic tangent of a number", "tanh(0)", "0")]
        public const string Tanh = "tanh";
        [Description("Converts an angle from degrees to radians", "rad(180)", "pi")]
        public const string Rad = "rad";
        [Description("Converts an angle from radians to degrees", "deg(pi)", "180")]
        public const string Deg = "deg";
        // Logarithms
        [Description("Calculates the base 10 logarithm of a number", "lg(10)", "1")]
        public const string Log10 = "lg";
        [Description("Calculates the natural logarithm of a number", "ln(e)", "1")]
        public const string LogE = "ln";
        [Description("Calculates the logarithm of a number", "log(2, 8)", "3")]
        public const string Log = "log";
        // Numbers
        [Description("Negates a number", "neg -23", "23")]
        public const string Negate = "neg";
        [Description("Returns the absolute value of a number", "abs(-15)", "15")]
        public const string Abs = "abs";
        [Description("Returns the smallest integer greater than or equal to the given number", "ceil(2.7)", "3")]
        public const string Ceil = "ceil";
        [Description("Returns the largest integer less than or equal to the given number", "flr(2.7)", "2")]
        public const string Floor = "flr";
        [Description("Rounds the given number to the nearest integer", "rnd(2.7)", "3")]
        public const string Round = "rnd";
        [Description("Returns the sign of a number", "sgn(-5)", "-1")]
        public const string Sign = "sgn";
        [Description("Returns the integral part of a number", "trun(3.54)", "3")]
        public const string Trunc = "trun";
        [Description("Returns the minimum of a given range of values", "min(6, -5, 2)", "-5")]
        public const string Min = "min";
        [Description("Returns the maximum of a given range of values", "max(6, -5, 2)", "6")]
        public const string Max = "max";
        [Description("Calculates the arithmetic mean of a given range of values", "avg(6, -5, 2)", "1")]
        public const string Avg = "avg";
        // Booleans
        [Description("Performs a boolean 'and' operation between all given values", "and(true, false, 0=0)", "false")]
        public const string AndFunc = "and";
        [Description("Performs a boolean 'or' operation between all given values", "or(true, false, 0=0)", "true")]
        public const string OrFunc = "or";
        [Description("Performs a boolean 'not' operation", "not true", "false")]
        public const string Not = "not";
        [Description("Performs a boolean 'xor' operation between all given values", "xor(true, false, 0=0)", "false")]
        public const string XorFunc = "xor";
        // Misc
        [Description("Calculates the factorial of a number", "fact(5)", "120")]
        public const string Fact = "fact";
        [Description("Returns one of two values based on a condition", "if(-5 >= 0, 1, -1)", "-1")]
        public const string Cond = "if";
        [Description("Calculates the sum of a sequence of terms", "sum(i, 1, 10, 2^i)", "2046")]
        public const string Sum = "sum";
        [Description("Calculates the product of a sequence of terms", "prod(i, 1, 5, i)", "120")]
        public const string Prod = "prod";

        // --- Constants ---
        [Description("Represents the number Pi")]
        public const string Pi = "pi";
        [Description("Represents Euler's number")]
        public const string E = "e";
        [Description("Represents the boolean true (has value 1)")]
        public const string True = "true";
        [Description("Represents the boolean false (has value 0)")]
        public const string False = "false";

        // --- Brackets ---
        [Description("An opening bracket")]
        public const string LBracket = "(";
        [Description("A closing bracket")]
        public const string RBracket = ")";

        /// <summary>
        /// Returns a dictionary of all valid parser symbols with description.
        /// </summary>
        /// <returns>A dictionary of parser symbols and their descriptions.</returns>
        public static Dictionary<string, DescriptionAttribute> GetParserSymbols()
        {
            var dict = new Dictionary<string, DescriptionAttribute>();
            foreach(FieldInfo field in typeof(ParserSymbols).GetRuntimeFields())
            {
                var attr = (DescriptionAttribute)field.GetCustomAttributes()
                                                    .Where(v => v is DescriptionAttribute)
                                                    .FirstOrDefault();
                dict.Add(field.GetValue(null).ToString(), attr);
            }
            return dict;
        }
    }

    public class DescriptionAttribute : Attribute
    {
        public string Description { get; private set; }
        public string UsageExample { get; private set; }
        public string UsageResult { get; private set; }

        public DescriptionAttribute(string description)
        {
            Description = description;
        }

        public DescriptionAttribute(string description, string usage, string result)
        {
            Description = description;
            UsageExample = usage;
            UsageResult = result;
        }
    }
}
