using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace DreadRemoteConnector.Lua;

public static partial class LuaHelper
{
	public static string ExpandTemplate(string template, IReadOnlyDictionary<string, object> replacements)
	{
		var builder = new StringBuilder();
		var previousEnd = Index.Start;

		foreach (var match in TemplateReplacementRegex.EnumerateMatches(template))
		{
			var thisStart = Index.FromStart(match.Index);
			var thisEnd = Index.FromStart(match.Index + match.Length);

			// Append everything from the end of the last match to the start of this one
			var beforeThisMatch = template[previousEnd..thisStart];

			builder.Append(beforeThisMatch);

			// Append replacement text for this match
			var thisMatch = template[thisStart..thisEnd];
			var thisMatchKey = thisMatch[3..^3]; // Strip off "T__" and "__T"

			if (!replacements.TryGetValue(thisMatchKey, out var replacementValue))
				throw new FormatException($"Replacement value for template item \"{thisMatchKey}\" was not provided");

			var replacementText = FormatLuaValue(replacementValue);

			builder.Append(replacementText);

			// Advance end marker
			previousEnd = thisEnd;
		}

		// Append everything after the end of the final match
		var remaining = template[previousEnd..Index.End];

		builder.Append(remaining);

		return builder.ToString();
	}

	public static string FormatLuaValue(object? value)
		=> value switch {
			null              => "nil",
			ILuaFormattable f => f.ToLuaExpression(),
			string s          => $"\"{EscapeDoubleQuotes(s)}\"",
			IDictionary d     => FormatLuaTable(d),
			IEnumerable e     => FormatLuaArray(e),
			_                 => value.ToString() ?? "nil",
		};

	public static string FormatLuaArray(IEnumerable items)
		=> "{" + string.Join(",", items.Cast<object>().Select(FormatLuaValue)) + "}";

	public static string FormatLuaTable(IDictionary entries)
	{
		var builder = new StringBuilder();
		var isFirst = true;

		builder.Append('{');

		foreach (var key in entries.Keys)
		{
			if (isFirst)
				isFirst = false;
			else
				builder.Append(',');

			// Write key
			if (key is string s && LuaIdentifierRegex.IsMatch(s))
			{
				// Key is a valid Lua identifier. We can use it as a key directly.
				builder.Append(s);
			}
			else
			{
				// Need to enclose the key in brackets in order to be syntactically valid.
				builder.Append('[');
				builder.Append(FormatLuaValue(key));
				builder.Append(']');
			}

			// Write separator and value
			builder.Append('=');
			builder.Append(FormatLuaValue(entries[key]));
		}

		builder.Append('}');

		return builder.ToString();
	}

	private static string EscapeDoubleQuotes(string str)
		=> str.Replace("\"", "\\\"");

	#region Regular Expressions

	private static readonly Regex TemplateReplacementRegex = GetTemplateReplacementRegex();
	private static readonly Regex LuaIdentifierRegex       = GetLuaIdentifierRegex();

	[GeneratedRegex(@"T__(\w+)__T")]
	private static partial Regex GetTemplateReplacementRegex();

	[GeneratedRegex(@"^[a-z]\w*$", RegexOptions.IgnoreCase)]
	private static partial Regex GetLuaIdentifierRegex();

	#endregion
}