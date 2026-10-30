using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Debugging;

internal static class CustomDebugInfoReader
{
	private static void ReadGlobalHeader(byte[] bytes, ref int offset, out byte version, out byte count)
	{
		version = bytes[offset];
		count = bytes[offset + 1];
		offset += 4;
	}

	private static void ReadRecordHeader(byte[] bytes, ref int offset, out byte version, out CustomDebugInfoKind kind, out int size, out int alignmentSize)
	{
		version = bytes[offset];
		kind = (CustomDebugInfoKind)bytes[offset + 1];
		alignmentSize = bytes[offset + 3];
		size = BitConverter.ToInt32(bytes, offset + 4);
		offset += 8;
	}

	public static ImmutableArray<byte> TryGetCustomDebugInfoRecord(byte[] customDebugInfo, CustomDebugInfoKind recordKind)
	{
		foreach (CustomDebugInfoRecord customDebugInfoRecord in GetCustomDebugInfoRecords(customDebugInfo))
		{
			if (customDebugInfoRecord.Kind == recordKind)
			{
				return customDebugInfoRecord.Data;
			}
		}
		return default;
	}

	public static IEnumerable<CustomDebugInfoRecord> GetCustomDebugInfoRecords(byte[] customDebugInfo)
	{
		if (customDebugInfo.Length < 4)
		{
			throw new InvalidOperationException("Invalid header.");
		}
		int offset = 0;
		ReadGlobalHeader(customDebugInfo, ref offset, out var version, out var _);
		if (version != 4)
		{
			yield break;
		}
		int bodySize;
		for (; offset <= customDebugInfo.Length - 8; offset += bodySize)
		{
			ReadRecordHeader(customDebugInfo, ref offset, out var version2, out var kind, out var size, out var alignmentSize);
			if (size < 8)
			{
				throw new InvalidOperationException("Invalid header.");
			}
			if (kind - 6 > CustomDebugInfoKind.ForwardModuleInfo)
			{
				alignmentSize = 0;
			}
			bodySize = size - 8;
			if (offset > customDebugInfo.Length - bodySize || alignmentSize > 3 || alignmentSize > bodySize)
			{
				throw new InvalidOperationException("Invalid header.");
			}
			yield return new CustomDebugInfoRecord(kind, version2, ImmutableArray.Create(customDebugInfo, offset, bodySize - alignmentSize));
		}
	}

	public static ImmutableArray<short> DecodeUsingRecord(ImmutableArray<byte> bytes)
	{
		int offset = 0;
		short num = ReadInt16(bytes, ref offset);
		ArrayBuilder<short> instance = ArrayBuilder<short>.GetInstance(num);
		for (int i = 0; i < num; i++)
		{
			instance.Add(ReadInt16(bytes, ref offset));
		}
		return instance.ToImmutableAndFree();
	}

	public static int DecodeForwardRecord(ImmutableArray<byte> bytes)
	{
		int offset = 0;
		return ReadInt32(bytes, ref offset);
	}

	public static int DecodeForwardToModuleRecord(ImmutableArray<byte> bytes)
	{
		int offset = 0;
		return ReadInt32(bytes, ref offset);
	}

	public static ImmutableArray<StateMachineHoistedLocalScope> DecodeStateMachineHoistedLocalScopesRecord(ImmutableArray<byte> bytes)
	{
		int offset = 0;
		int num = ReadInt32(bytes, ref offset);
		ArrayBuilder<StateMachineHoistedLocalScope> instance = ArrayBuilder<StateMachineHoistedLocalScope>.GetInstance(num);
		for (int i = 0; i < num; i++)
		{
			int num2 = ReadInt32(bytes, ref offset);
			int num3 = ReadInt32(bytes, ref offset);
			if (num2 != 0 || num3 != 0)
			{
				num3++;
			}
			instance.Add(new StateMachineHoistedLocalScope(num2, num3));
		}
		return instance.ToImmutableAndFree();
	}

	public static string DecodeForwardIteratorRecord(ImmutableArray<byte> bytes)
	{
		int offset = 0;
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		while (true)
		{
			char c = (char)ReadInt16(bytes, ref offset);
			if (c == '\0')
			{
				break;
			}
			builder.Append(c);
		}
		return instance.ToStringAndFree();
	}

