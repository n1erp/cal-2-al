using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class FieldGroupSymbol : FieldNameListSymbol, IFieldGroupSymbol, IFieldNameListSymbol, ISymbol, IEquatable<ISymbol>
{
	public override SymbolKind Kind => SymbolKind.FieldGroup;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.FieldGroupProperties;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitFieldGroup(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitFieldGroup(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitFieldGroup(this, a);
	}
}
