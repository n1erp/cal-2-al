using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal class CommonMethodEarlyWellKnownAttributeData : EarlyWellKnownAttributeData
{
	private ImmutableArray<string> _lazyConditionalSymbols = ImmutableArray<string>.Empty;

	private ObsoleteAttributeData _obsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

	public ImmutableArray<string> ConditionalSymbols => _lazyConditionalSymbols;

	public ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!_obsoleteAttributeData.IsUninitialized)
			{
				return _obsoleteAttributeData;
			}
			return null;
		}
		set
		{
			_obsoleteAttributeData = value;
		}
	}

	public void AddConditionalSymbol(string name)
	{
		_lazyConditionalSymbols = _lazyConditionalSymbols.Add(name);
	}
}
