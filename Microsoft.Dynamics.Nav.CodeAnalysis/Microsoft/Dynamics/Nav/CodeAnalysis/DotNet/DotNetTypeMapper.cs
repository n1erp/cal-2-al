using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

internal class DotNetTypeMapper : IDotNetTypeMapper
{
	private readonly ConcurrentDictionary<DotNetTypeInfo, DotNetTypeSymbol> typesSymbolCache = new ConcurrentDictionary<DotNetTypeInfo, DotNetTypeSymbol>();

	public IDotNetTypeSymbol MapDeclaredType(IDotNetTypeDeclarationSymbol declaringSymbol, DotNetTypeInfo typeInfo)
	{
		DebugAssertHelper.Assert(typeInfo != null);
		DotNetTypeDeclarationSymbol dotNetTypeDeclarationSymbol = (DotNetTypeDeclarationSymbol)declaringSymbol;
		return new DotNetTypeSymbol(dotNetTypeDeclarationSymbol.ContainingModule, typeInfo, dotNetTypeDeclarationSymbol);
	}

	public IDotNetTypeSymbol MapReferencedType(IModuleSymbol containingModule, DotNetTypeInfo typeInfo)
	{
		return MapReferencedType((ModuleSymbol)containingModule, typeInfo);
	}

	private DotNetTypeSymbol MapReferencedType(ModuleSymbol module, DotNetTypeInfo typeInfo)
	{
		DebugAssertHelper.Assert(typeInfo != null);
		if (typeInfo.RequiresCompletion)
		{
			return typesSymbolCache.GetOrAdd(typeInfo, (DotNetTypeInfo _) => new DotNetTypeSymbol(module, typeInfo, null));
		}
		DotNetTypeSymbol orAdd = typesSymbolCache.GetOrAdd(typeInfo, (DotNetTypeInfo _) => new DotNetTypeSymbol(module, typeInfo, null));
		if (!orAdd.DotNetTypeInfo.RequiresCompletion)
		{
			return orAdd;
		}
		return typesSymbolCache.AddOrUpdate(typeInfo, (DotNetTypeInfo _) => new DotNetTypeSymbol(module, typeInfo, null), (DotNetTypeInfo existingKey, DotNetTypeSymbol existingSymbol) => existingSymbol.DotNetTypeInfo.RequiresCompletion ? new DotNetTypeSymbol(module, typeInfo, null) : existingSymbol);
	}

	public ImmutableArray<ISymbol> MapTypeMembers(IDotNetTypeSymbol declaringSymbol)
	{
		DotNetTypeSymbol dotNetTypeSymbol = (DotNetTypeSymbol)declaringSymbol;
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		if (dotNetTypeSymbol.DotNetTypeInfo.IsEnum)
		{
			MapEnumMebers(dotNetTypeSymbol, instance);
		}
		else
		{
			MapConstructors(dotNetTypeSymbol, instance);
		}
		MapMethods(dotNetTypeSymbol, instance);
		MapProperties(dotNetTypeSymbol, instance);
		MapEvents(dotNetTypeSymbol, instance);
		return instance.ToImmutableAndFree().CastArray<ISymbol>();
	}

	private static ImmutableArray<ParameterSymbol> MapDotNetEventParametersToAlParameters(ImmutableArray<ParameterSymbol> dotNetParameters)
	{
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
		foreach (ParameterSymbol item in dotNetParameters)
		{
			instance.Add(MapDotNetEventParameterToAlParameterIfPossible(item));
		}
		return instance.ToImmutableAndFree();
	}

	private static ParameterSymbol MapDotNetEventParameterToAlParameterIfPossible(ParameterSymbol dotNetParameterSymbol)
	{
		DotNetTypeSymbol dotNetTypeSymbol = (DotNetTypeSymbol)dotNetParameterSymbol.Type;
		NavTypeKind? navTypeKindForDotNetType = GetNavTypeKindForDotNetType(dotNetTypeSymbol.DotNetTypeInfo);
		if (!navTypeKindForDotNetType.HasValue)
		{
			return dotNetParameterSymbol;
		}
		bool isByRef = dotNetTypeSymbol.DotNetTypeInfo.IsByRef;
		TypeSymbol navTypeSymbol = TypeSymbol.GetNavTypeSymbol(navTypeKindForDotNetType.Value);
		return new SynthesizedParameterSymbol(dotNetParameterSymbol.ContainingSymbol, navTypeSymbol, dotNetParameterSymbol.Ordinal, isByRef, dotNetParameterSymbol.Name);
	}

