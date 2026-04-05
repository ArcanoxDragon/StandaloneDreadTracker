using System;
using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Svg.Skia;
using ReactiveUI.Avalonia;

namespace StandaloneDreadTracker.UI.Avalonia.Android;

[Activity(
	Label = "StandaloneDreadTracker.UI.Avalonia.Android",
	Theme = "@style/MyTheme.NoActionBar",
	Icon = "@drawable/icon",
	MainLauncher = true,
	ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
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
}