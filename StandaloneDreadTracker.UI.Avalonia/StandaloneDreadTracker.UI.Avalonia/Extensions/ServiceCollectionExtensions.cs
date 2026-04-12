using System;
using System.IO;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using StandaloneDreadTracker.App.Extensions;
using StandaloneDreadTracker.App.Utility;
using StandaloneDreadTracker.UI.Avalonia.Services;

namespace StandaloneDreadTracker.UI.Avalonia.Extensions;

internal static class ServiceCollectionExtensions
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddAvaloniaServices()
		{
			services.AddLogging(logging => {
				var logPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConstants.AppDataFolderName, "Logs", "application.log");
				var serilogLogger = new LoggerConfiguration()
#if DEBUG
					.MinimumLevel.Debug()
#else
					.MinimumLevel.Information()
#endif
					.Enrich.FromLogContext()
					.WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
					.CreateLogger();

#if DEBUG
				logging.SetMinimumLevel(LogLevel.Debug);
				logging.AddDebug();
#endif

				logging.AddConsole();
				logging.AddSerilog(serilogLogger);
			});

			services.AddSingleton<App>();
			services.AddAlias<App, Application>();
			services.AddScoped<WindowContext>();
			services.AddApplicationServices();

			return services;
		}
	}
}