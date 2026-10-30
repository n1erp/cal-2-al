using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTemporary : BoundExpression
{
	public string Name { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundTemporary(SyntaxNode syntax, string name, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Temporary, syntax, type, hasErrors)
	{
		Name = name;
	}

	public BoundTemporary(SyntaxNode syntax, string name, TypeSymbol type)
		: base(BoundKind.Temporary, syntax, type)
	{
		Name = name;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTemporary(this);
	}

	public BoundTemporary Update(string name, TypeSymbol type)
	{
		if (name != Name || type != Type)
		{
			return new BoundTemporary(Syntax, name, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNoneOperation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNoneOperation(this, argument);
	}
}
