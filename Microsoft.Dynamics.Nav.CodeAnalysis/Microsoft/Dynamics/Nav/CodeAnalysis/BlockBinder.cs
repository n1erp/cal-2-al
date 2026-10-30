using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class BlockBinder : LocalScopeBinder
{
	private readonly SyntaxList<StatementSyntax> statements;

	public BlockBinder(Binder enclosing, SyntaxList<StatementSyntax> statements)
		: this(enclosing, statements, enclosing.Flags)
	{
	}

	public BlockBinder(Binder enclosing, SyntaxList<StatementSyntax> statements, BinderFlags additionalFlags)
		: base(enclosing, enclosing.Flags | additionalFlags)
	{
		this.statements = statements;
	}

	protected override ImmutableArray<VariableSymbol> BuildLocals()
	{
		return BuildLocals(statements);
	}

	internal override ImmutableArray<VariableSymbol> GetDeclaredLocalsForScope(SyntaxNode node)
	{
		if (node.Kind == SyntaxKind.Block)
		{
			if (((BlockSyntax)node).Statements == statements)
			{
				return Locals;
			}
		}
		else if (statements.Count == 1 && statements.First() == node)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable;
	}
}
