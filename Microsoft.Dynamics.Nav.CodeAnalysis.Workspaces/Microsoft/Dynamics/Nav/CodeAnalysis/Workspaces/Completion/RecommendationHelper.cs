using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class RecommendationHelper
{
	internal static IEnumerable<ISymbol> RecommendFieldSymbolsForFieldGroup(MemberSyntaxContext context, IContainerSymbol tableTypeSymbol, CancellationToken cancellationToken)
	{
		if (tableTypeSymbol.Kind == SymbolKind.Table || tableTypeSymbol.Kind == SymbolKind.TableExtension)
		{
			return GetFields(context, tableTypeSymbol, cancellationToken);
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	internal static IEnumerable<ISymbol> RecommendFieldSymbolsForKey(MemberSyntaxContext context, IContainerSymbol declaringTableSymbol, CancellationToken cancellationToken)
	{
		KeySyntax keySyntax = (KeySyntax)context.TargetToken.Parent;
		ISet<string> fieldsAlreadyInKey = keySyntax.Fields.Select((IdentifierNameSyntax x) => x.GetIdentifierOrLiteralValue()).ToSet(StringComparer.OrdinalIgnoreCase);
		if (declaringTableSymbol.Kind == SymbolKind.Table)
		{
			return from field in GetFields(context, declaringTableSymbol, cancellationToken)
				where !fieldsAlreadyInKey.Contains(field.Name)
				select field;
		}
		if (declaringTableSymbol.Kind == SymbolKind.TableExtension)
		{
			return from field in GetFields(context, declaringTableSymbol, cancellationToken)
				where field.ContainingSymbol == declaringTableSymbol && !fieldsAlreadyInKey.Contains(field.Name)
				select field;
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	internal static IEnumerable<ISymbol> GetFields(MemberSyntaxContext context, IContainerSymbol fieldOwner, CancellationToken cancellationToken)
	{
		if (fieldOwner == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (fieldOwner.Kind == SymbolKind.Table || fieldOwner.Kind == SymbolKind.Record)
		{
			LookupOptions lookupOptions = fieldOwner.GetLookupOptions();
			return from s in context.SemanticModel.LookupSymbols(context.Position, lookupOptions, fieldOwner, null, cancellationToken)
				where s.Kind == SymbolKind.Field
				select s;
		}
		if (fieldOwner.Kind == SymbolKind.TableExtension)
		{
			TableTypeSymbol fieldOwner2 = (TableTypeSymbol)((SourceTableExtensionTypeSymbol)fieldOwner).Target;
			return GetFields(context, fieldOwner2, cancellationToken);
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	internal static IEnumerable<ISymbol> GetMembers(MemberSyntaxContext context, IContainerSymbol owner, bool isImplicitInvocation, CancellationToken cancellationToken)
	{
		SymbolKind? symbolKind = owner?.Kind;
		if (symbolKind.HasValue)
		{
			switch ((int)symbolKind.GetValueOrDefault())
			{
			case 10:
			case 17:
			case 90:
				return GetFields(context, owner, cancellationToken);
			case 14:
				if (isImplicitInvocation)
				{
					return ((QueryTypeSymbol)owner).FlattenedColumns;
				}
				break;
			case 70:
				if (isImplicitInvocation)
				{
					return ((SourcePageTypeSymbol)((TestPageTypeSymbol)owner).Target).FlattenedControls.Where((ControlSymbol c) => c.ControlKind == ControlKind.Field);
				}
				break;
			case 76:
				if (!isImplicitInvocation)
				{
					break;
				}
				goto IL_00da;
			case 75:
				if (!isImplicitInvocation)
				{
					break;
				}
				goto IL_00da;
			case 73:
				{
					if (!isImplicitInvocation)
					{
						break;
					}
					goto IL_00da;
				}
				IL_00da:
				return owner.GetMembers();
			}
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}
}
