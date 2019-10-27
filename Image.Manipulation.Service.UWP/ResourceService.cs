using Get.the.solution.Image.Manipulation.Contract;
using Windows.ApplicationModel.Resources;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class ResourceService : IResourceService
    {
        protected ResourceLoader _resources;
        public ResourceService()
        {
            _resources = ResourceLoader.GetForViewIndependentUse("Get.the.solution.Image.Manipulation.Resources/Resources");
        }
        public string GetString(string resource)
        {
            return _resources.GetString(resource);
        }
    }
}
