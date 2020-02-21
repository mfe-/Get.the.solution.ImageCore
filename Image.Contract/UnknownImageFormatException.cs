using System;

namespace Get.the.solution.Image.Contract.Exceptions
{
    public class UnknownImageFormatException : Exception
    {
        public UnknownImageFormatException()
        {
        }
        public UnknownImageFormatException(string message) : base(message)
        {
        }
        public UnknownImageFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
