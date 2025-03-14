using PubDoomer.ViewModels.Pages;

namespace PubDoomer.Factory;

/// <summary>
///     Represents the factory that is responsible for providing the view model of a page through the given delegate.
/// </summary>
public sealed class PageViewModelFactory(
    PageViewModelFactory.CreatePageViewModelDelegate @delegate)
{
    public delegate PageViewModel? CreatePageViewModelDelegate(string pageName);

    public PageViewModel? CreatePageViewModel(string pageName)
    {
        return @delegate(pageName);
    }
}