using System;
using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Svg.Skia;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Avalonia;
using StandaloneDreadTracker.UI.Avalonia.Extensions;

namespace StandaloneDreadTracker.UI.Avalonia.Android;

[Activity(
	Label = "StandaloneDreadTracker.UI.Avalonia.Android",
	Theme = "@style/MyTheme.NoActionBar",
	Icon = "@drawable/icon",
	MainLauncher = true,
	ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
	private readonly IServiceProvider serviceProvider;

	public MainActivity()
	{
		var services = new ServiceCollection();

		ConfigureServices(services);

		this.serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions {
			ValidateOnBuild = true,
			ValidateScopes = true,
		});
	}

	protected override AppBuilder CreateAppBuilder()
		=> AppBuilder.Configure(() => this.serviceProvider.GetRequiredService<App>()).UseAndroid();

	protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
	{
		if (Design.IsDesignMode)
		{
			GC.KeepAlive(typeof(SvgImageExtension).Assembly);
			GC.KeepAlive(typeof(global::Avalonia.Svg.Skia.Svg).Assembly);
		}

		return base.CustomizeAppBuilder(builder)
			.WithInterFont()
			.UseReactiveUI(_ => { });
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		services.AddAvaloniaServices();
	}
}