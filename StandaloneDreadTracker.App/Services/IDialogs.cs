using System.Reactive;
using StandaloneDreadTracker.App.ViewModels;

namespace StandaloneDreadTracker.App.Services;

public interface IDialogs
{
	IObservable<Unit> Alert(string title, string message, string? buttonText = null);
	Task AlertAsync(string title, string message, string? buttonText = null);

	IObservable<bool> Confirm(string title, string message, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true);
	Task<bool> ConfirmAsync(string title, string message, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true);

	IObservable<string?> Prompt(string title, string message, string? defaultValue = null, string? inputWatermark = null, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true);
	Task<string?> PromptAsync(string title, string message, string? defaultValue = null, string? inputWatermark = null, string? positiveText = null, string? negativeText = null, bool positiveButtonAccent = true);

	Task<bool> EditTrackerAsync(TrackerViewModel tracker, string? title = "Edit Tracker");
}