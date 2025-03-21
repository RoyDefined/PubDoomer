﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Compile.Gdcc;

internal readonly struct GdccCompileResult
{
    public GdccCompileResult(GdccCompileResultType type, string message)
    {
        Type = type;
        Message = message;
    }

    public GdccCompileResultType Type { get; }
    public string Message { get; }
}
