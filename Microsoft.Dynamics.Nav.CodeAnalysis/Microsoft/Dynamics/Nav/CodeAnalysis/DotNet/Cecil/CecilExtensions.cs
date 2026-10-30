using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal static class CecilExtensions
{
	private static readonly string NullableTypeName = typeof(Nullable<>).FullName;

	internal static bool IsSubclassOf(this TypeDefinition childTypeDef, TypeDefinition parentTypeDef)
	{
		if (childTypeDef.MetadataToken != parentTypeDef.MetadataToken)
		{
			return childTypeDef.GetBaseClasses().Any((TypeDefinition b) => AreSame(b, parentTypeDef));
		}
		return false;
	}

	internal static bool IsUserVisibleEvent(this EventDefinition e)
	{
		if (e.AddMethod.IsPublic)
		{
			return e.RemoveMethod.IsPublic;
		}
		return false;
	}

	internal static bool IsStatic(this TypeDefinition type)
	{
		if (type.IsAbstract)
		{
			return type.IsSealed;
		}
		return false;
	}

	internal static bool IsReturnValueOptional(this MethodDefinition method)
	{
		if (!method.ReturnType.IsOptionalModifier)
		{
			return method.ReturnType.FullName == typeof(void).FullName;
		}
		return true;
	}

	internal static string GetNamespaceQualifiedTypeName(this TypeReference type)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			StringBuilder builder = instance.Builder;
			GetTypeNameCore(type, builder);
			return builder.ToString();
		}
		finally
		{
			instance.Free();
		}
		static void GetTypeNameCore(TypeReference t, StringBuilder stringBuilder)
		{
			if (t.IsNested)
			{
				GetTypeNameCore(t.DeclaringType, stringBuilder);
				stringBuilder.Append('+');
				stringBuilder.Append(t.Name);
			}
			else
			{
				stringBuilder.Append(t.Namespace + "." + t.Name);
			}
		}
	}

	internal static bool IsOpenGeneric(this TypeReference t)
	{
		if (!t.IsFullyOpenGenericType())
		{
			return t.IsPartiallyOpenGenericType();
		}
		return true;
	}

	internal static bool IsFullyOpenGenericType(this TypeReference t)
	{
		return t.HasGenericParameters;
	}

	internal static bool IsPartiallyOpenGenericType(this TypeReference t)
	{
		if (t.IsGenericInstance)
		{
			return t.ContainsGenericParameter;
		}
		return false;
	}

	internal static bool IsNullableValueType(this TypeReference t)
	{
		if (t.IsGenericInstance)
		{
			return t.FullName.StartsWith(NullableTypeName, StringComparison.Ordinal);
		}
		return false;
	}

	internal static string GetDisplayName(this TypeReference type)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			StringBuilder builder = instance.Builder;
			GetDisplayNameCore(type, builder);
			return builder.ToString();
		}
		finally
		{
			instance.Free();
		}
		static void GetDisplayNameCore(TypeReference t, StringBuilder stringBuilder)
		{
			if (t.IsArray)
			{
				GetDisplayNameCore(((ArrayType)t).ElementType, stringBuilder);
				stringBuilder.Append("[]");
			}
			else if (t.IsByReference)
			{
				GetDisplayNameCore(((ByReferenceType)t).ElementType, stringBuilder);
				stringBuilder.Append('&');
			}
			else if (t.IsPointer)
			{
				GetDisplayNameCore(((PointerType)t).ElementType, stringBuilder);
				stringBuilder.Append('*');
			}
			else if (t.IsNested)
			{
				GetDisplayNameCore(t.DeclaringType, stringBuilder);
				stringBuilder.Append('+');
				stringBuilder.Append(t.Name);
			}
			else if (t.IsGenericInstance)
			{
				int num = t.FullName.IndexOf('`');
				if (num == -1)
				{
					stringBuilder.Append(t.FullName);
				}
				else
				{
					Collection<TypeReference> genericArguments = ((GenericInstanceType)t).GenericArguments;
					stringBuilder.Append(t.FullName, 0, num);
					stringBuilder.Append("<");
					for (int i = 0; i < genericArguments.Count; i++)
					{
						if (i > 0)
						{
							stringBuilder.Append(", ");
						}
						GetDisplayNameCore(genericArguments[i], stringBuilder);
					}
					stringBuilder.Append(">");
				}
			}
			else
			{
				stringBuilder.Append(t.FullName);
			}
		}
	}

	internal static string GetFullName(this TypeReference typeReference)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		GetFullNameWithBuilder(typeReference, instance.Builder);
		return instance.ToStringAndFree();
		static void GetFullNameWithBuilder(TypeReference t, StringBuilder builder)
		{
			if (t.IsArray)
			{
				GetFullNameWithBuilder(((ArrayType)t).ElementType, builder);
				builder.Append("[]");
			}
			else if (t.IsByReference)
			{
				GetFullNameWithBuilder(((ByReferenceType)t).ElementType, builder);
				builder.Append('&');
			}
			else if (t.IsPointer)
			{
				GetFullNameWithBuilder(((PointerType)t).ElementType, builder);
				builder.Append('*');
			}
			else if (t.IsNested)
			{
				GetFullNameWithBuilder(t.DeclaringType, builder);
				builder.Append('+');
				builder.Append(t.Name);
			}
			else if (t.IsGenericInstance)
			{
				GenericInstanceType genericInstanceType = (GenericInstanceType)t;
				GetFullNameWithBuilder(genericInstanceType.ElementType, builder);
				builder.Append("[");
				for (int i = 0; i < genericInstanceType.GenericArguments.Count; i++)
				{
					builder.Append("[");
					GetFullNameWithBuilder(genericInstanceType.GenericArguments[i], builder);
					builder.Append(", ");
					TypeDefinition typeDefinition = genericInstanceType.GenericArguments[i].Resolve();
					if (typeDefinition != null)
					{
						builder.Append(typeDefinition.Module.Assembly.FullName);
					}
					else
					{
						builder.Append(genericInstanceType.GenericArguments[i].Module.Assembly.FullName);
					}
					builder.Append("]");
					if (i < genericInstanceType.GenericArguments.Count - 1)
					{
						builder.Append(",");
					}
				}
				builder.Append("]");
			}
			else
			{
				builder.Append(t.FullName);
			}
		}
	}

	internal static bool DoesTypeImplementInterface(this TypeDefinition childType, TypeDefinition parentInterfaceDef)
	{
		DebugAssertHelper.Assert(parentInterfaceDef.IsInterface);
		if (!childType.DoesSpecificTypeImplementInterface(parentInterfaceDef))
		{
			return childType.GetBaseClasses().Any((TypeDefinition typeDefinition) => typeDefinition.DoesSpecificTypeImplementInterface(parentInterfaceDef));
		}
		return true;
	}

	internal static bool IsAssignableFrom(this TypeDefinition target, TypeDefinition source)
	{
		if (target != source && !AreSame(target, source) && !source.IsSubclassOf(target))
		{
			if (target.IsInterface)
			{
				return source.DoesTypeImplementInterface(target);
			}
			return false;
		}
		return true;
	}

	internal static IEnumerable<TypeDefinition> GetBaseClasses(this TypeDefinition klassType)
	{
		for (TypeDefinition baseType = klassType.BaseType?.Resolve(); baseType != null; baseType = baseType.BaseType?.Resolve())
		{
			yield return baseType;
		}
	}

	internal static bool IsUserVisibleMethod(this MethodDefinition m)
	{
		if (m.IsPublic && !m.IsConstructor && !m.IsSetter && !m.IsGetter && !m.IsAddOn)
		{
			return !m.IsRemoveOn;
		}
		return false;
	}

	internal static bool IsOverrideMethod(this MethodDefinition m)
	{
		if (m.IsVirtual)
		{
			return !m.IsNewSlot;
		}
		return false;
	}

	internal static bool IsUserVisibleProperty(this PropertyDefinition m)
	{
		MethodDefinition getMethod = m.GetMethod;
		if (getMethod == null || !getMethod.IsPublic)
		{
			return m.SetMethod?.IsPublic ?? false;
		}
		return true;
	}

	internal static bool IsOverrideProperty(this PropertyDefinition p)
	{
		MethodDefinition getMethod = p.GetMethod;
		if (getMethod == null || !getMethod.IsOverrideMethod())
		{
			return p.SetMethod?.IsOverrideMethod() ?? false;
		}
		return true;
	}

	internal static MethodReference MakeGenericMethod(this MethodReference self, params TypeReference[] typeArguments)
	{
		if (!self.HasGenericParameters)
		{
			return self;
		}
		return ReplaceMethodTypeParameters(self, self.DeclaringType, typeArguments, substituteOnlyTypeParametersFromContainingType: false);
	}

	internal static MethodReference ReplaceTypeParametersWithContainingTypeArguments(this MethodReference self, TypeReference closedContainingType)
	{
		TypeReference typeReference = closedContainingType.ClearTypeModifiers();
		DebugAssertHelper.Assert(typeReference.IsGenericInstance);
		if (!self.ContainsGenericParameter && !self.HasGenericParameters && !self.IsGenericInstance)
		{
			return self;
		}
		Collection<TypeReference> genericArguments = ((GenericInstanceType)typeReference).GenericArguments;
		MethodReference methodReference = ReplaceMethodTypeParameters(self, typeReference, genericArguments, substituteOnlyTypeParametersFromContainingType: true);
		foreach (GenericParameter genericParameter in self.GenericParameters)
		{
			methodReference.GenericParameters.Add(genericParameter);
		}
		return methodReference;
	}

	private static MethodReference ReplaceMethodTypeParameters(MethodReference self, TypeReference closedContainingType, IList<TypeReference> typeArguments, bool substituteOnlyTypeParametersFromContainingType)
	{
		TypeReference parameterType = GetParameterType(self.ReturnType, typeArguments, substituteOnlyTypeParametersFromContainingType);
		MethodReference methodReference = new MethodReference(self.Name, parameterType, closedContainingType)
		{
			HasThis = self.HasThis,
			ExplicitThis = self.ExplicitThis,
			CallingConvention = self.CallingConvention
		};
		foreach (ParameterDefinition parameter in self.Parameters)
		{
			TypeReference parameterType2 = GetParameterType(parameter.ParameterType, typeArguments, substituteOnlyTypeParametersFromContainingType);
			methodReference.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, parameterType2));
		}
		return methodReference;
	}

	internal static ProcessorArchitecture? GetProcessorArchitecture(this ModuleDefinition module)
	{
		DebugAssertHelper.Assert(module != null);
		switch (module.Architecture)
		{
		case TargetArchitecture.I386:
			if (module.Attributes.HasFlag(ModuleAttributes.ILOnly | ModuleAttributes.Required32Bit))
			{
				return ProcessorArchitecture.X86;
			}
			return ProcessorArchitecture.MSIL;
		case TargetArchitecture.AMD64:
			return ProcessorArchitecture.Amd64;
		case TargetArchitecture.IA64:
			return ProcessorArchitecture.IA64;
		case TargetArchitecture.ARM:
		case TargetArchitecture.ARMv7:
		case TargetArchitecture.ARM64:
			return ProcessorArchitecture.Arm;
		default:
			return null;
		}
	}

	internal static TypeReference CloneTypeModifiersFrom(this TypeReference elementType, TypeReference modifiedType)
	{
		if (modifiedType.IsByReference)
		{
			ByReferenceType byReferenceType = (ByReferenceType)modifiedType;
			return elementType.CloneTypeModifiersFrom(byReferenceType.ElementType).MakeByReferenceType();
		}
		if (modifiedType.IsArray)
		{
			ArrayType arrayType = (ArrayType)modifiedType;
			return elementType.CloneTypeModifiersFrom(arrayType.ElementType).MakeArrayType(arrayType.Rank);
		}
		if (modifiedType.IsOptionalModifier)
		{
			OptionalModifierType optionalModifierType = (OptionalModifierType)modifiedType;
			return elementType.CloneTypeModifiersFrom(optionalModifierType.ElementType).MakeOptionalModifierType(optionalModifierType.ModifierType);
		}
		if (modifiedType.IsPinned)
		{
			PinnedType pinnedType = (PinnedType)modifiedType;
			return elementType.CloneTypeModifiersFrom(pinnedType.ElementType).MakePinnedType();
		}
		if (modifiedType.IsSentinel)
		{
			SentinelType sentinelType = (SentinelType)modifiedType;
			return elementType.CloneTypeModifiersFrom(sentinelType.ElementType).MakeSentinelType();
		}
		if (modifiedType.IsPointer)
		{
			PointerType pointerType = (PointerType)modifiedType;
			return elementType.CloneTypeModifiersFrom(pointerType.ElementType).MakePointerType();
		}
		return elementType;
	}

	internal static ArrayType MakeArrayType(this TypeReference self, int rank)
	{
		DebugAssertHelper.Assert(rank > 0, "Rank cannot be null.");
		ArrayType arrayType = new ArrayType(self);
		for (int i = 1; i < rank; i++)
		{
			arrayType.Dimensions.Add(default);
		}
		return arrayType;
	}

	internal static PointerType MakePointerType(this TypeReference self)
	{
		return new PointerType(self);
	}

	internal static OptionalModifierType MakeOptionalModifierType(this TypeReference self, TypeReference modifierType)
	{
		return new OptionalModifierType(modifierType, self);
	}

	internal static PinnedType MakePinnedType(this TypeReference self)
	{
		return new PinnedType(self);
	}

	internal static SentinelType MakeSentinelType(this TypeReference self)
	{
		return new SentinelType(self);
	}

	internal static TypeReference MakeClosedGenericType(this TypeReference self, IList<TypeReference> typeArguments)
	{
		DebugAssertHelper.Assert(self != null, "self");
		DebugAssertHelper.Assert(typeArguments != null, "typeArguments");
		DebugAssertHelper.Assert(typeArguments.Count != 0, "The Arguments array is empty.");
		DebugAssertHelper.Assert(self.HasGenericParameters);
		DebugAssertHelper.Assert(self.GenericParameters.Count == typeArguments.Count);
		GenericInstanceType genericInstanceType = new GenericInstanceType(self.GetElementType());
		foreach (TypeReference typeArgument in typeArguments)
		{
			genericInstanceType.GenericArguments.Add(typeArgument);
		}
		return genericInstanceType;
	}

	internal static bool ContainsUnboundGenericTypeParameters(this TypeReference t)
	{
		if (t == null)
		{
			return false;
		}
		if (t.IsGenericParameter)
		{
			return true;
		}
		if (t.IsGenericInstance)
		{
			foreach (TypeReference genericArgument in ((GenericInstanceType)t).GenericArguments)
			{
				if (genericArgument.ContainsUnboundGenericTypeParameters())
				{
					return true;
				}
			}
		}
		if (t.HasGenericParameters)
		{
			return true;
		}
		return false;
	}

	private static TypeReference MakeClosedGenericTypeFromOpenType(this TypeReference self, IList<TypeReference> typeArguments, bool substituteOnlyTypeParametersFromContainingType)
	{
		DebugAssertHelper.Assert(self.IsFullyOpenGenericType() || self.IsPartiallyOpenGenericType());
		if (self.IsFullyOpenGenericType())
		{
			return self.MakeClosedGenericType(typeArguments);
		}
		return self.MakeClosedGenericTypeFromPartiallyOpenType(typeArguments, substituteOnlyTypeParametersFromContainingType);
	}

	private static TypeReference MakeClosedGenericTypeFromPartiallyOpenType(this TypeReference self, IList<TypeReference> typeArguments, bool substituteOnlyTypeParametersFromContainingType)
	{
		GenericInstanceType genericInstanceType = (GenericInstanceType)self;
		GenericInstanceType genericInstanceType2 = new GenericInstanceType(self.GetElementType());
		for (int i = 0; i < genericInstanceType.GenericArguments.Count; i++)
		{
			TypeReference parameterType = genericInstanceType.GenericArguments[i];
			genericInstanceType2.GenericArguments.Add(GetParameterType(parameterType, typeArguments, substituteOnlyTypeParametersFromContainingType));
		}
		return genericInstanceType2;
	}

	private static TypeReference MakeClosedTypeParameter(this TypeReference self, IList<TypeReference> typeArguments, bool substituteOnlyTypeParametersFromContainingType)
	{
		DebugAssertHelper.Assert(self.IsGenericParameter);
		GenericParameter genericParameter = (GenericParameter)self;
		int position = genericParameter.Position;
		if (!substituteOnlyTypeParametersFromContainingType)
		{
			return typeArguments[position];
		}
		if (genericParameter.DeclaringType == null)
		{
			return self;
		}
		DebugAssertHelper.Assert(position < typeArguments.Count, "Failed to get type information for " + genericParameter.DeclaringType.GetFullName());
		return typeArguments[position];
	}

	internal static TypeReference MakeGenericInnerTypeIfNecessary(this TypeReference innerType, TypeReference outerType)
	{
		return innerType.ReplaceGenericTypeParametersInDependentType(outerType);
	}

	internal static TypeReference MakeGenericFromClosedSubClass(this TypeReference baseType, TypeReference subType)
	{
		return baseType.ReplaceGenericTypeParametersInDependentType(subType);
	}

	private static TypeReference ReplaceGenericTypeParametersInDependentType(this TypeReference dependentType, TypeReference dependeeType)
	{
		DebugAssertHelper.Assert(!dependeeType.IsOpenGeneric(), "The subtype must be closed before trying to do type substitution in the base-type.");
		if (!dependentType.IsOpenGeneric())
		{
			return dependentType;
		}
		Collection<TypeReference> genericArguments = ((GenericInstanceType)dependeeType).GenericArguments;
		return dependentType.MakeClosedGenericTypeFromOpenType(genericArguments, substituteOnlyTypeParametersFromContainingType: false);
	}

	internal static (TypeReference BaseType, TypeReference[] Interfaces) ResolveClosedParents(TypeDefinition type, TypeReference closedTypeReference, TypeDefinition systemArrayType, TypeDefinition genericListTypeDefinition, bool isArray)
	{
		TypeReference[] array = Array.Empty<TypeReference>();
		TypeReference item = null;
		if (type?.BaseType == null)
		{
			array = GetImplementedInterfaces(type, closedTypeReference).ToArray() ?? Array.Empty<TypeReference>();
		}
		else
		{
			if (type.HasInterfaces)
			{
				array = GetImplementedInterfaces(type, closedTypeReference).ToArray();
			}
			if (isArray)
			{
				TypeReference elementType = ((ArrayType)closedTypeReference).ElementType;
				TypeReference typeReference = genericListTypeDefinition.MakeClosedGenericType(new List<TypeReference> { elementType });
				IEnumerable<TypeReference> implementedInterfaces = GetImplementedInterfaces(genericListTypeDefinition, typeReference);
				array = array.Concat(typeReference).Concat(implementedInterfaces).ToArray();
				item = systemArrayType;
			}
			else
			{
				item = type.BaseType.MakeGenericFromClosedSubClass(closedTypeReference);
			}
		}
		return (BaseType: item, Interfaces: array);
	}

	internal static bool IsGetEnumeratorMethod(this DotNetMethodInfo m)
	{
		if (!m.IsStatic && m.Name == "GetEnumerator")
		{
			return m.Parameters.Length == 0;
		}
		return false;
	}

	internal static ByReferenceType MakeByReferenceType(this TypeReference self)
	{
		return new ByReferenceType(self);
	}

	private static IEnumerable<TypeReference> GetImplementedInterfaces(TypeDefinition type, TypeReference closedTypeReference)
	{
		if (type == null || !type.HasInterfaces)
		{
			return Array.Empty<TypeReference>();
		}
		return type.Interfaces.Select((InterfaceImplementation interfaceImpl) => interfaceImpl.InterfaceType.MakeGenericFromClosedSubClass(closedTypeReference));
	}

	private static TypeReference GetParameterType(TypeReference parameterType, IList<TypeReference> typeArguments, bool substituteOnlyTypeParametersFromContainingType)
	{
		TypeReference elementType = parameterType.GetElementType();
		if (elementType.IsGenericParameter)
		{
			return elementType.MakeClosedTypeParameter(typeArguments, substituteOnlyTypeParametersFromContainingType).CloneTypeModifiersFrom(parameterType);
		}
		if (elementType.HasGenericParameters)
		{
			TypeReference typeReference = parameterType.ClearTypeModifiers();
			if (typeReference.IsOpenGeneric())
			{
				return typeReference.MakeClosedGenericTypeFromOpenType(typeArguments, substituteOnlyTypeParametersFromContainingType).CloneTypeModifiersFrom(parameterType);
			}
		}
		return parameterType;
	}

	internal static TypeReference ClearTypeModifiers(this TypeReference type)
	{
		if (type.IsByReference)
		{
			return ((ByReferenceType)type).ElementType.ClearTypeModifiers();
		}
		if (type.IsArray)
		{
			return ((ArrayType)type).ElementType.ClearTypeModifiers();
		}
		if (type.IsOptionalModifier)
		{
			return ((OptionalModifierType)type).ElementType.ClearTypeModifiers();
		}
		if (type.IsPinned)
		{
			return ((PinnedType)type).ElementType.ClearTypeModifiers();
		}
		if (type.IsSentinel)
		{
			return ((SentinelType)type).ElementType.ClearTypeModifiers();
		}
		if (type.IsPointer)
		{
			return ((PointerType)type).ElementType.ClearTypeModifiers();
		}
		return type;
	}

	private static bool AreSame(TypeDefinition left, TypeDefinition right)
	{
		if (left.MetadataToken == right.MetadataToken)
		{
			return left.Module.MetadataToken == right.Module.MetadataToken;
		}
		return false;
	}

	private static bool DoesSpecificTypeImplementInterface(this TypeDefinition childTypeDef, TypeDefinition parentInterfaceDef)
	{
		DebugAssertHelper.Assert(parentInterfaceDef.IsInterface);
		return childTypeDef.Interfaces.Any((InterfaceImplementation ifaceDef) =>
		{
			TypeDefinition typeDefinition = ifaceDef.InterfaceType.Resolve();
			return typeDefinition != null && DoesSpecificInterfaceImplementInterface(typeDefinition, parentInterfaceDef);
		});
	}

	private static bool DoesSpecificInterfaceImplementInterface(TypeDefinition iface0, TypeDefinition iface1)
	{
		DebugAssertHelper.Assert(iface1.IsInterface);
		DebugAssertHelper.Assert(iface0.IsInterface);
		if (!(iface0.MetadataToken == iface1.MetadataToken))
		{
			return iface0.DoesTypeImplementInterface(iface1);
		}
		return true;
	}
}
