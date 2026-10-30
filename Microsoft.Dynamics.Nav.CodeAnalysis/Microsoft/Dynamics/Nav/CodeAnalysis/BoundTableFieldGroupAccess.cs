using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTableFieldGroupAccess : BoundExpression, ITableFieldGroupAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => FieldGroup;

	public BoundExpression ReceiverOpt { get; private set; }

	public FieldGroupSymbol FieldGroup { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation ITableFieldGroupAccess.Instance => ReceiverOpt;

	protected override OperationKind ExpressionKind => OperationKind.TableFieldGroupAccess;

	public BoundTableFieldGroupAccess(SyntaxNode syntax, BoundExpression receiverOpt, FieldGroupSymbol fieldGroup, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TableFieldGroupAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		FieldGroup = fieldGroup;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTableFieldGroupAccess(this);
	}

	public BoundTableFieldGroupAccess Update(BoundExpression receiverOpt, FieldGroupSymbol fieldGroup, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || fieldGroup != FieldGroup || resultKind != ResultKind || type != Type)
		{
			return new BoundTableFieldGroupAccess(Syntax, receiverOpt, fieldGroup, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTableFieldGroupAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTableFieldGroupAccess(this, argument);
	}
}
