using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class ConversionDotNet
{
	private enum DotNetConversionKind
	{
		NoConversion,
		Identity,
		ImplicitNumeric,
		ImplicitEnumeration,
		ImplicitNullable,
		ImplicitReference,
		Boxing,
		ImplicitDynamic,
		ExplicitDynamic,
		ExplicitNumeric,
		ExplicitEnumeration,
		ExplicitNullable,
		ExplicitReference,
		Unboxing
	}

	private static readonly byte[,] DotNetToDotNetConversionKind;

	private static readonly byte[,] DotNetToAlConversionKind;

	private static readonly byte[,] AlToDotNetRefConversionKind;

	private static readonly byte[,] AlToDotNetConversionKind;

	[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
	static ConversionDotNet()
	{
		DotNetToDotNetConversionKind = new byte[15, 15]
		{
			{
				1, 12, 13, 13, 13, 13, 13, 13, 13, 13,
				13, 13, 13, 13, 13
			},
			{
				5, 1, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0
			},
			{
				6, 0, 1, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0
			},
			{
				6, 0, 0, 1, 9, 9, 2, 2, 9, 2,
				2, 2, 2, 2, 2
			},
			{
				6, 0, 0, 9, 1, 2, 2, 2, 9, 9,
				9, 9, 2, 2, 2
			},
			{
				6, 0, 0, 9, 9, 1, 2, 2, 9, 9,
				9, 9, 2, 2, 2
			},
			{
				6, 0, 0, 9, 9, 9, 1, 2, 9, 9,
				9, 9, 2, 2, 2
			},
			{
				6, 0, 0, 9, 9, 9, 9, 1, 9, 9,
				9, 9, 2, 2, 2
			},
			{
				6, 0, 0, 9, 9, 2, 2, 2, 1, 2,
				2, 2, 2, 2, 2
			},
			{
				6, 0, 0, 9, 9, 9, 2, 2, 9, 1,
				2, 2, 2, 2, 2
			},
			{
				6, 0, 0, 9, 9, 9, 9, 2, 9, 9,
				1, 2, 2, 2, 2
			},
			{
				6, 0, 0, 9, 9, 9, 9, 9, 9, 9,
				9, 1, 2, 2, 2
			},
			{
				6, 0, 0, 9, 9, 9, 9, 9, 9, 9,
				9, 9, 1, 2, 9
			},
			{
				6, 0, 0, 9, 9, 9, 9, 9, 9, 9,
				9, 9, 9, 1, 9
			},
			{
				6, 0, 0, 9, 9, 9, 9, 9, 9, 9,
				9, 9, 9, 9, 1
			}
		};
		DotNetToAlConversionKind = new byte[22, 20]
		{
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			},
			{
				6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
				6, 6, 6, 6, 6, 6, 6, 6, 0, 6
			},
			{
				0, 6, 6, 6, 6, 6, 0, 0, 0, 0,
				0, 0, 0, 6, 6, 0, 6, 0, 0, 0
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			},
			{
				6, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 0, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 0, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 6, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 6, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 0, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 6, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 0, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 6, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 0, 6, 6, 6, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 6, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 0, 6, 6, 6, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 6, 6, 0, 6, 6, 6, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 4, 6,
				0, 0, 0, 0, 0, 4, 5, 6, 0, 5
			},
			{
				0, 6, 6, 6, 6, 6, 6, 6, 0, 0,
				0, 0, 0, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 6, 0, 0, 0, 6, 0, 0, 0
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 6, 6,
				0, 0, 0, 0, 0, 6, 6, 6, 0, 0
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 6, 6, 0, 6, 0, 0, 0
			}
		};
		AlToDotNetConversionKind = new byte[20, 22]
		{
			{
				0, 8, 0, 0, 9, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 7, 0, 0, 8, 7, 9, 8, 8,
				8, 8, 8, 8, 8, 8, 8, 0, 8, 0,
				0, 0
			},
			{
				0, 8, 7, 0, 0, 9, 7, 7, 7, 8,
				8, 8, 8, 8, 8, 8, 8, 8, 8, 0,
				0, 0
			},
			{
				0, 8, 7, 0, 0, 7, 7, 7, 7, 7,
				9, 8, 8, 8, 8, 8, 8, 0, 8, 0,
				0, 0
			},
			{
				0, 8, 7, 0, 0, 7, 7, 7, 7, 7,
				9, 8, 8, 8, 8, 8, 8, 0, 8, 0,
				0, 0
			},
			{
				0, 8, 7, 0, 0, 7, 7, 7, 7, 7,
				7, 7, 9, 7, 8, 7, 7, 0, 8, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 7, 7, 7, 7, 7,
				7, 7, 7, 7, 9, 7, 7, 0, 7, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 7, 7, 7, 7, 7,
				7, 7, 9, 7, 8, 7, 7, 0, 8, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 9, 0, 0,
				7, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 9, 0, 0,
				7, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 9,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 9
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 9
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 8, 0, 0,
				9, 0
			},
			{
				7, 9, 7, 0, 7, 7, 7, 7, 7, 7,
				7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
				7, 7
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 9, 0, 0,
				7, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 9, 0, 0,
				7, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 9, 0, 0,
				0, 0
			}
		};
		AlToDotNetRefConversionKind = new byte[20, 22]
		{
			{
				0, 8, 0, 0, 9, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 9, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 9, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 7, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 7, 7, 7, 7,
				9, 7, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 7, 9, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 7, 9, 7, 7, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 9, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 9, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 9,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 9
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 9
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				9, 0
			},
			{
				7, 8, 7, 0, 7, 7, 0, 7, 0, 0,
				7, 0, 7, 0, 7, 0, 0, 7, 7, 7,
				7, 7
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0
			},
			{
				0, 8, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 9, 0, 0,
				0, 0
			}
		};
	}

	private static ConversionKind ClassifyDotNetToAlConversion(TypeSymbol source, TypeSymbol target)
	{
		ConversionKind conversionKind = FastClassifyDotNetToAlConversion(source, target);
		if (conversionKind == ConversionKind.NoConversion)
		{
			conversionKind = SlowClassifyDotNetToAlConversion(source, target);
		}
		return conversionKind;
	}

	private static ConversionKind ClassifyAlToDotNetConversion(TypeSymbol source, TypeSymbol target)
	{
		ConversionKind conversionKind = ConversionKind.NoConversion;
		if (IsStronglyTypedDotNetSymbol(target))
		{
			conversionKind = FastClassifyAlToDotNetConversion(source, target);
		}
		if (conversionKind == ConversionKind.NoConversion)
		{
			conversionKind = SlowClassifyAlToDotNetConversion(source, target);
		}
		return conversionKind;
	}

	private static ConversionKind ClassifyAlToDotNetRefConversion(TypeSymbol source, IDotNetTypeSymbol target)
	{
		int num = AlTypeToIndexForDotNetToAlConversion(source.GetNavTypeKindSafe());
		if (num < 0)
		{
			return ConversionKind.NoConversion;
		}
		int num2 = DotNetTypeToIndexForDotNetToAlConversion(target.DotNetTypeInfo.SpecialType);
		if (num2 < 0)
		{
			return ConversionKind.NoConversion;
		}
		return (ConversionKind)AlToDotNetRefConversionKind[num, num2];
	}

	private static ConversionKind SlowClassifyAlToDotNetConversion(TypeSymbol source, TypeSymbol target)
	{
		if (source.NavTypeKind == NavTypeKind.Variant && IsWeaklyTypedDotNetSymbol(target))
		{
			return ConversionKind.FromVariant;
		}
		if (IsConversionFromSystemEnum(source, target))
		{
			return ConversionKind.Identity;
		}
		return ConversionKind.NoConversion;
	}

	private static bool IsConversionFromSystemEnum(TypeSymbol source, TypeSymbol target)
	{
		if (source.IsSystemOption())
		{
			return GetDotNetSpecialType(target) == DotNetSpecialType.System_Object;
		}
		return false;
	}

	private static ConversionKind FastClassifyAlToDotNetConversion(TypeSymbol source, TypeSymbol target)
	{
		int num = AlTypeToIndexForDotNetToAlConversion(source.GetNavTypeKindSafe());
		if (num < 0)
		{
			return ConversionKind.NoConversion;
		}
		int num2 = DotNetTypeToIndexForDotNetToAlConversion(GetDotNetSpecialType(target));
		if (num2 < 0)
		{
			return ConversionKind.NoConversion;
		}
		return (ConversionKind)AlToDotNetConversionKind[num, num2];
	}

	private static int DotNetTypeToIndexForDotNetToAlConversion(DotNetSpecialType specialType)
	{
		return specialType switch
		{
			DotNetSpecialType.None => 0, 
			DotNetSpecialType.System_Object => 1, 
			DotNetSpecialType.System_Enum => 2, 
			DotNetSpecialType.System_Void => 3, 
			DotNetSpecialType.System_Boolean => 4, 
			DotNetSpecialType.System_Char => 5, 
			DotNetSpecialType.System_SByte => 6, 
			DotNetSpecialType.System_Byte => 7, 
			DotNetSpecialType.System_Int16 => 8, 
			DotNetSpecialType.System_UInt16 => 9, 
			DotNetSpecialType.System_Int32 => 10, 
			DotNetSpecialType.System_UInt32 => 11, 
			DotNetSpecialType.System_Int64 => 12, 
			DotNetSpecialType.System_UInt64 => 13, 
			DotNetSpecialType.System_Decimal => 14, 
			DotNetSpecialType.System_Single => 15, 
			DotNetSpecialType.System_Double => 16, 
			DotNetSpecialType.System_String => 17, 
			DotNetSpecialType.System_TimeSpan => 18, 
			DotNetSpecialType.System_DateTime => 19, 
			DotNetSpecialType.System_Guid => 20, 
			DotNetSpecialType.System_Stream => 21, 
			_ => -1, 
		};
	}

	private static int AlTypeToIndexForDotNetToAlConversion(NavTypeKind Kind)
	{
		return Kind switch
		{
			NavTypeKind.Boolean => 0, 
			NavTypeKind.Byte => 1, 
			NavTypeKind.Char => 2, 
			NavTypeKind.Option => 3, 
			NavTypeKind.Integer => 4, 
			NavTypeKind.BigInteger => 5, 
			NavTypeKind.Decimal => 6, 
			NavTypeKind.Duration => 7, 
			NavTypeKind.Code => 8, 
			NavTypeKind.Text => 9, 
			NavTypeKind.Date => 10, 
			NavTypeKind.Time => 11, 
			NavTypeKind.DateTime => 12, 
			NavTypeKind.InStream => 13, 
			NavTypeKind.OutStream => 14, 
			NavTypeKind.Guid => 15, 
			NavTypeKind.Variant => 16, 
			NavTypeKind.String => 17, 
			NavTypeKind.Label => 18, 
			NavTypeKind.BigText => 19, 
			_ => -1, 
		};
	}

	private static int DotNetTypeToIndex(DotNetSpecialType type)
	{
		return type switch
		{
			DotNetSpecialType.System_Object => 0, 
			DotNetSpecialType.System_String => 1, 
			DotNetSpecialType.System_Boolean => 2, 
			DotNetSpecialType.System_Char => 3, 
			DotNetSpecialType.System_SByte => 4, 
			DotNetSpecialType.System_Int16 => 5, 
			DotNetSpecialType.System_Int32 => 6, 
			DotNetSpecialType.System_Int64 => 7, 
			DotNetSpecialType.System_Byte => 8, 
			DotNetSpecialType.System_UInt16 => 9, 
			DotNetSpecialType.System_UInt32 => 10, 
			DotNetSpecialType.System_UInt64 => 11, 
			DotNetSpecialType.System_Single => 12, 
			DotNetSpecialType.System_Double => 13, 
			DotNetSpecialType.System_Decimal => 14, 
			_ => -1, 
		};
	}

	public static bool IsDotNetConversion(TypeSymbol source, TypeSymbol target)
	{
		DebugAssertHelper.Assert((object)source != null);
		DebugAssertHelper.Assert((object)target != null);
		if (!IsStronglyTypedDotNetSymbol(source) && !IsStronglyTypedDotNetSymbol(target) && !IsWeaklyTypedDotNetSymbol(source))
		{
			return IsWeaklyTypedDotNetSymbol(target);
		}
		return true;
	}

	public static bool IsDotNetSymbol(TypeSymbol symbol)
	{
		if (!IsStronglyTypedDotNetSymbol(symbol))
		{
			return IsWeaklyTypedDotNetSymbol(symbol);
		}
		return true;
	}

	public static bool IsStronglyTypedDotNetSymbol(TypeSymbol symbol)
	{
		if ((object)symbol == null)
		{
			return false;
		}
		return symbol.Kind == SymbolKind.DotNet;
	}

	private static bool IsWeaklyTypedDotNetSymbol(TypeSymbol symbol)
	{
		if ((object)symbol != null && symbol.Kind == SymbolKind.NamedType)
		{
			if ((object)symbol == null)
			{
				return false;
			}
			return symbol.NavTypeKind == NavTypeKind.DotNet;
		}
		return false;
	}

	internal static bool TryGetMatchingAlType(TypeSymbol symbol, out TypeSymbol alTypeSymbol)
	{
		DebugAssertHelper.Assert(symbol.NavTypeKind == NavTypeKind.DotNet);
		alTypeSymbol = null;
		NavTypeKind? matchingAlType = GetMatchingAlType(GetDotNetSpecialType(symbol));
		if (!matchingAlType.HasValue)
		{
			return false;
		}
		alTypeSymbol = TypeSymbol.GetNavTypeSymbol(matchingAlType.Value);
		return true;
	}

	private static NavTypeKind? GetMatchingAlType(DotNetSpecialType specialType)
	{
		switch (specialType)
		{
		case DotNetSpecialType.System_Boolean:
			return NavTypeKind.Boolean;
		case DotNetSpecialType.System_Char:
			return NavTypeKind.Char;
		case DotNetSpecialType.System_Byte:
			return NavTypeKind.Byte;
		case DotNetSpecialType.System_Enum:
		case DotNetSpecialType.System_SByte:
		case DotNetSpecialType.System_Int16:
		case DotNetSpecialType.System_UInt16:
		case DotNetSpecialType.System_Int32:
			return NavTypeKind.Integer;
		case DotNetSpecialType.System_UInt32:
		case DotNetSpecialType.System_Int64:
			return NavTypeKind.BigInteger;
		case DotNetSpecialType.System_UInt64:
		case DotNetSpecialType.System_Decimal:
		case DotNetSpecialType.System_Single:
		case DotNetSpecialType.System_Double:
			return NavTypeKind.Decimal;
		case DotNetSpecialType.System_String:
			return NavTypeKind.Text;
		case DotNetSpecialType.System_TimeSpan:
			return NavTypeKind.Duration;
		case DotNetSpecialType.System_DateTime:
			return NavTypeKind.DateTime;
		case DotNetSpecialType.System_Guid:
			return NavTypeKind.Guid;
		default:
			return null;
		}
	}

	internal static Conversion ClassifyByRefConversion(TypeSymbol source, TypeSymbol byRefTarget)
	{
		ConversionKind dotNetConversion;
		if (source.NavTypeKind == NavTypeKind.DotNet && byRefTarget.NavTypeKind == NavTypeKind.DotNet)
		{
			dotNetConversion = ClassifyDotNetToDotNetRefConversion(source, byRefTarget);
		}
		else if (byRefTarget.NavTypeKind == NavTypeKind.DotNet)
		{
			dotNetConversion = ClassifyAlToDotNetRefConversion(source, (IDotNetTypeSymbol)byRefTarget);
		}
		else
		{
			if (source.NavTypeKind != NavTypeKind.DotNet)
			{
				throw ExceptionUtilities.Unreachable;
			}
			dotNetConversion = ClassifyDotNetToAlByRefConversion(GetDotNetSpecialType(source), byRefTarget);
		}
		return GetConversionFromKind(dotNetConversion);
	}

	private static ConversionKind ClassifyDotNetToAlByRefConversion(DotNetSpecialType dotNetSpecialType, TypeSymbol byRefTarget)
	{
		switch (dotNetSpecialType)
		{
		case DotNetSpecialType.System_String:
		{
			NavTypeKind navTypeKindSafe = byRefTarget.GetNavTypeKindSafe();
			if ((uint)(navTypeKindSafe - 16646153) <= 1u)
			{
				return ConversionKind.PreservingFromDotNet;
			}
			return ConversionKind.NoConversion;
		}
		case DotNetSpecialType.System_DateTime:
		{
			NavTypeKind navTypeKindSafe = byRefTarget.GetNavTypeKindSafe();
			if (navTypeKindSafe == NavTypeKind.DateTime)
			{
				return ConversionKind.PreservingFromDotNet;
			}
			return ConversionKind.NoConversion;
		}
		default:
			return ConversionKind.NoConversion;
		}
	}

	private static ConversionKind ClassifyDotNetToDotNetRefConversion(TypeSymbol source, TypeSymbol byRefTarget)
	{
		if (IsDotNetCastingConversionPossible(source, byRefTarget))
		{
			DotNetTypeInfo dotNetTypeInfo = ((IDotNetTypeSymbol)source).DotNetTypeInfo;
			DotNetTypeInfo dotNetTypeInfo2 = ((IDotNetTypeSymbol)byRefTarget).DotNetTypeInfo;
			if (dotNetTypeInfo.IsEquivalentToReferenceType(dotNetTypeInfo2))
			{
				return ConversionKind.Identity;
			}
			ConversionKind conversionKind = ClassifyDotNetObjectConversion(dotNetTypeInfo.SpecialType, dotNetTypeInfo2.SpecialType);
			if (conversionKind != ConversionKind.NoConversion)
			{
				return conversionKind;
			}
			return ClassifyDowncastingOrUpcastingConversion(dotNetTypeInfo, dotNetTypeInfo2);
		}
		return ConversionKind.NoConversion;
	}

	public static Conversion ClassifyDotNetConversion(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ConversionKind dotNetConversion;
		if (source.NavTypeKind == NavTypeKind.DotNet && target.NavTypeKind == NavTypeKind.DotNet)
		{
			dotNetConversion = ClassifyDotNetToDotNetConversion(source, target);
		}
		else if (source.NavTypeKind == NavTypeKind.DotNet)
		{
			dotNetConversion = ClassifyDotNetToAlConversion(source, target);
		}
		else
		{
			if (target.NavTypeKind != NavTypeKind.DotNet)
			{
				throw ExceptionUtilities.Unreachable;
			}
			dotNetConversion = ClassifyAlToDotNetConversion(source, target);
		}
		return GetConversionFromKind(dotNetConversion);
	}

	private static ConversionKind SlowClassifyDotNetToAlConversion(TypeSymbol source, TypeSymbol target)
	{
		if (GetDotNetSpecialType(source) == DotNetSpecialType.System_Object)
		{
			return ConversionKind.PreservingFromDotNet;
		}
		return ConversionKind.NoConversion;
	}

	private static ConversionKind FastClassifyDotNetToAlConversion(TypeSymbol source, TypeSymbol target)
	{
		int num = DotNetTypeToIndexForDotNetToAlConversion(GetDotNetSpecialType(source));
		if (num < 0)
		{
			return ConversionKind.NoConversion;
		}
		int num2 = AlTypeToIndexForDotNetToAlConversion(target.GetNavTypeKindSafe());
		if (num2 < 0)
		{
			return ConversionKind.NoConversion;
		}
		return (ConversionKind)DotNetToAlConversionKind[num, num2];
	}

	private static ConversionKind ClassifyDotNetToDotNetConversion(TypeSymbol source, TypeSymbol target)
	{
		ConversionKind conversionKind = ConversionKind.NoConversion;
		DotNetSpecialType dotNetSpecialType = GetDotNetSpecialType(source);
		DotNetSpecialType dotNetSpecialType2 = GetDotNetSpecialType(target);
		if (dotNetSpecialType != DotNetSpecialType.None && dotNetSpecialType2 != DotNetSpecialType.None)
		{
			conversionKind = ClassifyFastDotNetConversion(dotNetSpecialType, dotNetSpecialType2);
		}
		if (conversionKind == ConversionKind.NoConversion)
		{
			conversionKind = ClassifySlowDotNetConversion(source, target);
		}
		return conversionKind;
	}

	private static ConversionKind ClassifySlowDotNetConversion(TypeSymbol source, TypeSymbol target)
	{
		DotNetSpecialType dotNetSpecialType = GetDotNetSpecialType(source);
		DotNetSpecialType dotNetSpecialType2 = GetDotNetSpecialType(target);
		ConversionKind conversionKind = ClassifyDotNetObjectConversion(dotNetSpecialType, dotNetSpecialType2);
		if (conversionKind != ConversionKind.NoConversion)
		{
			return conversionKind;
		}
		conversionKind = ClassifyDotNetEnumConversion(dotNetSpecialType, dotNetSpecialType2);
		if (conversionKind != ConversionKind.NoConversion)
		{
			return conversionKind;
		}
		conversionKind = ClassifyNullableConversion(dotNetSpecialType, dotNetSpecialType2);
		if (conversionKind != ConversionKind.NoConversion)
		{
			return conversionKind;
		}
		return ClassifyDotNetInheritanceConversion(source, target);
	}

	private static ConversionKind ClassifyNullableConversion(DotNetSpecialType sourceType, DotNetSpecialType targetType)
	{
		if (sourceType == DotNetSpecialType.System_Nullable && targetType == DotNetSpecialType.System_Nullable)
		{
			return ConversionKind.PreservingFromDotNetToDotNet;
		}
		if (sourceType == DotNetSpecialType.System_Nullable)
		{
			return ConversionKind.NarrowingFromDotNetToDotNet;
		}
		if (targetType == DotNetSpecialType.System_Nullable)
		{
			return ConversionKind.WideningFromDotNetToDotNet;
		}
		return ConversionKind.NoConversion;
	}

	private static ConversionKind ClassifyDotNetInheritanceConversion(TypeSymbol source, TypeSymbol target)
	{
		if (IsDotNetCastingConversionPossible(source, target))
		{
			DotNetTypeInfo dotNetTypeInfo = ((IDotNetTypeSymbol)source).DotNetTypeInfo;
			DotNetTypeInfo dotNetTypeInfo2 = ((IDotNetTypeSymbol)target).DotNetTypeInfo;
			return ClassifyDowncastingOrUpcastingConversion(dotNetTypeInfo, dotNetTypeInfo2);
		}
		return ConversionKind.NoConversion;
	}

	private static ConversionKind ClassifyDotNetEnumConversion(DotNetSpecialType sourceType, DotNetSpecialType targetType)
	{
		if (sourceType == DotNetSpecialType.System_Enum && targetType == DotNetSpecialType.System_Enum)
		{
			return ConversionKind.PreservingFromDotNetToDotNet;
		}
		if (sourceType == DotNetSpecialType.System_Enum && IsIntegralType(targetType))
		{
			return ConversionKind.NarrowingFromDotNetToDotNet;
		}
		if (targetType == DotNetSpecialType.System_Enum && IsIntegralType(sourceType))
		{
			return ConversionKind.NarrowingFromDotNetToDotNet;
		}
		return ConversionKind.NoConversion;
	}

	private static ConversionKind ClassifyDotNetObjectConversion(DotNetSpecialType sourceType, DotNetSpecialType targetType)
	{
		if (sourceType == DotNetSpecialType.System_Object && targetType == DotNetSpecialType.System_Object)
		{
			return ConversionKind.PreservingFromDotNetToDotNet;
		}
		if (sourceType == DotNetSpecialType.System_Object)
		{
			return ConversionKind.NarrowingFromDotNetToDotNet;
		}
		if (targetType == DotNetSpecialType.System_Object)
		{
			return ConversionKind.WideningFromDotNetToDotNet;
		}
		return ConversionKind.NoConversion;
	}

	private static ConversionKind ClassifyDowncastingOrUpcastingConversion(DotNetTypeInfo sourceTypeInfo, DotNetTypeInfo targetTypeInfo)
	{
		if (sourceTypeInfo.IsEquivalentToType(targetTypeInfo))
		{
			return ConversionKind.PreservingFromDotNetToDotNet;
		}
		if (sourceTypeInfo.IsInterface && targetTypeInfo.IsInterface)
		{
			if (targetTypeInfo.Interfaces.Contains((DotNetTypeInfo i) => i.IsEquivalentToType(sourceTypeInfo)))
			{
				return ConversionKind.NarrowingFromDotNetToDotNet;
			}
			if (sourceTypeInfo.Interfaces.Contains((DotNetTypeInfo i) => i.IsEquivalentToType(targetTypeInfo)))
			{
				return ConversionKind.WideningFromDotNetToDotNet;
			}
		}
		else if (sourceTypeInfo.IsInterface)
		{
			if (targetTypeInfo.Interfaces.Contains((DotNetTypeInfo i) => i.IsEquivalentToType(sourceTypeInfo)))
			{
				return ConversionKind.NarrowingFromDotNetToDotNet;
			}
		}
		else if (targetTypeInfo.IsInterface)
		{
			if (sourceTypeInfo.Interfaces.Contains((DotNetTypeInfo i) => i.IsEquivalentToType(targetTypeInfo)))
			{
				return ConversionKind.WideningFromDotNetToDotNet;
			}
		}
		else
		{
			if (targetTypeInfo.BaseTypes.Contains((DotNetTypeInfo i) => i.IsEquivalentToType(sourceTypeInfo)))
			{
				return ConversionKind.NarrowingFromDotNetToDotNet;
			}
			if (sourceTypeInfo.BaseTypes.Contains((DotNetTypeInfo i) => i.IsEquivalentToType(targetTypeInfo)))
			{
				return ConversionKind.WideningFromDotNetToDotNet;
			}
		}
		return ConversionKind.NoConversion;
	}

	private static bool IsDotNetCastingConversionPossible(TypeSymbol source, TypeSymbol target)
	{
		if (IsStronglyTypedDotNetSymbol(source))
		{
			return IsStronglyTypedDotNetSymbol(target);
		}
		return false;
	}

	private static bool IsIntegralType(DotNetSpecialType kind)
	{
		if ((uint)(kind - 6) <= 7u)
		{
			return true;
		}
		return false;
	}

	internal static DotNetSpecialType GetDotNetSpecialType(TypeSymbol type)
	{
		if (IsStronglyTypedDotNetSymbol(type))
		{
			return ((IDotNetTypeSymbol)type).DotNetTypeInfo.SpecialType;
		}
		if (IsWeaklyTypedDotNetSymbol(type))
		{
			return DotNetSpecialType.System_Object;
		}
		throw ExceptionUtilities.UnexpectedValue(type.Kind);
	}

	private static ConversionKind ClassifyFastDotNetConversion(DotNetSpecialType sourceType, DotNetSpecialType targetType)
	{
		int num = DotNetTypeToIndex(sourceType);
		if (num < 0)
		{
			return ConversionKind.NoConversion;
		}
		int num2 = DotNetTypeToIndex(targetType);
		if (num2 < 0)
		{
			return ConversionKind.NoConversion;
		}
		return ToAlConversionKind((DotNetConversionKind)DotNetToDotNetConversionKind[num, num2]);
	}

	private static Conversion GetConversionFromKind(ConversionKind dotNetConversion)
	{
		return dotNetConversion switch
		{
			ConversionKind.Identity => Conversion.Identity, 
			ConversionKind.NoConversion => Conversion.NoConversion, 
			ConversionKind.NarrowingFromDotNetToDotNet => Conversion.NarrowingFromDotNetToDotNet, 
			ConversionKind.WideningFromDotNetToDotNet => Conversion.WideningFromDotNetToDotNet, 
			ConversionKind.PreservingFromDotNetToDotNet => Conversion.PreservingFromDotNetToDotNet, 
			ConversionKind.NarrowingToDotNet => Conversion.NarrowingToDotNet, 
			ConversionKind.WideningToDotNet => Conversion.WideningToDotNet, 
			ConversionKind.PreservingToDotNet => Conversion.PreservingToDotNet, 
			ConversionKind.PreservingFromDotNet => Conversion.PreservingFromDotNet, 
			ConversionKind.WideningFromDotNet => Conversion.WideningFromDotNet, 
			ConversionKind.NarrowingFromDotNet => Conversion.NarrowingFromDotNet, 
			ConversionKind.FromVariant => Conversion.FromVariant, 
			_ => throw ExceptionUtilities.UnexpectedValue(dotNetConversion), 
		};
	}

	private static ConversionKind ToAlConversionKind(DotNetConversionKind conversionKind)
	{
		switch (conversionKind)
		{
		case DotNetConversionKind.Identity:
			return ConversionKind.Identity;
		case DotNetConversionKind.ImplicitNumeric:
		case DotNetConversionKind.ImplicitEnumeration:
		case DotNetConversionKind.ImplicitNullable:
		case DotNetConversionKind.ImplicitReference:
		case DotNetConversionKind.Boxing:
		case DotNetConversionKind.ImplicitDynamic:
			return ConversionKind.WideningFromDotNetToDotNet;
		case DotNetConversionKind.ExplicitDynamic:
		case DotNetConversionKind.ExplicitNumeric:
		case DotNetConversionKind.ExplicitEnumeration:
		case DotNetConversionKind.ExplicitNullable:
		case DotNetConversionKind.ExplicitReference:
		case DotNetConversionKind.Unboxing:
			return ConversionKind.NarrowingFromDotNetToDotNet;
		default:
			return ConversionKind.NoConversion;
		}
	}
}
