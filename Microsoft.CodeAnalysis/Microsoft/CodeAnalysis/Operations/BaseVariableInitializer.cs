using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseVariableInitializer : SymbolInitializer, IVariableInitializerOperation, ISymbolInitializerOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Value != null)
			{
				yield return Value;
			}
		}
	}

	ImmutableArray<ILocalSymbol> ISymbolInitializerOperation.Locals => ImmutableArray<ILocalSymbol>.Empty;

	public BaseVariableInitializer(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.VariableInitializer, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitVariableInitializer(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitVariableInitializer(this, argument);
	}
}
