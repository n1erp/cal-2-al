using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class AttributeRequiredMethodSignature
{
	private const string Method = "Method";

	internal string AttributeName { get; }

	internal bool RequirePublic { get; }

	internal bool IsRequiredSignature { get; }

	internal string RequiredSignature { get; }

	internal AttributeParameter RequiredReturnValue { get; }

	internal ImmutableArray<AttributeParameter> RequiredParameters { get; }

	internal int MinRequiredSignatureParameterCount { get; }

	internal int MaxRequiredSignatureParameterCount { get; }

	internal VersionCompatibility Compatibility { get; }

	internal AttributeRequiredMethodSignature(string attributeName, string requiredSignature, bool? requirePublic, AttributeParameter returnValue, AttributeParameter[] requiredParameters, VersionCompatibility compatibility = null)
	{
		RequiredSignature = requiredSignature;
		Compatibility = compatibility;
		RequiredReturnValue = returnValue;
		RequiredParameters = requiredParameters.AsImmutableOrNull();
		RequirePublic = requirePublic ?? true;
		IsRequiredSignature = requiredParameters != null || returnValue != null || requirePublic.HasValue;
		if (RequiredParameters != null)
		{
			MaxRequiredSignatureParameterCount = RequiredParameters.Length;
			MinRequiredSignatureParameterCount = AttributeTypeInfo.ComputeMinimumParameterCount(RequiredParameters);
		}
	}

	internal string GetRequiredMethodSignature(string methodName = null)
	{
		if (!string.IsNullOrEmpty(methodName))
		{
			return methodName + RequiredSignature;
		}
		return AttributeName + "Method" + RequiredSignature;
	}
}
