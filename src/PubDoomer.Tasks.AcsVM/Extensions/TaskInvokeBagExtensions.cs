using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Tasks.AcsVM.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Tasks.AcsVM.Extensions;

public static class TaskInvokeBagExtensions
{
    public static TaskInvokeBag SetAcsVmExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[AcsVmTaskStatics.AcsVmExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetAcsVmExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, AcsVmTaskStatics.AcsVmExecutableFilePathKey);
    }

    private static string GetCompilerExecutableFilePathByKey(TaskInvokeBag taskInvokeBag, string key)
    {
        if (!taskInvokeBag.TryGetValue(key, out var filePathMaybe))
        {
            ThrowHelper.ThrowConfigurationKeyNotFoundException();
        }

        if (filePathMaybe is not string filePath)
        {
            ThrowHelper.ThrowConfigurationInvalidCastException();
            return null; // Unreachable, but the compiler doesn't know.
        }

        return filePath;
    }
}
