using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal abstract class MetadataVisitor
{
	public readonly EmitContext Context;

	public MetadataVisitor(EmitContext context)
	{
		Context = context;
	}

	public virtual void Visit(IArrayTypeReference arrayTypeReference)
	{
		Visit(arrayTypeReference.GetElementType(Context));
	}

	public void Visit(IEnumerable<IAssemblyReference> assemblyReferences)
	{
		foreach (IAssemblyReference assemblyReference in assemblyReferences)
		{
			Visit((IUnitReference)assemblyReference);
		}
	}

	public virtual void Visit(IAssemblyReference assemblyReference)
	{
	}

	public void Visit(IEnumerable<ICustomAttribute> customAttributes)
	{
		foreach (ICustomAttribute customAttribute in customAttributes)
		{
			Visit(customAttribute);
		}
	}

	public virtual void Visit(ICustomAttribute customAttribute)
	{
		IMethodReference methodReference = customAttribute.Constructor(Context, reportDiagnostics: false);
		if (methodReference != null)
		{
			Visit(customAttribute.GetArguments(Context));
			Visit(methodReference);
			Visit(customAttribute.GetNamedArguments(Context));
		}
	}

	public void Visit(ImmutableArray<ICustomModifier> customModifiers)
	{
		foreach (ICustomModifier item in customModifiers)
		{
			Visit(item);
		}
	}

	public virtual void Visit(ICustomModifier customModifier)
	{
		Visit(customModifier.GetModifier(Context));
	}

	public void Visit(IEnumerable<IEventDefinition> events)
	{
		foreach (IEventDefinition @event in events)
		{
			Visit((ITypeDefinitionMember)@event);
		}
	}

	public virtual void Visit(IEventDefinition eventDefinition)
	{
		Visit(eventDefinition.GetAccessors(Context));
		Visit(eventDefinition.GetType(Context));
	}

	public void Visit(IEnumerable<IFieldDefinition> fields)
	{
		foreach (IFieldDefinition field in fields)
		{
			Visit((ITypeDefinitionMember)field);
		}
	}

	public virtual void Visit(IFieldDefinition fieldDefinition)
	{
		MetadataConstant compileTimeValue = fieldDefinition.GetCompileTimeValue(Context);
		IMarshallingInformation marshallingInformation = fieldDefinition.MarshallingInformation;
		if (compileTimeValue != null)
		{
			Visit((IMetadataExpression)compileTimeValue);
		}
		if (marshallingInformation != null)
		{
			Visit(marshallingInformation);
		}
		Visit(fieldDefinition.GetType(Context));
	}

	public virtual void Visit(IFieldReference fieldReference)
	{
		Visit((ITypeMemberReference)fieldReference);
	}

	public void Visit(IEnumerable<IFileReference> fileReferences)
	{
		foreach (IFileReference fileReference in fileReferences)
		{
			Visit(fileReference);
		}
	}

	public virtual void Visit(IFileReference fileReference)
	{
	}

	public virtual void Visit(IGenericMethodInstanceReference genericMethodInstanceReference)
	{
	}

	public void Visit(IEnumerable<IGenericMethodParameter> genericParameters)
	{
		foreach (IGenericMethodParameter genericParameter in genericParameters)
		{
			Visit((IGenericParameter)genericParameter);
		}
	}

	public virtual void Visit(IGenericMethodParameter genericMethodParameter)
	{
	}

	public virtual void Visit(IGenericMethodParameterReference genericMethodParameterReference)
	{
	}

	public virtual void Visit(IGenericParameter genericParameter)
	{
		Visit(genericParameter.GetAttributes(Context));
		Visit(genericParameter.GetConstraints(Context));
		genericParameter.Dispatch(this);
	}

	public abstract void Visit(IGenericTypeInstanceReference genericTypeInstanceReference);

	public void Visit(IEnumerable<IGenericParameter> genericParameters)
	{
		foreach (IGenericTypeParameter genericParameter in genericParameters)
		{
			Visit((IGenericParameter)genericParameter);
		}
	}

	public virtual void Visit(IGenericTypeParameter genericTypeParameter)
	{
	}

	public virtual void Visit(IGenericTypeParameterReference genericTypeParameterReference)
	{
	}

	public virtual void Visit(IGlobalFieldDefinition globalFieldDefinition)
	{
		Visit((IFieldDefinition)globalFieldDefinition);
	}

	public virtual void Visit(IGlobalMethodDefinition globalMethodDefinition)
	{
		Visit((IMethodDefinition)globalMethodDefinition);
	}

	public void Visit(ImmutableArray<ILocalDefinition> localDefinitions)
	{
		foreach (ILocalDefinition item in localDefinitions)
		{
			Visit(item);
		}
	}

	public virtual void Visit(ILocalDefinition localDefinition)
	{
		Visit(localDefinition.CustomModifiers);
		Visit(localDefinition.Type);
	}

	public virtual void Visit(IMarshallingInformation marshallingInformation)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public virtual void Visit(MetadataConstant constant)
	{
	}

	public virtual void Visit(MetadataCreateArray createArray)
	{
		Visit(createArray.ElementType);
		Visit(createArray.Elements);
	}

	public void Visit(IEnumerable<IMetadataExpression> expressions)
	{
		foreach (IMetadataExpression expression in expressions)
		{
			Visit(expression);
		}
	}

	public virtual void Visit(IMetadataExpression expression)
	{
		Visit(expression.Type);
		expression.Dispatch(this);
	}

	public void Visit(IEnumerable<IMetadataNamedArgument> namedArguments)
	{
		foreach (IMetadataNamedArgument namedArgument in namedArguments)
		{
			Visit((IMetadataExpression)namedArgument);
		}
	}

	public virtual void Visit(IMetadataNamedArgument namedArgument)
	{
		Visit(namedArgument.ArgumentValue);
	}

	public virtual void Visit(MetadataTypeOf typeOf)
	{
		if (typeOf.TypeToGet != null)
		{
			Visit(typeOf.TypeToGet);
		}
	}

	public virtual void Visit(IMethodBody methodBody)
	{
		foreach (LocalScope localScope in methodBody.LocalScopes)
		{
			Visit(localScope.Constants);
		}
		Visit(methodBody.LocalVariables);
		Visit(methodBody.ExceptionRegions);
	}

	public void Visit(IEnumerable<IMethodDefinition> methods)
	{
		foreach (IMethodDefinition method in methods)
		{
			Visit((ITypeDefinitionMember)method);
		}
	}

	public virtual void Visit(IMethodDefinition method)
	{
		Visit(method.GetReturnValueAttributes(Context));
		Visit(method.RefCustomModifiers);
		Visit(method.ReturnValueCustomModifiers);
		if (method.HasDeclarativeSecurity)
		{
			Visit(method.SecurityAttributes);
		}
		if (method.IsGeneric)
		{
			Visit(method.GenericParameters);
		}
		Visit(method.GetType(Context));
		Visit(method.Parameters);
		if (method.IsPlatformInvoke)
		{
			Visit(method.PlatformInvokeData);
		}
	}

	public void Visit(IEnumerable<MethodImplementation> methodImplementations)
	{
		foreach (MethodImplementation methodImplementation in methodImplementations)
		{
			Visit(methodImplementation);
		}
	}

	public virtual void Visit(MethodImplementation methodImplementation)
	{
		Visit(methodImplementation.ImplementedMethod);
		Visit(methodImplementation.ImplementingMethod);
	}

	public void Visit(IEnumerable<IMethodReference> methodReferences)
	{
		foreach (IMethodReference methodReference in methodReferences)
		{
			Visit(methodReference);
		}
	}

	public virtual void Visit(IMethodReference methodReference)
	{
		IGenericMethodInstanceReference asGenericMethodInstanceReference = methodReference.AsGenericMethodInstanceReference;
		if (asGenericMethodInstanceReference != null)
		{
			Visit(asGenericMethodInstanceReference);
		}
		else
		{
			Visit((ITypeMemberReference)methodReference);
		}
	}

	public virtual void Visit(IModifiedTypeReference modifiedTypeReference)
	{
		Visit(modifiedTypeReference.CustomModifiers);
		Visit(modifiedTypeReference.UnmodifiedType);
	}

	public abstract void Visit(CommonPEModuleBuilder module);

	public void Visit(IEnumerable<IModuleReference> moduleReferences)
	{
		foreach (IModuleReference moduleReference in moduleReferences)
		{
			Visit((IUnitReference)moduleReference);
		}
	}

	public virtual void Visit(IModuleReference moduleReference)
	{
	}

	public void Visit(IEnumerable<INamedTypeDefinition> types)
	{
		foreach (INamedTypeDefinition type in types)
		{
			Visit(type);
		}
	}

	public virtual void Visit(INamespaceTypeDefinition namespaceTypeDefinition)
	{
	}

	public virtual void Visit(INamespaceTypeReference namespaceTypeReference)
	{
	}

	public void VisitNestedTypes(IEnumerable<INamedTypeDefinition> nestedTypes)
	{
		foreach (ITypeDefinitionMember nestedType in nestedTypes)
		{
			Visit(nestedType);
		}
	}

	public virtual void Visit(INestedTypeDefinition nestedTypeDefinition)
	{
	}

	public virtual void Visit(INestedTypeReference nestedTypeReference)
	{
		Visit(nestedTypeReference.GetContainingType(Context));
	}

	public void Visit(ImmutableArray<ExceptionHandlerRegion> exceptionRegions)
	{
		foreach (ExceptionHandlerRegion item in exceptionRegions)
		{
			Visit(item);
		}
	}

	public virtual void Visit(ExceptionHandlerRegion exceptionRegion)
	{
		ITypeReference exceptionType = exceptionRegion.ExceptionType;
		if (exceptionType != null)
		{
			Visit(exceptionType);
		}
	}

	public void Visit(ImmutableArray<IParameterDefinition> parameters)
	{
		foreach (IParameterDefinition item in parameters)
		{
			Visit(item);
		}
	}

	public virtual void Visit(IParameterDefinition parameterDefinition)
	{
		IMarshallingInformation marshallingInformation = parameterDefinition.MarshallingInformation;
		Visit(parameterDefinition.GetAttributes(Context));
		Visit(parameterDefinition.RefCustomModifiers);
		Visit(parameterDefinition.CustomModifiers);
		MetadataConstant defaultValue = parameterDefinition.GetDefaultValue(Context);
		if (defaultValue != null)
		{
			Visit((IMetadataExpression)defaultValue);
		}
		if (marshallingInformation != null)
		{
			Visit(marshallingInformation);
		}
		Visit(parameterDefinition.GetType(Context));
	}

	public void Visit(ImmutableArray<IParameterTypeInformation> parameterTypeInformations)
	{
		foreach (IParameterTypeInformation item in parameterTypeInformations)
		{
			Visit(item);
		}
	}

	public virtual void Visit(IParameterTypeInformation parameterTypeInformation)
	{
		Visit(parameterTypeInformation.RefCustomModifiers);
		Visit(parameterTypeInformation.CustomModifiers);
		Visit(parameterTypeInformation.GetType(Context));
	}

	public virtual void Visit(IPlatformInvokeInformation platformInvokeInformation)
	{
	}

	public virtual void Visit(IPointerTypeReference pointerTypeReference)
	{
		Visit(pointerTypeReference.GetTargetType(Context));
	}

	public void Visit(IEnumerable<IPropertyDefinition> properties)
	{
		foreach (IPropertyDefinition property in properties)
		{
			Visit((ITypeDefinitionMember)property);
		}
	}

	public virtual void Visit(IPropertyDefinition propertyDefinition)
	{
		Visit(propertyDefinition.GetAccessors(Context));
		Visit(propertyDefinition.Parameters);
	}

	public void Visit(IEnumerable<ManagedResource> resources)
	{
		foreach (ManagedResource resource in resources)
		{
			Visit(resource);
		}
	}

	public virtual void Visit(ManagedResource resource)
	{
	}

	public virtual void Visit(SecurityAttribute securityAttribute)
	{
		Visit(securityAttribute.Attribute);
	}

	public void Visit(IEnumerable<SecurityAttribute> securityAttributes)
	{
		foreach (SecurityAttribute securityAttribute in securityAttributes)
		{
			Visit(securityAttribute);
		}
	}

	public void Visit(IEnumerable<ITypeDefinitionMember> typeMembers)
	{
		foreach (ITypeDefinitionMember typeMember in typeMembers)
		{
			Visit(typeMember);
		}
	}

	public void Visit(IEnumerable<ITypeDefinition> types)
	{
		foreach (ITypeDefinition type in types)
		{
			Visit(type);
		}
	}

	public abstract void Visit(ITypeDefinition typeDefinition);

	public virtual void Visit(ITypeDefinitionMember typeMember)
	{
		ITypeDefinition typeDefinition = typeMember as INestedTypeDefinition;
		if (typeDefinition != null)
		{
			Visit(typeDefinition);
			return;
		}
		Visit(typeMember.GetAttributes(Context));
		typeMember.Dispatch(this);
	}

	public virtual void Visit(ITypeMemberReference typeMemberReference)
	{
		if (typeMemberReference.AsDefinition(Context) == null)
		{
			Visit(typeMemberReference.GetAttributes(Context));
		}
	}

	public void Visit(IEnumerable<ITypeReference> typeReferences)
	{
		foreach (ITypeReference typeReference in typeReferences)
		{
			Visit(typeReference);
		}
	}

	public void Visit(IEnumerable<TypeReferenceWithAttributes> typeRefsWithAttributes)
	{
		foreach (TypeReferenceWithAttributes typeRefsWithAttribute in typeRefsWithAttributes)
		{
			Visit(typeRefsWithAttribute.TypeRef);
			Visit(typeRefsWithAttribute.Attributes);
		}
	}

	public virtual void Visit(ITypeReference typeReference)
	{
		DispatchAsReference(typeReference);
	}

	protected void DispatchAsReference(ITypeReference typeReference)
	{
		INamespaceTypeReference asNamespaceTypeReference = typeReference.AsNamespaceTypeReference;
		if (asNamespaceTypeReference != null)
		{
			Visit(asNamespaceTypeReference);
			return;
		}
		IGenericTypeInstanceReference asGenericTypeInstanceReference = typeReference.AsGenericTypeInstanceReference;
		if (asGenericTypeInstanceReference != null)
		{
			Visit(asGenericTypeInstanceReference);
			return;
		}
		INestedTypeReference asNestedTypeReference = typeReference.AsNestedTypeReference;
		if (asNestedTypeReference != null)
		{
			Visit(asNestedTypeReference);
			return;
		}
		if (typeReference is IArrayTypeReference arrayTypeReference)
		{
			Visit(arrayTypeReference);
			return;
		}
		IGenericTypeParameterReference asGenericTypeParameterReference = typeReference.AsGenericTypeParameterReference;
		if (asGenericTypeParameterReference != null)
		{
			Visit(asGenericTypeParameterReference);
			return;
		}
		IGenericMethodParameterReference asGenericMethodParameterReference = typeReference.AsGenericMethodParameterReference;
		if (asGenericMethodParameterReference != null)
		{
			Visit(asGenericMethodParameterReference);
		}
		else if (typeReference is IPointerTypeReference pointerTypeReference)
		{
			Visit(pointerTypeReference);
		}
		else if (typeReference is IModifiedTypeReference modifiedTypeReference)
		{
			Visit(modifiedTypeReference);
		}
	}

	public void Visit(IEnumerable<IUnitReference> unitReferences)
	{
		foreach (IUnitReference unitReference in unitReferences)
		{
			Visit(unitReference);
		}
	}

	public virtual void Visit(IUnitReference unitReference)
	{
		DispatchAsReference(unitReference);
	}

	private void DispatchAsReference(IUnitReference unitReference)
	{
		if (unitReference is IAssemblyReference assemblyReference)
		{
			Visit(assemblyReference);
		}
		else if (unitReference is IModuleReference moduleReference)
		{
			Visit(moduleReference);
		}
	}

	public virtual void Visit(IWin32Resource win32Resource)
	{
	}
}
