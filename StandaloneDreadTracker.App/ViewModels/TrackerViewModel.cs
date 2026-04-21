using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using DreadRemoteConnector;
using DreadRemoteConnector.Inventory;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.Extensions;
using StandaloneDreadTracker.App.Tracker;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.ViewModels;

public partial class TrackerViewModel : ViewModelBase
{
	#region Static Factory

	public static TrackerViewModel Create(TrackerSettings settings, DreadConnector? connector = null)
		=> new() {
			Id = settings.Id,
			Name = settings.Name,
			TargetType = settings.TargetType,
			TargetAddress = settings.IpAddress,
			ShowItemGroups = settings.ShowItemGroups,
			ItemIconSize = settings.ItemIconSize,
			BossIconSize = settings.BossIconSize,
			PopOutBossSection = settings.PopOutBossSection,
			LastMainWindowSize = settings.LastMainWindowSize,
			LastMainWindowPosition = settings.LastMainWindowPosition,
			LastBossWindowPosition = settings.LastBossWindowPosition,
			Connector = connector,
		};

	#endregion

	private readonly SerialSubscription<DreadConnector> connectorSubscription;
	private readonly SerialSubscription<BossDnaHints>   dnaHintsSubscription;

	public TrackerViewModel()
	{
		this.connectorSubscription = new SerialSubscription<DreadConnector>(SubscribeConnector);
		this.dnaHintsSubscription = new SerialSubscription<BossDnaHints>(SubscribeDnaHints);

		this.WhenActivated(disposables => {
			this.WhenAnyValue(m => m.Connector)
				.SubscribeWith(this.connectorSubscription)
				.DisposeWith(disposables);

			this.WhenAnyValue(m => m.BossDnaHints)
				.SubscribeWith(this.dnaHintsSubscription)
				.DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial string? Id { get; set; }

	[Reactive(nameof(CurrentInventory))]
	public partial DreadConnector? Connector { get; set; }

	/// <summary>
	/// For design/testing.
	/// </summary>
	[Reactive(nameof(CurrentInventory))]
	internal partial DreadInventory? MockInventory { get; set; }

	public DreadInventory? CurrentInventory => MockInventory ?? Connector?.CurrentInventory;

	public DreadBosses  DefeatedBosses { get; set; } = new();
	public BossDnaHints BossDnaHints   { get; set; } = new();

	#region Settings

	[Reactive]
	public partial string? Name { get; set; }

	[Reactive(nameof(IsRemoteTarget), nameof(Description))]
	public partial TrackerTargetType TargetType { get; set; }

	[Reactive(nameof(Description))]
	public partial string? TargetAddress { get; set; }

	[Reactive]
	public partial bool ShowItemGroups { get; set; } = true;

	[Reactive]
	public partial int ItemIconSize { get; set; } = TrackerSettings.DefaultItemIconSize;

	[Reactive]
	public partial int BossIconSize { get; set; } = TrackerSettings.DefaultBossIconSize;

	[Reactive]
	public partial bool PopOutBossSection { get; set; }

	public Size  LastMainWindowSize     { get; set; }
	public Point LastMainWindowPosition { get; set; }
	public Point LastBossWindowPosition { get; set; }

	#endregion

	public bool IsRemoteTarget => TargetType == TrackerTargetType.Remote;

	public string Description
		=> TargetType == TrackerTargetType.LocalEmulator
			? "Local Emulator"
			: $"Console: {TargetAddress}";

	[ObservableAsProperty(ReadOnly = false, InitialValue = "\"Not Connected\"")]
	public partial string? State { get; }

	/// <summary>
	/// Copies all tracker settings from this view model to the <paramref name="other"/> view model.
	/// </summary>
	public void CopySettingsTo(TrackerViewModel other)
	{
		other.Name = Name;
		other.TargetType = TargetType;
		other.TargetAddress = TargetAddress;
		other.ShowItemGroups = ShowItemGroups;
		other.ItemIconSize = ItemIconSize;
		other.BossIconSize = BossIconSize;
		other.PopOutBossSection = PopOutBossSection;
		other.LastMainWindowSize = LastMainWindowSize;
		other.LastMainWindowPosition = LastMainWindowPosition;
		other.LastBossWindowPosition = LastBossWindowPosition;
	}

	/// <summary>
	/// Copies all tracker settings from this view model to the <paramref name="settings"/> object.
	/// </summary>
	public void CopySettingsTo(TrackerSettings settings)
	{
		settings.Name = Name;
		settings.TargetType = TargetType;
		settings.IpAddress = TargetAddress;
		settings.ShowItemGroups = ShowItemGroups;
		settings.ItemIconSize = ItemIconSize;
		settings.BossIconSize = BossIconSize;
		settings.LastMainWindowSize = LastMainWindowSize;
		settings.LastMainWindowPosition = LastMainWindowPosition;
	}

	#region Commands

	[ReactiveCommand]
	private void ToggleBossDnaHint(string? bossName)
	{
		if (bossName is null)
			return;

		BossDnaHints.ToggleDnaHint(bossName);
	}

	#endregion

	private void SubscribeConnector(DreadConnector connector, CompositeDisposable disposables)
	{
		// Bind ObservableAsPropertyHelper to the "State" property until this connector is disposed
		connector.WhenAnyValue(
				c => c.IsConnected,
				c => c.CurrentGameState,
				c => c.CurrentScenarioName,
				GetStateText,
				isDistinct: true)
			.ToProperty(this, m => m.State, out this._stateHelper)
			.DisposeWith(disposables);

		// Auto-toggle the defeated state of bosses with DNA locations assigned when DNA items change
		Observable.FromEventPattern<int>(
				h => connector.CurrentInventory.DnaStateChanged += h,
				h => connector.CurrentInventory.DnaStateChanged -= h)
			.Subscribe(@event => SetBossDefeatedForDna(@event.EventArgs))
			.DisposeWith(disposables);
	}

	private void SubscribeDnaHints(BossDnaHints dnaHints, CompositeDisposable disposables)
	{
		// Sync the defeated state of bosses when their DNA hint is changed
		Observable.FromEventPattern<BossValueChangedEventArgs>(
				h => dnaHints.ValueChanged += h,
				h => dnaHints.ValueChanged -= h)
			.Subscribe(@event => {
				var dnaNumber = BossDnaHints[@event.EventArgs.BossIndex];

				if (dnaNumber is < 1 or >= DreadInventory.Items.MaxMetroidDnaCount)
					return;

				SetBossDefeatedForDna(dnaNumber);
			})
			.DisposeWith(disposables);
	}

	private void SetBossDefeatedForDna(int dnaNumber)
	{
		if (CurrentInventory is not { } inventory)
			return;

		var dnaCollected = inventory.AllMetroidDna[dnaNumber - 1];

		for (var i = 0; i < DreadBosses.BossOrder.Count; i++)
		{
			if (BossDnaHints[i] != dnaNumber)
				continue;

			DefeatedBosses[i] = dnaCollected;
		}
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