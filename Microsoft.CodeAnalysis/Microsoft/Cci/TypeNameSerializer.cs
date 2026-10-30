using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.Cci;

internal static class TypeNameSerializer
{
	internal static string GetSerializedTypeName(this ITypeReference typeReference, EmitContext context)
	{
		bool isAssemblyQualified = true;
		return typeReference.GetSerializedTypeName(context, ref isAssemblyQualified);
	}

	internal static string GetSerializedTypeName(this ITypeReference typeReference, EmitContext context, ref bool isAssemblyQualified)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		if (typeReference is IArrayTypeReference arrayTypeReference)
		{
			typeReference = arrayTypeReference.GetElementType(context);
			bool isAssemQualified = false;
			AppendSerializedTypeName(builder, typeReference, ref isAssemQualified, context);
			if (arrayTypeReference.IsSZArray)
			{
				builder.Append("[]");
			}
			else
			{
				builder.Append('[');
				if (arrayTypeReference.Rank == 1)
				{
					builder.Append('*');
				}
				builder.Append(',', arrayTypeReference.Rank - 1);
				builder.Append(']');
			}
		}
		else if (typeReference is IPointerTypeReference pointerTypeReference)
		{
			typeReference = pointerTypeReference.GetTargetType(context);
			bool isAssemQualified2 = false;
			AppendSerializedTypeName(builder, typeReference, ref isAssemQualified2, context);
			builder.Append('*');
		}
		else
		{
			INamespaceTypeReference asNamespaceTypeReference = typeReference.AsNamespaceTypeReference;
			if (asNamespaceTypeReference != null)
			{
				string namespaceName = asNamespaceTypeReference.NamespaceName;
				if (namespaceName.Length != 0)
				{
					builder.Append(namespaceName);
					builder.Append('.');
				}
				builder.Append(GetMangledAndEscapedName(asNamespaceTypeReference));
			}
			else if (typeReference.IsTypeSpecification())
			{
				ITypeReference uninstantiatedGenericType = typeReference.GetUninstantiatedGenericType(context);
				ArrayBuilder<ITypeReference> instance2 = ArrayBuilder<ITypeReference>.GetInstance();
				typeReference.GetConsolidatedTypeArguments(instance2, context);
				bool isAssemblyQualified2 = false;
				builder.Append(uninstantiatedGenericType.GetSerializedTypeName(context, ref isAssemblyQualified2));
				builder.Append('[');
				bool flag = true;
				foreach (ITypeReference item in instance2)
				{
					if (flag)
					{
						flag = false;
					}
					else
					{
						builder.Append(',');
					}
					bool isAssemQualified3 = true;
					AppendSerializedTypeName(builder, item, ref isAssemQualified3, context);
				}
				instance2.Free();
				builder.Append(']');
			}
			else
			{
				INestedTypeReference asNestedTypeReference = typeReference.AsNestedTypeReference;
				if (asNestedTypeReference != null)
				{
					bool isAssemblyQualified3 = false;
					builder.Append(asNestedTypeReference.GetContainingType(context).GetSerializedTypeName(context, ref isAssemblyQualified3));
					builder.Append('+');
					builder.Append(GetMangledAndEscapedName(asNestedTypeReference));
				}
			}
		}
		if (isAssemblyQualified)
		{
			AppendAssemblyQualifierIfNecessary(builder, UnwrapTypeReference(typeReference, context), out isAssemblyQualified, context);
		}
		return instance.ToStringAndFree();
	}

	private static void AppendSerializedTypeName(StringBuilder sb, ITypeReference type, ref bool isAssemQualified, EmitContext context)
	{
		string serializedTypeName = type.GetSerializedTypeName(context, ref isAssemQualified);
		if (isAssemQualified)
		{
			sb.Append('[');
		}
		sb.Append(serializedTypeName);
		if (isAssemQualified)
		{
			sb.Append(']');
		}
	}

	private static void AppendAssemblyQualifierIfNecessary(StringBuilder sb, ITypeReference typeReference, out bool isAssemQualified, EmitContext context)
	{
		INestedTypeReference asNestedTypeReference = typeReference.AsNestedTypeReference;
		if (asNestedTypeReference != null)
		{
			AppendAssemblyQualifierIfNecessary(sb, asNestedTypeReference.GetContainingType(context), out isAssemQualified, context);
			return;
		}
		IGenericTypeInstanceReference asGenericTypeInstanceReference = typeReference.AsGenericTypeInstanceReference;
		if (asGenericTypeInstanceReference != null)
		{
			AppendAssemblyQualifierIfNecessary(sb, asGenericTypeInstanceReference.GetGenericType(context), out isAssemQualified, context);
			return;
		}
		if (typeReference is IArrayTypeReference arrayTypeReference)
		{
			AppendAssemblyQualifierIfNecessary(sb, arrayTypeReference.GetElementType(context), out isAssemQualified, context);
			return;
		}
		if (typeReference is IPointerTypeReference pointerTypeReference)
		{
			AppendAssemblyQualifierIfNecessary(sb, pointerTypeReference.GetTargetType(context), out isAssemQualified, context);
			return;
		}
		isAssemQualified = false;
		IAssemblyReference assemblyReference = null;
		INamespaceTypeReference asNamespaceTypeReference = typeReference.AsNamespaceTypeReference;
		if (asNamespaceTypeReference != null)
		{
			assemblyReference = asNamespaceTypeReference.GetUnit(context) as IAssemblyReference;
		}
		if (assemblyReference != null)
		{
			IAssemblyReference containingAssembly = context.Module.GetContainingAssembly(context);
			if (containingAssembly == null || assemblyReference != containingAssembly)
			{
				sb.Append(", ");
				sb.Append(MetadataWriter.StrongName(assemblyReference));
				isAssemQualified = true;
			}
		}
	}

	private static string GetMangledAndEscapedName(INamedTypeReference namedType)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		string name = namedType.Name;
		foreach (char value in name)
		{
			if ("\\[]*.+,& ".IndexOf(value) >= 0)
			{
				builder.Append('\\');
			}
			builder.Append(value);
		}
		if (namedType.MangleName && namedType.GenericParameterCount > 0)
		{
			builder.Append(MetadataHelpers.GetAritySuffix(namedType.GenericParameterCount));
		}
		return instance.ToStringAndFree();
	}

	private static ITypeReference UnwrapTypeReference(ITypeReference typeReference, EmitContext context)
	{
		while (true)
		{
			if (typeReference is IArrayTypeReference arrayTypeReference)
			{
				typeReference = arrayTypeReference.GetElementType(context);
				continue;
			}
			if (!(typeReference is IPointerTypeReference pointerTypeReference))
			{
				break;
			}
			typeReference = pointerTypeReference.GetTargetType(context);
		}
		return typeReference;
	}

	internal static string BuildQualifiedNamespaceName(INamespace @namespace)
	{
		if (@namespace.ContainingNamespace == null)
		{
			return @namespace.Name;
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		do
		{
			string name = @namespace.Name;
			if (name.Length != 0)
			{
				instance.Add(name);
			}
			@namespace = @namespace.ContainingNamespace;
		}
		while (@namespace != null);
		PooledStringBuilder instance2 = PooledStringBuilder.GetInstance();
		for (int num = instance.Count - 1; num >= 0; num--)
		{
			instance2.Builder.Append(instance[num]);
			if (num > 0)
			{
				instance2.Builder.Append('.');
			}
		}
		instance.Free();
		return instance2.ToStringAndFree();
	}
}
