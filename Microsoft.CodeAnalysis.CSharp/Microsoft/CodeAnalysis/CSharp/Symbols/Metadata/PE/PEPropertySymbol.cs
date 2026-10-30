using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.DocumentationComments;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal class PEPropertySymbol : PropertySymbol
{
	[Flags]
	private enum Flags : byte
	{
		IsSpecialName = 1,
		IsRuntimeSpecialName = 2,
		CallMethodsDirectly = 4
	}

	private sealed class PEPropertySymbolWithCustomModifiers : PEPropertySymbol
	{
		private readonly ImmutableArray<CustomModifier> _typeCustomModifiers;

		private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

		public override ImmutableArray<CustomModifier> TypeCustomModifiers => _typeCustomModifiers;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

		public PEPropertySymbolWithCustomModifiers(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, PropertyDefinitionHandle handle, PEMethodSymbol getMethod, PEMethodSymbol setMethod, ParamInfo<TypeSymbol>[] propertyParams, MetadataDecoder metadataDecoder)
			: base(moduleSymbol, containingType, handle, getMethod, setMethod, propertyParams[0].CustomModifiers.NullToEmpty().Length + propertyParams[0].RefCustomModifiers.NullToEmpty().Length, propertyParams, metadataDecoder)
		{
			ParamInfo<TypeSymbol> paramInfo = propertyParams[0];
			_typeCustomModifiers = CSharpCustomModifier.Convert(paramInfo.CustomModifiers);
			_refCustomModifiers = CSharpCustomModifier.Convert(paramInfo.RefCustomModifiers);
		}
	}

	private readonly string _name;

	private readonly PENamedTypeSymbol _containingType;

	private readonly PropertyDefinitionHandle _handle;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private readonly RefKind _refKind;

	private readonly TypeSymbol _propertyType;

	private readonly PEMethodSymbol _getMethod;

	private readonly PEMethodSymbol _setMethod;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private Tuple<CultureInfo, string> _lazyDocComment;

	private DiagnosticInfo _lazyUseSiteDiagnostic = CSDiagnosticInfo.EmptyErrorInfo;

	private ObsoleteAttributeData _lazyObsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

	private const int UnsetAccessibility = -1;

	private int _declaredAccessibility = -1;

	private readonly Flags _flags;

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override string Name
	{
		get
		{
			if (!IsIndexer)
			{
				return _name;
			}
			return "this[]";
		}
	}

	internal override bool HasSpecialName => (_flags & Flags.IsSpecialName) != 0;

	public override string MetadataName => _name;

	internal PropertyDefinitionHandle Handle => _handle;

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			if (_declaredAccessibility == -1)
			{
				Accessibility declaredAccessibilityFromAccessors;
				if (IsOverride)
				{
					bool flag = false;
					Accessibility accessibility = Accessibility.NotApplicable;
					Accessibility accessibility2 = Accessibility.NotApplicable;
					PropertySymbol propertySymbol = this;
					while (true)
					{
						if (accessibility == Accessibility.NotApplicable)
						{
							MethodSymbol getMethod = propertySymbol.GetMethod;
							if ((object)getMethod != null)
							{
								Accessibility declaredAccessibility = getMethod.DeclaredAccessibility;
								accessibility = (((declaredAccessibility == Accessibility.ProtectedOrInternal) & flag) ? Accessibility.Protected : declaredAccessibility);
							}
						}
						if (accessibility2 == Accessibility.NotApplicable)
						{
							MethodSymbol setMethod = propertySymbol.SetMethod;
							if ((object)setMethod != null)
							{
								Accessibility declaredAccessibility2 = setMethod.DeclaredAccessibility;
								accessibility2 = (((declaredAccessibility2 == Accessibility.ProtectedOrInternal) & flag) ? Accessibility.Protected : declaredAccessibility2);
							}
						}
						if (accessibility != Accessibility.NotApplicable && accessibility2 != Accessibility.NotApplicable)
						{
							break;
						}
						PropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;
						if ((object)overriddenProperty == null)
						{
							break;
						}
						if (!flag && !propertySymbol.ContainingAssembly.HasInternalAccessTo(overriddenProperty.ContainingAssembly))
						{
							flag = true;
						}
						propertySymbol = overriddenProperty;
					}
					declaredAccessibilityFromAccessors = PEPropertyOrEventHelpers.GetDeclaredAccessibilityFromAccessors(accessibility, accessibility2);
				}
				else
				{
					declaredAccessibilityFromAccessors = PEPropertyOrEventHelpers.GetDeclaredAccessibilityFromAccessors(GetMethod, SetMethod);
				}
				Interlocked.CompareExchange(ref _declaredAccessibility, (int)declaredAccessibilityFromAccessors, -1);
			}
			return (Accessibility)_declaredAccessibility;
		}
	}

	public override bool IsExtern
	{
		get
		{
			if ((object)_getMethod == null || !_getMethod.IsExtern)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsExtern;
				}
				return false;
			}
			return true;
		}
	}

	public override bool IsAbstract
	{
		get
		{
			if ((object)_getMethod == null || !_getMethod.IsAbstract)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsAbstract;
				}
				return false;
			}
			return true;
		}
	}

	public override bool IsSealed
	{
		get
		{
			if ((object)_getMethod == null || _getMethod.IsSealed)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsSealed;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsVirtual
	{
		get
		{
			if (!IsOverride && !IsAbstract)
			{
				if ((object)_getMethod == null || !_getMethod.IsVirtual)
				{
					if ((object)_setMethod != null)
					{
						return _setMethod.IsVirtual;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsOverride
	{
		get
		{
			if ((object)_getMethod == null || !_getMethod.IsOverride)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsOverride;
				}
				return false;
			}
			return true;
		}
	}

	public override bool IsStatic
	{
		get
		{
			if ((object)_getMethod == null || _getMethod.IsStatic)
			{
				if ((object)_setMethod != null)
				{
					return _setMethod.IsStatic;
				}
				return true;
			}
			return false;
		}
	}

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public override bool IsIndexer
	{
		get
		{
			if (ParameterCount > 0)
			{
				string defaultMemberName = _containingType.DefaultMemberName;
				if (!(_name == defaultMemberName) && ((object)GetMethod == null || !(GetMethod.Name == defaultMemberName)))
				{
					if ((object)SetMethod != null)
					{
						return SetMethod.Name == defaultMemberName;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsIndexedProperty
	{
		get
		{
			if (ParameterCount > 0)
			{
				return _containingType.IsComImport;
			}
			return false;
		}
	}

	public override RefKind RefKind => _refKind;

	public override TypeSymbol Type => _propertyType;

	public override ImmutableArray<CustomModifier> TypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override MethodSymbol GetMethod => _getMethod;

	public override MethodSymbol SetMethod => _setMethod;

	internal override CallingConvention CallingConvention => (CallingConvention)new MetadataDecoder(_containingType.ContainingPEModule, _containingType).GetSignatureHeaderForProperty(_handle).RawValue;

	public override ImmutableArray<Location> Locations => _containingType.ContainingPEModule.MetadataLocation.Cast<MetadataLocation, Location>();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations
	{
		get
		{
			if (((object)_getMethod == null || _getMethod.ExplicitInterfaceImplementations.Length == 0) && ((object)_setMethod == null || _setMethod.ExplicitInterfaceImplementations.Length == 0))
			{
				return ImmutableArray<PropertySymbol>.Empty;
			}
			ISet<PropertySymbol> propertiesForExplicitlyImplementedAccessor = PEPropertyOrEventHelpers.GetPropertiesForExplicitlyImplementedAccessor(_getMethod);
			ISet<PropertySymbol> propertiesForExplicitlyImplementedAccessor2 = PEPropertyOrEventHelpers.GetPropertiesForExplicitlyImplementedAccessor(_setMethod);
			ArrayBuilder<PropertySymbol> instance = ArrayBuilder<PropertySymbol>.GetInstance();
			foreach (PropertySymbol item in propertiesForExplicitlyImplementedAccessor)
			{
				if ((object)item.SetMethod == null || propertiesForExplicitlyImplementedAccessor2.Contains(item))
				{
					instance.Add(item);
				}
			}
			foreach (PropertySymbol item2 in propertiesForExplicitlyImplementedAccessor2)
			{
				if ((object)item2.GetMethod == null)
				{
					instance.Add(item2);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	internal override bool MustCallMethodsDirectly => (_flags & Flags.CallMethodsDirectly) != 0;

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			ObsoleteAttributeHelpers.InitializeObsoleteDataFromMetadata(ref _lazyObsoleteAttributeData, _handle, (PEModuleSymbol)ContainingModule, ignoreByRefLikeMarker: false);
			return _lazyObsoleteAttributeData;
		}
	}

	internal override bool HasRuntimeSpecialName => (_flags & Flags.IsRuntimeSpecialName) != 0;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal static PEPropertySymbol Create(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, PropertyDefinitionHandle handle, PEMethodSymbol getMethod, PEMethodSymbol setMethod)
	{
		MetadataDecoder metadataDecoder = new MetadataDecoder(moduleSymbol, containingType);
		ParamInfo<TypeSymbol>[] signatureForProperty = metadataDecoder.GetSignatureForProperty(handle, out var _, out var BadImageFormatException);
		ParamInfo<TypeSymbol> paramInfo = signatureForProperty[0];
		PEPropertySymbol pEPropertySymbol = ((paramInfo.CustomModifiers.IsDefaultOrEmpty && paramInfo.RefCustomModifiers.IsDefaultOrEmpty) ? new PEPropertySymbol(moduleSymbol, containingType, handle, getMethod, setMethod, 0, signatureForProperty, metadataDecoder) : new PEPropertySymbolWithCustomModifiers(moduleSymbol, containingType, handle, getMethod, setMethod, signatureForProperty, metadataDecoder));
		bool flag = pEPropertySymbol.RefKind == RefKind.In != pEPropertySymbol.RefCustomModifiers.HasInAttributeModifier();
		if ((BadImageFormatException != null) | flag)
		{
			pEPropertySymbol._lazyUseSiteDiagnostic = new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, pEPropertySymbol);
		}
		return pEPropertySymbol;
	}

	private PEPropertySymbol(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, PropertyDefinitionHandle handle, PEMethodSymbol getMethod, PEMethodSymbol setMethod, int countOfCustomModifiers, ParamInfo<TypeSymbol>[] propertyParams, MetadataDecoder metadataDecoder)
	{
		_containingType = containingType;
		PEModule module = moduleSymbol.Module;
		PropertyAttributes flags = PropertyAttributes.None;
		BadImageFormatException ex = null;
		try
		{
			module.GetPropertyDefPropsOrThrow(handle, out _name, out flags);
		}
		catch (BadImageFormatException ex2)
		{
			ex = ex2;
			if (_name == null)
			{
				_name = string.Empty;
			}
		}
		_getMethod = getMethod;
		_setMethod = setMethod;
		_handle = handle;
		BadImageFormatException metadataException = null;
		ParamInfo<TypeSymbol>[] array = (((object)getMethod == null) ? null : metadataDecoder.GetSignatureForMethod(getMethod.Handle, out var signatureHeader, out metadataException));
		BadImageFormatException metadataException2 = null;
		ParamInfo<TypeSymbol>[] array2 = (((object)setMethod == null) ? null : metadataDecoder.GetSignatureForMethod(setMethod.Handle, out signatureHeader, out metadataException2));
		_parameters = ((array2 == null) ? GetParameters(moduleSymbol, this, propertyParams, array, getMethod.IsMetadataVirtual(), out var anyParameterIsBad) : GetParameters(moduleSymbol, this, propertyParams, array2, setMethod.IsMetadataVirtual(), out anyParameterIsBad));
		if ((metadataException != null || metadataException2 != null || ex != null) | anyParameterIsBad)
		{
			_lazyUseSiteDiagnostic = new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this);
		}
		ParamInfo<TypeSymbol> paramInfo = propertyParams[0];
		if (paramInfo.IsByRef)
		{
			if (moduleSymbol.Module.HasIsReadOnlyAttribute(handle))
			{
				_refKind = RefKind.In;
			}
			else
			{
				_refKind = RefKind.Ref;
			}
		}
		else
		{
			_refKind = RefKind.None;
		}
		TypeSymbol type = paramInfo.Type;
		_propertyType = DynamicTypeDecoder.TransformType(type, countOfCustomModifiers, handle, moduleSymbol, _refKind);
		_propertyType = _propertyType.AsDynamicIfNoPia(_containingType);
		_propertyType = TupleTypeDecoder.DecodeTupleTypesIfApplicable(_propertyType, handle, moduleSymbol);
		int num;
		if (DoSignaturesMatch(module, metadataDecoder, propertyParams, _getMethod, array, _setMethod, array2))
		{
			num = (MustCallMethodsDirectlyCore() ? 1 : 0);
			if (num == 0)
			{
				if ((object)_getMethod != null)
				{
					_getMethod.SetAssociatedProperty(this, MethodKind.PropertyGet);
				}
				if ((object)_setMethod != null)
				{
					_setMethod.SetAssociatedProperty(this, MethodKind.PropertySet);
				}
			}
		}
		else
		{
			num = 1;
		}
		if (num != 0)
		{
			_flags |= Flags.CallMethodsDirectly;
		}
		if ((flags & PropertyAttributes.SpecialName) != 0)
		{
			_flags |= Flags.IsSpecialName;
		}
		if ((flags & PropertyAttributes.RTSpecialName) != 0)
		{
			_flags |= Flags.IsRuntimeSpecialName;
		}
	}

	private bool MustCallMethodsDirectlyCore()
	{
		if (RefKind != RefKind.None && _setMethod != null)
		{
			return true;
		}
		if (ParameterCount == 0)
		{
			return false;
		}
		if (IsIndexedProperty)
		{
			return IsStatic;
		}
		if (IsIndexer)
		{
			return this.HasRefOrOutParameter();
		}
		return true;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_lazyCustomAttributes.IsDefault)
		{
			ImmutableArray<CSharpAttributeData> customAttributesForToken = ((PEModuleSymbol)ContainingModule).GetCustomAttributesForToken(_handle, out var _, (RefKind == RefKind.In) ? AttributeDescription.IsReadOnlyAttribute : default(AttributeDescription));
			ImmutableInterlocked.InterlockedInitialize(ref _lazyCustomAttributes, customAttributesForToken);
		}
		return _lazyCustomAttributes;
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return GetAttributes();
	}

	private static bool DoSignaturesMatch(PEModule module, MetadataDecoder metadataDecoder, ParamInfo<TypeSymbol>[] propertyParams, PEMethodSymbol getMethod, ParamInfo<TypeSymbol>[] getMethodParams, PEMethodSymbol setMethod, ParamInfo<TypeSymbol>[] setMethodParams)
	{
		bool flag = getMethodParams != null;
		bool flag2 = setMethodParams != null;
		if (flag && !metadataDecoder.DoPropertySignaturesMatch(propertyParams, getMethodParams, comparingToSetter: false, compareParamByRef: true, compareReturnType: true))
		{
			return false;
		}
		if (flag2 && !metadataDecoder.DoPropertySignaturesMatch(propertyParams, setMethodParams, comparingToSetter: true, compareParamByRef: true, compareReturnType: true))
		{
			return false;
		}
		if (flag & flag2)
		{
			int num = propertyParams.Length - 1;
			ParameterHandle handle = getMethodParams[num].Handle;
			ParameterHandle handle2 = setMethodParams[num].Handle;
			bool num2 = !handle.IsNil && module.HasParamsAttribute(handle);
			bool flag3 = !handle2.IsNil && module.HasParamsAttribute(handle2);
			if (num2 != flag3)
			{
				return false;
			}
			if (getMethod.IsExtern != setMethod.IsExtern || getMethod.IsSealed != setMethod.IsSealed || getMethod.IsOverride != setMethod.IsOverride || getMethod.IsStatic != setMethod.IsStatic)
			{
				return false;
			}
		}
		return true;
	}

	private static ImmutableArray<ParameterSymbol> GetParameters(PEModuleSymbol moduleSymbol, PEPropertySymbol property, ParamInfo<TypeSymbol>[] propertyParams, ParamInfo<TypeSymbol>[] accessorParams, bool isPropertyVirtual, out bool anyParameterIsBad)
	{
		anyParameterIsBad = false;
		if (propertyParams.Length < 2)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		int num = accessorParams.Length;
		ParameterSymbol[] array = new ParameterSymbol[propertyParams.Length - 1];
		for (int i = 1; i < propertyParams.Length; i++)
		{
			ParamInfo<TypeSymbol> parameterInfo = propertyParams[i];
			ParameterHandle handle = ((i < num) ? accessorParams[i].Handle : parameterInfo.Handle);
			int num2 = i - 1;
			array[num2] = PEParameterSymbol.Create(moduleSymbol, property, isPropertyVirtual, num2, handle, parameterInfo, out var isBad);
			if (isBad)
			{
				anyParameterIsBad = true;
			}
		}
		return array.AsImmutableOrNull();
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return PEDocumentationCommentUtils.GetDocumentationComment(this, _containingType.ContainingPEModule, preferredCulture, cancellationToken, ref _lazyDocComment);
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (_lazyUseSiteDiagnostic == CSDiagnosticInfo.EmptyErrorInfo)
		{
			DiagnosticInfo result = null;
			CalculateUseSiteDiagnostic(ref result);
			_lazyUseSiteDiagnostic = result;
		}
		return _lazyUseSiteDiagnostic;
	}
}
