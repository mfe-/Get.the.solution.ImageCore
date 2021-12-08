using Get.the.solution.Image.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Image.Operation
{
    /// <summary>
    ///  Neighborhood filtering (convolution)
    /// </summary>
    public class KernelOperation : IKernelOperation
    {
        private readonly double[,] kernel;
        public KernelOperation(double[,] kernel)
        {
            if (kernel.GetLength(0) != kernel.GetLength(1)) throw new ArgumentException("Kernel structs needs to be size of n x n!");
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
                    newB[y][x] = ComputeJagged(x, y, kernel, ref vs);
                }
            }
            return newB;
        }
        private byte ComputeJagged(int x, int y, double[,] kernel, ref byte[][] vs)
        {
            int kernelmiddle = kernel.GetLength(0) / 2;
            int xstart = x - kernelmiddle;
            int ystart = y - kernelmiddle;
            double accumulator = 0;
            double temp = 0;
            //Computer Vision: Algorithms and Applications (September 3, 2010 draft) S112
            for (int yKernel = 0; yKernel < kernel.GetLength(0); yKernel++)
            {
                for (int xKernel = 0; xKernel < kernel.GetLength(0); xKernel++)
                {
                    if ((xstart < 0 || ystart < 0) || (xstart > vs.Length - 1 || ystart > vs.Length - 1))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = (vs[ystart][xstart] * kernel[yKernel, xKernel]);
                    }
                    accumulator += temp;
                    xstart++;
                }
                xstart = x - 1;
                ystart++;
            }
            return Convert.ToByte(accumulator);
        }
        public byte[,] Process(int height, int width, Func<int, int, byte> getByteAtPosition)
        {
            byte[,] newB = new byte[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    newB[y, x] = Compute(x, y, height, width, kernel, getByteAtPosition);
                }
            }
            return newB;
        }
        private byte Compute(int x, int y, int height, int width, double[,] kernel, Func<int, int, byte> getByteAtPosition)
        {
            int kernelmiddle = kernel.GetLength(0) / 2;
            int xstart = x - kernelmiddle;
            int ystart = y - kernelmiddle;
            double accumulator = 0;
            double temp = 0;
            //Computer Vision: Algorithms and Applications (September 3, 2010 draft) S112
            for (int yKernel = 0; yKernel < kernel.GetLength(0); yKernel++)
            {
                for (int xKernel = 0; xKernel < kernel.GetLength(0); xKernel++)
                {
                    if ((xstart < 0 || ystart < 0) || (xstart > width - 1 || ystart > height - 1))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = (getByteAtPosition.Invoke(ystart, xstart) * kernel[yKernel, xKernel]);
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
