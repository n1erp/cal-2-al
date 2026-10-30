using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ForLoopBinder : LoopBinder
{
	private readonly ForStatementSyntax _syntax;

	internal override SyntaxNode ScopeDesignator => _syntax;

	public ForLoopBinder(Binder enclosing, ForStatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		if (_syntax.Declaration != null)
		{
			foreach (VariableDeclaratorSyntax variable in _syntax.Declaration.Variables)
			{
				SourceLocalSymbol item = MakeLocal(_syntax.Declaration, variable, LocalDeclarationKind.RegularVariable);
				instance.Add(item);
				ExpressionVariableFinder.FindExpressionVariables(this, instance, variable);
			}
		}
		else
		{
			ExpressionVariableFinder.FindExpressionVariables(this, instance, _syntax.Initializers);
		}
		return instance.ToImmutableAndFree();
	}

	internal override BoundForStatement BindForParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return BindForParts(_syntax, originalBinder, diagnostics);
	}

	private BoundForStatement BindForParts(ForStatementSyntax node, Binder originalBinder, DiagnosticBag diagnostics)
	{
		BoundStatement initializer = ((_syntax.Declaration == null) ? originalBinder.BindStatementExpressionList(node.Initializers, diagnostics) : originalBinder.BindForOrUsingOrFixedDeclarations(node.Declaration, LocalDeclarationKind.RegularVariable, diagnostics, out var _));
		BoundExpression condition = null;
		ImmutableArray<LocalSymbol> innerLocals = ImmutableArray<LocalSymbol>.Empty;
		ExpressionSyntax condition2 = node.Condition;
		if (condition2 != null)
		{
			originalBinder = originalBinder.GetBinder(condition2);
			condition = originalBinder.BindBooleanExpression(condition2, diagnostics);
			innerLocals = originalBinder.GetDeclaredLocalsForScope(condition2);
		}
		BoundStatement boundStatement = null;
		SeparatedSyntaxList<ExpressionSyntax> incrementors = node.Incrementors;
		if (incrementors.Count > 0)
		{
			ExpressionSyntax expressionSyntax = incrementors.First();
			Binder binder = originalBinder.GetBinder(expressionSyntax);
			boundStatement = binder.BindStatementExpressionList(incrementors, diagnostics);
			ImmutableArray<LocalSymbol> declaredLocalsForScope = binder.GetDeclaredLocalsForScope(expressionSyntax);
			if (!declaredLocalsForScope.IsEmpty)
			{
				boundStatement = ((boundStatement.Kind != BoundKind.StatementList) ? new BoundBlock(boundStatement.Syntax, declaredLocalsForScope, ImmutableArray.Create(boundStatement))
				{
					WasCompilerGenerated = true
				} : new BoundBlock(expressionSyntax, declaredLocalsForScope, ((BoundStatementList)boundStatement).Statements)
				{
					WasCompilerGenerated = true
				});
			}
		}
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(node.Statement, diagnostics);
		return new BoundForStatement(node, Locals, initializer, innerLocals, condition, boundStatement, body, BreakLabel, ContinueLabel);
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
