using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDeconstructionAssignmentOperator : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Left, (BoundNode)Right);

	public BoundTupleExpression Left { get; }

	public BoundConversion Right { get; }

	public bool IsUsed { get; }

	public BoundDeconstructionAssignmentOperator(SyntaxNode syntax, BoundTupleExpression left, BoundConversion right, bool isUsed, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DeconstructionAssignmentOperator, syntax, type, hasErrors || left.HasErrors() || right.HasErrors())
	{
		Left = left;
		Right = right;
		IsUsed = isUsed;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDeconstructionAssignmentOperator(this);
	}

	public BoundDeconstructionAssignmentOperator Update(BoundTupleExpression left, BoundConversion right, bool isUsed, TypeSymbol type)
	{
		if (left != Left || right != Right || isUsed != IsUsed || type != Type)
		{
			return new BoundDeconstructionAssignmentOperator(Syntax, left, right, isUsed, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
