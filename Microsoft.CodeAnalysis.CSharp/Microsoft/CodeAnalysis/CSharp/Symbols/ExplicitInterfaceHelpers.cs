using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ExplicitInterfaceHelpers
{
	public static string GetMemberName(Binder binder, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierOpt, string name)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		string memberNameAndInterfaceSymbol = GetMemberNameAndInterfaceSymbol(binder, explicitInterfaceSpecifierOpt, name, instance, out var _, out var _);
		instance.Free();
		return memberNameAndInterfaceSymbol;
	}

	public static string GetMemberNameAndInterfaceSymbol(Binder binder, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierOpt, string name, DiagnosticBag diagnostics, out TypeSymbol explicitInterfaceTypeOpt, out string aliasQualifierOpt)
	{
		if (explicitInterfaceSpecifierOpt == null)
		{
			explicitInterfaceTypeOpt = null;
			aliasQualifierOpt = null;
			return name;
		}
		binder = binder.WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.SuppressObsoleteChecks);
		NameSyntax name2 = explicitInterfaceSpecifierOpt.Name;
		explicitInterfaceTypeOpt = binder.BindType(name2, diagnostics);
		aliasQualifierOpt = name2.GetAliasQualifierOpt();
		return GetMemberName(name, explicitInterfaceTypeOpt, aliasQualifierOpt);
	}

	public static string GetMemberName(string name, TypeSymbol explicitInterfaceTypeOpt, string aliasQualifierOpt)
	{
		if ((object)explicitInterfaceTypeOpt == null)
		{
			return name;
		}
		string text = explicitInterfaceTypeOpt.ToDisplayString(SymbolDisplayFormat.ExplicitInterfaceImplementationFormat);
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		if (!string.IsNullOrEmpty(aliasQualifierOpt))
		{
			builder.Append(aliasQualifierOpt);
			builder.Append("::");
		}
		string text2 = text;
		foreach (char c in text2)
		{
			if (c != ' ')
			{
				builder.Append(c);
			}
		}
		builder.Append(".");
		builder.Append(name);
		return instance.ToStringAndFree();
	}

	public static string GetMethodNameWithoutInterfaceName(this MethodSymbol method)
	{
		if (method.MethodKind != MethodKind.ExplicitInterfaceImplementation)
		{
			return method.Name;
		}
		return GetMemberNameWithoutInterfaceName(method.Name);
	}

	public static string GetMemberNameWithoutInterfaceName(string fullName)
	{
		int num = fullName.LastIndexOf('.');
		if (num <= 0)
		{
			return fullName;
		}
		return fullName.Substring(num + 1);
	}

	public static ImmutableArray<T> SubstituteExplicitInterfaceImplementations<T>(ImmutableArray<T> unsubstitutedExplicitInterfaceImplementations, TypeMap map) where T : Symbol
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		foreach (T item2 in unsubstitutedExplicitInterfaceImplementations)
		{
			NamedTypeSymbol containingType = item2.ContainingType;
			NamedTypeSymbol namedTypeSymbol = map.SubstituteNamedType(containingType);
			string name = item2.Name;
			T item = null;
			foreach (Symbol member in namedTypeSymbol.GetMembers(name))
			{
				if (member.OriginalDefinition == item2.OriginalDefinition)
				{
					item = (T)member;
					break;
				}
			}
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	internal static MethodSymbol FindExplicitlyImplementedMethod(this MethodSymbol implementingMethod, TypeSymbol explicitInterfaceType, string interfaceMethodName, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax, DiagnosticBag diagnostics)
	{
		return (MethodSymbol)FindExplicitlyImplementedMember(implementingMethod, explicitInterfaceType, interfaceMethodName, explicitInterfaceSpecifierSyntax, diagnostics);
	}

	internal static PropertySymbol FindExplicitlyImplementedProperty(this PropertySymbol implementingProperty, TypeSymbol explicitInterfaceType, string interfacePropertyName, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax, DiagnosticBag diagnostics)
	{
		return (PropertySymbol)FindExplicitlyImplementedMember(implementingProperty, explicitInterfaceType, interfacePropertyName, explicitInterfaceSpecifierSyntax, diagnostics);
	}

	internal static EventSymbol FindExplicitlyImplementedEvent(this EventSymbol implementingEvent, TypeSymbol explicitInterfaceType, string interfaceEventName, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax, DiagnosticBag diagnostics)
	{
		return (EventSymbol)FindExplicitlyImplementedMember(implementingEvent, explicitInterfaceType, interfaceEventName, explicitInterfaceSpecifierSyntax, diagnostics);
	}

	private static Symbol FindExplicitlyImplementedMember(Symbol implementingMember, TypeSymbol explicitInterfaceType, string interfaceMemberName, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax, DiagnosticBag diagnostics)
	{
		if ((object)explicitInterfaceType == null)
		{
			return null;
		}
		Location location = implementingMember.Locations[0];
		NamedTypeSymbol containingType = implementingMember.ContainingType;
		TypeKind typeKind = containingType.TypeKind;
		if (typeKind != TypeKind.Class && typeKind != TypeKind.Struct)
		{
			diagnostics.Add(ErrorCode.ERR_ExplicitInterfaceImplementationInNonClassOrStruct, location, implementingMember);
			return null;
		}
		if (!explicitInterfaceType.IsInterfaceType())
		{
			SourceLocation location2 = new SourceLocation(explicitInterfaceSpecifierSyntax.Name);
			diagnostics.Add(ErrorCode.ERR_ExplicitInterfaceImplementationNotInterface, location2, explicitInterfaceType);
			return null;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)explicitInterfaceType;
		if (!containingType.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Contains(namedTypeSymbol))
		{
			SourceLocation location3 = new SourceLocation(explicitInterfaceSpecifierSyntax.Name);
			diagnostics.Add(ErrorCode.ERR_ClassDoesntImplementInterface, location3, implementingMember, namedTypeSymbol);
		}
		bool flag = implementingMember.HasParamsParameter();
		bool flag2 = false;
		Symbol symbol = null;
		foreach (Symbol member in namedTypeSymbol.GetMembers(interfaceMemberName))
		{
			if (member.Kind != implementingMember.Kind || member.IsStatic || !MemberSignatureComparer.ExplicitImplementationComparer.Equals(implementingMember, member))
			{
				continue;
			}
			flag2 = true;
			if (member.IsAccessor() && !((MethodSymbol)member).IsIndexedPropertyAccessor())
			{
				diagnostics.Add(ErrorCode.ERR_ExplicitMethodImplAccessor, location, implementingMember, member);
				continue;
			}
			if (member.MustCallMethodsDirectly())
			{
				diagnostics.Add(ErrorCode.ERR_BogusExplicitImpl, location, implementingMember, member);
			}
			else if (flag && !member.HasParamsParameter())
			{
				diagnostics.Add(ErrorCode.ERR_ExplicitImplParams, location, implementingMember, member);
			}
			symbol = member;
			break;
		}
		if (!flag2)
		{
			diagnostics.Add(ErrorCode.ERR_InterfaceMemberNotFound, location, implementingMember);
		}
		if (implementingMember.ContainsTupleNames() && MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(implementingMember, symbol))
		{
			diagnostics.Add(ErrorCode.ERR_ImplBadTupleNames, location, implementingMember, symbol);
		}
		FindExplicitImplementationCollisions(implementingMember, symbol, diagnostics);
		return symbol;
	}

	private static void FindExplicitImplementationCollisions(Symbol implementingMember, Symbol implementedMember, DiagnosticBag diagnostics)
	{
		if ((object)implementedMember == null)
		{
			return;
		}
		NamedTypeSymbol containingType = implementedMember.ContainingType;
		bool isDefinition = containingType.IsDefinition;
		foreach (Symbol member in containingType.GetMembers(implementedMember.Name))
		{
			if (member.Kind != implementingMember.Kind || !(implementedMember != member))
			{
				continue;
			}
			if (!isDefinition && MemberSignatureComparer.RuntimeSignatureComparer.Equals(implementedMember, member))
			{
				bool flag = false;
				ImmutableArray<ParameterSymbol> parameters = implementedMember.GetParameters();
				ImmutableArray<ParameterSymbol> parameters2 = member.GetParameters();
				int length = parameters.Length;
				for (int i = 0; i < length; i++)
				{
					if (parameters[i].RefKind != parameters2[i].RefKind)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					diagnostics.Add(ErrorCode.ERR_ExplicitImplCollisionOnRefOut, containingType.Locations[0], containingType, implementedMember);
				}
				else
				{
					diagnostics.Add(ErrorCode.WRN_ExplicitImplCollision, implementingMember.Locations[0], implementingMember);
				}
				break;
			}
			if (MemberSignatureComparer.ExplicitImplementationComparer.Equals(implementedMember, member))
			{
				diagnostics.Add(ErrorCode.WRN_ExplicitImplCollision, implementingMember.Locations[0], implementingMember);
			}
		}
	}
}
