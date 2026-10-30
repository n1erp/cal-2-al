using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundListOfConstantValuePairsPropertyValue : BoundPropertyValue
{
	public ImmutableArray<BoundPairOfConstantValues> Value { get; private set; }

	public BoundListOfConstantValuePairsPropertyValue(SyntaxNode syntax, ImmutableArray<BoundPairOfConstantValues> value, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ListOfConstantValuePairsPropertyValue, syntax, type, hasErrors || value.HasErrors())
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitListOfConstantValuePairsPropertyValue(this);
	}

	public BoundListOfConstantValuePairsPropertyValue Update(ImmutableArray<BoundPairOfConstantValues> value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundListOfConstantValuePairsPropertyValue(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
