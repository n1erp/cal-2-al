using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundOptionMembersPropertyValue : BoundPropertyValue
{
	public TypeSymbol Value { get; private set; }

	internal override object ValueObject => ValueText;

	internal override string ValueText => (Value as OptionTypeSymbol)?.OptionMembers;

	public BoundOptionMembersPropertyValue(SyntaxNode syntax, TypeSymbol value, TypeSymbol type, bool hasErrors)
		: base(BoundKind.OptionMembersPropertyValue, syntax, type, hasErrors)
	{
		Value = value;
	}

	public BoundOptionMembersPropertyValue(SyntaxNode syntax, TypeSymbol value, TypeSymbol type)
		: base(BoundKind.OptionMembersPropertyValue, syntax, type)
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitOptionMembersPropertyValue(this);
	}

	public BoundOptionMembersPropertyValue Update(TypeSymbol value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundOptionMembersPropertyValue(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
