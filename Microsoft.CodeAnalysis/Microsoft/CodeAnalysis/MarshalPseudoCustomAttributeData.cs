using System;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis;

internal sealed class MarshalPseudoCustomAttributeData : IMarshallingInformation
{
	private UnmanagedType _marshalType;

	private int _marshalArrayElementType;

	private int _marshalArrayElementCount;

	private int _marshalParameterIndex;

	private object _marshalTypeNameOrSymbol;

	private string _marshalCookie;

	internal const int Invalid = -1;

	private const UnmanagedType InvalidUnmanagedType = (UnmanagedType)(-1);

	private const Microsoft.Cci.VarEnum InvalidVariantType = (Microsoft.Cci.VarEnum)(-1);

	internal const int MaxMarshalInteger = 536870911;

	public UnmanagedType UnmanagedType => _marshalType;

	int IMarshallingInformation.IidParameterIndex => _marshalParameterIndex;

	string IMarshallingInformation.CustomMarshallerRuntimeArgument => _marshalCookie;

	int IMarshallingInformation.NumberOfElements => _marshalArrayElementCount;

	short IMarshallingInformation.ParamIndex => (short)_marshalParameterIndex;

	UnmanagedType IMarshallingInformation.ElementType => (UnmanagedType)_marshalArrayElementType;

	Microsoft.Cci.VarEnum IMarshallingInformation.SafeArrayElementSubtype => (Microsoft.Cci.VarEnum)_marshalArrayElementType;

	internal void SetMarshalAsCustom(object typeSymbolOrName, string cookie)
	{
		_marshalType = UnmanagedType.CustomMarshaler;
		_marshalTypeNameOrSymbol = typeSymbolOrName;
		_marshalCookie = cookie;
	}

	internal void SetMarshalAsComInterface(UnmanagedType unmanagedType, int? parameterIndex)
	{
		_marshalType = unmanagedType;
		_marshalParameterIndex = parameterIndex ?? (-1);
	}

	internal void SetMarshalAsArray(UnmanagedType? elementType, int? elementCount, short? parameterIndex)
	{
		_marshalType = UnmanagedType.LPArray;
		_marshalArrayElementType = (int)(elementType ?? ((UnmanagedType)80));
		_marshalArrayElementCount = elementCount ?? (-1);
		_marshalParameterIndex = parameterIndex ?? (-1);
	}

	internal void SetMarshalAsFixedArray(UnmanagedType? elementType, int? elementCount)
	{
		_marshalType = UnmanagedType.ByValArray;
		_marshalArrayElementType = (int)(elementType ?? ((UnmanagedType)(-1)));
		_marshalArrayElementCount = elementCount ?? (-1);
	}

	internal void SetMarshalAsSafeArray(Microsoft.Cci.VarEnum? elementType, ITypeSymbol elementTypeSymbol)
	{
		_marshalType = UnmanagedType.SafeArray;
		_marshalArrayElementType = (int)(elementType ?? ((Microsoft.Cci.VarEnum)(-1)));
		_marshalTypeNameOrSymbol = elementTypeSymbol;
	}

	internal void SetMarshalAsFixedString(int elementCount)
	{
		_marshalType = UnmanagedType.ByValTStr;
		_marshalArrayElementCount = elementCount;
	}

	internal void SetMarshalAsSimpleType(UnmanagedType type)
	{
		_marshalType = type;
	}

	object IMarshallingInformation.GetCustomMarshaller(EmitContext context)
	{
		if (_marshalTypeNameOrSymbol is ITypeSymbol symbol)
		{
			return context.Module.Translate(symbol, context.SyntaxNodeOpt, context.Diagnostics);
		}
		return _marshalTypeNameOrSymbol;
	}

	ITypeReference IMarshallingInformation.GetSafeArrayElementUserDefinedSubtype(EmitContext context)
	{
		if (_marshalTypeNameOrSymbol == null)
		{
			return null;
		}
		return context.Module.Translate((ITypeSymbol)_marshalTypeNameOrSymbol, context.SyntaxNodeOpt, context.Diagnostics);
	}

	internal MarshalPseudoCustomAttributeData WithTranslatedTypes<TTypeSymbol, TArg>(Func<TTypeSymbol, TArg, TTypeSymbol> translator, TArg arg) where TTypeSymbol : ITypeSymbol
	{
		if (_marshalType != UnmanagedType.SafeArray || _marshalTypeNameOrSymbol == null)
		{
			return this;
		}
		TTypeSymbol val = translator((TTypeSymbol)_marshalTypeNameOrSymbol, arg);
		if ((object)val == _marshalTypeNameOrSymbol)
		{
			return this;
		}
		MarshalPseudoCustomAttributeData marshalPseudoCustomAttributeData = new MarshalPseudoCustomAttributeData();
		marshalPseudoCustomAttributeData.SetMarshalAsSafeArray((Microsoft.Cci.VarEnum)_marshalArrayElementType, val);
		return marshalPseudoCustomAttributeData;
	}

	internal ITypeSymbol TryGetSafeArrayElementUserDefinedSubtype()
	{
		return _marshalTypeNameOrSymbol as ITypeSymbol;
	}
}
