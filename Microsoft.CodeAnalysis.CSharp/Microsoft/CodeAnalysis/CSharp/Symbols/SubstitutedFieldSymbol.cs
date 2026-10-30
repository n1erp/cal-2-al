using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SubstitutedFieldSymbol : WrappedFieldSymbol
{
	private readonly SubstitutedNamedTypeSymbol _containingType;

	private TypeSymbol _lazyType;

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override FieldSymbol OriginalDefinition => _underlyingField;

	public override Symbol AssociatedSymbol => OriginalDefinition.AssociatedSymbol?.SymbolAsMember(ContainingType);

	public override ImmutableArray<CustomModifier> CustomModifiers => _containingType.TypeSubstitution.SubstituteCustomModifiers(OriginalDefinition.Type, OriginalDefinition.CustomModifiers);

	internal SubstitutedFieldSymbol(SubstitutedNamedTypeSymbol containingType, FieldSymbol substitutedFrom)
		: base(substitutedFrom.OriginalDefinition)
	{
		_containingType = containingType;
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		if ((object)_lazyType == null)
		{
			Interlocked.CompareExchange(ref _lazyType, _containingType.TypeSubstitution.SubstituteTypeWithTupleUnification(OriginalDefinition.GetFieldType(fieldsBeingBound)).Type, null);
		}
		return _lazyType;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return OriginalDefinition.GetAttributes();
	}

	internal override NamedTypeSymbol FixedImplementationType(PEModuleBuilder emitModule)
	{
		return (NamedTypeSymbol)_containingType.TypeSubstitution.SubstituteType(OriginalDefinition.FixedImplementationType(emitModule)).Type;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is SubstitutedFieldSymbol substitutedFieldSymbol && _containingType == substitutedFieldSymbol._containingType)
		{
			return OriginalDefinition == substitutedFieldSymbol.OriginalDefinition;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_containingType, OriginalDefinition.GetHashCode());
	}
}
