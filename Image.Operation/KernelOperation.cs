using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Image.Operation
{
    /// <summary>
    ///  Neighborhood filtering (convolution)
    /// </summary>
    public class KernelOperation
    {
        private readonly double[][] kernel;
        public KernelOperation(double[][] kernel)
        {
            if (kernel.Length != kernel[0].Length) throw new ArgumentException("Kernel structs needs to be size of n x n!");
            this.kernel = kernel;
        }
        static byte[][] CopyArrayLinq(byte[][] source)
        {
            return source.Select(s => s.ToArray()).ToArray();
        }
        public byte[][] Process(ref byte[][] vs)
        {
            byte[][] newB = CopyArrayLinq(vs);
            for (int y = 0; y < vs.Length; y++)
            {
                for (int x = 0; x < vs[y].Length; x++)
                {
                    newB[y][x] = Compute(x, y, kernel, ref vs);
                }
            }
            return newB;
        }
        private byte Compute(int x, int y, double[][] kernel, ref byte[][] vs)
        {
            int kernelmiddle = kernel.Length / 2;
            int xstart = x - kernelmiddle;
            int ystart = y - kernelmiddle;
            double accumulator = 0;
            double temp = 0;
            //Computer Vision: Algorithms and Applications (September 3, 2010 draft) S112
            for (int yKernel = 0; yKernel < kernel.Length; yKernel++)
            {
                for (int xKernel = 0; xKernel < kernel.Length; xKernel++)
                {
                    if ((xstart < 0 || ystart < 0) || (xstart > vs.Length - 1 || ystart > vs.Length - 1))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = (vs[ystart][xstart] * kernel[yKernel][xKernel]);
                    }
                    accumulator += temp;
                    xstart++;
                }
                xstart = x - 1;
                ystart++;
            }
            return Convert.ToByte(accumulator);
        }
    }
}
