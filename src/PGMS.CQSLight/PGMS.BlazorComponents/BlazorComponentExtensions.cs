using Blazorise;
using Microsoft.Extensions.DependencyInjection;

namespace PGMS.BlazorComponents;

public static class BlazorComponentExtensions
{
    public static void AddPgmsBlazorComponents(this IServiceCollection serviceCollection, Action<BlazorComponentsOptions> configureOptions = null)
    {

        // If options handler is not defined we will get an exception so
        // we need to initialize an empty action.
        configureOptions ??= _ => { };

        serviceCollection.AddSingleton(configureOptions);
        serviceCollection.AddSingleton<BlazorComponentsOptions>();

    }
}

public class BlazorComponentsOptions
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// A default constructors for <see cref="BlazorComponentsOptions"/>.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    /// <param name="configureOptions">A handler for setting the BlazorComponents options.</param>
    public BlazorComponentsOptions(IServiceProvider serviceProvider, Action<BlazorComponentsOptions> configureOptions)
    {
        this.serviceProvider = serviceProvider;

        ActionModalSettings = new ActionModalSettings();

        configureOptions?.Invoke(this);
    }

    public ActionModalSettings ActionModalSettings { get; set; }
}

public class ActionModalSettings
{
    public bool PreventClose { get; set; }
}