using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using PubDoomer.Engine.Tasks;

namespace PubDoomer.Project.Run;

public partial class ProfileRunContext : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private ObservableCollection<ProfileRunTask> _tasks;

    // Delegated from each task. Contains found validation results.
    // Can be completely empty, meaning the tasks yielded no warnings/errors.
    [ObservableProperty] private ObservableCollection<TaskValidationCollection>? _validations;

    // The status of the profile.
    [ObservableProperty] private ProfileRunContextStatus _status;
    
    // The total elapsed time that the profile ran for.
    [ObservableProperty] private int? _elapsedTimeMs;

    public ProfileRunContext(
        string name,
        IEnumerable<ProfileRunTask> tasks)
    {
        Name = name;
        Tasks = new ObservableCollection<ProfileRunTask>(tasks);
    }

    // Fully validates the current profile to ensure it can run properly.
    // This obviously does not imply that it actuall succeeds since there can still be issues during runtime.
    public void ValidateContext()
    {
        var validationResultsNullable = Tasks.Select(x =>
        {
            var results = x.Task.Validate();
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