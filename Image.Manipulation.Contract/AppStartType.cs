namespace Get.the.solution.Image.Manipulation.Contract
{
    public enum AppStartType
    {
        /// <summary>
        /// opened by calling the app (startmenue, commandline, ...)
        /// </summary>
        AppDirectStart,
        /// <summary>
        /// a file typ which is associated with the app was opened
        /// </summary>
        AppStartByFileAssociationType,
        /// <summary>
        /// The app was called as ShareTarget
        /// </summary>
        AppIsShareTarget,
        CommandLine,
    }
}
