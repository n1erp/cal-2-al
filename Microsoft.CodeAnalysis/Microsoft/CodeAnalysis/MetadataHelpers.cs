using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

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

		private int Offset => _offset;

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
			int offset = _offset;
			int num = 1;
			bool flag = false;
			Advance();
			while (!EndOfInput)
			{
				switch (Current)
				{
				case ',':
					num++;
					Advance();
					continue;
				case ']':
					if (arrayRanksBuilder == null)
					{
						arrayRanksBuilder = ArrayBuilder<int>.GetInstance();
					}
					arrayRanksBuilder.Add((num != 1 || flag) ? num : 0);
					Advance();
					return;
				case '*':
					if (num == 1)
					{
						Advance();
						if (Current != ']')
						{
							typeNameBuilder.Append(_input.Substring(offset, _offset - offset));
							return;
						}
						flag = true;
						continue;
					}
					break;
				}
				Advance();
				typeNameBuilder.Append(_input.Substring(offset, _offset - offset));
				return;
			}
			typeNameBuilder.Append(_input.Substring(offset, _offset - offset));
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
		if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || result < 0 || result > 32767 || text != result.ToString())
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
			return emittedTypeName;
		}
		return emittedTypeName.Substring(0, suffixStartsAt);
	}

	internal static string UnmangleMetadataNameForArity(string emittedTypeName, int arity)
	{
		if (arity == InferTypeArityFromMetadataName(emittedTypeName, out var suffixStartsAt))
		{
			return emittedTypeName.Substring(0, suffixStartsAt);
		}
		return emittedTypeName;
	}

	internal static ImmutableArray<string> SplitQualifiedName(string name)
	{
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
		if (!string.IsNullOrEmpty(qualifier))
		{
			return qualifier + "." + name;
		}
		return name;
	}

	public static void GetInfoForImmediateNamespaceMembers(bool isGlobalNamespace, int namespaceNameLength, IEnumerable<IGrouping<string, TypeDefinitionHandle>> typesByNS, StringComparer nameComparer, out IEnumerable<IGrouping<string, TypeDefinitionHandle>> types, out IEnumerable<KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>>> namespaces)
	{
		List<IGrouping<string, TypeDefinitionHandle>> list = new List<IGrouping<string, TypeDefinitionHandle>>();
		List<KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>>> list2 = new List<KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>>>();
		bool flag = false;
		IEnumerator<IGrouping<string, TypeDefinitionHandle>> enumerator = typesByNS.GetEnumerator();
		using (enumerator)
		{
			if (enumerator.MoveNext())
			{
				IGrouping<string, TypeDefinitionHandle> current = enumerator.Current;
				string text = null;
				List<IGrouping<string, TypeDefinitionHandle>> list3 = null;
				while (true)
				{
					if (current.Key.Length == namespaceNameLength)
					{
						list.Add(current);
						if (!enumerator.MoveNext())
						{
							break;
						}
						current = enumerator.Current;
						continue;
					}
					if (!isGlobalNamespace)
					{
						namespaceNameLength++;
					}
					do
					{
						current = enumerator.Current;
						string text2 = ExtractSimpleNameOfChildNamespace(namespaceNameLength, current.Key);
						int num = nameComparer.Compare(text, text2);
						if (num == 0)
						{
							list3.Add(current);
							continue;
						}
						if (num > 0)
						{
							flag = true;
						}
						if (list3 != null)
						{
							list2.Add(new KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>>(text, list3));
						}
						list3 = new List<IGrouping<string, TypeDefinitionHandle>>();
						text = text2;
						list3.Add(current);
					}
					while (enumerator.MoveNext());
					if (list3 != null)
					{
						list2.Add(new KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>>(text, list3));
					}
					break;
				}
			}
		}
		types = list;
		if (flag)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>(list2.Count, nameComparer);
			for (int num2 = list2.Count - 1; num2 >= 0; num2--)
			{
				dictionary[list2[num2].Key] = num2;
			}
			if (dictionary.Count != list2.Count)
			{
				for (int i = 1; i < list2.Count; i++)
				{
					KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>> keyValuePair = list2[i];
					int num3 = dictionary[keyValuePair.Key];
					if (num3 != i)
					{
						KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>> keyValuePair2 = list2[num3];
						list2[num3] = KeyValuePairUtil.Create(keyValuePair2.Key, keyValuePair2.Value.Concat(keyValuePair.Value));
						list2[i] = default;
					}
				}
				list2.RemoveAll((KeyValuePair<string, IEnumerable<IGrouping<string, TypeDefinitionHandle>>> pair) => pair.Key == null);
			}
		}
		namespaces = list2;
	}

	private static string ExtractSimpleNameOfChildNamespace(int parentNamespaceNameLength, string fullName)
	{
		int num = fullName.IndexOf('.', parentNamespaceNameLength);
		if (num < 0)
		{
			return fullName.Substring(parentNamespaceNameLength);
		}
		return fullName.Substring(parentNamespaceNameLength, num - parentNamespaceNameLength);
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

	internal static bool IsValidAssemblyOrModuleName(string name)
	{
		return GetAssemblyOrModuleNameErrorArgumentResourceName(name) == null;
	}

	internal static void CheckAssemblyOrModuleName(string name, CommonMessageProvider messageProvider, int code, DiagnosticBag diagnostics)
	{
		string assemblyOrModuleNameErrorArgumentResourceName = GetAssemblyOrModuleNameErrorArgumentResourceName(name);
		if (assemblyOrModuleNameErrorArgumentResourceName != null)
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(code, Location.None, new CodeAnalysisResourcesLocalizableErrorArgument(assemblyOrModuleNameErrorArgumentResourceName)));
		}
	}

	internal static void CheckAssemblyOrModuleName(string name, CommonMessageProvider messageProvider, int code, ArrayBuilder<Diagnostic> builder)
	{
		string assemblyOrModuleNameErrorArgumentResourceName = GetAssemblyOrModuleNameErrorArgumentResourceName(name);
		if (assemblyOrModuleNameErrorArgumentResourceName != null)
		{
			builder.Add(messageProvider.CreateDiagnostic(code, Location.None, new CodeAnalysisResourcesLocalizableErrorArgument(assemblyOrModuleNameErrorArgumentResourceName)));
		}
	}

	private static string GetAssemblyOrModuleNameErrorArgumentResourceName(string name)
	{
		if (name == null)
		{
			return "NameCannotBeNull";
		}
		if (name.Length == 0)
		{
			return "NameCannotBeEmpty";
		}
		if (char.IsWhiteSpace(name[0]))
		{
			return "NameCannotStartWithWhitespace";
		}
		if (!IsValidMetadataFileName(name))
		{
			return "NameContainsInvalidCharacter";
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

	internal static bool SplitNameEqualsFullyQualifiedName(string namespaceName, string typeName, string fullyQualified)
	{
		if (fullyQualified.Length == namespaceName.Length + typeName.Length + 1 && fullyQualified[namespaceName.Length] == '.' && fullyQualified.StartsWith(namespaceName, StringComparison.Ordinal))
		{
			return fullyQualified.EndsWith(typeName, StringComparison.Ordinal);
		}
		return false;
	}

	internal static bool IsValidPublicKey(ImmutableArray<byte> bytes)
	{
		return CryptoBlobParser.IsValidPublicKey(bytes);
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
