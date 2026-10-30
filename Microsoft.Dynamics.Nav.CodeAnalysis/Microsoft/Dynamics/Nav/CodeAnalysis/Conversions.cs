using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class Conversions
{
	private static class ConversionEasyOut
	{
		[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
		private static readonly byte[,] ConversionKind;

		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static ConversionEasyOut()
		{
			ConversionKind = new byte[14, 14]
			{
				{
					28, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0
				},
				{
					0, 28, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0
				},
				{
					0, 0, 28, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0
				},
				{
					0, 0, 0, 28, 16, 25, 15, 25, 13, 24,
					0, 0, 0, 0
				},
				{
					23, 23, 23, 13, 28, 25, 15, 25, 13, 24,
					0, 0, 0, 0
				},
				{
					0, 0, 0, 13, 13, 28, 15, 25, 13, 24,
					0, 0, 0, 15
				},
				{
					0, 0, 0, 13, 13, 28, 26, 25, 13, 24,
					0, 0, 0, 15
				},
				{
					0, 0, 0, 13, 13, 13, 15, 28, 13, 24,
					0, 0, 0, 0
				},
				{
					0, 0, 0, 13, 13, 13, 15, 13, 28, 24,
					0, 0, 0, 0
				},
				{
					0, 0, 0, 13, 13, 13, 15, 13, 13, 28,
					0, 0, 0, 0
				},
				{
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					28, 0, 0, 0
				},
				{
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 28, 0, 0
				},
				{
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 28, 0
				},
				{
					0, 0, 0, 0, 0, 28, 15, 0, 0, 0,
					0, 0, 0, 0
				}
			};
		}

		private static int TypeToIndex(TypeSymbol type)
		{
			return type.GetNavTypeKindSafe() switch
			{
				NavTypeKind.String => 0, 
				NavTypeKind.Text => 1, 
				NavTypeKind.Code => 2, 
				NavTypeKind.Byte => 3, 
				NavTypeKind.Char => 4, 
				NavTypeKind.Integer => 5, 
				NavTypeKind.Option => 6, 
				NavTypeKind.BigInteger => 7, 
				NavTypeKind.Duration => 8, 
				NavTypeKind.Decimal => 9, 
				NavTypeKind.Date => 10, 
				NavTypeKind.Time => 11, 
				NavTypeKind.DateTime => 12, 
				NavTypeKind.Enum => 13, 
				_ => -1, 
			};
		}

		public static ConversionKind ClassifyConversion(TypeSymbol source, TypeSymbol target)
		{
			if (source.HasLength || target.HasLength)
			{
				return Microsoft.Dynamics.Nav.CodeAnalysis.ConversionKind.NoConversion;
			}
			int num = TypeToIndex(source);
			if (num < 0)
			{
				return Microsoft.Dynamics.Nav.CodeAnalysis.ConversionKind.NoConversion;
			}
			int num2 = TypeToIndex(target);
			if (num2 < 0)
			{
				return Microsoft.Dynamics.Nav.CodeAnalysis.ConversionKind.NoConversion;
			}
			return (ConversionKind)ConversionKind[num, num2];
		}
	}

	private const bool F = false;

	private const bool T = true;

	[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
	private static readonly bool[,] ImplicitNumericConversions = new bool[7, 7]
	{
		{ false, true, true, true, true, true, true },
		{ false, false, true, true, true, true, true },
		{ false, false, false, true, true, true, true },
		{ false, false, false, false, true, false, true },
		{ false, false, false, false, false, true, true },
		{ false, false, false, false, false, false, true },
		{ false, false, false, false, false, false, false }
	};

	[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
	private static readonly bool[,] ImplicitNumericToSmallerConversions = new bool[7, 7]
	{
		{ false, true, false, true, false, true, false },
		{ true, false, false, false, false, false, false },
		{ true, true, false, true, false, true, false },
		{ true, false, false, false, false, true, false },
		{ true, true, true, false, false, true, false },
		{ true, true, true, false, true, false, false },
		{ true, true, true, false, true, true, false }
	};

	public Conversion ClassifyConversionFromExpression(BoundExpression sourceExpression, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(sourceExpression != null);
		DebugAssertHelper.Assert((object)destination != null);
		return ClassifyConversionFromExpression(sourceExpression, sourceExpression.Type, destination, ref useSiteDiagnostics);
	}

	public Conversion ClassifyConversionFromExpression(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(sourceExpression != null || (object)source != null);
		DebugAssertHelper.Assert(sourceExpression == null || (object)sourceExpression.Type == source);
		DebugAssertHelper.Assert((object)destination != null);
		return ClassifyImplicitConversionFromExpression(sourceExpression, source, destination, ref useSiteDiagnostics);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "useSiteDiagnostics")]
	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	private Conversion ClassifyImplicitBuiltInConversionFromExpression(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(sourceExpression != null || (object)source != null);
		DebugAssertHelper.Assert((object)destination != null);
		if (sourceExpression == null)
		{
			return Conversion.NoConversion;
		}
		ConversionKind conversionKind = ClassifyImplicitConstantExpressionConversion(sourceExpression, destination);
		if (conversionKind != ConversionKind.NoConversion)
		{
			return new Conversion(conversionKind);
		}
		return Conversion.NoConversion;
	}

	public Conversion ClassifyImplicitConversionFromExpression(BoundExpression sourceExpression, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(sourceExpression != null);
		DebugAssertHelper.Assert((object)destination != null);
		return ClassifyImplicitConversionFromExpression(sourceExpression, sourceExpression.Type, destination, ref useSiteDiagnostics);
	}

	public Conversion ClassifyConversionFromType(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert((object)source != null);
		DebugAssertHelper.Assert((object)destination != null);
		return ClassifyImplicitConversionFromExpression(null, source, destination, ref useSiteDiagnostics);
	}

	private Conversion ClassifyImplicitConversionFromExpression(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(sourceExpression != null || (object)source != null);
		DebugAssertHelper.Assert(sourceExpression == null || (object)sourceExpression.Type == source);
		DebugAssertHelper.Assert((object)destination != null);
		if ((object)source != null && HasIdentityConversion(source, destination))
		{
			return Conversion.Identity;
		}
		Conversion conversion = ClassifyImplicitBuiltInConversionFromExpression(sourceExpression, source, destination, ref useSiteDiagnostics);
		if (conversion.Exists)
		{
			return conversion;
		}
		if ((object)source != null)
		{
			Conversion result = FastClassifyConversion(source, destination);
			if (result.Exists)
			{
				if (!result.IsImplicit)
				{
					return Conversion.NoConversion;
				}
				return result;
			}
			conversion = ClassifyImplicitBuiltInConversionSlow(source, destination, ref useSiteDiagnostics);
			if (conversion.Exists)
			{
				return conversion;
			}
		}
		if (IsCharLiteral(sourceExpression))
		{
			return SlowClassifyConversionFromCharLiteral(destination, ref useSiteDiagnostics, ref conversion);
		}
		return Conversion.NoConversion;
	}

	internal static void ApplyCompatibilityEnumTypeSubstitution(ref TypeSymbol source, ref TypeSymbol destination)
	{
		if (!(source == null) && !(destination == null) && (source.NavTypeKind == NavTypeKind.Enum || destination.NavTypeKind == NavTypeKind.Enum) && (source.NavTypeKind != NavTypeKind.Enum || destination.NavTypeKind != NavTypeKind.Enum))
		{
			if (source.NavTypeKind == NavTypeKind.Enum && IsEnumConversionAllowed(destination.NavTypeKind))
			{
				source = NavCorLib.OptionType;
			}
			else if (destination.NavTypeKind == NavTypeKind.Enum && IsEnumConversionAllowed(source.NavTypeKind))
			{
				destination = NavCorLib.OptionType;
			}
		}
		static bool IsEnumConversionAllowed(NavTypeKind otherTypeKind)
		{
			switch (otherTypeKind)
			{
			case NavTypeKind.Integer:
			case NavTypeKind.Option:
				return true;
			default:
				return false;
			}
		}
	}

	private static Conversion SlowClassifyConversionFromCharLiteral(TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ref Conversion conversion)
	{
		switch (destination.NavTypeKind)
		{
		case NavTypeKind.Byte:
			return FastClassifyConversion(TypeSymbol.GetNavTypeSymbol(NavTypeKind.Char), destination);
		case NavTypeKind.DotNet:
		{
			Conversion result = ConversionDotNet.ClassifyDotNetConversion(TypeSymbol.GetNavTypeSymbol(NavTypeKind.Char), destination, ref useSiteDiagnostics);
			if (result.Kind == ConversionKind.PreservingToDotNet)
			{
				return Conversion.NarrowingToDotNet;
			}
			return result;
		}
		default:
			return Conversion.NoConversion;
		}
	}

	private static bool IsCharLiteral(BoundExpression sourceExpression)
	{
		if (sourceExpression != null && sourceExpression.ConstantValue?.IsString == true)
		{
			return sourceExpression.ConstantValue.StringValue.Length == 1;
		}
		return false;
	}

	private static ConversionKind ClassifyImplicitConstantExpressionConversion(BoundExpression source, TypeSymbol destination)
	{
		if (HasImplicitConstantExpressionConversion(source, destination))
		{
			return ConversionKind.ImplicitConstant;
		}
		return ConversionKind.NoConversion;
	}

	internal static bool HasImplicitConstantExpressionConversion(BoundExpression source, TypeSymbol destination)
	{
		ConstantValue constantValue = source.ConstantValue;
		if (constantValue == null)
		{
			return false;
		}
		if (destination.NavTypeKind == NavTypeKind.Char && constantValue.IsString && constantValue.StringValue.Length == 1)
		{
			return true;
		}
		NavTypeKind navTypeKindSafe = source.Type.GetNavTypeKindSafe();
		if (navTypeKindSafe == NavTypeKind.Decimal)
		{
			decimal num = (constantValue.IsBad ? 0m : constantValue.DecimalValue);
			if (destination.IsIntegralType() && decimal.Truncate(num) == num)
			{
				switch (destination.GetNavTypeKindSafe())
				{
				case NavTypeKind.Byte:
					if (0m <= num)
					{
						return num <= 255m;
					}
					return false;
				case NavTypeKind.Char:
					if (-32768m <= num)
					{
						return num <= 32767m;
					}
					return false;
				case NavTypeKind.Integer:
					if (-2147483648m <= num)
					{
						return num <= 2147483647m;
					}
					return false;
				case NavTypeKind.BigInteger:
					if (-9223372036854775808m <= num)
					{
						return num <= 9223372036854775807m;
					}
					return false;
				default:
					return false;
				}
			}
		}
		if (navTypeKindSafe == NavTypeKind.Integer)
		{
			int num2 = ((!constantValue.IsBad) ? constantValue.Int32Value : 0);
			switch (destination.GetNavTypeKindSafe())
			{
			case NavTypeKind.Byte:
				if (0 <= num2)
				{
					return num2 <= 255;
				}
				return false;
			case NavTypeKind.Char:
				if (-32768 <= num2)
				{
					return num2 <= 32767;
				}
				return false;
			default:
				return false;
			}
		}
		return false;
	}

	public static Conversion FastClassifyConversion(TypeSymbol source, TypeSymbol target)
	{
		if (source.Equals(target))
		{
			return Conversion.Identity;
		}
		return new Conversion(ConversionEasyOut.ClassifyConversion(source, target));
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "builtinOnly")]
	public static Conversion ClassifyConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool builtinOnly = false)
	{
		DebugAssertHelper.Assert((object)source != null);
		DebugAssertHelper.Assert((object)destination != null);
		Conversion result = FastClassifyConversion(source, destination);
		if (result.Exists)
		{
			return result;
		}
		Conversion result2 = ClassifyImplicitBuiltInConversionSlow(source, destination, ref useSiteDiagnostics);
		if (result2.Exists)
		{
			return result2;
		}
		return Conversion.NoConversion;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "useSiteDiagnostics")]
	internal static Conversion ClassifyStandardImplicitConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert((object)source != null);
		DebugAssertHelper.Assert((object)destination != null);
		if (HasIdentityConversion(source, destination))
		{
			return Conversion.Identity;
		}
		if (HasImplicitNumericConversion(source, destination))
		{
			return Conversion.ImplicitNumeric;
		}
		if (HasImplicitNumericToSmallerConversion(source, destination))
		{
			return Conversion.ImplicitNumericToSmaller;
		}
		if (HasImplicitTextToSmallerConversion(source, destination))
		{
			return Conversion.ImplicitTextToSmaller;
		}
		if (HasImplicitTextConversion(source, destination))
		{
			return Conversion.ImplicitText;
		}
		if (HasImplicitRecordConversion(source, destination))
		{
			return Conversion.ImplicitRecord;
		}
		if (HasImplicitCodeunitConversion(source, destination))
		{
			return Conversion.ImplicitCodeunit;
		}
		if (HasToTextConversion(source, destination))
		{
			return Conversion.ToText;
		}
		if (HasToGuidConversion(source, destination))
		{
			return Conversion.ToGuid;
		}
		if (HasImplicitArrayConversion(source, destination, ref useSiteDiagnostics))
		{
			return Conversion.ImplicitArray;
		}
		if (HasImplicitOptionConversion(source, destination))
		{
			return Conversion.ImplicitOption;
		}
		if (IsToVariantConversion(source, destination))
		{
			return Conversion.ToVariant;
		}
		if (IsFromVariantConversion(source, destination))
		{
			return Conversion.FromVariant;
		}
		if (IsFromJokerConversion(source))
		{
			return Conversion.FromJoker;
		}
		if (IsToJokerConversion(source, destination))
		{
			return Conversion.ToJoker;
		}
		if (IsIdentityDotNetControlConversion(source, destination))
		{
			return Conversion.Identity;
		}
		if (IsUserControlToControlAddInConversion(source, destination))
		{
			return Conversion.Identity;
		}
		if (ConversionDotNet.IsDotNetConversion(source, destination))
		{
			return ConversionDotNet.ClassifyDotNetConversion(source, destination, ref useSiteDiagnostics);
		}
		return Conversion.NoConversion;
	}

	private static Conversion ClassifyImplicitBuiltInConversionSlow(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert((object)source != null);
		DebugAssertHelper.Assert((object)destination != null);
		if (source.NavTypeKind == NavTypeKind.None || destination.NavTypeKind == NavTypeKind.None)
		{
			return Conversion.NoConversion;
		}
		Conversion result = ClassifyStandardImplicitConversion(source, destination, ref useSiteDiagnostics);
		if (result.Exists)
		{
			return result;
		}
		return Conversion.NoConversion;
	}

	public static Conversion ClassifyImplicitConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert((object)source != null);
		DebugAssertHelper.Assert((object)destination != null);
		if (HasIdentityConversion(source, destination))
		{
			return Conversion.Identity;
		}
		Conversion result = FastClassifyConversion(source, destination);
		if (result.Exists)
		{
			if (!result.IsImplicit)
			{
				return Conversion.NoConversion;
			}
			return result;
		}
		Conversion result2 = ClassifyImplicitBuiltInConversionSlow(source, destination, ref useSiteDiagnostics);
		if (result2.Exists)
		{
			return result2;
		}
		return Conversion.NoConversion;
	}

	public static bool HasIdentityConversion(TypeSymbol source, TypeSymbol destination)
	{
		DebugAssertHelper.Assert((object)source != null);
		DebugAssertHelper.Assert((object)destination != null);
		if (!source.Equals(destination))
		{
			return HasBuiltInTextIdentityConversion(source, destination);
		}
		return true;
	}

	public static bool HasBuiltInTextIdentityConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source.IsTextType() && destination.IsTextType())
		{
			SimpleNavTypeSymbol simpleNavTypeSymbol = destination as SimpleNavTypeSymbol;
			if (simpleNavTypeSymbol != null)
			{
				return simpleNavTypeSymbol.NavTypeKind == source.NavTypeKind;
			}
			return false;
		}
		return false;
	}

	internal static int GetNumericTypeIndex(NavTypeKind navTypeKind)
	{
		return navTypeKind switch
		{
			NavTypeKind.Byte => 0, 
			NavTypeKind.Char => 1, 
			NavTypeKind.Integer => 2, 
			NavTypeKind.Option => 3, 
			NavTypeKind.BigInteger => 4, 
			NavTypeKind.Duration => 5, 
			NavTypeKind.Decimal => 6, 
			_ => -1, 
		};
	}

	private static bool HasImplicitNumericConversion(TypeSymbol source, TypeSymbol destination)
	{
		int numericTypeIndex = GetNumericTypeIndex(source.NavTypeKind);
		if (numericTypeIndex < 0)
		{
			return false;
		}
		int numericTypeIndex2 = GetNumericTypeIndex(destination.NavTypeKind);
		if (numericTypeIndex2 < 0)
		{
			return false;
		}
		return ImplicitNumericConversions[numericTypeIndex, numericTypeIndex2];
	}

	internal static bool HasImplicitNumericToSmallerConversion(TypeSymbol source, TypeSymbol destination)
	{
		int numericTypeIndex = GetNumericTypeIndex(source.NavTypeKind);
		if (numericTypeIndex < 0)
		{
			return false;
		}
		int numericTypeIndex2 = GetNumericTypeIndex(destination.NavTypeKind);
		if (numericTypeIndex2 < 0)
		{
			return false;
		}
		return ImplicitNumericToSmallerConversions[numericTypeIndex, numericTypeIndex2];
	}

	internal static bool HasToTextConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (destination.IsTextType())
		{
			if (source.IsTextType())
			{
				return true;
			}
			NavTypeKind navTypeKind = source.NavTypeKind;
			if (navTypeKind == NavTypeKind.Guid)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool HasImplicitTextConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (destination.IsTextType())
		{
			if (source.IsTextType() && destination.NavTypeKind == source.NavTypeKind)
			{
				return true;
			}
			NavTypeKind navTypeKind = source.NavTypeKind;
			if (navTypeKind == NavTypeKind.String || navTypeKind == NavTypeKind.Char)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool HasImplicitTextToSmallerConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (destination.HasLength && source.IsTextType())
		{
			int length = source.Length;
			if (destination.Length < length)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool HasImplicitRecordConversion(TypeSymbol source, TypeSymbol destination)
	{
		if ((source.NavTypeKind == NavTypeKind.Table && destination.NavTypeKind == NavTypeKind.Record) || (source.NavTypeKind == NavTypeKind.Record && destination.NavTypeKind == NavTypeKind.Table))
		{
			return true;
		}
		if (source.NavTypeKind == NavTypeKind.Record && destination.NavTypeKind == NavTypeKind.Record)
		{
			return (source as RecordTypeSymbol).Equals(destination, ignoreTemporary: true);
		}
		return false;
	}

	internal static bool HasImplicitCodeunitConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source.NavTypeKind == NavTypeKind.Codeunit && destination.NavTypeKind == NavTypeKind.AnyCodeunit)
		{
			return true;
		}
		return false;
	}

	private static bool HasToGuidConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (destination.NavTypeKind == NavTypeKind.Guid && source.IsTextType())
		{
			return true;
		}
		return false;
	}

	private static bool HasImplicitOptionConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source.NavTypeKind == NavTypeKind.Option && destination.NavTypeKind == NavTypeKind.Option)
		{
			return true;
		}
		return false;
	}

	private static bool HasImplicitArrayConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.IsArray() || !destination.IsArray())
		{
			return false;
		}
		ArrayTypeSymbol arrayTypeSymbol = source as ArrayTypeSymbol;
		ArrayTypeSymbol arrayTypeSymbol2 = destination as ArrayTypeSymbol;
		if (arrayTypeSymbol2.ElementType.NavTypeKind == NavTypeKind.Joker)
		{
			return true;
		}
		Conversion conversion = ClassifyStandardImplicitConversion(arrayTypeSymbol.ElementType, arrayTypeSymbol2.ElementType, ref useSiteDiagnostics);
		if (!conversion.Kind.IsValidForArrayConversion() && !IsToBuiltinStringArrayConversion(conversion.Kind, arrayTypeSymbol2.ElementType.NavTypeKind))
		{
			return false;
		}
		if (arrayTypeSymbol.Dimensions.Length != arrayTypeSymbol2.Dimensions.Length && arrayTypeSymbol2.Dimensions.Length != 0)
		{
			return false;
		}
		return true;
	}

	private static bool IsToBuiltinStringArrayConversion(ConversionKind conversionKind, NavTypeKind destinationType)
	{
		if (conversionKind == ConversionKind.ToText)
		{
			return destinationType == NavTypeKind.String;
		}
		return false;
	}

	internal static bool IsFromJokerConversion(TypeSymbol source)
	{
		if (source.GetNavTypeKindSafe() == NavTypeKind.Joker)
		{
			return !source.IsArray();
		}
		return false;
	}

	internal static bool IsToJokerConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (destination.GetNavTypeKindSafe() == NavTypeKind.Joker && source.GetNavTypeKindSafe() != NavTypeKind.None && !source.IsArray())
		{
			return source.Kind != SymbolKind.Class;
		}
		return false;
	}

	internal static bool IsToVariantConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (destination.GetNavTypeKindSafe() == NavTypeKind.Variant && source.GetNavTypeKindSafe() != NavTypeKind.None && !source.IsArray())
		{
			return source.Kind != SymbolKind.Class;
		}
		return false;
	}

	internal static bool IsFromVariantConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source.GetNavTypeKindSafe() == NavTypeKind.Variant)
		{
			return IsSupportedVariantType(destination.GetNavTypeKindSafe());
		}
		return false;
	}

	internal static bool IsDotNetConversion(TypeSymbol source, TypeSymbol destination)
	{
		return ConversionDotNet.IsDotNetConversion(source, destination);
	}

	internal static Conversion ClassifyDotNetByRefConversion(TypeSymbol source, TypeSymbol byRefTarget)
	{
		return ConversionDotNet.ClassifyByRefConversion(source, byRefTarget);
	}

	internal static bool IsIdentityDotNetControlConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source.Kind == SymbolKind.Control && ((ControlSymbol)source).ControlKind == ControlKind.UserControl)
		{
			return destination.GetNavTypeKindSafe() == NavTypeKind.DotNet;
		}
		return false;
	}

	internal static bool IsUserControlToControlAddInConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (!source.IsKind(SymbolKind.Control) || !destination.IsKind(SymbolKind.ControlAddIn))
		{
			return false;
		}
		ControlSymbol controlSymbol = (ControlSymbol)source;
		if (controlSymbol.ControlKind != ControlKind.UserControl)
		{
			return false;
		}
		return destination.Equals(controlSymbol.RelatedControlAddInSymbol);
	}

	public static bool IsNumericType(NavTypeKind navTypeKind)
	{
		if ((uint)(navTypeKind - 3014659) <= 1u || (uint)(navTypeKind - 12451845) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsIntegralType(NavTypeKind navTypeKind)
	{
		if ((uint)(navTypeKind - 3014659) <= 1u || (uint)(navTypeKind - 12451845) <= 1u)
		{
			return true;
		}
		return false;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502")]
	public static bool IsSupportedVariantType(NavTypeKind navTypeKind)
	{
		switch (navTypeKind)
		{
		case NavTypeKind.Table:
		case NavTypeKind.RecordRef:
		case NavTypeKind.FieldRef:
		case NavTypeKind.KeyRef:
		case NavTypeKind.InStream:
		case NavTypeKind.OutStream:
		case NavTypeKind.File:
		case NavTypeKind.FilterPageBuilder:
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
		case NavTypeKind.JsonToken:
		case NavTypeKind.JsonObject:
		case NavTypeKind.JsonArray:
		case NavTypeKind.JsonValue:
		case NavTypeKind.TextBuilder:
		case NavTypeKind.XmlAttribute:
		case NavTypeKind.XmlAttributeCollection:
		case NavTypeKind.XmlComment:
		case NavTypeKind.XmlCData:
		case NavTypeKind.XmlDeclaration:
		case NavTypeKind.XmlDocument:
		case NavTypeKind.XmlDocumentType:
		case NavTypeKind.XmlElement:
		case NavTypeKind.XmlNamespaceManager:
		case NavTypeKind.XmlNameTable:
		case NavTypeKind.XmlNode:
		case NavTypeKind.XmlNodeList:
		case NavTypeKind.XmlProcessingInstruction:
		case NavTypeKind.XmlText:
		case NavTypeKind.XmlReadOptions:
		case NavTypeKind.XmlWriteOptions:
		case NavTypeKind.Action:
		case NavTypeKind.TransactionType:
		case NavTypeKind.String:
		case NavTypeKind.BigText:
		case NavTypeKind.Byte:
		case NavTypeKind.Char:
		case NavTypeKind.Blob:
		case NavTypeKind.DateFormula:
		case NavTypeKind.RecordId:
		case NavTypeKind.Boolean:
		case NavTypeKind.Integer:
		case NavTypeKind.BigInteger:
		case NavTypeKind.Decimal:
		case NavTypeKind.Option:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
		case NavTypeKind.Guid:
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return true;
		default:
			return false;
		}
	}
}
