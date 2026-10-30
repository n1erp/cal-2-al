using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit.NoPia;

internal abstract class EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter> : CommonEmbeddedTypesManager where TPEModuleBuilder : CommonPEModuleBuilder where TModuleCompilationState : CommonModuleCompilationState where TEmbeddedTypesManager : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter> where TSyntaxNode : SyntaxNode where TAttributeData : AttributeData, ICustomAttribute where TAssemblySymbol : class, TSymbol where TNamedTypeSymbol : class, TSymbol, INamespaceTypeReference where TFieldSymbol : class, TSymbol, IFieldReference where TMethodSymbol : class, TSymbol, IMethodReference where TEventSymbol : class, TSymbol, ITypeMemberReference where TPropertySymbol : class, TSymbol, ITypeMemberReference where TParameterSymbol : class, TSymbol, IParameterListEntry, INamedEntity where TTypeParameterSymbol : class, TSymbol, IGenericMethodParameterReference where TEmbeddedType : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedType where TEmbeddedField : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedField where TEmbeddedMethod : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedMethod where TEmbeddedEvent : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedEvent where TEmbeddedProperty : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedProperty where TEmbeddedParameter : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedParameter where TEmbeddedTypeParameter : EmbeddedTypesManager<TPEModuleBuilder, TModuleCompilationState, TEmbeddedTypesManager, TSyntaxNode, TAttributeData, TSymbol, TAssemblySymbol, TNamedTypeSymbol, TFieldSymbol, TMethodSymbol, TEventSymbol, TPropertySymbol, TParameterSymbol, TTypeParameterSymbol, TEmbeddedType, TEmbeddedField, TEmbeddedMethod, TEmbeddedEvent, TEmbeddedProperty, TEmbeddedParameter, TEmbeddedTypeParameter>.CommonEmbeddedTypeParameter
{
	internal abstract class CommonEmbeddedEvent : CommonEmbeddedMember<TEventSymbol>, IEventDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition
	{
		private readonly TEmbeddedMethod _adder;

		private readonly TEmbeddedMethod _remover;

		private readonly TEmbeddedMethod _caller;

		private int _isUsedForComAwareEventBinding;

		internal override TEmbeddedTypesManager TypeManager => AnAccessor.TypeManager;

		protected abstract bool IsRuntimeSpecial { get; }

		protected abstract bool IsSpecialName { get; }

		protected abstract TEmbeddedType ContainingType { get; }

		protected abstract TypeMemberVisibility Visibility { get; }

		protected abstract string Name { get; }

		public TEventSymbol UnderlyingEvent => UnderlyingSymbol;

		IMethodReference IEventDefinition.Adder => _adder;

		IMethodReference IEventDefinition.Remover => _remover;

		IMethodReference IEventDefinition.Caller => _caller;

		bool IEventDefinition.IsRuntimeSpecial => IsRuntimeSpecial;

		bool IEventDefinition.IsSpecialName => IsSpecialName;

		protected TEmbeddedMethod AnAccessor => _adder ?? _remover;

		ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

		TypeMemberVisibility ITypeDefinitionMember.Visibility => Visibility;

		string INamedEntity.Name => Name;

		protected CommonEmbeddedEvent(TEventSymbol underlyingEvent, TEmbeddedMethod adder, TEmbeddedMethod remover, TEmbeddedMethod caller)
			: base(underlyingEvent)
		{
			_adder = adder;
			_remover = remover;
			_caller = caller;
		}

