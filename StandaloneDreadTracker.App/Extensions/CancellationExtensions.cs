namespace StandaloneDreadTracker.App.Extensions;

internal static class CancellationExtensions
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
				await cts.CancelAsync();
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