using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTestFilterFieldAccess : BoundExpression, ITestFilterFieldAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public BoundExpression ReceiverOpt { get; private set; }

	public TestFilterFieldSymbol TestFilterField { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation ITestFilterFieldAccess.Instance => ReceiverOpt;

	IFieldSymbol ITestFilterFieldAccess.FieldSymbol => TestFilterField.RelatedField;

	protected override OperationKind ExpressionKind => OperationKind.TestFilterFieldAccess;

	public BoundTestFilterFieldAccess(SyntaxNode syntax, BoundExpression receiverOpt, TestFilterFieldSymbol testFilterField, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TestFilterFieldAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		TestFilterField = testFilterField;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTestFilterFieldAccess(this);
	}

	public BoundTestFilterFieldAccess Update(BoundExpression receiverOpt, TestFilterFieldSymbol testFilterField, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || testFilterField != TestFilterField || resultKind != ResultKind || type != Type)
		{
			return new BoundTestFilterFieldAccess(Syntax, receiverOpt, testFilterField, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTestFilterFieldAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTestFilterFieldAccess(this, argument);
	}
}
