using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ExpressionListVariableBinder : LocalScopeBinder
{
	private readonly SeparatedSyntaxList<ExpressionSyntax> _expressions;

	internal override SyntaxNode ScopeDesignator => _expressions[0];

	internal ExpressionListVariableBinder(SeparatedSyntaxList<ExpressionSyntax> expressions, Binder next)
		: base(next)
	{
		_expressions = expressions;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ExpressionVariableFinder.FindExpressionVariables(this, instance, _expressions);
		return instance.ToImmutableAndFree();
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (ScopeDesignator == scopeDesignator)
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
