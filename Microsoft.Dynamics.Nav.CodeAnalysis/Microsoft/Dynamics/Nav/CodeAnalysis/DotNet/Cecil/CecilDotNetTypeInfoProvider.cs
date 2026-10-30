using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

internal class CecilDotNetTypeInfoProvider : IDotNetTypeInfoProvider
{
	private const string IndexerPropertyName = "Item";

	private readonly TypeDefinition arrayTypeDefinition;

	private readonly TypeDefinition boolTypeDefinition;

	private readonly TypeDefinition dateTimeTypeDefinition;

	private readonly TypeDefinition genericIEnumerableTypeDefinition;

	private readonly TypeDefinition iEnumerableTypeDefinition;

	private readonly TypeDefinition objectTypeDefinition;

	private readonly TypeDefinition streamTypeDefinition;

	private readonly TypeDefinition stringTypeDefinition;

	private readonly TypeDefinition nullableTypeDefinition;

	private readonly TypeDefinition genericIListTypeDefinition;

	private static readonly ImmutableHashSet<string> SupportedByRefEventParameterTypes = new string[18]
	{
		typeof(object).MakeByRefType().FullName,
		typeof(bool).MakeByRefType().FullName,
		typeof(char).MakeByRefType().FullName,
		typeof(byte).MakeByRefType().FullName,
		typeof(sbyte).MakeByRefType().FullName,
		typeof(short).MakeByRefType().FullName,
		typeof(ushort).MakeByRefType().FullName,
		typeof(int).MakeByRefType().FullName,
		typeof(uint).MakeByRefType().FullName,
		typeof(long).MakeByRefType().FullName,
		typeof(ulong).MakeByRefType().FullName,
		typeof(decimal).MakeByRefType().FullName,
		typeof(float).MakeByRefType().FullName,
		typeof(double).MakeByRefType().FullName,
		typeof(string).MakeByRefType().FullName,
		typeof(TimeSpan).MakeByRefType().FullName,
		typeof(DateTime).MakeByRefType().FullName,
		typeof(Guid).MakeByRefType().FullName
	}.ToImmutableHashSet();

	private readonly object getTypeInfoLock = new object();

	private readonly Dictionary<string, DotNetTypeInfo> typeInfoCache = new Dictionary<string, DotNetTypeInfo>();

	private readonly Dictionary<string, CecilTypeInfo> cecilTypeInfos = new Dictionary<string, CecilTypeInfo>();

	private TypeReference MakeGenericTypeIfNeeded(TypeReference type)
	{
		DebugAssertHelper.Assert(!type.IsGenericParameter);
		if (!type.IsFullyOpenGenericType())
		{
			DebugAssertHelper.Assert(!type.IsOpenGeneric());
			return type;
		}
		Collection<GenericParameter> genericParameters = type.GenericParameters;
		if (!CanTypeParametersBeSubstituted(genericParameters))
		{
			return null;
		}
		TypeReference[] typeArguments = CreateGenericTypeArguments(genericParameters);
		return type.MakeClosedGenericType(typeArguments);
	}

	private MethodReference TrySubstituteGenericParameterTypesInMethod(TypeReference closedGenericType, MethodReference methodInfo)
	{
		TypeReference typeReference = closedGenericType.ClearTypeModifiers();
		if (!typeReference.HasGenericParameters && !methodInfo.HasGenericParameters && !methodInfo.ContainsGenericParameter)
		{
			return methodInfo;
		}
		if (methodInfo.ContainsGenericParameter && typeReference.IsGenericInstance)
		{
			methodInfo = methodInfo.ReplaceTypeParametersWithContainingTypeArguments(typeReference);
		}
		if (!methodInfo.HasGenericParameters)
		{
			return methodInfo;
		}
		Collection<GenericParameter> genericParameters = methodInfo.GenericParameters;
		if (!CanTypeParametersBeSubstituted(genericParameters))
		{
			return null;
		}
		TypeReference[] typeArguments = CreateGenericTypeArguments(genericParameters);
		return methodInfo.MakeGenericMethod(typeArguments);
	}

	private MethodReference TrySubstituteGenericParameterTypesInConstructor(CecilTypeInfo ti, MethodReference constructorInfo)
	{
		if (!constructorInfo.ContainsGenericParameter)
		{
			return constructorInfo;
		}
		MethodReference methodReference = constructorInfo.ReplaceTypeParametersWithContainingTypeArguments(ti.ClosedType);
		DebugAssertHelper.Assert(!methodReference.HasGenericParameters);
		return methodReference;
	}

	private TypeReference[] CreateGenericTypeArguments(Collection<GenericParameter> typeParameters)
	{
		TypeReference[] array = new TypeReference[typeParameters.Count];
		for (int i = 0; i < typeParameters.Count; i++)
		{
			GenericParameter genericParameter = typeParameters[i];
			array[i] = ((genericParameter.IsGenericParameter && !genericParameter.IsValueType) ? objectTypeDefinition : genericParameter.GetElementType());
		}
		return array;
	}

