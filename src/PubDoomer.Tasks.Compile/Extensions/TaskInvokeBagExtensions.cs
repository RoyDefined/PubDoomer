using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Tasks.Compile.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Tasks.Compile.Extensions;

public static class TaskInvokeBagExtensions
{
    internal const string AccCompilerExecutableFilePathKey = "AccCompilerExecutableFilePath";
    internal const string BccCompilerExecutableFilePathKey = "BccCompilerExecutableFilePath";
    internal const string GdccAccCompilerExecutableFilePathKey = "GdccAccCompilerExecutableFilePath";

    public static TaskInvokeBag SetAccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[AccCompilerExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetAccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, AccCompilerExecutableFilePathKey);
    }

    public static TaskInvokeBag SetBccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[BccCompilerExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetBccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, BccCompilerExecutableFilePathKey);
    }

    public static TaskInvokeBag SetGdccAccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[GdccAccCompilerExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetGdccAccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, GdccAccCompilerExecutableFilePathKey);
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
