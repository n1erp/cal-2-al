using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundBreakStatement : BoundStatement, IBreakStatement, IOperation
{
	public GeneratedLabelSymbol Label { get; private set; }

	protected override OperationKind StatementKind => OperationKind.BreakStatement;

	public BoundBreakStatement(SyntaxNode syntax, GeneratedLabelSymbol label, bool hasErrors)
		: base(BoundKind.BreakStatement, syntax, hasErrors)
	{
		Label = label;
	}

	public BoundBreakStatement(SyntaxNode syntax, GeneratedLabelSymbol label)
		: base(BoundKind.BreakStatement, syntax)
	{
		Label = label;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBreakStatement(this);
	}

	public BoundBreakStatement Update(GeneratedLabelSymbol label)
	{
		if (label != Label)
		{
			return new BoundBreakStatement(Syntax, label, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitBreakStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitBreakStatement(this, argument);
	}
}
