using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundScope : BoundStatementList
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundScope(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: base(BoundKind.Scope, syntax, statements, hasErrors || statements.HasErrors())
	{
		Locals = locals;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitScope(this);
	}

	public BoundScope Update(ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundStatement> statements)
	{
		if (locals != Locals || statements != Statements)
		{
			return new BoundScope(Syntax, locals, statements, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
