# About

[Get.the.solution.ImageCore](https://github.com/mfe-/Get.the.solution.ImageCore)

The name get.the.solution.ImageCore (aka resize image) of the project is kept general to cover other potential topics (regarding image manipulation) as well.

The goal of this repository is to track issues regarding [resize image](https://www.microsoft.com/en-us/p/resize-image/9p87m9tknkvl) and to make parts of the application public available. This repository contains functionality which are required to load images into an application for diffrent platforms like UWP and android.

- read or write files on uwp or android
- common image object which can be used on uwp or android
- displaying dialogs
- share service
- drag and drop
- resize service (using external lib depends on the platform)

## Project set up

Top to bottom

|   |   |
|---|---|
| Image.Manipulation.ViewModel  | Used by diffrent apps like [resize image](https://www.microsoft.com/en-us/p/resize-image/9p87m9tknkvl) or [resize.xf](https://play.google.com/store/apps/details?id=get.the.solution.Image.XF.Droid) |
| Image.Manipulation.Service.UWP  |  application services for the uwp platform |
| Image.Manipulation.ServiceBase  |  base service class used on all platforms  |
| Image.Manipulation.Contract | interfaces for common service layers |
| Image.Contract | interfaces which are only related to image processing |

## History

The project [filerenamer](https://www.mycsharp.de/wbb2/thread.php?threadid=115600) (done with WPF) is the predecessor of resize image. The idea of filerenamer was to rename mass images using the "date taken" attribute of the file. The next step was to resize images according to the users preferences.

At the time, the app was created, UWP was the new thing despite its restrictions (file system, file explorer integration). The app started with a single UWP project and grew over time. It took advantage of the new UWP features and the .NET Standard to make it easy to port the app to Xamarin.Forms. Currently bringing the app to Xamarin.Android and Xamarin.IOS is still in progress.

The [FileRenamer](https://www.microsoft.com/de-at/p/file-renamer/9nblggh4rkqt?rtc=1) was ported meanwhile to UWP.
