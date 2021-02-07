using Get.the.solution.Image.Contract;
using System;

namespace Image.Operation
{
    /// <summary>
    /// Detects whether the most pixels are dark or not
    /// </summary>
    public class DetectDarkImageOperation : IEditPixelOperator<bool>
    {
        /// <summary>
        /// Initializes <seealso cref="DetectDarkImageOperation"/>
        /// </summary>
        /// <param name="brightnessTolerance">Reference value for the max. allowed brightness. 
        /// A value above this parameter will treat the current summarized pixel (R,G,B) as "dark" pixel</param>
        /// <param name="darkPercentage">The Reference value at which percentage the image should be treated as "dark"</param>
        public DetectDarkImageOperation(byte brightnessTolerance, double darkPercentage)
        {
            this.brightnessThreshold = brightnessTolerance;
            this.darkPercentageThreshold = darkPercentage;
        }
        private readonly double darkPercentageThreshold;
        private readonly byte brightnessThreshold;
        private int amountDarkPixels = 0;
        private int amountPixels = 0;
        /// <summary>
        /// Gets whether the image contains mainly dark pixel values
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// Sums up the pixel of <paramref name="b1"/>, <paramref name="g1"/>, <paramref name="r1"/> with weighted values.
        /// Depending on the threshold the counter value for dark pixels will be increased
        /// </summary>
        /// <param name="b1">blue pixel</param>
        /// <param name="g1">green pixel</param>
        /// <param name="r1">red pixel</param>
        /// <param name="a1">alpha pixel</param>
        public void EditPixel(ref byte b1, ref byte g1, ref byte r1, ref byte a1)
        {
            amountPixels++;
            byte brightness = (byte)Math.Round((0.299 * r1 + 0.5876 * g1 + 0.114 * b1));

            if (brightness <= brightnessThreshold)
                amountDarkPixels++;
        }
        /// <summary>
        /// Calculates the relationship of the total amound of pixels versus the amount of dark pixels.
        /// Depending on the percentage threshold the image will be treated as "dark" image
        /// </summary>
        /// <returns></returns>
        public object SetResult()
        {
            Result = (1d * amountDarkPixels / amountPixels) >= darkPercentageThreshold;
            return Result;
        }
    }
}
