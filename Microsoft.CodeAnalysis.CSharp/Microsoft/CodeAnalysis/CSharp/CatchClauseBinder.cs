using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CatchClauseBinder : LocalScopeBinder
{
	private readonly CatchClauseSyntax _syntax;

	internal override SyntaxNode ScopeDesignator => _syntax;

	public CatchClauseBinder(Binder enclosing, CatchClauseSyntax syntax)
		: base(enclosing, (enclosing.Flags | BinderFlags.InCatchBlock) & ~BinderFlags.InNestedFinallyBlock)
	{
		_syntax = syntax;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		CatchDeclarationSyntax declaration = _syntax.Declaration;
		if (declaration != null && declaration.Identifier.Kind() != SyntaxKind.None)
		{
			instance.Add(SourceLocalSymbol.MakeLocal(ContainingMemberOrLambda, this, allowRefKind: false, declaration.Type, declaration.Identifier, LocalDeclarationKind.CatchVariable));
		}
		if (_syntax.Filter != null)
		{
			ExpressionVariableFinder.FindExpressionVariables(this, instance, _syntax.Filter.FilterExpression);
		}
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
