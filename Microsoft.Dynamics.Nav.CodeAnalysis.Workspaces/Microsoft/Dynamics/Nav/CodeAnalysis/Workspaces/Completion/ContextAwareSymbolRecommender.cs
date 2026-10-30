using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class ContextAwareSymbolRecommender
{
	private ObjectTypeSymbol declaringObject;

	protected internal MemberSyntaxContext Context { get; }

	protected ObjectTypeSymbol DeclaringObject => declaringObject ?? (declaringObject = Context.SemanticModel.GetDeclaredSymbol(Context.DeclaringObject) as ObjectTypeSymbol);

	protected internal virtual bool IsExclusive => false;

	protected ContextAwareSymbolRecommender(MemberSyntaxContext context)
	{
		Context = context;
	}

	protected internal virtual Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(SpecializedCollections.EmptyEnumerable<ISymbol>());
	}

	protected void GetMembersAndGroupsForPageLayoutChange(SyntaxToken token, CancellationToken cancellationToken, out IEnumerable<Symbol> members, out IEnumerable<Symbol> groupMembers, out IEnumerable<Symbol> areaMembers)
	{
		PageTypeSymbol targetPage = GetTargetPage();
		GetMembersAndGroupsForPageTarget(token, targetPage, cancellationToken, out members, out groupMembers, out areaMembers);
	}

	private PageTypeSymbol GetTargetPage()
	{
		if (Context.Page.HasFlag(PageContexts.View) && DeclaringObject.Kind == SymbolKind.Page)
		{
			return (PageTypeSymbol)DeclaringObject;
		}
		return (PageTypeSymbol)((DeclaringObject as PageExtensionBaseTypeSymbol)?.Target);
	}

	private void GetMembersAndGroupsForPageTarget(SyntaxToken token, PageTypeSymbol targetPage, CancellationToken cancellationToken, out IEnumerable<Symbol> members, out IEnumerable<Symbol> groupMembers, out IEnumerable<Symbol> areaMembers)
	{
		if (targetPage == null)
		{
			members = SpecializedCollections.EmptyEnumerable<Symbol>();
			groupMembers = SpecializedCollections.EmptyEnumerable<Symbol>();
			areaMembers = SpecializedCollections.EmptyEnumerable<Symbol>();
		}
		else if (token.Parent.GetFirstParent(SyntaxKind.PageExtensionLayout) != null)
		{
			BuildMembersGroupsAndAreas(token, targetPage, cancellationToken, SymbolKind.Control, (ControlSymbol x) => x.ControlKind.IsGroup(), (ControlSymbol x) => x.ControlKind == ControlKind.Area, out members, out groupMembers, out areaMembers);
		}
		else if (token.Parent.GetFirstParent(SyntaxKind.PageExtensionActionList) != null)
		{
			BuildMembersGroupsAndAreas(token, targetPage, cancellationToken, SymbolKind.Action, (ActionSymbol x) => x.ActionKind.IsGroup(), (ActionSymbol x) => x.ActionKind == ActionKind.Area, out members, out groupMembers, out areaMembers);
		}
		else if (token.Parent.GetFirstParent(SyntaxKind.PageExtensionViewList) != null)
		{
			BuildMembersGroupsAndAreas(token, targetPage, cancellationToken, SymbolKind.View, (ViewSymbol _) => false, (ViewSymbol _) => false, out members, out groupMembers, out areaMembers);
		}
		else
		{
			DebugAssertHelper.Fail("Couldn't resolve the member and groups for page extensions / customizations.");
			members = SpecializedCollections.EmptyEnumerable<Symbol>();
			groupMembers = SpecializedCollections.EmptyEnumerable<Symbol>();
			areaMembers = SpecializedCollections.EmptyEnumerable<Symbol>();
		}
	}

	private void BuildMembersGroupsAndAreas<T>(SyntaxToken token, PageTypeSymbol target, CancellationToken cancellationToken, SymbolKind kind, Func<T, bool> isGroup, Func<T, bool> isArea, out IEnumerable<Symbol> members, out IEnumerable<Symbol> groupMembers, out IEnumerable<Symbol> areaMembers) where T : Symbol
	{
		ArrayBuilder<Symbol> arrayBuilder = new ArrayBuilder<Symbol>();
		ArrayBuilder<Symbol> arrayBuilder2 = new ArrayBuilder<Symbol>();
		ArrayBuilder<Symbol> arrayBuilder3 = new ArrayBuilder<Symbol>();
		try
		{
			foreach (ISymbol item in Context.SemanticModel.LookupSymbols(token.SpanStart, LookupOptions.MustBePageElement, target, null, cancellationToken))
			{
				if (!item.IsKind(kind))
				{
					continue;
				}
				T val = (T)item;
				arrayBuilder.Add(val);
				if (isGroup(val))
				{
					if (isArea(val))
					{
						arrayBuilder3.Add(val);
					}
					else
					{
						arrayBuilder2.Add(val);
					}
				}
			}
			members = arrayBuilder.ToImmutable();
			groupMembers = arrayBuilder2.ToImmutable();
			areaMembers = arrayBuilder3.ToImmutable();
		}
		finally
		{
			arrayBuilder.Free();
			arrayBuilder2.Free();
			arrayBuilder3.Free();
		}
	}

	protected TableTypeSymbol GetRelatedTableSymbol(PropertyValueSyntax propertyValue)
	{
		PropertyTypeInfo propertyTypeInfo = propertyValue.GetPropertyTypeInfo();
		if (propertyTypeInfo == null)
		{
			return null;
		}
		switch (propertyTypeInfo.Kind)
		{
		case PropertyKind.SubPageView:
		case PropertyKind.SubPageLink:
			return GetSubPageViewAndLinkRelatedTable(propertyValue);
		case PropertyKind.RunPageView:
		case PropertyKind.RunPageLink:
			return GetRunPageViewAndLinkRelatedTable(propertyValue);
		case PropertyKind.LinkFields:
		case PropertyKind.DataItemTableFilter:
			return GetPropertyValueRelatedTable(propertyValue);
		default:
			return null;
		}
	}

	private TableTypeSymbol GetSubPageViewAndLinkRelatedTable(PropertyValueSyntax propertyValue)
	{
		PagePartSyntax pagePartSyntax = propertyValue.Parent.Parent.Parent as PagePartSyntax;
		if (pagePartSyntax?.PartName == null)
		{
			return null;
		}
		string identifierOrLiteralValue = pagePartSyntax.PartName.Identifier.GetIdentifierOrLiteralValue();
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = Context.SemanticModel.Compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Page, identifierOrLiteralValue);
		if (objectSymbolsByNameAcrossModules.Length != 1)
		{
			return null;
		}
		return (objectSymbolsByNameAcrossModules[0] as PageTypeSymbol)?.RelatedTable;
	}

	private TableTypeSymbol GetRunPageViewAndLinkRelatedTable(PropertyValueSyntax propertyValue)
	{
		if (!(propertyValue.Parent.Parent.Parent is PageActionSyntax pageActionSyntax))
		{
			return null;
		}
		string identifierOrLiteralValue = pageActionSyntax.Name.GetIdentifierOrLiteralValue();
		ActionSymbol actionSymbol = null;
		foreach (Symbol codeMember in DeclaringObject.GetCodeMembers())
		{
			if (codeMember.Kind == SymbolKind.Action && SemanticFacts.IsSameName(identifierOrLiteralValue, codeMember.Name))
			{
				actionSymbol = (ActionSymbol)codeMember;
				break;
			}
		}
		if (actionSymbol == null)
		{
			return null;
		}
		PropertySymbol property = actionSymbol.GetProperty(PropertyKind.RunObject);
		if (property == null || property.Property?.PropertyValue == null || property.Property.PropertyValue.Kind != BoundKind.ApplicationObjectReferencePropertyValue)
		{
			return null;
		}
		return ((BoundApplicationObjectReferencePropertyValue)property.Property.PropertyValue).Value.GetRelatedTableSymbol();
	}

	private TableTypeSymbol GetPropertyValueRelatedTable(PropertyValueSyntax propertyValue)
	{
		SyntaxNode parent = propertyValue.Parent.Parent.Parent;
		if (parent == null)
		{
			return null;
		}
		return parent.Kind switch
		{
			SyntaxKind.QueryDataItem => GetTableTypeSymbol(((QueryDataItemSyntax)parent).DataItemTable), 
			SyntaxKind.XmlPortTableElement => GetTableTypeSymbol(((XmlPortTableElementSyntax)parent).SourceTable), 
			_ => null, 
		};
	}

	private TableTypeSymbol GetTableTypeSymbol(ObjectNameOrIdSyntax tableIdentifierSyntax)
	{
		if (tableIdentifierSyntax == null)
		{
			return null;
		}
		string identifierOrLiteralValue = tableIdentifierSyntax.Identifier.GetIdentifierOrLiteralValue();
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = Context.SemanticModel.Compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Table, identifierOrLiteralValue);
		if (objectSymbolsByNameAcrossModules.Length != 1)
		{
			return null;
		}
		return objectSymbolsByNameAcrossModules[0] as TableTypeSymbol;
	}
}
