using Microsoft.Extensions.DependencyInjection;
using PubDoomer.ViewModels.Pages;
using PubDoomer.Views.Pages;

namespace PubDoomer.Factory;

public static class PageViewModelExtensions
{
    public static IServiceCollection AddPageViewModels(this IServiceCollection services)
    {
        // Add key-value pairs to point to view model types.
        services
            .AddKeyedTransient<PageViewModel, HomePageViewModel>("Home")
            .AddKeyedTransient<PageViewModel, ProfilesPageViewModel>("Profiles")
            .AddKeyedTransient<PageViewModel, TasksPageViewModel>("Tasks")
            .AddKeyedTransient<PageViewModel, ProjectPageViewModel>("Project")
            .AddKeyedTransient<PageViewModel, SettingsPageViewModel>("Settings")
            .AddKeyedTransient<PageViewModel, CodePageViewModel>("Code")
            .AddKeyedTransient<PageViewModel, EditMapPageViewModel>("EditMap")
            .AddKeyedTransient<PageViewModel, RunMapPageViewModel>("RunMap");

        return services

            // The delegate responsible for getting the page view model under a given key.
            .AddTransient<PageViewModelFactory.CreatePageViewModelDelegate>(
                x => x.GetKeyedService<PageViewModel>)

            // The actual factory implementing the providing of the view model.
            .AddTransient<PageViewModelFactory>();
    }
}