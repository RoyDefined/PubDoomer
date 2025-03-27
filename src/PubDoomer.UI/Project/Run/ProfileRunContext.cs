using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.TaskInvokation.Task;
using PubDoomer.Engine.Tasks;

namespace PubDoomer.Project.Run;

/// <summary>
/// Represents the main context of a profile that contains invokable tasks that can be invoked.
/// </summary>
public partial class ProfileRunContext : ObservableObject
{
    /// <summary>
    /// The name of the profile to display in the UI.
    /// </summary>
    [ObservableProperty] private string _name;
    
    /// <summary>
    /// The tasks to be invoked in order.
    /// </summary>
    [ObservableProperty] private ObservableCollection<ProfileRunTask> _tasks;

    /// <summary>
    /// Delegated from each task. Contains found validation results.
    /// <br /> Can be completely empty, meaning the tasks yielded no warnings/errors.
    /// </summary>
    [ObservableProperty] private ObservableCollection<TaskValidationCollection>? _validations;

    /// <summary>
    /// The status of the profile. Indicates how invokation goes.
    /// </summary>
    [ObservableProperty] private ProfileRunContextStatus _status;
    
    /// <summary>
    /// The total elapsed time that the profile ran for.
    /// <br /> If <c>null</c>, the profile has not run yet.
    /// </summary>
    [ObservableProperty] private int? _elapsedTimeMs;

    public ProfileRunContext(
        string name,
        IEnumerable<ProfileRunTask> tasks)
    {
        Name = name;
        Tasks = new ObservableCollection<ProfileRunTask>(tasks);
    }

    /// <summary>
    /// Fully validates the current profile to ensure it can run properly.
    /// <br/> Validation is before the tasks are invoked. This means it does not guarantee the tasks run succesfully at runtime.
    /// </summary>
    public void ValidateContext()
    {
        var validationResultsNullable = Tasks
            .Where(x => x.Task is IValidatableTask)
            .Select(x =>
        {
            var validatableTask = (IValidatableTask)x.Task;
            var results = validatableTask.Validate();
            var collection = new TaskValidationCollection(x, results);

            // Return `null` if no validation results were found.
            // This is filtered below.
            if (collection.Results.Count == 0)
            {
                return null;
            }

            return collection;
        });

        var validationResults = validationResultsNullable
            .Where(x => x != null)
            .Cast<TaskValidationCollection>();

        Validations = new ObservableCollection<TaskValidationCollection>(validationResults);
    }
}