namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundForEachDeconstructStep : BoundNode
{
	public BoundDeconstructionAssignmentOperator DeconstructionAssignment { get; }

	public BoundDeconstructValuePlaceholder TargetPlaceholder { get; }

	public BoundForEachDeconstructStep(SyntaxNode syntax, BoundDeconstructionAssignmentOperator deconstructionAssignment, BoundDeconstructValuePlaceholder targetPlaceholder, bool hasErrors = false)
		: base(BoundKind.ForEachDeconstructStep, syntax, hasErrors || deconstructionAssignment.HasErrors() || targetPlaceholder.HasErrors())
	{
		DeconstructionAssignment = deconstructionAssignment;
		TargetPlaceholder = targetPlaceholder;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitForEachDeconstructStep(this);
	}

	public BoundForEachDeconstructStep Update(BoundDeconstructionAssignmentOperator deconstructionAssignment, BoundDeconstructValuePlaceholder targetPlaceholder)
	{
		if (deconstructionAssignment != DeconstructionAssignment || targetPlaceholder != TargetPlaceholder)
		{
			return new BoundForEachDeconstructStep(Syntax, deconstructionAssignment, targetPlaceholder, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
