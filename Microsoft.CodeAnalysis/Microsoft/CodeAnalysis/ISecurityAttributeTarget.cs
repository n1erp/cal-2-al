namespace Microsoft.CodeAnalysis;

internal interface ISecurityAttributeTarget
{
	SecurityWellKnownAttributeData GetOrCreateData();
}
