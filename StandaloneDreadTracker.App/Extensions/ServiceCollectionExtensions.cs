using Arcanox.AppCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
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

		private void RegisterViewModels()
		{
			services.AddTransient<MainViewModel>();
		}
	}
}