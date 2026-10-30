using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCatchBlock : BoundNode
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundExpression ExceptionSourceOpt { get; }

	public TypeSymbol ExceptionTypeOpt { get; }

	public BoundExpression ExceptionFilterOpt { get; }

	public BoundBlock Body { get; }

	public bool IsSynthesizedAsyncCatchAll { get; }

	public BoundCatchBlock(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, BoundExpression exceptionSourceOpt, TypeSymbol exceptionTypeOpt, BoundExpression exceptionFilterOpt, BoundBlock body, bool isSynthesizedAsyncCatchAll, bool hasErrors = false)
		: base(BoundKind.CatchBlock, syntax, hasErrors || exceptionSourceOpt.HasErrors() || exceptionFilterOpt.HasErrors() || body.HasErrors())
	{
		Locals = locals;
		ExceptionSourceOpt = exceptionSourceOpt;
		ExceptionTypeOpt = exceptionTypeOpt;
		ExceptionFilterOpt = exceptionFilterOpt;
		Body = body;
		IsSynthesizedAsyncCatchAll = isSynthesizedAsyncCatchAll;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCatchBlock(this);
	}

	public BoundCatchBlock Update(ImmutableArray<LocalSymbol> locals, BoundExpression exceptionSourceOpt, TypeSymbol exceptionTypeOpt, BoundExpression exceptionFilterOpt, BoundBlock body, bool isSynthesizedAsyncCatchAll)
	{
		if (locals != Locals || exceptionSourceOpt != ExceptionSourceOpt || exceptionTypeOpt != ExceptionTypeOpt || exceptionFilterOpt != ExceptionFilterOpt || body != Body || isSynthesizedAsyncCatchAll != IsSynthesizedAsyncCatchAll)
		{
			return new BoundCatchBlock(Syntax, locals, exceptionSourceOpt, exceptionTypeOpt, exceptionFilterOpt, body, isSynthesizedAsyncCatchAll, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
