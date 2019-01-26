#About

The name get.the.solution.image (aka resize image) of the project is kept general to cover other potential topics as well. Currently the main goal of the app is to resize images.
#####UWP
[![Build status](https://build.appcenter.ms/v0.1/apps/9f10628c-39c0-4311-bcb9-178f4e0e27cb/branches/master/badge)](https://appcenter.ms)
#####XF
[![Build status](https://femartin.visualstudio.com/Get.the.solution.Image/_apis/build/status/Get.the.solution.Image-Xamarin.Android-CI%20(1))](https://femartin.visualstudio.com/Get.the.solution.Image/_build/latest?definitionId=3)
#History
The project [filerenamer](https://www.mycsharp.de/wbb2/thread.php?threadid=115600) (done with WPF) is the predecessor of resize image. The idea of filerenamer was to rename mass images using the "date taken" attribute of the file. The next step was to resize images according to the users preferences.

At the time the app was created, UWP was the new thing despite its restrictions (file system, file explorer integration). The app started with a single UWP project and grew over time. It took advantage of the new UWP features and the .NET Standard to make it easy to port the app to Xamarin.Forms. Currently bringing the app to Xamarin.Android and Xamarin.IOS is still in progress.  The [FileRenamer](https://www.microsoft.com/de-at/p/file-renamer/9nblggh4rkqt?rtc=1) was ported meanwhile to UWP.

[18:16:07] <thiago> martin-_-_: no, this isn't a common activity
[18:16:20] Bobdude!~Tilde@2605:6000:101e:42a6:bcf2:b5ae:fb66:9aeb has joined
[18:16:32] <thiago> martin-_-_: what you're looking for: git filter-branch with --directory-filter to extract the directory into a completely new history, then you publish that repo
[18:16:46] <j416> Soni: you can try to weak the params to repack to have it compress potentially better, but in most cases it won't be a significant improvement.
[18:16:48] <thiago> martin-_-_: then git rm -r dirname/ in the old repository and reimport as git submodule
[18:16:59] <martin-_-_> thanks thiago
[18:17:12] <thiago> martin-_-_: hint: usually, when publishing old proprietary content, you don't publish history