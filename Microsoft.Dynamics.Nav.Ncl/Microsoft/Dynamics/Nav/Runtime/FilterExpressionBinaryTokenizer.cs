using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class FilterExpressionBinaryTokenizer
{
	public static List<FilterToken> GetTokens(BinaryReader reader, FilterExpressionContext expressionContext)
	{
		List<FilterToken> list = new List<FilterToken>();
		byte[] array = new byte[8];
		while (true)
		{
			FilterExpressionUnmanagedToken token = (FilterExpressionUnmanagedToken)reader.ReadByte();
			FilterExpressionUnmanagedToken filterExpressionUnmanagedToken = token.MakeNormal();
			bool flag = token.IsSpecial();
			switch (filterExpressionUnmanagedToken)
			{
			case FilterExpressionUnmanagedToken.Terminator:
			case FilterExpressionUnmanagedToken.Terminator2:
				return list;
			case FilterExpressionUnmanagedToken.Equal:
				list.Add(FilterToken.NonLiteral(FilterTokenType.Equal));
				break;
			case FilterExpressionUnmanagedToken.NotEqual:
				list.Add(FilterToken.NonLiteral(FilterTokenType.NotEqual));
				break;
			case FilterExpressionUnmanagedToken.GreaterOrEqual:
				if (!flag)
				{
					list.Add(FilterToken.NonLiteral(FilterTokenType.GreaterThanOrEqual));
				}
				break;
			case FilterExpressionUnmanagedToken.LessThan:
				list.Add(FilterToken.NonLiteral(FilterTokenType.LessThan));
				break;
			case FilterExpressionUnmanagedToken.LessThanOrEqual:
				if (flag)
				{
					if (list.Count == 0 || list[list.Count - 1].TokenType != FilterTokenType.DotDot)
					{
						list.Add(FilterToken.NonLiteral(FilterTokenType.DotDot));
					}
				}
				else
				{
					list.Add(FilterToken.NonLiteral(FilterTokenType.LessThanOrEqual));
				}
				break;
			case FilterExpressionUnmanagedToken.GreaterThan:
				list.Add(FilterToken.NonLiteral(FilterTokenType.GreaterThan));
				break;
			case FilterExpressionUnmanagedToken.And:
				if (!flag)
				{
					list.Add(FilterToken.NonLiteral(FilterTokenType.And));
				}
				continue;
			case FilterExpressionUnmanagedToken.Or:
				list.Add(FilterToken.NonLiteral(FilterTokenType.Or));
				continue;
			case FilterExpressionUnmanagedToken.LeftParenthesis:
				list.Add(FilterToken.NonLiteral(FilterTokenType.OpenParenthesis));
				continue;
			case FilterExpressionUnmanagedToken.RightParenthesis:
				list.Add(FilterToken.NonLiteral(FilterTokenType.CloseParenthesis));
				continue;
			default:
				throw new NotSupportedException();
			case FilterExpressionUnmanagedToken.Wild:
			case FilterExpressionUnmanagedToken.NotWild:
				break;
			}
			int logicalLength = reader.ReadByte();
			int num = expressionContext.ByteLengthFromLogicalLength(logicalLength);
			if (num > array.Length)
			{
				uint num2 = RoundToNextHighestPowerOfTwo((uint)num);
				array = new byte[num2];
			}
			reader.Read(array, 0, num);
			NavValue navValue = expressionContext.ParseBinary(array, 0, num);
			if (filterExpressionUnmanagedToken - 2 > FilterExpressionUnmanagedToken.Terminator2)
			{
				if (filterExpressionUnmanagedToken == FilterExpressionUnmanagedToken.Wild)
				{
					goto IL_01d0;
				}
				if (filterExpressionUnmanagedToken == FilterExpressionUnmanagedToken.NotWild)
				{
					goto IL_01e6;
				}
			}
			else if (flag && expressionContext.SupportsWildcards && FilterExpressionParser.IsWildcard(navValue.ToString()))
			{
				list.RemoveAt(list.Count - 1);
				if (filterExpressionUnmanagedToken == FilterExpressionUnmanagedToken.Equal)
				{
					goto IL_01d0;
				}
				goto IL_01e6;
			}
			list.Add(new NavValueFilterToken(navValue));
			goto IL_0209;
			IL_01e6:
			list.Add(new FilterToken(FilterTokenType.BinaryNegatedWildcard, navValue.ToString()));
			goto IL_0209;
			IL_0209:
			if (flag && filterExpressionUnmanagedToken == FilterExpressionUnmanagedToken.GreaterOrEqual)
			{
				list.Add(FilterToken.NonLiteral(FilterTokenType.DotDot));
			}
			continue;
			IL_01d0:
			list.Add(new FilterToken(FilterTokenType.BinaryWildcard, navValue.ToString()));
			goto IL_0209;
		}
	}

	private static uint RoundToNextHighestPowerOfTwo(uint v)
	{
		v--;
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		v++;
		return v;
	}
}
