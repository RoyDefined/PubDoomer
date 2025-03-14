using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PubDoomer.Engine.Orchestration;

namespace PubDoomer.Project.Run;

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