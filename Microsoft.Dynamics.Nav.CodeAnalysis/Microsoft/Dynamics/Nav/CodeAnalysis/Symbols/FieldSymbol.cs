using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class FieldSymbol : TypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, IFieldSymbol, ITypeSymbol, IContainerSymbol, ISymbolWithTriggers, ISymbolWithType
{
	private readonly Lazy<ObsoleteStateKind> lazyObsoleteState;

	private readonly Lazy<string> lazyObsoleteReason;

	private readonly Lazy<FieldClassKind> lazyFieldClass;

	public abstract TypeSymbol Type { get; }

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Option;

	public abstract int Id { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.FieldProperties;

	public abstract ImmutableArray<MethodSymbol> Triggers { get; }

	public override SymbolKind Kind => SymbolKind.Field;

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override bool IsObsolete => lazyObsoleteState.Value == ObsoleteStateKind.Removed;

	public override bool IsDeprecated => lazyObsoleteState.Value == ObsoleteStateKind.Pending;

	public override string ObsoleteReason => lazyObsoleteReason.Value;

	public bool IsFlowFilter => lazyFieldClass.Value == FieldClassKind.FlowFilter;

	public override bool HasLength => Type.HasLength;

	public override int Length => Type.Length;

	FieldClassKind IFieldSymbol.FieldClass => lazyFieldClass.Value;

	ImmutableArray<IMethodSymbol> ISymbolWithTriggers.Triggers => ImmutableArray<IMethodSymbol>.CastUp(Triggers);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitField(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitField(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitField(this, a);
	}

	protected FieldSymbol()
	{
		lazyObsoleteState = new Lazy<ObsoleteStateKind>(LazyGetIsObsoleteState);
		lazyObsoleteReason = new Lazy<string>(LazyGetObsoleteReason);
		lazyFieldClass = new Lazy<FieldClassKind>(LazyGetFieldClass);
	}

	public TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetFieldTriggerInfo(name);
	}

	private ObsoleteStateKind LazyGetIsObsoleteState()
	{
		return LazyGetObsoleteStateAndReason().State;
	}

	private string LazyGetObsoleteReason()
	{
		return LazyGetObsoleteStateAndReason().Reason;
	}

	private (ObsoleteStateKind State, string Reason) LazyGetObsoleteStateAndReason()
	{
		ObsoleteStateKind enumPropertyValueOrDefault = this.GetEnumPropertyValueOrDefault<ObsoleteStateKind>(PropertyKind.ObsoleteState);
		string item = GetProperty(PropertyKind.ObsoleteReason)?.Value as string;
		if (ContainingSymbol.Kind == SymbolKind.Table)
		{
			return (State: enumPropertyValueOrDefault, Reason: item);
		}
		if (ContainingSymbol.IsObsolete && enumPropertyValueOrDefault < ObsoleteStateKind.Removed)
		{
			return (State: ObsoleteStateKind.Removed, Reason: ContainingSymbol.ObsoleteReason);
		}
		if (ContainingSymbol.IsDeprecated && enumPropertyValueOrDefault < ObsoleteStateKind.Pending)
		{
			return (State: ObsoleteStateKind.Pending, Reason: ContainingSymbol.ObsoleteReason);
		}
		return (State: enumPropertyValueOrDefault, Reason: item);
	}

	private FieldClassKind LazyGetFieldClass()
	{
		return this.GetEnumPropertyValueOrDefault<FieldClassKind>(PropertyKind.FieldClass);
	}
}
