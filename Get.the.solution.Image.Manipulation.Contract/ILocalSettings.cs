﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface ILocalSettings
    {
        IDictionary<string, object> Values { get; }
    }
}
