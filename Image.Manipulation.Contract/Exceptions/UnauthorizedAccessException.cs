using System;
using System.Collections.Generic;
using System.Text;

namespace Get.the.solution.Image.Manipulation.Contract.Exceptions
{
    public class UnauthorizedAccessException : Exception
    {
        public UnauthorizedAccessException() : base() { }
        public UnauthorizedAccessException(Exception exception) : base(nameof(UnauthorizedAccessException),exception)
        {
        }
    }
}
