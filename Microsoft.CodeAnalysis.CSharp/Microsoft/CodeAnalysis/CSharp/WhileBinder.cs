using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class WhileBinder : LoopBinder
{
	private readonly StatementSyntax _syntax;

	internal override SyntaxNode ScopeDesignator => _syntax;

	public WhileBinder(Binder enclosing, StatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	internal override BoundWhileStatement BindWhileParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		WhileStatementSyntax whileStatementSyntax = (WhileStatementSyntax)_syntax;
		BoundExpression condition = originalBinder.BindBooleanExpression(whileStatementSyntax.Condition, diagnostics);
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(whileStatementSyntax.Statement, diagnostics);
		return new BoundWhileStatement(whileStatementSyntax, Locals, condition, body, BreakLabel, ContinueLabel);
	}

	internal override BoundDoStatement BindDoParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		DoStatementSyntax doStatementSyntax = (DoStatementSyntax)_syntax;
		BoundExpression condition = originalBinder.BindBooleanExpression(doStatementSyntax.Condition, diagnostics);
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(doStatementSyntax.Statement, diagnostics);
		return new BoundDoStatement(doStatementSyntax, Locals, condition, body, BreakLabel, ContinueLabel);
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ExpressionVariableFinder.FindExpressionVariables(this, instance, _syntax.Kind() switch
		{
			SyntaxKind.WhileStatement => (CSharpSyntaxNode)((WhileStatementSyntax)_syntax).Condition, 
			SyntaxKind.DoStatement => ((DoStatementSyntax)_syntax).Condition, 
			_ => throw ExceptionUtilities.UnexpectedValue(_syntax.Kind()), 
		});
		return instance.ToImmutableAndFree();
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (_syntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
