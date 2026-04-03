using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace DreadRemoteConnector.Lua;

internal static partial class LuaSnippets
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

		if (replacements is null)
			return snippetText;

		var builder = new StringBuilder();
		var previousEnd = Index.Start;

		foreach (var match in TemplateReplacementRegex.EnumerateMatches(snippetText))
		{
			// Append everything from the end of the last match to the start of this one
			var thisStart = Index.FromStart(match.Index);
			var thisEnd = Index.FromStart(match.Index + match.Length);
			var beforeThisMatch = snippetText[previousEnd..thisStart];

			builder.Append(beforeThisMatch);

			// Append replacement text for this match
			var thisMatch = snippetText[thisStart..thisEnd];
			var thisMatchKey = thisMatch[3..^3]; // Strip off "T__" and "__T"

			if (!replacements.TryGetValue(thisMatchKey, out var replacementValue))
				throw new FormatException($"Template replacement \"{thisMatchKey}\" was not provided");

			var replacementText = FormatLuaValue(replacementValue);

			builder.Append(replacementText);

			// Advance end marker
			previousEnd = thisEnd;
		}

		// Append everything after the end of the final match
		var remaining = snippetText[previousEnd..Index.End];

		builder.Append(remaining);

		return builder.ToString();
	}

	public static string FormatLuaValue(object? value)
		=> value switch {
			null     => "nil",
			string s => $"\"{EscapeString(s)}\"",
			_        => value.ToString() ?? "nil",
		};

	private static string EscapeString(string str)
		=> str.Replace("\"", "\\\"");

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

	#region Regular Expressions

	private static readonly Regex TemplateReplacementRegex = GetTemplateReplacementRegex();

	[GeneratedRegex(@"T__(\w+)__T")]
	private static partial Regex GetTemplateReplacementRegex();

	#endregion
}