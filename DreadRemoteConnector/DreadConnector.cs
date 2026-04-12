using System.Net;
using DreadRemoteConnector.Events;
using DreadRemoteConnector.Inventory;
using DreadRemoteConnector.Lua;
using DreadRemoteConnector.Observability;
using DreadRemoteConnector.Packets.Receiving;
using DreadRemoteConnector.Packets.Sending;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DreadRemoteConnector;

[PublicAPI]
public sealed partial class DreadConnector : NotifyPropertyChangedObject, IDisposable, IAsyncDisposable
{
	private LoopContext? currentLoop;
	private bool         updateInterestedItems;

	public DreadConnector(IPAddress ipAddress, int port = DreadSocket.DefaultPort)
	{
		Socket = new DreadSocket(ipAddress, port);
		Socket.ConnectionLost += OnSocketConnectionLost;
	}

	public DreadConnector(string ipAddress, int port = DreadSocket.DefaultPort)
		: this(IPAddress.Parse(ipAddress), port) { }

	public event EventHandler<InventoryUpdatedEventArgs>? InventoryUpdated;

	public DreadSocket Socket { get; }

	public ConnectionInterests ConnectionInterests
	{
		get => Socket.ConnectionInterests;
		set => Socket.ConnectionInterests = value;
	}

	public string[] InventoryItemsOfInterest
	{
		get;
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			field = value;
			this.updateInterestedItems = true;
			this.currentLoop?.CancelAuxToken(); // Force the keep-alive loop to immediately update the interested items
		}
	} = DreadInventory.DefaultItemsOfInterest.ToArray();

	public DreadInventory CurrentInventory { get; } = new();

	public GameState CurrentGameState
	{
		get;
		set => SetField(ref field, value);
	}

	public string CurrentScenarioName
	{
		get;
		set => SetField(ref field, value);
	} = "Unknown";

	public TimeSpan KeepAliveInterval
	{
		get;
		set
		{
			ArgumentOutOfRangeException.ThrowIfLessThan((long) value.TotalMilliseconds, -1, nameof(value));
			ArgumentOutOfRangeException.ThrowIfEqual(value.Ticks, 0, nameof(value));
			field = value;
		}
	} = TimeSpan.FromSeconds(5);

	public TimeSpan SleepTimeBeforeReconnect
	{
		get;
		set
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(value.Ticks, 0, nameof(value));
			field = value;
		}
	} = TimeSpan.FromSeconds(15);

	public bool IsConnected
	{
		get;
		private set => SetField(ref field, value);
	}

	public ILogger? Logger
	{
		get => Log.loggerInstance;
		set
		{
			Log.loggerInstance = value ?? NullLogger.Instance;
			Socket.Logger = value;
		}
	}

	public async ValueTask StartAsync(CancellationToken cancellationToken = default)
	{
		if (this.currentLoop != null)
			throw new InvalidOperationException("Connector is already running and cannot be started again");

		var newLoop = new LoopContext();

		if (Interlocked.CompareExchange(ref this.currentLoop, newLoop, null) != null)
			// Someone snuck in between our early check and now
			throw new InvalidOperationException("Connector is already running and cannot be started again");

		// Create a combined cancellation token for connecting. The connection attempt will be canceled
		// if the passed-in token is canceled OR if "StopAsync" is called while we are still connecting.
		using var combinedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(newLoop.MainCancellationToken, cancellationToken);
		var combinedToken = combinedCancelSource.Token;

		try
		{
			await Socket.ConnectAsync(combinedToken).ConfigureAwait(false);
			OnSocketConnected();
		}
		catch
		{
			// Ignored - loops will start anyways and continually try to re-connect
		}

		// We should update the "interested items" list immediately upon connecting
		this.updateInterestedItems = true;

		// Start the loops
		var keepAliveTask = RunKeepAliveLoopAsync(newLoop);
		var receiveTask = RunReceiveLoopAsync(newLoop);

		newLoop.LoopTask = Task.WhenAll(keepAliveTask, receiveTask);
	}

	public async ValueTask StopAsync()
	{
		var currentLoop = Interlocked.Exchange(ref this.currentLoop, null);

		try
		{
			if (currentLoop != null)
				await currentLoop.ShutdownLoopAsync().ConfigureAwait(false);

			await Socket.DisconnectAsync().ConfigureAwait(false);
		}
		finally
		{
			IsConnected = false;
			currentLoop?.Dispose();
		}
	}

	#region Keep-Alive Loop

	private async Task RunKeepAliveLoopAsync(LoopContext loop)
	{
		while (!loop.MainCancellationToken.IsCancellationRequested)
		{
			// We use a special token for the Task.Delay that will be canceled when the "aux token" is canceled.
			// This allows us to, for example, signal that the "interested items" list should be immediately updated
			// without having to wait for the Task.Delay to expire.
			using var combinedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(loop.MainCancellationToken, loop.AuxCancellationToken);
			var combinedToken = combinedCancelSource.Token;

			try
			{
				await TryUpdateInterestedItemsAsync(loop.MainCancellationToken).ConfigureAwait(false); // No-op if the update flag is not set
				await Task.Delay(KeepAliveInterval, combinedToken).ConfigureAwait(false);
				await TrySendKeepAliveAsync(loop.MainCancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken == combinedToken)
			{
				// Ignore and loop again
			}
		}
	}

	private async ValueTask TryUpdateInterestedItemsAsync(CancellationToken cancellationToken)
	{
		if (!Interlocked.Exchange(ref this.updateInterestedItems, false))
			return;

		try
		{
			if (!IsConnected)
			{
				// Need to try again later
				this.updateInterestedItems = true;
				return;
			}

			var luaCode = LuaSnippets.GetSnippet(LuaSnippets.SnippetNames.UpdateInterestedItems, new Dictionary<string, object> {
				{ "interestedItems", InventoryItemsOfInterest },
			});

			await Socket.ExecuteLuaAsync(luaCode, waitForResponse: false, cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
		{
			// If the send attempt was canceled because the passed-in token was canceled (and not because of a timeout),
			// re-throw the exception so that the loop itself is also canceled.
			throw;
		}
		catch
		{
			// Ignore other exceptions, but try again
			this.updateInterestedItems = true;
		}
	}

	private async Task TrySendKeepAliveAsync(CancellationToken cancellationToken)
	{
		try
		{
			if (!IsConnected)
				return;

			await Socket.SendPacketAsync(new KeepAliveSendPacket(), cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
		{
			// If the receive attempt was canceled because the passed-in token was canceled (and not because of a timeout),
			// re-throw the exception so that the loop itself is also canceled.
			throw;
		}
		catch (Exception ex)
		{
			Log.ErrorSendingKeepAlive(ex);
		}
	}

	#endregion

	#region Receive Loop

	private async Task RunReceiveLoopAsync(LoopContext loop)
	{
		while (!loop.MainCancellationToken.IsCancellationRequested)
		{
			if (!IsConnected)
			{
				await Task.Delay(SleepTimeBeforeReconnect, loop.MainCancellationToken).ConfigureAwait(false);

				var reconnected = await AttemptReconnectAsync(loop.MainCancellationToken).ConfigureAwait(false);

				if (!reconnected)
					continue;
			}

			await TryWaitForPacketAsync(loop.MainCancellationToken).ConfigureAwait(false);
		}
	}

	private async Task TryWaitForPacketAsync(CancellationToken cancellationToken)
	{
		try
		{
			var packet = await Socket.WaitForAnyPacketAsync(cancellationToken).ConfigureAwait(false);

			HandleReceivedPacket(packet);
		}
		catch (UnknownPacketTypeException ex)
		{
			Log.UnknownPacketTypeReceived(ex.PacketType);
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
		{
			// If the receive attempt was canceled because the passed-in token was canceled (and not because of a timeout),
			// re-throw the exception so that the loop itself is also canceled.
			throw;
		}
		catch (Exception ex)
		{
			Log.ErrorDuringReceive(ex);
			IsConnected = false;
		}
	}

	private void HandleReceivedPacket(IReceivePacket packet)
	{
		switch (packet)
		{
			case LogMessageReceivePacket logPacket:
				Log.LogMessageReceived(logPacket.Message);
				break;
			case NewInventoryReceivePacket newInventoryPacket:
				HandleNewInventoryPacket(newInventoryPacket);
				break;
			case GameStateReceivePacket gameStatePacket:
				HandleGameStatePacket(gameStatePacket);
				break;
		}
	}

	private void HandleNewInventoryPacket(NewInventoryReceivePacket packet)
	{
		var itemsOfInterest = InventoryItemsOfInterest;

		if (packet.ItemQuantities.Length != itemsOfInterest.Length)
		{
			Log.InvalidInventoryUpdateReceived(itemsOfInterest.Length, packet.ItemQuantities.Length);
			return;
		}

		for (var i = 0; i < itemsOfInterest.Length; i++)
		{
			var itemName = itemsOfInterest[i];
			var quantity = packet.ItemQuantities[i];

			CurrentInventory.UpdateItemQuantity(itemName, quantity);
		}

		InventoryUpdated?.Invoke(this, new InventoryUpdatedEventArgs(CurrentInventory));
	}

	private void HandleGameStatePacket(GameStateReceivePacket packet)
	{
		CurrentGameState = packet.GameState;
		CurrentScenarioName = packet.ScenarioName;
	}

	private async Task<bool> AttemptReconnectAsync(CancellationToken cancellationToken)
	{
		try
		{
			await Socket.ConnectAsync(cancellationToken).ConfigureAwait(false);
			OnSocketConnected();
			Log.ConnectionRestored();
			return true;
		}
		catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
		{
			// If the connection attempt was canceled because the passed-in token was canceled (and not because of a timeout),
			// re-throw the exception so that the loop itself is also canceled.
			throw;
		}
		catch
		{
			return false;
		}
	}

	#endregion

	private void OnSocketConnected()
	{
		IsConnected = true;
		CurrentInventory.RequiredDnaCount = Socket.GameDetails.RequiredDnaCount;
	}

	private void OnSocketConnectionLost(object? sender, EventArgs e)
	{
		IsConnected = false;
		Log.LostConnection();
	}

	#region IDisposable

	private bool disposed;

	public void Dispose()
	{
		if (Interlocked.Exchange(ref this.disposed, true))
			return;

		var currentLoop = Interlocked.Exchange(ref this.currentLoop, null);

		IsConnected = false;
		currentLoop?.Dispose();
		Socket.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		if (Interlocked.Exchange(ref this.disposed, true))
			return;

		IsConnected = false;

		try
		{
			await StopAsync().ConfigureAwait(false);
		}
		catch
		{
			// Ignore any exceptions while disposing
		}

		Socket.Dispose();
	}

	#endregion

	private sealed class LoopContext : IDisposable
	{
		private readonly CancellationTokenSource mainCancelSource = new();

		private CancellationTokenSource auxCancelSource = new();

		public LoopContext()
		{
			MainCancellationToken = this.mainCancelSource.Token;
			AuxCancellationToken = this.auxCancelSource.Token;
		}

		public CancellationToken MainCancellationToken { get; }
		public CancellationToken AuxCancellationToken  { get; private set; }
		public Task?             LoopTask              { get; set; }

		public async ValueTask ShutdownLoopAsync()
		{
			try
			{
				await this.mainCancelSource.CancelAsync().ConfigureAwait(false);

				if (LoopTask != null)
					await LoopTask;
			}
			catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
			{
				// Ignored
			}
		}

		public void CancelAuxToken()
		{
			var newAuxSource = new CancellationTokenSource();
			var prevAuxSource = Interlocked.Exchange(ref this.auxCancelSource, newAuxSource);

			AuxCancellationToken = newAuxSource.Token;
			prevAuxSource.Cancel();
			prevAuxSource.Dispose();
		}

		public void Dispose()
		{
			this.mainCancelSource.Dispose();
			this.auxCancelSource.Dispose();
		}
	}
}