	private bool CanTypeParametersBeSubstituted(Collection<GenericParameter> genericTypeParameters)
	{
		foreach (GenericParameter genericTypeParameter in genericTypeParameters)
		{
			if (genericTypeParameter.IsGenericParameter && !CanGenericTypeParameterConstrainsBeSatisfied(genericTypeParameter))
			{
				return false;
			}
		}
		return true;
	}

	private bool CanGenericTypeParameterConstrainsBeSatisfied(GenericParameter typeParameter)
	{
		DebugAssertHelper.Assert(typeParameter.IsGenericParameter);
		if ((typeParameter.Attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
		{
			return true;
		}
		if (!typeParameter.HasConstraints)
		{
			return true;
		}
		bool flag = false;
		for (int i = 0; i < typeParameter.Constraints.Count; i++)
		{
			if (!typeParameter.Constraints[i].IsGenericParameter)
			{
				flag = true;
			}
		}
		return !flag;
	}

	private DotNetConstructorInfo[] BuildConstructorInfoList(CecilTypeInfo ti)
	{
		return (from c in ti.TypeDefinition.Methods
			where c.IsConstructor && c.IsPublic
			select BuildConstructorInfo(ti, c) into c
			where c != null
			select c).ToArray();
	}

	private DotNetConstructorInfo BuildConstructorInfo(CecilTypeInfo ti, MethodReference constructor)
	{
		MethodReference methodReference = TrySubstituteGenericParameterTypesInConstructor(ti, constructor);
		if (methodReference == null)
		{
			return null;
		}
		return new DotNetConstructorInfo
		{
			Name = DotNetUtilities.GetConstructorNameFromTypeName(ti.TypeDefinition.Name),
			Parameters = BuildParameterInfoList(methodReference.Parameters)
		};
	}

	private DotNetMethodInfo[] BuildMethodInfoList(CecilTypeInfo ti)
	{
		return GetMembersOfType(ti, DotNetMethodInfoSignatureComparer.Instance, GetMethods, (DotNetTypeInfo dotNetTypeInfo) => dotNetTypeInfo.Methods).ToArray();
		IEnumerable<DotNetMethodInfo> GetMethods(TypeDefinition typeDefinition, TypeReference closedTypeReference)
		{
			return from m in typeDefinition.Methods
				where m.IsUserVisibleMethod() && !m.IsOverrideMethod()
				select new CecilMethodInfo(m, TrySubstituteGenericParameterTypesInMethod(closedTypeReference, m)) into m
				where m.IsValid
				select BuildMethodInfo(m) into m
				where m != null
				select m;
		}
	}

	private DotNetMethodInfo BuildMethodInfo(CecilMethodInfo methodInformation)
	{
		DotNetParameterInfo[] parameters = BuildParameterInfoList(methodInformation.Parameters);
		return new DotNetMethodInfo
		{
			Name = methodInformation.Name,
			IsStatic = methodInformation.IsStatic,
			ReturnValue = new DotNetReturnValueInfo
			{
				IsOptional = true,
				Type = GetReturnTypeInfo(methodInformation.ReturnType)
			},
			IsVisibleInControlAddIns = ContainsApplicationVisibleAttribute(methodInformation.CustomAttributes),
			Parameters = parameters,
			Signature = ComputeSignatureWithoutClassName(methodInformation.Name, parameters)
		};
	}

	private static string ComputeSignatureWithoutClassName(string methodName, DotNetParameterInfo[] parameters, bool skipLastParameter = false)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			StringBuilder builder = instance.Builder;
			builder.Append(methodName);
			builder.Append("(");
			if (parameters != null && parameters.Length != 0)
			{
				for (int i = 0; i < parameters.Length; i++)
				{
					if (!skipLastParameter || i != parameters.Length - 1)
					{
						DotNetParameterInfo dotNetParameterInfo = parameters[i];
						if (i > 0)
						{
							builder.Append(",");
						}
						builder.Append(dotNetParameterInfo.ParameterType.FullName);
					}
				}
			}
			builder.Append(")");
			return builder.ToString();
		}
		finally
		{
			instance.Free();
		}
	}

	private static DotNetEnumeratorInfo[] BuildEnumMemberList(TypeDefinition type)
	{
		if (!type.IsEnum)
		{
			return Array.Empty<DotNetEnumeratorInfo>();
		}
		return (from f in type.Fields.Skip(1)
			select new DotNetEnumeratorInfo
			{
				Name = f.Name,
				Ordinal = Convert.ToInt64(f.Constant, CultureInfo.InvariantCulture)
			}).ToArray();
	}

