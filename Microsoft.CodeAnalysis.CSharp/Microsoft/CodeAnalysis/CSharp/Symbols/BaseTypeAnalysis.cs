using System.Collections.Generic;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class BaseTypeAnalysis
{
	internal static bool ClassDependsOn(NamedTypeSymbol depends, NamedTypeSymbol on)
	{
		PooledHashSet<Symbol> instance = PooledHashSet<Symbol>.GetInstance();
		ClassDependsClosure(depends, depends.DeclaringCompilation, instance);
		bool result = instance.Contains(on);
		instance.Free();
		return result;
	}

	private static void ClassDependsClosure(NamedTypeSymbol type, CSharpCompilation currentCompilation, HashSet<Symbol> partialClosure)
	{
		if ((object)type == null)
		{
			return;
		}
		type = type.OriginalDefinition;
		if (partialClosure.Add(type))
		{
			ClassDependsClosure(type.GetDeclaredBaseType(null), currentCompilation, partialClosure);
			if (currentCompilation != null && type.IsFromCompilation(currentCompilation))
			{
				ClassDependsClosure(type.ContainingType, currentCompilation, partialClosure);
			}
		}
	}

	internal static bool StructDependsOn(NamedTypeSymbol depends, NamedTypeSymbol on)
	{
		PooledHashSet<Symbol> instance = PooledHashSet<Symbol>.GetInstance();
		StructDependsClosure(depends, instance, on);
		bool result = instance.Contains(on);
		instance.Free();
		return result;
	}

	private static void StructDependsClosure(NamedTypeSymbol type, HashSet<Symbol> partialClosure, NamedTypeSymbol on)
	{
		if ((object)type.OriginalDefinition == on)
		{
			partialClosure.Add(on);
		}
		else
		{
			if (!partialClosure.Add(type))
			{
				return;
			}
			foreach (Symbol item in type.GetMembersUnordered())
			{
				if (item is FieldSymbol fieldSymbol && fieldSymbol.Type.TypeKind == TypeKind.Struct && !fieldSymbol.IsStatic)
				{
					StructDependsClosure((NamedTypeSymbol)fieldSymbol.Type, partialClosure, on);
				}
			}
		}
	}

	internal static bool IsManagedType(NamedTypeSymbol type)
	{
		switch (IsManagedTypeHelper(type))
		{
		case ThreeState.True:
			return true;
		case ThreeState.False:
			return false;
		default:
		{
			PooledHashSet<Symbol> instance = PooledHashSet<Symbol>.GetInstance();
			bool result = DependsOnDefinitelyManagedType(type, instance);
			instance.Free();
			return result;
		}
		}
	}

	private static bool DependsOnDefinitelyManagedType(NamedTypeSymbol type, HashSet<Symbol> partialClosure)
	{
		if (partialClosure.Add(type))
		{
			foreach (Symbol instanceFieldsAndEvent in type.GetInstanceFieldsAndEvents())
			{
				FieldSymbol fieldSymbol = instanceFieldsAndEvent.Kind switch
				{
					SymbolKind.Field => (FieldSymbol)instanceFieldsAndEvent, 
					SymbolKind.Event => ((EventSymbol)instanceFieldsAndEvent).AssociatedField, 
					_ => throw ExceptionUtilities.UnexpectedValue(instanceFieldsAndEvent.Kind), 
				};
				if ((object)fieldSymbol == null || fieldSymbol.HasPointerType)
				{
					continue;
				}
				TypeSymbol type2 = fieldSymbol.Type;
				if (!(type2 is NamedTypeSymbol type3))
				{
					if (type2.IsManagedType)
					{
						return true;
					}
					continue;
				}
				switch (IsManagedTypeHelper(type3))
				{
				case ThreeState.True:
					return true;
				case ThreeState.Unknown:
					if (DependsOnDefinitelyManagedType(type3, partialClosure))
					{
						return true;
					}
					break;
				}
			}
		}
		return false;
	}

	private static ThreeState IsManagedTypeHelper(NamedTypeSymbol type)
	{
		if (type.IsEnumType())
		{
			type = type.GetEnumUnderlyingType();
		}
		switch (type.SpecialType)
		{
		case SpecialType.System_Void:
		case SpecialType.System_Boolean:
		case SpecialType.System_Char:
		case SpecialType.System_SByte:
		case SpecialType.System_Byte:
		case SpecialType.System_Int16:
		case SpecialType.System_UInt16:
		case SpecialType.System_Int32:
		case SpecialType.System_UInt32:
		case SpecialType.System_Int64:
		case SpecialType.System_UInt64:
		case SpecialType.System_Decimal:
		case SpecialType.System_Single:
		case SpecialType.System_Double:
		case SpecialType.System_IntPtr:
		case SpecialType.System_UIntPtr:
		case SpecialType.System_TypedReference:
		case SpecialType.System_ArgIterator:
		case SpecialType.System_RuntimeArgumentHandle:
			return ThreeState.False;
		default:
			if (type.AllTypeArgumentCount() > 0)
			{
				return ThreeState.True;
			}
			return type.TypeKind switch
			{
				TypeKind.Enum => ThreeState.False, 
				TypeKind.Struct => ThreeState.Unknown, 
				_ => ThreeState.True, 
			};
		}
	}

	internal static bool InterfaceDependsOn(NamedTypeSymbol depends, NamedTypeSymbol on)
	{
		PooledHashSet<Symbol> instance = PooledHashSet<Symbol>.GetInstance();
		InterfaceDependsClosure(depends, instance);
		bool result = instance.Contains(on);
		instance.Free();
		return result;
	}

	private static void InterfaceDependsClosure(NamedTypeSymbol type, HashSet<Symbol> partialClosure)
	{
		type = type.OriginalDefinition;
		if (partialClosure.Add(type))
		{
			foreach (NamedTypeSymbol declaredInterface in type.GetDeclaredInterfaces(null))
			{
				InterfaceDependsClosure(declaredInterface, partialClosure);
			}
		}
	}
}
