using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class ReportDataItemSymbol : TypeSymbol, IReportDataItemSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithType, ISymbolWithId, ISymbolWithTriggers
{
	private TypeSymbol lazyType;

	public abstract int Id { get; }

	public abstract TableTypeSymbol RelatedTable { get; }

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ReportDataElement;

	public TypeSymbol Type
	{
		get
		{
			if (lazyType != null)
			{
				return lazyType;
			}
			Interlocked.CompareExchange(ref lazyType, LazyGetType(), null);
			return lazyType;
		}
	}

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override SymbolKind Kind => SymbolKind.ReportDataItem;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.ReportDataItemProperties;

	public abstract ImmutableArray<MethodSymbol> Triggers { get; }

	internal override TypeSymbol BaseType => RelatedTable;

	ImmutableArray<IMethodSymbol> ISymbolWithTriggers.Triggers => ImmutableArray<IMethodSymbol>.CastUp(Triggers);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitReportDataItem(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitReportDataItem(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitReportDataItem(this, a);
	}

	public TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetReportDataItemTriggerInfo(name);
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}

	private TypeSymbol LazyGetType()
	{
		if (RelatedTable == null)
		{
			return NavCorLib.None;
		}
		bool valueOrDefault = this.GetBooleanPropertyValue(PropertyKind.UseTemporary) == true;
		return ContainingModule.GetRecord(RelatedTable, valueOrDefault);
	}
}
