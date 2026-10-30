using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceReportTypeSymbol : ReportTypeSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<ReportDataItemSymbol>> lazyFlattenedDataItems;

	private readonly Lazy<ImmutableArray<VariableSymbol>> lazyGlobalVariables;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<RequestPageTypeSymbol> lazyRequestPage;

	public override int Id { get; }

	public override string Name { get; }

	public override RequestPageTypeSymbol RequestPage => lazyRequestPage.Value;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => lazyGlobalVariables.Value;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<ReportDataItemSymbol> FlattenedDataItems => lazyFlattenedDataItems.Value;

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public ReferenceReportTypeSymbol(ModuleSymbol module, ReportDefinition report)
		: base(module)
	{
		ReferenceReportTypeSymbol referenceReportTypeSymbol = this;
		DebugAssertHelper.Assert(report != null);
		Id = report.Id ?? (-1);
		Name = report.Name().UnquoteIdentifier();
		lazyRequestPage = ReferenceSymbolHelper.CreateLazy(() => referenceReportTypeSymbol.GetRequestPageLazy(report));
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, report);
		lazyGlobalVariables = ReferenceSymbolHelper.BuildGlobalVariablesLazy(this, report);
		lazyFlattenedDataItems = ReferenceSymbolHelper.CreateLazy(() => report.DataItems.AsImmutableOrEmpty((Func<ReportDataItemDefinition, ReportDataItemSymbol>)((ReportDataItemDefinition d) => new ReferenceReportDataItemSymbol(referenceReportTypeSymbol, d))));
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceReportTypeSymbol.GetMembersLazy(report));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	private ImmutableArray<Symbol> GetMembersLazy(ReportDefinition report)
	{
		int lengthSafe = report.Methods.GetLengthSafe();
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(GlobalVariables.Length + Properties.Length + lengthSafe + FlattenedDataItems.Length);
		if (lengthSafe > 0)
		{
			MethodDefinition[] methods = report.Methods;
			foreach (MethodDefinition method in methods)
			{
				instance.Add(new ReferenceMethodSymbol(this, method));
			}
		}
		instance.AddRange(GlobalVariables);
		instance.AddRange(Properties);
		instance.AddRange(FlattenedDataItems);
		return instance.ToImmutableAndFree();
	}

	private RequestPageTypeSymbol GetRequestPageLazy(ReportDefinition report)
	{
		if (report.RequestPage == null)
		{
			return new SynthesizedRequestPageTypeSymbol(this);
		}
		return new ReferenceRequestPageTypeSymbol(this, report.RequestPage);
	}
}
