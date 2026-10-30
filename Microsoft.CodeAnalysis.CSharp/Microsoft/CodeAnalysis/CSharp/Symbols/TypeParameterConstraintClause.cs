using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TypeParameterConstraintClause
{
	public readonly TypeParameterConstraintKind Constraints;

	public readonly ImmutableArray<TypeSymbol> ConstraintTypes;

	public TypeParameterConstraintClause(TypeParameterConstraintKind constraints, ImmutableArray<TypeSymbol> constraintTypes)
	{
		Constraints = constraints;
		ConstraintTypes = constraintTypes;
	}
}
