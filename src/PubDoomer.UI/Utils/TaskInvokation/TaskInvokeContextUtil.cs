using PubDoomer.Engine.TaskInvokation.Context;
using PubDoomer.Settings.Merged;
using PubDoomer.Tasks.AcsVM.Extensions;
using PubDoomer.Tasks.Compile.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Utils.TaskInvokation;

internal static class TaskInvokeContextUtil
{
    /// <summary>
    /// Builds the invoke context used to run profiles.
    /// </summary>
    internal static TaskInvokeContext BuildContext(MergedSettings settings)
    {
        var context = new TaskInvokeContext();
        context.ContextBag
            .SetAccCompilerExecutableFilePath(settings.AccCompilerExecutableFilePath)
            .SetBccCompilerExecutableFilePath(settings.BccCompilerExecutableFilePath)
            .SetGdccAccCompilerExecutableFilePath(settings.GdccAccCompilerExecutableFilePath)
            .SetGdccCcCompilerExecutableFilePath(settings.GdccCcCompilerExecutableFilePath)
            .SetGdccMakeLibCompilerExecutableFilePath(settings.GdccMakeLibCompilerExecutableFilePath)
            .SetGdccLdCompilerExecutableFilePath(settings.GdccLdCompilerExecutableFilePath)
            .SetAcsVmExecutableFilePath(settings.AcsVmExecutableFilePath);

        return context;
    }
}
