using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReportReferenceTypeSymbol : NavTypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol
{
	private readonly ReportTypeSymbol reportTypeSymbol;

	internal bool IsCurrReport { get; }

	public override TypeSymbol OriginalDefinition => reportTypeSymbol;

	internal override TypeSymbol BaseType => ReportInstanceClassTypeSymbol.Instance;

	protected override Symbol OriginalSymbolDefinition => reportTypeSymbol;

	public override TypeCategoryKind TypeCategory => reportTypeSymbol.TypeCategory;

	public override SymbolKind Kind => SymbolKind.ReportReference;

	public override string Name => OriginalSymbolDefinition.Name;

	public int Id => reportTypeSymbol.Id;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitReportReferenceType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitReportReferenceType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitReportReferenceType(this, a);
	}

	private ReportReferenceTypeSymbol(ReportTypeSymbol reportTypeSymbol, bool isCurrReport)
		: base(isCurrReport ? NavTypeKind.CurrReport : NavTypeKind.Report)
	{
		this.reportTypeSymbol = reportTypeSymbol;
		IsCurrReport = isCurrReport;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return reportTypeSymbol.GetMembers();
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return reportTypeSymbol.GetMembers(name);
	}

	internal static ReportReferenceTypeSymbol Create(ReportTypeSymbol reportTypeSymbol, bool isCurrReport = false)
	{
		return new ReportReferenceTypeSymbol(reportTypeSymbol, isCurrReport);
	}

	internal override bool Equals(TypeSymbol t2)
	{
		if (t2?.NavTypeKind != NavTypeKind)
		{
			return false;
		}
		return ((ReportReferenceTypeSymbol)t2).reportTypeSymbol.Equals(reportTypeSymbol);
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return reportTypeSymbol.GetCodeMembers();
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return reportTypeSymbol.GetCodeMembers(name);
	}
}
