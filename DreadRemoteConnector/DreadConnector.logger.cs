using DreadRemoteConnector.Packets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DreadRemoteConnector;

public partial class DreadConnector
{
	private LogImpl Log { get; } = new();

	private sealed partial class LogImpl
	{
		public ILogger loggerInstance = NullLogger.Instance;

		[LoggerMessage(LogLevel.Debug, "Game Log: {Message}")]
		public partial void LogMessageReceived(string message);

		[LoggerMessage(LogLevel.Warning, "The connection to the game was lost")]
		public partial void LostConnection();

		[LoggerMessage(LogLevel.Information, "The connection to the game was restored")]
		public partial void ConnectionRestored();

		[LoggerMessage(LogLevel.Error, "Could not send keep-alive packet")]
		public partial void ErrorSendingKeepAlive(Exception ex);

		[LoggerMessage(LogLevel.Error, "An error occurred while attempting to receive a packet")]
		public partial void ErrorDuringReceive(Exception ex);

		[LoggerMessage(LogLevel.Warning, "An packet was received with unknown type \"{PacketType}\"")]
		public partial void UnknownPacketTypeReceived(PacketType packetType);
	}
}