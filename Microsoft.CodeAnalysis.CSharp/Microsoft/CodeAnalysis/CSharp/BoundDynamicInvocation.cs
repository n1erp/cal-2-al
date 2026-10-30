using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDynamicInvocation : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(Arguments.Insert(0, Expression));

	public BoundExpression Expression { get; }

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<string> ArgumentNamesOpt { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public ImmutableArray<MethodSymbol> ApplicableMethods { get; }

	public BoundDynamicInvocation(SyntaxNode syntax, BoundExpression expression, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, ImmutableArray<MethodSymbol> applicableMethods, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DynamicInvocation, syntax, type, hasErrors || expression.HasErrors() || arguments.HasErrors())
	{
		Expression = expression;
		Arguments = arguments;
		ArgumentNamesOpt = argumentNamesOpt;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
		ApplicableMethods = applicableMethods;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDynamicInvocation(this);
	}

	public BoundDynamicInvocation Update(BoundExpression expression, ImmutableArray<BoundExpression> arguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, ImmutableArray<MethodSymbol> applicableMethods, TypeSymbol type)
	{
		if (expression != Expression || arguments != Arguments || argumentNamesOpt != ArgumentNamesOpt || argumentRefKindsOpt != ArgumentRefKindsOpt || applicableMethods != ApplicableMethods || type != Type)
		{
			return new BoundDynamicInvocation(Syntax, expression, arguments, argumentNamesOpt, argumentRefKindsOpt, applicableMethods, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
