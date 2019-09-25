using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calcex.Tests
{
    [TestClass]
    public class CompileTests
    {
        [DataTestMethod]
        [DynamicData(nameof(TestData.GetCommonData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDelegate_Common(double expected, string input, double delta)
        {
            Parser parser = new Parser();
            Func<double, double> func = parser.Parse(input).Compile("x");
            Assert.AreEqual(expected, func(0), delta);
        }

        [DataTestMethod]
        [DynamicData(nameof(TestData.GetIndexVarData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDelegate_IndexVariables(int expected, string input)
        {
            Parser parser = new Parser("i");
            Func<double, double> func = parser.Parse(input).Compile("i");
            Assert.AreEqual(expected, func(0));
        }

        [DataTestMethod]
        [DynamicData(nameof(TestData.GetBitwiseData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDelegate_Bitwise(int expected, string input)
        {
            Parser parser = new Parser();
            Func<double, double> func = parser.Parse(input).Compile("x");
            Assert.AreEqual(expected, func(0));
        }

        [DataTestMethod]
        [DynamicData(nameof(TestData.GetNaNData), typeof(TestData), DynamicDataSourceType.Method)]
        public void EvaluateDelegate_NaN(string input)
        {
            Parser parser = new Parser();
            Func<double, double> func = parser.Parse(input).Compile("x");
            Assert.AreEqual(double.NaN, func(0));
        }

        [DataTestMethod]
        [DataRow(557, "x*y+125", "y", 36)]
        [DataRow(59049, "(1.5*x)^(-y)", "x", 6)]
        [DataRow(-2.4, "x/y", "z", 0)]
        public void Compile_OneParam(double expected, string input, string var, double varValue)
        {
            Parser parser = new Parser();
            parser.SetVariable("x", 12);
            parser.SetVariable("y", -5);
            Func<double, double> func = parser.Parse(input).Compile(var);
            Assert.AreEqual(expected, func(varValue));
        }

        [DataTestMethod]
        [DataRow(55.01, 6, 3.5, 2.6)]
        [DataRow(260, 2, -16, 0)]
        public void Compile_ThreeParam(double expected, double a, double b, double c)
        {
            Parser parser = new Parser("a", "b", "c");
            var func = parser.Parse("a^2+b^2+c^2").Compile("a", "b", "c");
            Assert.AreEqual(expected, func(a, b, c));
        }

        [TestMethod]
        public void Compile_ArrayParam()
        {
            string[] param = new[] { "a", "b", "c", "d", "q" };
            double[] values = new[] { 34, 99, 0.1, 42, 8 };
            Parser parser = new Parser(param);
            var func = parser.Parse("a+b*c-d%q").Compile(param);
            Assert.AreEqual(41.9, func(values));
        }
    }
}
