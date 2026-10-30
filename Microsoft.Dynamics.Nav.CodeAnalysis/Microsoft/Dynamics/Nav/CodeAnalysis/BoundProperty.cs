using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class BoundProperty : BoundStatement
{
	public BoundLiteral PropertyName { get; private set; }

	public BoundPropertyValue PropertyValue { get; private set; }

	protected override OperationKind StatementKind => OperationKind.None;

	protected BoundProperty(BoundKind kind, SyntaxNode syntax, BoundLiteral propertyName, BoundPropertyValue propertyValue, bool hasErrors = false)
		: base(kind, syntax, hasErrors)
	{
		PropertyName = propertyName;
		PropertyValue = propertyValue;
	}

	public BoundProperty(SyntaxNode syntax, BoundLiteral propertyName, BoundPropertyValue propertyValue, bool hasErrors = false)
		: base(BoundKind.Property, syntax, hasErrors || propertyName.HasErrors() || propertyValue.HasErrors())
	{
		PropertyName = propertyName;
		PropertyValue = propertyValue;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitProperty(this);
	}

	public BoundProperty Update(BoundLiteral propertyName, BoundPropertyValue propertyValue)
	{
		if (propertyName != PropertyName || propertyValue != PropertyValue)
		{
			return new BoundProperty(Syntax, propertyName, propertyValue, HasErrors)
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
