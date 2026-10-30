using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct LoweredDynamicOperation(SyntheticBoundNodeFactory factory, BoundExpression siteInitialization, BoundExpression siteInvocation, TypeSymbol resultType, ImmutableArray<LocalSymbol> temps)
{
	private readonly SyntheticBoundNodeFactory _factory = factory;

	private readonly TypeSymbol _resultType = resultType;

	private readonly ImmutableArray<LocalSymbol> _temps = temps;

	public readonly BoundExpression SiteInitialization = siteInitialization;

	public readonly BoundExpression SiteInvocation = siteInvocation;

	public static LoweredDynamicOperation Bad(BoundExpression loweredReceiver, ImmutableArray<BoundExpression> loweredArguments, BoundExpression loweredRight, TypeSymbol resultType)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		instance.AddOptional(loweredReceiver);
		instance.AddRange(loweredArguments);
		instance.AddOptional(loweredRight);
		return Bad(resultType, instance.ToImmutableAndFree());
	}

	public static LoweredDynamicOperation Bad(TypeSymbol resultType, ImmutableArray<BoundExpression> children)
	{
		BoundBadExpression siteInvocation = new BoundBadExpression(children[0].Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, children, resultType);
		return new LoweredDynamicOperation(null, null, siteInvocation, resultType, default);
	}

	public BoundExpression ToExpression()
	{
		if (_factory == null)
		{
			return SiteInvocation;
		}
		if (_temps.IsDefaultOrEmpty)
		{
			return _factory.Sequence(new BoundExpression[1] { SiteInitialization }, SiteInvocation, _resultType);
		}
		return new BoundSequence(_factory.Syntax, _temps, ImmutableArray.Create(SiteInitialization), SiteInvocation, _resultType)
		{
			WasCompilerGenerated = true
		};
	}
}
