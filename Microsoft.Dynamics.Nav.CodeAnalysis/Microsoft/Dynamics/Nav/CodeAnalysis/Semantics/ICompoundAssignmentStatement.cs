namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface ICompoundAssignmentStatement : IAssignmentStatement, IOperation
{
	CompoundAssignmentKind CompoundAssignmentKind { get; }
}
