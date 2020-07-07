namespace Get.the.solution.Image.Contract
{
    /// <summary>
    /// Action with ref parameter for pixel operationss
    /// </summary>
    /// <typeparam name="T">Generic parameter for pixel</typeparam>
    /// <param name="b">blue value</param>
    /// <param name="g">green value</param>
    /// <param name="r">red value</param>
    /// <param name="a">alpha value</param>
    public delegate void ActionRef<T>(ref T b, ref T g, ref T r, ref T a);
    /// <summary>
    /// Action with ref parameter for pixel operationss
    /// </summary>
    /// <typeparam name="T">Generic parameter for pixel</typeparam>
    /// <param name="b">blue value</param>
    /// <param name="g">green value</param>
    /// <param name="r">red value</param>
    /// <param name="a">alpha value</param>
    public delegate void ActionRefs<T>(ref T b1, ref T g1, ref T r1, ref T a1, ref T b2, ref T g2, ref T r2, ref T a2);
    /// <summary>
    /// Action with ref parameter for pixel operationss
    /// </summary>
    /// <typeparam name="T">Generic parameter for pixel</typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="b1"></param>
    /// <param name="g1"></param>
    /// <param name="r1"></param>
    /// <param name="a1"></param>
    /// <param name="b2"></param>
    /// <param name="g2"></param>
    /// <param name="r2"></param>
    /// <param name="a2"></param>
    /// <returns></returns>
    public delegate TResult FuncRefs<T, out TResult>(ref T b1, ref T g1, ref T r1, ref T a1, ref T b2, ref T g2, ref T r2, ref T a2);
}
