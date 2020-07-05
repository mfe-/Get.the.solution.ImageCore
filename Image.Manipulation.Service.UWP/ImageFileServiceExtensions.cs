using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public static class ImageFileServiceExtensions
    {
        /// <summary>
        /// Creates a <seealso cref="IRandomAccessStream"/> from <paramref name="softwareBitmap"/>.
        /// </summary>
        /// <param name="softwareBitmap">The bitmap which should be converted to a stream.</param>
        /// <returns>The generatede stream</returns>
        public static async Task<IRandomAccessStream> ToRandomAccesStreamAsync(this SoftwareBitmap softwareBitmap)
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
        /// Converts a Memorystream to a <seealso cref="SoftwareBitmap"/>
        /// </summary>
        /// <param name="stream">The memorystream which contains the image</param>
        /// <returns>The generated <seealso cref="SoftwareBitmap"/></returns>
        public static async Task<SoftwareBitmap> ToSoftwareBitmapAsync(this Stream stream)
        {
            BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
            SoftwareBitmap softwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            return softwareBitmap;
        }
        public static async Task ToStorageFile(this Task<SoftwareBitmap> softwareBitmapTask, StorageFile storageFile)
        {
            var softwareBitmap = await softwareBitmapTask;
            await softwareBitmap.ToStorageFile(storageFile);
        }
        /// <summary>
        /// Saves the <paramref name="softwareBitmap"/> to the overgiven <paramref name="storageFile"/>
        /// </summary>
        /// <param name="softwareBitmap">The picture to save</param>
        /// <param name="storageFile">The file in which the pictures gets stored</param>
        /// <returns></returns>
        public static async Task ToStorageFile(this SoftwareBitmap softwareBitmap, StorageFile storageFile)
        {
            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (IRandomAccessStream memoryStream = await softwareBitmap.ToRandomAccesStreamAsync())
                {
                    await RandomAccessStream.CopyAndCloseAsync(memoryStream, stream);
                }
            }
        }
        /// <summary>
        /// Action with ref parameter for pixel operationss
        /// </summary>
        /// <typeparam name="T">Generic parameter for pixel</typeparam>
        /// <param name="b">blue value</param>
        /// <param name="g">green value</param>
        /// <param name="r">red value</param>
        /// <param name="a">alpha value</param>
        public delegate void ActionRef<T>(ref T b, ref T g, ref T r, ref T a);
        /// <summary>
        /// Action with ref parameter for pixel operationss
        /// </summary>
        /// <typeparam name="T">Generic parameter for pixel</typeparam>
        /// <param name="b">blue value</param>
        /// <param name="g">green value</param>
        /// <param name="r">red value</param>
        /// <param name="a">alpha value</param>
        public delegate void ActionRefs<T>(ref T b1, ref T g1, ref T r1, ref T a1,ref T b2, ref T g2, ref T r2, ref T a2);
        /// <summary>
        /// Iterate over each pixel of <paramref name="bitmap"/> and passes the pixel values to <paramref name="currPixelAction"/>
        /// </summary>
        /// <param name="bitmap">The bitmap to modify</param>
        /// <param name="currPixelAction">The pixel action which should be called</param>
        public static unsafe void EditPixels(SoftwareBitmap bitmap, ActionRef<byte> currPixelAction)
        {
            if (currPixelAction == null) throw new ArgumentException(nameof(currPixelAction));
            if (bitmap == null) throw new ArgumentException(nameof(currPixelAction));
            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8) throw new ArgumentException(nameof(bitmap), $"{BitmapPixelFormat.Bgra8} expected");
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

        /// <summary>
        /// Iterate over each pixel of <paramref name="bitmap"/> and passes the pixel values to <paramref name="currPixelAction"/>
        /// </summary>
        /// <param name="bitmap">The bitmap to modify</param>
        /// <param name="softwareBitmap"></param>
        /// <param name="currPixelAction">The pixel action which should be called</param>
        public static unsafe void EditPixels(SoftwareBitmap bitmap, SoftwareBitmap softwareBitmap, ActionRefs<byte> currPixelAction)
        {
            if (currPixelAction == null) throw new ArgumentException(nameof(currPixelAction));
            if (bitmap == null) throw new ArgumentException(nameof(currPixelAction));
            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8) throw new ArgumentException(nameof(bitmap), $"{BitmapPixelFormat.Bgra8} expected");
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8) throw new ArgumentException(nameof(softwareBitmap), $"{BitmapPixelFormat.Bgra8} expected");

            // Effect is hard-coded to operate on BGRA8 format only
            if (bitmap.BitmapPixelFormat == BitmapPixelFormat.Bgra8 && softwareBitmap.BitmapPixelFormat == BitmapPixelFormat.Bgra8)
            {
                // In BGRA8 format, each pixel is defined by 4 bytes
                const int BYTES_PER_PIXEL = 4;

                using (BitmapBuffer bufferBitmap = bitmap.LockBuffer(BitmapBufferAccessMode.ReadWrite))
                using (IMemoryBufferReference referenceBitmap = bufferBitmap.CreateReference())

                using (BitmapBuffer bufferSoftwareBitmap = softwareBitmap.LockBuffer(BitmapBufferAccessMode.ReadWrite))
                using (IMemoryBufferReference referenceSoftwareBitmap = bufferSoftwareBitmap.CreateReference())
                {
                    // Get a pointer to the pixel buffer
                    byte* dataBitmap;
                    uint capacityBitmap;
                    ((IMemoryBufferByteAccess)referenceBitmap).GetBuffer(out dataBitmap, out capacityBitmap);
                    byte* dataSoftwareBitmap;
                    uint capacitySoftwareBitmap;
                    ((IMemoryBufferByteAccess)referenceSoftwareBitmap).GetBuffer(out dataSoftwareBitmap, out capacitySoftwareBitmap);

                    // Get information about the BitmapBuffer
                    BitmapPlaneDescription descBitmap = bufferBitmap.GetPlaneDescription(0);
                    BitmapPlaneDescription descSoftwarebitmap = bufferSoftwareBitmap.GetPlaneDescription(0);

                    if (descBitmap.Width != descSoftwarebitmap.Width || descBitmap.Height != descSoftwarebitmap.Height)
                        throw new ArgumentException(nameof(descSoftwarebitmap), $"Diffrent {nameof(descBitmap.Width)} or {nameof(descBitmap.Height)}");

                    // Iterate over all pixels
                    for (uint row = 0; row < descBitmap.Height; row++)
                    {
                        for (uint col = 0; col < descBitmap.Width; col++)
                        {
                            // 8 bit or 1 byte for one data field 0... 255

                            // Index of the current pixel in the buffer (defined by the next 4 bytes, BGRA8)
                            long currPixelBitmap = descBitmap.StartIndex + descBitmap.Stride * row + BYTES_PER_PIXEL * col;
                            long currPixelSoftwareBitmap = descSoftwarebitmap.StartIndex + descSoftwarebitmap.Stride * row + BYTES_PER_PIXEL * col;


                            // Read the current pixel information into b,g,r channels (leave out alpha channel)
                            currPixelAction.Invoke(
                                //bitmap
                                ref dataBitmap[currPixelBitmap + 0],
                                ref dataBitmap[currPixelBitmap + 1],
                                ref dataBitmap[currPixelBitmap + 2],
                                ref dataBitmap[currPixelBitmap + 3],

                                //softwareBitmap
                                ref dataSoftwareBitmap[currPixelSoftwareBitmap + 0],
                                ref dataSoftwareBitmap[currPixelSoftwareBitmap + 1],
                                ref dataSoftwareBitmap[currPixelSoftwareBitmap + 2],
                                ref dataSoftwareBitmap[currPixelSoftwareBitmap + 3]
                                );
                        }
                    }
                }
            }
        }
    }
}
