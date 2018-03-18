using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public class ImageFile //: IDisposable
    {
        public ImageFile(Uri uri, Stream stream)
        {
            Path = uri;
            Stream = stream;
        }
        public ImageFile(Uri uri, Stream stream, int width, int height) : this(uri,stream)
        {
            Width = width;
            Height = height;
        }
        public ImageFile(Uri uri, Stream stream, int width, int height,FileInfo fileInfo) : 
            this(uri, stream, width, height)
        {
            FileInfo = fileInfo;
        }
        public Uri Path { get; set; }

        public Stream Stream { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public FileInfo FileInfo { get; set; }
    }
}
