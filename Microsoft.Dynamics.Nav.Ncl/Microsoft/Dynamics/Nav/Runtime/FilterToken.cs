using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FilterToken : ISizableObject
{
	private static readonly FilterToken[] nonLiteralTokens = BuildNonLiteralTokens();

	private readonly FilterTokenType tokenType;

	private readonly string value;

	private readonly bool quoted;

	public virtual int ApproximateByteSize => ((value != null) ? (value.Length * 2) : 0) + SizableObject.GetApproximateByteSize(3) + 24;

	public FilterTokenType TokenType
	{
		[DebuggerStepThrough]
		get
		{
			return tokenType;
		}
	}

	public virtual string Value
	{
		[DebuggerStepThrough]
		get
		{
			return value;
		}
	}

	public bool Quoted
	{
		[DebuggerStepThrough]
		get
		{
			return quoted;
		}
	}

	public FilterToken(FilterTokenType tokenType, string value)
		: this(tokenType, value, quoted: false)
	{
	}

	public FilterToken(FilterTokenType tokenType, string value, bool quoted)
	{
		this.tokenType = tokenType;
		this.value = value;
		this.quoted = quoted;
	}

	protected FilterToken(FilterTokenType tokenType)
	{
		this.tokenType = tokenType;
	}

	public static FilterToken NonLiteral(FilterTokenType tokenType)
	{
		if (tokenType == FilterTokenType.Literal)
		{
			throw new NotSupportedException();
		}
		return nonLiteralTokens[(int)tokenType];
	}

	public static FilterToken Literal(string value, bool quoted)
	{
		return new FilterToken(FilterTokenType.Literal, value, quoted);
	}

	private static FilterToken[] BuildNonLiteralTokens()
	{
		int num = Enum.GetValues(typeof(FilterTokenType)).Cast<int>().Max();
		FilterToken[] array = new FilterToken[num + 1];
		for (int i = 0; i < array.Length; i++)
		{
			FilterTokenType filterTokenType = (FilterTokenType)i;
			string text;
			switch (filterTokenType)
			{
			case FilterTokenType.Equal:
				text = "=";
				break;
			case FilterTokenType.NotEqual:
				text = "<>";
				break;
			case FilterTokenType.GreaterThan:
				text = ">";
				break;
			case FilterTokenType.LessThan:
				text = "<";
				break;
			case FilterTokenType.GreaterThanOrEqual:
				text = ">=";
				break;
			case FilterTokenType.LessThanOrEqual:
				text = "<=";
				break;
			case FilterTokenType.And:
				text = "&";
				break;
			case FilterTokenType.Or:
				text = "|";
				break;
			case FilterTokenType.DotDot:
				text = "..";
				break;
			case FilterTokenType.OpenParenthesis:
				text = "(";
				break;
			case FilterTokenType.CloseParenthesis:
				text = ")";
				break;
			default:
				throw new NotSupportedException();
			case FilterTokenType.Literal:
			case FilterTokenType.BinaryValue:
			case FilterTokenType.BinaryWildcard:
			case FilterTokenType.BinaryNegatedWildcard:
				continue;
			}
			array[i] = new FilterToken(filterTokenType, text);
		}
		return array;
	}

	public override string ToString()
	{
		return TokenType.ToString() + " (" + Value + ")";
	}
}
