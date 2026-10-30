using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundApplicationObjectReferencePropertyValue : BoundPropertyValue
{
	public ApplicationObjectTypeSymbol Value { get; private set; }

	internal override object ValueObject => Value;

	internal override string ValueText => Value?.Name;

	public BoundApplicationObjectReferencePropertyValue(SyntaxNode syntax, ApplicationObjectTypeSymbol value, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ApplicationObjectReferencePropertyValue, syntax, type, hasErrors)
	{
		Value = value;
	}

	public BoundApplicationObjectReferencePropertyValue(SyntaxNode syntax, ApplicationObjectTypeSymbol value, TypeSymbol type)
		: base(BoundKind.ApplicationObjectReferencePropertyValue, syntax, type)
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitApplicationObjectReferencePropertyValue(this);
	}

	public BoundApplicationObjectReferencePropertyValue Update(ApplicationObjectTypeSymbol value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundApplicationObjectReferencePropertyValue(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
