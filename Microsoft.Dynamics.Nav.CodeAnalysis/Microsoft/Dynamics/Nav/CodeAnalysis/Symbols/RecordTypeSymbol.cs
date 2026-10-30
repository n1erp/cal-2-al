using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class RecordTypeSymbol : NavTypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol
{
	private readonly TableTypeSymbol tableTypeSymbol;

	public override SymbolKind Kind => SymbolKind.Record;

	public override string Name => OriginalSymbolDefinition.Name;

	public override TypeSymbol OriginalDefinition => tableTypeSymbol;

	public int Id => tableTypeSymbol.Id;

	public bool Temporary { get; }

	internal override TypeSymbol BaseType => tableTypeSymbol;

	public override TypeCategoryKind TypeCategory => tableTypeSymbol.TypeCategory;

	protected override Symbol OriginalSymbolDefinition => tableTypeSymbol;

	public override bool IsDeprecated => tableTypeSymbol.IsDeprecated;

	public override bool IsObsolete => tableTypeSymbol.IsObsolete;

	public override string ObsoleteReason => tableTypeSymbol.ObsoleteReason;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitRecordType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitRecordType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitRecordType(this, a);
	}

	internal RecordTypeSymbol(TableTypeSymbol tableTypeSymbol, bool temporary)
		: base(NavTypeKind.Record)
	{
		this.tableTypeSymbol = tableTypeSymbol;
		Temporary = temporary;
	}

	internal override bool Equals(TypeSymbol t2)
	{
		return Equals(t2, ignoreTemporary: false);
	}

	internal bool Equals(TypeSymbol t2, bool ignoreTemporary)
	{
		if (t2?.NavTypeKind != NavTypeKind)
		{
			return false;
		}
		RecordTypeSymbol recordTypeSymbol = (RecordTypeSymbol)t2;
		if (recordTypeSymbol.tableTypeSymbol.Equals(tableTypeSymbol))
		{
			if (!ignoreTemporary)
			{
				return recordTypeSymbol.Temporary == Temporary;
			}
			return true;
		}
		return false;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}
