using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public struct Conversion : IEquatable<Conversion>, IConvertibleConversion
{
	private class UncommonData
	{
		internal readonly MethodSymbol _conversionMethod;

		internal readonly ImmutableArray<Conversion> _nestedConversionsOpt;

		internal readonly UserDefinedConversionResult _conversionResult;

		private const byte IsExtensionMethodMask = 1;

		private const byte IsArrayIndexMask = 2;

		private readonly byte _flags;

		internal bool IsExtensionMethod => (_flags & 1) != 0;

		internal bool IsArrayIndex => (_flags & 2) != 0;

		public UncommonData(bool isExtensionMethod, bool isArrayIndex, UserDefinedConversionResult conversionResult, MethodSymbol conversionMethod, ImmutableArray<Conversion> nestedConversions)
		{
			_conversionMethod = conversionMethod;
			_conversionResult = conversionResult;
			_nestedConversionsOpt = nestedConversions;
			_flags = (byte)(isExtensionMethod ? 1 : 0);
			if (isArrayIndex)
			{
				_flags |= 2;
			}
		}
	}

	private class DeconstructionUncommonData : UncommonData
	{
		internal readonly DeconstructMethodInfo DeconstructMethodInfo;

		internal DeconstructionUncommonData(DeconstructMethodInfo deconstructMethodInfoOpt, ImmutableArray<Conversion> nestedConversions)
			: base(isExtensionMethod: false, isArrayIndex: false, default, null, nestedConversions)
		{
			DeconstructMethodInfo = deconstructMethodInfoOpt;
		}
	}

	private static class ConversionSingletons
	{
		internal static ImmutableArray<Conversion> IdentityUnderlying = ImmutableArray.Create(Identity);

		internal static ImmutableArray<Conversion> ImplicitConstantUnderlying = ImmutableArray.Create(ImplicitConstant);

		internal static ImmutableArray<Conversion> ImplicitNumericUnderlying = ImmutableArray.Create(ImplicitNumeric);

		internal static ImmutableArray<Conversion> ExplicitNumericUnderlying = ImmutableArray.Create(ExplicitNumeric);

		internal static ImmutableArray<Conversion> ExplicitEnumerationUnderlying = ImmutableArray.Create(ExplicitEnumeration);

		internal static ImmutableArray<Conversion> PointerToIntegerUnderlying = ImmutableArray.Create(PointerToInteger);
	}

	private readonly ConversionKind _kind;

	private readonly UncommonData _uncommonData;

	internal static Conversion NoConversion => new Conversion(ConversionKind.NoConversion);

	internal static Conversion Identity => new Conversion(ConversionKind.Identity);

	internal static Conversion ImplicitConstant => new Conversion(ConversionKind.ImplicitConstant);

	internal static Conversion ImplicitNumeric => new Conversion(ConversionKind.ImplicitNumeric);

	internal static Conversion ImplicitReference => new Conversion(ConversionKind.ImplicitReference);

	internal static Conversion ImplicitEnumeration => new Conversion(ConversionKind.ImplicitEnumeration);

	internal static Conversion ImplicitThrow => new Conversion(ConversionKind.ImplicitThrow);

	internal static Conversion AnonymousFunction => new Conversion(ConversionKind.AnonymousFunction);

	internal static Conversion Boxing => new Conversion(ConversionKind.Boxing);

	internal static Conversion DefaultOrNullLiteral => new Conversion(ConversionKind.DefaultOrNullLiteral);

	internal static Conversion NullToPointer => new Conversion(ConversionKind.NullToPointer);

	internal static Conversion PointerToVoid => new Conversion(ConversionKind.PointerToVoid);

	internal static Conversion PointerToPointer => new Conversion(ConversionKind.PointerToPointer);

	internal static Conversion PointerToInteger => new Conversion(ConversionKind.PointerToInteger);

	internal static Conversion IntegerToPointer => new Conversion(ConversionKind.IntegerToPointer);

	internal static Conversion Unboxing => new Conversion(ConversionKind.Unboxing);

	internal static Conversion ExplicitReference => new Conversion(ConversionKind.ExplicitReference);

	internal static Conversion IntPtr => new Conversion(ConversionKind.IntPtr);

	internal static Conversion ExplicitEnumeration => new Conversion(ConversionKind.ExplicitEnumeration);

	internal static Conversion ExplicitNumeric => new Conversion(ConversionKind.ExplicitNumeric);

	internal static Conversion ImplicitDynamic => new Conversion(ConversionKind.ImplicitDynamic);

	internal static Conversion ExplicitDynamic => new Conversion(ConversionKind.ExplicitDynamic);

	internal static Conversion InterpolatedString => new Conversion(ConversionKind.InterpolatedString);

	internal static Conversion Deconstruction => new Conversion(ConversionKind.Deconstruction);

	internal static Conversion PinnedObjectToPointer => new Conversion(ConversionKind.PinnedObjectToPointer);

	internal static ImmutableArray<Conversion> IdentityUnderlying => ConversionSingletons.IdentityUnderlying;

	internal static ImmutableArray<Conversion> ImplicitConstantUnderlying => ConversionSingletons.ImplicitConstantUnderlying;

	internal static ImmutableArray<Conversion> ImplicitNumericUnderlying => ConversionSingletons.ImplicitNumericUnderlying;

	internal static ImmutableArray<Conversion> ExplicitNumericUnderlying => ConversionSingletons.ExplicitNumericUnderlying;

	internal static ImmutableArray<Conversion> ExplicitEnumerationUnderlying => ConversionSingletons.ExplicitEnumerationUnderlying;

	internal static ImmutableArray<Conversion> PointerToIntegerUnderlying => ConversionSingletons.PointerToIntegerUnderlying;

	internal ConversionKind Kind => _kind;

	internal bool IsExtensionMethod => _uncommonData?.IsExtensionMethod ?? false;

	internal bool IsArrayIndex => _uncommonData?.IsArrayIndex ?? false;

	internal ImmutableArray<Conversion> UnderlyingConversions => _uncommonData?._nestedConversionsOpt ?? default(ImmutableArray<Conversion>);

	internal MethodSymbol Method
	{
		get
		{
			UncommonData uncommonData = _uncommonData;
			if (uncommonData != null)
			{
				if ((object)uncommonData._conversionMethod != null)
				{
					return uncommonData._conversionMethod;
				}
				UserDefinedConversionResult conversionResult = uncommonData._conversionResult;
				if (conversionResult.Kind == UserDefinedConversionResultKind.Valid)
				{
					return conversionResult.Results[conversionResult.Best].Operator;
				}
				if (uncommonData is DeconstructionUncommonData deconstructionUncommonData && deconstructionUncommonData.DeconstructMethodInfo.Invocation is BoundCall boundCall)
				{
					return boundCall.Method;
				}
			}
			return null;
		}
	}

	internal DeconstructMethodInfo DeconstructionInfo => ((DeconstructionUncommonData)_uncommonData)?.DeconstructMethodInfo ?? default(DeconstructMethodInfo);

	internal bool IsValid
	{
		get
		{
			if (!Exists)
			{
				return false;
			}
			ImmutableArray<Conversion>? immutableArray = _uncommonData?._nestedConversionsOpt;
			if (immutableArray != null)
			{
				foreach (Conversion item in immutableArray.Value)
				{
					if (!item.IsValid)
					{
						return false;
					}
				}
				return true;
			}
			if (IsUserDefined && (object)Method == null)
			{
				UncommonData uncommonData = _uncommonData;
				if (uncommonData == null)
				{
					return false;
				}
				return uncommonData._conversionResult.Kind == UserDefinedConversionResultKind.Valid;
			}
			return true;
		}
	}

	public bool Exists => Kind != ConversionKind.NoConversion;

	public bool IsImplicit => Kind.IsImplicitConversion();

	public bool IsExplicit
	{
		get
		{
			if (Exists)
			{
				return !IsImplicit;
			}
			return false;
		}
	}

	public bool IsIdentity => Kind == ConversionKind.Identity;

	public bool IsStackAlloc
	{
		get
		{
			if (Kind != ConversionKind.StackAllocToPointerType)
			{
				return Kind == ConversionKind.StackAllocToSpanType;
			}
			return true;
		}
	}

	public bool IsNumeric
	{
		get
		{
			if (Kind != ConversionKind.ImplicitNumeric)
			{
				return Kind == ConversionKind.ExplicitNumeric;
			}
			return true;
		}
	}

	public bool IsEnumeration
	{
		get
		{
			if (Kind != ConversionKind.ImplicitEnumeration)
			{
				return Kind == ConversionKind.ExplicitEnumeration;
			}
			return true;
		}
	}

	public bool IsThrow => Kind == ConversionKind.ImplicitThrow;

	public bool IsInterpolatedString => Kind == ConversionKind.InterpolatedString;

	public bool IsNullable
	{
		get
		{
			if (Kind != ConversionKind.ImplicitNullable)
			{
				return Kind == ConversionKind.ExplicitNullable;
			}
			return true;
		}
	}

	public bool IsTupleLiteralConversion
	{
		get
		{
			if (Kind != ConversionKind.ImplicitTupleLiteral)
			{
				return Kind == ConversionKind.ExplicitTupleLiteral;
			}
			return true;
		}
	}

	public bool IsTupleConversion
	{
		get
		{
			if (Kind != ConversionKind.ImplicitTuple)
			{
				return Kind == ConversionKind.ExplicitTuple;
			}
			return true;
		}
	}

	public bool IsReference
	{
		get
		{
			if (Kind != ConversionKind.ImplicitReference)
			{
				return Kind == ConversionKind.ExplicitReference;
			}
			return true;
		}
	}

	public bool IsUserDefined => Kind.IsUserDefinedConversion();

	public bool IsBoxing => Kind == ConversionKind.Boxing;

	public bool IsUnboxing => Kind == ConversionKind.Unboxing;

	public bool IsNullLiteral => Kind == ConversionKind.DefaultOrNullLiteral;

	public bool IsDynamic => Kind.IsDynamic();

	public bool IsConstantExpression => Kind == ConversionKind.ImplicitConstant;

	public bool IsAnonymousFunction => Kind == ConversionKind.AnonymousFunction;

	public bool IsMethodGroup => Kind == ConversionKind.MethodGroup;

	public bool IsPointer => Kind.IsPointerConversion();

	public bool IsIntPtr => Kind == ConversionKind.IntPtr;

	public IMethodSymbol MethodSymbol => Method;

	internal LookupResultKind ResultKind
	{
		get
		{
			UserDefinedConversionResult userDefinedConversionResult = _uncommonData?._conversionResult ?? default(UserDefinedConversionResult);
			switch (userDefinedConversionResult.Kind)
			{
			case UserDefinedConversionResultKind.Valid:
				return LookupResultKind.Viable;
			case UserDefinedConversionResultKind.NoBestSourceType:
			case UserDefinedConversionResultKind.NoBestTargetType:
			case UserDefinedConversionResultKind.Ambiguous:
				return LookupResultKind.OverloadResolutionFailure;
			case UserDefinedConversionResultKind.NoApplicableOperators:
				if (userDefinedConversionResult.Results.IsDefaultOrEmpty)
				{
					if (Kind != ConversionKind.NoConversion)
					{
						return LookupResultKind.Viable;
					}
					return LookupResultKind.Empty;
				}
				return LookupResultKind.OverloadResolutionFailure;
			default:
				throw ExceptionUtilities.UnexpectedValue(userDefinedConversionResult.Kind);
			}
		}
	}

	internal Conversion UserDefinedFromConversion => BestUserDefinedConversionAnalysis?.SourceConversion ?? NoConversion;

	internal Conversion UserDefinedToConversion => BestUserDefinedConversionAnalysis?.TargetConversion ?? NoConversion;

	internal ImmutableArray<MethodSymbol> OriginalUserDefinedConversions
	{
		get
		{
			if (_uncommonData == null)
			{
				return ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>.Empty;
			}
			UserDefinedConversionResult conversionResult = _uncommonData._conversionResult;
			if (conversionResult.Kind == UserDefinedConversionResultKind.NoApplicableOperators)
			{
				return ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>.Empty;
			}
			ArrayBuilder<MethodSymbol> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>.GetInstance();
			foreach (UserDefinedConversionAnalysis result in conversionResult.Results)
			{
				instance.Add(result.Operator);
			}
			return instance.ToImmutableAndFree();
		}
	}

	internal UserDefinedConversionAnalysis BestUserDefinedConversionAnalysis
	{
		get
		{
			if (_uncommonData == null)
			{
				return null;
			}
			UserDefinedConversionResult conversionResult = _uncommonData._conversionResult;
			if (conversionResult.Kind == UserDefinedConversionResultKind.Valid)
			{
				return conversionResult.Results[conversionResult.Best];
			}
			return null;
		}
	}

	private Conversion(ConversionKind kind, UncommonData uncommonData)
	{
		_kind = kind;
		_uncommonData = uncommonData;
	}

	private Conversion(ConversionKind kind)
		: this(kind, null)
	{
	}

	internal Conversion(UserDefinedConversionResult conversionResult, bool isImplicit)
	{
		_kind = ((conversionResult.Kind != UserDefinedConversionResultKind.NoApplicableOperators) ? (isImplicit ? ConversionKind.ImplicitUserDefined : ConversionKind.ExplicitUserDefined) : ConversionKind.NoConversion);
		_uncommonData = new UncommonData(isExtensionMethod: false, isArrayIndex: false, conversionResult, null, default);
	}

	internal Conversion(ConversionKind kind, MethodSymbol conversionMethod, bool isExtensionMethod)
	{
		_kind = kind;
		_uncommonData = new UncommonData(isExtensionMethod, isArrayIndex: false, default, conversionMethod, default);
	}

	internal Conversion(ConversionKind kind, ImmutableArray<Conversion> nestedConversions)
	{
		_kind = kind;
		_uncommonData = new UncommonData(isExtensionMethod: false, isArrayIndex: false, default, null, nestedConversions);
	}

	internal Conversion(ConversionKind kind, DeconstructMethodInfo deconstructMethodInfo, ImmutableArray<Conversion> nestedConversions)
	{
		_kind = kind;
		_uncommonData = new DeconstructionUncommonData(deconstructMethodInfo, nestedConversions);
	}

	internal Conversion SetConversionMethod(MethodSymbol conversionMethod)
	{
		return new Conversion(Kind, conversionMethod, IsExtensionMethod);
	}

	internal Conversion SetArrayIndexConversionForDynamic()
	{
		return new Conversion(_kind, new UncommonData(isExtensionMethod: false, isArrayIndex: true, default, null, default));
	}

	[Conditional("DEBUG")]
	private static void AssertTrivialConversion(ConversionKind kind)
	{
		switch (kind)
		{
		}
	}

	internal static Conversion GetTrivialConversion(ConversionKind kind)
	{
		return new Conversion(kind);
	}

	internal static Conversion MakeStackAllocToPointerType(Conversion underlyingConversion)
	{
		return new Conversion(ConversionKind.StackAllocToPointerType, ImmutableArray.Create(underlyingConversion));
	}

	internal static Conversion MakeStackAllocToSpanType(Conversion underlyingConversion)
	{
		return new Conversion(ConversionKind.StackAllocToSpanType, ImmutableArray.Create(underlyingConversion));
	}

	internal static Conversion MakeNullableConversion(ConversionKind kind, Conversion nestedConversion)
	{
		return new Conversion(kind, nestedConversion.Kind switch
		{
			ConversionKind.Identity => IdentityUnderlying, 
			ConversionKind.ImplicitConstant => ImplicitConstantUnderlying, 
			ConversionKind.ImplicitNumeric => ImplicitNumericUnderlying, 
			ConversionKind.ExplicitNumeric => ExplicitNumericUnderlying, 
			ConversionKind.ExplicitEnumeration => ExplicitEnumerationUnderlying, 
			ConversionKind.PointerToInteger => PointerToIntegerUnderlying, 
			_ => ImmutableArray.Create(nestedConversion), 
		});
	}

	public CommonConversion ToCommonConversion()
	{
		IMethodSymbol methodSymbol = (IsUserDefined ? MethodSymbol : null);
		return new CommonConversion(Exists, IsIdentity, IsNumeric, IsReference, IsImplicit, methodSymbol);
	}

	public override string ToString()
	{
		return Kind.ToString();
	}

	public override bool Equals(object obj)
	{
		if (obj is Conversion)
		{
			return Equals((Conversion)obj);
		}
		return false;
	}

	public bool Equals(Conversion other)
	{
		if (Kind == other.Kind)
		{
			return Method == other.Method;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Method, (int)Kind);
	}

	public static bool operator ==(Conversion left, Conversion right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Conversion left, Conversion right)
	{
		return !(left == right);
	}
}
