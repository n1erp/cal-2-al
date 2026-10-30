using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct Conversion : IEquatable<Conversion>
{
	internal static readonly Conversion NoConversion = new Conversion(ConversionKind.NoConversion);

	internal static readonly Conversion Identity = new Conversion(ConversionKind.Identity);

	internal static readonly Conversion ImplicitNumeric = new Conversion(ConversionKind.ImplicitNumeric);

	internal static readonly Conversion ToDecimal = new Conversion(ConversionKind.ToDecimal);

	internal static readonly Conversion ImplicitNumericToSmaller = new Conversion(ConversionKind.ImplicitNumericToSmaller);

	internal static readonly Conversion ToText = new Conversion(ConversionKind.ToText);

	internal static readonly Conversion ImplicitRecord = new Conversion(ConversionKind.ImplicitRecord);

	internal static readonly Conversion ImplicitCodeunit = new Conversion(ConversionKind.ImplicitCodeunit);

	internal static readonly Conversion ImplicitText = new Conversion(ConversionKind.ImplicitText);

	internal static readonly Conversion ImplicitTextToSmaller = new Conversion(ConversionKind.ImplicitTextToSmaller);

	internal static readonly Conversion ImplicitOption = new Conversion(ConversionKind.ImplicitOption);

	internal static readonly Conversion ToGuid = new Conversion(ConversionKind.ToGuid);

	internal static readonly Conversion ToVariant = new Conversion(ConversionKind.ToVariant);

	internal static readonly Conversion FromVariant = new Conversion(ConversionKind.FromVariant);

	internal static readonly Conversion ImplicitArray = new Conversion(ConversionKind.ImplicitArray);

	internal static readonly Conversion ToJoker = new Conversion(ConversionKind.ImplicitJoker);

	internal static readonly Conversion FromJoker = new Conversion(ConversionKind.FromJoker);

	internal static readonly Conversion NarrowingToDotNet = new Conversion(ConversionKind.NarrowingToDotNet);

	internal static readonly Conversion WideningToDotNet = new Conversion(ConversionKind.WideningToDotNet);

	internal static readonly Conversion PreservingToDotNet = new Conversion(ConversionKind.PreservingToDotNet);

	internal static readonly Conversion PreservingFromDotNet = new Conversion(ConversionKind.PreservingFromDotNet);

	internal static readonly Conversion NarrowingFromDotNet = new Conversion(ConversionKind.NarrowingFromDotNet);

	internal static readonly Conversion WideningFromDotNet = new Conversion(ConversionKind.WideningFromDotNet);

	internal static readonly Conversion NarrowingFromDotNetToDotNet = new Conversion(ConversionKind.NarrowingFromDotNetToDotNet);

	internal static readonly Conversion WideningFromDotNetToDotNet = new Conversion(ConversionKind.WideningFromDotNetToDotNet);

	internal static readonly Conversion PreservingFromDotNetToDotNet = new Conversion(ConversionKind.PreservingFromDotNetToDotNet);

	internal readonly ConversionKind Kind;

	private readonly byte flags;

	private const byte IsExtensionMethodMask = 1;

	private const byte IsArrayIndexMask = 2;

	internal bool IsExtensionMethod => (flags & 1) != 0;

	internal bool IsArrayIndex => (flags & 2) != 0;

	internal bool IsValid => Exists;

	public bool Exists => Kind != ConversionKind.NoConversion;

	public bool IsImplicit => Kind.IsImplicitConversion();

	public bool IsPureImplicit => Kind.IsPureImplicitConversion();

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

	public bool IsNumeric
	{
		get
		{
			if (Kind != ConversionKind.ImplicitNumeric)
			{
				return Kind == ConversionKind.ImplicitNumericToSmaller;
			}
			return true;
		}
	}

	private Conversion(ConversionKind kind, bool isExtensionMethod, bool isArrayIndex)
	{
		Kind = kind;
		flags = (byte)(isExtensionMethod ? 1 : 0);
		if (isArrayIndex)
		{
			flags |= 2;
		}
	}

	internal Conversion(ConversionKind kind)
	{
		this = default;
		Kind = kind;
	}

	internal Conversion ToArrayIndexConversion()
	{
		return new Conversion(Kind, IsExtensionMethod, isArrayIndex: true);
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
		return Kind == other.Kind;
	}

	public override int GetHashCode()
	{
		return (int)Kind;
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
