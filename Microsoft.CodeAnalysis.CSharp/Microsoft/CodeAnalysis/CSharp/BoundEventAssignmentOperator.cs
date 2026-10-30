using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundEventAssignmentOperator : BoundExpression
{
	public EventSymbol Event { get; }

	public bool IsAddition { get; }

	public bool IsDynamic { get; }

	public BoundExpression ReceiverOpt { get; }

	public BoundExpression Argument { get; }

	public BoundEventAssignmentOperator(SyntaxNode syntax, EventSymbol @event, bool isAddition, bool isDynamic, BoundExpression receiverOpt, BoundExpression argument, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.EventAssignmentOperator, syntax, type, hasErrors || receiverOpt.HasErrors() || argument.HasErrors())
	{
		Event = @event;
		IsAddition = isAddition;
		IsDynamic = isDynamic;
		ReceiverOpt = receiverOpt;
		Argument = argument;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitEventAssignmentOperator(this);
	}

	public BoundEventAssignmentOperator Update(EventSymbol @event, bool isAddition, bool isDynamic, BoundExpression receiverOpt, BoundExpression argument, TypeSymbol type)
	{
		if (@event != Event || isAddition != IsAddition || isDynamic != IsDynamic || receiverOpt != ReceiverOpt || argument != Argument || type != Type)
		{
			return new BoundEventAssignmentOperator(Syntax, @event, isAddition, isDynamic, receiverOpt, argument, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
