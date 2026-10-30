using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ReportColumnSymbol : TypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, ISymbolWithType, IReportColumnSymbol
{
	public abstract int Id { get; }

	public abstract TypeSymbol Type { get; }

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ReportDataElement;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override SymbolKind Kind => SymbolKind.ReportColumn;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.ReportColumnProperties;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitReportColumn(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitReportColumn(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitReportColumn(this, a);
	}
}
