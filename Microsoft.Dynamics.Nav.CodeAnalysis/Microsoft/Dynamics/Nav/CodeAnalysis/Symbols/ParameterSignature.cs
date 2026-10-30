using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ParameterSignature
{
	internal static readonly ParameterSignature NoParams = new ParameterSignature(ImmutableArray<TypeSymbol>.Empty, default);

	internal ImmutableArray<TypeSymbol> ParameterTypes { get; }

	internal ImmutableArray<bool> ParameterRefKinds { get; }

	private ParameterSignature(ImmutableArray<TypeSymbol> parameterTypes, ImmutableArray<bool> parameterRefKinds)
	{
		ParameterTypes = parameterTypes;
		ParameterRefKinds = parameterRefKinds;
	}

	private static ParameterSignature MakeParamTypesAndRefKinds(ImmutableArray<ParameterSymbol> parameters)
	{
		if (parameters.Length == 0)
		{
			return NoParams;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<bool> arrayBuilder = null;
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterSymbol parameterSymbol = parameters[i];
			instance.Add(parameterSymbol.Type);
			bool isVar = parameterSymbol.IsVar;
			if (arrayBuilder == null)
			{
				if (isVar)
				{
					arrayBuilder = ArrayBuilder<bool>.GetInstance(i, fillWithValue: false);
					arrayBuilder.Add(isVar);
				}
			}
			else
			{
				arrayBuilder.Add(isVar);
			}
		}
		ImmutableArray<bool> parameterRefKinds = arrayBuilder?.ToImmutableAndFree() ?? default(ImmutableArray<bool>);
		return new ParameterSignature(instance.ToImmutableAndFree(), parameterRefKinds);
	}

	internal static void PopulateParameterSignature(ImmutableArray<ParameterSymbol> parameters, ref ParameterSignature lazySignature)
	{
		if (lazySignature == null)
		{
			Interlocked.CompareExchange(ref lazySignature, MakeParamTypesAndRefKinds(parameters), null);
		}
	}
}