	private DotNetTypeInfo BuildEnumeratorElementTypeInfo(CecilTypeInfo ti, DotNetTypeInfo partialTypeInfo)
	{
		DebugAssertHelper.Assert(partialTypeInfo.Methods != null);
		DebugAssertHelper.Assert(partialTypeInfo.Interfaces != null);
		DebugAssertHelper.Assert(partialTypeInfo.BaseTypes != null);
		if (!ImplementsEnumerablePattern(ti, partialTypeInfo))
		{
			return null;
		}
		return GetPartialTypeInfo(objectTypeDefinition);
	}

	private bool ImplementsEnumerablePattern(CecilTypeInfo ti, DotNetTypeInfo partialTypeInfo)
	{
		if (ti.ClosedType.IsArray)
		{
			return true;
		}
		if (InheritsEnumerableImplementation(partialTypeInfo))
		{
			return true;
		}
		if (ImplementsDuckedTypeEnumerablePattern(partialTypeInfo))
		{
			return true;
		}
		if (ImplementsGenericIEnumerable(partialTypeInfo))
		{
			return true;
		}
		return ImplementsIEnumerable(partialTypeInfo);
	}

	private bool InheritsEnumerableImplementation(DotNetTypeInfo partialTypeInfo)
	{
		DotNetTypeInfo[] interfaces = partialTypeInfo.Interfaces;
		for (int i = 0; i < interfaces.Length; i++)
		{
			if (interfaces[i].EnumeratorElementType != null)
			{
				return true;
			}
		}
		interfaces = partialTypeInfo.BaseTypes;
		for (int i = 0; i < interfaces.Length; i++)
		{
			if (interfaces[i].EnumeratorElementType != null)
			{
				return true;
			}
		}
		return false;
	}

	private bool ImplementsGenericIEnumerable(DotNetTypeInfo partialTypeInfo)
	{
		return ImplementsInterface(partialTypeInfo, genericIEnumerableTypeDefinition.FullName);
	}

	private bool ImplementsIEnumerable(DotNetTypeInfo partialTypeInfo)
	{
		return ImplementsInterface(partialTypeInfo, iEnumerableTypeDefinition.FullName);
	}

	private bool ImplementsInterface(DotNetTypeInfo partialTypeInfo, string interfaceName)
	{
		if (partialTypeInfo.TypeName == interfaceName)
		{
			return true;
		}
		DotNetTypeInfo[] interfaces = partialTypeInfo.Interfaces;
		for (int i = 0; i < interfaces.Length; i++)
		{
			if (interfaces[i].TypeName == interfaceName)
			{
				return true;
			}
		}
		return false;
	}

	private bool ImplementsDuckedTypeEnumerablePattern(DotNetTypeInfo partialTypeInfo)
	{
		IEnumerable<DotNetMethodInfo> source = partialTypeInfo.Methods.Where((DotNetMethodInfo m) => m.IsGetEnumeratorMethod());
		if (source.Count() != 1)
		{
			return false;
		}
		DotNetMethodInfo dotNetMethodInfo = source.First();
		DotNetTypeInfo fullTypeInfo = GetFullTypeInfo(dotNetMethodInfo.ReturnValue.Type);
		if (fullTypeInfo == null || fullTypeInfo.RequiresCompletion)
		{
			return false;
		}
		if (fullTypeInfo.Methods.Where((DotNetMethodInfo m) => !m.IsStatic && m.Name == "MoveNext" && m.ReturnValue.Type.FullName == boolTypeDefinition.FullName).Count() != 1)
		{
			return false;
		}
		if (fullTypeInfo.Properties.Where((DotNetPropertyInfo m) => m?.Getter != null && !m.Getter.IsStatic && m.Name == "Current").Count() != 1)
		{
			return false;
		}
		return true;
	}

	private DotNetEventInfo[] BuildEventInfoList(CecilTypeInfo ti)
	{
		return GetMembersOfType(ti, DotNetEventInfoSignatureComparer.Instance, GetPublicEvents, (DotNetTypeInfo t) => t.Events).ToArray();
		IEnumerable<DotNetEventInfo> GetPublicEvents(TypeDefinition typeDefinition, TypeReference closedTypeReference)
		{
			return from e in (from e in typeDefinition.Events
					where e.IsUserVisibleEvent()
					select GetCecilEventInfo(closedTypeReference, e) into e
					where e.IsValid
					select e).Select(BuildEventInfo)
				where e != null
				select e;
		}
	}

	private CecilEventInfo GetCecilEventInfo(TypeReference closedTypeReference, EventDefinition e)
	{
		TypeReference typeReference = e.EventType.MakeGenericInnerTypeIfNecessary(closedTypeReference);
		return new CecilEventInfo(e, typeReference.Resolve(), typeReference);
	}

