using DreadRemoteConnector.Inventory;

namespace DreadRemoteConnector.Events;

public sealed class InventoryUpdatedEventArgs(DreadInventory currentInventory) : EventArgs
{
	public DreadInventory CurrentInventory { get; } = currentInventory;
}