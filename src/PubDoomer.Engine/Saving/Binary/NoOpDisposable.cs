using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Saving.Binary;

internal sealed class NoOpDisposable : IDisposable
{
    public void Dispose() { }
}
