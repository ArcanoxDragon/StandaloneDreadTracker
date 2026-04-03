using System.Net;
using Microsoft.Extensions.Logging;

namespace DreadRemoteConnector;

internal partial class DreadSocket
{
	private LogImpl Log { get; } = new();

	private sealed partial class LogImpl
	{
		public ILogger? loggerInstance;

		[LoggerMessage(LogLevel.Debug, "Attempting connection to Dread game at {Address}:{Port}...")]
		public partial void Connecting(IPAddress address, int port);

		[LoggerMessage(LogLevel.Error, "Connection to {Address}:{Port} failed")]
		public partial void ConnectionFailed(Exception ex, IPAddress address, int port);
	}
}