	private DotNetEventInfo BuildEventInfo(CecilEventInfo eventInfo)
	{
		DebugAssertHelper.Assert(eventInfo.IsValid);
		MethodReference methodReference = eventInfo.EventType.Methods.FirstOrDefault((MethodDefinition m) => string.Equals("Invoke", m.Name));
		if (methodReference == null)
		{
			return null;
		}
		methodReference = TrySubstituteGenericParameterTypesInMethod(eventInfo.ClosedEventType, methodReference);
		Collection<ParameterDefinition> parameters = methodReference.Parameters;
		if (IsSupportedEvent(parameters))
		{
			return new DotNetEventInfo
			{
				Name = eventInfo.Name,
				Parameters = BuildParameterInfoList(parameters),
				Signature = eventInfo.Signature,
				IsVisibleInControlAddIns = ContainsApplicationVisibleAttribute(eventInfo.CustomAttributes)
			};
		}
		return null;
	}

	private bool IsSupportedEvent(Collection<ParameterDefinition> eventParameters)
	{
		return eventParameters.All((ParameterDefinition x) => !x.ParameterType.IsByReference || SupportedByRefEventParameterTypes.Contains(x.ParameterType.FullName));
	}

	private DotNetPropertyInfo[] BuildPropertyInfoList(CecilTypeInfo ti)
	{
		return GetMembersOfType(ti, DotNetPropertyInfoSignatureComparer.Instance, GetProperties, (DotNetTypeInfo info) => info.Properties).ToArray();
		IEnumerable<DotNetPropertyInfo> GetProperties(TypeDefinition typeDefinition, TypeReference closedTypeReference)
		{
			return from p in (from p in typeDefinition.Properties
					where p.IsUserVisibleProperty() && !p.IsOverrideProperty()
					select CreateCecilPropertyInfo(closedTypeReference, p)).Select(BuildPropertyInfo)
				where p != null
				select p;
		}
	}

	private CecilPropertyInfo CreateCecilPropertyInfo(TypeReference closedType, PropertyDefinition p)
	{
		MethodReference getMethod = null;
		if (p.GetMethod != null)
		{
			getMethod = TrySubstituteGenericParameterTypesInMethod(closedType, p.GetMethod);
		}
		MethodReference setMethod = null;
		if (p.SetMethod != null)
		{
			setMethod = TrySubstituteGenericParameterTypesInMethod(closedType, p.SetMethod);
		}
		return new CecilPropertyInfo(p, getMethod, setMethod);
	}

	private DotNetPropertyInfo BuildPropertyInfo(CecilPropertyInfo property)
	{
		if ((property == null || property.GetMethod?.IsPublic != true) && (property == null || property.SetMethod?.IsPublic != true))
		{
			return null;
		}
		CecilMethodInfo getMethod = property.GetMethod;
		DotNetMethodInfo getter = null;
		if (getMethod != null && getMethod.IsPublic)
		{
			DotNetParameterInfo[] parameters = BuildParameterInfoList(getMethod.Parameters);
			getter = new DotNetMethodInfo
			{
				Name = property.Name,
				IsStatic = getMethod.IsStatic,
				ReturnValue = new DotNetReturnValueInfo
				{
					IsOptional = getMethod.IsReturnValueOptional,
					Type = GetReturnTypeInfo(getMethod.ReturnType)
				},
				Parameters = parameters,
				Signature = ComputeSignatureWithoutClassName(property.Name, parameters)
			};
		}
		CecilMethodInfo setMethod = property.SetMethod;
		DotNetMethodInfo setter = null;
		if (setMethod != null && setMethod.IsPublic)
		{
			DotNetParameterInfo[] parameters2 = BuildParameterInfoList(setMethod.Parameters);
			setter = new DotNetMethodInfo
			{
				Name = property.Name,
				IsStatic = setMethod.IsStatic,
				ReturnValue = new DotNetReturnValueInfo
				{
					IsOptional = setMethod.IsReturnValueOptional,
					Type = GetReturnTypeInfo(setMethod.ReturnType)
				},
				Parameters = parameters2,
				Signature = ComputeSignatureWithoutClassName(property.Name, parameters2)
			};
		}
		return new DotNetPropertyInfo
		{
			Name = property.Name,
			IsVisibleInControlAddIns = ContainsApplicationVisibleAttribute(property.CustomAttributes),
			Signature = ComputePropertySignature(getter, setter),
			Getter = getter,
			Setter = setter
		};
	}

	private static string ComputePropertySignature(DotNetMethodInfo getter, DotNetMethodInfo setter)
	{
		if (getter != null)
		{
			return getter.Signature;
		}
		return ComputeSignatureWithoutClassName(setter.Name, setter.Parameters, skipLastParameter: true);
	}

