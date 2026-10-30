using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class OverriddenOrHiddenMembersHelpers
{
	internal static OverriddenOrHiddenMembersResult MakeOverriddenOrHiddenMembers(this MethodSymbol member)
	{
		return MakeOverriddenOrHiddenMembersWorker(member);
	}

	internal static OverriddenOrHiddenMembersResult MakeOverriddenOrHiddenMembers(this PropertySymbol member)
	{
		return MakeOverriddenOrHiddenMembersWorker(member);
	}

	internal static OverriddenOrHiddenMembersResult MakeOverriddenOrHiddenMembers(this EventSymbol member)
	{
		return MakeOverriddenOrHiddenMembersWorker(member);
	}

	private static OverriddenOrHiddenMembersResult MakeOverriddenOrHiddenMembersWorker(Symbol member)
	{
		if (!CanOverrideOrHide(member))
		{
			return OverriddenOrHiddenMembersResult.Empty;
		}
		if (member.IsAccessor())
		{
			MethodSymbol methodSymbol = member as MethodSymbol;
			Symbol associatedSymbol = methodSymbol.AssociatedSymbol;
			if ((object)associatedSymbol != null)
			{
				if (associatedSymbol.Kind == SymbolKind.Property)
				{
					return MakePropertyAccessorOverriddenOrHiddenMembers(methodSymbol, (PropertySymbol)associatedSymbol);
				}
				return MakeEventAccessorOverriddenOrHiddenMembers(methodSymbol, (EventSymbol)associatedSymbol);
			}
		}
		NamedTypeSymbol containingType = member.ContainingType;
		bool dangerous_IsFromSomeCompilation = member.Dangerous_IsFromSomeCompilation;
		if (containingType.IsInterface)
		{
			return MakeInterfaceOverriddenOrHiddenMembers(member, dangerous_IsFromSomeCompilation);
		}
		Symbol currTypeBestMatch = null;
		ArrayBuilder<Symbol> hiddenBuilder = null;
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = containingType.BaseTypeNoUseSiteDiagnostics;
		while ((object)baseTypeNoUseSiteDiagnostics != null && (object)currTypeBestMatch == null && hiddenBuilder == null)
		{
			FindOverriddenOrHiddenMembersInType(member, dangerous_IsFromSomeCompilation, containingType, baseTypeNoUseSiteDiagnostics, out currTypeBestMatch, out var _, out hiddenBuilder);
			baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
		}
		FindRelatedMembers(member.IsOverride, dangerous_IsFromSomeCompilation, member.Kind, currTypeBestMatch, out var overriddenMembers, out var runtimeOverriddenMembers, ref hiddenBuilder);
		ImmutableArray<Symbol> hiddenMembers = hiddenBuilder?.ToImmutableAndFree() ?? ImmutableArray<Symbol>.Empty;
		return OverriddenOrHiddenMembersResult.Create(overriddenMembers, hiddenMembers, runtimeOverriddenMembers);
	}

	private static OverriddenOrHiddenMembersResult MakePropertyAccessorOverriddenOrHiddenMembers(MethodSymbol accessor, PropertySymbol associatedProperty)
	{
		bool flag = accessor.MethodKind == MethodKind.PropertyGet;
		MethodSymbol methodSymbol = null;
		ArrayBuilder<Symbol> builder = null;
		OverriddenOrHiddenMembersResult overriddenOrHiddenMembers = associatedProperty.OverriddenOrHiddenMembers;
		foreach (Symbol hiddenMember in overriddenOrHiddenMembers.HiddenMembers)
		{
			if (hiddenMember.Kind == SymbolKind.Property)
			{
				PropertySymbol propertySymbol = (PropertySymbol)hiddenMember;
				MethodSymbol methodSymbol2 = (flag ? propertySymbol.GetMethod : propertySymbol.SetMethod);
				if ((object)methodSymbol2 != null)
				{
					AccessOrGetInstance(ref builder).Add(methodSymbol2);
				}
			}
		}
		if (overriddenOrHiddenMembers.OverriddenMembers.Any())
		{
			PropertySymbol property = (PropertySymbol)overriddenOrHiddenMembers.OverriddenMembers[0];
			MethodSymbol methodSymbol3 = (flag ? property.GetOwnOrInheritedGetMethod() : property.GetOwnOrInheritedSetMethod());
			if ((object)methodSymbol3 != null)
			{
				methodSymbol = methodSymbol3;
			}
		}
		bool dangerous_IsFromSomeCompilation = accessor.Dangerous_IsFromSomeCompilation;
		ImmutableArray<Symbol> overriddenMembers = ImmutableArray<Symbol>.Empty;
		ImmutableArray<Symbol> runtimeOverriddenMembers = ImmutableArray<Symbol>.Empty;
		if ((object)methodSymbol != null && IsOverriddenSymbolAccessible(methodSymbol, accessor.ContainingType) && (dangerous_IsFromSomeCompilation ? MemberSignatureComparer.CSharpAccessorOverrideComparer.Equals(accessor, methodSymbol) : MemberSignatureComparer.RuntimeSignatureComparer.Equals(accessor, methodSymbol)))
		{
			FindRelatedMembers(accessor.IsOverride, dangerous_IsFromSomeCompilation, accessor.Kind, methodSymbol, out overriddenMembers, out runtimeOverriddenMembers, ref builder);
		}
		ImmutableArray<Symbol> hiddenMembers = builder?.ToImmutableAndFree() ?? ImmutableArray<Symbol>.Empty;
		return OverriddenOrHiddenMembersResult.Create(overriddenMembers, hiddenMembers, runtimeOverriddenMembers);
	}

	private static OverriddenOrHiddenMembersResult MakeEventAccessorOverriddenOrHiddenMembers(MethodSymbol accessor, EventSymbol associatedEvent)
	{
		bool flag = accessor.MethodKind == MethodKind.EventAdd;
		MethodSymbol methodSymbol = null;
		ArrayBuilder<Symbol> builder = null;
		OverriddenOrHiddenMembersResult overriddenOrHiddenMembers = associatedEvent.OverriddenOrHiddenMembers;
		foreach (Symbol hiddenMember in overriddenOrHiddenMembers.HiddenMembers)
		{
			if (hiddenMember.Kind == SymbolKind.Event)
			{
				EventSymbol eventSymbol = (EventSymbol)hiddenMember;
				MethodSymbol methodSymbol2 = (flag ? eventSymbol.AddMethod : eventSymbol.RemoveMethod);
				if ((object)methodSymbol2 != null)
				{
					AccessOrGetInstance(ref builder).Add(methodSymbol2);
				}
			}
		}
		if (overriddenOrHiddenMembers.OverriddenMembers.Any())
		{
			MethodSymbol ownOrInheritedAccessor = ((EventSymbol)overriddenOrHiddenMembers.OverriddenMembers[0]).GetOwnOrInheritedAccessor(flag);
			if ((object)ownOrInheritedAccessor != null)
			{
				methodSymbol = ownOrInheritedAccessor;
			}
		}
		bool dangerous_IsFromSomeCompilation = accessor.Dangerous_IsFromSomeCompilation;
		ImmutableArray<Symbol> overriddenMembers = ImmutableArray<Symbol>.Empty;
		ImmutableArray<Symbol> runtimeOverriddenMembers = ImmutableArray<Symbol>.Empty;
		if ((object)methodSymbol != null && IsOverriddenSymbolAccessible(methodSymbol, accessor.ContainingType) && (dangerous_IsFromSomeCompilation ? MemberSignatureComparer.CSharpAccessorOverrideComparer.Equals(accessor, methodSymbol) : MemberSignatureComparer.RuntimeSignatureComparer.Equals(accessor, methodSymbol)))
		{
			FindRelatedMembers(accessor.IsOverride, dangerous_IsFromSomeCompilation, accessor.Kind, methodSymbol, out overriddenMembers, out runtimeOverriddenMembers, ref builder);
		}
		ImmutableArray<Symbol> hiddenMembers = builder?.ToImmutableAndFree() ?? ImmutableArray<Symbol>.Empty;
		return OverriddenOrHiddenMembersResult.Create(overriddenMembers, hiddenMembers, runtimeOverriddenMembers);
	}

	private static OverriddenOrHiddenMembersResult MakeInterfaceOverriddenOrHiddenMembers(Symbol member, bool memberIsFromSomeCompilation)
	{
		NamedTypeSymbol containingType = member.ContainingType;
		PooledHashSet<NamedTypeSymbol> instance = PooledHashSet<NamedTypeSymbol>.GetInstance();
		PooledHashSet<NamedTypeSymbol> instance2 = PooledHashSet<NamedTypeSymbol>.GetInstance();
		ArrayBuilder<Symbol> builder = null;
		foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in containingType.AllInterfacesNoUseSiteDiagnostics)
		{
			if (instance2.Contains(allInterfacesNoUseSiteDiagnostic))
			{
				continue;
			}
			FindOverriddenOrHiddenMembersInType(member, memberIsFromSomeCompilation, containingType, allInterfacesNoUseSiteDiagnostic, out var currTypeBestMatch, out var currTypeHasSameKindNonMatch, out var hiddenBuilder);
			bool flag = (object)currTypeBestMatch != null;
			if (flag)
			{
				foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic2 in allInterfacesNoUseSiteDiagnostic.AllInterfacesNoUseSiteDiagnostics)
				{
					instance2.Add(allInterfacesNoUseSiteDiagnostic2);
				}
				AccessOrGetInstance(ref builder).Add(currTypeBestMatch);
			}
			if (hiddenBuilder != null)
			{
				if (!instance.Contains(allInterfacesNoUseSiteDiagnostic))
				{
					if (!flag)
					{
						foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic3 in allInterfacesNoUseSiteDiagnostic.AllInterfacesNoUseSiteDiagnostics)
						{
							instance2.Add(allInterfacesNoUseSiteDiagnostic3);
						}
					}
					AccessOrGetInstance(ref builder).AddRange(hiddenBuilder);
				}
				hiddenBuilder.Free();
			}
			else if (currTypeHasSameKindNonMatch && !flag)
			{
				foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic4 in allInterfacesNoUseSiteDiagnostic.AllInterfacesNoUseSiteDiagnostics)
				{
					instance.Add(allInterfacesNoUseSiteDiagnostic4);
				}
			}
		}
		instance.Free();
		instance2.Free();
		ImmutableArray<Symbol> overriddenMembers = ImmutableArray<Symbol>.Empty;
		ImmutableArray<Symbol> runtimeOverriddenMembers = ImmutableArray<Symbol>.Empty;
		if (builder != null)
		{
			ArrayBuilder<Symbol> hiddenBuilder2 = null;
			foreach (Symbol item in builder)
			{
				FindRelatedMembers(member.IsOverride, memberIsFromSomeCompilation, member.Kind, item, out overriddenMembers, out runtimeOverriddenMembers, ref hiddenBuilder2);
			}
			builder.Free();
			builder = hiddenBuilder2;
		}
		ImmutableArray<Symbol> hiddenMembers = builder?.ToImmutableAndFree() ?? ImmutableArray<Symbol>.Empty;
		return OverriddenOrHiddenMembersResult.Create(overriddenMembers, hiddenMembers, runtimeOverriddenMembers);
	}

	private static void FindOverriddenOrHiddenMembersInType(Symbol member, bool memberIsFromSomeCompilation, NamedTypeSymbol memberContainingType, NamedTypeSymbol currType, out Symbol currTypeBestMatch, out bool currTypeHasSameKindNonMatch, out ArrayBuilder<Symbol> hiddenBuilder)
	{
		currTypeBestMatch = null;
		currTypeHasSameKindNonMatch = false;
		hiddenBuilder = null;
		bool flag = false;
		int num = int.MaxValue;
		IEqualityComparer<Symbol> equalityComparer = (memberIsFromSomeCompilation ? MemberSignatureComparer.CSharpCustomModifierOverrideComparer : MemberSignatureComparer.RuntimePlusRefOutSignatureComparer);
		IEqualityComparer<Symbol> equalityComparer2 = (memberIsFromSomeCompilation ? MemberSignatureComparer.CSharpOverrideComparer : MemberSignatureComparer.RuntimeSignatureComparer);
		SymbolKind kind = member.Kind;
		int memberArity = member.GetMemberArity();
		foreach (Symbol member2 in currType.GetMembers(member.Name))
		{
			if (!IsOverriddenSymbolAccessible(member2, memberContainingType) || (member2.IsAccessor() && !((MethodSymbol)member2).IsIndexedPropertyAccessor()))
			{
				continue;
			}
			if (member2.Kind != kind)
			{
				int memberArity2 = member2.GetMemberArity();
				if (memberArity2 == memberArity || (kind == SymbolKind.Method && memberArity2 == 0))
				{
					AddHiddenMemberIfApplicable(ref hiddenBuilder, member.Kind, member2);
				}
			}
			else
			{
				if (flag)
				{
					continue;
				}
				if (equalityComparer.Equals(member, member2))
				{
					flag = true;
					currTypeBestMatch = member2;
				}
				else if (equalityComparer2.Equals(member, member2))
				{
					int num2 = CustomModifierCount(member2);
					if (num2 < num)
					{
						num = num2;
						currTypeBestMatch = member2;
					}
				}
				else
				{
					currTypeHasSameKindNonMatch = true;
				}
			}
		}
		if (!(flag & memberIsFromSomeCompilation) || !member.IsDefinition || !TypeOrReturnTypeHasCustomModifiers(currTypeBestMatch))
		{
			return;
		}
		Symbol symbol = currTypeBestMatch;
		foreach (Symbol member3 in currType.GetMembers(member.Name))
		{
			if (member3.Kind == currTypeBestMatch.Kind && (object)member3 != currTypeBestMatch && MemberSignatureComparer.CSharpOverrideComparer.Equals(member3, currTypeBestMatch))
			{
				int num3 = CustomModifierCount(member3);
				if (num3 < num)
				{
					num = num3;
					symbol = member3;
				}
			}
		}
		currTypeBestMatch = symbol;
	}

	private static void FindRelatedMembers(bool isOverride, bool overridingMemberIsFromSomeCompilation, SymbolKind overridingMemberKind, Symbol representativeMember, out ImmutableArray<Symbol> overriddenMembers, out ImmutableArray<Symbol> runtimeOverriddenMembers, ref ArrayBuilder<Symbol> hiddenBuilder)
	{
		overriddenMembers = ImmutableArray<Symbol>.Empty;
		runtimeOverriddenMembers = ImmutableArray<Symbol>.Empty;
		if ((object)representativeMember == null)
		{
			return;
		}
		bool flag = !representativeMember.ContainingType.IsDefinition || representativeMember.IsIndexer();
		if (isOverride)
		{
			if (flag)
			{
				ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
				ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance();
				instance.Add(representativeMember);
				instance2.Add(representativeMember);
				FindOtherOverriddenMethodsInContainingType(representativeMember, overridingMemberIsFromSomeCompilation, instance, instance2);
				overriddenMembers = instance.ToImmutableAndFree();
				runtimeOverriddenMembers = instance2.ToImmutableAndFree();
			}
			else
			{
				overriddenMembers = ImmutableArray.Create(representativeMember);
				runtimeOverriddenMembers = overriddenMembers;
			}
		}
		else
		{
			AddHiddenMemberIfApplicable(ref hiddenBuilder, overridingMemberKind, representativeMember);
			if (flag)
			{
				FindOtherHiddenMembersInContainingType(overridingMemberKind, representativeMember, ref hiddenBuilder);
			}
		}
	}

	private static void AddHiddenMemberIfApplicable(ref ArrayBuilder<Symbol> hiddenBuilder, SymbolKind hidingMemberKind, Symbol hiddenMember)
	{
		if (hiddenMember.Kind != SymbolKind.Method || ((MethodSymbol)hiddenMember).CanBeHiddenByMemberKind(hidingMemberKind))
		{
			AccessOrGetInstance(ref hiddenBuilder).Add(hiddenMember);
		}
	}

	private static ArrayBuilder<T> AccessOrGetInstance<T>(ref ArrayBuilder<T> builder)
	{
		if (builder == null)
		{
			builder = ArrayBuilder<T>.GetInstance();
		}
		return builder;
	}

	private static void FindOtherOverriddenMethodsInContainingType(Symbol representativeMember, bool overridingMemberIsFromSomeCompilation, ArrayBuilder<Symbol> overriddenBuilder, ArrayBuilder<Symbol> runtimeOverriddenBuilder)
	{
		int num = -1;
		foreach (Symbol member in representativeMember.ContainingType.GetMembers(representativeMember.Name))
		{
			if (member.Kind != representativeMember.Kind || !(member != representativeMember))
			{
				continue;
			}
			if (overridingMemberIsFromSomeCompilation)
			{
				if (num < 0)
				{
					num = representativeMember.CustomModifierCount();
				}
				if (MemberSignatureComparer.CSharpOverrideComparer.Equals(member, representativeMember) && member.CustomModifierCount() == num)
				{
					overriddenBuilder.Add(member);
				}
			}
			else if (MemberSignatureComparer.CSharpCustomModifierOverrideComparer.Equals(member, representativeMember))
			{
				overriddenBuilder.Add(member);
			}
			if (MemberSignatureComparer.RuntimeSignatureComparer.Equals(member, representativeMember))
			{
				runtimeOverriddenBuilder.Add(member);
			}
		}
	}

	private static void FindOtherHiddenMembersInContainingType(SymbolKind hidingMemberKind, Symbol representativeMember, ref ArrayBuilder<Symbol> hiddenBuilder)
	{
		IEqualityComparer<Symbol> cSharpCustomModifierOverrideComparer = MemberSignatureComparer.CSharpCustomModifierOverrideComparer;
		foreach (Symbol member in representativeMember.ContainingType.GetMembers(representativeMember.Name))
		{
			if (member.Kind == representativeMember.Kind && member != representativeMember && cSharpCustomModifierOverrideComparer.Equals(member, representativeMember))
			{
				AddHiddenMemberIfApplicable(ref hiddenBuilder, hidingMemberKind, member);
			}
		}
	}

	private static bool CanOverrideOrHide(Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Event:
		case SymbolKind.Property:
			return !member.IsExplicitInterfaceImplementation();
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)member;
			if (MethodSymbol.CanOverrideOrHide(methodSymbol.MethodKind))
			{
				return (object)methodSymbol == methodSymbol.ConstructedFrom;
			}
			return false;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	private static bool TypeOrReturnTypeHasCustomModifiers(Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)member;
			if (!methodSymbol.ReturnTypeCustomModifiers.Any() && !methodSymbol.RefCustomModifiers.Any())
			{
				return methodSymbol.ReturnType.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds: false);
			}
			return true;
		}
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)member;
			if (!propertySymbol.TypeCustomModifiers.Any() && !propertySymbol.RefCustomModifiers.Any())
			{
				return propertySymbol.Type.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds: false);
			}
			return true;
		}
		case SymbolKind.Event:
			return ((EventSymbol)member).Type.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds: false);
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	private static int CustomModifierCount(Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).CustomModifierCount(), 
			SymbolKind.Property => ((PropertySymbol)member).CustomModifierCount(), 
			SymbolKind.Event => ((EventSymbol)member).Type.CustomModifierCount(), 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static bool RequiresExplicitOverride(this MethodSymbol method)
	{
		if (method.IsOverride)
		{
			MethodSymbol overriddenMethod = method.OverriddenMethod;
			if ((object)overriddenMethod == null)
			{
				return false;
			}
			MethodSymbol firstRuntimeOverriddenMethodIgnoringNewSlot = method.GetFirstRuntimeOverriddenMethodIgnoringNewSlot(ignoreInterfaceImplementationChanges: true);
			if (overriddenMethod != firstRuntimeOverriddenMethodIgnoringNewSlot)
			{
				return method.IsAccessor() != firstRuntimeOverriddenMethodIgnoringNewSlot.IsAccessor();
			}
			return false;
		}
		return false;
	}

	internal static MethodSymbol GetFirstRuntimeOverriddenMethodIgnoringNewSlot(this MethodSymbol method, bool ignoreInterfaceImplementationChanges)
	{
		if (!method.IsMetadataVirtual(ignoreInterfaceImplementationChanges))
		{
			return null;
		}
		NamedTypeSymbol containingType = method.ContainingType;
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = containingType.BaseTypeNoUseSiteDiagnostics;
		while ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			foreach (Symbol member in baseTypeNoUseSiteDiagnostics.GetMembers(method.Name))
			{
				if (member.Kind == SymbolKind.Method && IsOverriddenSymbolAccessible(member, containingType) && MemberSignatureComparer.RuntimeSignatureComparer.Equals(method, member))
				{
					MethodSymbol methodSymbol = (MethodSymbol)member;
					if (methodSymbol.IsMetadataVirtual(ignoreInterfaceImplementationChanges))
					{
						return methodSymbol;
					}
				}
			}
			baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
		}
		return null;
	}

	private static bool IsOverriddenSymbolAccessible(Symbol overridden, NamedTypeSymbol overridingContainingType)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		return AccessCheck.IsSymbolAccessible(overridden.OriginalDefinition, overridingContainingType.OriginalDefinition, ref useSiteDiagnostics);
	}
}
