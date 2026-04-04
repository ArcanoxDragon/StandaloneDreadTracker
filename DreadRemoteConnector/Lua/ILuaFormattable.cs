using JetBrains.Annotations;

namespace DreadRemoteConnector.Lua;

[PublicAPI]
public interface ILuaFormattable
{
	string ToLuaExpression();
}
