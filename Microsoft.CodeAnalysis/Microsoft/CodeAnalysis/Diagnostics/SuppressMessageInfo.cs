namespace Microsoft.CodeAnalysis.Diagnostics;

internal struct SuppressMessageInfo
{
	public string Id;

	public string Scope;

	public string Target;

	public string MessageId;

	public AttributeData Attribute;
}
