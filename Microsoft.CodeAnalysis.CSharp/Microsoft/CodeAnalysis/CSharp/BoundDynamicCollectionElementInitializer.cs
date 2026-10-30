using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDynamicCollectionElementInitializer : BoundExpression
{
	public ImmutableArray<BoundExpression> Arguments { get; }

	public BoundImplicitReceiver ImplicitReceiver { get; }

	public ImmutableArray<MethodSymbol> ApplicableMethods { get; }

	public BoundDynamicCollectionElementInitializer(SyntaxNode syntax, ImmutableArray<BoundExpression> arguments, BoundImplicitReceiver implicitReceiver, ImmutableArray<MethodSymbol> applicableMethods, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DynamicCollectionElementInitializer, syntax, type, hasErrors || arguments.HasErrors() || implicitReceiver.HasErrors())
	{
		Arguments = arguments;
		ImplicitReceiver = implicitReceiver;
		ApplicableMethods = applicableMethods;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDynamicCollectionElementInitializer(this);
	}

	public BoundDynamicCollectionElementInitializer Update(ImmutableArray<BoundExpression> arguments, BoundImplicitReceiver implicitReceiver, ImmutableArray<MethodSymbol> applicableMethods, TypeSymbol type)
	{
		if (arguments != Arguments || implicitReceiver != ImplicitReceiver || applicableMethods != ApplicableMethods || type != Type)
		{
			return new BoundDynamicCollectionElementInitializer(Syntax, arguments, implicitReceiver, applicableMethods, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
