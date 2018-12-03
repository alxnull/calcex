using System;
using System.Collections.Generic;
using Bluegrams.Calcex.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace Bluegrams.Calcex.Tests
{
    [TestClass]
    public class BigDecimalTests
    {
        [DataTestMethod]
        [DataRow("252602734305022989458258125319270.5452949161059356", "254727458263237.1356246819", "991658834219519273.110324")]
        [DataRow("-1549742053184473991.138081419961261088", "604294765797970.243872509502", "-2564.546544")]
        public void BigDecimal_MultiplyTest(string result, string s1, string s2)
        {
            decimal n1 = decimal.Parse(s1, CultureInfo.InvariantCulture);
            decimal n2 = decimal.Parse(s2, CultureInfo.InvariantCulture);
            BigDecimal bd1 = n1;
            BigDecimal bd2 = n2;
            Assert.AreEqual(s1, bd1.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(result, (bd1 * bd2).ToString(CultureInfo.InvariantCulture));
        }

        [DataTestMethod]
        [DataRow("9.42", "28.64", "-19.22")]
        [DataRow("0.0043588", "0.0043589", "-0.0000001")]
        [DataRow("2875497825733284.43857561797", "2875398949874312.12325609797", "98875858972.31531952")]
        [DataRow("94565809.8825082", "89587585.425675", "4978224.4568332")]
        public void BigDecimal_AddTest(string result, string s1, string s2)
        {
            decimal n1 = decimal.Parse(s1, CultureInfo.InvariantCulture);
            decimal n2 = decimal.Parse(s2, CultureInfo.InvariantCulture);
            BigDecimal bd1 = n1;
            BigDecimal bd2 = n2;
            Assert.AreEqual(s1, bd1.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(result, (bd1 + bd2).ToString(CultureInfo.InvariantCulture));
        }

        [DataTestMethod]
        [DataRow("0.1101", "5.1101", "5")]
        [DataRow("0.003", "123.123", "123.12")]
        public void BigDecimal_SubtractTest(string result, string s1, string s2)
        {
            decimal n1 = decimal.Parse(s1, CultureInfo.InvariantCulture);
            decimal n2 = decimal.Parse(s2, CultureInfo.InvariantCulture);
            BigDecimal bd1 = n1;
            BigDecimal bd2 = n2;
            Assert.AreEqual(result, (bd1 - bd2).ToString(CultureInfo.InvariantCulture));
        }

        [DataTestMethod]
        [DataRow("0.66666666666666666666666666666666666666666666666666", "2", "3")]
        [DataRow("0.015125732652675363962941955000945358290792210247683", "80", "5289")]
        [DataRow("", "6873053", "0")]
        public void BigDecimal_DivideTest(string result, string s1, string s2)
        {
            decimal n1 = decimal.Parse(s1, CultureInfo.InvariantCulture);
            decimal n2 = decimal.Parse(s2, CultureInfo.InvariantCulture);
            BigDecimal bd1 = n1;
            BigDecimal bd2 = n2;
            if (String.IsNullOrEmpty(result)) Assert.ThrowsException<DivideByZeroException>(() => bd1 / bd2);
            else Assert.AreEqual(result, (bd1 / bd2).ToString(CultureInfo.InvariantCulture));
        }

        [DataTestMethod]
        [DataRow("0", 0)]
        [DataRow("-65578646", -65578646)]
        [DataRow("-12.785", -12)]
        [DataRow("2342987976496784597492794724.5", null)]
        [DataRow("60429476575957970.2435437309872509378502", null)]
        public void BigDecimal_Parse(string input, int? compare)
        {
            BigDecimal bd = BigDecimal.Parse(input, CultureInfo.InvariantCulture);
            Assert.AreEqual(input, bd.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(double.Parse(input, CultureInfo.InvariantCulture), (double)bd);
            if (compare != null) Assert.AreEqual(compare, (int)bd);
        }
    }
}
