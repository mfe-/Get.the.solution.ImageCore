namespace Get.the.solution.Image.Contract
{
    public interface IEditPixelOperator
    {
        void EditPixel(ref byte b1, ref byte g1, ref byte r1, ref byte a1);
        object SetResult();
    }
    public interface IEditPixelOperator<TResult> : IEditPixelOperator
    {
        TResult Result { get; }
    }
}