	public static ImmutableArray<DynamicLocalInfo> DecodeDynamicLocalsRecord(ImmutableArray<byte> bytes)
	{
		ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance(64);
		PooledStringBuilder instance2 = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance2.Builder;
		int offset = 0;
		int num = ReadInt32(bytes, ref offset);
		ArrayBuilder<DynamicLocalInfo> instance3 = ArrayBuilder<DynamicLocalInfo>.GetInstance(num);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < 64; j++)
			{
				instance.Add(ReadByte(bytes, ref offset) != 0);
			}
			int num2 = ReadInt32(bytes, ref offset);
			if (num2 < instance.Count)
			{
				instance.Count = num2;
			}
			int slotId = ReadInt32(bytes, ref offset);
			int num3 = offset + 128;
			while (offset < num3)
			{
				char c = (char)ReadInt16(bytes, ref offset);
				if (c == '\0')
				{
					offset = num3;
					break;
				}
				builder.Append(c);
			}
			instance3.Add(new DynamicLocalInfo(instance.ToImmutable(), slotId, builder.ToString()));
			instance.Clear();
			builder.Clear();
		}
		instance.Free();
		instance2.Free();
		return instance3.ToImmutableAndFree();
	}

	public static ImmutableArray<TupleElementNamesInfo> DecodeTupleElementNamesRecord(ImmutableArray<byte> bytes)
	{
		int offset = 0;
		int num = ReadInt32(bytes, ref offset);
		ArrayBuilder<TupleElementNamesInfo> instance = ArrayBuilder<TupleElementNamesInfo>.GetInstance(num);
		for (int i = 0; i < num; i++)
		{
			instance.Add(DecodeTupleElementNamesInfo(bytes, ref offset));
		}
		return instance.ToImmutableAndFree();
	}

	private static TupleElementNamesInfo DecodeTupleElementNamesInfo(ImmutableArray<byte> bytes, ref int offset)
	{
		int num = ReadInt32(bytes, ref offset);
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance(num);
		for (int i = 0; i < num; i++)
		{
			string text = ReadUtf8String(bytes, ref offset);
			instance.Add(string.IsNullOrEmpty(text) ? null : text);
		}
		int slotIndex = ReadInt32(bytes, ref offset);
		int scopeStart = ReadInt32(bytes, ref offset);
		int scopeEnd = ReadInt32(bytes, ref offset);
		string localName = ReadUtf8String(bytes, ref offset);
		return new TupleElementNamesInfo(instance.ToImmutableAndFree(), slotIndex, localName, scopeStart, scopeEnd);
	}

	private static void ReadRawRecordBody(byte[] bytes, ref int offset, int size, out ImmutableArray<byte> body)
	{
		int num = size - 8;
		body = ImmutableArray.Create(bytes, offset, num);
		offset += num;
	}

	private static void SkipRecord(byte[] bytes, ref int offset, int size)
	{
		offset += size - 8;
	}

	public static ImmutableArray<ImmutableArray<string>> GetCSharpGroupedImportStrings<TArg>(int methodToken, TArg arg, Func<int, TArg, byte[]> getMethodCustomDebugInfo, Func<int, TArg, ImmutableArray<string>> getMethodImportStrings, out ImmutableArray<string> externAliasStrings)
	{
		externAliasStrings = default;
		ImmutableArray<short> immutableArray = default;
		bool flag = false;
		while (true)
		{
			IL_0012:
			byte[] array = getMethodCustomDebugInfo(methodToken, arg);
			if (array == null)
			{
				return default;
			}
			foreach (CustomDebugInfoRecord customDebugInfoRecord in GetCustomDebugInfoRecords(array))
			{
				switch (customDebugInfoRecord.Kind)
				{
				case CustomDebugInfoKind.UsingGroups:
					if (!immutableArray.IsDefault)
					{
						throw new InvalidOperationException($"Expected at most one Using record for method {FormatMethodToken(methodToken)}");
					}
					immutableArray = DecodeUsingRecord(customDebugInfoRecord.Data);
					break;
				case CustomDebugInfoKind.ForwardMethodInfo:
					if (!externAliasStrings.IsDefault)
					{
						throw new InvalidOperationException($"Did not expect both Forward and ForwardToModule records for method {FormatMethodToken(methodToken)}");
					}
					methodToken = DecodeForwardRecord(customDebugInfoRecord.Data);
					if (!flag)
					{
						flag = true;
						goto IL_0012;
					}
					break;
				case CustomDebugInfoKind.ForwardModuleInfo:
				{
					if (!externAliasStrings.IsDefault)
					{
						throw new InvalidOperationException($"Expected at most one ForwardToModule record for method {FormatMethodToken(methodToken)}");
					}
					int arg2 = DecodeForwardToModuleRecord(customDebugInfoRecord.Data);
					ImmutableArray<string> immutableArray2 = getMethodImportStrings(arg2, arg);
					ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
					foreach (string item in immutableArray2)
					{
						if (IsCSharpExternAliasInfo(item))
						{
							instance.Add(item);
						}
					}
					externAliasStrings = instance.ToImmutableAndFree();
					break;
				}
				}
			}
			break;
		}
		if (immutableArray.IsDefault)
		{
			return default;
		}
		ImmutableArray<string> immutableArray3 = getMethodImportStrings(methodToken, arg);
		ArrayBuilder<ImmutableArray<string>> instance2 = ArrayBuilder<ImmutableArray<string>>.GetInstance(immutableArray.Length);
		ArrayBuilder<string> instance3 = ArrayBuilder<string>.GetInstance();
		int i = 0;
		foreach (short item2 in immutableArray)
		{
			int num = 0;
			while (num < item2)
			{
				if (i >= immutableArray3.Length)
				{
					throw new InvalidOperationException($"Group size indicates more imports than there are import strings (method {FormatMethodToken(methodToken)}).");
				}
				string text = immutableArray3[i];
				if (IsCSharpExternAliasInfo(text))
				{
					throw new InvalidOperationException($"Encountered extern alias info before all import strings were consumed (method {FormatMethodToken(methodToken)}).");
				}
				instance3.Add(text);
				num++;
				i++;
			}
			instance2.Add(instance3.ToImmutable());
			instance3.Clear();
		}
		if (externAliasStrings.IsDefault)
		{
			for (; i < immutableArray3.Length; i++)
			{
				string text2 = immutableArray3[i];
				if (!IsCSharpExternAliasInfo(text2))
				{
					throw new InvalidOperationException($"Expected only extern alias info strings after consuming the indicated number of imports (method {FormatMethodToken(methodToken)}).");
				}
				instance3.Add(text2);
			}
			externAliasStrings = instance3.ToImmutableAndFree();
		}
		else
		{
			instance3.Free();
			if (i < immutableArray3.Length)
			{
				throw new InvalidOperationException($"Group size indicates fewer imports than there are import strings (method {FormatMethodToken(methodToken)}).");
			}
		}
		return instance2.ToImmutableAndFree();
	}

	public static ImmutableArray<string> GetVisualBasicImportStrings<TArg>(int methodToken, TArg arg, Func<int, TArg, ImmutableArray<string>> getMethodImportStrings)
	{
		ImmutableArray<string> result = getMethodImportStrings(methodToken, arg);
		if (result.IsEmpty)
		{
			return ImmutableArray<string>.Empty;
		}
		string text = result[0];
		if (text.Length >= 2 && text[0] == '@')
		{
			char c = text[1];
			if ('0' <= c && c <= '9' && int.TryParse(text.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out var result2))
			{
				result = getMethodImportStrings(result2, arg);
			}
		}
		return result;
	}

	private static void CheckVersion(byte globalVersion, int methodToken)
	{
		if (globalVersion != 4)
		{
			throw new InvalidOperationException($"Method {FormatMethodToken(methodToken)}: Expected version {(byte)4}, but found version {globalVersion}.");
		}
	}

	private static int ReadInt32(ImmutableArray<byte> bytes, ref int offset)
	{
		int num = offset;
		if (num + 4 > bytes.Length)
		{
			throw new InvalidOperationException("Read out of buffer.");
		}
		offset += 4;
		return bytes[num] | (bytes[num + 1] << 8) | (bytes[num + 2] << 16) | (bytes[num + 3] << 24);
	}

	private static short ReadInt16(ImmutableArray<byte> bytes, ref int offset)
	{
		int num = offset;
		if (num + 2 > bytes.Length)
		{
			throw new InvalidOperationException("Read out of buffer.");
		}
		offset += 2;
		return (short)(bytes[num] | (bytes[num + 1] << 8));
	}

	private static byte ReadByte(ImmutableArray<byte> bytes, ref int offset)
	{
		int num = offset;
		if (num + 1 > bytes.Length)
		{
			throw new InvalidOperationException("Read out of buffer.");
		}
		offset++;
		return bytes[num];
	}

	private static bool IsCSharpExternAliasInfo(string import)
	{
		if (import.Length > 0)
		{
			return import[0] == 'Z';
		}
		return false;
	}

	public static bool TryParseCSharpImportString(string import, out string alias, out string externAlias, out string target, out ImportTargetKind kind)
	{
		alias = null;
		externAlias = null;
		target = null;
		kind = ImportTargetKind.Namespace;
		if (string.IsNullOrEmpty(import))
		{
			return false;
		}
		switch (import[0])
		{
		case 'U':
			alias = null;
			externAlias = null;
			target = import.Substring(1);
			kind = ImportTargetKind.Namespace;
			return true;
		case 'E':
			if (!TrySplit(import, 1, ' ', out target, out externAlias))
			{
				return false;
			}
			alias = null;
			kind = ImportTargetKind.Namespace;
			return true;
		case 'T':
			alias = null;
			externAlias = null;
			target = import.Substring(1);
			kind = ImportTargetKind.Type;
			return true;
		case 'A':
			if (!TrySplit(import, 1, ' ', out alias, out target))
			{
				return false;
			}
			switch (target[0])
			{
			case 'U':
				kind = ImportTargetKind.Namespace;
				target = target.Substring(1);
				externAlias = null;
				return true;
			case 'T':
				kind = ImportTargetKind.Type;
				target = target.Substring(1);
				externAlias = null;
				return true;
			case 'E':
				kind = ImportTargetKind.Namespace;
				if (!TrySplit(target, 1, ' ', out target, out externAlias))
				{
					return false;
				}
				return true;
			default:
				return false;
			}
		case 'X':
			externAlias = null;
			alias = import.Substring(1);
			target = null;
			kind = ImportTargetKind.Assembly;
			return true;
		case 'Z':
			if (!TrySplit(import, 1, ' ', out alias, out target))
			{
				return false;
			}
			externAlias = null;
			kind = ImportTargetKind.Assembly;
			return true;
		default:
			return false;
		}
	}

	public static bool TryParseVisualBasicImportString(string import, out string alias, out string target, out ImportTargetKind kind, out VBImportScopeKind scope)
	{
		alias = null;
		target = null;
		kind = ImportTargetKind.Namespace;
		scope = VBImportScopeKind.Unspecified;
		if (import == null)
		{
			return false;
		}
		if (import.Length == 0)
		{
			alias = null;
			target = import;
			kind = ImportTargetKind.CurrentNamespace;
			scope = VBImportScopeKind.Unspecified;
			return true;
		}
		int num = 0;
		switch (import[num])
		{
		case '#':
		case '$':
		case '&':
			alias = null;
			target = import;
			kind = ImportTargetKind.Defunct;
			scope = VBImportScopeKind.Unspecified;
			return true;
		case '*':
			num++;
			alias = null;
			target = import.Substring(num);
			kind = ImportTargetKind.DefaultNamespace;
			scope = VBImportScopeKind.Unspecified;
			return true;
		case '@':
			num++;
			if (num >= import.Length)
			{
				return false;
			}
			scope = VBImportScopeKind.Unspecified;
			switch (import[num])
			{
			case 'F':
				scope = VBImportScopeKind.File;
				num++;
				break;
			case 'P':
				scope = VBImportScopeKind.Project;
				num++;
				break;
			}
			if (num >= import.Length)
			{
				return false;
			}
			switch (import[num])
			{
			case 'A':
				num++;
				if (import[num] != ':')
				{
					return false;
				}
				num++;
				if (!TrySplit(import, num, '=', out alias, out target))
				{
					return false;
				}
				kind = ImportTargetKind.NamespaceOrType;
				return true;
			case 'X':
				num++;
				if (import[num] != ':')
				{
					return false;
				}
				num++;
				if (!TrySplit(import, num, '=', out alias, out target))
				{
					return false;
				}
				kind = ImportTargetKind.XmlNamespace;
				return true;
			case 'T':
				num++;
				if (import[num] != ':')
				{
					return false;
				}
				num++;
				alias = null;
				target = import.Substring(num);
				kind = ImportTargetKind.Type;
				return true;
			case ':':
				num++;
				alias = null;
				target = import.Substring(num);
				kind = ImportTargetKind.Namespace;
				return true;
			default:
				alias = null;
				target = import.Substring(num);
				kind = ImportTargetKind.MethodToken;
				return true;
			}
		default:
			alias = null;
			target = import;
			kind = ImportTargetKind.CurrentNamespace;
			scope = VBImportScopeKind.Unspecified;
			return true;
		}
	}

	private static bool TrySplit(string input, int offset, char separator, out string before, out string after)
	{
		int num = input.IndexOf(separator, offset);
		if (offset <= num && num < input.Length)
		{
			before = input.Substring(offset, num - offset);
			after = ((num + 1 == input.Length) ? "" : input.Substring(num + 1));
			return true;
		}
		before = null;
		after = null;
		return false;
	}

	private static string FormatMethodToken(int methodToken)
	{
		return $"0x{methodToken:x8}";
	}

	private static string ReadUtf8String(ImmutableArray<byte> bytes, ref int offset)
	{
		ArrayBuilder<byte> instance = ArrayBuilder<byte>.GetInstance();
		while (true)
		{
			byte b = ReadByte(bytes, ref offset);
			if (b == 0)
			{
				break;
			}
			instance.Add(b);
		}
		byte[] array = instance.ToArrayAndFree();
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}
}
