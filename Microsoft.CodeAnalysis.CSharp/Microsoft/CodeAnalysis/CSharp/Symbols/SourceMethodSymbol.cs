using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceMethodSymbol : MethodSymbol
{
	public abstract ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses { get; }
}
