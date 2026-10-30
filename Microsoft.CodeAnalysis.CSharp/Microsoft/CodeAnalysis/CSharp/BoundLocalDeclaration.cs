using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLocalDeclaration : BoundStatement
{
	public LocalSymbol LocalSymbol { get; }

	public BoundTypeExpression DeclaredType { get; }

	public BoundExpression InitializerOpt { get; }

	public ImmutableArray<BoundExpression> ArgumentsOpt { get; }

	public BoundLocalDeclaration(SyntaxNode syntax, LocalSymbol localSymbol, BoundTypeExpression declaredType, BoundExpression initializerOpt, ImmutableArray<BoundExpression> argumentsOpt, bool hasErrors = false)
		: base(BoundKind.LocalDeclaration, syntax, hasErrors || declaredType.HasErrors() || initializerOpt.HasErrors() || argumentsOpt.HasErrors())
	{
		LocalSymbol = localSymbol;
		DeclaredType = declaredType;
		InitializerOpt = initializerOpt;
		ArgumentsOpt = argumentsOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLocalDeclaration(this);
	}

	public BoundLocalDeclaration Update(LocalSymbol localSymbol, BoundTypeExpression declaredType, BoundExpression initializerOpt, ImmutableArray<BoundExpression> argumentsOpt)
	{
		if (localSymbol != LocalSymbol || declaredType != DeclaredType || initializerOpt != InitializerOpt || argumentsOpt != ArgumentsOpt)
		{
			return new BoundLocalDeclaration(Syntax, localSymbol, declaredType, initializerOpt, argumentsOpt, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
