using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class KeySymbol : FieldNameListSymbol, IKeySymbol, IFieldNameListSymbol, ISymbol, IEquatable<ISymbol>
{
	private ObsoleteStateKind? obsoleteState;

	public override SymbolKind Kind => SymbolKind.Key;

	public override bool IsObsolete => GetIsObsoleteState() == ObsoleteStateKind.Removed;

	public override bool IsDeprecated => GetIsObsoleteState() == ObsoleteStateKind.Pending;

	public override string ObsoleteReason => GetProperty(PropertyKind.ObsoleteReason)?.Value as string;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.KeyProperties;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitKey(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitKey(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitKey(this, a);
	}

	private ObsoleteStateKind GetIsObsoleteState()
	{
		if (!obsoleteState.HasValue)
		{
			obsoleteState = this.GetEnumPropertyValueOrDefault<ObsoleteStateKind>(PropertyKind.ObsoleteState);
		}
		return obsoleteState.Value;
	}
}
