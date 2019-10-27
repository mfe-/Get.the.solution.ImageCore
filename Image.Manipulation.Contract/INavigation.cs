using System;

namespace Get.the.solution.Image.Manipulation.Contract
{
    //
    // Summary:
    //     The INavigationService interface is used for creating a navigation service for
    //     your Windows Store app. The default implementation of INavigationService is the
    //     FrameNavigationService class, that uses a class that implements the IFrameFacade
    //     interface to provide page navigation.
    public interface INavigation
    {
        bool Navigate(Type pageToken, object parameter);
    }
}
