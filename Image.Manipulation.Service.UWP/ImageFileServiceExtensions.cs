using Get.the.solution.Image.Contract;
using System;
using System.Collections.Generic;
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
            if(stream.Length!=0)
            {
                stream.Position = 0;
            }
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
        public static unsafe void EditPixels(this SoftwareBitmap bitmap, IEnumerable<IEditPixelOperator> editPixelOperatorList)
        {
            EditPixels(bitmap, currPixelAction: null, editPixelOperatorList: editPixelOperatorList);
        }
        public static unsafe void EditPixels(this SoftwareBitmap bitmap, ActionRef<byte> currPixelAction = null)
        {
            EditPixels(bitmap, currPixelAction: currPixelAction, editPixelOperatorList: null);
        }
        /// <summary>
        /// Iterate over each pixel of <paramref name="bitmap"/> and passes the pixel values to <paramref name="currPixelAction"/>
        /// </summary>
        /// <param name="bitmap">The bitmap to modify</param>
        /// <param name="currPixelAction">The pixel action which should be called</param>
        /// <param name="editPixelOperatorList">A list of pixel operators which should be applied on each pixel</param>
        private static unsafe void EditPixels(this SoftwareBitmap bitmap, ActionRef<byte> currPixelAction = null, IEnumerable<IEditPixelOperator> editPixelOperatorList = null)
        {
            if (bitmap == null) throw new ArgumentException(nameof(currPixelAction));
            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8) throw new ArgumentException(nameof(bitmap), $"{BitmapPixelFormat.Bgra8} expected");

            if (editPixelOperatorList == null) editPixelOperatorList = new List<IEditPixelOperator>();

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

                            // Read the current pixel information into b,g,r channels 
                            currPixelAction?.Invoke(
                                ref data[currPixel + 0],
                                ref data[currPixel + 1],
                                ref data[currPixel + 2],
                                ref data[currPixel + 3]
                                );
                            //iterate over pixel modification list
                            foreach (IEditPixelOperator editPixel in editPixelOperatorList)
                            {
                                // Read the current pixel information into b,g,r channels and pass it to the pixeloperators
                                editPixel.EditPixel(
                                    ref data[currPixel + 0],
                                    ref data[currPixel + 1],
                                    ref data[currPixel + 2],
                                    ref data[currPixel + 3]
                                );
                            }

                        }
                    }
                    //tell the editpixel interface that the pixel modification is completed
                    foreach (IEditPixelOperator editPixel in editPixelOperatorList)
                    {
                        editPixel.SetResult();
                    }
                }
            }
        }
        public static unsafe void EditPixels(this SoftwareBitmap bitmap, SoftwareBitmap softwareBitmap, IEnumerable<IEditPixelOperator> editPixelOperatorsList)
        {
            EditPixels(bitmap, softwareBitmap, null, editPixelOperatorsList);
        }
        public static unsafe void EditPixels(this SoftwareBitmap bitmap, SoftwareBitmap softwareBitmap, ActionRefs<byte> currPixelAction)
        {
            EditPixels(bitmap, softwareBitmap, currPixelAction, null);
        }
        /// <summary>
        /// Iterate over each pixel of <paramref name="bitmap"/> and passes the pixel values to <paramref name="currPixelAction"/>
        /// </summary>
        /// <param name="bitmap">The bitmap to modify</param>
        /// <param name="softwareBitmap">the reference bitmap</param>
        /// <param name="currPixelAction">The pixel action which should be called</param>
        /// <param name="editPixelOperatorsList">edit pixels instance</param>
        public static unsafe void EditPixels(this SoftwareBitmap bitmap, SoftwareBitmap softwareBitmap, ActionRefs<byte> currPixelAction = null, IEnumerable<IEditPixelOperator> editPixelOperatorsList = null)
        {
            if (bitmap == null) throw new ArgumentException(nameof(currPixelAction));
            if (softwareBitmap == null) throw new ArgumentException(nameof(softwareBitmap));
            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8) throw new ArgumentException(nameof(bitmap), $"{BitmapPixelFormat.Bgra8} expected");
            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8) throw new ArgumentException(nameof(softwareBitmap), $"{BitmapPixelFormat.Bgra8} expected");

            if (editPixelOperatorsList == null) editPixelOperatorsList = new List<IEditPixelOperators>();

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
                            currPixelAction?.Invoke(
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

                            foreach (IEditPixelOperator editPixelOperators in editPixelOperatorsList)
                            {
                                if (editPixelOperators is IEditPixelOperators pixelOperators)
                                {
                                    pixelOperators.EditPixels(
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
                                else
                                {
                                    editPixelOperators.EditPixel(
                                   //bitmap
                                   ref dataBitmap[currPixelBitmap + 0],
                                   ref dataBitmap[currPixelBitmap + 1],
                                   ref dataBitmap[currPixelBitmap + 2],
                                   ref dataBitmap[currPixelBitmap + 3]);
                                }
                            }
                        }
                    }
                    foreach (IEditPixelOperator editPixelOperators in editPixelOperatorsList)
                    {
                        editPixelOperators.SetResult();
                    }
                }
            }
        }
    }
}
