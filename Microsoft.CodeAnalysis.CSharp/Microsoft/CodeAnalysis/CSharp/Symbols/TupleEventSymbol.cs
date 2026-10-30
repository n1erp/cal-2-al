using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TupleEventSymbol : WrappedEventSymbol
{
	private readonly TupleTypeSymbol _containingType;

	public override bool IsTupleEvent => true;

	public override EventSymbol TupleUnderlyingEvent => _underlyingEvent;

	public override Symbol ContainingSymbol => _containingType;

	public override TypeSymbol Type => _underlyingEvent.Type;

	public override MethodSymbol AddMethod => _containingType.GetTupleMemberSymbolForUnderlyingMember(_underlyingEvent.AddMethod);

	public override MethodSymbol RemoveMethod => _containingType.GetTupleMemberSymbolForUnderlyingMember(_underlyingEvent.RemoveMethod);

	internal override FieldSymbol AssociatedField => _containingType.GetTupleMemberSymbolForUnderlyingMember(_underlyingEvent.AssociatedField);

	internal override bool IsExplicitInterfaceImplementation => _underlyingEvent.IsExplicitInterfaceImplementation;

	public override ImmutableArray<EventSymbol> ExplicitInterfaceImplementations => _underlyingEvent.ExplicitInterfaceImplementations;

	internal override bool MustCallMethodsDirectly => _underlyingEvent.MustCallMethodsDirectly;

	public TupleEventSymbol(TupleTypeSymbol container, EventSymbol underlyingEvent)
		: base(underlyingEvent)
	{
		_containingType = container;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		DiagnosticInfo result = base.GetUseSiteDiagnostic();
		MergeUseSiteDiagnostics(ref result, _underlyingEvent.GetUseSiteDiagnostic());
		return result;
	}

	public override int GetHashCode()
	{
		return _underlyingEvent.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as TupleEventSymbol);
	}

	public bool Equals(TupleEventSymbol other)
	{
		if ((object)other == this)
		{
			return true;
		}
		if ((object)other != null && _containingType == other._containingType)
		{
			return _underlyingEvent == other._underlyingEvent;
		}
		return false;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingEvent.GetAttributes();
	}
}
