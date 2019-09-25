using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calcex.Tests
{
    [TestClass]
    public class ParserIsValidTests
    {
        [DataTestMethod]
        [DataRow("7++-5", false)]
        [DataRow("12.5--7/-2", true)]
        [DataRow("x*10^y+4", true)]
        [DataRow("lg5%-4.13*sin(2)*pi", true)]
        [DataRow("-5*pi*12+(-4)", true)]
        public void IsValid_Expressions(string exp, bool valid)
        {
            var parser = new Parser("x", "y");
            Assert.AreEqual(valid, parser.IsValid(exp));
        }

        // --- Valid syntax for variables ---
        [DataTestMethod]
        [DataRow("2*pi^2+2a", true)]
        [DataRow("2*a*b+a^b+sin(a)+lgb", true)]
        [DataRow("87*pia+12-b", false)]
        [DataRow("542+ba*2/1876.5", false)]
        [DataRow("15b-18a+2pi", true)]
        [DataRow("(-54)a*(-2)e", false)]
        [DataRow("54-b5+0", false)]
        public void IsValid_Vars(string exp, bool valid)
        {
            var parser = new Parser("a", "b");
            Assert.AreEqual(valid, parser.IsValid(exp));
        }

        // --- Valid syntax for functions ---
        [DataTestMethod]
        [DataRow("sin(pi)+ln(e)-log(2, 10)", true, null)]
        [DataRow("sqrt(sqrt(9)+sin(pi))", true, null)]
        [DataRow("min(min(2, 3)-1, sqrt(sqrt(4))+4, cos9*2)", true, null)]
        [DataRow("min(1, 2, 3, -4, -5, 6) + max(-3.5, -2.5, -1, 2.5, 3.5)", true, null)]
        [DataRow("cos10*ceil3.45+fact10-degtan5", true, null)]
        [DataRow("atan-19", false, typeof(ParserSyntaxException))]
        [DataRow("log2,10", false, typeof(ParserCharException))]
        public void IsValid_FunctionSyntax(string exp, bool valid, Type exType)
        {
            var parser = new Parser();
            Assert.AreEqual(valid, parser.IsValid(exp, out Exception exception));
            if (!valid)
            {
                Assert.IsInstanceOfType(exception, exType);
            }
        }

        [DataTestMethod]
        [DataRow("2+3*7-h*-4", typeof(ParserCharException))]
        [DataRow("15*sin(12, 2)", typeof(ParserFunctionArgumentException))]
        [DataRow("log(10)", typeof(ParserFunctionArgumentException))]
        [DataRow("(3.141*5.6-sin(3)*()+5)", typeof(ParserSyntaxException))]
        [DataRow("sum(2, 3, 4, 5)+12*2", typeof(ParserSyntaxException))]
        [DataRow("prod(1a, 1, 5, 1a*2)", typeof(ParserSyntaxException))]
        [DataRow("sum(i, 1, 10, i^2)+i", typeof(ParserCharException))]
        public void IsValid_Exceptions(string exp, Type exType)
        {
            var parser = new Parser("a", "b");
            Assert.AreEqual(false, parser.IsValid(exp, out Exception exception));
            Assert.IsInstanceOfType(exception, exType);
        }

        [DataTestMethod]
        [DataRow("(((2+(((3-2))))", false)]
        [DataRow("(((2+3))*(-4)*(12-546.56))", true)]
        [DataRow("((2+3+5+6+(7-12))*31))", false)]
        [DataRow("17+3.3*sin(54+(9.9-12)+45", false)]
        public void IsValid_BracketException(string exp, bool valid)
        {
            var parser = new Parser();
            Assert.AreEqual(valid, parser.IsValid(exp, out Exception exception));
            if (!valid)
            {
                Assert.IsInstanceOfType(exception, typeof(ParserBracketException));
            }
        }

        // --- Valid number representations ---
        [DataTestMethod]
        [DataRow("3.14", true)]
        [DataRow("1 543 376", true)]
        [DataRow("2.5E16", true)]
        [DataRow("1.7E-20", true)]
        [DataRow("E156", false)]
        [DataRow("87E+2.5", false)]
        [DataRow("25E+", false)]
        // Note: Scientific notation must use a capital E, e stands for Euler's number.
        [DataRow("4e+10", false)]
        public void IsValidChar_Number(string str, bool valid)
        {
            var parser = new Parser();
            Assert.AreEqual(valid, parser.IsValidItem(str));
        }

        // --- Valid representations of decimal numbers ---
        [DataTestMethod]
        [DataRow(".013", true)]
        [DataRow(".56E+10", true)]
        [DataRow(".E20", false)]
        [DataRow("12E.5", false)]
        public void IsValidChar_Decimal(string str, bool valid)
        {
            var parser = new Parser();
            Assert.AreEqual(valid, parser.IsValidItem(str));
        }

        [DataTestMethod]
        [DataRow("xyz", true)]
        [DataRow("a", false)]
        [DataRow(".", true)]
        [DataRow("4+2", false)]
        [DataRow("sin", true)]
        public void IsValidChar_Misc(string str, bool valid)
        {
            Parser parser = new Parser("xyz");
            Assert.AreEqual(valid, parser.IsValidItem(str));
        }
    }
}