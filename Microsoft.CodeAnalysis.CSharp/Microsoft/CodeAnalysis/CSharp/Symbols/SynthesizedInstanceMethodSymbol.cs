using System.Collections.Immutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedInstanceMethodSymbol : MethodSymbol
{
	private ParameterSymbol _lazyThisParameter;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public sealed override bool IsImplicitlyDeclared => true;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal override bool TryGetThisParameter(out ParameterSymbol thisParameter)
	{
		if ((object)_lazyThisParameter == null)
		{
			Interlocked.CompareExchange(ref _lazyThisParameter, new ThisParameterSymbol(this), null);
		}
		thisParameter = _lazyThisParameter;
		return true;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