	private static NavTypeKind? GetNavTypeKindForDotNetType(DotNetTypeInfo typeInfo)
	{
		switch (typeInfo.SpecialType)
		{
		case DotNetSpecialType.System_Object:
			return NavTypeKind.Variant;
		case DotNetSpecialType.System_Boolean:
			return NavTypeKind.Boolean;
		case DotNetSpecialType.System_Char:
			return NavTypeKind.Char;
		case DotNetSpecialType.System_Byte:
			return NavTypeKind.Byte;
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
		case DotNetSpecialType.None:
		case DotNetSpecialType.System_Enum:
		case DotNetSpecialType.System_Stream:
		case DotNetSpecialType.System_Nullable:
			return null;
		default:
			throw ExceptionUtilities.UnexpectedValue(typeInfo);
		}
	}

	private static void MapEnumMebers(DotNetTypeSymbol declaringType, ArrayBuilder<Symbol> memberBuilder)
	{
		DotNetEnumeratorInfo[] enumMembers = declaringType.DotNetTypeInfo.EnumMembers;
		foreach (DotNetEnumeratorInfo dotNetEnumeratorInfo in enumMembers)
		{
			memberBuilder.Add(new DotNetEnumMemberSymbol(declaringType, dotNetEnumeratorInfo.Name, dotNetEnumeratorInfo.Ordinal, declaringType.GetNextMemberId()));
		}
	}

	private void MapEvents(DotNetTypeSymbol containingType, ArrayBuilder<Symbol> memberBuilder)
	{
		DotNetEventInfo[] events = containingType.DotNetTypeInfo.Events;
		foreach (DotNetEventInfo dotNetEventInfo in events)
		{
			memberBuilder.Add(new DotNetEventSymbol(containingType, dotNetEventInfo.Name, MapDotNetEventParameters(containingType, dotNetEventInfo.Parameters), MapDotNetEventParametersToAlParameters, dotNetEventInfo.IsVisibleInControlAddIns));
		}
	}

	private Func<Symbol, ImmutableArray<ParameterSymbol>> MapDotNetEventParameters(DotNetTypeSymbol containingType, DotNetParameterInfo[] parameters = null)
	{
		return (Symbol symbol) => MapParameters(containingType, parameters)?.Invoke(symbol).ToImmutableArray() ?? ImmutableArray<ParameterSymbol>.Empty;
	}

	private void MapConstructors(DotNetTypeSymbol containingType, ArrayBuilder<Symbol> memberBuilder)
	{
		if (containingType.DotNetTypeInfo.IsValueType && containingType.DotNetTypeInfo.Constructors.Length == 0)
		{
			memberBuilder.Add(new DotNetConstructorTypeSymbol(containingType, DotNetUtilities.GetConstructorNameFromTypeName(containingType.DotNetTypeInfo.DisplayName), MapParameters(containingType)));
			return;
		}
		foreach (DotNetConstructorInfo accessibleConstructor in GetAccessibleConstructors(containingType.DotNetTypeInfo))
		{
			memberBuilder.Add(new DotNetConstructorTypeSymbol(containingType, accessibleConstructor.Name, MapParameters(containingType, accessibleConstructor.Parameters)));
		}
	}

	private IEnumerable<DotNetConstructorInfo> GetAccessibleConstructors(DotNetTypeInfo dotNetTypeInfo)
	{
		if (dotNetTypeInfo.Constructors == null || dotNetTypeInfo.Constructors.Length == 0)
		{
			return dotNetTypeInfo.Constructors;
		}
		HashSet<string> signatureSet = new HashSet<string>();
		string name = dotNetTypeInfo.Constructors[0].Name;
		AddSignaturesOfPotentiallyConflictingMembers(dotNetTypeInfo, name, signatureSet);
		return ExtractNonConflictingConstructors(dotNetTypeInfo, signatureSet);
	}

