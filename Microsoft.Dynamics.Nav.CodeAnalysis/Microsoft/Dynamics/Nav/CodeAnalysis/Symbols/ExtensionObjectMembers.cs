using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal abstract class ExtensionObjectMembers<T, ExtensionTarget> : ApplicationObjectMembers<T> where T : ObjectTypeSymbol where ExtensionTarget : ApplicationObjectTypeSymbol
{
	internal abstract ExtensionTarget Target { get; }

	protected abstract SymbolKind TargetKind { get; }

	protected new ApplicationObjectExtensionSyntax Declaration => (ApplicationObjectExtensionSyntax)base.Declaration;

	internal ExtensionObjectMembers(T containingSymbol, ApplicationObjectSyntax declaration)
		: base(containingSymbol, declaration)
	{
	}

	protected virtual ImmutableArray<Symbol> GetTargetMembers()
	{
		ImmutableArray<Symbol> result = Target.GetMembers();
		foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in ContainingSymbol.ContainingModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(Target))
		{
			if (applicationObjectExtensionTypeSymbolsAcrossModule.Id != Id)
			{
				result = result.AddRange(applicationObjectExtensionTypeSymbolsAcrossModule.GetMembers().Cast<Symbol>());
			}
		}
		return result;
	}

	protected bool CheckTargetRelation(DiagnosticBag diagnostics)
	{
		if (Target == null)
		{
			diagnostics.Add(ErrorCode.ERR_ExtensionTargetNotFound, Declaration.BaseObject.Location, TargetKind, Declaration.BaseObject.Identifier);
			return false;
		}
		if (!Target.CanCompileWithTarget(ContainingSymbol.DeclaringCompilation.Options.Target))
		{
			diagnostics.Add(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, Declaration.BaseObject.Location, Target.Name, ContainingSymbol.DeclaringCompilation.Options.Target.ToString());
			return false;
		}
		ImmutableArray<Symbol> members = GetMembers();
		ImmutableArray<Symbol> targetMembers = GetTargetMembers();
		PooledList<MethodSymbol> instance = PooledList<MethodSymbol>.GetInstance();
		PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
		PooledList<MethodSymbol> instance3 = PooledList<MethodSymbol>.GetInstance();
		try
		{
			ObjectMembers<T>.PopulatePublicMethods(members, instance);
			ObjectMembers<T>.PopulatePublicMethodNames(instance, instance2);
			ObjectMembers<T>.PopulatePublicMethods(targetMembers, instance3, instance2);
			foreach (MethodSymbol item in instance)
			{
				foreach (MethodSymbol item2 in instance3)
				{
					if (item.HasMatchingSignature(item2))
					{
						ObjectTypeSymbol containingSymbolOfType = item2.GetContainingSymbolOfType<ObjectTypeSymbol>();
						if (containingSymbolOfType == null)
						{
							diagnostics.Add(ErrorCode.WRN_MemberAlreadyDeclaredInBase, item.GetLocation(), item.Name);
						}
						else
						{
							diagnostics.Add(ErrorCode.WRN_MemberAlreadyExists, item.GetLocation(), containingSymbolOfType, item.Name);
						}
						break;
					}
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
}
