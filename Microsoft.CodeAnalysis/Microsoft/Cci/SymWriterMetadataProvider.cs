using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Microsoft.DiaSymReader;

namespace Microsoft.Cci;

internal sealed class SymWriterMetadataProvider : ISymWriterMetadataProvider
{
	private readonly MetadataWriter _writer;

	private int _lastTypeDef;

	private string _lastTypeDefName;

	private string _lastTypeDefNamespace;

	internal SymWriterMetadataProvider(MetadataWriter writer)
	{
		_writer = writer;
	}

	public bool TryGetTypeDefinitionInfo(int typeDefinitionToken, out string namespaceName, out string typeName, out TypeAttributes attributes)
	{
		if (typeDefinitionToken == 0)
		{
			namespaceName = null;
			typeName = null;
			attributes = TypeAttributes.NotPublic;
			return false;
		}
		ITypeDefinition typeDefinition = _writer.GetTypeDefinition(typeDefinitionToken);
		if (_lastTypeDef == typeDefinitionToken)
		{
			typeName = _lastTypeDefName;
			namespaceName = _lastTypeDefNamespace;
		}
		else
		{
			typeName = MetadataWriter.GetMangledName((INamedTypeReference)typeDefinition);
			INamespaceTypeDefinition namespaceTypeDefinition;
			if ((namespaceTypeDefinition = typeDefinition.AsNamespaceTypeDefinition(_writer.Context)) != null)
			{
				namespaceName = namespaceTypeDefinition.NamespaceName;
			}
			else
			{
				namespaceName = null;
			}
			_lastTypeDef = typeDefinitionToken;
			_lastTypeDefName = typeName;
			_lastTypeDefNamespace = namespaceName;
		}
		attributes = _writer.GetTypeAttributes(typeDefinition.GetResolvedType(_writer.Context));
		return true;
	}

	public bool TryGetMethodInfo(int methodDefinitionToken, out string methodName, out int declaringTypeToken)
	{
		IMethodDefinition methodDefinition = _writer.GetMethodDefinition(methodDefinitionToken);
		methodName = methodDefinition.Name;
		declaringTypeToken = MetadataTokens.GetToken(_writer.GetTypeHandle(methodDefinition.GetContainingType(_writer.Context)));
		return true;
	}

	public bool TryGetEnclosingType(int nestedTypeToken, out int enclosingTypeToken)
	{
		INestedTypeReference nestedTypeReference = _writer.GetNestedTypeReference(nestedTypeToken);
		if (nestedTypeReference == null)
		{
			enclosingTypeToken = 0;
			return false;
		}
		enclosingTypeToken = MetadataTokens.GetToken(_writer.GetTypeHandle(nestedTypeReference.GetContainingType(_writer.Context)));
		return true;
	}
}
