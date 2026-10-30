using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class EmbeddedStatementBinder : LocalScopeBinder
{
	private readonly SyntaxList<StatementSyntax> _statements;

	internal override bool IsLocalFunctionsScopeBinder => true;

	internal override bool IsLabelsScopeBinder => true;

	internal override SyntaxNode ScopeDesignator => _statements.First();

	public EmbeddedStatementBinder(Binder enclosing, StatementSyntax statement)
		: base(enclosing, enclosing.Flags)
	{
		_statements = new SyntaxList<StatementSyntax>(statement);
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		return BuildLocals(_statements, this);
	}

	protected override ImmutableArray<LocalFunctionSymbol> BuildLocalFunctions()
	{
		return BuildLocalFunctions(_statements);
	}

	protected override ImmutableArray<LabelSymbol> BuildLabels()
	{
		ArrayBuilder<LabelSymbol> labels = null;
		BuildLabels(_statements, ref labels);
		return labels?.ToImmutableAndFree() ?? ImmutableArray<LabelSymbol>.Empty;
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
		if (ScopeDesignator == scopeDesignator)
		{
			return LocalFunctions;
		}
		throw ExceptionUtilities.Unreachable;
	}
}
