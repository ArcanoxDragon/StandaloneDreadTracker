using System.Net;
using DreadRemoteConnector.Packets;
using DreadRemoteConnector.Packets.Receiving;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DreadRemoteConnector;

public partial class DreadSocket
{
	private LogImpl Log { get; } = new();

	private sealed partial class LogImpl
	{
		public ILogger loggerInstance = NullLogger.Instance;

		[LoggerMessage(LogLevel.Debug, "Attempting connection to Dread game at {Address}:{Port}...")]
		public partial void Connecting(IPAddress address, int port);

		[LoggerMessage(LogLevel.Error, "Connection to {Address}:{Port} failed")]
		public partial void ConnectionFailed(Exception ex, IPAddress address, int port);

		[LoggerMessage(LogLevel.Debug, "Connection succeeded. Sending handshake with interests: {Interests}")]
		public partial void ConnectionSucceeded(ConnectionInterests interests);

		[LoggerMessage(LogLevel.Error, "An error occurred while disconnecting")]
		public partial void ErrorDuringDisconnect(Exception ex);

		[LoggerMessage(LogLevel.Debug, "Requesting game details")]
		public partial void RequestingGameDetails();

		public void GotGameDetails(GameDetails gameDetails)
			=> GotGameDetails(gameDetails.ApiVersion, gameDetails.GameVersion, gameDetails.BufferSize, gameDetails.WorldGuid);

		[LoggerMessage(LogLevel.Debug,
					   "Got game details. API version: {ApiVersion}. Game version: {GameVersion}. " +
					   "Buffer size: {BufferSize}. World GUID: {WorldGuid:B}.")]
		private partial void GotGameDetails(int apiVersion, string gameVersion, int bufferSize, Guid worldGuid);

		public void MalformedPacket(MalformedPacketReceivePacket info)
			=> MalformedPacket(info.MalformedPacketType, info.ExpectedBytes, info.ReceivedBytes);

		[LoggerMessage(LogLevel.Warning, "The game reported a malformed \"{PacketType}\" packet. It expected to receive {ExpectedBytes}, but got {ReceivedBytes}.")]
		private partial void MalformedPacket(PacketType packetType, int expectedBytes, int receivedBytes);
	}
}