using Get.the.solution.Image.Contract;
using System;

namespace Image.Operation
{
    public class ToGrayScalePixelOperator : IEditPixelOperator
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
        /// Calculates for the current pixels of a,r,g,b a grayscale value.
        /// This method needs to be called on every pixel of the image
        /// </summary>
        /// <remarks>http://www.imageprocessingbasics.com/rgb-to-grayscale/</remarks>
        /// <param name="b1">Blue</param>
        /// <param name="g1">Green</param>
        /// <param name="r1">Red</param>
        /// <param name="a1">alpha</param>
        public void EditPixel(ref byte b1, ref byte g1, ref byte r1, ref byte a1)
        {
            ToGrayScale(ref b1, ref g1, ref r1, ref a1);
        }

        public object SetResult()
        {
            return 0;
        }
    }
}
