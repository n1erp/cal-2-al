using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ExpressionVariableBinder : LocalScopeBinder
{
	internal override SyntaxNode ScopeDesignator { get; }

	internal ExpressionVariableBinder(SyntaxNode scopeDesignator, Binder next)
		: base(next)
	{
		ScopeDesignator = scopeDesignator;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ExpressionVariableFinder.FindExpressionVariables(this, instance, (CSharpSyntaxNode)ScopeDesignator, GetBinder((CSharpSyntaxNode)ScopeDesignator));
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
