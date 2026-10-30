using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class TableViewParser
{
	private const RegexOptions Options = RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant;

	private const string Space = "[ ]";

	private const string Spaces = "[ ]*";

	private const string Fail = "?!";

	private static readonly Lazy<Regex> tableViewRegex = new Lazy<Regex>(BuildTableViewRegex);

	private static readonly Lazy<Regex> tableFiltersRegex = new Lazy<Regex>(BuildTableFiltersRegex);

	private static readonly Regex cultureInvariantRegex = new Regex("[ ]*(VERSION\\(\\d+\\))", RegexOptions.Compiled);

	private static Regex BuildTableViewRegex()
	{
		string text = ViewClause("SORTING", GetSortingBody());
		string text2 = ViewClause("ORDER", GetOrderBody());
		string text3 = ViewClause("WHERE", GetWhereBody());
		return new Regex(WholeString(ZeroOrMore(Padded(Alternatives(text, text2, text3)))), RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
	}

	private static Regex BuildTableFiltersRegex()
	{
		return new Regex(WholeString(Padded(GetWhereBody(withinParens: false))), RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
	}

	private static string GetSortingBody()
	{
		string optionalUnquotedTerminatorRegex = "\\)" + NotFollowedBy("[ ]*" + Alternatives("$", CaseInsentitive(Alternatives("ORDER", "SORTING", "WHERE"))) + "[ ]*\\(");
		string groupBody = ReadValue('"', ",)", optionalUnquotedTerminatorRegex);
		return CommaDelimitedRepetitionWithinParens(Group("SortingFieldName", groupBody));
	}

	private static string GetOrderBody()
	{
		return CommaDelimitedRepetitionWithinParens(Group("OrderValue", CaseInsentitive(Alternatives("0", "1", PartialMatch("ASCENDING"), PartialMatch("DESCENDING")))));
	}

	private static string GetWhereBody(bool withinParens = true)
	{
		string text = Group("WhereFieldName", ReadValue('"', "="));
		string text2 = FieldExpressionType(Alternatives(PartialMatch("FILTER"), "1"), ConsumeWithBalancedParens(Alternatives(ReadValueOrEmpty('\'', "'()"), ReadValueOrEmpty('\'', "()"))));
		string text3 = FieldExpressionType(Alternatives(PartialMatch("CONST"), PartialMatch("FIELD"), "0", "2"), ReadValueOrEmpty('"', ")"));
		string body = ConcatIgnoreSpaces(text, "=", Alternatives(text2, text3));
		if (!withinParens)
		{
			return CommaDelimitedRepetition(body);
		}
		return CommaDelimitedRepetitionWithinParens(body);
	}

	public static string FieldExpressionType(string matchType, string matchBody)
	{
		return ConcatIgnoreSpaces(Group("WhereExprType", CaseInsentitive(matchType)), Parenthesized(Group("WhereExpr", matchBody)));
	}

	private static string ViewClause(string name, string body)
	{
		return Atomic(Condition(name, "?!", Group(name, ConcatIgnoreSpaces(CaseInsentitive(name), Parenthesized(body)))));
	}

	private static string PartialMatch(string value)
	{
		return Group(value.Substring(0, 1) + Alternatives((from i in Enumerable.Range(0, value.Length)
			select value.Substring(1, value.Length - 1 - i)).ToArray()));
	}

	private static string ConcatIgnoreSpaces(params string[] items)
	{
		return string.Join("[ ]*", items);
	}

	private static string Condition(string test, string yes, string no = null)
	{
		if (no == null)
		{
			return string.Format(CultureInfo.InvariantCulture, "(?({0}) ({1}))", test, yes);
		}
		return string.Format(CultureInfo.InvariantCulture, "(?({0}) ({1}) | ({2}))", test, yes, no);
	}

	private static string ConsumeWithBalancedParens(string body)
	{
		return OneOrMore(Alternatives(body, Group("Depth", "\\("), PopGroup("Depth", "\\)"))) + Condition("Depth", "?!");
	}

	private static string ZeroOrMore(string body)
	{
		return string.Format(CultureInfo.InvariantCulture, "({0})*", body);
	}

	private static string OneOrMore(string body)
	{
		return string.Format(CultureInfo.InvariantCulture, "({0})+", body);
	}

	private static string WholeString(string body)
	{
		return "^" + body + "$";
	}

	private static string Not(char c)
	{
		return NoneOf(c.ToString(CultureInfo.InvariantCulture));
	}

	private static string NoneOf(string characters)
	{
		return "[^" + characters + "]";
	}

	private static string Alternatives(params string[] alternatives)
	{
		return Group(string.Join("|", alternatives));
	}

	private static string Parenthesized(string body)
	{
		return ConcatIgnoreSpaces("\\(", body, "\\)");
	}

	private static string Padded(string body)
	{
		return "[ ]*" + body + "[ ]*";
	}

	private static string CommaDelimitedRepetitionWithinParens(string body)
	{
		return OneOrMore(Condition("Done", "?!") + body + "[ ]*" + Alternatives(",[ ]*", Group("Done", null))) + NotPrecededBy(",[ ]*") + PopGroup("Done", null);
	}

	private static string CommaDelimitedRepetition(string body)
	{
		return ZeroOrMore(body + "[ ]*" + Optional(",[ ]*")) + NotPrecededBy(",[ ]*");
	}

	private static string Optional(string body)
	{
		return Group(body) + "?";
	}

	private static string Group(string groupName, string groupBody)
	{
		return string.Format(CultureInfo.InvariantCulture, "(?<{0}>{1})", groupName, groupBody);
	}

	private static string Group(string body)
	{
		return string.Format(CultureInfo.InvariantCulture, "({0})", body);
	}

	private static string PopGroup(string groupName, string groupBody)
	{
		return Group("-" + groupName, groupBody);
	}

	private static string Atomic(string body)
	{
		return "(?>" + body + ")";
	}

	private static string NotPrecededBy(string body)
	{
		return "(?<!" + body + ")";
	}

	private static string NotFollowedBy(string body)
	{
		return "(?!" + body + ")";
	}

	private static string CaseInsentitive(string body)
	{
		return "(?i: " + body + ")";
	}

	private static string ReadValueOrEmpty(char quotingChar, string delimiters)
	{
		return Alternatives(ReadValue(quotingChar, delimiters), string.Empty);
	}

	private static string ReadValue(char quotingChar, string delimiters, string optionalUnquotedTerminatorRegex = null)
	{
		string text = NoneOf(quotingChar + delimiters) + ZeroOrMore(NoneOf(delimiters));
		if (optionalUnquotedTerminatorRegex != null)
		{
			optionalUnquotedTerminatorRegex += ZeroOrMore(string.Format(CultureInfo.InvariantCulture, "[^{0}]*", delimiters));
			text = OneOrMore(Alternatives(text, optionalUnquotedTerminatorRegex));
		}
		text += NotPrecededBy("[ ]");
		string text2 = quotingChar + ZeroOrMore(Alternatives(Not(quotingChar), quotingChar.ToString(CultureInfo.InvariantCulture) + quotingChar)) + quotingChar;
		return Alternatives(text, text2);
	}

	internal static void ParseTableView(string tableView, TableViewResolver resolver)
	{
		bool flag = IsCultureInvariant(tableView, out var captureStart, out var captureLength);
		if (flag)
		{
			tableView = tableView.Remove(captureStart, captureLength);
		}
		Match match = tableViewRegex.Value.Match(tableView);
		if (!match.Success)
		{
			throw resolver.GetInvalidExpressionException();
		}
		ProcessSortingFields(match, resolver);
		ProcessSortOrder(match, resolver);
		ProcessWhere(tableView, match, resolver, flag);
	}

	internal static bool IsCultureInvariant(string tableView, out int captureStart, out int captureLength)
	{
		captureStart = -1;
		captureLength = -1;
		if (string.IsNullOrEmpty(tableView))
		{
			return false;
		}
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		Match match = cultureInvariantRegex.Match(tableView);
		if (match.Success)
		{
			captureStart = match.Captures[0].Index;
			captureLength = match.Captures[0].Length;
			mostSpecificInstance.SendTraceTag("00000X1", Category.FilterParsing, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Detected culture invariant table view");
			return true;
		}
		mostSpecificInstance.SendTraceTag("00000X2", Category.FilterParsing, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Table view is not culture invariant");
		return false;
	}

	internal static void ParseTableFilters(string input, TableFilterResolver resolver)
	{
		Match match = tableFiltersRegex.Value.Match(input);
		if (!match.Success)
		{
			throw resolver.GetInvalidExpressionException();
		}
		ProcessWhere(input, match, resolver);
	}

	private static void ProcessSortingFields(Match match, TableViewResolver resolver)
	{
		Group obj = match.Groups["SortingFieldName"];
		if (obj.Success)
		{
			CaptureCollection captures = obj.Captures;
			for (int i = 0; i < captures.Count; i++)
			{
				resolver.AddSortingField(RemoveDoubleQuotes(captures[i].Value));
			}
		}
	}

	private static void ProcessSortOrder(Match match, TableViewResolver resolver)
	{
		Group obj = match.Groups["OrderValue"];
		if (!obj.Success)
		{
			return;
		}
		CaptureCollection captures = obj.Captures;
		for (int i = 0; i < captures.Count; i++)
		{
			if (captures[i].Length == 0)
			{
				resolver.AddSortOrder(SortOrder.Ascending);
				continue;
			}
			char c = captures[i].Value[0];
			if (c == '0' || c == 'A' || c == 'a')
			{
				resolver.AddSortOrder(SortOrder.Ascending);
			}
			else
			{
				resolver.AddSortOrder(SortOrder.Descending);
			}
		}
	}

	private static void ProcessWhere(string input, Match match, TableFilterResolver resolver, bool isCultureInvariant = false)
	{
		Group obj = match.Groups["WhereFieldName"];
		if (!obj.Success)
		{
			return;
		}
		Group obj2 = match.Groups["WhereExprType"];
		Group obj3 = match.Groups["WhereExpr"];
		int count = obj.Captures.Count;
		for (int i = 0; i < count; i++)
		{
			string fieldName = RemoveDoubleQuotes(obj.Captures[i].Value);
			Capture capture = obj2.Captures[i];
			int index = capture.Index;
			NCLMetaFilterType nCLMetaFilterType;
			switch (input[index])
			{
			case '0':
			case '1':
			case '2':
				nCLMetaFilterType = (NCLMetaFilterType)(input[index] - 48);
				break;
			case 'C':
			case 'c':
				nCLMetaFilterType = NCLMetaFilterType.Const;
				break;
			default:
				nCLMetaFilterType = ((string.Compare("filter", 0, input, index, capture.Length, StringComparison.OrdinalIgnoreCase) == 0) ? NCLMetaFilterType.Expression : NCLMetaFilterType.Field);
				break;
			}
			string text = obj3.Captures[i].Value;
			if (nCLMetaFilterType == NCLMetaFilterType.Const || nCLMetaFilterType == NCLMetaFilterType.Field)
			{
				text = RemoveDoubleQuotes(text);
			}
			resolver.AddFilter(fieldName, nCLMetaFilterType, text, isCultureInvariant);
		}
	}

	private static string RemoveDoubleQuotes(string value)
	{
		if (value.Length > 0 && value[0] == '"')
		{
			return value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
		}
		return value;
	}
}
