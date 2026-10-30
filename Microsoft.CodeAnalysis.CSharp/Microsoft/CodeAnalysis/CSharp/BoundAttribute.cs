using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAttribute : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => Constructor;

	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(ConstructorArguments.AddRange(NamedArguments));

	public MethodSymbol Constructor { get; }

	public ImmutableArray<BoundExpression> ConstructorArguments { get; }

	public ImmutableArray<string> ConstructorArgumentNamesOpt { get; }

	public ImmutableArray<BoundExpression> NamedArguments { get; }

	public override LookupResultKind ResultKind => _ResultKind;

	public BoundAttribute(SyntaxNode syntax, MethodSymbol constructor, ImmutableArray<BoundExpression> constructorArguments, ImmutableArray<string> constructorArgumentNamesOpt, ImmutableArray<BoundExpression> namedArguments, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.Attribute, syntax, type, hasErrors || constructorArguments.HasErrors() || namedArguments.HasErrors())
	{
		Constructor = constructor;
		ConstructorArguments = constructorArguments;
		ConstructorArgumentNamesOpt = constructorArgumentNamesOpt;
		NamedArguments = namedArguments;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAttribute(this);
	}

	public BoundAttribute Update(MethodSymbol constructor, ImmutableArray<BoundExpression> constructorArguments, ImmutableArray<string> constructorArgumentNamesOpt, ImmutableArray<BoundExpression> namedArguments, LookupResultKind resultKind, TypeSymbol type)
	{
		if (constructor != Constructor || constructorArguments != ConstructorArguments || constructorArgumentNamesOpt != ConstructorArgumentNamesOpt || namedArguments != NamedArguments || resultKind != ResultKind || type != Type)
		{
			return new BoundAttribute(Syntax, constructor, constructorArguments, constructorArgumentNamesOpt, namedArguments, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
