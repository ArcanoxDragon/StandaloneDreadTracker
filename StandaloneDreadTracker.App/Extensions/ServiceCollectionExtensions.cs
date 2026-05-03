using Arcanox.AppCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.App.Extensions;

public static class ServiceCollectionExtensions
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddApplicationServices()
		{
			services.ConfigureAppDataFolder(AppConstants.AppDataFolderName);
			services.AddSettingsManager("settings.json", SettingsJsonContext.Default.ApplicationSettings);
			services.AddSingleton<TrackerManager>();
			services.RegisterViewModels();
			return services;
		}

		public IServiceCollection AddAlias<TService, TAlias>()
		where TService : class, TAlias
		where TAlias : class
		{
			services.TryAddEnumerable(ServiceDescriptor.Transient<TAlias, TService>(provider => provider.GetRequiredService<TService>()));
			return services;
		}

		private void RegisterViewModels()
		{
			services.AddTransient<MainViewModel>();
		}
	}
}