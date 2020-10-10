using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface ILocalSettings<TSetting>
    {
        /// <summary>
        /// Loads the current settings
        /// </summary>
        /// <returns></returns>
        Task LoadSettingsAsync();
        /// <summary>
        /// saves the current settings
        /// </summary>
        /// <returns></returns>
        Task SaveSettingsAsync();
        /// <summary>
        /// rests the settings, use this if the settings are faulty
        /// </summary>
        /// <returns></returns>
        Task ResetSettingsAsync();
        /// <summary>
        /// the current settings object
        /// </summary>
        TSetting Settings { get; set; }
    }
}