	public CecilDotNetTypeInfoProvider(ReaderParameters moduleReaderParameters)
	{
		ModuleDefinition moduleDefinition = ModuleDefinition.ReadModule(typeof(object).Module.FullyQualifiedName, moduleReaderParameters);
		objectTypeDefinition = moduleDefinition.ImportReference(typeof(object)).Resolve();
		boolTypeDefinition = moduleDefinition.ImportReference(typeof(bool)).Resolve();
		stringTypeDefinition = moduleDefinition.ImportReference(typeof(string)).Resolve();
		iEnumerableTypeDefinition = moduleDefinition.ImportReference(typeof(IEnumerable)).Resolve();
		genericIEnumerableTypeDefinition = moduleDefinition.ImportReference(typeof(IEnumerable<>)).Resolve();
		streamTypeDefinition = moduleDefinition.ImportReference(typeof(Stream)).Resolve();
		dateTimeTypeDefinition = moduleDefinition.ImportReference(typeof(DateTime)).Resolve();
		arrayTypeDefinition = moduleDefinition.ImportReference(typeof(Array)).Resolve();
		nullableTypeDefinition = moduleDefinition.ImportReference(typeof(Nullable<>)).Resolve();
		genericIListTypeDefinition = moduleDefinition.ImportReference(typeof(IList<>)).Resolve();
		CacheReservedTypeInfo(moduleDefinition, typeof(object), DotNetSpecialType.System_Object);
		CacheReservedTypeInfo(moduleDefinition, typeof(Enum), DotNetSpecialType.System_Enum);
		CacheReservedTypeInfo(moduleDefinition, typeof(void), DotNetSpecialType.System_Void);
		CacheReservedTypeInfo(moduleDefinition, typeof(bool), DotNetSpecialType.System_Boolean);
		CacheReservedTypeInfo(moduleDefinition, typeof(char), DotNetSpecialType.System_Char);
		CacheReservedTypeInfo(moduleDefinition, typeof(sbyte), DotNetSpecialType.System_SByte);
		CacheReservedTypeInfo(moduleDefinition, typeof(byte), DotNetSpecialType.System_Byte);
		CacheReservedTypeInfo(moduleDefinition, typeof(short), DotNetSpecialType.System_Int16);
		CacheReservedTypeInfo(moduleDefinition, typeof(ushort), DotNetSpecialType.System_UInt16);
		CacheReservedTypeInfo(moduleDefinition, typeof(int), DotNetSpecialType.System_Int32);
		CacheReservedTypeInfo(moduleDefinition, typeof(uint), DotNetSpecialType.System_UInt32);
		CacheReservedTypeInfo(moduleDefinition, typeof(long), DotNetSpecialType.System_Int64);
		CacheReservedTypeInfo(moduleDefinition, typeof(ulong), DotNetSpecialType.System_UInt64);
		CacheReservedTypeInfo(moduleDefinition, typeof(decimal), DotNetSpecialType.System_Decimal);
		CacheReservedTypeInfo(moduleDefinition, typeof(float), DotNetSpecialType.System_Single);
		CacheReservedTypeInfo(moduleDefinition, typeof(double), DotNetSpecialType.System_Double);
		CacheReservedTypeInfo(moduleDefinition, typeof(string), DotNetSpecialType.System_String);
		CacheReservedTypeInfo(moduleDefinition, typeof(TimeSpan), DotNetSpecialType.System_TimeSpan);
		CacheReservedTypeInfo(moduleDefinition, typeof(DateTime), DotNetSpecialType.System_DateTime);
		CacheReservedTypeInfo(moduleDefinition, typeof(Guid), DotNetSpecialType.System_Guid);
		CacheReservedTypeInfo(moduleDefinition, typeof(Stream), DotNetSpecialType.System_Stream);
	}

	public DotNetAssemblyInfo GetAssemblyInfo(AssemblyDefinition assembly)
	{
		if (assembly == null)
		{
			return null;
		}
		return new DotNetAssemblyInfo(assembly.Name.Name, assembly.Name.Version.ToString(), assembly.Name.Culture, DotNetUtilities.GetPublicKeyToken(assembly.Name.PublicKeyToken), assembly.MainModule.FileName);
	}

	public DotNetTypeInfo GetTypeInfo(TypeReference type)
	{
		lock (getTypeInfoLock)
		{
			return GetTypeInfoCore(type);
		}
	}

	private DotNetTypeInfo GetTypeInfoCore(TypeReference type)
	{
		return BuildTypeInfo(type);
	}

	private DotNetSpecialType GetSpecialType(CecilTypeInfo t)
	{
		if (t.HasDefinition)
		{
			TypeDefinition typeDefinition = t.TypeDefinition;
			if (typeDefinition.IsEnum)
			{
				return DotNetSpecialType.System_Enum;
			}
			if (streamTypeDefinition.IsAssignableFrom(typeDefinition))
			{
				return DotNetSpecialType.System_Stream;
			}
			if (dateTimeTypeDefinition.IsAssignableFrom(typeDefinition))
			{
				return DotNetSpecialType.System_DateTime;
			}
			if (stringTypeDefinition.IsAssignableFrom(typeDefinition))
			{
				return DotNetSpecialType.System_String;
			}
			if (nullableTypeDefinition.IsAssignableFrom(typeDefinition))
			{
				return DotNetSpecialType.System_Nullable;
			}
		}
		return DotNetSpecialType.None;
	}

