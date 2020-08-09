namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IResourceService
    {
        //
        // Summary:
        //     Gets the value of the named resource.
        //
        // Parameters:
        //   resource:
        //     The resource name.
        //
        // Returns:
        //     The named resource value.
        string GetString(string resource);
    }
}
