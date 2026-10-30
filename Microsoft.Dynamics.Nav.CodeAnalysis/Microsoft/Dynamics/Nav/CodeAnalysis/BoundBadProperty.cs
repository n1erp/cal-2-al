using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundBadProperty : BoundProperty
{
	public BoundBadStatement BadProperty { get; private set; }

	public BoundBadProperty(SyntaxNode syntax, BoundBadStatement badProperty, BoundLiteral propertyName, BoundPropertyValue propertyValue, bool hasErrors = false)
		: base(BoundKind.BadProperty, syntax, propertyName, propertyValue, hasErrors || badProperty.HasErrors() || propertyName.HasErrors() || propertyValue.HasErrors())
	{
		BadProperty = badProperty;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBadProperty(this);
	}

	public BoundBadProperty Update(BoundBadStatement badProperty, BoundLiteral propertyName, BoundPropertyValue propertyValue)
	{
		if (badProperty != BadProperty || propertyName != PropertyName || propertyValue != PropertyValue)
		{
			return new BoundBadProperty(Syntax, badProperty, propertyName, propertyValue, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
