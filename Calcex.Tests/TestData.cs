using System;
using System.Collections.Generic;

namespace Calcex.Tests
{
    public static class TestData
    {
        public static IEnumerable<object[]> GetCommonData()
        {
            // Test addition, subtraction, '-' sign
            yield return new object[] { 2500, "1 000 + 1 500", 0 };
            yield return new object[] { 20, "4.5--12.5--3", 0 };
            yield return new object[] { 40.7, "+56-+8.3--2+-9+-7++0--7", 1E-12 };
            // Test multiplication, division
            yield return new object[] { -63, "7.5*(-14)*1.2*0.5", 0 };
            yield return new object[] { 691 / 45.0, "691/15/3", 1E-12 };
            // Test brackets
            yield return new object[] { -30, "(((-2)*(12-6))-(13+5))", 0 };
            yield return new object[] { -299.877, "((((4.123-546)+((-32))*(6-12.5))))+((((+(((34)))))))", 1E-12 };
            yield return new object[] { 9187 / 11660.0, "-5/-5++6^(12*-56++2/3.654)-(123.65/583)", 1E-12 };
            // Test number notations
            yield return new object[] { 10.5, ".8+5.6-.4+.5+4", 0 };
            yield return new object[] { 1.3E+12, "5E+12*4E-5*6.5E+3", 0.001 };
            // Test power
            yield return new object[] { 512, "2^3^2", 0 };
            yield return new object[] { 4.294529258, "2^1.45^(9-12+3.5)^(-1)", 1E-8 };
            yield return new object[] { -3.00407041116273e20, "-(((2+3)*pi)*12)^((2+3)+4)-5", 3e5 };
            yield return new object[] { 78.6686731871678027, "12*exp(2)-10", 1e-16 };
            // Test modulo
            yield return new object[] { 8, "8653579%2478534%(12%5-43)%23", 0 };
            // Test cbrt, sqrt
            yield return new object[] { 2, "sqrt2^2", 1e-14 };
            yield return new object[] { -8146.575991708, "sqrt1897.5*-187.23+sqrt(89-4)", 1e-8 };
            yield return new object[] { 17.707912, "12*cbrt0.85/0.5-cbrt(12/3--123)", 1e-8 };
            // Test lg, abs, ln, log
            yield return new object[] { 54.08, "26.54*lg(abs(800/-8))+ln(e)", 1e-8 };
            yield return new object[] { 20, "log(pi, lg(10))+log(2, 512)+log(5, 48828125)", 1e-8 };
            // Test sin, cos, tan, rad
            yield return new object[] { 0.961592814325542, "sin(783.5*(5+2400)+-1.5)", 1e-8 };
            yield return new object[] { -5, "cos(rad(180))*5+tan(rad(360))", 1e-8 };
            // Test deg, asin, atan
            yield return new object[] { 1350, "deg(asin(0.5))*deg(atan(1))", 1e-8 };
            // test sinh, cosh, tanh
            yield return new object[] { 74.03474733, "cosh(-5)+tanh12.5-sinh(10.5-9.5)", 1e-8 };
            // Test fact
            yield return new object[] { 3628800, "fact10", 0 };
            // Test min, max, sign, avg
            yield return new object[] { 24.3, "min(45.2, max(-123.5, sgn(65436), 24.3, 3.5))", 0 };
            yield return new object[] { 34.5, "avg(-54, 19, 43, 123, 99, -23)", 0 };
            // Test less, if condition
            yield return new object[] { 1, "6.9999<7", 0 };
            yield return new object[] { 5, "if(12.5-6=6.5, 5, -5)", 0 };
            yield return new object[] { 0, "if(-22*(-1)>10=false, 1, 0)", 0 };
            // Test misc
            yield return new object[] { -13.433252489462211, "+(-2*6.789/212*sin(4.5^2)-43*pi^(cos(e)))+sqrt(pi)", 1e-8 };
        }

        public static IEnumerable<object[]> GetIndexVarData()
        {
            // Test sum, prod
            yield return new object[] { 2046, "sum(i, 1, 10, 2^i)" };
            yield return new object[] { 120, "prod(i, 1, 5, i)" };
        }

        public static IEnumerable<object[]> GetBooleanData()
        {
            // Test operators
            yield return new object[] { true, "12.5=20-8+0.5" };
            yield return new object[] { false, "pi<0" };
            yield return new object[] { true, "12^0>=12=false" };
            yield return new object[] { false, "2*9/3<>3+3&&13*0.1>1" };
            yield return new object[] { true, "76*0.01=0.76||false||0" };
            // Test boolean functions
            yield return new object[] { false, "not or(false, and(-12=12, true), (-12)^2=288/2, 0<>0)" };
            yield return new object[] { false, "xor(12||0, (sin(165)&&1)=true, -65*(-3)*(-2.5)>0)" };
        }

        public static IEnumerable<object[]> GetBitwiseData()
        {
            yield return new object[] { 691, "547.98|3487&657.2" };
            yield return new object[] { 479523, "7672376>>4" };
            yield return new object[] { -471592, "-7545463>>4" };
            yield return new object[] { 10880, "(1985^|657)<<3" };
            yield return new object[] { 536870803, "-865>>>3" };
        }

        public static IEnumerable<string[]> GetNaNData()
        {
            yield return new[] { "15/(87.5-87.5)" };
            yield return new[] { "sqrt(18-27)+5" };
            yield return new[] { "35+5-7*lg((12-32)/3)" };
        }

        public static IEnumerable<string[]> GetEvaluateDecimalData()
        {
            for (int i = 1; i <= 10; i++)
            {
                string initial = new string('1', i);
                string subtract = new string('1', i - 1) + '0';
                string remain = new string('0', i - 1) + '1';
                yield return new string[] { $"0.{remain}", $"0.{initial}", $"0.{subtract}" };
            }
        }
    }
}
