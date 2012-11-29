using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalNetworks
{

    /// <summary>
    /// This class provides some simple math helper functions
    /// </summary>
    public class MathHelper
    {
        /// <summary>
        /// Computes a mixed fraction representation of a double, like e.g. 5 + 1/2 for 5.5. 
        /// </summary>
        /// <param name="input">The double to compute a mixed fraction for</param>
        /// <param name="accuracy">The accuracy of the rounding</param>
        /// <param name="whole">An out parameter containing the whole part</param>
        /// <param name="numerator">An out parameter containing the numerator of the fractional part</param>
        /// <param name="denominator">An out parameter containing the denominator of the fractional part</param>
        public static void RoundToMixedFraction(double input, int accuracy, out int whole, out int numerator, out int denominator)
        {
            double dblAccuracy = (double)accuracy;
            whole = (int)(Math.Truncate(input));
            var fraction = Math.Abs(input - whole);
            if (fraction == 0)
            {
                numerator = 0;
                denominator = 1;
                return;
            }
            var n = Enumerable.Range(0, accuracy + 1).SkipWhile(e => (e / dblAccuracy) < fraction).First();
            var hi = n / dblAccuracy;
            var lo = (n - 1) / dblAccuracy;
            if ((fraction - lo) < (hi - fraction)) n--;
            if (n == accuracy)
            {
                whole++;
                numerator = 0;
                denominator = 1;
                return;
            }
            var gcd = GCD(n, accuracy);
            numerator = n / gcd;
            denominator = accuracy / gcd;
        }

        /// <summary>
        /// Returns the least common multiple of two numbers
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int LCM(int a, int b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }

        /// <summary>
        /// Returns the greatest common divisor of two numbers
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int GCD(int a, int b)
        {
            if (b == 0) return a;
            else return GCD(b, a % b);
        }
    }
}
