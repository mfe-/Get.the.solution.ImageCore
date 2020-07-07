using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class MediaCaptureService
    {
        // MediaCapture and its state variables
        private MediaCapture _mediaCapture;

        public MediaCapture MediaCapture
        {
            get
            {
                return _mediaCapture;
            }
        }

        public async Task InitializeCameraAsync()
        {
            if (_mediaCapture == null)
            {
                // Attempt to get the back camera if one is available, but use any camera device if not
                var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);
                if (cameraDevice == null)
                {
                    Debug.WriteLine("No camera device found!");
                    throw new InvalidOperationException($"No camera device found! {nameof(cameraDevice)} is null");
                }

                // Create MediaCapture and its settings
                _mediaCapture = new MediaCapture();

                // Register for a notification when something goes wrong
                _mediaCapture.Failed += MediaCapture_Failed;

                var settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = cameraDevice.Id,
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };

                // Initialize MediaCapture
                try
                {
                    await _mediaCapture.InitializeAsync(settings);
                }
                catch (UnauthorizedAccessException u)
                {
                    //capability microphone and webcam is required
                    Debug.WriteLine("The app was denied access to the camera");
                    throw;
                }
            }
        }
        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            Debug.WriteLine($"{nameof(MediaCaptureService)} The app was denied access to the camera");
        }
        /// <summary>
        /// Queries the available video capture devices to try and find one mounted on the desired panel
        /// </summary>
        /// <param name="desiredPanel">The panel on the device that the desired camera is mounted on</param>
        /// <returns>A DeviceInformation instance with a reference to the camera mounted on the desired panel if available,
        ///          any other camera if not, or null if no camera is available.</returns>
        public static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        private async Task<SoftwareBitmap> TakePictureAsync(VideoFrame videoFrame, Action<SoftwareBitmap> modifySoftwareBitmapAction = null)
        {
            if (_mediaCapture != null)
            {
                VideoFrame snapShot = await _mediaCapture.GetPreviewFrameAsync(videoFrame);
                {
                    // Collect the resulting frame
                    SoftwareBitmap previewSoftwareBitmap = snapShot.SoftwareBitmap;

                    modifySoftwareBitmapAction?.Invoke(previewSoftwareBitmap);

                    SoftwareBitmap bitmapBGRA8 = SoftwareBitmap.Convert(previewSoftwareBitmap,
                        BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    return bitmapBGRA8;

                }
            }
            else
            {
                throw new InvalidOperationException($"{nameof(_mediaCapture)} is null. Is cameria initialized?");
            }
        }
        public async Task<SoftwareBitmap> TakePictureAsync(Action<SoftwareBitmap> modifySoftwareBitmapAction = null)
        {
            // Get information about the preview
            if (_mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) is VideoEncodingProperties videoEncodingProperties)
            {
                // Create a video frame in the desired format for the preview frame
                VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)videoEncodingProperties.Width, (int)videoEncodingProperties.Height, BitmapAlphaMode.Ignore);
                SoftwareBitmap bitmapBGRA8 = await TakePictureAsync(videoFrame, modifySoftwareBitmapAction);
                return bitmapBGRA8;
            }
            return null;
        }
        public Task CleanupCameraAsync()
        {
            try
            {
                if (_mediaCapture != null)
                {
                    _mediaCapture.Dispose();
                    _mediaCapture = null;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            return Task.CompletedTask;
        }
    }
}
