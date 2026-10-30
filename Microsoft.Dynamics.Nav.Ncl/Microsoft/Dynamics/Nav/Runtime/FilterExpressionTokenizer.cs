using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class FilterExpressionTokenizer
{
	public static List<FilterToken> GetTokens(string input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		List<FilterToken> list = new List<FilterToken>(16);
		int unquotedLiteralStartPosition = -1;
		int num = -1;
		for (int i = 0; i < input.Length; i++)
		{
			switch (input[i])
			{
			case '=':
				AddAnyUnquotedLiteral(ref unquotedLiteralStartPosition, list, input, i, num);
				list.Add(FilterToken.NonLiteral(FilterTokenType.Equal));
				continue;
			case '<':
				AddAnyUnquotedLiteral(ref unquotedLiteralStartPosition, list, input, i, num);
				switch (CharAtOrDefault(i + 1, input))
				{
				case '>':
					list.Add(FilterToken.NonLiteral(FilterTokenType.NotEqual));
					i++;
					break;
				case '=':
					list.Add(FilterToken.NonLiteral(FilterTokenType.LessThanOrEqual));
					i++;
					break;
				default:
					list.Add(FilterToken.NonLiteral(FilterTokenType.LessThan));
					break;
				}
				continue;
			case '>':
			{
				AddAnyUnquotedLiteral(ref unquotedLiteralStartPosition, list, input, i, num);
				char c = CharAtOrDefault(i + 1, input);
				if (c == '=')
				{
					list.Add(FilterToken.NonLiteral(FilterTokenType.GreaterThanOrEqual));
					i++;
				}
				else
				{
					list.Add(FilterToken.NonLiteral(FilterTokenType.GreaterThan));
				}
				continue;
			}
			case '.':
				if (CharAtOrDefault(i + 1, input) == '.')
				{
					AddAnyUnquotedLiteral(ref unquotedLiteralStartPosition, list, input, i, num);
					list.Add(FilterToken.NonLiteral(FilterTokenType.DotDot));
					i++;
					continue;
				}
				break;
			case '(':
				AddAnyUnquotedLiteral(ref unquotedLiteralStartPosition, list, input, i, num);
				list.Add(FilterToken.NonLiteral(FilterTokenType.OpenParenthesis));
				continue;
			case ')':
				AddAnyUnquotedLiteral(ref unquotedLiteralStartPosition, list, input, i, num);
				list.Add(FilterToken.NonLiteral(FilterTokenType.CloseParenthesis));
				continue;
			case '&':
				AddAnyUnquotedLiteral(ref unquotedLiteralStartPosition, list, input, i, num);
				list.Add(FilterToken.NonLiteral(FilterTokenType.And));
				continue;
			case '|':
				AddAnyUnquotedLiteral(ref unquotedLiteralStartPosition, list, input, i, num);
				list.Add(FilterToken.NonLiteral(FilterTokenType.Or));
				continue;
			case '\'':
			{
				if (unquotedLiteralStartPosition >= 0)
				{
					continue;
				}
				int num2 = ++i;
				int num3 = -1;
				bool flag = false;
				for (; i < input.Length; i++)
				{
					if (input[i] == '\'')
					{
						if (CharAtOrDefault(i + 1, input) != '\'')
						{
							num3 = i;
							break;
						}
						i++;
						flag = true;
					}
				}
				if (num3 < 0)
				{
					throw new NavInvalidFilterExpressionException(Lang.FilterExpressionQuoteMissing);
				}
				string text = input.Substring(num2, num3 - num2);
				if (flag)
				{
					text = text.Replace("''", "'");
				}
				list.Add(FilterToken.Literal(text, quoted: true));
				continue;
			}
			case '\t':
			case ' ':
				if (num < 0)
				{
					num = i;
				}
				continue;
			}
			if (unquotedLiteralStartPosition < 0)
			{
				unquotedLiteralStartPosition = i;
			}
			num = -1;
		}
		AddAnyUnquotedLiteral(ref unquotedLiteralStartPosition, list, input, input.Length, num);
		return list;
	}

	private static void AddAnyUnquotedLiteral(ref int unquotedLiteralStartPosition, List<FilterToken> tokens, string input, int currentPosition, int lastWhitespaceStartPosition)
	{
		if (unquotedLiteralStartPosition >= 0)
		{
			int num = ((lastWhitespaceStartPosition < 0) ? currentPosition : lastWhitespaceStartPosition) - 1;
			tokens.Add(FilterToken.Literal(input.Substring(unquotedLiteralStartPosition, num - unquotedLiteralStartPosition + 1), quoted: false));
			unquotedLiteralStartPosition = -1;
		}
	}

	private static char CharAtOrDefault(int index, string input)
	{
		if (index >= input.Length)
		{
			return '\0';
		}
		return input[index];
	}
}
