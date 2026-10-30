using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal static class MetadataHelpers
{
	internal struct AssemblyQualifiedTypeName
	{
		internal readonly string TopLevelType;

		internal readonly string[] NestedTypes;

		internal readonly AssemblyQualifiedTypeName[] TypeArguments;

		internal readonly int PointerCount;

		internal readonly int[] ArrayRanks;

		internal readonly string AssemblyName;

		internal AssemblyQualifiedTypeName(string topLevelType, string[] nestedTypes, AssemblyQualifiedTypeName[] typeArguments, int pointerCount, int[] arrayRanks, string assemblyName)
		{
			TopLevelType = topLevelType;
			NestedTypes = nestedTypes;
			TypeArguments = typeArguments;
			PointerCount = pointerCount;
			ArrayRanks = arrayRanks;
			AssemblyName = assemblyName;
		}
	}

	private struct SerializedTypeDecoder
	{
		private static readonly char[] s_typeNameDelimiters = new char[5] { '+', ',', '[', ']', '*' };

		private readonly string _input;

		private int _offset;

		private bool EndOfInput => _offset >= _input.Length;

		private char Current => _input[_offset];

		internal SerializedTypeDecoder(string s)
		{
			_input = s;
			_offset = 0;
		}

		private void Advance()
		{
			if (!EndOfInput)
			{
				_offset++;
			}
		}

		private void AdvanceTo(int i)
		{
			if (i <= _input.Length)
			{
				_offset = i;
			}
		}

		internal AssemblyQualifiedTypeName DecodeTypeName(bool isTypeArgument = false, bool isTypeArgumentWithAssemblyName = false)
		{
			DebugAssertHelper.Assert(!isTypeArgumentWithAssemblyName | isTypeArgument);
			string topLevelType = null;
			ArrayBuilder<string> nestedTypesBuilder = null;
			AssemblyQualifiedTypeName[] array = null;
			int num = 0;
			ArrayBuilder<int> arrayRanksBuilder = null;
			string assemblyName = null;
			bool decodingTopLevelType = true;
			bool flag = false;
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			StringBuilder builder = instance.Builder;
			while (!EndOfInput)
			{
				int num2 = _input.IndexOfAny(s_typeNameDelimiters, _offset);
				if (num2 >= 0)
				{
					char c = _input[num2];
					string text = DecodeGenericName(num2);
					DebugAssertHelper.Assert(text != null);
					flag = flag || text.IndexOf('`') >= 0;
					builder.Append(text);
					switch (c)
					{
					case '*':
						if (arrayRanksBuilder != null)
						{
							builder.Append(c);
						}
						else
						{
							num++;
						}
						Advance();
						continue;
					case '+':
						if (arrayRanksBuilder != null || num > 0)
						{
							builder.Append(c);
						}
						else
						{
							HandleDecodedTypeName(builder.ToString(), decodingTopLevelType, ref topLevelType, ref nestedTypesBuilder);
							builder.Clear();
							decodingTopLevelType = false;
						}
						Advance();
						continue;
					case '[':
						if (flag && array == null)
						{
							Advance();
							if (arrayRanksBuilder != null || num > 0)
							{
								builder.Append(c);
							}
							else
							{
								array = DecodeTypeArguments();
							}
						}
						else
						{
							DecodeArrayShape(builder, ref arrayRanksBuilder);
						}
						continue;
					case ']':
						if (!isTypeArgument)
						{
							builder.Append(c);
							Advance();
							continue;
						}
						break;
					case ',':
						if (!isTypeArgument | isTypeArgumentWithAssemblyName)
						{
							Advance();
							if (!EndOfInput && char.IsWhiteSpace(Current))
							{
								Advance();
							}
							assemblyName = DecodeAssemblyName(isTypeArgumentWithAssemblyName);
						}
						break;
					default:
						throw ExceptionUtilities.UnexpectedValue(c);
					}
				}
				else
				{
					builder.Append(DecodeGenericName(_input.Length));
				}
				break;
			}
			HandleDecodedTypeName(builder.ToString(), decodingTopLevelType, ref topLevelType, ref nestedTypesBuilder);
			instance.Free();
			return new AssemblyQualifiedTypeName(topLevelType, nestedTypesBuilder?.ToArrayAndFree(), array, num, arrayRanksBuilder?.ToArrayAndFree(), assemblyName);
		}

		private static void HandleDecodedTypeName(string decodedTypeName, bool decodingTopLevelType, ref string topLevelType, ref ArrayBuilder<string> nestedTypesBuilder)
		{
			if (decodedTypeName.Length == 0)
			{
				return;
			}
			if (decodingTopLevelType)
			{
				DebugAssertHelper.Assert(topLevelType == null);
				topLevelType = decodedTypeName;
				return;
			}
			if (nestedTypesBuilder == null)
			{
				nestedTypesBuilder = ArrayBuilder<string>.GetInstance();
			}
			nestedTypesBuilder.Add(decodedTypeName);
		}

		private string DecodeGenericName(int i)
		{
			DebugAssertHelper.Assert(i == _input.Length || s_typeNameDelimiters.Contains(_input[i]));
			if (i - _offset == 0)
			{
				return string.Empty;
			}
			int offset = _offset;
			AdvanceTo(i);
			return _input.Substring(offset, _offset - offset);
		}

		private AssemblyQualifiedTypeName[] DecodeTypeArguments()
		{
			if (EndOfInput)
			{
				return null;
			}
			ArrayBuilder<AssemblyQualifiedTypeName> instance = ArrayBuilder<AssemblyQualifiedTypeName>.GetInstance();
			while (!EndOfInput)
			{
				instance.Add(DecodeTypeArgument());
				if (EndOfInput)
				{
					continue;
				}
				switch (Current)
				{
				case ',':
					Advance();
					if (!EndOfInput && char.IsWhiteSpace(Current))
					{
						Advance();
					}
					break;
				case ']':
					Advance();
					return instance.ToArrayAndFree();
				default:
					throw ExceptionUtilities.UnexpectedValue(EndOfInput);
				}
			}
			return instance.ToArrayAndFree();
		}

		private AssemblyQualifiedTypeName DecodeTypeArgument()
		{
			bool flag = false;
			if (Current == '[')
			{
				flag = true;
				Advance();
			}
			AssemblyQualifiedTypeName result = DecodeTypeName(isTypeArgument: true, flag);
			if (flag && !EndOfInput && Current == ']')
			{
				Advance();
			}
			return result;
		}

		private string DecodeAssemblyName(bool isTypeArgumentWithAssemblyName)
		{
			if (EndOfInput)
			{
				return null;
			}
			int num;
			if (isTypeArgumentWithAssemblyName)
			{
				num = _input.IndexOf(']', _offset);
				if (num < 0)
				{
					num = _input.Length;
				}
			}
			else
			{
				num = _input.Length;
			}
			string result = _input.Substring(_offset, num - _offset);
			AdvanceTo(num);
			return result;
		}

		private void DecodeArrayShape(StringBuilder typeNameBuilder, ref ArrayBuilder<int> arrayRanksBuilder)
		{
			DebugAssertHelper.Assert(Current == '[');
			int offset = _offset;
			int num = 1;
			Advance();
			while (!EndOfInput)
			{
				switch (Current)
				{
				case ',':
					break;
				case ']':
					if (arrayRanksBuilder == null)
					{
						arrayRanksBuilder = ArrayBuilder<int>.GetInstance();
					}
					arrayRanksBuilder.Add(num);
					Advance();
					return;
				default:
					Advance();
					typeNameBuilder.Append(_input.Substring(offset, _offset - offset));
					return;
				}
				num++;
				Advance();
			}
			typeNameBuilder.Append(_input.Substring(offset, _offset - offset));
		}
	}

	private static class PublicKeyDecoder
	{
		private enum AlgorithmClass
		{
			Signature = 1,
			Hash = 4
		}

		private enum AlgorithmSubId
		{
			Sha1Hash = 4,
			MacHash,
			RipeMdHash,
			RipeMd160Hash,
			Ssl3ShaMD5Hash,
			HmacHash,
			Tls1PrfHash,
			HashReplacOwfHash,
			Sha256Hash,
			Sha384Hash,
			Sha512Hash
		}

		private struct AlgorithmId
		{
			private const int AlgorithmClassOffset = 13;

			private const int AlgorithmClassMask = 7;

			private const int AlgorithmSubIdOffset = 0;

			private const int AlgorithmSubIdMask = 511;

			private readonly uint _flags;

			public bool IsSet => _flags != 0;

			public AlgorithmClass Class => (AlgorithmClass)((_flags >> 13) & 7);

			public AlgorithmSubId SubId => (AlgorithmSubId)(_flags & 0x1FF);

			public AlgorithmId(uint flags)
			{
				_flags = flags;
			}
		}

		private static readonly ImmutableArray<byte> s_ecmaKey = ImmutableArray.Create(new byte[16]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 4, 0,
			0, 0, 0, 0, 0, 0
		});

		private const int SigAlgIdOffset = 0;

		private const int HashAlgIdOffset = 4;

		private const int PublicKeySizeOffset = 8;

		private const int PublicKeyDataOffset = 12;

		private const int HeaderSize = 12;

		private const byte PublicKeyBlob = 6;

		private static uint ToUInt32(ImmutableArray<byte> bytes, int offset)
		{
			DebugAssertHelper.Assert(bytes.Length - offset > 4);
			return (uint)(bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24));
		}

		public static bool TryDecode(ImmutableArray<byte> bytes)
		{
			if (bytes.IsDefault || bytes.Length < 13)
			{
				return false;
			}
			uint num = ToUInt32(bytes, 8);
			if (bytes.Length != 12 + num)
			{
				return false;
			}
			if (ByteSequenceComparer.Equals(bytes, s_ecmaKey))
			{
				return true;
			}
			AlgorithmId algorithmId = new AlgorithmId(ToUInt32(bytes, 0));
			if (algorithmId.IsSet && algorithmId.Class != AlgorithmClass.Signature)
			{
				return false;
			}
			AlgorithmId algorithmId2 = new AlgorithmId(ToUInt32(bytes, 4));
			if (algorithmId2.IsSet && (algorithmId2.Class != AlgorithmClass.Hash || algorithmId2.SubId < AlgorithmSubId.Sha1Hash))
			{
				return false;
			}
			if (bytes[12] != 6)
			{
				return false;
			}
			return true;
		}
	}

	public const char DotDelimiter = '.';

	public const string DotDelimiterString = ".";

	public const char GenericTypeNameManglingChar = '`';

	private const string GenericTypeNameManglingString = "`";

	public const int MaxStringLengthForParamSize = 22;

	public const int MaxStringLengthForIntToStringConversion = 22;

	public const string SystemString = "System";

	public const char MangledNameRegionStartChar = '<';

	public const char MangledNameRegionEndChar = '>';

	private static readonly string[] s_aritySuffixesOneToNine = new string[9] { "`1", "`2", "`3", "`4", "`5", "`6", "`7", "`8", "`9" };

	private static readonly ImmutableArray<string> s_splitQualifiedNameSystem = ImmutableArray.Create("System");

	internal static AssemblyQualifiedTypeName DecodeTypeName(string s)
	{
		return new SerializedTypeDecoder(s).DecodeTypeName();
	}

	internal static string GetAritySuffix(int arity)
	{
		DebugAssertHelper.Assert(arity > 0);
		if (arity > 9)
		{
			return "`" + arity.ToString(CultureInfo.InvariantCulture);
		}
		return s_aritySuffixesOneToNine[arity - 1];
	}

	internal static string ComposeAritySuffixedMetadataName(string name, int arity)
	{
		if (arity != 0)
		{
			return name + GetAritySuffix(arity);
		}
		return name;
	}

	internal static int InferTypeArityFromMetadataName(string emittedTypeName)
	{
		int suffixStartsAt;
		return InferTypeArityFromMetadataName(emittedTypeName, out suffixStartsAt);
	}

	private static short InferTypeArityFromMetadataName(string emittedTypeName, out int suffixStartsAt)
	{
		DebugAssertHelper.Assert(emittedTypeName != null, "NULL actual name unexpected!!!");
		int length = emittedTypeName.Length;
		int num = length;
		while (num >= 1 && emittedTypeName[num - 1] != '`')
		{
			num--;
		}
		if (num < 2 || length - num == 0 || length - num > 22)
		{
			suffixStartsAt = -1;
			return 0;
		}
		string text = emittedTypeName.Substring(num);
		if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || result < 0 || result > 32767 || text != result.ToString(CultureInfo.InvariantCulture))
		{
			suffixStartsAt = -1;
			return 0;
		}
		suffixStartsAt = num - 1;
		return (short)result;
	}

	internal static string InferTypeArityAndUnmangleMetadataName(string emittedTypeName, out short arity)
	{
		arity = InferTypeArityFromMetadataName(emittedTypeName, out var suffixStartsAt);
		if (arity == 0)
		{
			DebugAssertHelper.Assert(suffixStartsAt == -1);
			return emittedTypeName;
		}
		DebugAssertHelper.Assert(suffixStartsAt > 0 && suffixStartsAt < emittedTypeName.Length - 1);
		return emittedTypeName.Substring(0, suffixStartsAt);
	}

	internal static string UnmangleMetadataNameForArity(string emittedTypeName, int arity)
	{
		DebugAssertHelper.Assert(arity > 0);
		if (arity == InferTypeArityFromMetadataName(emittedTypeName, out var suffixStartsAt))
		{
			DebugAssertHelper.Assert(suffixStartsAt > 0 && suffixStartsAt < emittedTypeName.Length - 1);
			return emittedTypeName.Substring(0, suffixStartsAt);
		}
		return emittedTypeName;
	}

	internal static ImmutableArray<string> SplitQualifiedName(string name)
	{
		DebugAssertHelper.Assert(name != null);
		if (name.Length == 0)
		{
			return ImmutableArray<string>.Empty;
		}
		int num = 0;
		for (int i = 0; i < name.Length; i++)
		{
			if (name[i] == '.')
			{
				num++;
			}
		}
		if (num == 0)
		{
			if (!(name == "System"))
			{
				return ImmutableArray.Create(name);
			}
			return s_splitQualifiedNameSystem;
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance(num + 1);
		int num2 = 0;
		int num3 = 0;
		while (num > 0)
		{
			if (name[num3] == '.')
			{
				int num4 = num3 - num2;
				if (num4 == 6 && num2 == 0 && name.StartsWith("System", StringComparison.Ordinal))
				{
					instance.Add("System");
				}
				else
				{
					instance.Add(name.Substring(num2, num4));
				}
				num--;
				num2 = num3 + 1;
			}
			num3++;
		}
		instance.Add(name.Substring(num2));
		return instance.ToImmutableAndFree();
	}

	internal static string SplitQualifiedName(string pstrName, out string qualifier)
	{
		DebugAssertHelper.Assert(pstrName != null);
		int num = 0;
		int num2 = -1;
		for (int i = 0; i < pstrName.Length; i++)
		{
			switch (pstrName[i])
			{
			case '<':
				num++;
				break;
			case '>':
				num--;
				break;
			case '.':
				if (num == 0 && (i == 0 || num2 < i - 1))
				{
					num2 = i;
				}
				break;
			}
		}
		DebugAssertHelper.Assert(num == 0);
		if (num2 < 0)
		{
			qualifier = string.Empty;
			return pstrName;
		}
		if (num2 == 6 && pstrName.StartsWith("System", StringComparison.Ordinal))
		{
			qualifier = "System";
		}
		else
		{
			qualifier = pstrName.Substring(0, num2);
		}
		return pstrName.Substring(num2 + 1);
	}

	internal static string BuildQualifiedName(string qualifier, string name)
	{
		DebugAssertHelper.Assert(name != null);
		if (!string.IsNullOrEmpty(qualifier))
		{
			return qualifier + "." + name;
		}
		return name;
	}

	internal static bool IsValidMetadataIdentifier(string str)
	{
		if (!string.IsNullOrEmpty(str) && str.IsValidUnicodeString())
		{
			return str.IndexOf('\0') == -1;
		}
		return false;
	}

	internal static bool IsValidUnicodeString(string str)
	{
		return str?.IsValidUnicodeString() ?? true;
	}

	internal static void ValidateAssemblyOrModuleName(string name, string argumentName)
	{
		Exception ex = CheckAssemblyOrModuleName(name, argumentName);
		if (ex != null)
		{
			throw ex;
		}
	}

	internal static bool IsValidAssemblyOrModuleName(string name)
	{
		return CheckAssemblyOrModuleName(name, null) == null;
	}

	internal static Exception CheckAssemblyOrModuleName(string name, string argumentName)
	{
		if (name == null)
		{
			return new ArgumentNullException(argumentName);
		}
		if (name.Length == 0)
		{
			return new ArgumentException(CodeAnalysisResources.NameCannotBeEmpty, argumentName);
		}
		if (char.IsWhiteSpace(name[0]))
		{
			return new ArgumentException(CodeAnalysisResources.NameCannotStartWithWhitespace, argumentName);
		}
		if (!IsValidMetadataFileName(name))
		{
			return new ArgumentException(CodeAnalysisResources.NameContainsInvalidCharacter, argumentName);
		}
		return null;
	}

	internal static bool IsValidMetadataFileName(string name)
	{
		if (FileNameUtilities.IsFileName(name))
		{
			return IsValidMetadataIdentifier(name);
		}
		return false;
	}

	internal static bool IsValidPublicKey(ImmutableArray<byte> bytes)
	{
		return PublicKeyDecoder.TryDecode(bytes);
	}

	internal static string MangleForTypeNameIfNeeded(string moduleName)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append(moduleName);
		builder.Replace("Q", "QQ");
		builder.Replace("_", "Q_");
		builder.Replace('.', '_');
		return instance.ToStringAndFree();
	}
}
