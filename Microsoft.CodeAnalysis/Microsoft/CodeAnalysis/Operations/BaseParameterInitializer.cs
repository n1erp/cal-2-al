using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseParameterInitializer : SymbolInitializer, IParameterInitializerOperation, ISymbolInitializerOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> Locals { get; }

	public IParameterSymbol Parameter { get; }

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

	public BaseParameterInitializer(ImmutableArray<ILocalSymbol> locals, IParameterSymbol parameter, OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Locals = locals;
		Parameter = parameter;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitParameterInitializer(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitParameterInitializer(this, argument);
	}
}
