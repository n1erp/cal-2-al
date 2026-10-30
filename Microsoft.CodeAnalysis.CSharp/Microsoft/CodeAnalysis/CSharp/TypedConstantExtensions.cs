using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

public static class TypedConstantExtensions
{
	public static string ToCSharpString(this TypedConstant constant)
	{
		if (constant.IsNull)
		{
			return "null";
		}
		if (constant.Kind == TypedConstantKind.Array)
		{
			return "{" + string.Join(", ", constant.Values.Select((TypedConstant v) => v.ToCSharpString())) + "}";
		}
		if (constant.Kind == TypedConstantKind.Type || constant.Type.SpecialType == SpecialType.System_Object)
		{
			return "typeof(" + constant.Value.ToString() + ")";
		}
		if (constant.Kind == TypedConstantKind.Enum)
		{
			return DisplayEnumConstant(constant);
		}
		return SymbolDisplay.FormatPrimitive(constant.Value, quoteStrings: true, useHexadecimalNumbers: false);
	}

	private static string DisplayEnumConstant(TypedConstant constant)
	{
		SpecialType specialType = ((INamedTypeSymbol)constant.Type).EnumUnderlyingType.SpecialType;
		ConstantValue constantValue = ConstantValue.Create(constant.Value, specialType);
		string typeName = constant.Type.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat);
		if (constantValue.IsUnsigned)
		{
			return DisplayUnsignedEnumConstant(constant, specialType, constantValue.UInt64Value, typeName);
		}
		return DisplaySignedEnumConstant(constant, specialType, constantValue.Int64Value, typeName);
	}

	private static string DisplayUnsignedEnumConstant(TypedConstant constant, SpecialType specialType, ulong constantToDecode, string typeName)
	{
		ulong num = 0uL;
		PooledStringBuilder pooledStringBuilder = null;
		StringBuilder stringBuilder = null;
		foreach (ISymbol member in constant.Type.GetMembers())
		{
			if (!(member is IFieldSymbol { HasConstantValue: not false } fieldSymbol))
			{
				continue;
			}
			ulong uInt64Value = ConstantValue.Create(fieldSymbol.ConstantValue, specialType).UInt64Value;
			if (uInt64Value == constantToDecode)
			{
				pooledStringBuilder?.Free();
				return typeName + "." + fieldSymbol.Name;
			}
			if ((uInt64Value & constantToDecode) == uInt64Value)
			{
				num |= uInt64Value;
				if (stringBuilder == null)
				{
					pooledStringBuilder = PooledStringBuilder.GetInstance();
					stringBuilder = pooledStringBuilder.Builder;
				}
				else
				{
					stringBuilder.Append(" | ");
				}
				stringBuilder.Append(typeName);
				stringBuilder.Append(".");
				stringBuilder.Append(fieldSymbol.Name);
			}
		}
		if (pooledStringBuilder != null)
		{
			if (num == constantToDecode)
			{
				return pooledStringBuilder.ToStringAndFree();
			}
			pooledStringBuilder.Free();
		}
		return constant.Value.ToString();
	}

	private static string DisplaySignedEnumConstant(TypedConstant constant, SpecialType specialType, long constantToDecode, string typeName)
	{
		long num = 0L;
		PooledStringBuilder pooledStringBuilder = null;
		StringBuilder stringBuilder = null;
		foreach (ISymbol member in constant.Type.GetMembers())
		{
			if (!(member is IFieldSymbol { HasConstantValue: not false } fieldSymbol))
			{
				continue;
			}
			long int64Value = ConstantValue.Create(fieldSymbol.ConstantValue, specialType).Int64Value;
			if (int64Value == constantToDecode)
			{
				pooledStringBuilder?.Free();
				return typeName + "." + fieldSymbol.Name;
			}
			if ((int64Value & constantToDecode) == int64Value)
			{
				num |= int64Value;
				if (stringBuilder == null)
				{
					pooledStringBuilder = PooledStringBuilder.GetInstance();
					stringBuilder = pooledStringBuilder.Builder;
				}
				else
				{
					stringBuilder.Append(" | ");
				}
				stringBuilder.Append(typeName);
				stringBuilder.Append(".");
				stringBuilder.Append(fieldSymbol.Name);
			}
		}
		if (pooledStringBuilder != null)
		{
			if (num == constantToDecode)
			{
				return pooledStringBuilder.ToStringAndFree();
			}
			pooledStringBuilder.Free();
		}
		return constant.Value.ToString();
	}
}