	private static IEnumerable<DotNetConstructorInfo> ExtractNonConflictingConstructors(DotNetTypeInfo dotNetTypeInfo, HashSet<string> signatureSet)
	{
		if (signatureSet.Count == 0)
		{
			return dotNetTypeInfo.Constructors;
		}
		return dotNetTypeInfo.Constructors.Where((DotNetConstructorInfo c) => !signatureSet.Contains(GetConstructorSignature(c)));
	}

	private static string GetConstructorSignature(DotNetConstructorInfo constructor)
	{
		return GetSignature(constructor.Name, constructor.Parameters);
	}

	private static string GetMethodSignature(DotNetMethodInfo method)
	{
		return GetSignature(method.Name, method.Parameters);
	}

	private static string GetSignature(string methodName, DotNetParameterInfo[] parameters)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			StringBuilder builder = instance.Builder;
			builder.Append(methodName);
			builder.Append("(");
			if (parameters != null && parameters.Length != 0)
			{
				for (int i = 0; i < parameters.Length; i++)
				{
					DotNetParameterInfo dotNetParameterInfo = parameters[i];
					if (i > 0)
					{
						builder.Append(",");
					}
					if (dotNetParameterInfo.IsParams)
					{
						builder.Append("...,");
					}
					builder.Append(dotNetParameterInfo.ParameterType.FullName);
				}
			}
			builder.Append(")");
			return builder.ToString();
		}
		finally
		{
			instance.Free();
		}
	}

	private static void AddSignaturesOfPotentiallyConflictingMembers(DotNetTypeInfo dotNetTypeInfo, string constructorName, HashSet<string> signatureSet)
	{
		DotNetMethodInfo[] methods = dotNetTypeInfo.Methods;
		foreach (DotNetMethodInfo dotNetMethodInfo in methods)
		{
			if (dotNetMethodInfo.Name == constructorName)
			{
				signatureSet.Add(GetMethodSignature(dotNetMethodInfo));
			}
		}
		DotNetPropertyInfo[] properties = dotNetTypeInfo.Properties;
		foreach (DotNetPropertyInfo dotNetPropertyInfo in properties)
		{
			if (dotNetPropertyInfo.Name == constructorName)
			{
				if (dotNetPropertyInfo.Getter != null)
				{
					signatureSet.Add(GetMethodSignature(dotNetPropertyInfo.Getter));
				}
				if (dotNetPropertyInfo.Setter != null)
				{
					signatureSet.Add(GetMethodSignature(dotNetPropertyInfo.Setter));
				}
			}
		}
	}

	private Func<Symbol, ParameterSymbol[]> MapParameters(DotNetTypeSymbol containingType, DotNetParameterInfo[] parameters = null)
	{
		if (parameters == null)
		{
			return null;
		}
		return (Symbol symbol) =>
		{
			ParameterSymbol[] array = new ParameterSymbol[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = new DotNetParameterSymbol(symbol, MapReferencedType(containingType.ContainingModule, parameters[i].ParameterType), parameters[i].Name, parameters[i].Ordinal, parameters[i].IsVar, parameters[i].IsOptional, parameters[i].IsParams);
			}
			return array;
		};
	}

	private Func<Symbol, ParameterSymbol[]> MapPropertyParameters(DotNetTypeSymbol containingType, DotNetParameterInfo[] getterParameters, DotNetParameterInfo[] setterParameters)
	{
		if (setterParameters == null && getterParameters == null)
		{
			return null;
		}
		return (Symbol symbol) =>
		{
			int num = getterParameters?.Length ?? 0;
			int num2 = setterParameters?.Length ?? 0;
			DebugAssertHelper.Assert(num2 >= num);
			ParameterSymbol[] array = new ParameterSymbol[num2];
			for (int i = 0; i < num2; i++)
			{
				array[i] = new DotNetParameterSymbol(symbol, MapReferencedType(containingType.ContainingModule, setterParameters[i].ParameterType), setterParameters[i].Name, setterParameters[i].Ordinal, setterParameters[i].IsVar, setterParameters[i].IsOptional || i >= num, setterParameters[i].IsParams);
			}
			return array;
		};
	}

	private void MapMethods(DotNetTypeSymbol containingType, ArrayBuilder<Symbol> memberBuilder)
	{
		DotNetMethodInfo[] methods = containingType.DotNetTypeInfo.Methods;
		foreach (DotNetMethodInfo dotNetMethodInfo in methods)
		{
			memberBuilder.Add(new DotNetMethodTypeSymbol(containingType, dotNetMethodInfo.Name, containingType.GetNextMemberId(), dotNetMethodInfo.IsVisibleInControlAddIns, dotNetMethodInfo.IsStatic, MapReturnType(containingType.ContainingModule, dotNetMethodInfo.ReturnValue.Type), dotNetMethodInfo.ReturnValue.IsOptional, MapParameters(containingType, dotNetMethodInfo.Parameters)));
		}
	}

	private Func<TypeSymbol> MapReturnType(ModuleSymbol moduleSymbol, DotNetTypeInfo typeInfo)
	{
		return () =>
		{
			if (typeInfo.RequiresCompletion)
			{
				DotNetTypeInfo dotNetTypeInfo = moduleSymbol.DotNetResolver.LoadType(typeInfo.Assembly, typeInfo.FullName);
				if (dotNetTypeInfo != null)
				{
					return MapReferencedType(moduleSymbol, dotNetTypeInfo);
				}
				return NavCorLib.None;
			}
			return MapReferencedType(moduleSymbol, typeInfo);
		};
	}

	private void MapProperties(DotNetTypeSymbol containingType, ArrayBuilder<Symbol> memberBuilder)
	{
		DotNetPropertyInfo[] properties = containingType.DotNetTypeInfo.Properties;
		foreach (DotNetPropertyInfo dotNetPropertyInfo in properties)
		{
			if (dotNetPropertyInfo.Getter != null && dotNetPropertyInfo.Setter != null)
			{
				memberBuilder.Add(new DotNetPropertyTypeSymbol(containingType, dotNetPropertyInfo.Name, containingType.GetNextMemberId(), dotNetPropertyInfo.IsVisibleInControlAddIns, DotNetInvokeKind.InvokePropertyGetOrSet, dotNetPropertyInfo.Setter.IsStatic, MapReturnType(containingType.ContainingModule, dotNetPropertyInfo.Getter.ReturnValue.Type), returnValueIsOptional: true, MapPropertyParameters(containingType, dotNetPropertyInfo.Getter.Parameters, dotNetPropertyInfo.Setter.Parameters)));
			}
			else if (dotNetPropertyInfo.Getter != null)
			{
				memberBuilder.Add(new DotNetPropertyTypeSymbol(containingType, dotNetPropertyInfo.Name, containingType.GetNextMemberId(), dotNetPropertyInfo.IsVisibleInControlAddIns, DotNetInvokeKind.InvokePropertyGet, dotNetPropertyInfo.Getter.IsStatic, MapReturnType(containingType.ContainingModule, dotNetPropertyInfo.Getter.ReturnValue.Type), dotNetPropertyInfo.Getter.ReturnValue.IsOptional, MapParameters(containingType, dotNetPropertyInfo.Getter.Parameters)));
			}
			else if (dotNetPropertyInfo.Setter != null)
			{
				memberBuilder.Add(new DotNetPropertyTypeSymbol(containingType, dotNetPropertyInfo.Name, containingType.GetNextMemberId(), dotNetPropertyInfo.IsVisibleInControlAddIns, DotNetInvokeKind.InvokePropertySet, dotNetPropertyInfo.Setter.IsStatic, MapReturnType(containingType.ContainingModule, dotNetPropertyInfo.Setter.ReturnValue.Type), dotNetPropertyInfo.Setter.ReturnValue.IsOptional, MapParameters(containingType, dotNetPropertyInfo.Setter.Parameters)));
			}
		}
	}
}
