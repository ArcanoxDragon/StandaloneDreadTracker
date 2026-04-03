namespace DreadRemoteConnector;

public sealed class DreadLuaException(string message, string? responseText = null)
	: ApplicationException(message)
{
	public string? ResponseText { get; } = responseText;
}