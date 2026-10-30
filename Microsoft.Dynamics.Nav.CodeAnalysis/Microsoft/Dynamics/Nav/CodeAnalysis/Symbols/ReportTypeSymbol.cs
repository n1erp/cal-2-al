using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class ReportTypeSymbol : ApplicationObjectTypeSymbol, IReportTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	public override SymbolKind Kind => SymbolKind.Report;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ApplicationObject;

	internal override TypeSymbol BaseType => ReportInstanceClassTypeSymbol.Instance;

	public override NavTypeKind NavTypeKind => NavTypeKind.Report;

	public abstract ImmutableArray<ReportDataItemSymbol> FlattenedDataItems { get; }

	public abstract RequestPageTypeSymbol RequestPage { get; }

	public override string MetadataName => "Report" + Id.ToString(CultureInfo.InvariantCulture);

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.ReportProperties;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitReportType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitReportType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitReportType(this, a);
	}

	protected ReportTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetReportTriggerInfo(name);
	}
}
