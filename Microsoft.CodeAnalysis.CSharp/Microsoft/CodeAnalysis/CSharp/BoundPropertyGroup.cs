using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPropertyGroup : BoundMethodOrPropertyGroup
{
	public override object Display
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public ImmutableArray<PropertySymbol> Properties { get; }

	public BoundPropertyGroup(SyntaxNode syntax, ImmutableArray<PropertySymbol> properties, BoundExpression receiverOpt, LookupResultKind resultKind, bool hasErrors = false)
		: base(BoundKind.PropertyGroup, syntax, receiverOpt, resultKind, hasErrors || receiverOpt.HasErrors())
	{
		Properties = properties;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPropertyGroup(this);
	}

	public BoundPropertyGroup Update(ImmutableArray<PropertySymbol> properties, BoundExpression receiverOpt, LookupResultKind resultKind)
	{
		if (properties != Properties || receiverOpt != ReceiverOpt || resultKind != ResultKind)
		{
			return new BoundPropertyGroup(Syntax, properties, receiverOpt, resultKind, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
