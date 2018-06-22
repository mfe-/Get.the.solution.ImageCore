using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Get.the.solution.Image.Manipulation.Contract
{
    [DebuggerDisplay("Name={Name},Width={Width},Height={Height}")]
    public class ImageFile //: IDisposable
    {
        public ImageFile(string path, Stream stream)
        {
            Path = path;
            Stream = stream;
        }
        public ImageFile(string path, Stream stream, int width, int height) : this(path, stream)
        {
            Width = width;
            Height = height;
        }
        public ImageFile(string path, Stream stream, int width, int height, FileInfo fileInfo) :
            this(path, stream, width, height)
        {
            FileInfo = fileInfo;
        }

        private String _Name;
        public String Name
        {
            get { return FileInfo.Name; }
            set { value = _Name; }
        }

        //
        // Summary:
        //     Gets the full file-system path of the item, if the item has a path.
        //
        // Returns:
        //     The full path of the item, if the item has a path in the user's file-system.
        public string Path { get; set; }

        public Stream Stream { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public FileInfo FileInfo { get; set; }

        public object Tag { get; set; }

        public bool IsReadOnly { get; set; }
    }
}
