using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBlock : BoundStatementList
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public ImmutableArray<LocalFunctionSymbol> LocalFunctions { get; }

	public BoundBlock(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: this(syntax, locals, ImmutableArray<LocalFunctionSymbol>.Empty, statements, hasErrors)
	{
	}

	public static BoundBlock SynthesizedNoLocals(SyntaxNode syntax, BoundStatement statement)
	{
		return new BoundBlock(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(statement))
		{
			WasCompilerGenerated = true
		};
	}

	public static BoundBlock SynthesizedNoLocals(SyntaxNode syntax, ImmutableArray<BoundStatement> statements)
	{
		return new BoundBlock(syntax, ImmutableArray<LocalSymbol>.Empty, statements)
		{
			WasCompilerGenerated = true
		};
	}

	public static BoundBlock SynthesizedNoLocals(SyntaxNode syntax, params BoundStatement[] statements)
	{
		return new BoundBlock(syntax, ImmutableArray<LocalSymbol>.Empty, statements.AsImmutableOrNull())
		{
			WasCompilerGenerated = true
		};
	}

	public BoundBlock(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, ImmutableArray<LocalFunctionSymbol> localFunctions, ImmutableArray<BoundStatement> statements, bool hasErrors = false)
		: base(BoundKind.Block, syntax, statements, hasErrors || statements.HasErrors())
	{
		Locals = locals;
		LocalFunctions = localFunctions;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBlock(this);
	}

	public BoundBlock Update(ImmutableArray<LocalSymbol> locals, ImmutableArray<LocalFunctionSymbol> localFunctions, ImmutableArray<BoundStatement> statements)
	{
		if (locals != Locals || localFunctions != LocalFunctions || statements != Statements)
		{
			return new BoundBlock(Syntax, locals, localFunctions, statements, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
