using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PubDoomer.Engine.Validation;

namespace PubDoomer.Project.Run;

/// <summary>
/// A collection that contains validation results of a given task.
/// </summary>
public sealed class TaskValidationCollection
{
    public TaskValidationCollection(
        ProfileRunTask task,
        IEnumerable<ValidateResult> results)
    {
        Task = task;
        Results = new ObservableCollection<ValidateResult>(results);
    }

    public ProfileRunTask Task { get; }
    public Collection<ValidateResult> Results { get; }
}