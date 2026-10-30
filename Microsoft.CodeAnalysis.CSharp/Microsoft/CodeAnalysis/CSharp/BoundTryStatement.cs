using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTryStatement : BoundStatement
{
	public BoundBlock TryBlock { get; }

	public ImmutableArray<BoundCatchBlock> CatchBlocks { get; }

	public BoundBlock FinallyBlockOpt { get; }

	public bool PreferFaultHandler { get; }

	public BoundTryStatement(SyntaxNode syntax, BoundBlock tryBlock, ImmutableArray<BoundCatchBlock> catchBlocks, BoundBlock finallyBlockOpt)
		: this(syntax, tryBlock, catchBlocks, finallyBlockOpt, preferFaultHandler: false)
	{
	}

	public BoundTryStatement(SyntaxNode syntax, BoundBlock tryBlock, ImmutableArray<BoundCatchBlock> catchBlocks, BoundBlock finallyBlockOpt, bool preferFaultHandler, bool hasErrors = false)
		: base(BoundKind.TryStatement, syntax, hasErrors || tryBlock.HasErrors() || catchBlocks.HasErrors() || finallyBlockOpt.HasErrors())
	{
		TryBlock = tryBlock;
		CatchBlocks = catchBlocks;
		FinallyBlockOpt = finallyBlockOpt;
		PreferFaultHandler = preferFaultHandler;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTryStatement(this);
	}

	public BoundTryStatement Update(BoundBlock tryBlock, ImmutableArray<BoundCatchBlock> catchBlocks, BoundBlock finallyBlockOpt, bool preferFaultHandler)
	{
		if (tryBlock != TryBlock || catchBlocks != CatchBlocks || finallyBlockOpt != FinallyBlockOpt || preferFaultHandler != PreferFaultHandler)
		{
			return new BoundTryStatement(Syntax, tryBlock, catchBlocks, finallyBlockOpt, preferFaultHandler, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
