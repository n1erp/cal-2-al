using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class TupleFieldSymbol : WrappedFieldSymbol
{
	protected readonly TupleTypeSymbol _containingTuple;

	private readonly int _tupleElementIndex;

	public override bool IsTupleField => true;

	public override int TupleElementIndex
	{
		get
		{
			if (_tupleElementIndex < 0)
			{
				return -1;
			}
			return _tupleElementIndex >> 1;
		}
	}

	public override bool IsDefaultTupleElement => (_tupleElementIndex & -2147483647) == 0;

	public override FieldSymbol TupleUnderlyingField => _underlyingField;

	public override Symbol AssociatedSymbol => _containingTuple.GetTupleMemberSymbolForUnderlyingMember(_underlyingField.AssociatedSymbol);

	public override Symbol ContainingSymbol => _containingTuple;

	public override ImmutableArray<CustomModifier> CustomModifiers => _underlyingField.CustomModifiers;

	public TupleFieldSymbol(TupleTypeSymbol container, FieldSymbol underlyingField, int tupleElementIndex)
		: base(underlyingField)
	{
		_containingTuple = container;
		_tupleElementIndex = tupleElementIndex;
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _underlyingField.GetFieldType(fieldsBeingBound);
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _underlyingField.GetAttributes();
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		DiagnosticInfo result = base.GetUseSiteDiagnostic();
		MergeUseSiteDiagnostics(ref result, _underlyingField.GetUseSiteDiagnostic());
		return result;
	}

	public sealed override int GetHashCode()
	{
		int hashCode = _containingTuple.GetHashCode();
		int tupleElementIndex = _tupleElementIndex;
		return Hash.Combine(hashCode, tupleElementIndex.GetHashCode());
	}

	public sealed override bool Equals(object obj)
	{
		TupleFieldSymbol tupleFieldSymbol = obj as TupleFieldSymbol;
		if ((object)tupleFieldSymbol == this)
		{
			return true;
		}
		if ((object)tupleFieldSymbol != null && _tupleElementIndex == tupleFieldSymbol._tupleElementIndex)
		{
			return _containingTuple == tupleFieldSymbol._containingTuple;
		}
		return false;
	}
}
