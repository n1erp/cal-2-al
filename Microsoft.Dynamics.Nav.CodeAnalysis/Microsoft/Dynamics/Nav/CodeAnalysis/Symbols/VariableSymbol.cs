using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class VariableSymbol : Symbol, IVariableSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithAttributes, ISymbolWithType
{
	public abstract ImmutableArray<AttributeSymbol> Attributes { get; }

	public override SymbolKind Kind => SymbolKind.GlobalVariable;

	public abstract TypeSymbol Type { get; }

	public virtual VariableKind VariableKind => VariableKind.Normal;

	public virtual bool IsArray => Type.IsArray();

	public bool IsWritable => Type.IsVariableTypeWritable();

	public bool IsInDataSet
	{
		get
		{
			if (Kind != SymbolKind.GlobalVariable)
			{
				return false;
			}
			return Attributes.Any((AttributeSymbol a) =>
			{
				AttributeTypeInfo attributeInfo = a.AttributeInfo;
				return attributeInfo != null && attributeInfo.Kind == AttributeKind.InDataSet;
			});
		}
	}

	public bool IsNonDebuggable
	{
		get
		{
			if (Kind != SymbolKind.GlobalVariable && Kind != SymbolKind.LocalVariable)
			{
				return false;
			}
			return Attributes.Any((AttributeSymbol a) =>
			{
				AttributeTypeInfo attributeInfo = a.AttributeInfo;
				return attributeInfo != null && attributeInfo.Kind == AttributeKind.NonDebuggable;
			});
		}
	}

	ITypeSymbol IVariableSymbol.Type => Type;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitVariable(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitVariable(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitVariable(this, a);
	}
}
