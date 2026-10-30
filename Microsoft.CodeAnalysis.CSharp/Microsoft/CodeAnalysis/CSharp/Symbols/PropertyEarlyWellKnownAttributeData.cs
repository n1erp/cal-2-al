namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class PropertyEarlyWellKnownAttributeData : CommonPropertyEarlyWellKnownAttributeData
{
	private string _indexerName;

	public string IndexerName
	{
		get
		{
			return _indexerName;
		}
		set
		{
			if (_indexerName == null)
			{
				_indexerName = value;
			}
		}
	}
}
