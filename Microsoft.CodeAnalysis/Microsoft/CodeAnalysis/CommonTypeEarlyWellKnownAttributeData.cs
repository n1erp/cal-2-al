using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal class CommonTypeEarlyWellKnownAttributeData : EarlyWellKnownAttributeData
{
	private AttributeUsageInfo _attributeUsageInfo = AttributeUsageInfo.Null;

	private bool _hasComImportAttribute;

	private ImmutableArray<string> _lazyConditionalSymbols = ImmutableArray<string>.Empty;

	private ObsoleteAttributeData _obsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

	private bool _hasCodeAnalysisEmbeddedAttribute;

	public AttributeUsageInfo AttributeUsageInfo
	{
		get
		{
			return _attributeUsageInfo;
		}
		set
		{
			_attributeUsageInfo = value;
		}
	}

	public bool HasComImportAttribute
	{
		get
		{
			return _hasComImportAttribute;
		}
		set
		{
			_hasComImportAttribute = value;
		}
	}

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

	public bool HasCodeAnalysisEmbeddedAttribute
	{
		get
		{
			return _hasCodeAnalysisEmbeddedAttribute;
		}
		set
		{
			_hasCodeAnalysisEmbeddedAttribute = value;
		}
	}

	public void AddConditionalSymbol(string name)
	{
		_lazyConditionalSymbols = _lazyConditionalSymbols.Add(name);
	}
}
