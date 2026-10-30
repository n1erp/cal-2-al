using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.DiaSymReader;

internal sealed class SymWriterMetadataAdapter : MetadataAdapterBase
{
	private readonly ISymWriterMetadataProvider _metadataProvider;

	public SymWriterMetadataAdapter(ISymWriterMetadataProvider metadataProvider)
	{
		_metadataProvider = metadataProvider;
	}

	public unsafe override int GetTokenFromSig(byte* voidPointerSig, int byteCountSig)
	{
		return 285212672;
	}

	public unsafe override int GetTypeDefProps(int typeDef, [Out] char* qualifiedName, int qualifiedNameBufferLength, [Out] int* qualifiedNameLength, [Out] TypeAttributes* attributes, [Out] int* baseType)
	{
		if (!_metadataProvider.TryGetTypeDefinitionInfo(typeDef, out var namespaceName, out var typeName, out var attributes2))
		{
			return -2147024809;
		}
		if (qualifiedNameLength != null || qualifiedName != null)
		{
			InteropUtilities.CopyQualifiedTypeName(qualifiedName, qualifiedNameBufferLength, qualifiedNameLength, namespaceName, typeName);
		}
		if (attributes != null)
		{
			*attributes = attributes2;
		}
		return 0;
	}

	public unsafe override int GetTypeRefProps(int typeRef, [Out] int* resolutionScope, [Out] char* qualifiedName, int qualifiedNameBufferLength, [Out] int* qualifiedNameLength)
	{
		throw new NotImplementedException();
	}

	public override int GetNestedClassProps(int nestedClass, out int enclosingClass)
	{
		if (!_metadataProvider.TryGetEnclosingType(nestedClass, out enclosingClass))
		{
			return -2147467259;
		}
		return 0;
	}

	public unsafe override int GetMethodProps(int methodDef, [Out] int* declaringTypeDef, [Out] char* name, int nameBufferLength, [Out] int* nameLength, [Out] MethodAttributes* attributes, [Out] byte** signature, [Out] int* signatureLength, [Out] int* relativeVirtualAddress, [Out] MethodImplAttributes* implAttributes)
	{
		if (!_metadataProvider.TryGetMethodInfo(methodDef, out var methodName, out var declaringTypeToken))
		{
			return -2147024809;
		}
		if (name != null || nameLength != null)
		{
			int num = Math.Min(methodName.Length, nameBufferLength - 1);
			if (nameLength != null)
			{
				*nameLength = num;
			}
			if (name != null && nameBufferLength > 0)
			{
				char* ptr = name;
				for (int i = 0; i < num; i++)
				{
					*ptr = methodName[i];
					ptr++;
				}
				*ptr = '\0';
			}
		}
		if (declaringTypeDef != null)
		{
			*declaringTypeDef = declaringTypeToken;
		}
		return 0;
	}
}