	private void CacheReservedTypeInfo(ModuleDefinition corLib, Type t, DotNetSpecialType specialType)
	{
		TypeReference type = corLib.GetType(t.FullName);
		GetPartialTypeInfo(type).SpecialType = specialType;
		TypeReference type2 = type.MakeByReferenceType();
		GetTypeInfoCore(type2).SpecialType = specialType;
		GetTypeInfoCore(type).SpecialType = specialType;
	}

	private DotNetTypeInfo GetPartialTypeInfo(TypeReference type)
	{
		if (type == null)
		{
			return null;
		}
		return BuildTypeInfo(type, getMemberInfo: false);
	}

	private DotNetTypeInfo GetReturnTypeInfo(TypeReference type)
	{
		TypeReference type2 = RemoveNullableTypeWrapper(type);
		return GetPartialTypeInfo(type2);
	}

	private DotNetTypeInfo GetParameterTypeInfo(TypeReference type)
	{
		TypeReference type2 = RemoveNullableTypeWrapper(type);
		return GetPartialTypeInfo(type2);
	}

	private TypeReference RemoveNullableTypeWrapper(TypeReference type)
	{
		if (!type.IsNullableValueType())
		{
			return type;
		}
		DebugAssertHelper.Assert(type.IsGenericInstance);
		return ((GenericInstanceType)type).GenericArguments[0];
	}

	private DotNetTypeInfo BuildTypeInfo(TypeReference openTypeReference, bool getMemberInfo = true)
	{
		CecilTypeInfo typeInformation = GetTypeInformation(openTypeReference);
		if (typeInformation == null || !typeInformation.IsSupportedType)
		{
			return null;
		}
		if (getMemberInfo && !typeInformation.HasDefinition)
		{
			return null;
		}
		if (typeInformation.HasDefinition)
		{
			return BuildTypeInfoFromDefinition(typeInformation, getMemberInfo);
		}
		return BuildTypeInfoFromReference(typeInformation);
	}

	private DotNetTypeInfo BuildTypeInfoFromReference(CecilTypeInfo ti)
	{
		if (!typeInfoCache.TryGetValue(ti.ClosedType.FullName, out var value))
		{
			value = CreateTypeInfoFromTypeReference(ti);
			typeInfoCache.Add(ti.ClosedType.FullName, value);
		}
		return value;
	}

	private DotNetTypeInfo BuildTypeInfoFromDefinition(CecilTypeInfo ti, bool getMemberInfo)
	{
		if (typeInfoCache.TryGetValue(ti.ClosedType.FullName, out var value) && value != null && !value.RequiresCompletion)
		{
			return value;
		}
		bool num = (value == null) | getMemberInfo;
		if (num)
		{
			value = CreateTypeInfoFromTypeDefinition(ti);
		}
		if (getMemberInfo)
		{
			DebugAssertHelper.Assert(value.RequiresCompletion);
			value.Constructors = BuildConstructorInfoList(ti);
			value.Methods = BuildMethodInfoList(ti);
			value.Properties = BuildPropertyInfoList(ti);
			value.EnumMembers = BuildEnumMemberList(ti.TypeDefinition);
			value.EnumeratorElementType = BuildEnumeratorElementTypeInfo(ti, value);
			value.Events = BuildEventInfoList(ti);
			value.RequiresCompletion = false;
		}
		if (num)
		{
			typeInfoCache[ti.ClosedType.FullName] = value;
		}
		return value;
	}

	private DotNetTypeInfo GetFullTypeInfo(DotNetTypeInfo info)
	{
		if (!info.RequiresCompletion)
		{
			return info;
		}
		if (!cecilTypeInfos.TryGetValue(info.InternalTypeName, out var value))
		{
			return null;
		}
		return BuildTypeInfo(value.ClosedType);
	}

	private DotNetTypeInfo[] BuildInterfaceList(CecilTypeInfo ti)
	{
		if (ti.TypeDefinition == null)
		{
			return Array.Empty<DotNetTypeInfo>();
		}
		PooledHashSet<DotNetTypeInfo> instance = PooledHashSet<DotNetTypeInfo>.GetInstance();
		try
		{
			if (ti.BaseClass != null)
			{
				DotNetTypeInfo partialTypeInfo = GetPartialTypeInfo(ti.BaseClass);
				if (partialTypeInfo != null)
				{
					instance.AddAll(partialTypeInfo.Interfaces);
				}
			}
			TypeReference[] interfaces = ti.Interfaces;
			foreach (TypeReference type in interfaces)
			{
				DotNetTypeInfo partialTypeInfo2 = GetPartialTypeInfo(type);
				if (partialTypeInfo2 != null)
				{
					instance.Add(partialTypeInfo2);
					instance.AddAll(partialTypeInfo2.Interfaces);
				}
			}
			return instance.ToArray();
		}
		finally
		{
			instance.Free();
		}
	}

