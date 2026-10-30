namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class SuppressionInfo
{
	public string Id { get; }

	public AttributeData Attribute { get; }

	internal SuppressionInfo(string id, AttributeData attribute)
	{
		Id = id;
		Attribute = attribute;
	}
}
