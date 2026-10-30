using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class ConversionsBase
{
	private static class ConversionEasyOut
	{
		private static readonly byte[,] s_convkind;

		static ConversionEasyOut()
		{
			s_convkind = new byte[28, 28]
			{
				{
					1, 24, 25, 25, 25, 25, 25, 25, 25, 25,
					25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
					25, 25, 25, 25, 25, 25, 25, 25
				},
				{
					11, 1, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0
				},
				{
					12, 0, 1, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 9, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0
				},
				{
					12, 0, 0, 1, 21, 21, 2, 2, 21, 2,
					2, 2, 2, 2, 2, 0, 9, 23, 23, 9,
					9, 23, 9, 9, 9, 9, 9, 9
				},
				{
					12, 0, 0, 21, 1, 2, 2, 2, 21, 21,
					21, 21, 2, 2, 2, 0, 23, 9, 9, 9,
					9, 23, 23, 23, 23, 9, 9, 9
				},
				{
					12, 0, 0, 21, 21, 1, 2, 2, 21, 21,
					21, 21, 2, 2, 2, 0, 23, 23, 9, 9,
					9, 23, 23, 23, 23, 9, 9, 9
				},
				{
					12, 0, 0, 21, 21, 21, 1, 2, 21, 21,
					21, 21, 2, 2, 2, 0, 23, 23, 23, 9,
					9, 23, 23, 23, 23, 9, 9, 9
				},
				{
					12, 0, 0, 21, 21, 21, 21, 1, 21, 21,
					21, 21, 2, 2, 2, 0, 23, 23, 23, 23,
					9, 23, 23, 23, 23, 9, 9, 9
				},
				{
					12, 0, 0, 21, 21, 2, 2, 2, 1, 2,
					2, 2, 2, 2, 2, 0, 23, 23, 9, 9,
					9, 9, 9, 9, 9, 9, 9, 9
				},
				{
					12, 0, 0, 21, 21, 21, 2, 2, 21, 1,
					2, 2, 2, 2, 2, 0, 23, 23, 23, 9,
					9, 23, 9, 9, 9, 9, 9, 9
				},
				{
					12, 0, 0, 21, 21, 21, 21, 2, 21, 21,
					1, 2, 2, 2, 2, 0, 23, 23, 23, 23,
					9, 23, 23, 9, 9, 9, 9, 9
				},
				{
					12, 0, 0, 21, 21, 21, 21, 21, 21, 21,
					21, 1, 2, 2, 2, 0, 23, 23, 23, 23,
					23, 23, 23, 23, 9, 9, 9, 9
				},
				{
					12, 0, 0, 21, 21, 21, 21, 21, 21, 21,
					21, 21, 1, 2, 21, 0, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 9, 9, 23
				},
				{
					12, 0, 0, 21, 21, 21, 21, 21, 21, 21,
					21, 21, 21, 1, 21, 0, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 23, 9, 23
				},
				{
					12, 0, 0, 21, 21, 21, 21, 21, 21, 21,
					21, 21, 21, 21, 1, 0, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 23, 23, 9
				},
				{
					12, 0, 23, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 1, 23, 23, 9,
					9, 23, 9, 9, 9, 9, 9, 9
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 1, 9, 9,
					9, 23, 23, 23, 23, 9, 9, 9
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 1, 9,
					9, 23, 23, 23, 23, 9, 9, 9
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 23, 1,
					9, 23, 23, 23, 23, 9, 9, 9
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 23, 23,
					1, 23, 23, 23, 23, 9, 9, 9
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 9, 9,
					9, 1, 9, 9, 9, 9, 9, 9
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 23, 9,
					9, 23, 1, 9, 9, 9, 9, 9
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 23, 23,
					9, 23, 23, 1, 9, 9, 9, 9
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 23, 23,
					23, 23, 23, 23, 1, 9, 9, 9
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 1, 9, 23
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 23, 1, 23
				},
				{
					12, 0, 0, 23, 23, 23, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 0, 23, 23, 23, 23,
					23, 23, 23, 23, 23, 23, 23, 1
				}
			};
		}

		private static int TypeToIndex(TypeSymbol type)
		{
			switch (type.GetSpecialTypeSafe())
			{
			case SpecialType.System_Object:
				return 0;
			case SpecialType.System_String:
				return 1;
			case SpecialType.System_Boolean:
				return 2;
			case SpecialType.System_Char:
				return 3;
			case SpecialType.System_SByte:
				return 4;
			case SpecialType.System_Int16:
				return 5;
			case SpecialType.System_Int32:
				return 6;
			case SpecialType.System_Int64:
				return 7;
			case SpecialType.System_Byte:
				return 8;
			case SpecialType.System_UInt16:
				return 9;
			case SpecialType.System_UInt32:
				return 10;
			case SpecialType.System_UInt64:
				return 11;
			case SpecialType.System_Single:
				return 12;
			case SpecialType.System_Double:
				return 13;
			case SpecialType.System_Decimal:
				return 14;
			case SpecialType.None:
				if ((object)type != null && type.IsNullableType())
				{
					switch (type.GetNullableUnderlyingType().GetSpecialTypeSafe())
					{
					case SpecialType.System_Boolean:
						return 15;
					case SpecialType.System_Char:
						return 16;
					case SpecialType.System_SByte:
						return 17;
					case SpecialType.System_Int16:
						return 18;
					case SpecialType.System_Int32:
						return 19;
					case SpecialType.System_Int64:
						return 20;
					case SpecialType.System_Byte:
						return 21;
					case SpecialType.System_UInt16:
						return 22;
					case SpecialType.System_UInt32:
						return 23;
					case SpecialType.System_UInt64:
						return 24;
					case SpecialType.System_Single:
						return 25;
					case SpecialType.System_Double:
						return 26;
					case SpecialType.System_Decimal:
						return 27;
					}
				}
				break;
			}
			return -1;
		}

		public static ConversionKind ClassifyConversion(TypeSymbol source, TypeSymbol target)
		{
			int num = TypeToIndex(source);
			if (num < 0)
			{
				return ConversionKind.NoConversion;
			}
			int num2 = TypeToIndex(target);
			if (num2 < 0)
			{
				return ConversionKind.NoConversion;
			}
			return (ConversionKind)s_convkind[num, num2];
		}
	}

	private delegate Conversion ClassifyConversionFromExpressionDelegate(ConversionsBase conversions, BoundExpression sourceExpression, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool arg);

	private delegate Conversion ClassifyConversionFromTypeDelegate(ConversionsBase conversions, TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool arg);

	private const int MaximumRecursionDepth = 50;

	protected readonly AssemblySymbol corLibrary;

	private readonly int _currentRecursionDepth;

	private const bool F = false;

	private const bool T = true;

	private static readonly bool[,] s_implicitNumericConversions = new bool[12, 12]
	{
		{
			false, false, true, false, true, false, true, false, false, true,
			true, true
		},
		{
			false, false, true, true, true, true, true, true, false, true,
			true, true
		},
		{
			false, false, false, false, true, false, true, false, false, true,
			true, true
		},
		{
			false, false, false, false, true, true, true, true, false, true,
			true, true
		},
		{
			false, false, false, false, false, false, true, false, false, true,
			true, true
		},
		{
			false, false, false, false, false, false, true, true, false, true,
			true, true
		},
		{
			false, false, false, false, false, false, false, false, false, true,
			true, true
		},
		{
			false, false, false, false, false, false, false, false, false, true,
			true, true
		},
		{
			false, false, false, true, true, true, true, true, false, true,
			true, true
		},
		{
			false, false, false, false, false, false, false, false, false, false,
			true, false
		},
		{
			false, false, false, false, false, false, false, false, false, false,
			false, false
		},
		{
			false, false, false, false, false, false, false, false, false, false,
			false, false
		}
	};

	private static readonly bool[,] s_explicitNumericConversions = new bool[12, 12]
	{
		{
			false, true, false, true, false, true, false, true, true, false,
			false, false
		},
		{
			true, false, false, false, false, false, false, false, true, false,
			false, false
		},
		{
			true, true, false, true, false, true, false, true, true, false,
			false, false
		},
		{
			true, true, true, false, false, false, false, false, true, false,
			false, false
		},
		{
			true, true, true, true, false, true, false, true, true, false,
			false, false
		},
		{
			true, true, true, true, true, false, false, false, true, false,
			false, false
		},
		{
			true, true, true, true, true, true, false, true, true, false,
			false, false
		},
		{
			true, true, true, true, true, true, true, false, true, false,
			false, false
		},
		{
			true, true, true, false, false, false, false, false, false, false,
			false, false
		},
		{
			true, true, true, true, true, true, true, true, true, false,
			false, true
		},
		{
			true, true, true, true, true, true, true, true, true, true,
			false, true
		},
		{
			true, true, true, true, true, true, true, true, true, true,
			true, false
		}
	};

	internal AssemblySymbol CorLibrary => corLibrary;

	protected ConversionsBase(AssemblySymbol corLibrary, int currentRecursionDepth)
	{
		this.corLibrary = corLibrary;
		_currentRecursionDepth = currentRecursionDepth;
	}

	public abstract Conversion GetMethodGroupConversion(BoundMethodGroup source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics);

	public abstract Conversion GetStackAllocConversion(BoundStackAllocArrayCreation sourceExpression, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics);

	protected abstract ConversionsBase CreateInstance(int currentRecursionDepth);

	protected abstract Conversion GetInterpolatedStringConversion(BoundInterpolatedString source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics);

	public Conversion ClassifyImplicitConversionFromExpression(BoundExpression sourceExpression, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol type = sourceExpression.Type;
		if ((object)type != null && HasIdentityConversion(type, destination))
		{
			return Conversion.Identity;
		}
		Conversion result = ClassifyImplicitBuiltInConversionFromExpression(sourceExpression, type, destination, ref useSiteDiagnostics);
		if (result.Exists)
		{
			return result;
		}
		if ((object)type != null)
		{
			Conversion result2 = FastClassifyConversion(type, destination);
			if (result2.Exists)
			{
				if (!result2.IsImplicit)
				{
					return Conversion.NoConversion;
				}
				return result2;
			}
			result = ClassifyImplicitBuiltInConversionSlow(type, destination, ref useSiteDiagnostics);
			if (result.Exists)
			{
				return result;
			}
		}
		return GetImplicitUserDefinedConversion(sourceExpression, type, destination, ref useSiteDiagnostics);
	}

	public Conversion ClassifyImplicitConversionFromType(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
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
		return GetImplicitUserDefinedConversion(null, source, destination, ref useSiteDiagnostics);
	}

	public Conversion ClassifyConversionFromExpressionType(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (HasImplicitDynamicConversionFromExpression(source, destination))
		{
			return Conversion.ImplicitDynamic;
		}
		return ClassifyConversionFromType(source, destination, ref useSiteDiagnostics);
	}

	public Conversion ClassifyConversionFromExpression(BoundExpression sourceExpression, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool forCast = false)
	{
		if (forCast)
		{
			return ClassifyConversionFromExpressionForCast(sourceExpression, destination, ref useSiteDiagnostics);
		}
		Conversion result = ClassifyImplicitConversionFromExpression(sourceExpression, destination, ref useSiteDiagnostics);
		if (result.Exists)
		{
			return result;
		}
		return ClassifyExplicitOnlyConversionFromExpression(sourceExpression, destination, ref useSiteDiagnostics, forCast: false);
	}

	public Conversion ClassifyConversionFromType(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool forCast = false)
	{
		if (forCast)
		{
			return ClassifyConversionFromTypeForCast(source, destination, ref useSiteDiagnostics);
		}
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
		Conversion implicitUserDefinedConversion = GetImplicitUserDefinedConversion(null, source, destination, ref useSiteDiagnostics);
		if (implicitUserDefinedConversion.Exists)
		{
			return implicitUserDefinedConversion;
		}
		implicitUserDefinedConversion = ClassifyExplicitBuiltInOnlyConversion(source, destination, ref useSiteDiagnostics, forCast: false);
		if (implicitUserDefinedConversion.Exists)
		{
			return implicitUserDefinedConversion;
		}
		return GetExplicitUserDefinedConversion(null, source, destination, ref useSiteDiagnostics);
	}

	private Conversion ClassifyConversionFromExpressionForCast(BoundExpression source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion conversion = ClassifyImplicitConversionFromExpression(source, destination, ref useSiteDiagnostics);
		if (conversion.Exists && !ExplicitConversionMayDifferFromImplicit(conversion))
		{
			return conversion;
		}
		Conversion result = ClassifyExplicitOnlyConversionFromExpression(source, destination, ref useSiteDiagnostics, forCast: true);
		if (result.Exists)
		{
			return result;
		}
		if (!conversion.Exists)
		{
			return Conversion.NoConversion;
		}
		return conversion;
	}

	private Conversion ClassifyConversionFromTypeForCast(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion result = FastClassifyConversion(source, destination);
		if (result.Exists)
		{
			return result;
		}
		Conversion conversion = ClassifyImplicitBuiltInConversionSlow(source, destination, ref useSiteDiagnostics);
		if (conversion.Exists && !ExplicitConversionMayDifferFromImplicit(conversion))
		{
			return conversion;
		}
		Conversion result2 = ClassifyExplicitBuiltInOnlyConversion(source, destination, ref useSiteDiagnostics, forCast: true);
		if (result2.Exists)
		{
			return result2;
		}
		if (conversion.Exists)
		{
			return conversion;
		}
		Conversion explicitUserDefinedConversion = GetExplicitUserDefinedConversion(null, source, destination, ref useSiteDiagnostics);
		if (explicitUserDefinedConversion.Exists)
		{
			return explicitUserDefinedConversion;
		}
		return GetImplicitUserDefinedConversion(null, source, destination, ref useSiteDiagnostics);
	}

	public static Conversion FastClassifyConversion(TypeSymbol source, TypeSymbol target)
	{
		ConversionKind conversionKind = ConversionEasyOut.ClassifyConversion(source, target);
		if (conversionKind != ConversionKind.ImplicitNullable && conversionKind != ConversionKind.ExplicitNullable)
		{
			return Conversion.GetTrivialConversion(conversionKind);
		}
		return Conversion.MakeNullableConversion(conversionKind, FastClassifyConversion(source.StrippedType(), target.StrippedType()));
	}

	public Conversion ClassifyBuiltInConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
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
		return ClassifyExplicitBuiltInOnlyConversion(source, destination, ref useSiteDiagnostics, forCast: false);
	}

	public Conversion ClassifyStandardConversion(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion result = ClassifyStandardImplicitConversion(sourceExpression, source, destination, ref useSiteDiagnostics);
		if (result.Exists)
		{
			return result;
		}
		if ((object)source != null)
		{
			return DeriveStandardExplicitFromOppositeStandardImplicitConversion(source, destination, ref useSiteDiagnostics);
		}
		return Conversion.NoConversion;
	}

	private Conversion ClassifyStandardImplicitConversion(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion result = ClassifyImplicitBuiltInConversionFromExpression(sourceExpression, source, destination, ref useSiteDiagnostics);
		if (result.Exists)
		{
			return result;
		}
		if ((object)source != null)
		{
			return ClassifyStandardImplicitConversion(source, destination, ref useSiteDiagnostics);
		}
		return Conversion.NoConversion;
	}

	private Conversion ClassifyStandardImplicitConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (HasIdentityConversion(source, destination))
		{
			return Conversion.Identity;
		}
		if (HasImplicitNumericConversion(source, destination))
		{
			return Conversion.ImplicitNumeric;
		}
		Conversion result = ClassifyImplicitNullableConversion(source, destination, ref useSiteDiagnostics);
		if (result.Exists)
		{
			return result;
		}
		if (HasImplicitReferenceConversion(source, destination, ref useSiteDiagnostics))
		{
			return Conversion.ImplicitReference;
		}
		if (HasBoxingConversion(source, destination, ref useSiteDiagnostics))
		{
			return Conversion.Boxing;
		}
		if (HasImplicitPointerConversion(source, destination))
		{
			return Conversion.PointerToVoid;
		}
		Conversion result2 = ClassifyImplicitTupleConversion(source, destination, ref useSiteDiagnostics);
		if (result2.Exists)
		{
			return result2;
		}
		return Conversion.NoConversion;
	}

	private Conversion ClassifyImplicitBuiltInConversionSlow(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.SpecialType == SpecialType.System_Void || destination.SpecialType == SpecialType.System_Void)
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

	private Conversion GetImplicitUserDefinedConversion(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return new Conversion(AnalyzeImplicitUserDefinedConversions(sourceExpression, source, destination, ref useSiteDiagnostics), isImplicit: true);
	}

	private Conversion ClassifyExplicitBuiltInOnlyConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool forCast)
	{
		if (source.SpecialType == SpecialType.System_Void || destination.SpecialType == SpecialType.System_Void)
		{
			return Conversion.NoConversion;
		}
		if (HasSpecialIntPtrConversion(source, destination))
		{
			return Conversion.IntPtr;
		}
		if (HasExplicitEnumerationConversion(source, destination))
		{
			return Conversion.ExplicitEnumeration;
		}
		Conversion result = ClassifyExplicitNullableConversion(source, destination, ref useSiteDiagnostics, forCast);
		if (result.Exists)
		{
			return result;
		}
		if (HasExplicitReferenceConversion(source, destination, ref useSiteDiagnostics))
		{
			if (source.Kind != SymbolKind.DynamicType)
			{
				return Conversion.ExplicitReference;
			}
			return Conversion.ExplicitDynamic;
		}
		if (HasUnboxingConversion(source, destination, ref useSiteDiagnostics))
		{
			return Conversion.Unboxing;
		}
		Conversion result2 = ClassifyExplicitTupleConversion(source, destination, ref useSiteDiagnostics, forCast);
		if (result2.Exists)
		{
			return result2;
		}
		if (HasPointerToPointerConversion(source, destination))
		{
			return Conversion.PointerToPointer;
		}
		if (HasPointerToIntegerConversion(source, destination))
		{
			return Conversion.PointerToInteger;
		}
		if (HasIntegerToPointerConversion(source, destination))
		{
			return Conversion.IntegerToPointer;
		}
		if (HasExplicitDynamicConversion(source, destination))
		{
			return Conversion.ExplicitDynamic;
		}
		return Conversion.NoConversion;
	}

	private Conversion GetExplicitUserDefinedConversion(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return new Conversion(AnalyzeExplicitUserDefinedConversions(sourceExpression, source, destination, ref useSiteDiagnostics), isImplicit: false);
	}

	private Conversion DeriveStandardExplicitFromOppositeStandardImplicitConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion conversion = ClassifyStandardImplicitConversion(destination, source, ref useSiteDiagnostics);
		switch (conversion.Kind)
		{
		case ConversionKind.Identity:
			return Conversion.Identity;
		case ConversionKind.ImplicitNumeric:
			return Conversion.ExplicitNumeric;
		case ConversionKind.ImplicitReference:
			return Conversion.ExplicitReference;
		case ConversionKind.Boxing:
			return Conversion.Unboxing;
		case ConversionKind.NoConversion:
			return Conversion.NoConversion;
		case ConversionKind.PointerToVoid:
			return Conversion.PointerToPointer;
		case ConversionKind.ImplicitTuple:
			return Conversion.NoConversion;
		case ConversionKind.ImplicitNullable:
		{
			TypeSymbol source2 = source.StrippedType();
			TypeSymbol destination2 = destination.StrippedType();
			Conversion nestedConversion = DeriveStandardExplicitFromOppositeStandardImplicitConversion(source2, destination2, ref useSiteDiagnostics);
			return nestedConversion.Exists ? Conversion.MakeNullableConversion(ConversionKind.ExplicitNullable, nestedConversion) : Conversion.NoConversion;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(conversion.Kind);
		}
	}

	public static bool IsBaseInterface(TypeSymbol baseType, TypeSymbol derivedType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!baseType.IsInterfaceType())
		{
			return false;
		}
		if (!(derivedType is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		foreach (NamedTypeSymbol item in namedTypeSymbol.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics))
		{
			if (HasIdentityConversion(item, baseType))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBaseClass(TypeSymbol derivedType, TypeSymbol baseType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!baseType.IsClassType())
		{
			return false;
		}
		TypeSymbol typeSymbol = derivedType.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		while ((object)typeSymbol != null)
		{
			if (HasIdentityConversion(typeSymbol, baseType))
			{
				return true;
			}
			typeSymbol = typeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		return false;
	}

	private static bool ExplicitConversionMayDifferFromImplicit(Conversion implicitConversion)
	{
		switch (implicitConversion.Kind)
		{
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ImplicitNullable:
		case ConversionKind.ImplicitDynamic:
		case ConversionKind.ImplicitUserDefined:
			return true;
		default:
			return false;
		}
	}

	private Conversion ClassifyImplicitBuiltInConversionFromExpression(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (HasImplicitDynamicConversionFromExpression(source, destination))
		{
			return Conversion.ImplicitDynamic;
		}
		if (sourceExpression == null)
		{
			return Conversion.NoConversion;
		}
		if (HasImplicitEnumerationConversion(sourceExpression, destination))
		{
			return Conversion.ImplicitEnumeration;
		}
		Conversion result = ClassifyImplicitConstantExpressionConversion(sourceExpression, destination);
		if (result.Exists)
		{
			return result;
		}
		switch (sourceExpression.Kind)
		{
		case BoundKind.Literal:
		{
			Conversion result3 = ClassifyNullLiteralConversion(sourceExpression, destination);
			if (result3.Exists)
			{
				return result3;
			}
			break;
		}
		case BoundKind.DefaultExpression:
			if ((object)((BoundDefaultExpression)sourceExpression).Type == null)
			{
				return Conversion.DefaultOrNullLiteral;
			}
			break;
		case BoundKind.TupleLiteral:
		{
			Conversion result2 = ClassifyImplicitTupleLiteralConversion((BoundTupleLiteral)sourceExpression, destination, ref useSiteDiagnostics);
			if (result2.Exists)
			{
				return result2;
			}
			break;
		}
		case BoundKind.UnboundLambda:
			if (HasAnonymousFunctionConversion(sourceExpression, destination))
			{
				return Conversion.AnonymousFunction;
			}
			break;
		case BoundKind.MethodGroup:
		{
			Conversion methodGroupConversion = GetMethodGroupConversion((BoundMethodGroup)sourceExpression, destination, ref useSiteDiagnostics);
			if (methodGroupConversion.Exists)
			{
				return methodGroupConversion;
			}
			break;
		}
		case BoundKind.InterpolatedString:
		{
			Conversion interpolatedStringConversion = GetInterpolatedStringConversion((BoundInterpolatedString)sourceExpression, destination, ref useSiteDiagnostics);
			if (interpolatedStringConversion.Exists)
			{
				return interpolatedStringConversion;
			}
			break;
		}
		case BoundKind.StackAllocArrayCreation:
		{
			Conversion stackAllocConversion = GetStackAllocConversion((BoundStackAllocArrayCreation)sourceExpression, destination, ref useSiteDiagnostics);
			if (stackAllocConversion.Exists)
			{
				return stackAllocConversion;
			}
			break;
		}
		case BoundKind.ThrowExpression:
			return Conversion.ImplicitThrow;
		}
		return Conversion.NoConversion;
	}

	private static Conversion ClassifyNullLiteralConversion(BoundExpression source, TypeSymbol destination)
	{
		if (!source.IsLiteralNull())
		{
			return Conversion.NoConversion;
		}
		if (destination.IsNullableType())
		{
			return Conversion.DefaultOrNullLiteral;
		}
		if (destination.IsReferenceType)
		{
			return Conversion.ImplicitReference;
		}
		if (destination is PointerTypeSymbol)
		{
			return Conversion.NullToPointer;
		}
		return Conversion.NoConversion;
	}

	private static Conversion ClassifyImplicitConstantExpressionConversion(BoundExpression source, TypeSymbol destination)
	{
		if (HasImplicitConstantExpressionConversion(source, destination))
		{
			return Conversion.ImplicitConstant;
		}
		if (destination.Kind == SymbolKind.NamedType)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)destination;
			if (namedTypeSymbol.OriginalDefinition.GetSpecialTypeSafe() == SpecialType.System_Nullable_T && HasImplicitConstantExpressionConversion(source, namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[0]))
			{
				return new Conversion(ConversionKind.ImplicitNullable, Conversion.ImplicitConstantUnderlying);
			}
		}
		return Conversion.NoConversion;
	}

	private Conversion ClassifyImplicitTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion implicitTupleLiteralConversion = GetImplicitTupleLiteralConversion(source, destination, ref useSiteDiagnostics);
		if (implicitTupleLiteralConversion.Exists)
		{
			return implicitTupleLiteralConversion;
		}
		if (destination.Kind == SymbolKind.NamedType)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)destination;
			if (namedTypeSymbol.OriginalDefinition.GetSpecialTypeSafe() == SpecialType.System_Nullable_T)
			{
				Conversion implicitTupleLiteralConversion2 = GetImplicitTupleLiteralConversion(source, namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[0], ref useSiteDiagnostics);
				if (implicitTupleLiteralConversion2.Exists)
				{
					return new Conversion(ConversionKind.ImplicitNullable, ImmutableArray.Create(implicitTupleLiteralConversion2));
				}
			}
		}
		return Conversion.NoConversion;
	}

	private Conversion ClassifyExplicitTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool forCast)
	{
		Conversion explicitTupleLiteralConversion = GetExplicitTupleLiteralConversion(source, destination, ref useSiteDiagnostics, forCast);
		if (explicitTupleLiteralConversion.Exists)
		{
			return explicitTupleLiteralConversion;
		}
		if (destination.Kind == SymbolKind.NamedType)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)destination;
			if (namedTypeSymbol.OriginalDefinition.GetSpecialTypeSafe() == SpecialType.System_Nullable_T)
			{
				Conversion explicitTupleLiteralConversion2 = GetExplicitTupleLiteralConversion(source, namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[0], ref useSiteDiagnostics, forCast);
				if (explicitTupleLiteralConversion2.Exists)
				{
					return new Conversion(ConversionKind.ExplicitNullable, ImmutableArray.Create(explicitTupleLiteralConversion2));
				}
			}
		}
		return Conversion.NoConversion;
	}

	internal static bool HasImplicitConstantExpressionConversion(BoundExpression source, TypeSymbol destination)
	{
		ConstantValue constantValue = source.ConstantValue;
		if (constantValue == null || (object)source.Type == null)
		{
			return false;
		}
		switch (source.Type.GetSpecialTypeSafe())
		{
		case SpecialType.System_Int32:
		{
			int num = ((!constantValue.IsBad) ? constantValue.Int32Value : 0);
			switch (destination.GetSpecialTypeSafe())
			{
			case SpecialType.System_Byte:
				if (0 <= num)
				{
					return num <= 255;
				}
				return false;
			case SpecialType.System_SByte:
				if (-128 <= num)
				{
					return num <= 127;
				}
				return false;
			case SpecialType.System_Int16:
				if (-32768 <= num)
				{
					return num <= 32767;
				}
				return false;
			case SpecialType.System_UInt32:
				return 0L <= (long)num;
			case SpecialType.System_UInt64:
				return 0 <= num;
			case SpecialType.System_UInt16:
				if (0 <= num)
				{
					return num <= 65535;
				}
				return false;
			default:
				return false;
			}
		}
		case SpecialType.System_Int64:
			if (destination.GetSpecialTypeSafe() == SpecialType.System_UInt64 && (constantValue.IsBad || 0 <= constantValue.Int64Value))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private Conversion ClassifyExplicitOnlyConversionFromExpression(BoundExpression sourceExpression, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool forCast)
	{
		TypeSymbol type = sourceExpression.Type;
		if (sourceExpression.Kind == BoundKind.TupleLiteral)
		{
			Conversion result = ClassifyExplicitTupleLiteralConversion((BoundTupleLiteral)sourceExpression, destination, ref useSiteDiagnostics, forCast);
			if (result.Exists)
			{
				return result;
			}
		}
		if ((object)type != null)
		{
			Conversion result2 = FastClassifyConversion(type, destination);
			if (result2.Exists)
			{
				return result2;
			}
			Conversion result3 = ClassifyExplicitBuiltInOnlyConversion(type, destination, ref useSiteDiagnostics, forCast);
			if (result3.Exists)
			{
				return result3;
			}
		}
		return GetExplicitUserDefinedConversion(sourceExpression, type, destination, ref useSiteDiagnostics);
	}

	private static bool HasImplicitEnumerationConversion(BoundExpression source, TypeSymbol destination)
	{
		if (!destination.IsEnumType() && (!destination.IsNullableType() || !destination.GetNullableUnderlyingType().IsEnumType()))
		{
			return false;
		}
		ConstantValue constantValue = source.ConstantValue;
		if (constantValue != null && IsNumericType(source.Type.GetSpecialTypeSafe()))
		{
			return IsConstantNumericZero(constantValue);
		}
		return false;
	}

	private static LambdaConversionResult IsAnonymousFunctionCompatibleWithDelegate(UnboundLambda anonymousFunction, TypeSymbol type)
	{
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
		MethodSymbol delegateInvokeMethod = namedTypeSymbol.DelegateInvokeMethod;
		if ((object)delegateInvokeMethod == null || delegateInvokeMethod.HasUseSiteError)
		{
			return LambdaConversionResult.BadTargetType;
		}
		ImmutableArray<ParameterSymbol> parameters = delegateInvokeMethod.Parameters;
		if (anonymousFunction.HasSignature)
		{
			if (anonymousFunction.ParameterCount != delegateInvokeMethod.ParameterCount)
			{
				return LambdaConversionResult.BadParameterCount;
			}
			if (anonymousFunction.HasExplicitlyTypedParameterList)
			{
				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i].RefKind != anonymousFunction.RefKind(i) || !parameters[i].Type.Equals(anonymousFunction.ParameterType(i), TypeCompareKind.AllIgnoreOptions))
					{
						return LambdaConversionResult.MismatchedParameterType;
					}
				}
			}
			else
			{
				for (int j = 0; j < parameters.Length; j++)
				{
					if (parameters[j].RefKind != RefKind.None)
					{
						return LambdaConversionResult.RefInImplicitlyTypedLambda;
					}
				}
				for (int k = 0; k < parameters.Length; k++)
				{
					if (parameters[k].Type.IsStatic)
					{
						return LambdaConversionResult.StaticTypeInImplicitlyTypedLambda;
					}
				}
			}
		}
		else
		{
			for (int l = 0; l < parameters.Length; l++)
			{
				if (parameters[l].RefKind == RefKind.Out)
				{
					return LambdaConversionResult.MissingSignatureWithOutParameter;
				}
			}
		}
		if (ErrorFacts.PreventsSuccessfulDelegateConversion(anonymousFunction.Bind(namedTypeSymbol).Diagnostics))
		{
			return LambdaConversionResult.BindingFailed;
		}
		return LambdaConversionResult.Success;
	}

	private static LambdaConversionResult IsAnonymousFunctionCompatibleWithExpressionTree(UnboundLambda anonymousFunction, NamedTypeSymbol type)
	{
		TypeSymbol type2 = type.TypeArgumentsNoUseSiteDiagnostics[0];
		if (!type2.IsDelegateType())
		{
			return LambdaConversionResult.ExpressionTreeMustHaveDelegateTypeArgument;
		}
		if (anonymousFunction.Syntax.Kind() == SyntaxKind.AnonymousMethodExpression)
		{
			return LambdaConversionResult.ExpressionTreeFromAnonymousMethod;
		}
		return IsAnonymousFunctionCompatibleWithDelegate(anonymousFunction, type2);
	}

	public static LambdaConversionResult IsAnonymousFunctionCompatibleWithType(UnboundLambda anonymousFunction, TypeSymbol type)
	{
		if (type.IsDelegateType())
		{
			return IsAnonymousFunctionCompatibleWithDelegate(anonymousFunction, type);
		}
		if (type.IsExpressionTree())
		{
			return IsAnonymousFunctionCompatibleWithExpressionTree(anonymousFunction, (NamedTypeSymbol)type);
		}
		return LambdaConversionResult.BadTargetType;
	}

	private static bool HasAnonymousFunctionConversion(BoundExpression source, TypeSymbol destination)
	{
		if (source.Kind != BoundKind.UnboundLambda)
		{
			return false;
		}
		return IsAnonymousFunctionCompatibleWithType((UnboundLambda)source, destination) == LambdaConversionResult.Success;
	}

	internal Conversion ClassifyImplicitUserDefinedConversionForV6SwitchGoverningType(TypeSymbol sourceType, out TypeSymbol switchGoverningType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		UserDefinedConversionResult conversionResult = AnalyzeImplicitUserDefinedConversionForV6SwitchGoverningType(sourceType, ref useSiteDiagnostics);
		if (conversionResult.Kind == UserDefinedConversionResultKind.Valid)
		{
			UserDefinedConversionAnalysis userDefinedConversionAnalysis = conversionResult.Results[conversionResult.Best];
			switchGoverningType = userDefinedConversionAnalysis.ToType;
		}
		else
		{
			switchGoverningType = null;
		}
		return new Conversion(conversionResult, isImplicit: true);
	}

	internal Conversion GetCallerLineNumberConversion(TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax syntax = new Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax(new Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LiteralExpressionSyntax(SyntaxKind.NumericLiteralExpression, new Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken(SyntaxKind.NumericLiteralToken)), null, 0);
		TypeSymbol specialType = corLibrary.GetSpecialType(SpecialType.System_Int32);
		BoundLiteral sourceExpression = new BoundLiteral(syntax, ConstantValue.Create(int.MaxValue), specialType);
		return ClassifyStandardImplicitConversion(sourceExpression, specialType, destination, ref useSiteDiagnostics);
	}

	internal bool HasCallerLineNumberConversion(TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return GetCallerLineNumberConversion(destination, ref useSiteDiagnostics).Exists;
	}

	internal bool HasCallerInfoStringConversion(TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol specialType = corLibrary.GetSpecialType(SpecialType.System_String);
		return ClassifyStandardImplicitConversion(specialType, destination, ref useSiteDiagnostics).Exists;
	}

	public static bool HasIdentityConversion(TypeSymbol type1, TypeSymbol type2)
	{
		return type1.Equals(type2, TypeCompareKind.AllIgnoreOptions);
	}

	public static bool HasIdentityConversionToAny<T>(T type, ArrayBuilder<T> targetTypes) where T : TypeSymbol
	{
		int i = 0;
		for (int count = targetTypes.Count; i < count; i++)
		{
			if (HasIdentityConversion(type, targetTypes[i]))
			{
				return true;
			}
		}
		return false;
	}

	public Conversion ConvertExtensionMethodThisArg(TypeSymbol parameterType, TypeSymbol thisType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion conversion = ClassifyImplicitExtensionMethodThisArgConversion(null, thisType, parameterType, ref useSiteDiagnostics);
		if (!IsValidExtensionMethodThisArgConversion(conversion))
		{
			return Conversion.NoConversion;
		}
		return conversion;
	}

	public Conversion ClassifyImplicitExtensionMethodThisArgConversion(BoundExpression sourceExpressionOpt, TypeSymbol sourceType, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)sourceType != null)
		{
			if (HasIdentityConversion(sourceType, destination))
			{
				return Conversion.Identity;
			}
			if (HasBoxingConversion(sourceType, destination, ref useSiteDiagnostics))
			{
				return Conversion.Boxing;
			}
			if (HasImplicitReferenceConversion(sourceType, destination, ref useSiteDiagnostics))
			{
				return Conversion.ImplicitReference;
			}
		}
		if (sourceExpressionOpt != null && sourceExpressionOpt.Kind == BoundKind.TupleLiteral)
		{
			Conversion tupleLiteralConversion = GetTupleLiteralConversion((BoundTupleLiteral)sourceExpressionOpt, destination, ref useSiteDiagnostics, ConversionKind.ImplicitTupleLiteral, delegate(ConversionsBase conversions, BoundExpression s, TypeSymbol d, ref HashSet<DiagnosticInfo> u, bool a)
			{
				return conversions.ClassifyImplicitExtensionMethodThisArgConversion(s, s.Type, d, ref u);
			}, arg: false);
			if (tupleLiteralConversion.Exists)
			{
				return tupleLiteralConversion;
			}
		}
		if ((object)sourceType != null)
		{
			Conversion result = ClassifyTupleConversion(sourceType, destination, ref useSiteDiagnostics, ConversionKind.ImplicitTuple, delegate(ConversionsBase conversions, TypeSymbol s, TypeSymbol d, ref HashSet<DiagnosticInfo> u, bool a)
			{
				return conversions.ClassifyImplicitExtensionMethodThisArgConversion(null, s, d, ref u);
			}, arg: false);
			if (result.Exists)
			{
				return result;
			}
		}
		return Conversion.NoConversion;
	}

	public static bool IsValidExtensionMethodThisArgConversion(Conversion conversion)
	{
		switch (conversion.Kind)
		{
		case ConversionKind.Identity:
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
			return true;
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ImplicitTuple:
			foreach (Conversion underlyingConversion in conversion.UnderlyingConversions)
			{
				if (!IsValidExtensionMethodThisArgConversion(underlyingConversion))
				{
					return false;
				}
			}
			return true;
		default:
			return false;
		}
	}

	private static int GetNumericTypeIndex(SpecialType specialType)
	{
		return specialType switch
		{
			SpecialType.System_SByte => 0, 
			SpecialType.System_Byte => 1, 
			SpecialType.System_Int16 => 2, 
			SpecialType.System_UInt16 => 3, 
			SpecialType.System_Int32 => 4, 
			SpecialType.System_UInt32 => 5, 
			SpecialType.System_Int64 => 6, 
			SpecialType.System_UInt64 => 7, 
			SpecialType.System_Char => 8, 
			SpecialType.System_Single => 9, 
			SpecialType.System_Double => 10, 
			SpecialType.System_Decimal => 11, 
			_ => -1, 
		};
	}

	private static bool HasImplicitNumericConversion(TypeSymbol source, TypeSymbol destination)
	{
		int numericTypeIndex = GetNumericTypeIndex(source.SpecialType);
		if (numericTypeIndex < 0)
		{
			return false;
		}
		int numericTypeIndex2 = GetNumericTypeIndex(destination.SpecialType);
		if (numericTypeIndex2 < 0)
		{
			return false;
		}
		return s_implicitNumericConversions[numericTypeIndex, numericTypeIndex2];
	}

	private static bool HasExplicitNumericConversion(TypeSymbol source, TypeSymbol destination)
	{
		int numericTypeIndex = GetNumericTypeIndex(source.SpecialType);
		if (numericTypeIndex < 0)
		{
			return false;
		}
		int numericTypeIndex2 = GetNumericTypeIndex(destination.SpecialType);
		if (numericTypeIndex2 < 0)
		{
			return false;
		}
		return s_explicitNumericConversions[numericTypeIndex, numericTypeIndex2];
	}

	public static bool IsConstantNumericZero(ConstantValue value)
	{
		switch (value.Discriminator)
		{
		case ConstantValueTypeDiscriminator.SByte:
			return value.SByteValue == 0;
		case ConstantValueTypeDiscriminator.Byte:
			return value.ByteValue == 0;
		case ConstantValueTypeDiscriminator.Int16:
			return value.Int16Value == 0;
		case ConstantValueTypeDiscriminator.Int32:
			return value.Int32Value == 0;
		case ConstantValueTypeDiscriminator.Int64:
			return value.Int64Value == 0;
		case ConstantValueTypeDiscriminator.UInt16:
			return value.UInt16Value == 0;
		case ConstantValueTypeDiscriminator.UInt32:
			return value.UInt32Value == 0;
		case ConstantValueTypeDiscriminator.UInt64:
			return value.UInt64Value == 0;
		case ConstantValueTypeDiscriminator.Single:
		case ConstantValueTypeDiscriminator.Double:
			return value.DoubleValue == 0.0;
		case ConstantValueTypeDiscriminator.Decimal:
			return value.DecimalValue == 0m;
		default:
			return false;
		}
	}

	public static bool IsNumericType(SpecialType specialType)
	{
		if ((uint)(specialType - 8) <= 11u)
		{
			return true;
		}
		return false;
	}

	private static bool HasSpecialIntPtrConversion(TypeSymbol source, TypeSymbol target)
	{
		TypeSymbol typeSymbol = source.StrippedType();
		TypeSymbol typeSymbol2 = target.StrippedType();
		if (typeSymbol.SpecialType != SpecialType.System_UIntPtr && typeSymbol.SpecialType != SpecialType.System_IntPtr && typeSymbol2.SpecialType != SpecialType.System_UIntPtr && typeSymbol2.SpecialType != SpecialType.System_IntPtr)
		{
			return false;
		}
		TypeSymbol typeSymbol3 = ((typeSymbol.SpecialType == SpecialType.System_UIntPtr || typeSymbol.SpecialType == SpecialType.System_IntPtr) ? typeSymbol2 : typeSymbol);
		if (typeSymbol3.TypeKind == TypeKind.Pointer)
		{
			return true;
		}
		if (typeSymbol3.TypeKind == TypeKind.Enum)
		{
			return true;
		}
		SpecialType specialType = typeSymbol3.SpecialType;
		if ((uint)(specialType - 8) <= 11u)
		{
			return true;
		}
		return false;
	}

	private static bool HasExplicitEnumerationConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (IsNumericType(source.SpecialType) && destination.IsEnumType())
		{
			return true;
		}
		if (IsNumericType(destination.SpecialType) && source.IsEnumType())
		{
			return true;
		}
		if (source.IsEnumType() && destination.IsEnumType())
		{
			return true;
		}
		return false;
	}

	private Conversion ClassifyImplicitNullableConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!destination.IsNullableType())
		{
			return Conversion.NoConversion;
		}
		TypeSymbol nullableUnderlyingType = destination.GetNullableUnderlyingType();
		TypeSymbol typeSymbol = source.StrippedType();
		if (!typeSymbol.IsValueType)
		{
			return Conversion.NoConversion;
		}
		if (HasIdentityConversion(typeSymbol, nullableUnderlyingType))
		{
			return new Conversion(ConversionKind.ImplicitNullable, Conversion.IdentityUnderlying);
		}
		if (HasImplicitNumericConversion(typeSymbol, nullableUnderlyingType))
		{
			return new Conversion(ConversionKind.ImplicitNullable, Conversion.ImplicitNumericUnderlying);
		}
		Conversion item = ClassifyImplicitTupleConversion(typeSymbol, nullableUnderlyingType, ref useSiteDiagnostics);
		if (item.Exists)
		{
			return new Conversion(ConversionKind.ImplicitNullable, ImmutableArray.Create(item));
		}
		return Conversion.NoConversion;
	}

	private Conversion GetImplicitTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return GetTupleLiteralConversion(source, destination, ref useSiteDiagnostics, ConversionKind.ImplicitTupleLiteral, delegate(ConversionsBase conversions, BoundExpression s, TypeSymbol d, ref HashSet<DiagnosticInfo> u, bool a)
		{
			return conversions.ClassifyImplicitConversionFromExpression(s, d, ref u);
		}, arg: false);
	}

	private Conversion GetExplicitTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool forCast)
	{
		return GetTupleLiteralConversion(source, destination, ref useSiteDiagnostics, ConversionKind.ExplicitTupleLiteral, delegate(ConversionsBase conversions, BoundExpression s, TypeSymbol d, ref HashSet<DiagnosticInfo> u, bool a)
		{
			return conversions.ClassifyConversionFromExpression(s, d, ref u, a);
		}, forCast);
	}

	private Conversion GetTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConversionKind kind, ClassifyConversionFromExpressionDelegate classifyConversion, bool arg)
	{
		ImmutableArray<BoundExpression> arguments = source.Arguments;
		if (!destination.IsTupleOrCompatibleWithTupleOfCardinality(arguments.Length))
		{
			return Conversion.NoConversion;
		}
		ImmutableArray<TypeSymbol> elementTypesOfTupleOrCompatible = destination.GetElementTypesOfTupleOrCompatible();
		ArrayBuilder<Conversion> instance = ArrayBuilder<Conversion>.GetInstance(arguments.Length);
		for (int i = 0; i < arguments.Length; i++)
		{
			BoundExpression sourceExpression = arguments[i];
			Conversion item = classifyConversion(this, sourceExpression, elementTypesOfTupleOrCompatible[i], ref useSiteDiagnostics, arg);
			if (!item.Exists)
			{
				instance.Free();
				return Conversion.NoConversion;
			}
			instance.Add(item);
		}
		return new Conversion(kind, instance.ToImmutableAndFree());
	}

	private Conversion ClassifyImplicitTupleConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return ClassifyTupleConversion(source, destination, ref useSiteDiagnostics, ConversionKind.ImplicitTuple, delegate(ConversionsBase conversions, TypeSymbol s, TypeSymbol d, ref HashSet<DiagnosticInfo> u, bool a)
		{
			return conversions.ClassifyImplicitConversionFromType(s, d, ref u);
		}, arg: false);
	}

	private Conversion ClassifyExplicitTupleConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool forCast)
	{
		return ClassifyTupleConversion(source, destination, ref useSiteDiagnostics, ConversionKind.ExplicitTuple, delegate(ConversionsBase conversions, TypeSymbol s, TypeSymbol d, ref HashSet<DiagnosticInfo> u, bool a)
		{
			return conversions.ClassifyConversionFromType(s, d, ref u, a);
		}, forCast);
	}

	private Conversion ClassifyTupleConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConversionKind kind, ClassifyConversionFromTypeDelegate classifyConversion, bool arg)
	{
		if (!source.TryGetElementTypesIfTupleOrCompatible(out var elementTypes) || !destination.TryGetElementTypesIfTupleOrCompatible(out var elementTypes2) || elementTypes.Length != elementTypes2.Length)
		{
			return Conversion.NoConversion;
		}
		ArrayBuilder<Conversion> instance = ArrayBuilder<Conversion>.GetInstance(elementTypes.Length);
		for (int i = 0; i < elementTypes.Length; i++)
		{
			Conversion item = classifyConversion(this, elementTypes[i], elementTypes2[i], ref useSiteDiagnostics, arg);
			if (!item.Exists)
			{
				instance.Free();
				return Conversion.NoConversion;
			}
			instance.Add(item);
		}
		return new Conversion(kind, instance.ToImmutableAndFree());
	}

	private Conversion ClassifyExplicitNullableConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool forCast)
	{
		if (!source.IsNullableType() && !destination.IsNullableType())
		{
			return Conversion.NoConversion;
		}
		TypeSymbol typeSymbol = source.StrippedType();
		TypeSymbol typeSymbol2 = destination.StrippedType();
		if (HasIdentityConversion(typeSymbol, typeSymbol2))
		{
			return new Conversion(ConversionKind.ExplicitNullable, Conversion.IdentityUnderlying);
		}
		if (HasImplicitNumericConversion(typeSymbol, typeSymbol2))
		{
			return new Conversion(ConversionKind.ExplicitNullable, Conversion.ImplicitNumericUnderlying);
		}
		if (HasExplicitNumericConversion(typeSymbol, typeSymbol2))
		{
			return new Conversion(ConversionKind.ExplicitNullable, Conversion.ExplicitNumericUnderlying);
		}
		Conversion item = ClassifyExplicitTupleConversion(typeSymbol, typeSymbol2, ref useSiteDiagnostics, forCast);
		if (item.Exists)
		{
			return new Conversion(ConversionKind.ExplicitNullable, ImmutableArray.Create(item));
		}
		if (HasExplicitEnumerationConversion(typeSymbol, typeSymbol2))
		{
			return new Conversion(ConversionKind.ExplicitNullable, Conversion.ExplicitEnumerationUnderlying);
		}
		if (HasPointerToIntegerConversion(typeSymbol, typeSymbol2))
		{
			return new Conversion(ConversionKind.ExplicitNullable, Conversion.PointerToIntegerUnderlying);
		}
		return Conversion.NoConversion;
	}

	private bool HasCovariantArrayConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayTypeSymbol arrayTypeSymbol = source as ArrayTypeSymbol;
		ArrayTypeSymbol arrayTypeSymbol2 = destination as ArrayTypeSymbol;
		if ((object)arrayTypeSymbol == null || (object)arrayTypeSymbol2 == null)
		{
			return false;
		}
		if (arrayTypeSymbol.HasSameShapeAs(arrayTypeSymbol2))
		{
			return HasImplicitReferenceConversion(arrayTypeSymbol.ElementType, arrayTypeSymbol2.ElementType, ref useSiteDiagnostics);
		}
		return false;
	}

	public bool HasIdentityOrImplicitReferenceConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (HasIdentityConversion(source, destination))
		{
			return true;
		}
		return HasImplicitReferenceConversion(source, destination, ref useSiteDiagnostics);
	}

	private static bool HasImplicitDynamicConversionFromExpression(TypeSymbol expressionType, TypeSymbol destination)
	{
		if ((object)expressionType != null && expressionType.Kind == SymbolKind.DynamicType)
		{
			return !destination.IsPointerType();
		}
		return false;
	}

	private static bool HasExplicitDynamicConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source.Kind == SymbolKind.DynamicType)
		{
			return !destination.IsPointerType();
		}
		return false;
	}

	private bool HasArrayConversionToInterface(ArrayTypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.IsSZArray)
		{
			return false;
		}
		if (!destination.IsInterfaceType())
		{
			return false;
		}
		if (destination.SpecialType == SpecialType.System_Collections_IEnumerable)
		{
			return true;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)destination;
		if (namedTypeSymbol.AllTypeArgumentCount() != 1)
		{
			return false;
		}
		if (!namedTypeSymbol.IsPossibleArrayGenericInterface())
		{
			return false;
		}
		TypeSymbol destination2 = namedTypeSymbol.TypeArgumentWithDefinitionUseSiteDiagnostics(0, ref useSiteDiagnostics);
		return HasIdentityOrImplicitReferenceConversion(source.ElementType, destination2, ref useSiteDiagnostics);
	}

	private bool HasImplicitReferenceConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.IsErrorType())
		{
			return false;
		}
		if (!source.IsReferenceType)
		{
			return false;
		}
		if (destination.SpecialType == SpecialType.System_Object || destination.Kind == SymbolKind.DynamicType)
		{
			return true;
		}
		switch (source.TypeKind)
		{
		case TypeKind.Class:
			if (destination.IsClassType() && IsBaseClass(source, destination, ref useSiteDiagnostics))
			{
				return true;
			}
			return HasImplicitConversionToInterface(source, destination, ref useSiteDiagnostics);
		case TypeKind.Interface:
			return HasImplicitConversionToInterface(source, destination, ref useSiteDiagnostics);
		case TypeKind.Delegate:
			return HasImplicitConversionFromDelegate(source, destination, ref useSiteDiagnostics);
		case TypeKind.TypeParameter:
			return HasImplicitReferenceTypeParameterConversion((TypeParameterSymbol)source, destination, ref useSiteDiagnostics);
		case TypeKind.Array:
			return HasImplicitConversionFromArray(source, destination, ref useSiteDiagnostics);
		default:
			return false;
		}
	}

	private bool HasImplicitConversionToInterface(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!destination.IsInterfaceType())
		{
			return false;
		}
		if (source.IsClassType())
		{
			return HasAnyBaseInterfaceConversion(source, destination, ref useSiteDiagnostics);
		}
		if (source.IsInterfaceType())
		{
			if (HasAnyBaseInterfaceConversion(source, destination, ref useSiteDiagnostics))
			{
				return true;
			}
			if (source != destination && HasInterfaceVarianceConversion(source, destination, ref useSiteDiagnostics))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasImplicitConversionFromArray(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!(source is ArrayTypeSymbol source2))
		{
			return false;
		}
		if (HasCovariantArrayConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (destination.GetSpecialTypeSafe() == SpecialType.System_Array)
		{
			return true;
		}
		if (IsBaseInterface(destination, corLibrary.GetDeclaredSpecialType(SpecialType.System_Array), ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasArrayConversionToInterface(source2, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitConversionFromDelegate(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.IsDelegateType())
		{
			return false;
		}
		SpecialType specialTypeSafe = destination.GetSpecialTypeSafe();
		if (specialTypeSafe == SpecialType.System_MulticastDelegate || specialTypeSafe == SpecialType.System_Delegate || IsBaseInterface(destination, corLibrary.GetDeclaredSpecialType(SpecialType.System_MulticastDelegate), ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasDelegateVarianceConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		return false;
	}

	public bool HasImplicitTypeParameterConversion(TypeParameterSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (HasImplicitReferenceTypeParameterConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasImplicitBoxingTypeParameterConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (destination.TypeKind == TypeKind.TypeParameter && source.DependsOn((TypeParameterSymbol)destination))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitReferenceTypeParameterConversion(TypeParameterSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.IsValueType)
		{
			return false;
		}
		if (HasImplicitEffectiveBaseConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasImplicitEffectiveInterfaceSetConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (destination.TypeKind == TypeKind.TypeParameter && source.DependsOn((TypeParameterSymbol)destination))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitEffectiveBaseConversion(TypeParameterSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		NamedTypeSymbol namedTypeSymbol = source.EffectiveBaseClass(ref useSiteDiagnostics);
		if (HasIdentityConversion(namedTypeSymbol, destination))
		{
			return true;
		}
		if (IsBaseClass(namedTypeSymbol, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasAnyBaseInterfaceConversion(namedTypeSymbol, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitEffectiveInterfaceSetConversion(TypeParameterSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!destination.IsInterfaceType())
		{
			return false;
		}
		foreach (NamedTypeSymbol item in source.AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics))
		{
			if (HasInterfaceVarianceConversion(item, destination, ref useSiteDiagnostics))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasAnyBaseInterfaceConversion(TypeSymbol derivedType, TypeSymbol baseType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!baseType.IsInterfaceType())
		{
			return false;
		}
		if (!(derivedType is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		foreach (NamedTypeSymbol item in namedTypeSymbol.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics))
		{
			if (HasInterfaceVarianceConversion(item, baseType, ref useSiteDiagnostics))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasInterfaceVarianceConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		NamedTypeSymbol namedTypeSymbol = source as NamedTypeSymbol;
		NamedTypeSymbol namedTypeSymbol2 = destination as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null || (object)namedTypeSymbol2 == null)
		{
			return false;
		}
		if (!namedTypeSymbol.IsInterfaceType() || !namedTypeSymbol2.IsInterfaceType())
		{
			return false;
		}
		return HasVariantConversion(namedTypeSymbol, namedTypeSymbol2, ref useSiteDiagnostics);
	}

	private bool HasDelegateVarianceConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		NamedTypeSymbol namedTypeSymbol = source as NamedTypeSymbol;
		NamedTypeSymbol namedTypeSymbol2 = destination as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null || (object)namedTypeSymbol2 == null)
		{
			return false;
		}
		if (!namedTypeSymbol.IsDelegateType() || !namedTypeSymbol2.IsDelegateType())
		{
			return false;
		}
		return HasVariantConversion(namedTypeSymbol, namedTypeSymbol2, ref useSiteDiagnostics);
	}

	private bool HasVariantConversion(NamedTypeSymbol source, NamedTypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (_currentRecursionDepth >= 50)
		{
			return false;
		}
		ThreeState value = HasVariantConversionQuick(source, destination);
		if (value.HasValue())
		{
			return value.Value();
		}
		return CreateInstance(_currentRecursionDepth + 1).HasVariantConversionNoCycleCheck(source, destination, ref useSiteDiagnostics);
	}

	private static ThreeState HasVariantConversionQuick(NamedTypeSymbol source, NamedTypeSymbol destination)
	{
		if (HasIdentityConversion(source, destination))
		{
			return ThreeState.True;
		}
		if (source.OriginalDefinition != destination.OriginalDefinition)
		{
			return ThreeState.False;
		}
		return ThreeState.Unknown;
	}

	private bool HasVariantConversionNoCycleCheck(NamedTypeSymbol source, NamedTypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance3 = ArrayBuilder<TypeSymbol>.GetInstance();
		try
		{
			source.OriginalDefinition.GetAllTypeArguments(instance, ref useSiteDiagnostics);
			source.GetAllTypeArguments(instance2, ref useSiteDiagnostics);
			destination.GetAllTypeArguments(instance3, ref useSiteDiagnostics);
			for (int i = 0; i < instance.Count; i++)
			{
				TypeSymbol typeSymbol = instance2[i];
				TypeSymbol typeSymbol2 = instance3[i];
				if (HasIdentityConversion(typeSymbol, typeSymbol2))
				{
					continue;
				}
				TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)instance[i];
				switch (typeParameterSymbol.Variance)
				{
				case VarianceKind.None:
					return false;
				case VarianceKind.Out:
					if (!HasImplicitReferenceConversion(typeSymbol, typeSymbol2, ref useSiteDiagnostics))
					{
						return false;
					}
					break;
				case VarianceKind.In:
					if (!HasImplicitReferenceConversion(typeSymbol2, typeSymbol, ref useSiteDiagnostics))
					{
						return false;
					}
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(typeParameterSymbol.Variance);
				}
			}
		}
		finally
		{
			instance.Free();
			instance2.Free();
			instance3.Free();
		}
		return true;
	}

	private bool HasImplicitBoxingTypeParameterConversion(TypeParameterSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.IsReferenceType)
		{
			return false;
		}
		if (HasImplicitEffectiveBaseConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasImplicitEffectiveInterfaceSetConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (destination.TypeKind == TypeKind.TypeParameter && source.DependsOn((TypeParameterSymbol)destination))
		{
			return true;
		}
		if (destination.Kind == SymbolKind.DynamicType)
		{
			return true;
		}
		return false;
	}

	public bool HasBoxingConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.TypeKind == TypeKind.TypeParameter && HasImplicitBoxingTypeParameterConversion((TypeParameterSymbol)source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (!source.IsValueType || !destination.IsReferenceType)
		{
			return false;
		}
		if (source.IsNullableType())
		{
			return HasBoxingConversion(source.GetNullableUnderlyingType(), destination, ref useSiteDiagnostics);
		}
		if (source.IsRestrictedType())
		{
			return false;
		}
		if (destination.Kind == SymbolKind.DynamicType)
		{
			return !source.IsPointerType();
		}
		if (IsBaseClass(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasAnyBaseInterfaceConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		return false;
	}

	private static bool HasImplicitPointerConversion(TypeSymbol source, TypeSymbol destination)
	{
		PointerTypeSymbol pointerTypeSymbol = destination as PointerTypeSymbol;
		PointerTypeSymbol pointerTypeSymbol2 = source as PointerTypeSymbol;
		if ((object)pointerTypeSymbol != null && (object)pointerTypeSymbol2 != null)
		{
			return pointerTypeSymbol.PointedAtType.SpecialType == SpecialType.System_Void;
		}
		return false;
	}

	private bool HasIdentityOrReferenceConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (HasIdentityConversion(source, destination))
		{
			return true;
		}
		if (HasImplicitReferenceConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasExplicitReferenceConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		return false;
	}

	private bool HasExplicitReferenceConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.SpecialType == SpecialType.System_Object)
		{
			if (destination.IsReferenceType)
			{
				return true;
			}
		}
		else if (source.Kind == SymbolKind.DynamicType && destination.IsReferenceType)
		{
			return true;
		}
		if (destination.IsClassType() && IsBaseClass(destination, source, ref useSiteDiagnostics))
		{
			return true;
		}
		if (source.IsClassType() && destination.IsInterfaceType() && !source.IsSealed && !HasAnyBaseInterfaceConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (source.IsInterfaceType() && destination.IsClassType() && (!destination.IsSealed || HasAnyBaseInterfaceConversion(destination, source, ref useSiteDiagnostics)))
		{
			return true;
		}
		if (source.IsInterfaceType() && destination.IsInterfaceType() && !HasImplicitConversionToInterface(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasExplicitArrayConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasExplicitDelegateConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasExplicitReferenceTypeParameterConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		return false;
	}

	private bool HasExplicitReferenceTypeParameterConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeParameterSymbol typeParameterSymbol = source as TypeParameterSymbol;
		TypeParameterSymbol typeParameterSymbol2 = destination as TypeParameterSymbol;
		if ((object)typeParameterSymbol2 != null && typeParameterSymbol2.IsReferenceType)
		{
			NamedTypeSymbol namedTypeSymbol = typeParameterSymbol2.EffectiveBaseClass(ref useSiteDiagnostics);
			while ((object)namedTypeSymbol != null)
			{
				if (HasIdentityConversion(namedTypeSymbol, source))
				{
					return true;
				}
				namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
			}
		}
		if ((object)typeParameterSymbol2 != null && source.IsInterfaceType() && typeParameterSymbol2.IsReferenceType)
		{
			return true;
		}
		if ((object)typeParameterSymbol != null && typeParameterSymbol.IsReferenceType && destination.IsInterfaceType() && !HasImplicitReferenceTypeParameterConversion(typeParameterSymbol, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if ((object)typeParameterSymbol != null && (object)typeParameterSymbol2 != null && typeParameterSymbol2.IsReferenceType && typeParameterSymbol2.DependsOn(typeParameterSymbol))
		{
			return true;
		}
		return false;
	}

	private bool HasUnboxingTypeParameterConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeParameterSymbol typeParameterSymbol = source as TypeParameterSymbol;
		TypeParameterSymbol typeParameterSymbol2 = destination as TypeParameterSymbol;
		if ((object)typeParameterSymbol2 != null && !typeParameterSymbol2.IsReferenceType)
		{
			NamedTypeSymbol namedTypeSymbol = typeParameterSymbol2.EffectiveBaseClass(ref useSiteDiagnostics);
			while ((object)namedTypeSymbol != null)
			{
				if (namedTypeSymbol == source)
				{
					return true;
				}
				namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
			}
		}
		if (source.IsInterfaceType() && (object)typeParameterSymbol2 != null && !typeParameterSymbol2.IsReferenceType)
		{
			return true;
		}
		if ((object)typeParameterSymbol != null && !typeParameterSymbol.IsReferenceType && destination.IsInterfaceType() && !HasImplicitReferenceTypeParameterConversion(typeParameterSymbol, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		if ((object)typeParameterSymbol != null && (object)typeParameterSymbol2 != null && !typeParameterSymbol2.IsReferenceType && typeParameterSymbol2.DependsOn(typeParameterSymbol))
		{
			return true;
		}
		return false;
	}

	private bool HasExplicitDelegateConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (destination.IsDelegateType())
		{
			if (source.SpecialType == SpecialType.System_Delegate || source.SpecialType == SpecialType.System_MulticastDelegate)
			{
				return true;
			}
			if (HasImplicitConversionToInterface(corLibrary.GetDeclaredSpecialType(SpecialType.System_Delegate), source, ref useSiteDiagnostics))
			{
				return true;
			}
		}
		if (!source.IsDelegateType() || !destination.IsDelegateType())
		{
			return false;
		}
		if (source.OriginalDefinition != destination.OriginalDefinition)
		{
			return false;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)source;
		NamedTypeSymbol namedTypeSymbol2 = (NamedTypeSymbol)destination;
		NamedTypeSymbol originalDefinition = namedTypeSymbol.OriginalDefinition;
		if (HasIdentityConversion(source, destination))
		{
			return false;
		}
		if (HasDelegateVarianceConversion(source, destination, ref useSiteDiagnostics))
		{
			return false;
		}
		ImmutableArray<TypeSymbol> immutableArray = namedTypeSymbol.TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		ImmutableArray<TypeSymbol> immutableArray2 = namedTypeSymbol2.TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		for (int i = 0; i < immutableArray.Length; i++)
		{
			TypeSymbol typeSymbol = immutableArray[i];
			TypeSymbol typeSymbol2 = immutableArray2[i];
			switch (originalDefinition.TypeParameters[i].Variance)
			{
			case VarianceKind.None:
				if (!HasIdentityConversion(typeSymbol, typeSymbol2))
				{
					return false;
				}
				break;
			case VarianceKind.Out:
				if (!HasIdentityOrReferenceConversion(typeSymbol, typeSymbol2, ref useSiteDiagnostics))
				{
					return false;
				}
				break;
			case VarianceKind.In:
			{
				bool flag = HasIdentityConversion(typeSymbol, typeSymbol2);
				bool flag2 = typeSymbol.IsReferenceType && typeSymbol2.IsReferenceType;
				if (!(flag | flag2))
				{
					return false;
				}
				break;
			}
			}
		}
		return true;
	}

	private bool HasExplicitArrayConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayTypeSymbol arrayTypeSymbol = source as ArrayTypeSymbol;
		ArrayTypeSymbol arrayTypeSymbol2 = destination as ArrayTypeSymbol;
		if ((object)arrayTypeSymbol != null && (object)arrayTypeSymbol2 != null)
		{
			if (arrayTypeSymbol.HasSameShapeAs(arrayTypeSymbol2))
			{
				return HasExplicitReferenceConversion(arrayTypeSymbol.ElementType, arrayTypeSymbol2.ElementType, ref useSiteDiagnostics);
			}
			return false;
		}
		if ((object)arrayTypeSymbol2 != null)
		{
			if (source.SpecialType == SpecialType.System_Array)
			{
				return true;
			}
			foreach (NamedTypeSymbol item in corLibrary.GetDeclaredSpecialType(SpecialType.System_Array).AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics))
			{
				if (HasIdentityConversion(item, source))
				{
					return true;
				}
			}
		}
		if ((object)arrayTypeSymbol != null && arrayTypeSymbol.IsSZArray && destination.IsPossibleArrayGenericInterface() && HasExplicitReferenceConversion(arrayTypeSymbol.ElementType, ((NamedTypeSymbol)destination).TypeArgumentWithDefinitionUseSiteDiagnostics(0, ref useSiteDiagnostics), ref useSiteDiagnostics))
		{
			return true;
		}
		if ((object)arrayTypeSymbol2 != null && arrayTypeSymbol2.IsSZArray)
		{
			SpecialType specialType = source.OriginalDefinition.SpecialType;
			if (specialType == SpecialType.System_Collections_Generic_IList_T || specialType == SpecialType.System_Collections_Generic_ICollection_T || specialType == SpecialType.System_Collections_Generic_IEnumerable_T || specialType == SpecialType.System_Collections_Generic_IReadOnlyList_T || specialType == SpecialType.System_Collections_Generic_IReadOnlyCollection_T)
			{
				TypeSymbol typeSymbol = ((NamedTypeSymbol)source).TypeArgumentWithDefinitionUseSiteDiagnostics(0, ref useSiteDiagnostics);
				TypeSymbol elementType = arrayTypeSymbol2.ElementType;
				if (HasIdentityConversion(typeSymbol, elementType))
				{
					return true;
				}
				if (HasImplicitReferenceConversion(typeSymbol, elementType, ref useSiteDiagnostics))
				{
					return true;
				}
				if (HasExplicitReferenceConversion(typeSymbol, elementType, ref useSiteDiagnostics))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasUnboxingConversion(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (destination.IsPointerType())
		{
			return false;
		}
		if (destination.IsRestrictedType())
		{
			return false;
		}
		SpecialType specialType = source.SpecialType;
		if ((specialType == SpecialType.System_Object || specialType == SpecialType.System_ValueType) && destination.IsValueType && !destination.IsNullableType())
		{
			return true;
		}
		if (source.IsInterfaceType() && destination.IsValueType && !destination.IsNullableType() && HasBoxingConversion(destination, source, ref useSiteDiagnostics))
		{
			return true;
		}
		if (source.SpecialType == SpecialType.System_Enum && destination.IsEnumType())
		{
			return true;
		}
		if (source.IsReferenceType && destination.IsNullableType() && HasUnboxingConversion(source, destination.GetNullableUnderlyingType(), ref useSiteDiagnostics))
		{
			return true;
		}
		if (HasUnboxingTypeParameterConversion(source, destination, ref useSiteDiagnostics))
		{
			return true;
		}
		return false;
	}

	private static bool HasPointerToPointerConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source is PointerTypeSymbol)
		{
			return destination is PointerTypeSymbol;
		}
		return false;
	}

	private static bool HasPointerToIntegerConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (!(source is PointerTypeSymbol))
		{
			return false;
		}
		SpecialType specialType = destination.StrippedType().SpecialType;
		if ((uint)(specialType - 9) <= 7u)
		{
			return true;
		}
		return false;
	}

	private static bool HasIntegerToPointerConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (!(destination is PointerTypeSymbol))
		{
			return false;
		}
		SpecialType specialType = source.SpecialType;
		if ((uint)(specialType - 9) <= 7u)
		{
			return true;
		}
		return false;
	}

	private static TypeSymbol GetUnderlyingEffectiveType(TypeSymbol type, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)type != null)
		{
			type = type.StrippedType();
			if (type.IsTypeParameter())
			{
				type = ((TypeParameterSymbol)type).EffectiveBaseClass(ref useSiteDiagnostics);
			}
		}
		return type;
	}

	public static void AddTypesParticipatingInUserDefinedConversion(ArrayBuilder<NamedTypeSymbol> result, TypeSymbol type, bool includeBaseTypes, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)type == null)
		{
			return;
		}
		bool flag = result.Count > 0;
		if (type.IsClassType() || type.IsStructType())
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
			if (!flag || !HasIdentityConversionToAny(namedTypeSymbol, result))
			{
				result.Add(namedTypeSymbol);
			}
		}
		if (!includeBaseTypes)
		{
			return;
		}
		NamedTypeSymbol namedTypeSymbol2 = type.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		while ((object)namedTypeSymbol2 != null)
		{
			if (!flag || !HasIdentityConversionToAny(namedTypeSymbol2, result))
			{
				result.Add(namedTypeSymbol2);
			}
			namedTypeSymbol2 = namedTypeSymbol2.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
	}

	private UserDefinedConversionResult AnalyzeExplicitUserDefinedConversions(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		ComputeUserDefinedExplicitConversionTypeSet(source, target, instance, ref useSiteDiagnostics);
		ArrayBuilder<UserDefinedConversionAnalysis> instance2 = ArrayBuilder<UserDefinedConversionAnalysis>.GetInstance();
		ComputeApplicableUserDefinedExplicitConversionSet(sourceExpression, source, target, instance, instance2, ref useSiteDiagnostics);
		instance.Free();
		ImmutableArray<UserDefinedConversionAnalysis> immutableArray = instance2.ToImmutableAndFree();
		if (immutableArray.Length == 0)
		{
			return UserDefinedConversionResult.NoApplicableOperators(immutableArray);
		}
		TypeSymbol typeSymbol = MostSpecificSourceTypeForExplicitUserDefinedConversion(immutableArray, sourceExpression, source, ref useSiteDiagnostics);
		if ((object)typeSymbol == null)
		{
			return UserDefinedConversionResult.NoBestSourceType(immutableArray);
		}
		TypeSymbol typeSymbol2 = MostSpecificTargetTypeForExplicitUserDefinedConversion(immutableArray, target, ref useSiteDiagnostics);
		if ((object)typeSymbol2 == null)
		{
			return UserDefinedConversionResult.NoBestTargetType(immutableArray);
		}
		int? num = MostSpecificConversionOperator(typeSymbol, typeSymbol2, immutableArray);
		if (!num.HasValue)
		{
			return UserDefinedConversionResult.Ambiguous(immutableArray);
		}
		return UserDefinedConversionResult.Valid(immutableArray, num.Value);
	}

	private static void ComputeUserDefinedExplicitConversionTypeSet(TypeSymbol source, TypeSymbol target, ArrayBuilder<NamedTypeSymbol> d, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol underlyingEffectiveType = GetUnderlyingEffectiveType(source, ref useSiteDiagnostics);
		TypeSymbol underlyingEffectiveType2 = GetUnderlyingEffectiveType(target, ref useSiteDiagnostics);
		AddTypesParticipatingInUserDefinedConversion(d, underlyingEffectiveType, includeBaseTypes: true, ref useSiteDiagnostics);
		AddTypesParticipatingInUserDefinedConversion(d, underlyingEffectiveType2, includeBaseTypes: true, ref useSiteDiagnostics);
	}

	private void ComputeApplicableUserDefinedExplicitConversionSet(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, ArrayBuilder<NamedTypeSymbol> d, ArrayBuilder<UserDefinedConversionAnalysis> u, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		foreach (NamedTypeSymbol item in d)
		{
			AddUserDefinedConversionsToExplicitCandidateSet(sourceExpression, source, target, u, item, "op_Explicit", ref useSiteDiagnostics);
			AddUserDefinedConversionsToExplicitCandidateSet(sourceExpression, source, target, u, item, "op_Implicit", ref useSiteDiagnostics);
		}
	}

	private void AddUserDefinedConversionsToExplicitCandidateSet(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, ArrayBuilder<UserDefinedConversionAnalysis> u, NamedTypeSymbol declaringType, string operatorName, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (((object)source != null && source.IsInterfaceType()) || target.IsInterfaceType())
		{
			return;
		}
		foreach (MethodSymbol @operator in declaringType.GetOperators(operatorName))
		{
			if (@operator.ReturnsVoid || @operator.ParameterCount != 1 || @operator.ReturnType.TypeKind == TypeKind.Error)
			{
				continue;
			}
			TypeSymbol typeSymbol = @operator.ParameterTypes[0];
			TypeSymbol typeSymbol2 = @operator.ReturnType;
			Conversion sourceConversion = EncompassingExplicitConversion(sourceExpression, source, typeSymbol, ref useSiteDiagnostics);
			Conversion targetConversion = EncompassingExplicitConversion(null, typeSymbol2, target, ref useSiteDiagnostics);
			if (!sourceConversion.Exists && (object)source != null && source.IsNullableType() && EncompassingExplicitConversion(null, source.GetNullableUnderlyingType(), typeSymbol, ref useSiteDiagnostics).Exists)
			{
				sourceConversion = ClassifyBuiltInConversion(source, typeSymbol, ref useSiteDiagnostics);
			}
			if (!targetConversion.Exists && (object)target != null && target.IsNullableType() && EncompassingExplicitConversion(null, typeSymbol2, target.GetNullableUnderlyingType(), ref useSiteDiagnostics).Exists)
			{
				targetConversion = ClassifyBuiltInConversion(typeSymbol2, target, ref useSiteDiagnostics);
			}
			if (!sourceConversion.Exists || !targetConversion.Exists)
			{
				continue;
			}
			if ((object)source != null && source.IsNullableType() && typeSymbol.IsNonNullableValueType() && target.CanBeAssignedNull())
			{
				TypeSymbol typeSymbol3 = MakeNullableType(typeSymbol);
				TypeSymbol typeSymbol4 = (typeSymbol2.IsNonNullableValueType() ? MakeNullableType(typeSymbol2) : typeSymbol2);
				Conversion sourceConversion2 = EncompassingExplicitConversion(sourceExpression, source, typeSymbol3, ref useSiteDiagnostics);
				Conversion targetConversion2 = EncompassingExplicitConversion(null, typeSymbol4, target, ref useSiteDiagnostics);
				u.Add(UserDefinedConversionAnalysis.Lifted(@operator, sourceConversion2, targetConversion2, typeSymbol3, typeSymbol4));
				continue;
			}
			if (target.IsNullableType() && typeSymbol2.IsNonNullableValueType())
			{
				typeSymbol2 = MakeNullableType(typeSymbol2);
				targetConversion = EncompassingExplicitConversion(null, typeSymbol2, target, ref useSiteDiagnostics);
			}
			if ((object)source != null && source.IsNullableType() && typeSymbol.IsNonNullableValueType())
			{
				typeSymbol = MakeNullableType(typeSymbol);
				sourceConversion = EncompassingExplicitConversion(null, typeSymbol, source, ref useSiteDiagnostics);
			}
			u.Add(UserDefinedConversionAnalysis.Normal(@operator, sourceConversion, targetConversion, typeSymbol, typeSymbol2));
		}
	}

	private TypeSymbol MostSpecificSourceTypeForExplicitUserDefinedConversion(ImmutableArray<UserDefinedConversionAnalysis> u, BoundExpression sourceExpression, TypeSymbol source, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)source != null)
		{
			if (u.Any((UserDefinedConversionAnalysis conv) => conv.FromType == source))
			{
				return source;
			}
			HashSet<DiagnosticInfo> _useSiteDiagnostics = useSiteDiagnostics;
			Func<UserDefinedConversionAnalysis, bool> func = (UserDefinedConversionAnalysis conv) => IsEncompassedBy(sourceExpression, source, conv.FromType, ref _useSiteDiagnostics);
			if (u.Any(func))
			{
				TypeSymbol result = MostEncompassedType(u, func, (UserDefinedConversionAnalysis conv) => conv.FromType, ref _useSiteDiagnostics);
				useSiteDiagnostics = _useSiteDiagnostics;
				return result;
			}
			useSiteDiagnostics = _useSiteDiagnostics;
		}
		return MostEncompassingType(u, (UserDefinedConversionAnalysis conv) => conv.FromType, ref useSiteDiagnostics);
	}

	private TypeSymbol MostSpecificTargetTypeForExplicitUserDefinedConversion(ImmutableArray<UserDefinedConversionAnalysis> u, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (u.Any((UserDefinedConversionAnalysis conv) => conv.ToType == target))
		{
			return target;
		}
		HashSet<DiagnosticInfo> _useSiteDiagnostics = useSiteDiagnostics;
		Func<UserDefinedConversionAnalysis, bool> func = (UserDefinedConversionAnalysis conv) => IsEncompassedBy(null, conv.ToType, target, ref _useSiteDiagnostics);
		if (u.Any(func))
		{
			TypeSymbol result = MostEncompassingType(u, func, (UserDefinedConversionAnalysis conv) => conv.ToType, ref _useSiteDiagnostics);
			useSiteDiagnostics = _useSiteDiagnostics;
			return result;
		}
		useSiteDiagnostics = _useSiteDiagnostics;
		return MostEncompassedType(u, (UserDefinedConversionAnalysis conv) => conv.ToType, ref useSiteDiagnostics);
	}

	private Conversion EncompassingExplicitConversion(BoundExpression expr, TypeSymbol a, TypeSymbol b, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion result = ClassifyStandardConversion(expr, a, b, ref useSiteDiagnostics);
		if (!result.IsEnumeration)
		{
			return result;
		}
		return Conversion.NoConversion;
	}

	private UserDefinedConversionResult AnalyzeImplicitUserDefinedConversions(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		ComputeUserDefinedImplicitConversionTypeSet(source, target, instance, ref useSiteDiagnostics);
		ArrayBuilder<UserDefinedConversionAnalysis> instance2 = ArrayBuilder<UserDefinedConversionAnalysis>.GetInstance();
		ComputeApplicableUserDefinedImplicitConversionSet(sourceExpression, source, target, instance, instance2, ref useSiteDiagnostics);
		instance.Free();
		ImmutableArray<UserDefinedConversionAnalysis> immutableArray = instance2.ToImmutableAndFree();
		if (immutableArray.Length == 0)
		{
			return UserDefinedConversionResult.NoApplicableOperators(immutableArray);
		}
		TypeSymbol typeSymbol = MostSpecificSourceTypeForImplicitUserDefinedConversion(immutableArray, source, ref useSiteDiagnostics);
		if ((object)typeSymbol == null)
		{
			return UserDefinedConversionResult.NoBestSourceType(immutableArray);
		}
		TypeSymbol typeSymbol2 = MostSpecificTargetTypeForImplicitUserDefinedConversion(immutableArray, target, ref useSiteDiagnostics);
		if ((object)typeSymbol2 == null)
		{
			return UserDefinedConversionResult.NoBestTargetType(immutableArray);
		}
		int? num = MostSpecificConversionOperator(typeSymbol, typeSymbol2, immutableArray);
		if (!num.HasValue)
		{
			return UserDefinedConversionResult.Ambiguous(immutableArray);
		}
		return UserDefinedConversionResult.Valid(immutableArray, num.Value);
	}

	private static void ComputeUserDefinedImplicitConversionTypeSet(TypeSymbol s, TypeSymbol t, ArrayBuilder<NamedTypeSymbol> d, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol underlyingEffectiveType = GetUnderlyingEffectiveType(s, ref useSiteDiagnostics);
		TypeSymbol underlyingEffectiveType2 = GetUnderlyingEffectiveType(t, ref useSiteDiagnostics);
		AddTypesParticipatingInUserDefinedConversion(d, underlyingEffectiveType, includeBaseTypes: true, ref useSiteDiagnostics);
		AddTypesParticipatingInUserDefinedConversion(d, underlyingEffectiveType2, includeBaseTypes: false, ref useSiteDiagnostics);
	}

	private void ComputeApplicableUserDefinedImplicitConversionSet(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, ArrayBuilder<NamedTypeSymbol> d, ArrayBuilder<UserDefinedConversionAnalysis> u, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool allowAnyTarget = false)
	{
		if (((object)source != null && source.IsInterfaceType()) || ((object)target != null && target.IsInterfaceType()))
		{
			return;
		}
		foreach (NamedTypeSymbol item in d)
		{
			foreach (MethodSymbol @operator in item.GetOperators("op_Implicit"))
			{
				if (@operator.ReturnsVoid || @operator.ParameterCount != 1)
				{
					continue;
				}
				TypeSymbol typeSymbol = @operator.ParameterTypes[0];
				TypeSymbol typeSymbol2 = @operator.ReturnType;
				Conversion sourceConversion = EncompassingImplicitConversion(sourceExpression, source, typeSymbol, ref useSiteDiagnostics);
				Conversion targetConversion = (allowAnyTarget ? Conversion.Identity : EncompassingImplicitConversion(null, typeSymbol2, target, ref useSiteDiagnostics));
				if (sourceConversion.Exists && targetConversion.Exists)
				{
					if ((object)target != null && target.IsNullableType() && typeSymbol2.IsNonNullableValueType())
					{
						typeSymbol2 = MakeNullableType(typeSymbol2);
						targetConversion = (allowAnyTarget ? Conversion.Identity : EncompassingImplicitConversion(null, typeSymbol2, target, ref useSiteDiagnostics));
					}
					u.Add(UserDefinedConversionAnalysis.Normal(@operator, sourceConversion, targetConversion, typeSymbol, typeSymbol2));
				}
				else if ((object)source != null && source.IsNullableType() && typeSymbol.IsNonNullableValueType() && (allowAnyTarget || target.CanBeAssignedNull()))
				{
					TypeSymbol typeSymbol3 = MakeNullableType(typeSymbol);
					TypeSymbol typeSymbol4 = (typeSymbol2.IsNonNullableValueType() ? MakeNullableType(typeSymbol2) : typeSymbol2);
					Conversion sourceConversion2 = EncompassingImplicitConversion(sourceExpression, source, typeSymbol3, ref useSiteDiagnostics);
					Conversion targetConversion2 = ((!allowAnyTarget) ? EncompassingImplicitConversion(null, typeSymbol4, target, ref useSiteDiagnostics) : Conversion.Identity);
					if (sourceConversion2.Exists && targetConversion2.Exists)
					{
						u.Add(UserDefinedConversionAnalysis.Lifted(@operator, sourceConversion2, targetConversion2, typeSymbol3, typeSymbol4));
					}
				}
			}
		}
	}

	private TypeSymbol MostSpecificSourceTypeForImplicitUserDefinedConversion(ImmutableArray<UserDefinedConversionAnalysis> u, TypeSymbol source, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)source != null && u.Any((UserDefinedConversionAnalysis conv) => conv.FromType == source))
		{
			return source;
		}
		return MostEncompassedType(u, (UserDefinedConversionAnalysis conv) => conv.FromType, ref useSiteDiagnostics);
	}

	private TypeSymbol MostSpecificTargetTypeForImplicitUserDefinedConversion(ImmutableArray<UserDefinedConversionAnalysis> u, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (u.Any((UserDefinedConversionAnalysis conv) => conv.ToType == target))
		{
			return target;
		}
		return MostEncompassingType(u, (UserDefinedConversionAnalysis conv) => conv.ToType, ref useSiteDiagnostics);
	}

	private static int LiftingCount(UserDefinedConversionAnalysis conv)
	{
		int num = 0;
		if (conv.FromType != conv.Operator.ParameterTypes[0])
		{
			num++;
		}
		if (conv.ToType != conv.Operator.ReturnType)
		{
			num++;
		}
		return num;
	}

	private static int? MostSpecificConversionOperator(TypeSymbol sx, TypeSymbol tx, ImmutableArray<UserDefinedConversionAnalysis> u)
	{
		return MostSpecificConversionOperator((UserDefinedConversionAnalysis conv) => conv.FromType == sx && conv.ToType == tx, u);
	}

	private static int? MostSpecificConversionOperator(Func<UserDefinedConversionAnalysis, bool> constraint, ImmutableArray<UserDefinedConversionAnalysis> u)
	{
		BestIndex bestIndex = UniqueIndex(u, (UserDefinedConversionAnalysis conv) => constraint(conv) && LiftingCount(conv) == 0);
		if (bestIndex.Kind == BestIndexKind.Best)
		{
			return bestIndex.Best;
		}
		if (bestIndex.Kind == BestIndexKind.Ambiguous)
		{
			return null;
		}
		BestIndex bestIndex2 = UniqueIndex(u, (UserDefinedConversionAnalysis conv) => constraint(conv) && LiftingCount(conv) == 1);
		if (bestIndex2.Kind == BestIndexKind.Best)
		{
			return bestIndex2.Best;
		}
		if (bestIndex2.Kind == BestIndexKind.Ambiguous)
		{
			return null;
		}
		BestIndex bestIndex3 = UniqueIndex(u, (UserDefinedConversionAnalysis conv) => constraint(conv) && LiftingCount(conv) == 2);
		if (bestIndex3.Kind == BestIndexKind.Best)
		{
			return bestIndex3.Best;
		}
		_ = bestIndex3.Kind;
		return null;
	}

	private static BestIndex UniqueIndex<T>(ImmutableArray<T> items, Func<T, bool> predicate)
	{
		if (items.IsEmpty)
		{
			return BestIndex.None();
		}
		int? num = null;
		for (int i = 0; i < items.Length; i++)
		{
			if (predicate(items[i]))
			{
				if (num.HasValue)
				{
					return BestIndex.IsAmbiguous(num.Value, i);
				}
				num = i;
			}
		}
		if (num.HasValue)
		{
			return BestIndex.HasBest(num.Value);
		}
		return BestIndex.None();
	}

	private bool IsEncompassedBy(BoundExpression aExpr, TypeSymbol a, TypeSymbol b, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return EncompassingImplicitConversion(aExpr, a, b, ref useSiteDiagnostics).Exists;
	}

	private Conversion EncompassingImplicitConversion(BoundExpression aExpr, TypeSymbol a, TypeSymbol b, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Conversion result = ClassifyStandardImplicitConversion(aExpr, a, b, ref useSiteDiagnostics);
		if (!IsEncompassingImplicitConversionKind(result.Kind))
		{
			return Conversion.NoConversion;
		}
		return result;
	}

	private static bool IsEncompassingImplicitConversionKind(ConversionKind kind)
	{
		switch (kind)
		{
		case ConversionKind.NoConversion:
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ExplicitTupleLiteral:
		case ConversionKind.ExplicitTuple:
		case ConversionKind.ImplicitDynamic:
		case ConversionKind.ExplicitDynamic:
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.AnonymousFunction:
		case ConversionKind.MethodGroup:
		case ConversionKind.ExplicitNumeric:
		case ConversionKind.ExplicitEnumeration:
		case ConversionKind.ExplicitNullable:
		case ConversionKind.ExplicitReference:
		case ConversionKind.Unboxing:
		case ConversionKind.ExplicitUserDefined:
		case ConversionKind.PointerToPointer:
		case ConversionKind.IntegerToPointer:
		case ConversionKind.PointerToInteger:
		case ConversionKind.IntPtr:
		case ConversionKind.InterpolatedString:
		case ConversionKind.StackAllocToPointerType:
		case ConversionKind.StackAllocToSpanType:
			return false;
		case ConversionKind.Identity:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitThrow:
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ImplicitNullable:
		case ConversionKind.DefaultOrNullLiteral:
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
		case ConversionKind.PointerToVoid:
		case ConversionKind.NullToPointer:
		case ConversionKind.ImplicitConstant:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private TypeSymbol MostEncompassedType<T>(ImmutableArray<T> items, Func<T, TypeSymbol> extract, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return MostEncompassedType(items, (T x) => true, extract, ref useSiteDiagnostics);
	}

	private TypeSymbol MostEncompassedType<T>(ImmutableArray<T> items, Func<T, bool> valid, Func<T, TypeSymbol> extract, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		HashSet<DiagnosticInfo> _useSiteDiagnostics = useSiteDiagnostics;
		int? num = UniqueBestValidIndex(items, valid, (T left, T right) =>
		{
			TypeSymbol typeSymbol = extract(left);
			TypeSymbol typeSymbol2 = extract(right);
			if (typeSymbol == typeSymbol2)
			{
				return BetterResult.Equal;
			}
			bool flag = IsEncompassedBy(null, typeSymbol, typeSymbol2, ref _useSiteDiagnostics);
			bool flag2 = IsEncompassedBy(null, typeSymbol2, typeSymbol, ref _useSiteDiagnostics);
			if (flag == flag2)
			{
				return BetterResult.Neither;
			}
			return (!flag) ? BetterResult.Right : BetterResult.Left;
		});
		useSiteDiagnostics = _useSiteDiagnostics;
		if (num.HasValue)
		{
			return extract(items[num.Value]);
		}
		return null;
	}

	private TypeSymbol MostEncompassingType<T>(ImmutableArray<T> items, Func<T, TypeSymbol> extract, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return MostEncompassingType(items, (T x) => true, extract, ref useSiteDiagnostics);
	}

	private TypeSymbol MostEncompassingType<T>(ImmutableArray<T> items, Func<T, bool> valid, Func<T, TypeSymbol> extract, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		HashSet<DiagnosticInfo> _useSiteDiagnostics = useSiteDiagnostics;
		int? num = UniqueBestValidIndex(items, valid, (T left, T right) =>
		{
			TypeSymbol typeSymbol = extract(left);
			TypeSymbol typeSymbol2 = extract(right);
			if (typeSymbol == typeSymbol2)
			{
				return BetterResult.Equal;
			}
			bool flag = IsEncompassedBy(null, typeSymbol2, typeSymbol, ref _useSiteDiagnostics);
			bool flag2 = IsEncompassedBy(null, typeSymbol, typeSymbol2, ref _useSiteDiagnostics);
			if (flag == flag2)
			{
				return BetterResult.Neither;
			}
			return (!flag) ? BetterResult.Right : BetterResult.Left;
		});
		useSiteDiagnostics = _useSiteDiagnostics;
		if (num.HasValue)
		{
			return extract(items[num.Value]);
		}
		return null;
	}

	private static int? UniqueBestValidIndex<T>(ImmutableArray<T> items, Func<T, bool> valid, Func<T, T, BetterResult> better)
	{
		if (items.IsEmpty)
		{
			return null;
		}
		int? result = null;
		T arg = default;
		for (int i = 0; i < items.Length; i++)
		{
			T val = items[i];
			if (!valid(val))
			{
				continue;
			}
			if (!result.HasValue)
			{
				result = i;
				arg = val;
				continue;
			}
			switch (better(arg, val))
			{
			case BetterResult.Neither:
				result = null;
				arg = default;
				break;
			case BetterResult.Right:
				result = i;
				arg = val;
				break;
			}
		}
		if (!result.HasValue)
		{
			return null;
		}
		for (int j = 0; j < result.Value; j++)
		{
			T val2 = items[j];
			if (valid(val2))
			{
				BetterResult betterResult = better(arg, val2);
				if (betterResult != BetterResult.Left && betterResult != BetterResult.Equal)
				{
					return null;
				}
			}
		}
		return result;
	}

	private NamedTypeSymbol MakeNullableType(TypeSymbol type)
	{
		return corLibrary.GetDeclaredSpecialType(SpecialType.System_Nullable_T).Construct(type);
	}

	protected UserDefinedConversionResult AnalyzeImplicitUserDefinedConversionForV6SwitchGoverningType(TypeSymbol source, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		ComputeUserDefinedImplicitConversionTypeSet(source, null, instance, ref useSiteDiagnostics);
		ArrayBuilder<UserDefinedConversionAnalysis> instance2 = ArrayBuilder<UserDefinedConversionAnalysis>.GetInstance();
		ComputeApplicableUserDefinedImplicitConversionSet(null, source, null, instance, instance2, ref useSiteDiagnostics, allowAnyTarget: true);
		instance.Free();
		ImmutableArray<UserDefinedConversionAnalysis> immutableArray = instance2.ToImmutableAndFree();
		int? num = MostSpecificConversionOperator((UserDefinedConversionAnalysis conv) => conv.ToType.IsValidV6SwitchGoverningType(isTargetTypeOfUserDefinedOp: true), immutableArray);
		if (num.HasValue)
		{
			return UserDefinedConversionResult.Valid(immutableArray, num.Value);
		}
		return UserDefinedConversionResult.NoApplicableOperators(immutableArray);
	}
}
