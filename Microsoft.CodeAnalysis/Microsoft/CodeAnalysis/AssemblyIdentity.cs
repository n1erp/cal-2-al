using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public sealed class AssemblyIdentity : IEquatable<AssemblyIdentity>
{
	private readonly AssemblyContentType _contentType;

	private readonly string _name;

	private readonly Version _version;

	private readonly string _cultureName;

	private readonly ImmutableArray<byte> _publicKey;

	private ImmutableArray<byte> _lazyPublicKeyToken;

	private readonly bool _isRetargetable;

	private string _lazyDisplayName;

	private int _lazyHashCode;

	internal const int PublicKeyTokenSize = 8;

	internal static readonly Version NullVersion = new Version(0, 0, 0, 0);

	internal const string InvariantCultureDisplay = "neutral";

	private const int PublicKeyTokenBytes = 8;

	public string Name => _name;

	public Version Version => _version;

	public string CultureName => _cultureName;

	public AssemblyNameFlags Flags => (AssemblyNameFlags)((_isRetargetable ? 256 : 0) | (HasPublicKey ? 1 : 0));

	public AssemblyContentType ContentType => _contentType;

	public bool HasPublicKey => _publicKey.Length > 0;

	public ImmutableArray<byte> PublicKey => _publicKey;

	public ImmutableArray<byte> PublicKeyToken
	{
		get
		{
			if (_lazyPublicKeyToken.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _lazyPublicKeyToken, CalculatePublicKeyToken(_publicKey), default);
			}
			return _lazyPublicKeyToken;
		}
	}

	public bool IsStrongName
	{
		get
		{
			if (!HasPublicKey)
			{
				return _lazyPublicKeyToken.Length > 0;
			}
			return true;
		}
	}

	public bool IsRetargetable => _isRetargetable;

	private AssemblyIdentity(AssemblyIdentity other, Version version)
	{
		_contentType = other.ContentType;
		_name = other._name;
		_cultureName = other._cultureName;
		_publicKey = other._publicKey;
		_lazyPublicKeyToken = other._lazyPublicKeyToken;
		_isRetargetable = other._isRetargetable;
		_version = version;
		_lazyDisplayName = null;
		_lazyHashCode = 0;
	}

	internal AssemblyIdentity WithVersion(Version version)
	{
		if (!(version == _version))
		{
			return new AssemblyIdentity(this, version);
		}
		return this;
	}

	public AssemblyIdentity(string name, Version version = null, string cultureName = null, ImmutableArray<byte> publicKeyOrToken = default(ImmutableArray<byte>), bool hasPublicKey = false, bool isRetargetable = false, AssemblyContentType contentType = AssemblyContentType.Default)
	{
		if (!IsValid(contentType))
		{
			throw new ArgumentOutOfRangeException("contentType", CodeAnalysisResources.InvalidContentType);
		}
		if (!IsValidName(name))
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.InvalidAssemblyName, name), "name");
		}
		if (!IsValidCultureName(cultureName))
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.InvalidCultureName, cultureName), "cultureName");
		}
		if (!IsValid(version))
		{
			throw new ArgumentOutOfRangeException("version");
		}
		if (hasPublicKey)
		{
			if (!MetadataHelpers.IsValidPublicKey(publicKeyOrToken))
			{
				throw new ArgumentException(CodeAnalysisResources.InvalidPublicKey, "publicKeyOrToken");
			}
		}
		else if (!publicKeyOrToken.IsDefaultOrEmpty && publicKeyOrToken.Length != 8)
		{
			throw new ArgumentException(CodeAnalysisResources.InvalidSizeOfPublicKeyToken, "publicKeyOrToken");
		}
		if (isRetargetable && contentType == AssemblyContentType.WindowsRuntime)
		{
			throw new ArgumentException(CodeAnalysisResources.WinRTIdentityCantBeRetargetable, "isRetargetable");
		}
		_name = name;
		_version = version ?? NullVersion;
		_cultureName = NormalizeCultureName(cultureName);
		_isRetargetable = isRetargetable;
		_contentType = contentType;
		InitializeKey(publicKeyOrToken, hasPublicKey, out _publicKey, out _lazyPublicKeyToken);
	}

	internal AssemblyIdentity(string name, Version version, string cultureName, ImmutableArray<byte> publicKeyOrToken, bool hasPublicKey)
	{
		_name = name;
		_version = version ?? NullVersion;
		_cultureName = NormalizeCultureName(cultureName);
		_isRetargetable = false;
		_contentType = AssemblyContentType.Default;
		InitializeKey(publicKeyOrToken, hasPublicKey, out _publicKey, out _lazyPublicKeyToken);
	}

	internal AssemblyIdentity(bool noThrow, string name, Version version = null, string cultureName = null, ImmutableArray<byte> publicKeyOrToken = default(ImmutableArray<byte>), bool hasPublicKey = false, bool isRetargetable = false, AssemblyContentType contentType = AssemblyContentType.Default)
	{
		_name = name;
		_version = version ?? NullVersion;
		_cultureName = NormalizeCultureName(cultureName);
		_contentType = (IsValid(contentType) ? contentType : AssemblyContentType.Default);
		_isRetargetable = isRetargetable && _contentType != AssemblyContentType.WindowsRuntime;
		InitializeKey(publicKeyOrToken, hasPublicKey, out _publicKey, out _lazyPublicKeyToken);
	}

	private static string NormalizeCultureName(string cultureName)
	{
		if (cultureName != null && !AssemblyIdentityComparer.CultureComparer.Equals(cultureName, "neutral"))
		{
			return cultureName;
		}
		return string.Empty;
	}

	private static void InitializeKey(ImmutableArray<byte> publicKeyOrToken, bool hasPublicKey, out ImmutableArray<byte> publicKey, out ImmutableArray<byte> publicKeyToken)
	{
		if (hasPublicKey)
		{
			publicKey = publicKeyOrToken;
			publicKeyToken = default;
		}
		else
		{
			publicKey = ImmutableArray<byte>.Empty;
			publicKeyToken = publicKeyOrToken.NullToEmpty();
		}
	}

	internal static bool IsValidCultureName(string name)
	{
		if (name != null)
		{
			return name.IndexOf('\0') < 0;
		}
		return true;
	}

	private static bool IsValidName(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			return name.IndexOf('\0') < 0;
		}
		return false;
	}

	private static bool IsValid(Version value)
	{
		if (!(value == null))
		{
			if (value.Major >= 0 && value.Minor >= 0 && value.Build >= 0 && value.Revision >= 0 && value.Major <= 65535 && value.Minor <= 65535 && value.Build <= 65535)
			{
				return value.Revision <= 65535;
			}
			return false;
		}
		return true;
	}

	private static bool IsValid(AssemblyContentType value)
	{
		if (value >= AssemblyContentType.Default)
		{
			return value <= AssemblyContentType.WindowsRuntime;
		}
		return false;
	}

	internal static bool IsFullName(AssemblyIdentityParts parts)
	{
		if ((parts & (AssemblyIdentityParts.Version | AssemblyIdentityParts.Name | AssemblyIdentityParts.Culture)) == (AssemblyIdentityParts.Version | AssemblyIdentityParts.Name | AssemblyIdentityParts.Culture))
		{
			return (parts & AssemblyIdentityParts.PublicKeyOrToken) != 0;
		}
		return false;
	}

	public static bool operator ==(AssemblyIdentity left, AssemblyIdentity right)
	{
		return EqualityComparer<AssemblyIdentity>.Default.Equals(left, right);
	}

	public static bool operator !=(AssemblyIdentity left, AssemblyIdentity right)
	{
		return !(left == right);
	}

	public bool Equals(AssemblyIdentity obj)
	{
		if ((object)obj != null && (_lazyHashCode == 0 || obj._lazyHashCode == 0 || _lazyHashCode == obj._lazyHashCode))
		{
			return MemberwiseEqual(this, obj) == true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as AssemblyIdentity);
	}

	public override int GetHashCode()
	{
		if (_lazyHashCode == 0)
		{
			_lazyHashCode = Hash.Combine(AssemblyIdentityComparer.SimpleNameComparer.GetHashCode(_name), Hash.Combine(_version.GetHashCode(), GetHashCodeIgnoringNameAndVersion()));
		}
		return _lazyHashCode;
	}

	internal int GetHashCodeIgnoringNameAndVersion()
	{
		return Hash.Combine((int)_contentType, Hash.Combine(_isRetargetable, AssemblyIdentityComparer.CultureComparer.GetHashCode(_cultureName)));
	}

	internal static ImmutableArray<byte> CalculatePublicKeyToken(ImmutableArray<byte> publicKey)
	{
		ImmutableArray<byte> immutableArray = CryptographicHashProvider.ComputeSha1(publicKey);
		int num = immutableArray.Length - 1;
		ArrayBuilder<byte> instance = ArrayBuilder<byte>.GetInstance(8);
		for (int i = 0; i < 8; i++)
		{
			instance.Add(immutableArray[num - i]);
		}
		return instance.ToImmutableAndFree();
	}

	internal static bool? MemberwiseEqual(AssemblyIdentity x, AssemblyIdentity y)
	{
		if ((object)x == y)
		{
			return true;
		}
		if (!AssemblyIdentityComparer.SimpleNameComparer.Equals(x._name, y._name))
		{
			return false;
		}
		if (x._version.Equals(y._version) && EqualIgnoringNameAndVersion(x, y))
		{
			return true;
		}
		return null;
	}

	internal static bool EqualIgnoringNameAndVersion(AssemblyIdentity x, AssemblyIdentity y)
	{
		if (x.IsRetargetable == y.IsRetargetable && x.ContentType == y.ContentType && AssemblyIdentityComparer.CultureComparer.Equals(x.CultureName, y.CultureName))
		{
			return KeysEqual(x, y);
		}
		return false;
	}

	internal static bool KeysEqual(AssemblyIdentity x, AssemblyIdentity y)
	{
		ImmutableArray<byte> lazyPublicKeyToken = x._lazyPublicKeyToken;
		ImmutableArray<byte> lazyPublicKeyToken2 = y._lazyPublicKeyToken;
		if (!lazyPublicKeyToken.IsDefault && !lazyPublicKeyToken2.IsDefault)
		{
			return lazyPublicKeyToken.SequenceEqual(lazyPublicKeyToken2);
		}
		if (lazyPublicKeyToken.IsDefault && lazyPublicKeyToken2.IsDefault)
		{
			return x._publicKey.SequenceEqual(y._publicKey);
		}
		if (lazyPublicKeyToken.IsDefault)
		{
			return x.PublicKeyToken.SequenceEqual(lazyPublicKeyToken2);
		}
		return lazyPublicKeyToken.SequenceEqual(y.PublicKeyToken);
	}

	public static AssemblyIdentity FromAssemblyDefinition(Assembly assembly)
	{
		if ((object)assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		return FromAssemblyDefinition(assembly.GetName());
	}

	internal static AssemblyIdentity FromAssemblyDefinition(AssemblyName name)
	{
		byte[] publicKey = name.GetPublicKey();
		ImmutableArray<byte> publicKeyOrToken = ((publicKey != null) ? ImmutableArray.Create(publicKey) : ImmutableArray<byte>.Empty);
		return new AssemblyIdentity(name.Name, name.Version, name.CultureName, publicKeyOrToken, publicKeyOrToken.Length > 0, (name.Flags & AssemblyNameFlags.Retargetable) != 0, name.ContentType);
	}

	internal static AssemblyIdentity FromAssemblyReference(AssemblyName name)
	{
		return new AssemblyIdentity(name.Name, name.Version, name.CultureName, ImmutableArray.Create(name.GetPublicKeyToken()), hasPublicKey: false, (name.Flags & AssemblyNameFlags.Retargetable) != 0, name.ContentType);
	}

	public string GetDisplayName(bool fullKey = false)
	{
		if (fullKey)
		{
			return BuildDisplayName(fullKey: true);
		}
		if (_lazyDisplayName == null)
		{
			_lazyDisplayName = BuildDisplayName(fullKey: false);
		}
		return _lazyDisplayName;
	}

	public override string ToString()
	{
		return GetDisplayName();
	}

	internal static string PublicKeyToString(ImmutableArray<byte> key)
	{
		if (key.IsDefaultOrEmpty)
		{
			return "";
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		_ = instance.Builder;
		AppendKey(instance, key);
		return instance.ToStringAndFree();
	}

	private string BuildDisplayName(bool fullKey)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		EscapeName(builder, Name);
		builder.Append(", Version=");
		builder.Append(_version.Major);
		builder.Append(".");
		builder.Append(_version.Minor);
		builder.Append(".");
		builder.Append(_version.Build);
		builder.Append(".");
		builder.Append(_version.Revision);
		builder.Append(", Culture=");
		if (_cultureName.Length == 0)
		{
			builder.Append("neutral");
		}
		else
		{
			EscapeName(builder, _cultureName);
		}
		if (fullKey && HasPublicKey)
		{
			builder.Append(", PublicKey=");
			AppendKey(builder, _publicKey);
		}
		else
		{
			builder.Append(", PublicKeyToken=");
			if (PublicKeyToken.Length > 0)
			{
				AppendKey(builder, PublicKeyToken);
			}
			else
			{
				builder.Append("null");
			}
		}
		if (IsRetargetable)
		{
			builder.Append(", Retargetable=Yes");
		}
		switch (_contentType)
		{
		case AssemblyContentType.WindowsRuntime:
			builder.Append(", ContentType=WindowsRuntime");
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(_contentType);
		case AssemblyContentType.Default:
			break;
		}
		string result = builder.ToString();
		instance.Free();
		return result;
	}

	private static void AppendKey(StringBuilder sb, ImmutableArray<byte> key)
	{
		foreach (byte item in key)
		{
			sb.Append(item.ToString("x2"));
		}
	}

	private string GetDebuggerDisplay()
	{
		return GetDisplayName(fullKey: true);
	}

	public static bool TryParseDisplayName(string displayName, out AssemblyIdentity identity)
	{
		if (displayName == null)
		{
			throw new ArgumentNullException("displayName");
		}
		AssemblyIdentityParts parts;
		return TryParseDisplayName(displayName, out identity, out parts);
	}

	public static bool TryParseDisplayName(string displayName, out AssemblyIdentity identity, out AssemblyIdentityParts parts)
	{
		identity = null;
		parts = (AssemblyIdentityParts)0;
		if (displayName == null)
		{
			throw new ArgumentNullException("displayName");
		}
		if (displayName.IndexOf('\0') >= 0)
		{
			return false;
		}
		int position = 0;
		if (!TryParseNameToken(displayName, ref position, out var value))
		{
			return false;
		}
		AssemblyIdentityParts assemblyIdentityParts = AssemblyIdentityParts.Name;
		AssemblyIdentityParts assemblyIdentityParts2 = AssemblyIdentityParts.Name;
		Version version = null;
		string cultureName = null;
		bool flag = false;
		AssemblyContentType assemblyContentType = AssemblyContentType.Default;
		ImmutableArray<byte> immutableArray = default;
		ImmutableArray<byte> immutableArray2 = default;
		while (position < displayName.Length)
		{
			if (displayName[position] != ',')
			{
				return false;
			}
			position++;
			if (!TryParseNameToken(displayName, ref position, out var value2))
			{
				return false;
			}
			if (position >= displayName.Length || displayName[position] != '=')
			{
				return false;
			}
			position++;
			if (!TryParseNameToken(displayName, ref position, out var value3))
			{
				return false;
			}
			if (string.Equals(value2, "Version", StringComparison.OrdinalIgnoreCase))
			{
				if ((assemblyIdentityParts2 & AssemblyIdentityParts.Version) != 0)
				{
					return false;
				}
				assemblyIdentityParts2 |= AssemblyIdentityParts.Version;
				if (!(value3 == "*"))
				{
					if (!TryParseVersion(value3, out var result, out var parts2))
					{
						return false;
					}
					version = ToVersion(result);
					assemblyIdentityParts |= parts2;
				}
			}
			else if (string.Equals(value2, "Culture", StringComparison.OrdinalIgnoreCase) || string.Equals(value2, "Language", StringComparison.OrdinalIgnoreCase))
			{
				if ((assemblyIdentityParts2 & AssemblyIdentityParts.Culture) != 0)
				{
					return false;
				}
				assemblyIdentityParts2 |= AssemblyIdentityParts.Culture;
				if (!(value3 == "*"))
				{
					cultureName = (string.Equals(value3, "neutral", StringComparison.OrdinalIgnoreCase) ? null : value3);
					assemblyIdentityParts |= AssemblyIdentityParts.Culture;
				}
			}
			else if (string.Equals(value2, "PublicKey", StringComparison.OrdinalIgnoreCase))
			{
				if ((assemblyIdentityParts2 & AssemblyIdentityParts.PublicKey) != 0)
				{
					return false;
				}
				assemblyIdentityParts2 |= AssemblyIdentityParts.PublicKey;
				if (!(value3 == "*"))
				{
					if (!TryParsePublicKey(value3, out var key))
					{
						return false;
					}
					immutableArray = key;
					assemblyIdentityParts |= AssemblyIdentityParts.PublicKey;
				}
			}
			else if (string.Equals(value2, "PublicKeyToken", StringComparison.OrdinalIgnoreCase))
			{
				if ((assemblyIdentityParts2 & AssemblyIdentityParts.PublicKeyToken) != 0)
				{
					return false;
				}
				assemblyIdentityParts2 |= AssemblyIdentityParts.PublicKeyToken;
				if (!(value3 == "*"))
				{
					if (!TryParsePublicKeyToken(value3, out var token))
					{
						return false;
					}
					immutableArray2 = token;
					assemblyIdentityParts |= AssemblyIdentityParts.PublicKeyToken;
				}
			}
			else if (string.Equals(value2, "Retargetable", StringComparison.OrdinalIgnoreCase))
			{
				if ((assemblyIdentityParts2 & AssemblyIdentityParts.Retargetability) != 0)
				{
					return false;
				}
				assemblyIdentityParts2 |= AssemblyIdentityParts.Retargetability;
				if (value3 == "*")
				{
					continue;
				}
				if (string.Equals(value3, "Yes", StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
				}
				else
				{
					if (!string.Equals(value3, "No", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					flag = false;
				}
				assemblyIdentityParts |= AssemblyIdentityParts.Retargetability;
			}
			else if (string.Equals(value2, "ContentType", StringComparison.OrdinalIgnoreCase))
			{
				if ((assemblyIdentityParts2 & AssemblyIdentityParts.ContentType) != 0)
				{
					return false;
				}
				assemblyIdentityParts2 |= AssemblyIdentityParts.ContentType;
				if (!(value3 == "*"))
				{
					if (!string.Equals(value3, "WindowsRuntime", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					assemblyContentType = AssemblyContentType.WindowsRuntime;
					assemblyIdentityParts |= AssemblyIdentityParts.ContentType;
				}
			}
			else
			{
				assemblyIdentityParts |= AssemblyIdentityParts.Unknown;
			}
		}
		if (flag && assemblyContentType == AssemblyContentType.WindowsRuntime)
		{
			return false;
		}
		bool flag2 = !immutableArray.IsDefault;
		bool flag3 = !immutableArray2.IsDefault;
		identity = new AssemblyIdentity(value, version, cultureName, flag2 ? immutableArray : immutableArray2, flag2, flag, assemblyContentType);
		if ((flag2 & flag3) && !identity.PublicKeyToken.SequenceEqual(immutableArray2))
		{
			identity = null;
			return false;
		}
		parts = assemblyIdentityParts;
		return true;
	}

	private static bool TryParseNameToken(string displayName, ref int position, out string value)
	{
		int i = position;
		while (true)
		{
			if (i == displayName.Length)
			{
				value = null;
				return false;
			}
			if (!IsWhiteSpace(displayName[i]))
			{
				break;
			}
			i++;
		}
		char c = (IsQuote(displayName[i]) ? displayName[i++] : '\0');
		int num = i;
		int num2 = displayName.Length;
		bool flag = false;
		while (true)
		{
			if (i >= displayName.Length)
			{
				i = displayName.Length;
				break;
			}
			char c2 = displayName[i];
			if (c2 == '\\')
			{
				flag = true;
				i += 2;
				continue;
			}
			if (c == '\0')
			{
				if (IsNameTokenTerminator(c2))
				{
					break;
				}
				if (IsQuote(c2))
				{
					value = null;
					return false;
				}
			}
			else if (c2 == c)
			{
				num2 = i;
				i++;
				break;
			}
			i++;
		}
		if (c == '\0')
		{
			int num3 = i - 1;
			while (num3 >= num && IsWhiteSpace(displayName[num3]))
			{
				num3--;
			}
			num2 = num3 + 1;
		}
		else
		{
			for (; i < displayName.Length; i++)
			{
				char c3 = displayName[i];
				if (!IsWhiteSpace(c3))
				{
					if (IsNameTokenTerminator(c3))
					{
						break;
					}
					value = null;
					return false;
				}
			}
		}
		position = i;
		if (num2 == num)
		{
			value = null;
			return false;
		}
		if (!flag)
		{
			value = displayName.Substring(num, num2 - num);
			return true;
		}
		return TryUnescape(displayName, num, num2, out value);
	}

	private static bool IsNameTokenTerminator(char c)
	{
		if (c != '=')
		{
			return c == ',';
		}
		return true;
	}

	private static bool IsQuote(char c)
	{
		if (c != '"')
		{
			return c == '\'';
		}
		return true;
	}

	internal static Version ToVersion(ulong version)
	{
		return new Version((ushort)(version >> 48), (ushort)(version >> 32), (ushort)(version >> 16), (ushort)version);
	}

	internal static bool TryParseVersion(string str, out ulong result, out AssemblyIdentityParts parts)
	{
		parts = (AssemblyIdentityParts)0;
		result = 0uL;
		int num = 48;
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		bool flag2 = false;
		int num4 = 0;
		while (true)
		{
			char c = ((num4 < str.Length) ? str[num4++] : '\0');
			switch (c)
			{
			case '\0':
			case '.':
				if (num2 == 4 || (flag & flag2))
				{
					return false;
				}
				result |= (ulong)((long)num3 << num);
				if (flag | flag2)
				{
					parts |= (AssemblyIdentityParts)(2 << num2);
				}
				if (c == '\0')
				{
					return true;
				}
				num3 = 0;
				num -= 16;
				num2++;
				flag2 = (flag = false);
				break;
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				flag = true;
				num3 = num3 * 10 + c - 48;
				if (num3 > 65535)
				{
					return false;
				}
				break;
			default:
				if (c == '*')
				{
					flag2 = true;
					break;
				}
				return false;
			}
		}
	}

	private static bool TryParsePublicKey(string value, out ImmutableArray<byte> key)
	{
		if (!TryParseHexBytes(value, out key) || !MetadataHelpers.IsValidPublicKey(key))
		{
			key = default;
			return false;
		}
		return true;
	}

	private static bool TryParsePublicKeyToken(string value, out ImmutableArray<byte> token)
	{
		if (string.Equals(value, "null", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "neutral", StringComparison.OrdinalIgnoreCase))
		{
			token = ImmutableArray<byte>.Empty;
			return true;
		}
		if (value.Length != 16 || !TryParseHexBytes(value, out var result))
		{
			token = default;
			return false;
		}
		token = result;
		return true;
	}

	private static bool TryParseHexBytes(string value, out ImmutableArray<byte> result)
	{
		if (value.Length == 0 || value.Length % 2 != 0)
		{
			result = default;
			return false;
		}
		int num = value.Length / 2;
		ArrayBuilder<byte> instance = ArrayBuilder<byte>.GetInstance(num);
		for (int i = 0; i < num; i++)
		{
			int num2 = HexValue(value[i * 2]);
			int num3 = HexValue(value[i * 2 + 1]);
			if (num2 < 0 || num3 < 0)
			{
				result = default;
				instance.Free();
				return false;
			}
			instance.Add((byte)((num2 << 4) | num3));
		}
		result = instance.ToImmutableAndFree();
		return true;
	}

	internal static int HexValue(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return c - 48;
		}
		if (c >= 'a' && c <= 'f')
		{
			return c - 97 + 10;
		}
		if (c >= 'A' && c <= 'F')
		{
			return c - 65 + 10;
		}
		return -1;
	}

	private static bool IsWhiteSpace(char c)
	{
		if (c != ' ' && c != '\t' && c != '\r')
		{
			return c == '\n';
		}
		return true;
	}

	private static void EscapeName(StringBuilder result, string name)
	{
		bool flag = false;
		if (IsWhiteSpace(name[0]) || IsWhiteSpace(name[name.Length - 1]))
		{
			result.Append('"');
			flag = true;
		}
		foreach (char c in name)
		{
			switch (c)
			{
			case '"':
			case '\'':
			case ',':
			case '=':
			case '\\':
				result.Append('\\');
				result.Append(c);
				break;
			case '\t':
				result.Append("\\t");
				break;
			case '\r':
				result.Append("\\r");
				break;
			case '\n':
				result.Append("\\n");
				break;
			default:
				result.Append(c);
				break;
			}
		}
		if (flag)
		{
			result.Append('"');
		}
	}

	private static bool TryUnescape(string str, int start, int end, out string value)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		int i = start;
		while (i < end)
		{
			char c = str[i++];
			if (c == '\\')
			{
				if (!Unescape(instance.Builder, str, ref i))
				{
					value = null;
					return false;
				}
			}
			else
			{
				instance.Builder.Append(c);
			}
		}
		value = instance.ToStringAndFree();
		return true;
	}

	private static bool Unescape(StringBuilder sb, string str, ref int i)
	{
		if (i == str.Length)
		{
			return false;
		}
		char c = str[i++];
		switch (c)
		{
		case '"':
		case '\'':
		case ',':
		case '/':
		case '=':
		case '\\':
			sb.Append(c);
			return true;
		case 't':
			sb.Append("\t");
			return true;
		case 'n':
			sb.Append("\n");
			return true;
		case 'r':
			sb.Append("\r");
			return true;
		case 'u':
		{
			int num = str.IndexOf(';', i);
			if (num == -1)
			{
				return false;
			}
			try
			{
				int num2 = Convert.ToInt32(str.Substring(i, num - i), 16);
				if (num2 == 0)
				{
					return false;
				}
				sb.Append(char.ConvertFromUtf32(num2));
			}
			catch
			{
				return false;
			}
			i = num + 1;
			return true;
		}
		default:
			return false;
		}
	}
}
