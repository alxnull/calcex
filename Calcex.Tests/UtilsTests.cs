using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bluegrams.Calcex;

namespace Bluegrams.Calcex.Tests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void Conversion_FromBase10()
        {
            Assert.AreEqual("1001110", Utils.ConvertFromBase10(78, 2));
            Assert.AreEqual("E057", Utils.ConvertFromBase10(57431, 16));
            Assert.AreEqual("A0", Utils.ConvertFromBase10(290, 29));
            Assert.AreEqual("-1Zd", Utils.ConvertFromBase10(-3765, 46));
        }

        [TestMethod]
        public void Conversion_ToBase10()
        {
            Assert.AreEqual(3817, Utils.ConvertToBase10("zZ", 62));
            Assert.AreEqual(517183, Utils.ConvertToBase10("1111110010000111111", 2));
            Assert.AreEqual(-895031, Utils.ConvertToBase10("-U1AU", 31));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Given base can not be greater than 62.")]
        public void Conversion_ExceptionBaseToBig()
        {
            Utils.ConvertToBase10("ff", 63);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException), "String contains invalid characters.")]
        public void Conversion_ExceptionInvalidChar()
        {
            Utils.ConvertToBase10("1234", 2);
        }
    }
}
