using System;

namespace Image.Operation
{
    public static class ImageOperations
    {
        /// <summary>
        /// Calculates for the current pixels of a,r,g,b a grayscale value.
        /// This method needs to be called on every pixel of the image
        /// </summary>
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

            var gray = 0.3 * r + 0.59 * g + 0.11 * b;

            // Boost the green channel, leave the other two untouched
            b = (byte)Convert.ToInt32(gray);
            g = (byte)Convert.ToInt32(gray);
            r = (byte)Convert.ToInt32(gray);
            //a = a; ignore alpha channel
        }
    }
}
