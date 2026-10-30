using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ReportLabelSymbol : TypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, IReportLabelSymbol
{
	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Unknown;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public abstract int Id { get; }

	public override SymbolKind Kind => SymbolKind.ReportLabel;

	public abstract bool IsMultilanguageReportLabel { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitReportLabel(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitReportLabel(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitReportLabel(this, a);
	}
}
