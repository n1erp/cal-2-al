using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTestFilterAccess : BoundExpression, ITestFilterAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public BoundExpression ReceiverOpt { get; private set; }

	public TestFilterSymbol TestFilter { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation ITestFilterAccess.Instance => ReceiverOpt;

	ISymbol ITestFilterAccess.TableSymbol => TestFilter.RelatedTable;

	protected override OperationKind ExpressionKind => OperationKind.TestFilterAccess;

	public BoundTestFilterAccess(SyntaxNode syntax, BoundExpression receiverOpt, TestFilterSymbol testFilter, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TestFilterAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		TestFilter = testFilter;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTestFilterAccess(this);
	}

	public BoundTestFilterAccess Update(BoundExpression receiverOpt, TestFilterSymbol testFilter, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || testFilter != TestFilter || resultKind != ResultKind || type != Type)
		{
			return new BoundTestFilterAccess(Syntax, receiverOpt, testFilter, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTestFilterAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTestFilterAccess(this, argument);
	}
}
