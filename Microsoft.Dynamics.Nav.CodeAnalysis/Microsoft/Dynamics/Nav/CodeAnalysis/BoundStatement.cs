using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class BoundStatement : BoundNode, IOperation
{
	protected abstract OperationKind StatementKind { get; }

	OperationKind IOperation.Kind => StatementKind;

	bool IOperation.IsInvalid => HasErrors;

	SyntaxNode IOperation.Syntax => Syntax;

	ITypeSymbol IOperation.Type => null;

	Optional<object> IOperation.ConstantValue => default;

	protected BoundStatement(BoundKind kind, SyntaxNode syntax, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
	}

	protected BoundStatement(BoundKind kind, SyntaxNode syntax)
		: base(kind, syntax)
	{
	}

	public abstract void Accept(OperationVisitor visitor);

	public abstract TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument);
}
