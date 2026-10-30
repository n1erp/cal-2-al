using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseFieldInitializer : SymbolInitializer, IFieldInitializerOperation, ISymbolInitializerOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> Locals { get; }

	public ImmutableArray<IFieldSymbol> InitializedFields { get; }

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

	public BaseFieldInitializer(ImmutableArray<ILocalSymbol> locals, ImmutableArray<IFieldSymbol> initializedFields, OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Locals = locals;
		InitializedFields = initializedFields;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitFieldInitializer(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitFieldInitializer(this, argument);
	}
}
