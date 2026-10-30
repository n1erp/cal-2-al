using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNameOfOperator : BoundExpression
{
	public override ConstantValue ConstantValue => ConstantValueOpt;

	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Argument);

	public BoundExpression Argument { get; }

	public ConstantValue ConstantValueOpt { get; }

	public BoundNameOfOperator(SyntaxNode syntax, BoundExpression argument, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.NameOfOperator, syntax, type, hasErrors || argument.HasErrors())
	{
		Argument = argument;
		ConstantValueOpt = constantValueOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNameOfOperator(this);
	}

	public BoundNameOfOperator Update(BoundExpression argument, ConstantValue constantValueOpt, TypeSymbol type)
	{
		if (argument != Argument || constantValueOpt != ConstantValueOpt || type != Type)
		{
			return new BoundNameOfOperator(Syntax, argument, constantValueOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
