using System;
using System.Collections.Generic;
using System.Text;

namespace Get.the.solution.Image.Contract
{
    public interface IKernelOperation
    {
        byte[][] Process(ref byte[][] vs);
    }
}
