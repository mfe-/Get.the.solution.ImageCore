# About

[Get.the.solution.ImageCore](https://github.com/mfe-/Get.the.solution.ImageCore) is used in the app "[resize image](https://www.microsoft.com/en-us/p/resize-image/9p87m9tknkvl)", "[image-viewer](https://www.microsoft.com/en-us/p/image-viewer/9nd45c9v3zpw)" and was outsourced to this repository to make most parts of the application open source. The name "ImageCore" is kept general to cover other potential topics (regarding image manipulation) as well. Common functionallity can be used by other "image manipulation" apps, according to the licence (AGPL-3.0)

This repository contains functionality which is required to load images into an application for diffrent platforms like UWP and android.

- read or write files on uwp or android
- common image object which can be used on uwp or android
- displaying dialogs
- share service
- drag and drop

Another goal of this repository is to track [issues](https://github.com/mfe-/Get.the.solution.ImageCore/issues) regarding "[resize image](https://www.microsoft.com/en-us/p/resize-image/9p87m9tknkvl)", and "[image-viewer](https://www.microsoft.com/en-us/p/image-viewer/9nd45c9v3zpw)".

The last versions (offical and unoffical) of ["resize image" can be downloaded here](https://dev.azure.com/get-the-solution/get-the-solution/_build?definitionId=5)

## Project set up

Top to bottom

| Projectname  | Purpose   |
|---|---|
| Image.Manipulation.ViewModel  | Used by diffrent apps like [resize image](https://www.microsoft.com/en-us/p/resize-image/9p87m9tknkvl) or [resize.xf](https://play.google.com/store/apps/details?id=get.the.solution.Image.XF.Droid) |
| Image.Manipulation.Service.UWP  |  application services for the uwp platform |
| Image.Manipulation.ServiceBase  |  base service class used on all platforms  |
| Image.Manipulation.Contract | interfaces for common service layers |
| Image.Contract | interfaces which are only related to image processing |

## History

The project [filerenamer](https://www.mycsharp.de/wbb2/thread.php?threadid=115600) (done with WPF) is the predecessor of resize image. The idea of filerenamer was to rename mass images using the "date taken" attribute of the file. The next step was to resize images according to the users preferences.

At the time the app was created, UWP was the new and popular despite its restrictions (file system, file explorer integration). The app started with a single UWP project and grew over time. It took advantage of the new UWP features and the .NET Standard to make it easy to port the app to Xamarin.Forms.

Since then [FileRenamer](https://www.microsoft.com/de-at/p/file-renamer/9nblggh4rkqt?rtc=1) was ported to UWP.
