using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

public struct DeconstructionInfo
{
	private readonly Conversion _conversion;

	public IMethodSymbol Method
	{
		get
		{
			if (_conversion.Kind != ConversionKind.Deconstruction)
			{
				return null;
			}
			return _conversion.Method;
		}
	}

	public Conversion? Conversion
	{
		get
		{
			if (_conversion.Kind != ConversionKind.Deconstruction)
			{
				return _conversion;
			}
			return null;
		}
	}

	public ImmutableArray<DeconstructionInfo> Nested
	{
		get
		{
			ImmutableArray<Conversion> underlyingConversions = _conversion.UnderlyingConversions;
			if (!underlyingConversions.IsDefault)
			{
				return underlyingConversions.SelectAsArray((Conversion c) => new DeconstructionInfo(c));
			}
			return ImmutableArray<DeconstructionInfo>.Empty;
		}
	}

	internal DeconstructionInfo(Conversion conversion)
	{
		_conversion = conversion;
	}
}