	private DotNetTypeInfo[] BuildBaseTypeList(CecilTypeInfo ti)
	{
		TypeDefinition typeDefinition = ti.TypeDefinition;
		if (typeDefinition == null || typeDefinition.IsInterface || ti.BaseClass == null)
		{
			return Array.Empty<DotNetTypeInfo>();
		}
		DotNetTypeInfo partialTypeInfo = GetPartialTypeInfo(ti.BaseClass);
		if (partialTypeInfo == null)
		{
			return Array.Empty<DotNetTypeInfo>();
		}
		DotNetTypeInfo[] array = new DotNetTypeInfo[partialTypeInfo.BaseTypes.Length + 1];
		array[0] = partialTypeInfo;
		for (int i = 1; i <= partialTypeInfo.BaseTypes.Length; i++)
		{
			array[i] = partialTypeInfo.BaseTypes[i - 1];
		}
		return array;
	}

	private DotNetTypeInfo CreateTypeInfoFromTypeReference(CecilTypeInfo ti)
	{
		TypeReference closedType = ti.ClosedType;
		DotNetAssemblyInfo assemblyInfo = GetAssemblyInfo(closedType.Module.Assembly);
		return InitializeTypeInfoWithEmptyMembers(new DotNetTypeInfo
		{
			Assembly = assemblyInfo,
			FullName = closedType.GetFullName(),
			TypeName = closedType.GetNamespaceQualifiedTypeName(),
			DisplayName = closedType.GetDisplayName(),
			InternalTypeName = closedType.FullName,
			IsStatic = false,
			IsValueType = closedType.IsValueType,
			IsByRef = closedType.IsByReference,
			IsInterface = false,
			SpecialType = GetSpecialType(ti)
		});
	}

	private DotNetTypeInfo InitializeTypeInfoWithEmptyMembers(DotNetTypeInfo typeInfo)
	{
		typeInfo.Constructors = typeInfo.Constructors ?? Array.Empty<DotNetConstructorInfo>();
		typeInfo.Methods = typeInfo.Methods ?? Array.Empty<DotNetMethodInfo>();
		typeInfo.Properties = typeInfo.Properties ?? Array.Empty<DotNetPropertyInfo>();
		typeInfo.EnumMembers = typeInfo.EnumMembers ?? Array.Empty<DotNetEnumeratorInfo>();
		typeInfo.EnumeratorElementType = null;
		typeInfo.Events = typeInfo.Events ?? Array.Empty<DotNetEventInfo>();
		typeInfo.BaseTypes = typeInfo.BaseTypes ?? Array.Empty<DotNetTypeInfo>();
		typeInfo.Interfaces = typeInfo.Interfaces ?? Array.Empty<DotNetTypeInfo>();
		return typeInfo;
	}

	private DotNetTypeInfo CreateTypeInfoFromTypeDefinition(CecilTypeInfo ti)
	{
		if (ti == null)
		{
			return null;
		}
		DotNetAssemblyInfo assemblyInfo = GetAssemblyInfo(ti.TypeDefinition.Module.Assembly);
		return InitializeTypeInfoWithEmptyMembers(new DotNetTypeInfo
		{
			Assembly = assemblyInfo,
			FullName = ti.ClosedType.GetFullName(),
			TypeName = ti.ClosedType.GetNamespaceQualifiedTypeName(),
			DisplayName = ti.ClosedType.GetDisplayName(),
			InternalTypeName = ti.ClosedType.FullName,
			IsStatic = ti.TypeDefinition.IsStatic(),
			IsValueType = ti.ClosedType.IsValueType,
			IsByRef = ti.ClosedType.IsByReference,
			IsInterface = ti.TypeDefinition.IsInterface,
			SpecialType = GetSpecialType(ti),
			BaseTypes = BuildBaseTypeList(ti),
			Interfaces = BuildInterfaceList(ti),
			ControlAddInName = GetControlAddInNameFromAttribute(ti.TypeDefinition)
		});
	}

	private CecilTypeInfo GetTypeInformation(TypeReference type)
	{
		if (type == null)
		{
			return null;
		}
		if (!cecilTypeInfos.TryGetValue(type.FullName, out var value))
		{
			value = GetCecilTypeInfo(type);
			cecilTypeInfos.Add(type.FullName, value);
		}
		return value;
	}

	private CecilTypeInfo GetCecilTypeInfo(TypeReference type)
	{
		TypeReference type2 = type;
		int num;
		if (!type.IsArray)
		{
			num = (type.GetElementType().IsFullyOpenGenericType() ? 1 : 0);
			if (num != 0)
			{
				type2 = type.ClearTypeModifiers();
			}
		}
		else
		{
			num = 0;
		}
		TypeReference typeReference = MakeGenericTypeIfNeeded(type2);
		TypeDefinition typeDefinition = GetTypeDefinition(type);
		var (baseClass, interfaces) = GetParentTypes(type, typeDefinition, typeReference);
		if (num != 0)
		{
			typeReference = typeReference.CloneTypeModifiersFrom(type);
		}
		return new CecilTypeInfo(typeDefinition, typeReference, baseClass, interfaces);
	}

