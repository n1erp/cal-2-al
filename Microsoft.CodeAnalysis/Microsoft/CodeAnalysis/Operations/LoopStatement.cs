using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class LoopStatement : Operation, ILoopOperation, IOperation
{
	public LoopKind LoopKind { get; }

	public ImmutableArray<ILocalSymbol> Locals { get; }

	public ILabelSymbol ContinueLabel { get; }

	public ILabelSymbol ExitLabel { get; }

	public abstract IOperation Body { get; }

	protected LoopStatement(LoopKind loopKind, ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		LoopKind = loopKind;
		Locals = locals;
		ContinueLabel = continueLabel;
		ExitLabel = exitLabel;
	}
}
