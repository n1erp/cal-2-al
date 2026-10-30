using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingModuleSymbol : NonMissingModuleSymbol
{
	private struct DestinationData
	{
		public AssemblySymbol To;

		private ConcurrentDictionary<NamedTypeSymbol, NamedTypeSymbol> _symbolMap;

		public ConcurrentDictionary<NamedTypeSymbol, NamedTypeSymbol> SymbolMap => LazyInitializer.EnsureInitialized(ref _symbolMap);
	}

	internal class RetargetingSymbolTranslator : CSharpSymbolVisitor<RetargetOptions, Symbol>
	{
		private class RetargetedTypeMethodFinder : RetargetingSymbolTranslator
		{
			private RetargetedTypeMethodFinder(RetargetingModuleSymbol retargetingModule)
				: base(retargetingModule)
			{
			}

			public static MethodSymbol Find(RetargetingSymbolTranslator translator, MethodSymbol method, NamedTypeSymbol retargetedType, IEqualityComparer<MethodSymbol> retargetedMethodComparer)
			{
				if (!method.IsGenericMethod)
				{
					return FindWorker(translator, method, retargetedType, retargetedMethodComparer);
				}
				return FindWorker(new RetargetedTypeMethodFinder(translator._retargetingModule), method, retargetedType, retargetedMethodComparer);
			}

			private static MethodSymbol FindWorker(RetargetingSymbolTranslator translator, MethodSymbol method, NamedTypeSymbol retargetedType, IEqualityComparer<MethodSymbol> retargetedMethodComparer)
			{
				ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(method.Parameters.Length);
				bool modifiersHaveChanged;
				foreach (ParameterSymbol parameter in method.Parameters)
				{
					instance.Add(new SignatureOnlyParameterSymbol(translator.Retarget(parameter.Type, RetargetOptions.RetargetPrimitiveTypesByTypeCode), translator.RetargetModifiers(parameter.CustomModifiers, out modifiersHaveChanged), translator.RetargetModifiers(parameter.RefCustomModifiers, out modifiersHaveChanged), parameter.IsParams, parameter.RefKind));
				}
				SignatureOnlyMethodSymbol y = new SignatureOnlyMethodSymbol(method.Name, retargetedType, method.MethodKind, method.CallingConvention, IndexedTypeParameterSymbol.Take(method.Arity), instance.ToImmutableAndFree(), method.RefKind, translator.Retarget(method.ReturnType, RetargetOptions.RetargetPrimitiveTypesByTypeCode), translator.RetargetModifiers(method.ReturnTypeCustomModifiers, out modifiersHaveChanged), translator.RetargetModifiers(method.RefCustomModifiers, out modifiersHaveChanged), ImmutableArray<MethodSymbol>.Empty);
				foreach (Symbol member in retargetedType.GetMembers(method.Name))
				{
					if (member.Kind == SymbolKind.Method)
					{
						MethodSymbol methodSymbol = (MethodSymbol)member;
						if (retargetedMethodComparer.Equals(methodSymbol, y))
						{
							return methodSymbol;
						}
					}
				}
				return null;
			}

			public override TypeParameterSymbol Retarget(TypeParameterSymbol typeParameter)
			{
				if ((object)typeParameter.ContainingModule == UnderlyingModule)
				{
					return base.Retarget(typeParameter);
				}
				return IndexedTypeParameterSymbol.GetTypeParameter(typeParameter.Ordinal);
			}
		}

		private readonly RetargetingModuleSymbol _retargetingModule;

		private ConcurrentDictionary<Symbol, Symbol> SymbolMap => _retargetingModule._symbolMap;

		private RetargetingAssemblySymbol RetargetingAssembly => _retargetingModule._retargetingAssembly;

		private SourceModuleSymbol UnderlyingModule => _retargetingModule._underlyingModule;

		private Dictionary<AssemblySymbol, DestinationData> RetargetingAssemblyMap => _retargetingModule._retargetingAssemblyMap;

		public RetargetingSymbolTranslator(RetargetingModuleSymbol retargetingModule)
		{
			_retargetingModule = retargetingModule;
		}

		public Symbol Retarget(Symbol symbol)
		{
			return symbol.Accept(this, RetargetOptions.RetargetPrimitiveTypesByName);
		}

		public MarshalPseudoCustomAttributeData Retarget(MarshalPseudoCustomAttributeData marshallingInfo)
		{
			return marshallingInfo?.WithTranslatedTypes((TypeSymbol type, RetargetingSymbolTranslator translator) => translator.Retarget(type, RetargetOptions.RetargetPrimitiveTypesByTypeCode), this);
		}

		public TypeSymbol Retarget(TypeSymbol symbol, RetargetOptions options)
		{
			return (TypeSymbol)symbol.Accept(this, options);
		}

		public NamespaceSymbol Retarget(NamespaceSymbol ns)
		{
			return (NamespaceSymbol)SymbolMap.GetOrAdd(ns, _retargetingModule._createRetargetingNamespace);
		}

		private NamedTypeSymbol RetargetNamedTypeDefinition(NamedTypeSymbol type, RetargetOptions options)
		{
			if (options == RetargetOptions.RetargetPrimitiveTypesByTypeCode)
			{
				PrimitiveTypeCode primitiveTypeCode = type.PrimitiveTypeCode;
				if (primitiveTypeCode != PrimitiveTypeCode.NotPrimitive)
				{
					return RetargetingAssembly.GetPrimitiveType(primitiveTypeCode);
				}
			}
			if (type.Kind == SymbolKind.ErrorType)
			{
				return Retarget((ErrorTypeSymbol)type);
			}
			AssemblySymbol containingAssembly = type.ContainingAssembly;
			if (((object)containingAssembly != RetargetingAssembly.UnderlyingAssembly) ? containingAssembly.IsLinked : type.IsExplicitDefinitionOfNoPiaLocalType)
			{
				return RetargetNoPiaLocalType(type);
			}
			if ((object)containingAssembly == RetargetingAssembly.UnderlyingAssembly)
			{
				return RetargetNamedTypeDefinitionFromUnderlyingAssembly(type);
			}
			if (!RetargetingAssemblyMap.TryGetValue(containingAssembly, out var value))
			{
				return type;
			}
			type = PerformTypeRetargeting(ref value, type);
			RetargetingAssemblyMap[containingAssembly] = value;
			return type;
		}

		private NamedTypeSymbol RetargetNamedTypeDefinitionFromUnderlyingAssembly(NamedTypeSymbol type)
		{
			ModuleSymbol containingModule = type.ContainingModule;
			if ((object)containingModule == UnderlyingModule)
			{
				NamedTypeSymbol containingType = type.ContainingType;
				while ((object)containingType != null)
				{
					if (containingType.IsExplicitDefinitionOfNoPiaLocalType)
					{
						return (NamedTypeSymbol)SymbolMap.GetOrAdd(type, new UnsupportedMetadataTypeSymbol());
					}
					containingType = containingType.ContainingType;
				}
				return (NamedTypeSymbol)SymbolMap.GetOrAdd(type, _retargetingModule._createRetargetingNamedType);
			}
			PEModuleSymbol addedModule = (PEModuleSymbol)RetargetingAssembly.Modules[containingModule.Ordinal];
			return RetargetNamedTypeDefinition((PENamedTypeSymbol)type, addedModule);
		}

		private NamedTypeSymbol RetargetNoPiaLocalType(NamedTypeSymbol type)
		{
			ConcurrentDictionary<NamedTypeSymbol, NamedTypeSymbol> noPiaUnificationMap = RetargetingAssembly.NoPiaUnificationMap;
			if (noPiaUnificationMap.TryGetValue(type, out var value))
			{
				return value;
			}
			NamedTypeSymbol value2;
			if (type.ContainingSymbol.Kind != SymbolKind.NamedType && type.Arity == 0)
			{
				bool isInterface = type.IsInterface;
				bool flag = false;
				string guidString = null;
				string guidString2 = null;
				if (isInterface)
				{
					flag = type.GetGuidString(out guidString);
				}
				MetadataTypeName name = MetadataTypeName.FromFullName(type.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat), useCLSCompliantNameArityEncoding: false, type.Arity);
				string identifier = null;
				if ((object)type.ContainingModule == _retargetingModule.UnderlyingModule)
				{
					foreach (CSharpAttributeData attribute in type.GetAttributes())
					{
						int targetAttributeSignatureIndex = attribute.GetTargetAttributeSignatureIndex(type, AttributeDescription.TypeIdentifierAttribute);
						if (targetAttributeSignatureIndex != -1)
						{
							if (targetAttributeSignatureIndex == 1 && attribute.CommonConstructorArguments.Length == 2)
							{
								guidString2 = attribute.CommonConstructorArguments[0].Value as string;
								identifier = attribute.CommonConstructorArguments[1].Value as string;
							}
							break;
						}
					}
				}
				else if (!(flag & isInterface))
				{
					type.ContainingAssembly.GetGuidString(out guidString2);
					identifier = name.FullName;
				}
				value2 = MetadataDecoder.SubstituteNoPiaLocalType(ref name, isInterface, type.BaseTypeNoUseSiteDiagnostics, guidString, guidString2, identifier, RetargetingAssembly);
			}
			else
			{
				value2 = new UnsupportedMetadataTypeSymbol();
			}
			return noPiaUnificationMap.GetOrAdd(type, value2);
		}

		private static NamedTypeSymbol RetargetNamedTypeDefinition(PENamedTypeSymbol type, PEModuleSymbol addedModule)
		{
			if (addedModule.TypeHandleToTypeMap.TryGetValue(type.Handle, out var value))
			{
				return (NamedTypeSymbol)value;
			}
			NamedTypeSymbol containingType = type.ContainingType;
			MetadataTypeName emittedTypeName;
			if ((object)containingType != null)
			{
				NamedTypeSymbol namedTypeSymbol = RetargetNamedTypeDefinition((PENamedTypeSymbol)containingType, addedModule);
				emittedTypeName = MetadataTypeName.FromTypeName(type.MetadataName, useCLSCompliantNameArityEncoding: false, type.Arity);
				return namedTypeSymbol.LookupMetadataType(ref emittedTypeName);
			}
			emittedTypeName = MetadataTypeName.FromNamespaceAndTypeName(type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat), type.MetadataName, useCLSCompliantNameArityEncoding: false, type.Arity);
			return addedModule.LookupTopLevelMetadataType(ref emittedTypeName);
		}

		private static NamedTypeSymbol PerformTypeRetargeting(ref DestinationData destination, NamedTypeSymbol type)
		{
			if (!destination.SymbolMap.TryGetValue(type, out var value))
			{
				NamedTypeSymbol containingType = type.ContainingType;
				NamedTypeSymbol value2;
				if ((object)containingType != null)
				{
					NamedTypeSymbol namedTypeSymbol = PerformTypeRetargeting(ref destination, containingType);
					MetadataTypeName emittedTypeName = MetadataTypeName.FromTypeName(type.MetadataName, useCLSCompliantNameArityEncoding: false, type.Arity);
					value2 = namedTypeSymbol.LookupMetadataType(ref emittedTypeName);
				}
				else
				{
					MetadataTypeName emittedTypeName = MetadataTypeName.FromNamespaceAndTypeName(type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat), type.MetadataName, useCLSCompliantNameArityEncoding: false, type.Arity);
					value2 = destination.To.LookupTopLevelMetadataType(ref emittedTypeName, digThroughForwardedTypes: true);
				}
				return destination.SymbolMap.GetOrAdd(type, value2);
			}
			return value;
		}

		public NamedTypeSymbol Retarget(NamedTypeSymbol type, RetargetOptions options)
		{
			if (type.IsTupleType)
			{
				NamedTypeSymbol namedTypeSymbol = Retarget(type.TupleUnderlyingType, options);
				if (namedTypeSymbol.IsTupleOrCompatibleWithTupleOfCardinality(type.TupleElementTypes.Length))
				{
					return ((TupleTypeSymbol)type).WithUnderlyingType(namedTypeSymbol);
				}
				return namedTypeSymbol;
			}
			NamedTypeSymbol originalDefinition = type.OriginalDefinition;
			NamedTypeSymbol namedTypeSymbol2 = RetargetNamedTypeDefinition(originalDefinition, options);
			if ((object)type == originalDefinition)
			{
				return namedTypeSymbol2;
			}
			if (namedTypeSymbol2.Kind == SymbolKind.ErrorType && !namedTypeSymbol2.IsGenericType)
			{
				return namedTypeSymbol2;
			}
			if (type.IsUnboundGenericType)
			{
				if ((object)namedTypeSymbol2 == originalDefinition)
				{
					return type;
				}
				return namedTypeSymbol2.AsUnboundGenericType();
			}
			NamedTypeSymbol namedTypeSymbol3 = type;
			ArrayBuilder<TypeWithModifiers> instance = ArrayBuilder<TypeWithModifiers>.GetInstance();
			int num = int.MaxValue;
			while ((object)namedTypeSymbol3 != null)
			{
				if (num == int.MaxValue && !namedTypeSymbol3.IsInterface)
				{
					num = instance.Count;
				}
				int arity = namedTypeSymbol3.Arity;
				if (arity > 0)
				{
					ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics = namedTypeSymbol3.TypeArgumentsNoUseSiteDiagnostics;
					if (namedTypeSymbol3.HasTypeArgumentsCustomModifiers)
					{
						for (int i = 0; i < arity; i++)
						{
							instance.Add(new TypeWithModifiers(typeArgumentsNoUseSiteDiagnostics[i], namedTypeSymbol3.GetTypeArgumentCustomModifiers(i)));
						}
					}
					else
					{
						for (int j = 0; j < arity; j++)
						{
							instance.Add(new TypeWithModifiers(typeArgumentsNoUseSiteDiagnostics[j]));
						}
					}
				}
				namedTypeSymbol3 = namedTypeSymbol3.ContainingType;
			}
			bool flag = !originalDefinition.Equals(namedTypeSymbol2);
			ArrayBuilder<TypeWithModifiers> instance2 = ArrayBuilder<TypeWithModifiers>.GetInstance(instance.Count);
			foreach (TypeWithModifiers item2 in instance)
			{
				TypeWithModifiers item = new TypeWithModifiers((TypeSymbol)item2.Type.Accept(this, RetargetOptions.RetargetPrimitiveTypesByTypeCode), RetargetModifiers(item2.CustomModifiers, out var modifiersHaveChanged));
				if (!flag && (modifiersHaveChanged || item.Type != item2.Type))
				{
					flag = true;
				}
				instance2.Add(item);
			}
			bool flag2 = IsNoPiaIllegalGenericInstantiation(instance, instance2, num);
			instance.Free();
			NamedTypeSymbol namedTypeSymbol4;
			if (!flag)
			{
				namedTypeSymbol4 = type;
			}
			else
			{
				namedTypeSymbol3 = namedTypeSymbol2;
				ArrayBuilder<TypeParameterSymbol> instance3 = ArrayBuilder<TypeParameterSymbol>.GetInstance(instance2.Count);
				while ((object)namedTypeSymbol3 != null)
				{
					if (namedTypeSymbol3.Arity > 0)
					{
						instance3.AddRange(namedTypeSymbol3.TypeParameters);
					}
					namedTypeSymbol3 = namedTypeSymbol3.ContainingType;
				}
				namedTypeSymbol4 = new TypeMap(instance3.ToImmutableAndFree(), instance2.ToImmutable()).SubstituteNamedType(namedTypeSymbol2);
			}
			instance2.Free();
			if (flag2)
			{
				return new NoPiaIllegalGenericInstantiationSymbol(_retargetingModule, namedTypeSymbol4);
			}
			return namedTypeSymbol4;
		}

		private bool IsNoPiaIllegalGenericInstantiation(ArrayBuilder<TypeWithModifiers> oldArguments, ArrayBuilder<TypeWithModifiers> newArguments, int startOfNonInterfaceArguments)
		{
			if (UnderlyingModule.ContainsExplicitDefinitionOfNoPiaLocalTypes)
			{
				for (int i = startOfNonInterfaceArguments; i < oldArguments.Count; i++)
				{
					if (IsOrClosedOverAnExplicitLocalType(oldArguments[i].Type))
					{
						return true;
					}
				}
			}
			ImmutableArray<AssemblySymbol> assembliesToEmbedTypesFrom = UnderlyingModule.GetAssembliesToEmbedTypesFrom();
			if (assembliesToEmbedTypesFrom.Length > 0)
			{
				for (int j = startOfNonInterfaceArguments; j < oldArguments.Count; j++)
				{
					if (MetadataDecoder.IsOrClosedOverATypeFromAssemblies(oldArguments[j].Type, assembliesToEmbedTypesFrom))
					{
						return true;
					}
				}
			}
			ImmutableArray<AssemblySymbol> linkedReferencedAssemblies = RetargetingAssembly.GetLinkedReferencedAssemblies();
			if (!linkedReferencedAssemblies.IsDefaultOrEmpty)
			{
				for (int k = startOfNonInterfaceArguments; k < newArguments.Count; k++)
				{
					if (MetadataDecoder.IsOrClosedOverATypeFromAssemblies(newArguments[k].Type, linkedReferencedAssemblies))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsOrClosedOverAnExplicitLocalType(TypeSymbol symbol)
		{
			switch (symbol.Kind)
			{
			case SymbolKind.TypeParameter:
				return false;
			case SymbolKind.ArrayType:
				return IsOrClosedOverAnExplicitLocalType(((ArrayTypeSymbol)symbol).ElementType);
			case SymbolKind.PointerType:
				return IsOrClosedOverAnExplicitLocalType(((PointerTypeSymbol)symbol).PointedAtType);
			case SymbolKind.DynamicType:
				return false;
			case SymbolKind.ErrorType:
			case SymbolKind.NamedType:
			{
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)symbol;
				if (namedTypeSymbol.IsTupleType)
				{
					namedTypeSymbol = namedTypeSymbol.TupleUnderlyingType;
				}
				if ((object)symbol.OriginalDefinition.ContainingModule == _retargetingModule.UnderlyingModule && namedTypeSymbol.IsExplicitDefinitionOfNoPiaLocalType)
				{
					return true;
				}
				do
				{
					foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics)
					{
						if (IsOrClosedOverAnExplicitLocalType(typeArgumentsNoUseSiteDiagnostic))
						{
							return true;
						}
					}
					namedTypeSymbol = namedTypeSymbol.ContainingType;
				}
				while ((object)namedTypeSymbol != null);
				return false;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
			}
		}

		public virtual TypeParameterSymbol Retarget(TypeParameterSymbol typeParameter)
		{
			return (TypeParameterSymbol)SymbolMap.GetOrAdd(typeParameter, _retargetingModule._createRetargetingTypeParameter);
		}

		public ArrayTypeSymbol Retarget(ArrayTypeSymbol type)
		{
			TypeSymbol elementType = type.ElementType;
			TypeSymbol typeSymbol = Retarget(elementType, RetargetOptions.RetargetPrimitiveTypesByTypeCode);
			ImmutableArray<CustomModifier> customModifiers = RetargetModifiers(type.CustomModifiers, out var modifiersHaveChanged);
			if (!modifiersHaveChanged && elementType.Equals(typeSymbol))
			{
				return type;
			}
			if (type.IsSZArray)
			{
				return ArrayTypeSymbol.CreateSZArray(RetargetingAssembly, typeSymbol, customModifiers);
			}
			return ArrayTypeSymbol.CreateMDArray(RetargetingAssembly, typeSymbol, type.Rank, type.Sizes, type.LowerBounds, customModifiers);
		}

		internal ImmutableArray<CustomModifier> RetargetModifiers(ImmutableArray<CustomModifier> oldModifiers, out bool modifiersHaveChanged)
		{
			ArrayBuilder<CustomModifier> arrayBuilder = null;
			for (int i = 0; i < oldModifiers.Length; i++)
			{
				CustomModifier customModifier = oldModifiers[i];
				NamedTypeSymbol namedTypeSymbol = Retarget((NamedTypeSymbol)customModifier.Modifier, RetargetOptions.RetargetPrimitiveTypesByName);
				if (!namedTypeSymbol.Equals(customModifier.Modifier))
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<CustomModifier>.GetInstance(oldModifiers.Length);
						arrayBuilder.AddRange(oldModifiers, i);
					}
					arrayBuilder.Add(customModifier.IsOptional ? CSharpCustomModifier.CreateOptional(namedTypeSymbol) : CSharpCustomModifier.CreateRequired(namedTypeSymbol));
				}
				else
				{
					arrayBuilder?.Add(customModifier);
				}
			}
			modifiersHaveChanged = arrayBuilder != null;
			if (!modifiersHaveChanged)
			{
				return oldModifiers;
			}
			return arrayBuilder.ToImmutableAndFree();
		}

		public PointerTypeSymbol Retarget(PointerTypeSymbol type)
		{
			TypeSymbol pointedAtType = type.PointedAtType;
			TypeSymbol typeSymbol = Retarget(pointedAtType, RetargetOptions.RetargetPrimitiveTypesByTypeCode);
			ImmutableArray<CustomModifier> customModifiers = RetargetModifiers(type.CustomModifiers, out var modifiersHaveChanged);
			if (!modifiersHaveChanged && pointedAtType.Equals(typeSymbol))
			{
				return type;
			}
			return new PointerTypeSymbol(typeSymbol, customModifiers);
		}

		public static ErrorTypeSymbol Retarget(ErrorTypeSymbol type)
		{
			if (type.GetUseSiteDiagnostic() != null)
			{
				return type;
			}
			return (type as ExtendedErrorTypeSymbol)?.AsUnreported() ?? new ExtendedErrorTypeSymbol(type, type.ResultKind, type.ErrorInfo ?? new CSDiagnosticInfo(ErrorCode.ERR_ErrorInReferencedAssembly, type.ContainingAssembly?.Identity.GetDisplayName() ?? string.Empty), unreported: true);
		}

		public ImmutableArray<Symbol> Retarget(ImmutableArray<Symbol> arr)
		{
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(arr.Length);
			foreach (Symbol item in arr)
			{
				instance.Add(Retarget(item));
			}
			return instance.ToImmutableAndFree();
		}

		public ImmutableArray<NamedTypeSymbol> Retarget(ImmutableArray<NamedTypeSymbol> sequence)
		{
			ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance(sequence.Length);
			foreach (NamedTypeSymbol item in sequence)
			{
				instance.Add(Retarget(item, RetargetOptions.RetargetPrimitiveTypesByName));
			}
			return instance.ToImmutableAndFree();
		}

		public ImmutableArray<TypeSymbol> Retarget(ImmutableArray<TypeSymbol> sequence)
		{
			ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance(sequence.Length);
			foreach (TypeSymbol item in sequence)
			{
				instance.Add(Retarget(item, RetargetOptions.RetargetPrimitiveTypesByName));
			}
			return instance.ToImmutableAndFree();
		}

		public ImmutableArray<TypeParameterSymbol> Retarget(ImmutableArray<TypeParameterSymbol> list)
		{
			ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance(list.Length);
			foreach (TypeParameterSymbol item in list)
			{
				instance.Add(Retarget(item));
			}
			return instance.ToImmutableAndFree();
		}

		public MethodSymbol Retarget(MethodSymbol method)
		{
			return (MethodSymbol)SymbolMap.GetOrAdd(method, _retargetingModule._createRetargetingMethod);
		}

		public MethodSymbol Retarget(MethodSymbol method, IEqualityComparer<MethodSymbol> retargetedMethodComparer)
		{
			if ((object)method.ContainingModule == UnderlyingModule && (object)method == method.OriginalDefinition)
			{
				return Retarget(method);
			}
			NamedTypeSymbol containingType = method.ContainingType;
			NamedTypeSymbol namedTypeSymbol = Retarget(containingType, RetargetOptions.RetargetPrimitiveTypesByName);
			if ((object)namedTypeSymbol != containingType)
			{
				return FindMethodInRetargetedType(method, namedTypeSymbol, retargetedMethodComparer);
			}
			return method;
		}

		public FieldSymbol Retarget(FieldSymbol field)
		{
			return (FieldSymbol)SymbolMap.GetOrAdd(field, _retargetingModule._createRetargetingField);
		}

		public PropertySymbol Retarget(PropertySymbol property)
		{
			return (PropertySymbol)SymbolMap.GetOrAdd(property, _retargetingModule._createRetargetingProperty);
		}

		public PropertySymbol Retarget(PropertySymbol property, IEqualityComparer<PropertySymbol> retargetedPropertyComparer)
		{
			if ((object)property.ContainingModule == UnderlyingModule && (object)property == property.OriginalDefinition)
			{
				return Retarget(property);
			}
			NamedTypeSymbol containingType = property.ContainingType;
			NamedTypeSymbol namedTypeSymbol = Retarget(containingType, RetargetOptions.RetargetPrimitiveTypesByName);
			if ((object)namedTypeSymbol != containingType)
			{
				return FindPropertyInRetargetedType(property, namedTypeSymbol, retargetedPropertyComparer);
			}
			return property;
		}

		public EventSymbol Retarget(EventSymbol @event)
		{
			if ((object)@event.ContainingModule == UnderlyingModule && (object)@event == @event.OriginalDefinition)
			{
				return (EventSymbol)SymbolMap.GetOrAdd(@event, _retargetingModule._createRetargetingEvent);
			}
			NamedTypeSymbol containingType = @event.ContainingType;
			NamedTypeSymbol namedTypeSymbol = Retarget(containingType, RetargetOptions.RetargetPrimitiveTypesByName);
			if ((object)namedTypeSymbol != containingType)
			{
				return FindEventInRetargetedType(@event, namedTypeSymbol);
			}
			return @event;
		}

		private MethodSymbol FindMethodInRetargetedType(MethodSymbol method, NamedTypeSymbol retargetedType, IEqualityComparer<MethodSymbol> retargetedMethodComparer)
		{
			return RetargetedTypeMethodFinder.Find(this, method, retargetedType, retargetedMethodComparer);
		}

		private PropertySymbol FindPropertyInRetargetedType(PropertySymbol property, NamedTypeSymbol retargetedType, IEqualityComparer<PropertySymbol> retargetedPropertyComparer)
		{
			ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(property.Parameters.Length);
			bool modifiersHaveChanged;
			foreach (ParameterSymbol parameter in property.Parameters)
			{
				instance.Add(new SignatureOnlyParameterSymbol(Retarget(parameter.Type, RetargetOptions.RetargetPrimitiveTypesByTypeCode), RetargetModifiers(parameter.CustomModifiers, out modifiersHaveChanged), RetargetModifiers(parameter.RefCustomModifiers, out modifiersHaveChanged), parameter.IsParams, parameter.RefKind));
			}
			SignatureOnlyPropertySymbol y = new SignatureOnlyPropertySymbol(property.Name, retargetedType, instance.ToImmutableAndFree(), property.RefKind, Retarget(property.Type, RetargetOptions.RetargetPrimitiveTypesByTypeCode), RetargetModifiers(property.TypeCustomModifiers, out modifiersHaveChanged), RetargetModifiers(property.RefCustomModifiers, out modifiersHaveChanged), property.IsStatic, ImmutableArray<PropertySymbol>.Empty);
			foreach (Symbol member in retargetedType.GetMembers(property.Name))
			{
				if (member.Kind == SymbolKind.Property)
				{
					PropertySymbol propertySymbol = (PropertySymbol)member;
					if (retargetedPropertyComparer.Equals(propertySymbol, y))
					{
						return propertySymbol;
					}
				}
			}
			return null;
		}

		private EventSymbol FindEventInRetargetedType(EventSymbol @event, NamedTypeSymbol retargetedType)
		{
			TypeSymbol typeSymbol = Retarget(@event.Type, RetargetOptions.RetargetPrimitiveTypesByTypeCode);
			foreach (Symbol member in retargetedType.GetMembers(@event.Name))
			{
				if (member.Kind == SymbolKind.Event)
				{
					EventSymbol eventSymbol = (EventSymbol)member;
					if (eventSymbol.Type == typeSymbol)
					{
						return eventSymbol;
					}
				}
			}
			return null;
		}

		internal ImmutableArray<CustomModifier> RetargetModifiers(ImmutableArray<CustomModifier> oldModifiers, ref ImmutableArray<CustomModifier> lazyCustomModifiers)
		{
			if (lazyCustomModifiers.IsDefault)
			{
				ImmutableArray<CustomModifier> value = RetargetModifiers(oldModifiers, out var _);
				ImmutableInterlocked.InterlockedCompareExchange(ref lazyCustomModifiers, value, default);
			}
			return lazyCustomModifiers;
		}

		internal CustomModifiersTuple RetargetModifiers(ImmutableArray<CustomModifier> oldTypeModifiers, ImmutableArray<CustomModifier> oldRefModifiers, ref CustomModifiersTuple lazyCustomModifiers)
		{
			if (lazyCustomModifiers == null)
			{
				ImmutableArray<CustomModifier> typeCustomModifiers = RetargetModifiers(oldTypeModifiers, out var modifiersHaveChanged);
				ImmutableArray<CustomModifier> refCustomModifiers = RetargetModifiers(oldRefModifiers, out modifiersHaveChanged);
				Interlocked.CompareExchange(ref lazyCustomModifiers, CustomModifiersTuple.Create(typeCustomModifiers, refCustomModifiers), null);
			}
			return lazyCustomModifiers;
		}

		private ImmutableArray<CSharpAttributeData> RetargetAttributes(ImmutableArray<CSharpAttributeData> oldAttributes)
		{
			return oldAttributes.SelectAsArray((CSharpAttributeData a, RetargetingSymbolTranslator t) => t.RetargetAttributeData(a), this);
		}

		internal IEnumerable<CSharpAttributeData> RetargetAttributes(IEnumerable<CSharpAttributeData> attributes)
		{
			foreach (CSharpAttributeData attribute in attributes)
			{
				yield return RetargetAttributeData(attribute);
			}
		}

		private CSharpAttributeData RetargetAttributeData(CSharpAttributeData oldAttributeData)
		{
			SourceAttributeData sourceAttributeData = (SourceAttributeData)oldAttributeData;
			MethodSymbol attributeConstructor = sourceAttributeData.AttributeConstructor;
			MethodSymbol methodSymbol = (((object)attributeConstructor == null) ? null : Retarget(attributeConstructor, MemberSignatureComparer.RetargetedExplicitImplementationComparer));
			NamedTypeSymbol attributeClass = sourceAttributeData.AttributeClass;
			NamedTypeSymbol attributeClass2;
			if ((object)methodSymbol != null)
			{
				attributeClass2 = methodSymbol.ContainingType;
			}
			else
			{
				attributeClass2 = (((object)attributeClass == null) ? null : Retarget(attributeClass, RetargetOptions.RetargetPrimitiveTypesByTypeCode));
			}
			ImmutableArray<TypedConstant> commonConstructorArguments = sourceAttributeData.CommonConstructorArguments;
			ImmutableArray<TypedConstant> constructorArguments = RetargetAttributeConstructorArguments(commonConstructorArguments);
			ImmutableArray<KeyValuePair<string, TypedConstant>> commonNamedArguments = sourceAttributeData.CommonNamedArguments;
			ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments = RetargetAttributeNamedArguments(commonNamedArguments);
			return new RetargetingAttributeData(sourceAttributeData.ApplicationSyntaxReference, attributeClass2, methodSymbol, constructorArguments, sourceAttributeData.ConstructorArgumentsSourceIndices, namedArguments, sourceAttributeData.HasErrors, sourceAttributeData.IsConditionallyOmitted);
		}

		private ImmutableArray<TypedConstant> RetargetAttributeConstructorArguments(ImmutableArray<TypedConstant> constructorArguments)
		{
			ImmutableArray<TypedConstant> result = constructorArguments;
			bool typedConstantChanged = false;
			if (!constructorArguments.IsDefault && constructorArguments.Any())
			{
				ArrayBuilder<TypedConstant> instance = ArrayBuilder<TypedConstant>.GetInstance(constructorArguments.Length);
				foreach (TypedConstant item2 in constructorArguments)
				{
					TypedConstant item = RetargetTypedConstant(item2, ref typedConstantChanged);
					instance.Add(item);
				}
				if (typedConstantChanged)
				{
					result = instance.ToImmutable();
				}
				instance.Free();
			}
			return result;
		}

		private TypedConstant RetargetTypedConstant(TypedConstant oldConstant, ref bool typedConstantChanged)
		{
			TypeSymbol typeSymbol = (TypeSymbol)oldConstant.Type;
			TypeSymbol typeSymbol2 = (((object)typeSymbol == null) ? null : Retarget(typeSymbol, RetargetOptions.RetargetPrimitiveTypesByTypeCode));
			if (oldConstant.Kind == TypedConstantKind.Array)
			{
				ImmutableArray<TypedConstant> immutableArray = RetargetAttributeConstructorArguments(oldConstant.Values);
				if (typeSymbol2 != typeSymbol || immutableArray != oldConstant.Values)
				{
					typedConstantChanged = true;
					return new TypedConstant(typeSymbol2, immutableArray);
				}
				return oldConstant;
			}
			object value = oldConstant.Value;
			object obj = ((oldConstant.Kind != TypedConstantKind.Type || value == null) ? value : Retarget((TypeSymbol)value, RetargetOptions.RetargetPrimitiveTypesByTypeCode));
			if (typeSymbol2 != typeSymbol || obj != value)
			{
				typedConstantChanged = true;
				return new TypedConstant(typeSymbol2, oldConstant.Kind, obj);
			}
			return oldConstant;
		}

		private ImmutableArray<KeyValuePair<string, TypedConstant>> RetargetAttributeNamedArguments(ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
		{
			ImmutableArray<KeyValuePair<string, TypedConstant>> result = namedArguments;
			bool flag = false;
			if (namedArguments.Any())
			{
				ArrayBuilder<KeyValuePair<string, TypedConstant>> instance = ArrayBuilder<KeyValuePair<string, TypedConstant>>.GetInstance(namedArguments.Length);
				foreach (KeyValuePair<string, TypedConstant> item in namedArguments)
				{
					TypedConstant value = item.Value;
					bool typedConstantChanged = false;
					TypedConstant value2 = RetargetTypedConstant(value, ref typedConstantChanged);
					if (typedConstantChanged)
					{
						instance.Add(new KeyValuePair<string, TypedConstant>(item.Key, value2));
						flag = true;
					}
					else
					{
						instance.Add(item);
					}
				}
				if (flag)
				{
					result = instance.ToImmutable();
				}
				instance.Free();
			}
			return result;
		}

		internal ImmutableArray<CSharpAttributeData> GetRetargetedAttributes(ImmutableArray<CSharpAttributeData> underlyingAttributes, ref ImmutableArray<CSharpAttributeData> lazyCustomAttributes)
		{
			if (lazyCustomAttributes.IsDefault)
			{
				ImmutableArray<CSharpAttributeData> value = RetargetAttributes(underlyingAttributes);
				ImmutableInterlocked.InterlockedCompareExchange(ref lazyCustomAttributes, value, default);
			}
			return lazyCustomAttributes;
		}

		public override Symbol VisitModule(ModuleSymbol symbol, RetargetOptions options)
		{
			return _retargetingModule;
		}

		public override Symbol VisitNamespace(NamespaceSymbol symbol, RetargetOptions options)
		{
			return Retarget(symbol);
		}

		public override Symbol VisitNamedType(NamedTypeSymbol symbol, RetargetOptions options)
		{
			return Retarget(symbol, options);
		}

		public override Symbol VisitArrayType(ArrayTypeSymbol symbol, RetargetOptions options)
		{
			return Retarget(symbol);
		}

		public override Symbol VisitPointerType(PointerTypeSymbol symbol, RetargetOptions options)
		{
			return Retarget(symbol);
		}

		public override Symbol VisitMethod(MethodSymbol symbol, RetargetOptions options)
		{
			return Retarget(symbol);
		}

		public override Symbol VisitParameter(ParameterSymbol symbol, RetargetOptions options)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public override Symbol VisitField(FieldSymbol symbol, RetargetOptions options)
		{
			return Retarget(symbol);
		}

		public override Symbol VisitProperty(PropertySymbol symbol, RetargetOptions argument)
		{
			return Retarget(symbol);
		}

		public override Symbol VisitTypeParameter(TypeParameterSymbol symbol, RetargetOptions options)
		{
			return Retarget(symbol);
		}

		public override Symbol VisitErrorType(ErrorTypeSymbol symbol, RetargetOptions options)
		{
			return Retarget(symbol);
		}

		public override Symbol VisitEvent(EventSymbol symbol, RetargetOptions options)
		{
			return Retarget(symbol);
		}

		public override Symbol VisitDynamicType(DynamicTypeSymbol symbol, RetargetOptions argument)
		{
			return symbol;
		}
	}

	private readonly RetargetingAssemblySymbol _retargetingAssembly;

	private readonly SourceModuleSymbol _underlyingModule;

	private readonly Dictionary<AssemblySymbol, DestinationData> _retargetingAssemblyMap = new Dictionary<AssemblySymbol, DestinationData>();

	internal readonly RetargetingSymbolTranslator RetargetingTranslator;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private readonly ConcurrentDictionary<Symbol, Symbol> _symbolMap = new ConcurrentDictionary<Symbol, Symbol>(2, 4);

	private readonly Func<Symbol, RetargetingMethodSymbol> _createRetargetingMethod;

	private readonly Func<Symbol, RetargetingNamespaceSymbol> _createRetargetingNamespace;

	private readonly Func<Symbol, RetargetingTypeParameterSymbol> _createRetargetingTypeParameter;

	private readonly Func<Symbol, RetargetingNamedTypeSymbol> _createRetargetingNamedType;

	private readonly Func<Symbol, RetargetingFieldSymbol> _createRetargetingField;

	private readonly Func<Symbol, RetargetingPropertySymbol> _createRetargetingProperty;

	private readonly Func<Symbol, RetargetingEventSymbol> _createRetargetingEvent;

	internal override int Ordinal => 0;

	internal override Machine Machine => _underlyingModule.Machine;

	internal override bool Bit32Required => _underlyingModule.Bit32Required;

	public SourceModuleSymbol UnderlyingModule => _underlyingModule;

	public override NamespaceSymbol GlobalNamespace => RetargetingTranslator.Retarget(_underlyingModule.GlobalNamespace);

	public override bool IsImplicitlyDeclared => _underlyingModule.IsImplicitlyDeclared;

	public override string Name => _underlyingModule.Name;

	public override Symbol ContainingSymbol => _retargetingAssembly;

	public override AssemblySymbol ContainingAssembly => _retargetingAssembly;

	public override ImmutableArray<Location> Locations => _underlyingModule.Locations;

	internal override ICollection<string> TypeNames => _underlyingModule.TypeNames;

	internal override ICollection<string> NamespaceNames => _underlyingModule.NamespaceNames;

	internal override bool HasAssemblyCompilationRelaxationsAttribute => _underlyingModule.HasAssemblyCompilationRelaxationsAttribute;

	internal override bool HasAssemblyRuntimeCompatibilityAttribute => _underlyingModule.HasAssemblyRuntimeCompatibilityAttribute;

	internal override CharSet? DefaultMarshallingCharSet => _underlyingModule.DefaultMarshallingCharSet;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public RetargetingModuleSymbol(RetargetingAssemblySymbol retargetingAssembly, SourceModuleSymbol underlyingModule)
	{
		_retargetingAssembly = retargetingAssembly;
		_underlyingModule = underlyingModule;
		RetargetingTranslator = new RetargetingSymbolTranslator(this);
		_createRetargetingMethod = CreateRetargetingMethod;
		_createRetargetingNamespace = CreateRetargetingNamespace;
		_createRetargetingNamedType = CreateRetargetingNamedType;
		_createRetargetingField = CreateRetargetingField;
		_createRetargetingProperty = CreateRetargetingProperty;
		_createRetargetingEvent = CreateRetargetingEvent;
		_createRetargetingTypeParameter = CreateRetargetingTypeParameter;
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _underlyingModule.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}

	internal override void SetReferences(ModuleReferences<AssemblySymbol> moduleReferences, SourceAssemblySymbol originatingSourceAssemblyDebugOnly)
	{
		base.SetReferences(moduleReferences, originatingSourceAssemblyDebugOnly);
		_retargetingAssemblyMap.Clear();
		ImmutableArray<AssemblySymbol> referencedAssemblySymbols = _underlyingModule.GetReferencedAssemblySymbols();
		ImmutableArray<AssemblySymbol> symbols = moduleReferences.Symbols;
		int num = 0;
		int i = 0;
		while (num < symbols.Length)
		{
			for (; referencedAssemblySymbols[i].IsLinked; i++)
			{
			}
			if ((object)symbols[num] != referencedAssemblySymbols[i] && !_retargetingAssemblyMap.TryGetValue(referencedAssemblySymbols[i], out var _))
			{
				_retargetingAssemblyMap.Add(referencedAssemblySymbols[i], new DestinationData
				{
					To = symbols[num]
				});
			}
			num++;
			i++;
		}
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return RetargetingTranslator.GetRetargetedAttributes(_underlyingModule.GetAttributes(), ref _lazyCustomAttributes);
	}

	public override ModuleMetadata GetMetadata()
	{
		return _underlyingModule.GetMetadata();
	}

	private RetargetingMethodSymbol CreateRetargetingMethod(Symbol symbol)
	{
		return new RetargetingMethodSymbol(this, (MethodSymbol)symbol);
	}

	private RetargetingNamespaceSymbol CreateRetargetingNamespace(Symbol symbol)
	{
		return new RetargetingNamespaceSymbol(this, (NamespaceSymbol)symbol);
	}

	private RetargetingNamedTypeSymbol CreateRetargetingNamedType(Symbol symbol)
	{
		return new RetargetingNamedTypeSymbol(this, (NamedTypeSymbol)symbol);
	}

	private RetargetingFieldSymbol CreateRetargetingField(Symbol symbol)
	{
		return new RetargetingFieldSymbol(this, (FieldSymbol)symbol);
	}

	private RetargetingPropertySymbol CreateRetargetingProperty(Symbol symbol)
	{
		return new RetargetingPropertySymbol(this, (PropertySymbol)symbol);
	}

	private RetargetingEventSymbol CreateRetargetingEvent(Symbol symbol)
	{
		return new RetargetingEventSymbol(this, (EventSymbol)symbol);
	}

	private RetargetingTypeParameterSymbol CreateRetargetingTypeParameter(Symbol symbol)
	{
		return new RetargetingTypeParameterSymbol(this, (TypeParameterSymbol)symbol);
	}
}
