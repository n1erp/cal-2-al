using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundLabel : BoundExpression
{
	public ConstantValue Text { get; private set; }

	public ConstantValue Comment { get; private set; }

	public ConstantValue MaxLength { get; private set; }

	public ConstantValue Locked { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundLabel(SyntaxNode syntax, ConstantValue text, ConstantValue comment, ConstantValue maxLength, ConstantValue locked, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Label, syntax, type, hasErrors)
	{
		Text = text;
		Comment = comment;
		MaxLength = maxLength;
		Locked = locked;
	}

	public BoundLabel(SyntaxNode syntax, ConstantValue text, ConstantValue comment, ConstantValue maxLength, ConstantValue locked, TypeSymbol type)
		: base(BoundKind.Label, syntax, type)
	{
		Text = text;
		Comment = comment;
		MaxLength = maxLength;
		Locked = locked;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLabel(this);
	}

	public BoundLabel Update(ConstantValue text, ConstantValue comment, ConstantValue maxLength, ConstantValue locked, TypeSymbol type)
	{
		if (text != Text || comment != Comment || maxLength != MaxLength || locked != Locked || type != Type)
		{
			return new BoundLabel(Syntax, text, comment, maxLength, locked, type, HasErrors)
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
