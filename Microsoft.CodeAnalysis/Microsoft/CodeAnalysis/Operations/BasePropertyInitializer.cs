using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BasePropertyInitializer : SymbolInitializer, IPropertyInitializerOperation, ISymbolInitializerOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> Locals { get; }

	public ImmutableArray<IPropertySymbol> InitializedProperties { get; }

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

	public BasePropertyInitializer(ImmutableArray<ILocalSymbol> locals, ImmutableArray<IPropertySymbol> initializedProperties, OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Locals = locals;
		InitializedProperties = initializedProperties;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitPropertyInitializer(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitPropertyInitializer(this, argument);
	}
}
