using CommunityToolkit.Mvvm.ComponentModel;

namespace PubDoomer.Tasks.Compile.Observables;

public partial class ObservableString : ObservableObject
{
    [ObservableProperty] private string? _value;
}