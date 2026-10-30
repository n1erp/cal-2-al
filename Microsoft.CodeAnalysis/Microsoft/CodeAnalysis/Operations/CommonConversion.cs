using System;

namespace Microsoft.CodeAnalysis.Operations;

public struct CommonConversion
{
	[Flags]
	private enum ConversionKind
	{
		None = 0,
		Exists = 1,
		IsIdentity = 2,
		IsNumeric = 4,
		IsReference = 8,
		IsImplicit = 0x10
	}

	private readonly ConversionKind _conversionKind;

	public bool Exists => (_conversionKind & ConversionKind.Exists) == ConversionKind.Exists;

	public bool IsIdentity => (_conversionKind & ConversionKind.IsIdentity) == ConversionKind.IsIdentity;

	public bool IsNumeric => (_conversionKind & ConversionKind.IsNumeric) == ConversionKind.IsNumeric;

	public bool IsReference => (_conversionKind & ConversionKind.IsReference) == ConversionKind.IsReference;

	public bool IsImplicit => (_conversionKind & ConversionKind.IsImplicit) == ConversionKind.IsImplicit;

	public bool IsUserDefined => MethodSymbol != null;

	public IMethodSymbol MethodSymbol { get; }

	internal CommonConversion(bool exists, bool isIdentity, bool isNumeric, bool isReference, bool isImplicit, IMethodSymbol methodSymbol)
	{
		_conversionKind = (ConversionKind)((exists ? 1 : 0) | (isIdentity ? 2 : 0) | (isNumeric ? 4 : 0) | (isReference ? 8 : 0) | (isImplicit ? 16 : 0));
		MethodSymbol = methodSymbol;
	}
}
