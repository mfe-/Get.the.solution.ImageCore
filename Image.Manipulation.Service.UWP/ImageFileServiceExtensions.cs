using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public static class ImageFileServiceExtensions
    {
        public delegate void ActionRef<T>(ref T b, ref T g, ref T r, ref T a);
        public static async Task<IRandomAccessStream> SoftwareBitmapToRandomAccesStreamAsync(this SoftwareBitmap softwareBitmap)
        {
            var stream = new InMemoryRandomAccessStream();

            WriteableBitmap bitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
            softwareBitmap.CopyToBuffer(bitmap.PixelBuffer);

            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
            Stream pixelStream = bitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[pixelStream.Length];
            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                (uint)bitmap.PixelWidth,
                                (uint)bitmap.PixelHeight,
                                96,
                                96,
                                pixels);
            await encoder.FlushAsync();
            return stream;
        }
        /// <summary>
        /// Iterate over each pixel of <paramref name="bitmap"/> and passes the pixel values to <paramref name="currPixelAction"/>
        /// </summary>
        /// <param name="bitmap">The bitmap to modify</param>
        /// <param name="currPixelAction">The pixel action which should be called</param>
        public static unsafe void EditPixels(SoftwareBitmap bitmap, ActionRef<byte> currPixelAction)
        {
            if (currPixelAction == null) throw new ArgumentException(nameof(currPixelAction));
            // Effect is hard-coded to operate on BGRA8 format only
            if (bitmap.BitmapPixelFormat == BitmapPixelFormat.Bgra8)
            {
                // In BGRA8 format, each pixel is defined by 4 bytes
                const int BYTES_PER_PIXEL = 4;

                using (var buffer = bitmap.LockBuffer(BitmapBufferAccessMode.ReadWrite))
                using (var reference = buffer.CreateReference())
                {
                    // Get a pointer to the pixel buffer
                    byte* data;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out data, out capacity);

                    // Get information about the BitmapBuffer
                    var desc = buffer.GetPlaneDescription(0);

                    // Iterate over all pixels
                    for (uint row = 0; row < desc.Height; row++)
                    {
                        for (uint col = 0; col < desc.Width; col++)
                        {
                            // 8 bit or 1 byte for one data field 0... 255

                            // Index of the current pixel in the buffer (defined by the next 4 bytes, BGRA8)
                            var currPixel = desc.StartIndex + desc.Stride * row + BYTES_PER_PIXEL * col;

                            // Read the current pixel information into b,g,r channels (leave out alpha channel)
                            currPixelAction.Invoke(
                                ref data[currPixel + 0],
                                ref data[currPixel + 1],
                                ref data[currPixel + 2],
                                ref data[currPixel + 3]);
                        }
                    }
                }
            }
        }
    }
}
