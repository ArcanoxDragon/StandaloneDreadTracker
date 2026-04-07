using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Svg.Skia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia;
using StandaloneDreadTracker.App.Services;
using StandaloneDreadTracker.UI.Avalonia.Desktop.Services;
using StandaloneDreadTracker.UI.Avalonia.Extensions;

namespace StandaloneDreadTracker.UI.Avalonia.Desktop;

internal sealed class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		BuildAvaloniaApp()
			.StartWithClassicDesktopLifetime(args);
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
	{
		if (Design.IsDesignMode)
		{
			GC.KeepAlive(typeof(SvgImageExtension).Assembly);
			GC.KeepAlive(typeof(global::Avalonia.Svg.Skia.Svg).Assembly);
		}

		var services = new ServiceCollection();

		ConfigureServices(services);

		var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions {
			ValidateOnBuild = true,
			ValidateScopes = true,
		});

		return AppBuilder.Configure(serviceProvider.GetRequiredService<App>)
			.UsePlatformDetect()
			.WithInterFont()
			.UseReactiveUI(_ => { })
			.LogToTrace();
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		services.AddAvaloniaServices();
		services.AddScoped<IDialogs, DesktopAvaloniaDialogs>();
	}
}