	private (TypeReference BaseType, TypeReference[] Interfaces) GetParentTypes(TypeReference type, TypeDefinition typeDefinition, TypeReference closedTypeReference)
	{
		TypeReference item = null;
		TypeReference[] item2 = Array.Empty<TypeReference>();
		if (typeDefinition != null && closedTypeReference != null)
		{
			(item, item2) = CecilExtensions.ResolveClosedParents(typeDefinition, closedTypeReference, arrayTypeDefinition, genericIListTypeDefinition, type.IsArray);
		}
		return (BaseType: item, Interfaces: item2);
	}

	private TypeDefinition GetTypeDefinition(TypeReference type)
	{
		if (type != null && type.IsArray)
		{
			return arrayTypeDefinition;
		}
		TypeDefinition typeDefinition = type.Resolve();
		if (typeDefinition != null)
		{
			return typeDefinition;
		}
		if (type.Module.HasExportedTypes)
		{
			foreach (ExportedType exportedType in type.Module.ExportedTypes)
			{
				if (string.Equals(exportedType.FullName, type.FullName, StringComparison.Ordinal))
				{
					return exportedType.Resolve();
				}
			}
		}
		return null;
	}

	private DotNetParameterInfo[] BuildParameterInfoList(Collection<ParameterDefinition> parameterInfo)
	{
		return parameterInfo.Select((ParameterDefinition p) => new DotNetParameterInfo
		{
			Name = p.Name,
			IsOptional = p.IsOptional,
			IsParams = false,
			IsVar = p.ParameterType.IsByReference,
			ParameterType = GetParameterTypeInfo(p.ParameterType)
		}).ToArray();
	}

	private IEnumerable<T> GetMembersOfType<T>(CecilTypeInfo ti, IComparer<T> memberComparer, Func<TypeDefinition, TypeReference, IEnumerable<T>> memberFactory, Func<DotNetTypeInfo, IEnumerable<T>> memberSelector)
	{
		SortedSet<T> sortedSet = new SortedSet<T>(memberFactory(ti.TypeDefinition, ti.ClosedType), memberComparer);
		if (ti.TypeDefinition.IsInterface)
		{
			TypeReference[] interfaces = ti.Interfaces;
			foreach (TypeReference type in interfaces)
			{
				DotNetTypeInfo typeInfoCore = GetTypeInfoCore(type);
				if (typeInfoCore != null && !typeInfoCore.RequiresCompletion)
				{
					IEnumerable<T> values = memberSelector(typeInfoCore);
					sortedSet.AddAll(values);
				}
			}
		}
		else if (ti.BaseClass != null)
		{
			DotNetTypeInfo typeInfoCore2 = GetTypeInfoCore(ti.BaseClass);
			if (typeInfoCore2 != null && !typeInfoCore2.RequiresCompletion)
			{
				IEnumerable<T> values2 = memberSelector(typeInfoCore2);
				sortedSet.AddAll(values2);
			}
		}
		return sortedSet;
	}

	private static string GetControlAddInNameFromAttribute(TypeDefinition typeDefinition)
	{
		Collection<CustomAttribute> customAttributes = typeDefinition.CustomAttributes;
		if (customAttributes.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < customAttributes.Count; i++)
		{
			if (customAttributes[i].AttributeType.FullName == "Microsoft.Dynamics.Framework.UI.Extensibility.ControlAddInExportAttribute")
			{
				Collection<CustomAttributeArgument> constructorArguments = customAttributes[i].ConstructorArguments;
				if (constructorArguments.Count == 1)
				{
					return constructorArguments[0].Value as string;
				}
			}
		}
		return null;
	}

	private static bool HasAttribute(TypeDefinition typeDefinition, string fullyQualifiedAttributeClassName)
	{
		return ContainsCustomAttribute(typeDefinition.CustomAttributes, fullyQualifiedAttributeClassName);
	}

	private static bool ContainsApplicationVisibleAttribute(Collection<CustomAttribute> customAttributes)
	{
		return ContainsCustomAttribute(customAttributes, "Microsoft.Dynamics.Framework.UI.Extensibility.ApplicationVisibleAttribute");
	}

	private static bool ContainsCustomAttribute(Collection<CustomAttribute> customAttributes, string fullyQualifiedAttributeClassName)
	{
		if (customAttributes.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < customAttributes.Count; i++)
		{
			if (customAttributes[i].AttributeType.FullName == fullyQualifiedAttributeClassName)
			{
				return true;
			}
		}
		return false;
	}
}
