﻿using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public interface IFileService
    {
        Task<bool> HasGlobalWriteAccessAsync();
        Task CleanUpStorageItemTokensAsync(int removeOlderThanDays = 14);
    }
}
