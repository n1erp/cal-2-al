using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public static class AlTableRelationCalcFormula
{
	public static string QuoteIdentifiers(string value)
	{
		try
		{
			return ParseFunction(value);
		}
		catch (Exception)
		{
			return value;
		}
	}

	private static bool IsQuote(char ch)
	{
		if (ch != '"')
		{
			return ch == '\'';
		}
		return true;
	}

	private static string ExtractQuotedText(string value, ref int index)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char c = value[index];
		stringBuilder.Append(value[index]);
		index++;
		while (index < value.Length)
		{
			char c2 = value[index];
			stringBuilder.Append(c2);
			if (c2 == c)
			{
				return stringBuilder.ToString();
			}
			index++;
		}
		return stringBuilder.ToString();
	}

	private static string ParseFunction(string value)
	{
		if (int.TryParse(value, out var _))
		{
			return value;
		}
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		bool flag = false;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsQuote(c))
			{
				string value2 = ExtractQuotedText(value, ref i);
				stringBuilder2.Append(value2);
				flag = false;
				continue;
			}
			if (string.Equals("if", stringBuilder2.ToString().Trim(), StringComparison.OrdinalIgnoreCase))
			{
				return ParseConditions(value);
			}
			switch (c)
			{
			case '=':
			{
				string text = QuoteIdentifierIfNeeded(stringBuilder2.ToString());
				stringBuilder.Append(text + "=");
				stringBuilder2.Clear();
				flag = false;
				break;
			}
			case '(':
			{
				string text2 = ExtractContentInsideParantheses(value.Substring(i));
				string a = FromParanthesisFindFunction(value, i, out var functionStartIndex);
				string value3;
				if (string.Equals(a, "where", StringComparison.OrdinalIgnoreCase))
				{
					string text3 = value.Substring(functionStartIndex, i - functionStartIndex).ToLowerInvariant();
					string text4 = QuoteIdentifierIfNeeded(stringBuilder2.ToString().Substring(0, stringBuilder2.Length - text3.Length));
					stringBuilder2.Clear();
					stringBuilder2.Append(text4 + text3);
					value3 = ParseArgumentList(text2);
					stringBuilder.Append(stringBuilder2);
				}
				else if (string.Equals(a, "filter", StringComparison.OrdinalIgnoreCase))
				{
					value3 = FilterFunction(text2);
					stringBuilder.Append(stringBuilder2.ToString().ToLowerInvariant());
				}
				else if (string.Equals(a, "const", StringComparison.OrdinalIgnoreCase))
				{
					value3 = ConstFunction(text2);
					stringBuilder.Append(stringBuilder2.ToString().ToLowerInvariant());
				}
				else
				{
					value3 = ParseFunction(text2);
					stringBuilder.Append(stringBuilder2.ToString().ToLowerInvariant());
				}
				stringBuilder.Append("(");
				stringBuilder.Append(value3);
				stringBuilder.Append(")");
				i += text2.Length + 1;
				stringBuilder2.Clear();
				flag = false;
				break;
			}
			default:
				if (!char.IsLetter(c))
				{
					flag = true;
				}
				stringBuilder2.Append(c);
				break;
			}
		}
		if (stringBuilder2.Length > 0)
		{
			if (flag || AlUtilities.IsKeyword(stringBuilder2.ToString().Trim()))
			{
				stringBuilder.Append(QuoteIdentifierIfNeeded(stringBuilder2.ToString()));
			}
			else
			{
				stringBuilder.Append(stringBuilder2);
			}
		}
		return stringBuilder.ToString();
	}

	private static string ParseArgumentList(string value)
	{
		List<string> list = ArgumentsSplitter(value, ',');
		List<string> list2 = new List<string>();
		foreach (string item in list)
		{
			List<string> list3 = ArgumentsSplitter(item, '=');
			if (list3.Count != 2)
			{
				ArgumentException ex = new ArgumentException("A parameter in the parameterList did not contain just a single equals sign \r\n");
				ex.Data["parameter"] = item;
				throw ex;
			}
			string text = "=";
			string text2 = QuoteIdentifierIfNeeded(list3[0]);
			string text3 = ParseFunction(list3[1]);
			list2.Add(text2 + text + text3);
		}
		return string.Join(",", list2);
	}

	private static string ExtractContentInsideParantheses(string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsQuote(c))
			{
				string value2 = ExtractQuotedText(value, ref i);
				stringBuilder.Append(value2);
				continue;
			}
			stringBuilder.Append(c);
			if (c == '(')
			{
				num++;
			}
			if (c == ')')
			{
				num--;
			}
			if (c == ')' && num == 0)
			{
				break;
			}
		}
		string text = stringBuilder.ToString();
		return text.Substring(1, text.Length - 2);
	}

	private static string ConstFunction(string value)
	{
		return ConvertIfBoolean(value);
	}

	private static string ConvertIfBoolean(string value)
	{
		if (string.Equals(value, "No", StringComparison.OrdinalIgnoreCase))
		{
			return "true";
		}
		if (string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase))
		{
			return "false";
		}
		return value;
	}

	private static string FilterFunction(string value)
	{
		IEnumerable<string> values = from orValue in ArgumentsSplitter(value, '|')
			select string.Join("&", ArgumentsSplitter(orValue, '&').Select(FilterValue));
		return string.Join("|", values);
	}

	private static string FilterValue(string value)
	{
		Tuple<string, string, string> tuple = OperatorSplit(value);
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(tuple.Item1))
		{
			string text = ConvertIfBoolean(tuple.Item1);
			if (text == tuple.Item1 && !IsConvertedBooleanValue(text))
			{
				text = QuoteIdentifierIfNeeded(text);
			}
			stringBuilder.Append(text);
		}
		if (!string.IsNullOrEmpty(tuple.Item2))
		{
			stringBuilder.Append(tuple.Item2);
		}
		if (!string.IsNullOrEmpty(tuple.Item3))
		{
			string text2 = ConvertIfBoolean(tuple.Item3);
			if (text2 == tuple.Item3 && !IsConvertedBooleanValue(text2))
			{
				text2 = QuoteIdentifierIfNeeded(text2);
			}
			stringBuilder.Append(text2);
		}
		return stringBuilder.ToString();
	}

	private static bool IsConvertedBooleanValue(string value)
	{
		if (!value.Equals("true", StringComparison.OrdinalIgnoreCase))
		{
			return value.Equals("false", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static Tuple<string, string, string> OperatorSplit(string value)
	{
		string item = null;
		string text = null;
		string item2 = null;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			string filterOperator;
			if (IsQuote(c))
			{
				stringBuilder.Append(ExtractQuotedText(value, ref i));
			}
			else if (IsFilterOperator(value, i, out filterOperator))
			{
				item = stringBuilder.ToString();
				stringBuilder.Clear();
				text = filterOperator;
				i += filterOperator.Length - 1;
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			item = stringBuilder.ToString();
		}
		else
		{
			item2 = stringBuilder.ToString();
		}
		return new Tuple<string, string, string>(item, text, item2);
	}

	private static bool IsFilterOperator(string value, int index, out string filterOperator)
	{
		char c = value[index];
		if (index + 1 >= value.Length)
		{
			filterOperator = null;
			return false;
		}
		string text = value.Substring(index, 2);
		if (new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "<>", "<=", ">=", ".." }.Contains(text))
		{
			filterOperator = text;
			return true;
		}
		switch (c)
		{
		case '<':
			filterOperator = "<";
			return true;
		case '>':
			filterOperator = ">";
			return true;
		case '=':
			filterOperator = "=";
			return true;
		default:
			filterOperator = null;
			return false;
		}
	}

	private static List<string> ArgumentsSplitter(string value, char seperator)
	{
		List<string> list = new List<string>();
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsQuote(c))
			{
				string value2 = ExtractQuotedText(value, ref i);
				stringBuilder.Append(value2);
				continue;
			}
			switch (c)
			{
			case '(':
				num++;
				break;
			case ')':
				num--;
				break;
			}
			if (c == seperator && num == 0)
			{
				list.Add(stringBuilder.ToString());
				stringBuilder.Clear();
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		list.Add(stringBuilder.ToString());
		return list;
	}

	private static string ParseConditions(string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		bool flag = false;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsQuote(c))
			{
				string value2 = ExtractQuotedText(value, ref i);
				stringBuilder2.Append(value2);
				continue;
			}
			if ((c == '(') & flag)
			{
				string text = ExtractContentInsideParantheses(value.Substring(i));
				stringBuilder.Append(stringBuilder2);
				stringBuilder2.Clear();
				stringBuilder.Append("(");
				stringBuilder.Append(ParseArgumentList(text));
				stringBuilder.Append(")");
				i += text.Length + 1;
				flag = false;
				continue;
			}
			switch (c)
			{
			case 'I':
			case 'i':
				stringBuilder2.Append(c);
				if (i + "if".Length < value.Length && string.Equals("if", value.Substring(i, "if".Length), StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
				}
				continue;
			case 'E':
			case 'e':
				break;
			default:
				stringBuilder2.Append(c);
				continue;
			}
			if (i + "else".Length < value.Length && string.Equals("else", value.Substring(i, "else".Length), StringComparison.OrdinalIgnoreCase))
			{
				bool flag2 = false;
				for (int j = i + "else".Length; j < value.Length; j++)
				{
					if ((value[j] == 'i' || value[j] == 'I') && j + "if".Length < value.Length && string.Equals("if", value.Substring(j, "if".Length), StringComparison.OrdinalIgnoreCase))
					{
						flag = true;
						string value3 = ParseFunction(stringBuilder2.ToString());
						stringBuilder2.Clear();
						stringBuilder.Append(value3);
						string text2 = value.Substring(i, j - i + "if".Length);
						stringBuilder2.Append(text2);
						i += text2.Length - 1;
						flag2 = true;
						break;
					}
					if (!char.IsWhiteSpace(value[j]))
					{
						break;
					}
				}
				if (!flag2)
				{
					string value4 = ParseFunction(stringBuilder2.ToString());
					stringBuilder.Append(value4);
					stringBuilder2.Clear();
					string value5 = value.Substring(i, "else".Length);
					stringBuilder.Append(value5);
					string value6 = ParseFunction(value.Substring(i + "else".Length));
					stringBuilder.Append(value6);
					stringBuilder2.Clear();
					break;
				}
			}
			else
			{
				stringBuilder2.Append(c);
			}
		}
		stringBuilder.Append(ParseFunction(stringBuilder2.ToString()));
		return stringBuilder.ToString();
	}

	private static string FromParanthesisFindFunction(string value, int index, out int functionStartIndex)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		functionStartIndex = -1;
		int num = -1;
		for (int num2 = index; num2 >= 0; num2--)
		{
			if (char.IsLetterOrDigit(value[num2]))
			{
				if (num == -1)
				{
					num = num2;
				}
			}
			else if (num > 0)
			{
				functionStartIndex = num2 + 1;
				return value.Substring(num2 + 1, num - num2);
			}
		}
		if (num > -1)
		{
			functionStartIndex = 0;
			return value.Substring(0, num + 1);
		}
		throw new ArgumentException("Text did not contain a function", "value");
	}

	private static string QuoteIdentifierIfNeeded(string identifier)
	{
		if (string.IsNullOrWhiteSpace(identifier))
		{
			return identifier;
		}
		if (int.TryParse(identifier, out var _))
		{
			return identifier;
		}
		string text = identifier.Substring(0, identifier.Length - identifier.TrimStart().Length);
		string text2 = identifier.Substring(identifier.Length - (identifier.Length - identifier.TrimEnd().Length));
		string text3 = identifier.Trim();
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		List<string> list = new List<string>();
		for (int i = 0; i < text3.Length; i++)
		{
			char c = text3[i];
			if (IsQuote(c))
			{
				if (stringBuilder.Length == 0)
				{
					flag = false;
				}
				string value = ExtractQuotedText(text3, ref i);
				stringBuilder.Append(value);
			}
			else if (c == '.')
			{
				if (i == text3.Length - 1)
				{
					flag = true;
					stringBuilder.Append(c);
				}
				else if (char.IsWhiteSpace(text3[i + 1]) || text3[i + 1] == '-')
				{
					flag = true;
					stringBuilder.Append(c);
					continue;
				}
				string text4 = stringBuilder.ToString();
				stringBuilder.Clear();
				if (flag || AlUtilities.IsKeyword(text4))
				{
					text4 = "\"" + text4 + "\"";
				}
				list.Add(text4);
				flag = false;
			}
			else if (char.IsLetter(c))
			{
				stringBuilder.Append(c);
			}
			else
			{
				flag = true;
				stringBuilder.Append(c);
			}
		}
		if (stringBuilder.Length > 0)
		{
			string text5 = stringBuilder.ToString();
			stringBuilder.Clear();
			if (flag || AlUtilities.IsKeyword(text5))
			{
				text5 = "\"" + text5 + "\"";
			}
			list.Add(text5);
		}
		string text6 = string.Join(".", list);
		if (AlUtilities.IsKeyword(text6))
		{
			text6 = "\"" + text6 + "\"";
		}
		return text + text6 + text2;
	}
}
