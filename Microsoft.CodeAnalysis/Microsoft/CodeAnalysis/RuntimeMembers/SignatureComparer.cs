using System.Collections.Immutable;
using System.Reflection.Metadata;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.RuntimeMembers;

internal abstract class SignatureComparer<MethodSymbol, FieldSymbol, PropertySymbol, TypeSymbol, ParameterSymbol> where MethodSymbol : class where FieldSymbol : class where PropertySymbol : class where TypeSymbol : class where ParameterSymbol : class
{
	public bool MatchFieldSignature(FieldSymbol field, ImmutableArray<byte> signature)
	{
		int position = 0;
		return MatchType(GetFieldType(field), signature, ref position);
	}

	public bool MatchPropertySignature(PropertySymbol property, ImmutableArray<byte> signature)
	{
		int position = 0;
		byte b = signature[position++];
		ImmutableArray<ParameterSymbol> parameters = GetParameters(property);
		if (b != parameters.Length)
		{
			return false;
		}
		if (!MatchType(GetPropertyType(property), signature, ref position))
		{
			return false;
		}
		foreach (ParameterSymbol item in parameters)
		{
			if (!MatchParameter(item, signature, ref position))
			{
				return false;
			}
		}
		return true;
	}

	public bool MatchMethodSignature(MethodSymbol method, ImmutableArray<byte> signature)
	{
		int position = 0;
		byte b = signature[position++];
		ImmutableArray<ParameterSymbol> parameters = GetParameters(method);
		if (b != parameters.Length)
		{
			return false;
		}
		bool flag = IsByRef(signature, ref position);
		if (IsByRefMethod(method) != flag)
		{
			return false;
		}
		if (!MatchType(GetReturnType(method), signature, ref position))
		{
			return false;
		}
		foreach (ParameterSymbol item in parameters)
		{
			if (!MatchParameter(item, signature, ref position))
			{
				return false;
			}
		}
		return true;
	}

	private bool MatchParameter(ParameterSymbol parameter, ImmutableArray<byte> signature, ref int position)
	{
		bool flag = IsByRef(signature, ref position);
		if (IsByRefParam(parameter) != flag)
		{
			return false;
		}
		return MatchType(GetParamType(parameter), signature, ref position);
	}

	private static bool IsByRef(ImmutableArray<byte> signature, ref int position)
	{
		if (signature[position] == 16)
		{
			position++;
			return true;
		}
		return false;
	}

	private bool MatchType(TypeSymbol type, ImmutableArray<byte> signature, ref int position)
	{
		if (type == null)
		{
			return false;
		}
		SignatureTypeCode signatureTypeCode = (SignatureTypeCode)signature[position++];
		switch (signatureTypeCode)
		{
		case SignatureTypeCode.TypeHandle:
		{
			short typeId = ReadTypeId(signature, ref position);
			return MatchTypeToTypeId(type, typeId);
		}
		case SignatureTypeCode.Array:
		{
			if (!MatchType(GetMDArrayElementType(type), signature, ref position))
			{
				return false;
			}
			int countOfDimensions = signature[position++];
			return MatchArrayRank(type, countOfDimensions);
		}
		case SignatureTypeCode.SZArray:
			return MatchType(GetSZArrayElementType(type), signature, ref position);
		case SignatureTypeCode.Pointer:
			return MatchType(GetPointedToType(type), signature, ref position);
		case SignatureTypeCode.GenericTypeParameter:
		{
			int paramPosition = signature[position++];
			return IsGenericTypeParam(type, paramPosition);
		}
		case SignatureTypeCode.GenericMethodParameter:
		{
			int paramPosition = signature[position++];
			return IsGenericMethodTypeParam(type, paramPosition);
		}
		case SignatureTypeCode.GenericTypeInstance:
		{
			if (!MatchType(GetGenericTypeDefinition(type), signature, ref position))
			{
				return false;
			}
			int num = signature[position++];
			for (int i = 0; i < num; i++)
			{
				if (!MatchType(GetGenericTypeArgument(type, i), signature, ref position))
				{
					return false;
				}
			}
			return true;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(signatureTypeCode);
		}
	}

	private static short ReadTypeId(ImmutableArray<byte> signature, ref int position)
	{
		byte b = signature[position++];
		if (b == byte.MaxValue)
		{
			return (short)(signature[position++] + 255);
		}
		return b;
	}

	protected abstract TypeSymbol GetGenericTypeArgument(TypeSymbol type, int argumentIndex);

	protected abstract TypeSymbol GetGenericTypeDefinition(TypeSymbol type);

	protected abstract bool IsGenericMethodTypeParam(TypeSymbol type, int paramPosition);

	protected abstract bool IsGenericTypeParam(TypeSymbol type, int paramPosition);

	protected abstract TypeSymbol GetPointedToType(TypeSymbol type);

	protected abstract TypeSymbol GetSZArrayElementType(TypeSymbol type);

	protected abstract bool MatchArrayRank(TypeSymbol type, int countOfDimensions);

	protected abstract TypeSymbol GetMDArrayElementType(TypeSymbol type);

	protected abstract bool MatchTypeToTypeId(TypeSymbol type, int typeId);

	protected abstract TypeSymbol GetReturnType(MethodSymbol method);

	protected abstract ImmutableArray<ParameterSymbol> GetParameters(MethodSymbol method);

	protected abstract TypeSymbol GetPropertyType(PropertySymbol property);

	protected abstract ImmutableArray<ParameterSymbol> GetParameters(PropertySymbol property);

	protected abstract TypeSymbol GetParamType(ParameterSymbol parameter);

	protected abstract bool IsByRefParam(ParameterSymbol parameter);

	protected abstract bool IsByRefMethod(MethodSymbol method);

	protected abstract TypeSymbol GetFieldType(FieldSymbol field);
}
