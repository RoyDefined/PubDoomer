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
    public static TaskInvokeBag SetAccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[CompileTaskStatics.AccCompilerExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetAccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, CompileTaskStatics.AccCompilerExecutableFilePathKey);
    }

    public static TaskInvokeBag SetBccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[CompileTaskStatics.BccCompilerExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetBccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, CompileTaskStatics.BccCompilerExecutableFilePathKey);
    }

    public static TaskInvokeBag SetGdccAccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[CompileTaskStatics.GdccAccCompilerExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetGdccAccCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, CompileTaskStatics.GdccAccCompilerExecutableFilePathKey);
    }

    public static TaskInvokeBag SetGdccCcCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[CompileTaskStatics.GdccCcCompilerExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetGdccCcCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, CompileTaskStatics.GdccCcCompilerExecutableFilePathKey);
    }

    public static TaskInvokeBag SetGdccMakeLibCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[CompileTaskStatics.GdccMakeLibCompilerExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetGdccMakeLibCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, CompileTaskStatics.GdccMakeLibCompilerExecutableFilePathKey);
    }

    public static TaskInvokeBag SetGdccLdCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag, string? filePath)
    {
        taskInvokeBag[CompileTaskStatics.GdccLdCompilerExecutableFilePathKey] = filePath;
        return taskInvokeBag;
    }

    public static string GetGdccLdCompilerExecutableFilePath(this TaskInvokeBag taskInvokeBag)
    {
        return GetCompilerExecutableFilePathByKey(taskInvokeBag, CompileTaskStatics.GdccLdCompilerExecutableFilePathKey);
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
