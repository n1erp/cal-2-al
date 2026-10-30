using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class ParameterSignature
{
	internal readonly ImmutableArray<TypeSymbol> parameterTypes;

	internal readonly ImmutableArray<RefKind> parameterRefKinds;

	internal static readonly ParameterSignature NoParams = new ParameterSignature(ImmutableArray<TypeSymbol>.Empty, default);

	private ParameterSignature(ImmutableArray<TypeSymbol> parameterTypes, ImmutableArray<RefKind> parameterRefKinds)
	{
		this.parameterTypes = parameterTypes;
		this.parameterRefKinds = parameterRefKinds;
	}

	private static ParameterSignature MakeParamTypesAndRefKinds(ImmutableArray<ParameterSymbol> parameters)
	{
		if (parameters.Length == 0)
		{
			return NoParams;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<RefKind> arrayBuilder = null;
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterSymbol parameterSymbol = parameters[i];
			instance.Add(parameterSymbol.Type);
			RefKind refKind = parameterSymbol.RefKind;
			if (arrayBuilder == null)
			{
				if (refKind != RefKind.None)
				{
					arrayBuilder = ArrayBuilder<RefKind>.GetInstance(i, RefKind.None);
					arrayBuilder.Add(refKind);
				}
			}
			else
			{
				arrayBuilder.Add(refKind);
			}
		}
		ImmutableArray<RefKind> immutableArray = arrayBuilder?.ToImmutableAndFree() ?? default(ImmutableArray<RefKind>);
		return new ParameterSignature(instance.ToImmutableAndFree(), immutableArray);
	}

	internal static void PopulateParameterSignature(ImmutableArray<ParameterSymbol> parameters, ref ParameterSignature lazySignature)
	{
		if (lazySignature == null)
		{
			Interlocked.CompareExchange(ref lazySignature, MakeParamTypesAndRefKinds(parameters), null);
		}
	}
}
