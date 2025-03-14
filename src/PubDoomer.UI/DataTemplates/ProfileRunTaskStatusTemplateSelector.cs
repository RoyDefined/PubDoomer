using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using PubDoomer.Project.Run;

namespace PubDoomer.DataTemplates;

public sealed class ProfileRunTaskStatusTemplateSelector : IDataTemplate
{
    public Control Build(object? param)
    {
        if (param is not ProfileRunTaskStatus status) return new TextBlock { Text = "Invalid" };

        return status switch
        {
            ProfileRunTaskStatus.Pending => new TextBlock { Text = "Pending...", Foreground = Brushes.Gray },
            ProfileRunTaskStatus.Running => new ProgressBar { IsIndeterminate = true },
            ProfileRunTaskStatus.Success => new TextBlock { Text = "✔ Success", Foreground = Brushes.Green },
            ProfileRunTaskStatus.Error => new TextBlock { Text = "❌ Error", Foreground = Brushes.Red },
            _ => new TextBlock { Text = "Unknown" }
        };
    }

    public bool Match(object? data)
    {
        return data is ProfileRunTask;
    }
}