using Get.the.solution.Image.Contract;
using System;
using System.Collections.Generic;

namespace Image.Operation
{
    /// <summary>
    /// Substracts and calculates the standard deviation
    /// </summary>
    public class SubstractOperator : IEditPixelOperators<Tuple<double, double, double>>
    {
        private int meanB = 0;
        private int meanG = 0;
        private int meanR = 0;
        private int amountPixels = 0;
        private readonly List<byte> pixelBList = new List<byte>();
        private readonly List<byte> pixelGList = new List<byte>();
        private readonly List<byte> pixelRList = new List<byte>();
        /// <summary>
        /// Computes absolute differences between the two input pixels and calculates the the mathematical mean value
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="g1"></param>
        /// <param name="r1"></param>
        /// <param name="a1"></param>
        /// <param name="b2"></param>
        /// <param name="g2"></param>
        /// <param name="r2"></param>
        /// <param name="a2"></param>
        public void EditPixels(ref byte b1, ref byte g1, ref byte r1, ref byte a1, ref byte b2, ref byte g2, ref byte r2, ref byte a2)
        {
            byte b = (byte)Math.Abs(b1 - b2);
            b1 = b;
            pixelBList.Add(b);
            meanB += b;
            
            byte g = (byte)Math.Abs(g1 - g2);
            g1 = g;
            pixelGList.Add(g);
            meanG += g;
            
            byte r = (byte)Math.Abs(r1 - r2);
            r1 = r;
            pixelRList.Add(r);
            meanR += r;
            
            amountPixels++;
        }
        public object SetResult()
        {
            //calculate mean
            meanB = meanB / amountPixels;

            int varianceB = CalculateVariance(pixelBList);
            int varianceG = CalculateVariance(pixelGList);
            int varianceR = CalculateVariance(pixelRList);

            //standard deviation
            double standard_deviationB = Math.Sqrt(varianceB);
            double standard_deviationG = Math.Sqrt(varianceG);
            double standard_deviationR = Math.Sqrt(varianceR);

            Result = new Tuple<double,double,double>(standard_deviationB,standard_deviationG,standard_deviationR);

            //reset init values
            meanB = 0;
            meanG = 0;
            meanR = 0;
            amountPixels = 0;
            pixelBList.Clear();
            pixelGList.Clear();
            pixelRList.Clear();
            return standard_deviationB;
        }

        private int CalculateVariance(List<byte> pixelList)
        {
            int varianceB = 0;
            foreach (byte b in pixelList)
            {
                //(x-meanB)^2+...+(x-meanB)^2
                varianceB = varianceB + (int)Math.Pow((b - meanB), 2);
            }
            varianceB = varianceB / amountPixels;
            return varianceB;
        }

        public Tuple<double, double, double> Result { get; private set; }

        public void EditPixel(ref byte b1, ref byte g1, ref byte r1, ref byte a1)
        {
            //nothing to do
        }
        /// <summary>
        /// Computes absolute differences between the two input pixels
        /// </summary>
        /// <remarks>
        /// https://homepages.inf.ed.ac.uk/rbf/HIPR2/pixsub.htm
        /// </remarks>
        /// <param name="b1">Blue first pixel</param>
        /// <param name="g1">Green first pixel</param>
        /// <param name="r1">Red first pixel</param>
        /// <param name="a1">alpha first pixel</param>
        /// <param name="b2">Blue second pixel</param>
        /// <param name="g2">Green second pixel</param>
        /// <param name="r2">Red second pixel</param>
        /// <param name="a2">Alpha second pixel</param>
        public static void Substract(ref byte b1, ref byte g1, ref byte r1, ref byte a1, ref byte b2, ref byte g2, ref byte r2, ref byte a2)
        {
            int b = Math.Abs(b1 - b2);
            int g = Math.Abs(g1 - g2);
            int r = Math.Abs(r1 - r2);
            //int a = Math.Abs(a1 - a2); ignore alpha channel

            b1 = (byte)b;
            g1 = (byte)g;
            r1 = (byte)r;
            //a1 = (byte)a;
        }
    }
}
