using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IResizeService
    {
        MemoryStream Resize(Stream inputStream, int width, int height);
    }
}
