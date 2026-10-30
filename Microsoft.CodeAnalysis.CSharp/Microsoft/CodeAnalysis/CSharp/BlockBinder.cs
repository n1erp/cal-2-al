using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BlockBinder : LocalScopeBinder
{
	private readonly BlockSyntax _block;

	internal override bool IsLocalFunctionsScopeBinder => true;

	internal override bool IsLabelsScopeBinder => true;

	internal override SyntaxNode ScopeDesignator => _block;

	public BlockBinder(Binder enclosing, BlockSyntax block)
		: this(enclosing, block, enclosing.Flags)
	{
	}

	public BlockBinder(Binder enclosing, BlockSyntax block, BinderFlags additionalFlags)
		: base(enclosing, enclosing.Flags | additionalFlags)
	{
		_block = block;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		return BuildLocals(_block.Statements, this);
	}

	protected override ImmutableArray<LocalFunctionSymbol> BuildLocalFunctions()
	{
		return BuildLocalFunctions(_block.Statements);
	}

	protected override ImmutableArray<LabelSymbol> BuildLabels()
	{
		ArrayBuilder<LabelSymbol> labels = null;
		BuildLabels(_block.Statements, ref labels);
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
