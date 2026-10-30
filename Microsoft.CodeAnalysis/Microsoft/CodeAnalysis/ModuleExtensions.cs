using System;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis;

internal static class ModuleExtensions
{
	private const string VTableGapMethodNamePrefix = "_VtblGap";

	public static bool ShouldImportNestedType(this PEModule module, TypeDefinitionHandle typeDef)
	{
		return true;
	}

	public static bool ShouldImportField(this PEModule module, FieldDefinitionHandle field, MetadataImportOptions importOptions)
	{
		try
		{
			return ShouldImportField(module.GetFieldDefFlagsOrThrow(field), importOptions);
		}
		catch (BadImageFormatException)
		{
			return true;
		}
	}

	public static bool ShouldImportField(FieldAttributes flags, MetadataImportOptions importOptions)
	{
		switch (flags & FieldAttributes.FieldAccessMask)
		{
		case FieldAttributes.PrivateScope:
		case FieldAttributes.Private:
			return importOptions == MetadataImportOptions.All;
		case FieldAttributes.Assembly:
			return (int)importOptions >= 1;
		default:
			return true;
		}
	}

	public static bool ShouldImportMethod(this PEModule module, MethodDefinitionHandle methodDef, MetadataImportOptions importOptions)
	{
		try
		{
			MethodAttributes methodDefFlagsOrThrow = module.GetMethodDefFlagsOrThrow(methodDef);
			if ((methodDefFlagsOrThrow & MethodAttributes.Virtual) == 0)
			{
				switch (methodDefFlagsOrThrow & MethodAttributes.MemberAccessMask)
				{
				case MethodAttributes.PrivateScope:
				case MethodAttributes.Private:
					if (importOptions != MetadataImportOptions.All)
					{
						return false;
					}
					break;
				case MethodAttributes.Assembly:
					if (importOptions == MetadataImportOptions.Public)
					{
						return false;
					}
					break;
				}
			}
		}
		catch (BadImageFormatException)
		{
		}
		try
		{
			return !module.GetMethodDefNameOrThrow(methodDef).StartsWith("_VtblGap", StringComparison.Ordinal);
		}
		catch (BadImageFormatException)
		{
			return true;
		}
	}

	public static int GetVTableGapSize(string emittedMethodName)
	{
		if (emittedMethodName.StartsWith("_VtblGap", StringComparison.Ordinal))
		{
			int i;
			for (i = "_VtblGap".Length; i < emittedMethodName.Length && char.IsDigit(emittedMethodName, i); i++)
			{
			}
			if (i == "_VtblGap".Length || i >= emittedMethodName.Length - 1 || emittedMethodName[i] != '_' || !char.IsDigit(emittedMethodName, i + 1))
			{
				return 1;
			}
			if (int.TryParse(emittedMethodName.Substring(i + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var result) && result > 0)
			{
				return result;
			}
			return 1;
		}
		return 0;
	}

	public static string GetVTableGapName(int sequenceNumber, int countOfSlots)
	{
		return $"_VtblGap{sequenceNumber}_{countOfSlots}";
	}
}
