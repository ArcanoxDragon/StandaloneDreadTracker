using DreadRemoteConnector.Inventory;

namespace StandaloneDreadTracker.App.Tracker;

public class ItemLocationHints : DreadItemContainer<char>
{
	public ItemLocationHints()
	{
		Fill('?');
	}
}