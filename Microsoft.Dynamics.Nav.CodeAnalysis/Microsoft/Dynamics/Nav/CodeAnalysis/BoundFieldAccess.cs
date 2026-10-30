using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundFieldAccess : BoundExpression, IFieldAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => FieldSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public FieldSymbol FieldSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IFieldAccess.Instance => ReceiverOpt;

	IFieldSymbol IFieldAccess.FieldSymbol => FieldSymbol;

	protected override OperationKind ExpressionKind => OperationKind.FieldAccess;

	public BoundFieldAccess(SyntaxNode syntax, BoundExpression receiverOpt, FieldSymbol fieldSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.FieldAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		FieldSymbol = fieldSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitFieldAccess(this);
	}

	public BoundFieldAccess Update(BoundExpression receiverOpt, FieldSymbol fieldSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || fieldSymbol != FieldSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundFieldAccess(Syntax, receiverOpt, fieldSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitFieldAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitFieldAccess(this, argument);
	}
}
