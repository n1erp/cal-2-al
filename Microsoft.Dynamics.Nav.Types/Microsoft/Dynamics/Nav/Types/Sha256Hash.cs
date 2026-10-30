using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Dynamics.Nav.Types;

public class Sha256Hash
{
	public const int HexStringLength = 64;

	public string HexString { get; }

	private Sha256Hash(string hexString)
	{
		if (hexString == null)
		{
			throw new ArgumentNullException("hexString");
		}
		string text = hexString.Trim().ToUpperInvariant();
		if (text.Length != 64 || !IsHexString(text))
		{
			throw new ArgumentException("A hash string must be composed of exactly " + 64 + " hex characters. The string " + text + " is not.");
		}
		HexString = text;
	}

	public static Sha256Hash ComputeHashForContent(byte[] content)
	{
		if (content == null)
		{
			return null;
		}
		using SHA256 sHA = SHA256.Create();
		return new Sha256Hash(ConvertHashToUppercaseHexString(sHA.ComputeHash(content, 0, content.Length)));
	}

	public static Sha256Hash CreateFromHexString(string hexString)
	{
		return new Sha256Hash(hexString);
	}

	private static bool IsHexString(string str)
	{
		foreach (char c in str)
		{
			if ((c < '0' || c > '9') && (c < 'A' || c > 'F'))
			{
				return false;
			}
		}
		return true;
	}

	private static string ConvertHashToUppercaseHexString(byte[] hash)
	{
		StringBuilder stringBuilder = new StringBuilder(hash.Length * 2);
		foreach (byte b in hash)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", b);
		}
		return stringBuilder.ToString();
	}

	protected bool Equals(Sha256Hash other)
	{
		return string.Equals(HexString, other.HexString);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((Sha256Hash)obj);
	}

	public override int GetHashCode()
	{
		if (HexString == null)
		{
			return 0;
		}
		return HexString.GetHashCode();
	}
}
