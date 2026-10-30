using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundObjectAccess : BoundExpression, IObjectAccess, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override Symbol ExpressionSymbol => ObjectTypeSymbol;

	public BoundExpression ReceiverOpt { get; private set; }

	public ObjectTypeSymbol ObjectTypeSymbol { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IObjectAccess.Instance => ReceiverOpt;

	IObjectTypeSymbol IObjectAccess.ObjectTypeSymbol => ObjectTypeSymbol;

	protected override OperationKind ExpressionKind => OperationKind.FieldAccess;

	public BoundObjectAccess(SyntaxNode syntax, BoundExpression receiverOpt, ObjectTypeSymbol objectTypeSymbol, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ObjectAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		ObjectTypeSymbol = objectTypeSymbol;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitObjectAccess(this);
	}

	public BoundObjectAccess Update(BoundExpression receiverOpt, ObjectTypeSymbol objectTypeSymbol, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || objectTypeSymbol != ObjectTypeSymbol || resultKind != ResultKind || type != Type)
		{
			return new BoundObjectAccess(Syntax, receiverOpt, objectTypeSymbol, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitObjectAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitObjectAccess(this, argument);
	}
}
