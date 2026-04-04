using System.Collections.Concurrent;
using System.Text;

namespace DreadRemoteConnector.Lua;

internal static class LuaSnippets
{
	public static class SnippetNames
	{
		public const string GetGameDetails = "get_game_details";
	}

	private static readonly ConcurrentDictionary<string, string> SnippetCache        = [];
	private static readonly Func<string, string>                 LoadSnippetFunction = LoadSnippet;

	public static string GetSnippet(string name, IReadOnlyDictionary<string, object>? replacements = null)
	{
		var snippetText = GetSnippetText(name);

		return replacements == null
			? snippetText
			: LuaHelper.ExpandTemplate(snippetText, replacements);
	}

	private static string GetSnippetText(string name)
		=> SnippetCache.GetOrAdd(name, LoadSnippetFunction);

	private static string LoadSnippet(string name)
	{
		var assembly = typeof(LuaSnippets).Assembly;
		using var stream = assembly.GetManifestResourceStream(typeof(LuaSnippets), $"{name}.lua");

		if (stream is null)
			throw new ApplicationException($"Could not read Lua snippet \"{name}\"");

		using var reader = new StreamReader(stream, Encoding.UTF8);

		return reader.ReadToEnd();
	}
}