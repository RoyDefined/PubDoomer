using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Tasks.Compile.Utils;

internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void ThrowConfigurationKeyNotFoundException()
    {
        throw new KeyNotFoundException("Failed to retrieve executable file path for ACS VM: the configuration does not exist.");
    }

    [DoesNotReturn]
    internal static void ThrowConfigurationInvalidCastException()
    {
        throw new InvalidCastException("Failed to retrieve executable file path for ACS VM: the configured executable file path is not a valid file path.");
    }
}
