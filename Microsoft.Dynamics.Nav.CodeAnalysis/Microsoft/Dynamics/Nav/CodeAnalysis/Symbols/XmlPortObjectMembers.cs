using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class XmlPortObjectMembers : ApplicationObjectMembers<XmlPortTypeSymbol>
{
	private readonly Lazy<ImmutableArray<VariableSymbol>> globalVariableList;

	private readonly Lazy<RequestPageTypeSymbol> requestPage;

	private ImmutableArray<Symbol> lazyCodeMembers;

	internal RequestPageTypeSymbol RequestPage => requestPage.Value;

	internal ImmutableArray<VariableSymbol> GlobalVariables => globalVariableList.Value;

	internal new XmlPortSyntax Declaration => (XmlPortSyntax)base.Declaration;

	private Lazy<ImmutableArray<XmlPortNodeSymbol>> FlattenedNodesList { get; }

	internal ImmutableArray<XmlPortNodeSymbol> FlattenedNodes => FlattenedNodesList.Value;

	public XmlPortObjectMembers(XmlPortTypeSymbol containingSymbol, ApplicationObjectSyntax declaration)
		: base(containingSymbol, declaration)
	{
		XmlPortObjectMembers xmlPortObjectMembers = this;
		requestPage = new Lazy<RequestPageTypeSymbol>(() => (xmlPortObjectMembers.GetMembers().FirstOrDefault((Symbol member) => member.Kind == SymbolKind.RequestPage) as RequestPageTypeSymbol) ?? new SynthesizedRequestPageTypeSymbol(containingSymbol));
		globalVariableList = new Lazy<ImmutableArray<VariableSymbol>>(() =>
		{
			ArrayBuilder<VariableSymbol> instance = ArrayBuilder<VariableSymbol>.GetInstance();
			try
			{
				instance.AddRange(xmlPortObjectMembers.GetGlobalVariables());
				SourceRequestPageTypeSymbol sourceRequestPageTypeSymbol = xmlPortObjectMembers.GetMembers().FirstOrDefault((Symbol member) => member.Kind == SymbolKind.RequestPage) as SourceRequestPageTypeSymbol;
				if (sourceRequestPageTypeSymbol != null && sourceRequestPageTypeSymbol.RelatedTable != null)
				{
					RecordTypeSymbol record = xmlPortObjectMembers.ContainingSymbol.ContainingModule.GetRecord(sourceRequestPageTypeSymbol.RelatedTable, isTemporary: false);
					instance.Add(SynthesizedGlobalVariableSymbol.Create(containingSymbol, record, "Rec", isThis: false, hasImplicitWith: true));
					instance.Add(SynthesizedGlobalVariableSymbol.Create(containingSymbol, record, "xRec"));
				}
				return instance.ToImmutable();
			}
			finally
			{
				instance.Free();
			}
		});
		FlattenedNodesList = new Lazy<ImmutableArray<XmlPortNodeSymbol>>(() => new FlattenedMemberList<XmlPortNodeSymbol>(xmlPortObjectMembers.GetMembers().WhereAsCastArray<Symbol, XmlPortNodeSymbol>((Symbol m) => m.Kind == SymbolKind.XmlPortNode), GetNodesOfNode).FlattenedList.ToImmutableArray());
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetXmlPortTriggerInfo(name);
	}

	protected override void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
		members.Add(SynthesizedGlobalVariableSymbol.Create(ContainingSymbol, XmlPortReferenceTypeSymbol.Create(ContainingSymbol, isCurrXmlPort: true), "currXMLport"));
		if (Declaration.XmlPortSchema != null)
		{
			foreach (XmlPortNodeSyntax item in Declaration.XmlPortSchema.XmlPortSchema)
			{
				members.Add(SourceXmlPortNodeSymbol.Create(ContainingSymbol, item));
			}
		}
		if (Declaration.XmlPortRequestPage != null)
		{
			members.Add(new SourceRequestPageTypeSymbol(ContainingSymbol, Declaration.XmlPortRequestPage));
		}
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		ObjectMembers<XmlPortTypeSymbol>.CheckMembersForDuplicateNames(GetCodeMembers(), diagnostics, (Symbol m) => true, ErrorCode.ERR_DuplicateMemberName);
		CheckRootNodeRules(GetMembers(), diagnostics);
	}

	protected override bool IsNameRestrictedForTriggers(string name)
	{
		if (!TriggerDefinitions.IsValidXmlPortTrigger(name) && !TriggerDefinitions.IsValidXmlPortTableElementTrigger(name) && !TriggerDefinitions.IsValidXmlPortFieldElementTrigger(name))
		{
			return TriggerDefinitions.IsValidXmlPortTextElementTrigger(name);
		}
		return true;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (lazyCodeMembers != null)
		{
			return lazyCodeMembers;
		}
		ImmutableArray<Symbol> value = GetMembers().Union(FlattenedNodes.Where((XmlPortNodeSymbol n) => n.SourceTypeKind != XmlPortSourceTypeKind.Field)).ToImmutableArray();
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyCodeMembers, value, default);
		return lazyCodeMembers;
	}

	private static void CheckRootNodeRules(ImmutableArray<Symbol> members, DiagnosticBag diagnostics)
	{
		IEnumerable<Symbol> enumerable = members.Where((Symbol m) => m.Kind == SymbolKind.XmlPortNode);
		if (enumerable.HasMoreThan(1))
		{
			foreach (Symbol item in enumerable)
			{
				diagnostics.Add(ErrorCode.ERR_ThereMustBeOneRootNode, item.GetLocation());
			}
			return;
		}
		Symbol symbol = enumerable.FirstOrDefault();
		if (!(symbol == null) && ((XmlPortNodeSymbol)symbol).NodeXmlKind != XmlPortNodeXmlKind.Element)
		{
			diagnostics.Add(ErrorCode.ERR_ThereMustBeOneRootNode, ((XmlPortNodeSymbol)symbol).GetLocation());
		}
	}

	private static ImmutableArray<XmlPortNodeSymbol> GetNodesOfNode(Symbol possibleNode)
	{
		if (!(possibleNode is SourceXmlPortNodeSymbol { NodeXmlKind: <=XmlPortNodeXmlKind.Element } sourceXmlPortNodeSymbol))
		{
			return ImmutableArray<XmlPortNodeSymbol>.Empty;
		}
		return sourceXmlPortNodeSymbol.Nodes.ToImmutableArray();
	}
}
