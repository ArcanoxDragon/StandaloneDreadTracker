using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using StandaloneDreadTracker.App.Services;
using StandaloneDreadTracker.UI.Avalonia.Services;
using StandaloneDreadTracker.UI.Avalonia.Views.Dialogs;

namespace StandaloneDreadTracker.UI.Avalonia.Desktop.Services;

internal class DesktopAvaloniaDialogs(WindowContext windowContext) : IDialogs
{
	private Window ParentWindow
	{
		get
		{
			var parentWindow = windowContext.CurrentWindow;

			if (parentWindow is null && Application.Current is { ApplicationLifetime: IClassicDesktopStyleApplicationLifetime desktopLifetime })
				parentWindow = desktopLifetime.MainWindow;

			if (parentWindow is null)
				throw new InvalidOperationException("Could not find a parent window for which to show the dialog!");

			return parentWindow;
		}
	}

	public IObservable<Unit> Alert(string title, string message, string? buttonText = null)
		=> Observable.FromAsync(() => AlertAsync(title, message, buttonText));

	public async Task AlertAsync(string title, string message, string? buttonText = null)
	{
		await Dispatcher.UIThread.InvokeAsync(async () => {
			var dialog = new AlertDialog { Title = title, Message = message };

			if (buttonText != null)
				dialog.ButtonText = buttonText;

			await dialog.ShowDialog(ParentWindow);
		});
	}

	public IObservable<bool> Confirm(string title, string message, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
		=> Observable.FromAsync(() => ConfirmAsync(title, message, positiveText, negativeText, positiveButtonAccent));

	public async Task<bool> ConfirmAsync(string title, string message, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	{
		return await Dispatcher.UIThread.InvokeAsync(async () => {
			var dialog = new ConfirmDialog { Title = title, Message = message, PositiveButtonAccent = positiveButtonAccent };

			if (positiveText != null)
				dialog.PositiveText = positiveText;
			if (negativeText != null)
				dialog.NegativeText = negativeText;

			return await dialog.ShowDialog<bool>(ParentWindow);
		});
	}

	public IObservable<string?> Prompt(string title, string message, string? defaultValue = null, string? inputWatermark = null, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
		=> Observable.FromAsync(() => PromptAsync(title, message, defaultValue, inputWatermark, positiveText, negativeText, positiveButtonAccent));

	public async Task<string?> PromptAsync(string title, string message, string? defaultValue = null, string? inputWatermark = null, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true)
	{
		return await Dispatcher.UIThread.InvokeAsync(async () => {
			var dialog = new PromptDialog { Title = title, Message = message, PositiveButtonAccent = positiveButtonAccent };

			if (defaultValue != null)
				dialog.InputText = defaultValue;
			if (inputWatermark != null)
				dialog.InputWatermark = inputWatermark;
			if (positiveText != null)
				dialog.PositiveText = positiveText;
			if (negativeText != null)
				dialog.NegativeText = negativeText;

			return await dialog.ShowDialog<string?>(ParentWindow);
		});
	}
}