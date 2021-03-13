using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Image.Operation.Test
{
    public class KernelOperationTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public KernelOperationTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        [Fact]
        public void Kernel_with_wrong_size_should_throw_exception()
        {
            double[,] kernel = new double[,]
            {
                {0.1,0.1,0.1 },
                {0.1,0.1,0.1 },
            };
            Assert.Throws<ArgumentException>(() => new KernelOperation(kernel));
        }
        [Fact]
        public void Neighborhood_filtering_with3x3_kernel_should_return_expected_output()
        {
            double[,] kernel = new double[,]
            {
                {0.1,0.1,0.1 },
                {0.1,0.2,0.1 },
                {0.1,0.1,0.1 }
            };
            KernelOperation kernelOperation = new KernelOperation(kernel);
            //Sample from Computer Vision: Algorithms and Applications (September 3, 2010) S112
            byte[][] image = new byte[][]
            {
                 new byte[] { 45, 60, 98, 127, 132, 133, 137, 133 }
                ,new byte[] { 46, 65, 98, 123, 126, 128, 131, 133 }
                ,new byte[] { 47, 65, 96, 115, 119, 123, 135, 137 }
                ,new byte[] { 47, 63, 91, 107, 113, 122, 138, 134 }
                ,new byte[] { 50, 59, 80, 97 , 110, 123, 133, 134 }
                ,new byte[] { 49, 53, 68, 83 , 97 , 113, 128, 133 }
                ,new byte[] { 50, 50, 58, 70 , 84 , 102, 116, 126 }
                ,new byte[] { 50, 50, 52, 58 , 69 , 86 , 101, 120 }
            };
            Print(image);
            image = kernelOperation.Process(ref image);
            Print(image);

            byte[][] expected = new byte[][]
            {
                 new byte[] { 26, 47, 67, 83, 90, 92, 93, 67 }
                ,new byte[] { 37, 68, 94, 116, 125, 129, 132, 94 }
                ,new byte[] { 38, 68,   92, 110, 120, 126, 132,   95 }
                ,new byte[] { 38, 66,   86, 104, 114, 124, 132,   94 }
                ,new byte[] { 37, 62,   78, 94,  108, 120, 129,   93 }
                ,new byte[] { 36, 57,   69, 83,  98, 112, 124,    90 }
                ,new byte[] { 35, 53,   60, 71,  85, 100, 114,    85 }
                ,new byte[] { 25, 36, 39, 45,  54 ,64, 75, 58 }
            };
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], image[i]);
            }

        }
        public void Print(byte[][] vs)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in vs)
            {
                foreach (var i in item)
                {
                    stringBuilder.Append($"{i} ");
                }
                stringBuilder.Append(Environment.NewLine);
            }
            _testOutputHelper.WriteLine(stringBuilder.ToString());
        }
    }
}
