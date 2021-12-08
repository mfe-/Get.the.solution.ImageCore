using System;
using System.Collections.Generic;
using System.Text;

namespace Get.the.solution.Image.Contract
{
    public interface IKernelOperation
    {
        byte[][] Process(ref byte[][] vs);
        byte[,] Process(int height, int width, Func<int, int, byte> getByteAtPosition);
    }
}
