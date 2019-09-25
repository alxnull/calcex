using System;
using Calcex.Parsing;
using Calcex.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calcex.Tests
{
    [TestClass]
    public class ParserResultTests
    {
        [DataTestMethod]
        [DynamicData(nameof(TestData.GetCommonData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDouble_Common(double expected, string input, double epsilon)
        {
            Parser parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).Evaluate(), epsilon);
        }

        [DataTestMethod]
        [DynamicData(nameof(TestData.GetIndexVarData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDouble_IndexVariables(int expected, string input)
        {
            Parser parser = new Parser();
            parser.SetVariable("i", 0);
            Assert.AreEqual(expected, parser.Parse(input).Evaluate());
        }

        [DataTestMethod]
        [DynamicData(nameof(TestData.GetBooleanData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateBoolean(bool expected, string input)
        {
            var parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).EvaluateBool());
        }

        [DataTestMethod]
        [DynamicData(nameof(TestData.GetBitwiseData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDouble_Bitwise(int expected, string input)
        {
            var parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).Evaluate());
        }

        [DataTestMethod]
        [DynamicData(nameof(TestData.GetNaNData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDouble_NaN(string input)
        {
            Parser parser = new Parser();
            Assert.AreEqual(double.NaN, parser.Parse(input).Evaluate());
        }

        [DataTestMethod]
        [DataRow("0.1101", "5.1101", "5")]
        [DataRow("0.003", "123.123", "123.12")]
        [DynamicData(nameof(TestData.GetEvaluateDecimalData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDecimal_Subtraction(string result, string a, string b)
        {
            var parser = new Parser();
            ParserResult res = parser.Parse(String.Format("{0} - {1}", a, b));
            var expected = decimal.Parse(result, System.Globalization.CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, res.EvaluateDecimal());
        }

        [DataTestMethod]
        [DynamicData(nameof(TestData.GetBitwiseData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDecimal_Bitwise(int expected, string input)
        {
            var parser = new Parser();
            Assert.AreEqual(expected, parser.Parse(input).EvaluateDecimal());
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
        [DataRow("sum(i,0,10,2^i)", "sum(i, 0, 10, 2 ^ i)", SeparatorStyle.Dot)]
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
