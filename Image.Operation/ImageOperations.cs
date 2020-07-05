using System;

namespace Image.Operation
{
    public static class ImageOperations
    {
        /// <summary>
        /// Calculates for the current pixels of a,r,g,b a grayscale value.
        /// This method needs to be called on every pixel of the image
        /// </summary>
        /// <remarks>http://www.imageprocessingbasics.com/rgb-to-grayscale/</remarks>
        /// <param name="b">Blue</param>
        /// <param name="g">Green</param>
        /// <param name="r">Red</param>
        /// <param name="a">alpha</param>
        public static void ToGrayScale(ref byte b, ref byte g, ref byte r, ref byte a)
        {
            //When converting an RGB image to grayscale, we have to take the RGB values for each pixel and make as output a single value 
            //reflecting the brightness of that pixel. 
            //One such approach is to take the average of the contribution from each channel: (R + B + C) / 3.
            //However, since the perceived brightness is often dominated by the green component, a different, more "human-oriented", 
            //method is to take a weighted average, e.g.: 0.3R + 0.59G + 0.11B.

            double gray = 0.3 * r + 0.59 * g + 0.11 * b;

            // Boost the green channel, leave the other two untouched
            b = (byte)Convert.ToInt32(gray);
            g = (byte)Convert.ToInt32(gray);
            r = (byte)Convert.ToInt32(gray);
            //a = a; ignore alpha channel
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
            int a = Math.Abs(a1 - a2);

            b1 = (byte)b;
            g1 = (byte)g;
            r1 = (byte)r;
            a1 = (byte)a;
        }
    }
}
