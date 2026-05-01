using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
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
			BossesOrientation = settings.BossesOrientation,
			LastMainWindowSize = settings.LastMainWindowSize,
			LastMainWindowPosition = settings.LastMainWindowPosition,
			LastBossWindowPosition = settings.LastBossWindowPosition,
			Connector = connector,
		};

	#endregion

	private readonly SerialSubscription<DreadConnector>    connectorSubscription;
	private readonly SerialSubscription<ItemLocationHints> itemHintsSubscription;
	private readonly SerialSubscription<BossDnaHints>      dnaHintsSubscription;

	[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "The referenced types/properties are strongly referenced elsewhere")]
	public TrackerViewModel()
	{
		this.connectorSubscription = new SerialSubscription<DreadConnector>(SubscribeConnector);
		this.itemHintsSubscription = new SerialSubscription<ItemLocationHints>(SubscribeItemHints);
		this.dnaHintsSubscription = new SerialSubscription<BossDnaHints>(SubscribeDnaHints);

		this.WhenActivated(disposables => {
			this.WhenAnyValue(m => m.Connector)
				.SubscribeWith(this.connectorSubscription)
				.DisposeWith(disposables);

			this.WhenAnyValue(m => m.ItemLocationHints)
				.SubscribeWith(this.itemHintsSubscription)
				.DisposeWith(disposables);

			this.WhenAnyValue(m => m.BossDnaHints)
				.SubscribeWith(this.dnaHintsSubscription)
				.DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial string? Id { get; set; }

	[Reactive(nameof(CurrentInventory), nameof(DefeatedBosses))]
	public partial DreadConnector? Connector { get; set; }

	/// <summary>
	/// For design/testing.
	/// </summary>
	[Reactive(nameof(CurrentInventory))]
	internal partial DreadInventory? MockInventory { get; set; }

	public DreadInventory? CurrentInventory => MockInventory ?? Connector?.CurrentInventory;

	/// <summary>
	/// For design/testing.
	/// </summary>
	[Reactive(nameof(DefeatedBosses))]
	internal partial DreadBosses? MockDefeatedBosses { get; set; }

	public DreadBosses? DefeatedBosses => MockDefeatedBosses ?? Connector?.DefeatedBosses;

	public ItemLocationHints ItemLocationHints { get; set; } = new();
	public BossDnaHints      BossDnaHints      { get; set; } = new();

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

	[Reactive]
	public partial TrackerOrientation BossesOrientation { get; set; } = TrackerOrientation.Horizontal;

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
		other.BossesOrientation = BossesOrientation;
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
		settings.PopOutBossSection = PopOutBossSection;
		settings.BossesOrientation = BossesOrientation;
		settings.LastMainWindowSize = LastMainWindowSize;
		settings.LastMainWindowPosition = LastMainWindowPosition;
		settings.LastBossWindowPosition = LastBossWindowPosition;
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

	[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "The referenced types/properties are strongly referenced elsewhere")]
	private void SubscribeConnector(DreadConnector connector, CompositeDisposable disposables)
	{
		// Bind ObservableAsPropertyHelper to the "State" property until this connector is disposed
		connector.WhenAnyValue(
				c => c.IsConnecting,
				c => c.IsConnected,
				c => c.CurrentGameState,
				c => c.CurrentScenarioName,
				GetStateText,
				isDistinct: true)
			.ToProperty(this, m => m.State, out this._stateHelper)
			.DisposeWith(disposables);
	}

	private void SubscribeItemHints(ItemLocationHints itemHints, CompositeDisposable disposables)
	{
		// TODO: Persist state
	}

	private void SubscribeDnaHints(BossDnaHints dnaHints, CompositeDisposable disposables)
	{
		// TODO: Persist state
	}

	private static string GetStateText(bool isConnecting, bool isConnected, GameState gameState, string scenarioName)
	{
		if (!isConnected)
			return isConnecting ? "Connecting..." : "Not Connected";

		return gameState switch {
			GameState.InGame      => $"In Game: {scenarioName}",
			GameState.Loading     => "Loading...",
			GameState.TitleScreen => "Title Screen",
			_                     => "Not Connected",
		};
	}
}