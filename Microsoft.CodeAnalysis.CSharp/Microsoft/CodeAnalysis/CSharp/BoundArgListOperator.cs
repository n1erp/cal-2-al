using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundArgListOperator : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(Arguments);

	public override object Display => "__arglist";

	public ImmutableArray<BoundExpression> Arguments { get; }

	public ImmutableArray<RefKind> ArgumentRefKindsOpt { get; }

	public BoundArgListOperator(SyntaxNode syntax, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> argumentRefKindsOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ArgListOperator, syntax, type, hasErrors || arguments.HasErrors())
	{
		Arguments = arguments;
		ArgumentRefKindsOpt = argumentRefKindsOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitArgListOperator(this);
	}

	public BoundArgListOperator Update(ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> argumentRefKindsOpt, TypeSymbol type)
	{
		if (arguments != Arguments || argumentRefKindsOpt != ArgumentRefKindsOpt || type != Type)
		{
			return new BoundArgListOperator(Syntax, arguments, argumentRefKindsOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
