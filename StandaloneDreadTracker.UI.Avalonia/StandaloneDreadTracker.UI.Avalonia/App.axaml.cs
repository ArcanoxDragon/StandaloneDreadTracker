using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.ViewModels;
using StandaloneDreadTracker.UI.Avalonia.Services;
using StandaloneDreadTracker.UI.Avalonia.Views;

namespace StandaloneDreadTracker.UI.Avalonia;

public class App(IServiceProvider serviceProvider) : Application
{
	public IServiceProvider ServiceProvider { get; } = serviceProvider;

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);

		// Load and save settings each time the app starts, which will create the config file if it doesn't exist
		var settingsManager = ServiceProvider.GetRequiredService<ISettingsManager>();

		settingsManager.Modify(_ => { });
	}

	public override void OnFrameworkInitializationCompleted()
	{
		var mainWindowScope = ServiceProvider.CreateScope();
		var mainWindowContext = mainWindowScope.ServiceProvider.GetRequiredService<WindowContext>();
		var mainViewModel = mainWindowScope.ServiceProvider.GetRequiredService<MainViewModel>();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			var window = new MainWindow {
				DataContext = mainViewModel,
			};

			mainWindowContext.CurrentWindow = window;
			desktop.MainWindow = window;
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			singleViewPlatform.MainView = new MainView {
				DataContext = mainViewModel,
			};
		}

		base.OnFrameworkInitializationCompleted();
	}
}