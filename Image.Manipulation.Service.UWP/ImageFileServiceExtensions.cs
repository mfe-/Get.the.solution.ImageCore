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
    }
}
