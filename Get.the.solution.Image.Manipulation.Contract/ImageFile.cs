using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public class ImageFile
    {
        public ImageFile(Uri uri, Stream stream)
        {
            Path = uri;
            Stream = stream;
        }
        public Uri Path { get; set; }

        public Stream Stream { get; set; }

    }
}