		protected abstract ITypeReference GetType(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

		protected abstract void EmbedCorrespondingComEventInterfaceMethodInternal(TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding);

		internal void EmbedCorrespondingComEventInterfaceMethod(TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding)
		{
			if (_isUsedForComAwareEventBinding == 0 && (!isUsedForComAwareEventBinding || Interlocked.CompareExchange(ref _isUsedForComAwareEventBinding, 1, 0) == 0))
			{
				EmbedCorrespondingComEventInterfaceMethodInternal(syntaxNodeOpt, diagnostics, isUsedForComAwareEventBinding);
			}
		}

		IEnumerable<IMethodReference> IEventDefinition.GetAccessors(EmitContext context)
		{
			if (_adder != null)
			{
				yield return _adder;
			}
			if (_remover != null)
			{
				yield return _remover;
			}
			if (_caller != null)
			{
				yield return _caller;
			}
		}

		ITypeReference IEventDefinition.GetType(EmitContext context)
		{
			return GetType((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		}

		ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
		{
			return ContainingType;
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return this;
		}
	}

	internal abstract class CommonEmbeddedField : CommonEmbeddedMember<TFieldSymbol>, IFieldDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IFieldReference
	{
		public readonly TEmbeddedType ContainingType;

		public TFieldSymbol UnderlyingField => UnderlyingSymbol;

		protected abstract bool IsCompileTimeConstant { get; }

		protected abstract bool IsNotSerialized { get; }

		protected abstract bool IsReadOnly { get; }

		protected abstract bool IsRuntimeSpecial { get; }

		protected abstract bool IsSpecialName { get; }

		protected abstract bool IsStatic { get; }

		protected abstract bool IsMarshalledExplicitly { get; }

		protected abstract IMarshallingInformation MarshallingInformation { get; }

		protected abstract ImmutableArray<byte> MarshallingDescriptor { get; }

		protected abstract int? TypeLayoutOffset { get; }

		protected abstract TypeMemberVisibility Visibility { get; }

		protected abstract string Name { get; }

		ImmutableArray<byte> IFieldDefinition.MappedData => default;

		bool IFieldDefinition.IsCompileTimeConstant => IsCompileTimeConstant;

		bool IFieldDefinition.IsNotSerialized => IsNotSerialized;

		bool IFieldDefinition.IsReadOnly => IsReadOnly;

		bool IFieldDefinition.IsRuntimeSpecial => IsRuntimeSpecial;

		bool IFieldDefinition.IsSpecialName => IsSpecialName;

		bool IFieldDefinition.IsStatic => IsStatic;

		bool IFieldDefinition.IsMarshalledExplicitly => IsMarshalledExplicitly;

		IMarshallingInformation IFieldDefinition.MarshallingInformation => MarshallingInformation;

		ImmutableArray<byte> IFieldDefinition.MarshallingDescriptor => MarshallingDescriptor;

		int IFieldDefinition.Offset => TypeLayoutOffset ?? 0;

		ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

		TypeMemberVisibility ITypeDefinitionMember.Visibility => Visibility;

		string INamedEntity.Name => Name;

		ISpecializedFieldReference IFieldReference.AsSpecializedFieldReference => null;

		bool IFieldReference.IsContextualNamedEntity => false;

		protected CommonEmbeddedField(TEmbeddedType containingType, TFieldSymbol underlyingField)
			: base(underlyingField)
		{
			ContainingType = containingType;
		}

		protected abstract MetadataConstant GetCompileTimeValue(EmitContext context);

		MetadataConstant IFieldDefinition.GetCompileTimeValue(EmitContext context)
		{
			return GetCompileTimeValue(context);
		}

		ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
		{
			return ContainingType;
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return this;
		}

		ITypeReference IFieldReference.GetType(EmitContext context)
		{
			return UnderlyingField.GetType(context);
		}

		IFieldDefinition IFieldReference.GetResolvedField(EmitContext context)
		{
			return this;
		}
	}

	internal abstract class CommonEmbeddedMember
	{
		internal abstract TEmbeddedTypesManager TypeManager { get; }
	}

	internal abstract class CommonEmbeddedMember<TMember> : CommonEmbeddedMember, IReference where TMember : TSymbol, ITypeMemberReference
	{
		protected readonly TMember UnderlyingSymbol;

		private ImmutableArray<TAttributeData> _lazyAttributes;

		protected CommonEmbeddedMember(TMember underlyingSymbol)
		{
			UnderlyingSymbol = underlyingSymbol;
		}

		protected abstract IEnumerable<TAttributeData> GetCustomAttributesToEmit(TPEModuleBuilder moduleBuilder);

		protected virtual TAttributeData PortAttributeIfNeedTo(TAttributeData attrData, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			return null;
		}

		private ImmutableArray<TAttributeData> GetAttributes(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			ArrayBuilder<TAttributeData> instance = ArrayBuilder<TAttributeData>.GetInstance();
			foreach (TAttributeData item in GetCustomAttributesToEmit(moduleBuilder))
			{
				if (TypeManager.IsTargetAttribute((TSymbol)(object)UnderlyingSymbol, item, AttributeDescription.DispIdAttribute))
				{
					if (item.CommonConstructorArguments.Length == 1)
					{
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_DispIdAttribute__ctor, item, syntaxNodeOpt, diagnostics));
					}
				}
				else
				{
					instance.AddOptional(PortAttributeIfNeedTo(item, syntaxNodeOpt, diagnostics));
				}
			}
			return instance.ToImmutableAndFree();
		}

		IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
		{
			if (_lazyAttributes.IsDefault)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				ImmutableArray<TAttributeData> attributes = GetAttributes((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, instance);
				if (ImmutableInterlocked.InterlockedInitialize(ref _lazyAttributes, attributes))
				{
					context.Diagnostics.AddRange(instance);
				}
				instance.Free();
			}
			return _lazyAttributes;
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			throw ExceptionUtilities.Unreachable;
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal abstract class CommonEmbeddedMethod : CommonEmbeddedMember<TMethodSymbol>, IMethodDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IMethodReference, ISignature
	{
		private sealed class EmptyBody : IMethodBody
		{
			private readonly CommonEmbeddedMethod _method;

			ImmutableArray<ExceptionHandlerRegion> IMethodBody.ExceptionRegions => ImmutableArray<ExceptionHandlerRegion>.Empty;

			bool IMethodBody.LocalsAreZeroed => false;

			ImmutableArray<ILocalDefinition> IMethodBody.LocalVariables => ImmutableArray<ILocalDefinition>.Empty;

			IMethodDefinition IMethodBody.MethodDefinition => _method;

			ushort IMethodBody.MaxStack => 0;

			ImmutableArray<byte> IMethodBody.IL => ImmutableArray<byte>.Empty;

			ImmutableArray<Microsoft.Cci.SequencePoint> IMethodBody.SequencePoints => ImmutableArray<Microsoft.Cci.SequencePoint>.Empty;

			bool IMethodBody.HasDynamicLocalVariables => false;

			StateMachineMoveNextBodyDebugInfo IMethodBody.MoveNextBodyInfo => null;

			DynamicAnalysisMethodBodyData IMethodBody.DynamicAnalysisData => null;

			ImmutableArray<Microsoft.Cci.LocalScope> IMethodBody.LocalScopes => ImmutableArray<Microsoft.Cci.LocalScope>.Empty;

			IImportScope IMethodBody.ImportScope => null;

			ImmutableArray<StateMachineHoistedLocalScope> IMethodBody.StateMachineHoistedLocalScopes => default;

			string IMethodBody.StateMachineTypeName => null;

			ImmutableArray<EncHoistedLocalInfo> IMethodBody.StateMachineHoistedLocalSlots => default;

			ImmutableArray<ITypeReference> IMethodBody.StateMachineAwaiterSlots => default;

			ImmutableArray<ClosureDebugInfo> IMethodBody.ClosureDebugInfo => default;

			ImmutableArray<LambdaDebugInfo> IMethodBody.LambdaDebugInfo => default;

			public DebugId MethodId => default;

			public EmptyBody(CommonEmbeddedMethod method)
			{
				_method = method;
			}
		}

		public readonly TEmbeddedType ContainingType;

		private readonly ImmutableArray<TEmbeddedTypeParameter> _typeParameters;

		private readonly ImmutableArray<TEmbeddedParameter> _parameters;

		protected abstract bool IsAbstract { get; }

		protected abstract bool IsAccessCheckedOnOverride { get; }

		protected abstract bool IsConstructor { get; }

		protected abstract bool IsExternal { get; }

		protected abstract bool IsHiddenBySignature { get; }

		protected abstract bool IsNewSlot { get; }

		protected abstract IPlatformInvokeInformation PlatformInvokeData { get; }

		protected abstract bool IsRuntimeSpecial { get; }

		protected abstract bool IsSpecialName { get; }

		protected abstract bool IsSealed { get; }

		protected abstract bool IsStatic { get; }

		protected abstract bool IsVirtual { get; }

		protected abstract bool ReturnValueIsMarshalledExplicitly { get; }

		protected abstract IMarshallingInformation ReturnValueMarshallingInformation { get; }

		protected abstract ImmutableArray<byte> ReturnValueMarshallingDescriptor { get; }

		protected abstract TypeMemberVisibility Visibility { get; }

		protected abstract string Name { get; }

		protected abstract bool AcceptsExtraArguments { get; }

		protected abstract ISignature UnderlyingMethodSignature { get; }

		protected abstract INamespace ContainingNamespace { get; }

		public TMethodSymbol UnderlyingMethod => UnderlyingSymbol;

		IEnumerable<IGenericMethodParameter> IMethodDefinition.GenericParameters => _typeParameters;

		bool IMethodDefinition.IsImplicitlyDeclared => true;

		bool IMethodDefinition.HasDeclarativeSecurity => false;

		bool IMethodDefinition.IsAbstract => IsAbstract;

		bool IMethodDefinition.IsAccessCheckedOnOverride => IsAccessCheckedOnOverride;

		bool IMethodDefinition.IsConstructor => IsConstructor;

		bool IMethodDefinition.IsExternal => IsExternal;

		bool IMethodDefinition.IsHiddenBySignature => IsHiddenBySignature;

		bool IMethodDefinition.IsNewSlot => IsNewSlot;

		bool IMethodDefinition.IsPlatformInvoke => PlatformInvokeData != null;

		IPlatformInvokeInformation IMethodDefinition.PlatformInvokeData => PlatformInvokeData;

		bool IMethodDefinition.IsRuntimeSpecial => IsRuntimeSpecial;

		bool IMethodDefinition.IsSpecialName => IsSpecialName;

		bool IMethodDefinition.IsSealed => IsSealed;

		bool IMethodDefinition.IsStatic => IsStatic;

		bool IMethodDefinition.IsVirtual => IsVirtual;

		ImmutableArray<IParameterDefinition> IMethodDefinition.Parameters => StaticCast<IParameterDefinition>.From(_parameters);

		bool IMethodDefinition.RequiresSecurityObject => false;

		bool IMethodDefinition.ReturnValueIsMarshalledExplicitly => ReturnValueIsMarshalledExplicitly;

		IMarshallingInformation IMethodDefinition.ReturnValueMarshallingInformation => ReturnValueMarshallingInformation;

		ImmutableArray<byte> IMethodDefinition.ReturnValueMarshallingDescriptor => ReturnValueMarshallingDescriptor;

		IEnumerable<SecurityAttribute> IMethodDefinition.SecurityAttributes => SpecializedCollections.EmptyEnumerable<SecurityAttribute>();

		ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

		INamespace IMethodDefinition.ContainingNamespace => ContainingNamespace;

		TypeMemberVisibility ITypeDefinitionMember.Visibility => Visibility;

		string INamedEntity.Name => Name;

		bool IMethodReference.AcceptsExtraArguments => AcceptsExtraArguments;

		ushort IMethodReference.GenericParameterCount => (ushort)_typeParameters.Length;

		bool IMethodReference.IsGeneric => _typeParameters.Length > 0;

		ImmutableArray<IParameterTypeInformation> IMethodReference.ExtraParameters => ImmutableArray<IParameterTypeInformation>.Empty;

		IGenericMethodInstanceReference IMethodReference.AsGenericMethodInstanceReference => null;

		ISpecializedMethodReference IMethodReference.AsSpecializedMethodReference => null;

		Microsoft.Cci.CallingConvention ISignature.CallingConvention => UnderlyingMethodSignature.CallingConvention;

		ushort ISignature.ParameterCount => (ushort)_parameters.Length;

		ImmutableArray<ICustomModifier> ISignature.RefCustomModifiers => UnderlyingMethodSignature.RefCustomModifiers;

		ImmutableArray<ICustomModifier> ISignature.ReturnValueCustomModifiers => UnderlyingMethodSignature.ReturnValueCustomModifiers;

		bool ISignature.ReturnValueIsByRef => UnderlyingMethodSignature.ReturnValueIsByRef;

		protected CommonEmbeddedMethod(TEmbeddedType containingType, TMethodSymbol underlyingMethod)
			: base(underlyingMethod)
		{
			ContainingType = containingType;
			_typeParameters = GetTypeParameters();
			_parameters = GetParameters();
		}

		protected abstract ImmutableArray<TEmbeddedTypeParameter> GetTypeParameters();

		protected abstract ImmutableArray<TEmbeddedParameter> GetParameters();

		protected abstract MethodImplAttributes GetImplementationAttributes(EmitContext context);

		protected sealed override TAttributeData PortAttributeIfNeedTo(TAttributeData attrData, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			if (TypeManager.IsTargetAttribute((TSymbol)UnderlyingMethod, attrData, AttributeDescription.LCIDConversionAttribute) && attrData.CommonConstructorArguments.Length == 1)
			{
				return TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_LCIDConversionAttribute__ctor, attrData, syntaxNodeOpt, diagnostics);
			}
			return null;
		}

		IMethodBody IMethodDefinition.GetBody(EmitContext context)
		{
			if (this.HasBody())
			{
				return new EmptyBody(this);
			}
			return null;
		}

		MethodImplAttributes IMethodDefinition.GetImplementationAttributes(EmitContext context)
		{
			return GetImplementationAttributes(context);
		}

		IEnumerable<ICustomAttribute> IMethodDefinition.GetReturnValueAttributes(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
		}

		ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
		{
			return ContainingType;
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return this;
		}

		IMethodDefinition IMethodReference.GetResolvedMethod(EmitContext context)
		{
			return this;
		}

		ImmutableArray<IParameterTypeInformation> ISignature.GetParameters(EmitContext context)
		{
			return StaticCast<IParameterTypeInformation>.From(_parameters);
		}

		ITypeReference ISignature.GetType(EmitContext context)
		{
			return UnderlyingMethodSignature.GetType(context);
		}

		public override string ToString()
		{
			return ((ISymbol)UnderlyingMethod).ToDisplayString(SymbolDisplayFormat.ILVisualizationFormat);
		}
	}

	internal abstract class CommonEmbeddedParameter : IParameterDefinition, IDefinition, IReference, INamedEntity, IParameterTypeInformation, IParameterListEntry
	{
		public readonly CommonEmbeddedMember ContainingPropertyOrMethod;

		public readonly TParameterSymbol UnderlyingParameter;

		private ImmutableArray<TAttributeData> _lazyAttributes;

		protected TEmbeddedTypesManager TypeManager => ContainingPropertyOrMethod.TypeManager;

		protected abstract bool HasDefaultValue { get; }

		protected abstract bool IsIn { get; }

		protected abstract bool IsOut { get; }

		protected abstract bool IsOptional { get; }

		protected abstract bool IsMarshalledExplicitly { get; }

		protected abstract IMarshallingInformation MarshallingInformation { get; }

		protected abstract ImmutableArray<byte> MarshallingDescriptor { get; }

		protected abstract string Name { get; }

		protected abstract IParameterTypeInformation UnderlyingParameterTypeInformation { get; }

		protected abstract ushort Index { get; }

		bool IParameterDefinition.HasDefaultValue => HasDefaultValue;

		bool IParameterDefinition.IsIn => IsIn;

		bool IParameterDefinition.IsOut => IsOut;

		bool IParameterDefinition.IsOptional => IsOptional;

		bool IParameterDefinition.IsMarshalledExplicitly => IsMarshalledExplicitly;

		IMarshallingInformation IParameterDefinition.MarshallingInformation => MarshallingInformation;

		ImmutableArray<byte> IParameterDefinition.MarshallingDescriptor => MarshallingDescriptor;

		string INamedEntity.Name => Name;

		ImmutableArray<ICustomModifier> IParameterTypeInformation.CustomModifiers => UnderlyingParameterTypeInformation.CustomModifiers;

		bool IParameterTypeInformation.IsByReference => UnderlyingParameterTypeInformation.IsByReference;

		ImmutableArray<ICustomModifier> IParameterTypeInformation.RefCustomModifiers => UnderlyingParameterTypeInformation.RefCustomModifiers;

		ushort IParameterListEntry.Index => Index;

		protected CommonEmbeddedParameter(CommonEmbeddedMember containingPropertyOrMethod, TParameterSymbol underlyingParameter)
		{
			ContainingPropertyOrMethod = containingPropertyOrMethod;
			UnderlyingParameter = underlyingParameter;
		}

		protected abstract MetadataConstant GetDefaultValue(EmitContext context);

		protected abstract IEnumerable<TAttributeData> GetCustomAttributesToEmit(TPEModuleBuilder moduleBuilder);

		private bool IsTargetAttribute(TAttributeData attrData, AttributeDescription description)
		{
			return TypeManager.IsTargetAttribute((TSymbol)UnderlyingParameter, attrData, description);
		}

		private ImmutableArray<TAttributeData> GetAttributes(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			ArrayBuilder<TAttributeData> instance = ArrayBuilder<TAttributeData>.GetInstance();
			foreach (TAttributeData item in GetCustomAttributesToEmit(moduleBuilder))
			{
				if (IsTargetAttribute(item, AttributeDescription.ParamArrayAttribute))
				{
					if (item.CommonConstructorArguments.Length == 0)
					{
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_ParamArrayAttribute__ctor, item, syntaxNodeOpt, diagnostics));
					}
					continue;
				}
				if (IsTargetAttribute(item, AttributeDescription.DateTimeConstantAttribute))
				{
					if (item.CommonConstructorArguments.Length == 1)
					{
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_CompilerServices_DateTimeConstantAttribute__ctor, item, syntaxNodeOpt, diagnostics));
					}
					continue;
				}
				int targetAttributeSignatureIndex = TypeManager.GetTargetAttributeSignatureIndex((TSymbol)UnderlyingParameter, item, AttributeDescription.DecimalConstantAttribute);
				if (targetAttributeSignatureIndex != -1)
				{
					if (item.CommonConstructorArguments.Length == 5)
					{
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute((targetAttributeSignatureIndex == 0) ? WellKnownMember.System_Runtime_CompilerServices_DecimalConstantAttribute__ctor : WellKnownMember.System_Runtime_CompilerServices_DecimalConstantAttribute__ctorByteByteInt32Int32Int32, item, syntaxNodeOpt, diagnostics));
					}
				}
				else if (IsTargetAttribute(item, AttributeDescription.DefaultParameterValueAttribute) && item.CommonConstructorArguments.Length == 1)
				{
					instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_DefaultParameterValueAttribute__ctor, item, syntaxNodeOpt, diagnostics));
				}
			}
			return instance.ToImmutableAndFree();
		}

		MetadataConstant IParameterDefinition.GetDefaultValue(EmitContext context)
		{
			return GetDefaultValue(context);
		}

		IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
		{
			if (_lazyAttributes.IsDefault)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				ImmutableArray<TAttributeData> attributes = GetAttributes((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, instance);
				if (ImmutableInterlocked.InterlockedInitialize(ref _lazyAttributes, attributes))
				{
					context.Diagnostics.AddRange(instance);
				}
				instance.Free();
			}
			return _lazyAttributes;
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			throw ExceptionUtilities.Unreachable;
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return this;
		}

		ITypeReference IParameterTypeInformation.GetType(EmitContext context)
		{
			return UnderlyingParameterTypeInformation.GetType(context);
		}

		public override string ToString()
		{
			return ((ISymbol)UnderlyingParameter).ToDisplayString(SymbolDisplayFormat.ILVisualizationFormat);
		}
	}

	internal abstract class CommonEmbeddedProperty : CommonEmbeddedMember<TPropertySymbol>, IPropertyDefinition, ISignature, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition
	{
		private readonly ImmutableArray<TEmbeddedParameter> _parameters;

		private readonly TEmbeddedMethod _getter;

		private readonly TEmbeddedMethod _setter;

		internal override TEmbeddedTypesManager TypeManager => AnAccessor.TypeManager;

		protected abstract bool IsRuntimeSpecial { get; }

		protected abstract bool IsSpecialName { get; }

		protected abstract ISignature UnderlyingPropertySignature { get; }

		protected abstract TEmbeddedType ContainingType { get; }

		protected abstract TypeMemberVisibility Visibility { get; }

		protected abstract string Name { get; }

		public TPropertySymbol UnderlyingProperty => UnderlyingSymbol;

		IMethodReference IPropertyDefinition.Getter => _getter;

		IMethodReference IPropertyDefinition.Setter => _setter;

		bool IPropertyDefinition.HasDefaultValue => false;

		MetadataConstant IPropertyDefinition.DefaultValue => null;

		bool IPropertyDefinition.IsRuntimeSpecial => IsRuntimeSpecial;

		bool IPropertyDefinition.IsSpecialName => IsSpecialName;

		ImmutableArray<IParameterDefinition> IPropertyDefinition.Parameters => StaticCast<IParameterDefinition>.From(_parameters);

		Microsoft.Cci.CallingConvention ISignature.CallingConvention => UnderlyingPropertySignature.CallingConvention;

		ushort ISignature.ParameterCount => (ushort)_parameters.Length;

		ImmutableArray<ICustomModifier> ISignature.ReturnValueCustomModifiers => UnderlyingPropertySignature.ReturnValueCustomModifiers;

		ImmutableArray<ICustomModifier> ISignature.RefCustomModifiers => UnderlyingPropertySignature.RefCustomModifiers;

		bool ISignature.ReturnValueIsByRef => UnderlyingPropertySignature.ReturnValueIsByRef;

		protected TEmbeddedMethod AnAccessor => _getter ?? _setter;

		ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

		TypeMemberVisibility ITypeDefinitionMember.Visibility => Visibility;

		string INamedEntity.Name => Name;

		protected CommonEmbeddedProperty(TPropertySymbol underlyingProperty, TEmbeddedMethod getter, TEmbeddedMethod setter)
			: base(underlyingProperty)
		{
			_getter = getter;
			_setter = setter;
			_parameters = GetParameters();
		}

		protected abstract ImmutableArray<TEmbeddedParameter> GetParameters();

		IEnumerable<IMethodReference> IPropertyDefinition.GetAccessors(EmitContext context)
		{
			if (_getter != null)
			{
				yield return _getter;
			}
			if (_setter != null)
			{
				yield return _setter;
			}
		}

		ImmutableArray<IParameterTypeInformation> ISignature.GetParameters(EmitContext context)
		{
			return StaticCast<IParameterTypeInformation>.From(_parameters);
		}

		ITypeReference ISignature.GetType(EmitContext context)
		{
			return UnderlyingPropertySignature.GetType(context);
		}

		ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
		{
			return ContainingType;
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			visitor.Visit(this);
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return this;
		}
	}

	internal abstract class CommonEmbeddedType : INamespaceTypeDefinition, INamedTypeDefinition, ITypeDefinition, IDefinition, IReference, ITypeReference, INamedTypeReference, INamedEntity, INamespaceTypeReference
	{
		public readonly TEmbeddedTypesManager TypeManager;

		public readonly TNamedTypeSymbol UnderlyingNamedType;

		private ImmutableArray<IFieldDefinition> _lazyFields;

		private ImmutableArray<IMethodDefinition> _lazyMethods;

		private ImmutableArray<IPropertyDefinition> _lazyProperties;

		private ImmutableArray<IEventDefinition> _lazyEvents;

		private ImmutableArray<TAttributeData> _lazyAttributes;

		private int _lazyAssemblyRefIndex = -1;

		protected abstract bool IsPublic { get; }

		protected abstract bool IsAbstract { get; }

		protected abstract bool IsBeforeFieldInit { get; }

		protected abstract bool IsComImport { get; }

		protected abstract bool IsInterface { get; }

		protected abstract bool IsSerializable { get; }

		protected abstract bool IsSpecialName { get; }

		protected abstract bool IsWindowsRuntimeImport { get; }

		protected abstract bool IsSealed { get; }

		protected abstract CharSet StringFormat { get; }

		public int AssemblyRefIndex
		{
			get
			{
				if (_lazyAssemblyRefIndex == -1)
				{
					_lazyAssemblyRefIndex = GetAssemblyRefIndex();
				}
				return _lazyAssemblyRefIndex;
			}
		}

		bool INamespaceTypeDefinition.IsPublic => IsPublic;

		IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters => SpecializedCollections.EmptyEnumerable<IGenericTypeParameter>();

		ushort ITypeDefinition.GenericParameterCount => 0;

		bool ITypeDefinition.HasDeclarativeSecurity => false;

		bool ITypeDefinition.IsAbstract => IsAbstract;

		bool ITypeDefinition.IsBeforeFieldInit => IsBeforeFieldInit;

		bool ITypeDefinition.IsComObject
		{
			get
			{
				if (!IsInterface)
				{
					return IsComImport;
				}
				return true;
			}
		}

		bool ITypeDefinition.IsGeneric => false;

		bool ITypeDefinition.IsInterface => IsInterface;

		bool ITypeDefinition.IsRuntimeSpecial => false;

		bool ITypeDefinition.IsSerializable => IsSerializable;

		bool ITypeDefinition.IsSpecialName => IsSpecialName;

		bool ITypeDefinition.IsWindowsRuntimeImport => IsWindowsRuntimeImport;

		bool ITypeDefinition.IsSealed => IsSealed;

		LayoutKind ITypeDefinition.Layout => GetTypeLayoutIfStruct()?.Kind ?? LayoutKind.Auto;

		ushort ITypeDefinition.Alignment => (ushort)(GetTypeLayoutIfStruct()?.Alignment ?? 0);

		uint ITypeDefinition.SizeOf => (uint)(GetTypeLayoutIfStruct()?.Size ?? 0);

		IEnumerable<SecurityAttribute> ITypeDefinition.SecurityAttributes => SpecializedCollections.EmptyEnumerable<SecurityAttribute>();

		CharSet ITypeDefinition.StringFormat => StringFormat;

		bool ITypeReference.IsEnum => UnderlyingNamedType.IsEnum;

		bool ITypeReference.IsValueType => UnderlyingNamedType.IsValueType;

		Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode => Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;

		TypeDefinitionHandle ITypeReference.TypeDef => default;

		IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => null;

		IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference => null;

		IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

		INamespaceTypeReference ITypeReference.AsNamespaceTypeReference => this;

		INestedTypeReference ITypeReference.AsNestedTypeReference => null;

		ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference => null;

		ushort INamedTypeReference.GenericParameterCount => 0;

		bool INamedTypeReference.MangleName => UnderlyingNamedType.MangleName;

		string INamedEntity.Name => UnderlyingNamedType.Name;

		string INamespaceTypeReference.NamespaceName => UnderlyingNamedType.NamespaceName;

		protected CommonEmbeddedType(TEmbeddedTypesManager typeManager, TNamedTypeSymbol underlyingNamedType)
		{
			TypeManager = typeManager;
			UnderlyingNamedType = underlyingNamedType;
		}

		protected abstract int GetAssemblyRefIndex();

		protected abstract IEnumerable<TFieldSymbol> GetFieldsToEmit();

		protected abstract IEnumerable<TMethodSymbol> GetMethodsToEmit();

		protected abstract IEnumerable<TEventSymbol> GetEventsToEmit();

		protected abstract IEnumerable<TPropertySymbol> GetPropertiesToEmit();

		protected abstract ITypeReference GetBaseClass(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

		protected abstract IEnumerable<TypeReferenceWithAttributes> GetInterfaces(EmitContext context);

		protected abstract TypeLayout? GetTypeLayoutIfStruct();

		protected abstract TAttributeData CreateTypeIdentifierAttribute(bool hasGuid, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

		protected abstract void EmbedDefaultMembers(string defaultMember, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

		protected abstract IEnumerable<TAttributeData> GetCustomAttributesToEmit(TPEModuleBuilder moduleBuilder);

		protected abstract void ReportMissingAttribute(AttributeDescription description, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

		private bool IsTargetAttribute(TAttributeData attrData, AttributeDescription description)
		{
			return TypeManager.IsTargetAttribute((TSymbol)UnderlyingNamedType, attrData, description);
		}

		private ImmutableArray<TAttributeData> GetAttributes(TPEModuleBuilder moduleBuilder, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
		{
			ArrayBuilder<TAttributeData> instance = ArrayBuilder<TAttributeData>.GetInstance();
			instance.AddOptional(TypeManager.CreateCompilerGeneratedAttribute());
			bool flag = false;
			bool flag2 = false;
			foreach (TAttributeData item in GetCustomAttributesToEmit(moduleBuilder))
			{
				if (IsTargetAttribute(item, AttributeDescription.GuidAttribute))
				{
					if (item.TryGetGuidAttributeValue(out var _))
					{
						flag = true;
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_GuidAttribute__ctor, item, syntaxNodeOpt, diagnostics));
					}
					continue;
				}
				if (IsTargetAttribute(item, AttributeDescription.ComEventInterfaceAttribute))
				{
					if (item.CommonConstructorArguments.Length == 2)
					{
						flag2 = true;
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_ComEventInterfaceAttribute__ctor, item, syntaxNodeOpt, diagnostics));
					}
					continue;
				}
				int targetAttributeSignatureIndex = TypeManager.GetTargetAttributeSignatureIndex((TSymbol)UnderlyingNamedType, item, AttributeDescription.InterfaceTypeAttribute);
				if (targetAttributeSignatureIndex != -1)
				{
					if (item.CommonConstructorArguments.Length == 1)
					{
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute((targetAttributeSignatureIndex == 0) ? WellKnownMember.System_Runtime_InteropServices_InterfaceTypeAttribute__ctorInt16 : WellKnownMember.System_Runtime_InteropServices_InterfaceTypeAttribute__ctorComInterfaceType, item, syntaxNodeOpt, diagnostics));
					}
				}
				else if (IsTargetAttribute(item, AttributeDescription.BestFitMappingAttribute))
				{
					if (item.CommonConstructorArguments.Length == 1)
					{
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_BestFitMappingAttribute__ctor, item, syntaxNodeOpt, diagnostics));
					}
				}
				else if (IsTargetAttribute(item, AttributeDescription.CoClassAttribute))
				{
					if (item.CommonConstructorArguments.Length == 1)
					{
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_CoClassAttribute__ctor, item, syntaxNodeOpt, diagnostics));
					}
				}
				else if (IsTargetAttribute(item, AttributeDescription.FlagsAttribute))
				{
					if (item.CommonConstructorArguments.Length == 0 && UnderlyingNamedType.IsEnum)
					{
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_FlagsAttribute__ctor, item, syntaxNodeOpt, diagnostics));
					}
				}
				else if (IsTargetAttribute(item, AttributeDescription.DefaultMemberAttribute))
				{
					if (item.CommonConstructorArguments.Length == 1)
					{
						instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Reflection_DefaultMemberAttribute__ctor, item, syntaxNodeOpt, diagnostics));
						if (item.CommonConstructorArguments[0].Value is string defaultMember)
						{
							EmbedDefaultMembers(defaultMember, syntaxNodeOpt, diagnostics);
						}
					}
				}
				else if (IsTargetAttribute(item, AttributeDescription.UnmanagedFunctionPointerAttribute) && item.CommonConstructorArguments.Length == 1)
				{
					instance.AddOptional(TypeManager.CreateSynthesizedAttribute(WellKnownMember.System_Runtime_InteropServices_UnmanagedFunctionPointerAttribute__ctor, item, syntaxNodeOpt, diagnostics));
				}
			}
			if (IsInterface && !flag2)
			{
				if (!IsComImport)
				{
					ReportMissingAttribute(AttributeDescription.ComImportAttribute, syntaxNodeOpt, diagnostics);
				}
				else if (!flag)
				{
					ReportMissingAttribute(AttributeDescription.GuidAttribute, syntaxNodeOpt, diagnostics);
				}
			}
			instance.AddOptional(CreateTypeIdentifierAttribute(flag && IsInterface, syntaxNodeOpt, diagnostics));
			return instance.ToImmutableAndFree();
		}

		ITypeReference ITypeDefinition.GetBaseClass(EmitContext context)
		{
			return GetBaseClass((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		}

		IEnumerable<IEventDefinition> ITypeDefinition.GetEvents(EmitContext context)
		{
			if (_lazyEvents.IsDefault)
			{
				ArrayBuilder<IEventDefinition> instance = ArrayBuilder<IEventDefinition>.GetInstance();
				foreach (TEventSymbol item in GetEventsToEmit())
				{
					if (TypeManager.EmbeddedEventsMap.TryGetValue(item, out var value))
					{
						instance.Add(value);
					}
				}
				ImmutableInterlocked.InterlockedInitialize(ref _lazyEvents, instance.ToImmutableAndFree());
			}
			return _lazyEvents;
		}

		IEnumerable<Microsoft.Cci.MethodImplementation> ITypeDefinition.GetExplicitImplementationOverrides(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<Microsoft.Cci.MethodImplementation>();
		}

		IEnumerable<IFieldDefinition> ITypeDefinition.GetFields(EmitContext context)
		{
			if (_lazyFields.IsDefault)
			{
				ArrayBuilder<IFieldDefinition> instance = ArrayBuilder<IFieldDefinition>.GetInstance();
				foreach (TFieldSymbol item in GetFieldsToEmit())
				{
					if (TypeManager.EmbeddedFieldsMap.TryGetValue(item, out var value))
					{
						instance.Add(value);
					}
				}
				ImmutableInterlocked.InterlockedInitialize(ref _lazyFields, instance.ToImmutableAndFree());
			}
			return _lazyFields;
		}

		IEnumerable<TypeReferenceWithAttributes> ITypeDefinition.Interfaces(EmitContext context)
		{
			return GetInterfaces(context);
		}

		IEnumerable<IMethodDefinition> ITypeDefinition.GetMethods(EmitContext context)
		{
			if (_lazyMethods.IsDefault)
			{
				ArrayBuilder<IMethodDefinition> instance = ArrayBuilder<IMethodDefinition>.GetInstance();
				int num = 1;
				int num2 = 0;
				foreach (TMethodSymbol item in GetMethodsToEmit())
				{
					if (item != null)
					{
						if (TypeManager.EmbeddedMethodsMap.TryGetValue(item, out var value))
						{
							if (num2 > 0)
							{
								instance.Add(new VtblGap(this, ModuleExtensions.GetVTableGapName(num, num2)));
								num++;
								num2 = 0;
							}
							instance.Add(value);
						}
						else
						{
							num2++;
						}
					}
					else
					{
						num2++;
					}
				}
				ImmutableInterlocked.InterlockedInitialize(ref _lazyMethods, instance.ToImmutableAndFree());
			}
			return _lazyMethods;
		}

		IEnumerable<INestedTypeDefinition> ITypeDefinition.GetNestedTypes(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<INestedTypeDefinition>();
		}

		IEnumerable<IPropertyDefinition> ITypeDefinition.GetProperties(EmitContext context)
		{
			if (_lazyProperties.IsDefault)
			{
				ArrayBuilder<IPropertyDefinition> instance = ArrayBuilder<IPropertyDefinition>.GetInstance();
				foreach (TPropertySymbol item in GetPropertiesToEmit())
				{
					if (TypeManager.EmbeddedPropertiesMap.TryGetValue(item, out var value))
					{
						instance.Add(value);
					}
				}
				ImmutableInterlocked.InterlockedInitialize(ref _lazyProperties, instance.ToImmutableAndFree());
			}
			return _lazyProperties;
		}

		IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
		{
			if (_lazyAttributes.IsDefault)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				ImmutableArray<TAttributeData> attributes = GetAttributes((TPEModuleBuilder)context.Module, (TSyntaxNode)context.SyntaxNodeOpt, instance);
				if (ImmutableInterlocked.InterlockedInitialize(ref _lazyAttributes, attributes))
				{
					context.Diagnostics.AddRange(instance);
				}
				instance.Free();
			}
			return _lazyAttributes;
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			throw ExceptionUtilities.Unreachable;
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return this;
		}

		ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
		{
			return this;
		}

		INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
		{
			return this;
		}

		INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
		{
			return null;
		}

		ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
		{
			return this;
		}

		IUnitReference INamespaceTypeReference.GetUnit(EmitContext context)
		{
			return TypeManager.ModuleBeingBuilt;
		}

		public override string ToString()
		{
			return ((ISymbol)UnderlyingNamedType).ToDisplayString(SymbolDisplayFormat.ILVisualizationFormat);
		}
	}

	internal abstract class CommonEmbeddedTypeParameter : IGenericMethodParameter, IGenericParameter, IGenericParameterReference, ITypeReference, IReference, INamedEntity, IParameterListEntry, IGenericMethodParameterReference
	{
		public readonly TEmbeddedMethod ContainingMethod;

		public readonly TTypeParameterSymbol UnderlyingTypeParameter;

		protected abstract bool MustBeReferenceType { get; }

		protected abstract bool MustBeValueType { get; }

		protected abstract bool MustHaveDefaultConstructor { get; }

		protected abstract string Name { get; }

		protected abstract ushort Index { get; }

		IMethodDefinition IGenericMethodParameter.DefiningMethod => ContainingMethod;

		bool IGenericParameter.MustBeReferenceType => MustBeReferenceType;

		bool IGenericParameter.MustBeValueType => MustBeValueType;

		bool IGenericParameter.MustHaveDefaultConstructor => MustHaveDefaultConstructor;

		TypeParameterVariance IGenericParameter.Variance => TypeParameterVariance.NonVariant;

		IGenericMethodParameter IGenericParameter.AsGenericMethodParameter => this;

		IGenericTypeParameter IGenericParameter.AsGenericTypeParameter => null;

		bool ITypeReference.IsEnum => false;

		bool ITypeReference.IsValueType => false;

		Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode => Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;

		TypeDefinitionHandle ITypeReference.TypeDef => default;

		IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => this;

		IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference => null;

		IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

		INamespaceTypeReference ITypeReference.AsNamespaceTypeReference => null;

		INestedTypeReference ITypeReference.AsNestedTypeReference => null;

		ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference => null;

		string INamedEntity.Name => Name;

		ushort IParameterListEntry.Index => Index;

		IMethodReference IGenericMethodParameterReference.DefiningMethod => ContainingMethod;

		protected CommonEmbeddedTypeParameter(TEmbeddedMethod containingMethod, TTypeParameterSymbol underlyingTypeParameter)
		{
			ContainingMethod = containingMethod;
			UnderlyingTypeParameter = underlyingTypeParameter;
		}

		protected abstract IEnumerable<TypeReferenceWithAttributes> GetConstraints(EmitContext context);

		IEnumerable<TypeReferenceWithAttributes> IGenericParameter.GetConstraints(EmitContext context)
		{
			return GetConstraints(context);
		}

		ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
		{
			return null;
		}

		INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
		{
			return null;
		}

		INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
		{
			return null;
		}

		ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
		{
			return null;
		}

		IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
		{
			return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
		}

		void IReference.Dispatch(MetadataVisitor visitor)
		{
			throw ExceptionUtilities.Unreachable;
		}

		IDefinition IReference.AsDefinition(EmitContext context)
		{
			return null;
		}
	}

	private sealed class TypeComparer : IComparer<TEmbeddedType>
	{
		public static readonly TypeComparer Instance = new TypeComparer();

		private TypeComparer()
		{
		}

		public int Compare(TEmbeddedType x, TEmbeddedType y)
		{
			int num = string.Compare(((INamespaceTypeReference)x).NamespaceName, ((INamespaceTypeReference)y).NamespaceName, StringComparison.Ordinal);
			if (num == 0)
			{
				num = string.Compare(((INamedEntity)x).Name, ((INamedEntity)y).Name, StringComparison.Ordinal);
				if (num == 0)
				{
					num = x.AssemblyRefIndex - y.AssemblyRefIndex;
				}
			}
			return num;
		}
	}

	public readonly TPEModuleBuilder ModuleBeingBuilt;

	public readonly ConcurrentDictionary<TNamedTypeSymbol, TEmbeddedType> EmbeddedTypesMap = new ConcurrentDictionary<TNamedTypeSymbol, TEmbeddedType>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	public readonly ConcurrentDictionary<TFieldSymbol, TEmbeddedField> EmbeddedFieldsMap = new ConcurrentDictionary<TFieldSymbol, TEmbeddedField>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	public readonly ConcurrentDictionary<TMethodSymbol, TEmbeddedMethod> EmbeddedMethodsMap = new ConcurrentDictionary<TMethodSymbol, TEmbeddedMethod>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	public readonly ConcurrentDictionary<TPropertySymbol, TEmbeddedProperty> EmbeddedPropertiesMap = new ConcurrentDictionary<TPropertySymbol, TEmbeddedProperty>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	public readonly ConcurrentDictionary<TEventSymbol, TEmbeddedEvent> EmbeddedEventsMap = new ConcurrentDictionary<TEventSymbol, TEmbeddedEvent>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	private ImmutableArray<TEmbeddedType> _frozen;

	public override bool IsFrozen => !_frozen.IsDefault;

	protected EmbeddedTypesManager(TPEModuleBuilder moduleBeingBuilt)
	{
		ModuleBeingBuilt = moduleBeingBuilt;
	}

	public override ImmutableArray<INamespaceTypeDefinition> GetTypes(DiagnosticBag diagnostics, HashSet<string> namesOfTopLevelTypes)
	{
		if (_frozen.IsDefault)
		{
			ArrayBuilder<TEmbeddedType> instance = ArrayBuilder<TEmbeddedType>.GetInstance();
			instance.AddRange(EmbeddedTypesMap.Values);
			instance.Sort(TypeComparer.Instance);
			if (ImmutableInterlocked.InterlockedInitialize(ref _frozen, instance.ToImmutableAndFree()) && _frozen.Length > 0)
			{
				INamespaceTypeDefinition namespaceTypeDefinition = _frozen[0];
				bool flag = HasNameConflict(namesOfTopLevelTypes, _frozen[0], diagnostics);
				for (int i = 1; i < _frozen.Length; i++)
				{
					INamespaceTypeDefinition namespaceTypeDefinition2 = _frozen[i];
					if (namespaceTypeDefinition.NamespaceName == namespaceTypeDefinition2.NamespaceName && namespaceTypeDefinition.Name == namespaceTypeDefinition2.Name)
					{
						if (!flag)
						{
							ReportNameCollisionBetweenEmbeddedTypes(_frozen[i - 1], _frozen[i], diagnostics);
							flag = true;
						}
					}
					else
					{
						namespaceTypeDefinition = namespaceTypeDefinition2;
						flag = HasNameConflict(namesOfTopLevelTypes, _frozen[i], diagnostics);
					}
				}
				OnGetTypesCompleted(_frozen, diagnostics);
			}
		}
		return StaticCast<INamespaceTypeDefinition>.From(_frozen);
	}

	private bool HasNameConflict(HashSet<string> namesOfTopLevelTypes, TEmbeddedType type, DiagnosticBag diagnostics)
	{
		if (namesOfTopLevelTypes.Contains(MetadataHelpers.BuildQualifiedName(((INamespaceTypeReference)type).NamespaceName, ((INamedEntity)type).Name)))
		{
			ReportNameCollisionWithAlreadyDeclaredType(type, diagnostics);
			return true;
		}
		return false;
	}

	internal abstract int GetTargetAttributeSignatureIndex(TSymbol underlyingSymbol, TAttributeData attrData, AttributeDescription description);

	internal bool IsTargetAttribute(TSymbol underlyingSymbol, TAttributeData attrData, AttributeDescription description)
	{
		return GetTargetAttributeSignatureIndex(underlyingSymbol, attrData, description) != -1;
	}

	internal abstract TAttributeData CreateSynthesizedAttribute(WellKnownMember constructor, TAttributeData attrData, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

	internal abstract void ReportIndirectReferencesToLinkedAssemblies(TAssemblySymbol assembly, DiagnosticBag diagnostics);

	protected abstract void OnGetTypesCompleted(ImmutableArray<TEmbeddedType> types, DiagnosticBag diagnostics);

	protected abstract void ReportNameCollisionBetweenEmbeddedTypes(TEmbeddedType typeA, TEmbeddedType typeB, DiagnosticBag diagnostics);

	protected abstract void ReportNameCollisionWithAlreadyDeclaredType(TEmbeddedType type, DiagnosticBag diagnostics);

	protected abstract TAttributeData CreateCompilerGeneratedAttribute();

	protected void EmbedReferences(ITypeDefinitionMember embeddedMember, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		new NoPiaReferenceIndexer(new EmitContext(ModuleBeingBuilt, syntaxNodeOpt, diagnostics, metadataOnly: false, includePrivateMembers: true)).Visit(embeddedMember);
	}

	protected abstract TEmbeddedType GetEmbeddedTypeForMember(TSymbol member, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

	internal abstract TEmbeddedField EmbedField(TEmbeddedType type, TFieldSymbol field, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

	internal abstract TEmbeddedMethod EmbedMethod(TEmbeddedType type, TMethodSymbol method, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

	internal abstract TEmbeddedProperty EmbedProperty(TEmbeddedType type, TPropertySymbol property, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics);

	internal abstract TEmbeddedEvent EmbedEvent(TEmbeddedType type, TEventSymbol @event, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding);

	internal IFieldReference EmbedFieldIfNeedTo(TFieldSymbol fieldSymbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		TEmbeddedType embeddedTypeForMember = GetEmbeddedTypeForMember((TSymbol)fieldSymbol, syntaxNodeOpt, diagnostics);
		if (embeddedTypeForMember != null)
		{
			return EmbedField(embeddedTypeForMember, fieldSymbol, syntaxNodeOpt, diagnostics);
		}
		return fieldSymbol;
	}

	internal IMethodReference EmbedMethodIfNeedTo(TMethodSymbol methodSymbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		TEmbeddedType embeddedTypeForMember = GetEmbeddedTypeForMember((TSymbol)methodSymbol, syntaxNodeOpt, diagnostics);
		if (embeddedTypeForMember != null)
		{
			return EmbedMethod(embeddedTypeForMember, methodSymbol, syntaxNodeOpt, diagnostics);
		}
		return methodSymbol;
	}

	internal void EmbedEventIfNeedTo(TEventSymbol eventSymbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding)
	{
		TEmbeddedType embeddedTypeForMember = GetEmbeddedTypeForMember((TSymbol)eventSymbol, syntaxNodeOpt, diagnostics);
		if (embeddedTypeForMember != null)
		{
			EmbedEvent(embeddedTypeForMember, eventSymbol, syntaxNodeOpt, diagnostics, isUsedForComAwareEventBinding);
		}
	}

	internal void EmbedPropertyIfNeedTo(TPropertySymbol propertySymbol, TSyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		TEmbeddedType embeddedTypeForMember = GetEmbeddedTypeForMember((TSymbol)propertySymbol, syntaxNodeOpt, diagnostics);
		if (embeddedTypeForMember != null)
		{
			EmbedProperty(embeddedTypeForMember, propertySymbol, syntaxNodeOpt, diagnostics);
		}
	}
}
