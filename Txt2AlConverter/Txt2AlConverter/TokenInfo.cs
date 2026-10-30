using System;
using Microsoft.Dynamics.Nav.Model.Compiler;

namespace Txt2AlConverter;

public class TokenInfo : IEquatable<TokenInfo>, IComparable<TokenInfo>
{
	public Token Token { get; private set; }

	public int Position { get; private set; }

	public int Length { get; private set; }

	public bool IsMultiLineComment => Token == Token.t_comment;

	public TokenInfo(Token tokenparameter, int columnParameter, int length)
	{
		Token = tokenparameter;
		Position = columnParameter;
		Length = length;
	}

	public static bool operator ==(TokenInfo left, TokenInfo right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(TokenInfo left, TokenInfo right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		TokenInfo tokenInfo = obj as TokenInfo;
		if (tokenInfo != null)
		{
			TokenInfo other = tokenInfo;
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Token.GetHashCode() ^ Position.GetHashCode();
	}

	public bool Equals(TokenInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (Token == other.Token)
		{
			return Position == other.Position;
		}
		return false;
	}

	public int CompareTo(TokenInfo other)
	{
		if (other == null)
		{
			return 1;
		}
		if (other.Equals(this))
		{
			return 0;
		}
		return Position - other.Position;
	}
}
