using System.Reflection;

namespace Microsoft.DiaSymReader;

internal interface ISymWriterMetadataProvider
{
	bool TryGetTypeDefinitionInfo(int typeDefinitionToken, out string namespaceName, out string typeName, out TypeAttributes attributes);

	bool TryGetEnclosingType(int nestedTypeToken, out int enclosingTypeToken);

	bool TryGetMethodInfo(int methodDefinitionToken, out string methodName, out int declaringTypeToken);
}
