using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Arcanox.AppCore.Reactive;
using Arcanox.AppCore.Reactive.Extensions;
using DreadRemoteConnector;
using DreadRemoteConnector.Inventory;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using StandaloneDreadTracker.App.Configuration;
using StandaloneDreadTracker.App.State;
using StandaloneDreadTracker.App.Tracker;
using StandaloneDreadTracker.App.Utility;

namespace StandaloneDreadTracker.App.ViewModels;

public partial class TrackerViewModel : BaseViewModel
{
	#region Static Factory

	public static TrackerViewModel Create(IServiceProvider serviceProvider, TrackerSettings settings, DreadConnector? connector = null)
		=> new(serviceProvider) {
			Id = settings.Id,
			Name = settings.Name,
			TargetType = settings.TargetType,
			TargetAddress = settings.IpAddress,
			ShowItemGroups = settings.ShowItemGroups,
			ItemIconSize = settings.ItemIconSize,
			BossIconSize = settings.BossIconSize,
			PopOutBossSection = settings.PopOutBossSection,
			UseCustomBackground = settings.UseCustomBackground,
			CustomBackgroundColor = settings.CustomBackgroundColor,
			BossesOrientation = settings.BossesOrientation,
			LastMainWindowSize = settings.LastMainWindowSize,
			LastMainWindowPosition = settings.LastMainWindowPosition,
			LastBossWindowPosition = settings.LastBossWindowPosition,
			Connector = connector,
		};

	#endregion

	private static readonly TimeSpan SaveStateThrottleTime = TimeSpan.FromSeconds(1.0);

	private readonly IAppStateManager?                  appStateManager;
	private readonly IOptionsMonitor<ApplicationState>? appStateMonitor;

	private readonly SerialSubscription<DreadConnector> connectorSubscription;
	private readonly SerialSubscription<HintsContainer> hintsSubscription;

	public TrackerViewModel()
		: this(null) { }

	[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "The referenced types/properties are strongly referenced elsewhere")]
	public TrackerViewModel(IServiceProvider? serviceProvider)
	{
		this.appStateManager = serviceProvider?.GetRequiredService<IAppStateManager>();
		this.appStateMonitor = serviceProvider?.GetRequiredService<IOptionsMonitor<ApplicationState>>();

		this.connectorSubscription = new SerialSubscription<DreadConnector>(SubscribeConnector);
		this.hintsSubscription = new SerialSubscription<HintsContainer>(SubscribeHints);

		this.WhenAnyValue(m => m.UseCustomBackground)
			.DistinctUntilChanged()
			.Subscribe(useCustom => {
				if (useCustom && CustomBackgroundColor is null)
					CustomBackgroundColor = TrackerSettings.DefaultCustomBackgroundColor;
			});

		this.WhenActivated(disposables => {
			this.WhenAnyValue(m => m.Connector)
				.SubscribeWith(this.connectorSubscription)
				.DisposeWith(disposables);

			this.WhenAnyValue(m => m.ItemLocationHints, m => m.BossDnaHints, HintsContainer.Create)
				.SubscribeWith(this.hintsSubscription)
				.DisposeWith(disposables);

			this.appStateMonitor?.WhenChanged()
				.CombineLatest(this.WhenAnyValue(m => m.Id))
				.Select(pair => {
					var (state, id) = pair;

					return id != null && state.TrackerSessions.ContainsKey(id);
				})
				.ToProperty(this, m => m.HasSavedSession, out this._hasSavedSessionHelper)
				.DisposeWith(disposables);
		});
	}

	[Reactive]
	public partial string? Id { get; set; }

	[Reactive]
	public partial bool IsOpen { get; set; }

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

	[Reactive]
	public partial ItemLocationHints ItemLocationHints { get; set; } = new();

	[Reactive]
	public partial BossDnaHints BossDnaHints { get; set; } = new();

	[ObservableAsProperty(ReadOnly = false)]
	public partial bool HasSavedSession { get; }

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
	public partial bool UseCustomBackground { get; set; }

	[Reactive]
	public partial string? CustomBackgroundColor { get; set; }

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

	public void ResetState()
	{
		ItemLocationHints = new ItemLocationHints();
		BossDnaHints = new BossDnaHints();
	}

	public void LoadStateFrom(TrackerSession session)
	{
		session.ItemLocationHints.CopyTo(ItemLocationHints);
		session.BossDnaHints.CopyTo(BossDnaHints);
	}

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
		other.UseCustomBackground = UseCustomBackground;
		other.CustomBackgroundColor = CustomBackgroundColor;
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
		settings.UseCustomBackground = UseCustomBackground;
		settings.CustomBackgroundColor = CustomBackgroundColor;
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

	[ReactiveCommand]
	private async Task PersistStateAsync(CancellationToken cancellationToken)
	{
		if (Id is null || this.appStateManager is null)
			return;

		await this.appStateManager.ModifyAsync(state => {
			var session = state.GetOrCreateSession(Id);

			ItemLocationHints.CopyTo(session.ItemLocationHints);
			BossDnaHints.CopyTo(session.BossDnaHints);
		});
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

	private void SubscribeHints(HintsContainer hints, CompositeDisposable disposables)
	{
		var itemChanged = hints.ItemLocationHints.WhenAnyPropertyChanged().StartWith((ItemLocationHints) null!);
		var bossChanged = hints.BossDnaHints.WhenAnyPropertyChanged().StartWith((BossDnaHints) null!);

		itemChanged
			.CombineLatest(bossChanged)
			// We need both inner observables to have a starting value in order for CombineLatest to work,
			// but we don't care about the first set of starting values, so we have to skip it. It seems
			// ridiculous that there isn't a built-in way to accomplish this in RxUI.
			.Skip(1)
			.Select(_ => Unit.Default)
			.Throttle(SaveStateThrottleTime)
			.InvokeCommand(PersistStateCommand)
			.DisposeWith(disposables);
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

	private sealed record HintsContainer(ItemLocationHints ItemLocationHints, BossDnaHints BossDnaHints)
	{
		public static HintsContainer Create(ItemLocationHints itemLocationHints, BossDnaHints bossDnaHints)
			=> new(itemLocationHints, bossDnaHints);
	}
}