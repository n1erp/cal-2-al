using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundFixedStatement : BoundStatement
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Declarations, (BoundNode)Body);

	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundMultipleLocalDeclarations Declarations { get; }

	public BoundStatement Body { get; }

	public BoundFixedStatement(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, BoundMultipleLocalDeclarations declarations, BoundStatement body, bool hasErrors = false)
		: base(BoundKind.FixedStatement, syntax, hasErrors || declarations.HasErrors() || body.HasErrors())
	{
		Locals = locals;
		Declarations = declarations;
		Body = body;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitFixedStatement(this);
	}

	public BoundFixedStatement Update(ImmutableArray<LocalSymbol> locals, BoundMultipleLocalDeclarations declarations, BoundStatement body)
	{
		if (locals != Locals || declarations != Declarations || body != Body)
		{
			return new BoundFixedStatement(Syntax, locals, declarations, body, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
