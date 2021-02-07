using Get.the.solution.Image.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace Image.Operation
{
    /// <summary>
    /// Performace a invert operation on the pixels
    /// </summary>
    public class InvertOperation : IEditPixelOperator
    {
        /// <summary>
        /// Inverts each pixel by deducting 255
        /// </summary>
        /// <param name="b1">blue channel</param>
        /// <param name="g1">green channel </param>
        /// <param name="r1">red channel</param>
        /// <param name="a1">alpha channel</param>
        public void EditPixel(ref byte b1, ref byte g1, ref byte r1, ref byte a1)
        {
            //255-R
            //255-G
            //255-B
            b1 = (byte)(255 - b1);
            g1 = (byte)(255 - g1);
            r1 = (byte)(255 - r1);
        }

        public object SetResult()
        {
            //nothing to do here
            return null;
        }
    }
}
