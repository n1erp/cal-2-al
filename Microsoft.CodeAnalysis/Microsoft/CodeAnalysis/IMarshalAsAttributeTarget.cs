namespace Microsoft.CodeAnalysis;

internal interface IMarshalAsAttributeTarget
{
	MarshalPseudoCustomAttributeData GetOrCreateData();
}
