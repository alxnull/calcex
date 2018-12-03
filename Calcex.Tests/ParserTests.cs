using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bluegrams.Calcex;
using Bluegrams.Calcex.Parsing;
using Bluegrams.Calcex.Numerics;

namespace Bluegrams.Calcex.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void SeparatorStyle_Test()
        {
            Parser parser = new Parser();
            parser.AddMultiParamFunction("add", calc2);
            Assert.AreEqual(1018.65, parser.Parse("add(5.5, 1000.5, 12.65)").Evaluate());
            parser.SeparatorStyle = SeparatorStyle.Comma;
            Assert.AreEqual(1018.65, parser.Parse("add(5,5; 1000,5; 12,65)").Evaluate());
        }

        [DataTestMethod]
        [DataRow(true, -47576.88, "7.8*17*(-1.3)*276")]
        [DataRow(false, double.NaN, "6.9+34*a-3")]
        [DataRow(false, double.NaN, "2*asin(5)/6-76")]
        public void TryEvaluate_Test(bool success, double expected, string input)
        {
            double result;
            Assert.AreEqual(success, Parser.TryEvaluate(input, out result));
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AddVariable_SetVariable_Test()
        {
            Parser parser = new Parser("y");
            parser.AddVariable("s");
            int y = DateTime.Now.Hour;
            parser.SetVariable("y", y);
            parser.SetVariable("s", 7.5);
            Assert.AreEqual(7.5, parser.GetVariable("s"));
            Assert.AreEqual(-2 * Math.Pow(y, 2) + 2 * 7.5, parser.Parse("-2*y^2+2*s").Evaluate());
        }

        [TestMethod]
        public void SetVariable_Test()
        {
            Parser parser = new Parser("x", "y", "z");
            parser.SetVariable("sqrttwo", Math.Sqrt(2));
            parser.SetVariable("a", -22.5);
            Assert.AreEqual("x;y;z;sqrttwo;a", String.Join(";", parser.CustomVariables));
            Assert.IsNull(parser.GetVariable("z"));
            Assert.IsNotNull(parser.GetVariable("a"));
            Assert.AreEqual(22.5, parser.Parse("a*-1").Evaluate());
            Assert.AreEqual(Math.Sqrt(2) + 2, parser.Parse("((sqrttwo+2))").Evaluate());
        }

        [TestMethod]
        public void SetVariable_AfterParsing()
        {
            Parser parser = new Parser("x");
            parser.SetVariable("y", 5);
            var res1 = parser.Parse("3*y+x-10");
            var res2 = parser.Parse("y*(-4)+2");
            Assert.AreEqual(-18, res2.Evaluate());
            parser.SetVariable("x", -13);
            Assert.AreEqual(-8, res1.Evaluate());
            parser.SetVariable("x", 0);
            Assert.AreEqual(5, res1.Evaluate());
        }

        [DataTestMethod]
        [DataRow(12, (int)12)]
        [DataRow(99, "99")]
        [DataRow(1, true)]
        public void SetVariable_Types(double expected, object value)
        {
            var parser = new Parser();
            parser.SetVariable("x", value);
            Assert.AreEqual(expected, parser.Parse("x").Evaluate());
        }

        [TestMethod]
        public void SetVariable_BigDecimal()
        {
            var parser = new Parser();
            parser.SetVariable("x", new BigDecimal(14, 12));
            Assert.AreEqual(14E12, parser.Parse("x").Evaluate());
        }

        [TestMethod]
        public void RemoveVariable_Test()
        {
            var parser = new Parser("a", "b");
            parser.SetVariable("c", 2);
            parser.AddVariable("d");
            parser.RemoveVariable("b");
            Assert.IsNull(parser.GetVariable("b"));
            parser.RemoveVariable("c");
            Assert.IsNull(parser.GetVariable("c"));
            parser.RemoveVariable("d");
            Assert.IsNull(parser.GetVariable("d"));
        }

        [TestMethod]
        public void RemoveAllVariables_Test()
        {
            string[] vars = new string[] { "a", "b", "c", "d", "xyz", "alpha" };
            var parser = new Parser("a", "b", "c");
            parser.SetVariable(vars[3], 2);
            parser.AddVariable(vars[4]);
            parser.AddVariable(vars[5]);
            parser.RemoveAllVariables();
            foreach (string var in vars)
            {
                Assert.IsNull(parser.GetVariable(var));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ParserUnassignedVariableException))]
        public void UnassignedVeriableException_Test()
        {
            Parser parser = new Parser("x");
            parser.Parse("2*x+5").Evaluate();
        }

        [TestMethod]
        public void VariableAssignment_Test()
        {
            Parser parser = new Parser("x");
            parser.AddVariable("y");
            ParserResult res = parser.Parse("2+x+3+y");
            parser.SetVariable("x", -1.5);
            parser.SetVariable("y", 5.001);
            Assert.AreEqual(8.501, res.Evaluate(), 0.0000001);
        }

        [TestMethod]
        public void ArgumentException_AlreadyExists()
        {
            var parser = new Parser("x");
            Assert.ThrowsException<ArgumentException>(() => parser.AddVariable("x"));
            Assert.ThrowsException<ArgumentException>(() => parser.AddVariable("sin"));
            Assert.ThrowsException<ArgumentException>(() => parser.AddFunction("pi", "2*r", "r"));
        }

        [DataTestMethod]
        [DataRow("123ab")]
        [DataRow("a1b")]
        [DataRow("uö")]
        public void ArgumentException_InvalidName(string name)
        {
            var parser = new Parser();
            Assert.ThrowsException<ArgumentException>(() => parser.AddFunction(name, "2*x", "x"));
            Assert.ThrowsException<ArgumentException>(() => parser.AddVariable(name));
        }
                    
        [TestMethod]
        public void CustomFunctions_Test()
        {
            var parser = new Parser();
            parser.AddOneParamFunction("sqr", calc);
            parser.AddTwoParamFunction("add", (a, b) => a + b);
            parser.AddThreeParamFunction("av", (a, b, c) => (a + b + c) / 3);
            parser.AddMultiParamFunction("sum", calc2);
            parser.AddFunction("circ", "pi*r^2", "r");
            Assert.IsTrue(Array.IndexOf(parser.CustomFunctions, "add") > -1);
            Assert.AreEqual(64516, parser.Parse("sqr254").Evaluate());
            Assert.ThrowsException<ParserFunctionArgumentException>(() => parser.Parse("add(1, 2, 3)"));
            Assert.AreEqual(20, parser.Parse("av(32, 22, 6)").Evaluate());
            Assert.AreEqual(12.9, parser.Parse("sum(7, -4, -2.5, 12.4)").Evaluate());
            Assert.AreEqual(Math.PI * 295.84, parser.Parse("circ(17.2)").Evaluate());
        }

        [TestMethod]
        public void RemoveFunctions_Test()
        {
            string[] vars = new string[] { "a", "b", "c", "d", "xyz", "alpha" };
            var parser = new Parser();
            foreach (var s in vars)
            {
                parser.AddOneParamFunction(s, calc);
            }
            parser.RemoveFunction("xyz");
            Assert.IsFalse(Array.IndexOf(parser.CustomFunctions, "xyz") > -1);
            Assert.ThrowsException<ArgumentException>(() => parser.RemoveFunction("xyz"));
            parser.RemoveAllFunctions();
            foreach (string var in vars)
            {
                Assert.IsFalse(Array.IndexOf(parser.CustomFunctions, var) > -1);
            }
        }

        [TestMethod]
        public void VariablesFunctionsSeparation_Test()
        {
            var parser = new Parser("x", "y", "z", "a");
            Assert.IsTrue(Array.IndexOf(parser.CustomFunctions, "z") == -1);
            Assert.ThrowsException<ArgumentException>(() => parser.RemoveFunction("x"));
            Assert.IsTrue(Array.IndexOf(parser.CustomVariables, "x") > -1);
            parser.AddFunction("fun", "2*x", "x");
            Assert.IsNull(parser.GetVariable("fun"));
            parser.RemoveAllVariables();
            Assert.IsTrue(Array.IndexOf(parser.CustomFunctions, "fun") > -1);
        }

        private double calc(double arg)
        {
            return arg * arg;
        }

        private double calc2(double[] args)
        {
            double sum = 0;
            for (int i = 0; i < args.Length; i++)
            {
                sum += args[i];
            }
            return sum;
        }

        [TestMethod]
        public void IsDefined_Test()
        {
            var parser = new Parser("x");
            parser.AddFunction("sqr", "x^2", "x");
            Assert.IsTrue(parser.IsDefined("x"));
            Assert.IsTrue(parser.IsDefined("sqr"));
            Assert.IsTrue(parser.IsDefined("sin"));
            Assert.IsFalse(parser.IsDefined("y"));
        }

        [TestMethod]
        public void ParserSymbols_Get()
        {
            var dict = ParserSymbols.GetParserSymbols();
            Assert.IsTrue(dict.ContainsKey("*"));
            Assert.AreEqual("An opening bracket", dict["("].Description);
        }
    }
}