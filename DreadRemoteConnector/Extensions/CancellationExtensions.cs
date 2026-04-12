namespace DreadRemoteConnector.Extensions;

public static class CancellationExtensions
{
	extension(CancellationTokenSource cts)
	{
		public bool TryCancel()
		{
			try
			{
				cts.Cancel();
				return true;
			}
			catch (ObjectDisposedException)
			{
				// Ignore already-disposed CTS
				return false;
			}
		}

		public async ValueTask<bool> TryCancelAsync()
		{
			try
			{
				await cts.CancelAsync().ConfigureAwait(false);
				return true;
			}
			catch (ObjectDisposedException)
			{
				// Ignore already-disposed CTS
				return false;
			}
		}
	}
}