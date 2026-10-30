using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTableKeyAccess : BoundExpression, ITableKeyAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => Key;

	public BoundExpression ReceiverOpt { get; private set; }

	public FieldNameListSymbol Key { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation ITableKeyAccess.Instance => ReceiverOpt;

	protected override OperationKind ExpressionKind => OperationKind.TableKeyAccess;

	public BoundTableKeyAccess(SyntaxNode syntax, BoundExpression receiverOpt, FieldNameListSymbol key, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TableKeyAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		Key = key;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTableKeyAccess(this);
	}

	public BoundTableKeyAccess Update(BoundExpression receiverOpt, FieldNameListSymbol key, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || key != Key || resultKind != ResultKind || type != Type)
		{
			return new BoundTableKeyAccess(Syntax, receiverOpt, key, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTableKeyAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTableKeyAccess(this, argument);
	}
}
