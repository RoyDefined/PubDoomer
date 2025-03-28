using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.Process;

public sealed record class ProcessInvokeContext(string ExeFilePath, IEnumerable<string> Arguments, Stream? stdOutStream, Stream? stdErrStream);
