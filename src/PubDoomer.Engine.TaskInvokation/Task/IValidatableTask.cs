using PubDoomer.Engine.TaskInvokation.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PubDoomer.Engine.TaskInvokation.Task;

public interface IValidatableTask
{
    IEnumerable<ValidateResult> Validate();
}
