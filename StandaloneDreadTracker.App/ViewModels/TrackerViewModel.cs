using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using DreadRemoteConnector;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.ViewModels;

public partial class TrackerViewModel : ViewModelBase
{
	#region Static Factory

	public static TrackerViewModel Create(TrackerSettings settings, DreadConnector? connector = null)
		=> new() {
			Name = settings.Name,
			TargetType = settings.TargetType,
			TargetAddress = settings.IpAddress,
			LastWindowSize = settings.LastWindowSize,
			LastWindowPosition = settings.LastWindowPosition,
			Connector = connector,
		};

	#endregion

	private CompositeDisposable? connectorDisposable;

	public TrackerViewModel()
	{
		this.WhenActivated(disposables => {
			this.WhenAnyValue(m => m.Connector)
				.Subscribe(SubscribeConnector)
				.DisposeWith(disposables);

			// Clean up current subscription when de-activating
			Disposable.Create(() => {
				this.connectorDisposable?.Dispose();
				this.connectorDisposable = null;
			}).DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial DreadConnector? Connector { get; set; }

	#region Settings

	[Reactive]
	public partial string? Name { get; set; }

	[Reactive(nameof(IsRemoteTarget), nameof(Description))]
	public partial TrackerTargetType TargetType { get; set; }

	[Reactive(nameof(Description))]
	public partial string? TargetAddress { get; set; }

	public Size  LastWindowSize     { get; set; }
	public Point LastWindowPosition { get; set; }

	#endregion

	public bool IsRemoteTarget => TargetType == TrackerTargetType.Remote;

	public string Description
		=> TargetType == TrackerTargetType.LocalEmulator
			? "Local Emulator"
			: $"Console: {TargetAddress}";

	[ObservableAsProperty(ReadOnly = false, InitialValue = "\"Not Connected\"")]
	public partial string? State { get; }

	private void SubscribeConnector(DreadConnector? connector)
	{
		CompositeDisposable? previousDisposable;

		if (connector is null)
		{
			previousDisposable = Interlocked.Exchange(ref this.connectorDisposable, null);
			previousDisposable?.Dispose();
			return;
		}

		var newDisposable = new CompositeDisposable();

		previousDisposable = Interlocked.Exchange(ref this.connectorDisposable, newDisposable);
		previousDisposable?.Dispose();

		// Bind ObservableAsPropertyHelper to the "State" property until this connector is disposed
		connector.WhenAnyValue(
				c => c.IsConnected,
				c => c.CurrentGameState,
				c => c.CurrentScenarioName,
				GetStateText,
				isDistinct: true)
			.ToProperty(this, m => m.State, out this._stateHelper)
			.DisposeWith(newDisposable);
	}

	private static string GetStateText(bool isConnected, GameState gameState, string scenarioName)
	{
		if (!isConnected)
			return "Not Connected";

		return gameState switch {
			GameState.InGame      => $"In Game: {scenarioName}",
			GameState.Loading     => "Loading...",
			GameState.TitleScreen => "Title Screen",
			_                     => "Not Connected",
		};
	}
}