using System;
using System.Collections.Generic;
using Bluegrams.Calcex;
using Bluegrams.Calcex.Parsing;
using Bluegrams.Calcex.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bluegrams.Calcex.Tests
{
    [TestClass]
    public class ParserResultTests
    {
        [DataTestMethod]
        // Test addition, subtraction, '-' sign
        [DataRow(2500, "1 000 + 1 500", 0)]
        [DataRow(20, "4.5--12.5--3", 0)]
        [DataRow(40.7, "+56-+8.3--2+-9+-7++0--7", 1E-12)]
        // Test multiplication, division
        [DataRow(-63, "7.5*(-14)*1.2*0.5", 0)]
        [DataRow(691 / 45.0, "691/15/3", 1E-12)]
        // Test brackets
        [DataRow(-30, "(((-2)*(12-6))-(13+5))", 0)]
        [DataRow(-299.877, "((((4.123-546)+((-32))*(6-12.5))))+((((+(((34)))))))", 1E-12)]
        [DataRow(9187 / 11660.0, "-5/-5++6^(12*-56++2/3.654)-(123.65/583)", 1E-12)]
        // Test number notations
        [DataRow(10.5, ".8+5.6-.4+.5+4", 0)]
        [DataRow(1.3E+12, "5E+12*4E-5*6.5E+3", 0.001)]
        // Test power
        [DataRow(512, "2^3^2", 0)]
        [DataRow(4.294529258, "2^1.45^(9-12+3.5)^(-1)", 1E-8)]
        [DataRow(-3.00407041116273e20, "-(((2+3)*pi)*12)^((2+3)+4)-5", 3e5)]
        // Test modulo
        [DataRow(8, "8653579%2478534%(12%5-43)%23", 0)]
        public void EvaluateDouble_Operators(double expected, string input, double epsilon)
        {
            Parser parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).Evaluate(), epsilon);
        }

        [DataTestMethod]
        // Test cbrt, sqrt
        [DataRow(2, "sqrt2^2", 1e-14)]
        [DataRow(-8146.575991708, "sqrt1897.5*-187.23+sqrt(89-4)", 1e-8)]
        [DataRow(17.707912, "12*cbrt0.85/0.5-cbrt(12/3--123)", 1e-8)]
        // Test lg, abs, ln, log
        [DataRow(54.08, "26.54*lg(abs(800/-8))+ln(e)", 1e-8)]
        [DataRow(20, "log(pi, lg(10))+log(2, 512)+log(5, 48828125)", 1e-8)]
        // Test sin, cos, tan, rad
        [DataRow(0.961592814325542, "sin(783.5*(5+2400)+-1.5)", 1e-8)]
        [DataRow(-5, "cos(rad(180))*5+tan(rad(360))", 1e-8)]
        // Test deg, asin, atan
        [DataRow(1350, "deg(asin(0.5))*deg(atan(1))", 1e-8)]
        // test sinh, cosh, tanh
        [DataRow(74.03474733, "cosh(-5)+tanh12.5-sinh(10.5-9.5)", 1e-8)]
        // Test fact
        [DataRow(3628800, "fact10", 0)]
        // Test min, max, sign, avg
        [DataRow(24.3, "min(45.2, max(-123.5, sgn(65436), 24.3, 3.5))", 0)]
        [DataRow(34.5, "avg(-54, 19, 43, 123, 99, -23)", 0)]
        // Test if condition
        [DataRow(5, "if(12.5-6=6.5, 5, -5)", 0)]
        [DataRow(0, "if(-22*(-1)>10=false, 1, 0)", 0)]
        // Test misc
        [DataRow(-13.433252489462211, "+(-2*6.789/212*sin(4.5^2)-43*pi^(cos(e)))+sqrt(pi)", 1e-8)]
        public void EvaluateDouble_Functions(double expected, string input, double delta)
        {
            Parser parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).Evaluate(), delta);
        }

        [DataTestMethod]
        // Test operators
        [DataRow(true, "12.5=20-8+0.5")]
        [DataRow(false, "pi<0")]
        [DataRow(true, "12^0>=12=false")]
        [DataRow(false, "2*9/3<>3+3&&13*0.1>1")]
        [DataRow(true, "76*0.01=0.76||false||0")]
        // Test boolean functions
        [DataRow(false, "not or(false, and(-12=12, true), (-12)^2=288/2, 0<>0)")]
        [DataRow(false, "xor(12||0, (sin(165)&&1)=true, -65*(-3)*(-2.5)>0)")]
        public void EvaluateBoolean(bool expected, string input)
        {
            var parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).EvaluateBool());
        }

        [DataTestMethod]
        [DataRow(691, "547.98|3487&657.2")]
        [DataRow(479523, "7672376>>4")]
        [DataRow(-471592, "-7545463>>4")]
        [DataRow(10880, "(1985^|657)<<3")]
        [DataRow(536870803, "-865>>>3")]
        public void EvaluateDouble_Bitwise(double expected, string input)
        {
            var parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).Evaluate());
        }

        [DataTestMethod]
        [DataRow(double.NaN, "15/(87.5-87.5)")]
        [DataRow(double.NaN, "sqrt(18-27)+5")]
        [DataRow(double.NaN, "35+5-7*lg((12-32)/3)")]
        public void EvaluateDouble_NaN(double expected, string input)
        {
            Parser parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).Evaluate());
        }

        private static IEnumerable<string[]> getEvaluateDecimalData()
        {
            for (int i = 1; i <= 10; i++)
            {
                string rest = new string('1', i);
                yield return new string[] { String.Format("{0}.{1}", i.ToString(), rest), i.ToString(), String.Format("0.{0}", rest) };
            }
        }

        [DataTestMethod]
        [DataRow("5.1101", "5", "0.1101")]
        [DataRow("123.123", "123.12", "0.003")]
        [DynamicData(nameof(getEvaluateDecimalData), DynamicDataSourceType.Method)]
        public void EvaluateDecimal_Subtraction(string a, string b, string c)
        {
            var parser = new Parser();
            ParserResult res = parser.Parse(String.Format("{0} - {1}", a, b));
            var expected = decimal.Parse(c, System.Globalization.CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, res.EvaluateDecimal());
        }

        [DataTestMethod]
        [DataRow("7.98/((2^0)-1)")]
        [DataRow("3*asin(18)+678")]
        [DataRow("fact(-18)*12.3+-65.4")]
        [DataRow("log(176, -16)")]
        public void EvaluateDecimal_ArithmeticException(string input)
        {
            Parser parser = new Parser();
            Assert.ThrowsException<ParserArithmeticException>(() => parser.Parse(input).EvaluateDecimal());
        }

        [TestMethod]
        public void EvaluateList_Test()
        {
            var parser = new Parser("x");
            var res = parser.Parse("x^3");
            var list = res.EvaluateList("x", 0, 11);
            for (int i = 0; i < 11; i++)
                Assert.AreEqual(Math.Pow(i, 3), list[i]);
        }

        [DataTestMethod]
        [DataRow("-(14+354-12)", "-(14+354-12)", SeparatorStyle.Dot)]
        [DataRow("-(-(3+2*pi))", "-(-(+3+2*pi))", SeparatorStyle.Dot)]
        [DataRow("7+3.15--12*23", "7 + +3.15 - -12*  23", SeparatorStyle.Dot)]
        [DataRow("sin(9)-cos(124)%12.265", " sin(  +9  )  -  cos  124  %  12.265", SeparatorStyle.Dot)]
        [DataRow("(lg(4.2)-78)*12", "(((lg( 4.2 ) - (78)) * ((12))))", SeparatorStyle.Dot)]
        [DataRow("log(150;3)+4,98*max(12;33;-5)",  "log(150; 3) + 4,98 * max(12; +33; -5)",  SeparatorStyle.Comma)]
        public void InfixExpression_Test(string expected, string input, SeparatorStyle separator)
        {
            Parser parser = new Parser();
            parser.SeparatorStyle = separator;
            Assert.AreEqual(expected, parser.Parse(input).GetExpression());
        }

        [DataTestMethod]
        [DataRow("14 354 + 12 - -", "-(14+354-12)")]
        [DataRow("18 32 2 * 4.2 * + 664 - 89 2 ^ +", "18 + 32 * 2 * 4.2 - 664 + 89^2")]
        [DataRow("3 4 2 * 1 5 - 2 3 ^ ^ / +", "3 + 4 * 2 / ( 1 - 5 ) ^ 2 ^ 3")]
        [DataRow("2 3 max 3 / 3.1415 * sin", "sin ( max ( 2, 3 ) / 3 * 3.1415 )")]
        [DataRow("15 7 1 1 + - / 3 * 2 1 1 + + -", " ((15 / (7 - (1 + 1))) * 3) - (2 + (1 + 1))")]
        [DataRow("5.56 12 - - 3 - +", "+5.56 - -12 + -3")]
        public void PostfixExpression_Test(string expected, string input)
        {
            Parser parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).GetPostfixExpression());
        }

        [TestMethod]
        public void EvaluateDecimal_StrictMode()
        {
            var parser = new Parser();
            var result = parser.Parse("lg(10)*2");
            var options = new EvaluationOptions() { StrictMode = true };
            Assert.ThrowsException<UnsupportedOperationException>(() => result.EvaluateDecimal(options));
        }

        [DataTestMethod]
        [DataRow(false, "sin(pi) = 0", 0)]
        [DataRow(true, "sin(pi) = 0", 1e-15)]
        public void EvaluateBool_Epsilon(bool expeced, string input, double epsilon)
        {
            var parser = new Parser();
            var options = new EvaluationOptions() { DoubleCompareEpsilon = epsilon };
            Assert.AreEqual(expeced, parser.Parse(input).EvaluateBool(options));
        }
    }
}
