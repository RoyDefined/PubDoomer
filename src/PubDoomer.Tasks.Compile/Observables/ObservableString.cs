using CommunityToolkit.Mvvm.ComponentModel;

namespace PubDoomer.Tasks.Compile.Observables;

public partial class ObservableString : ObservableObject
{
    public ObservableString()
    {
    }

    public ObservableString(string? value)
    {
        Value = value;
    }

    [ObservableProperty] private string? _value;
}