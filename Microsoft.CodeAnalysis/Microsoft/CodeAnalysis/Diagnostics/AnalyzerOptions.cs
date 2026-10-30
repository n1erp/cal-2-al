using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

public class AnalyzerOptions
{
	internal static readonly AnalyzerOptions Empty = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);

	public ImmutableArray<AdditionalText> AdditionalFiles { get; }

	public AnalyzerOptions(ImmutableArray<AdditionalText> additionalFiles)
	{
		AdditionalFiles = additionalFiles.NullToEmpty();
	}

	public AnalyzerOptions WithAdditionalFiles(ImmutableArray<AdditionalText> additionalFiles)
	{
		if (AdditionalFiles == additionalFiles)
		{
			return this;
		}
		return new AnalyzerOptions(additionalFiles);
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is AnalyzerOptions analyzerOptions)
		{
			if (!(AdditionalFiles == analyzerOptions.AdditionalFiles))
			{
				return AdditionalFiles.SequenceEqual(analyzerOptions.AdditionalFiles, object.ReferenceEquals);
			}
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.CombineValues(AdditionalFiles);
	}
}
