using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables.Fluent;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.ViewModels;

public partial class EditTrackerViewModel : ViewModelBase
{
	public EditTrackerViewModel()
	{
		this.WhenAnyValue(
				m => m.IsNameValid,
				m => m.IsTargetTypeValid,
				m => m.IsIpAddressValid,
				(nameValid, typeValid, addressValid) => nameValid && typeValid && addressValid)
			.ToProperty(this, m => m.CanSave, out this._canSaveHelper);

		this.WhenActivated(disposables => {
			Tracker
				.WhenAnyValue(t => t.Name, name => !string.IsNullOrWhiteSpace(name))
				.ToProperty(this, m => m.IsNameValid, out this._isNameValidHelper)
				.DisposeWith(disposables);

			Tracker
				.WhenAnyValue(t => t.TargetType, type => type is TrackerTargetType.Remote or TrackerTargetType.LocalEmulator)
				.ToProperty(this, m => m.IsTargetTypeValid, out this._isTargetTypeValidHelper)
				.DisposeWith(disposables);

			Tracker
				.WhenAnyValue(t => t.TargetType, type => type == TrackerTargetType.Remote)
				.ToProperty(this, m => m.IsIpAddressVisible, out this._isIpAddressVisibleHelper)
				.DisposeWith(disposables);

			Tracker
				.WhenAnyValue(t => t.TargetAddress, t => t.TargetType, ValidateIpAddress)
				.ToProperty(this, m => m.IsIpAddressValid, out this._isIpAddressValidHelper)
				.DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial TrackerViewModel Tracker { get; set; } = new();

	[ObservableAsProperty(ReadOnly = false)]
	public partial bool IsNameValid { get; }

	[ObservableAsProperty(ReadOnly = false)]
	public partial bool IsTargetTypeValid { get; }

	[ObservableAsProperty(ReadOnly = false)]
	public partial bool IsIpAddressVisible { get; }

	[ObservableAsProperty(ReadOnly = false)]
	public partial bool IsIpAddressValid { get; }

	[ObservableAsProperty]
	public partial bool CanSave { get; }

	private bool ValidateIpAddress(string? ipAddress, TrackerTargetType targetType)
	{
		if (targetType != TrackerTargetType.Remote)
			return true;

		if (string.IsNullOrEmpty(ipAddress))
			return false;

		if (!IPAddress.TryParse(ipAddress, out var parsed))
			return false;

		// Technically, IPv4 addresses with fewer than 4 segments are considered valid,
		// but such address notations are extremely uncommon and not likely to work.
		// We want to only accept full-notation addresses.
		if (ipAddress.Count('.') < 3)
			return false;

		// Ensure the address is IPv4
		return parsed.AddressFamily == AddressFamily.InterNetwork;
	}
}