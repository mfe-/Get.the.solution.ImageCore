namespace Get.the.solution.Image.Contract
{
    public interface IEditPixelOperators : IEditPixelOperator
    {
        void EditPixels(ref byte b1, ref byte g1, ref byte r1, ref byte a1, ref byte b2, ref byte g2, ref byte r2, ref byte a2);
    }
    public interface IEditPixelOperators<TResult> : IEditPixelOperators
    {
        TResult Result { get; }
    }
}
