using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.DocumentationComments;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class PEFieldSymbol : FieldSymbol
{
	private readonly FieldDefinitionHandle _handle;

	private readonly string _name;

	private readonly FieldAttributes _flags;

	private readonly PENamedTypeSymbol _containingType;

	private bool _lazyIsVolatile;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private ImmutableArray<CustomModifier> _lazyCustomModifiers;

	private ConstantValue _lazyConstantValue = Microsoft.CodeAnalysis.ConstantValue.Unset;

	private Tuple<CultureInfo, string> _lazyDocComment;

	private DiagnosticInfo _lazyUseSiteDiagnostic = CSDiagnosticInfo.EmptyErrorInfo;

	private ObsoleteAttributeData _lazyObsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

	private TypeSymbol _lazyType;

	private int _lazyFixedSize;

	private NamedTypeSymbol _lazyFixedImplementationType;

	private PEEventSymbol _associatedEventOpt;

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override string Name => _name;

	internal FieldAttributes Flags => _flags;

	internal override bool HasSpecialName => (_flags & FieldAttributes.SpecialName) != 0;

	internal override bool HasRuntimeSpecialName => (_flags & FieldAttributes.RTSpecialName) != 0;

	internal override bool IsNotSerialized => (_flags & FieldAttributes.NotSerialized) != 0;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => null;

	internal override bool IsMarshalledExplicitly => (_flags & FieldAttributes.HasFieldMarshal) != 0;

	internal override UnmanagedType MarshallingType
	{
		get
		{
			if ((_flags & FieldAttributes.HasFieldMarshal) == 0)
			{
				return (UnmanagedType)0;
			}
			return _containingType.ContainingPEModule.Module.GetMarshallingType(_handle);
		}
	}

	internal override ImmutableArray<byte> MarshallingDescriptor
	{
		get
		{
			if ((_flags & FieldAttributes.HasFieldMarshal) == 0)
			{
				return default;
			}
			return _containingType.ContainingPEModule.Module.GetMarshallingDescriptor(_handle);
		}
	}

	internal override int? TypeLayoutOffset => _containingType.ContainingPEModule.Module.GetFieldOffset(_handle);

	internal FieldDefinitionHandle Handle => _handle;

	private PEModuleSymbol ContainingPEModule => ((PENamespaceSymbol)ContainingNamespace).ContainingPEModule;

	public override bool IsFixed
	{
		get
		{
			EnsureSignatureIsLoaded();
			return (object)_lazyFixedImplementationType != null;
		}
	}

	public override int FixedSize
	{
		get
		{
			EnsureSignatureIsLoaded();
			return _lazyFixedSize;
		}
	}

	public override ImmutableArray<CustomModifier> CustomModifiers
	{
		get
		{
			EnsureSignatureIsLoaded();
			return _lazyCustomModifiers;
		}
	}

	public override Symbol AssociatedSymbol => _associatedEventOpt;

	public override bool IsReadOnly => (_flags & FieldAttributes.InitOnly) != 0;

	public override bool IsVolatile
	{
		get
		{
			EnsureSignatureIsLoaded();
			return _lazyIsVolatile;
		}
	}

	public override bool IsConst
	{
		get
		{
			if ((_flags & FieldAttributes.Literal) == 0)
			{
				return GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false) != null;
			}
			return true;
		}
	}

	public override ImmutableArray<Location> Locations => _containingType.ContainingPEModule.MetadataLocation.Cast<MetadataLocation, Location>();

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			switch (_flags & FieldAttributes.FieldAccessMask)
			{
			case FieldAttributes.Assembly:
				return Accessibility.Internal;
			case FieldAttributes.FamORAssem:
				return Accessibility.ProtectedOrInternal;
			case FieldAttributes.FamANDAssem:
				return Accessibility.ProtectedAndInternal;
			case FieldAttributes.PrivateScope:
			case FieldAttributes.Private:
				return Accessibility.Private;
			case FieldAttributes.Public:
				return Accessibility.Public;
			case FieldAttributes.Family:
				return Accessibility.Protected;
			default:
				return Accessibility.Private;
			}
		}
	}

	public override bool IsStatic => (_flags & FieldAttributes.Static) != 0;

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			ObsoleteAttributeHelpers.InitializeObsoleteDataFromMetadata(ref _lazyObsoleteAttributeData, _handle, (PEModuleSymbol)ContainingModule, ignoreByRefLikeMarker: false);
			return _lazyObsoleteAttributeData;
		}
	}

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal PEFieldSymbol(PEModuleSymbol moduleSymbol, PENamedTypeSymbol containingType, FieldDefinitionHandle fieldDef)
	{
		_handle = fieldDef;
		_containingType = containingType;
		try
		{
			moduleSymbol.Module.GetFieldDefPropsOrThrow(fieldDef, out _name, out _flags);
		}
		catch (BadImageFormatException)
		{
			if (_name == null)
			{
				_name = string.Empty;
			}
			_lazyUseSiteDiagnostic = new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this);
		}
	}

	internal void SetAssociatedEvent(PEEventSymbol eventSymbol)
	{
		if ((object)_associatedEventOpt == null)
		{
			_associatedEventOpt = eventSymbol;
		}
	}

	private void EnsureSignatureIsLoaded()
	{
		if ((object)_lazyType == null)
		{
			PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
			TypeSymbol metadataType = new MetadataDecoder(containingPEModule, _containingType).DecodeFieldSignature(_handle, out var isVolatile, out var customModifiers);
			ImmutableArray<CustomModifier> value = CSharpCustomModifier.Convert(customModifiers);
			metadataType = DynamicTypeDecoder.TransformType(metadataType, value.Length, _handle, containingPEModule);
			metadataType = TupleTypeDecoder.DecodeTupleTypesIfApplicable(metadataType, _handle, containingPEModule);
			_lazyIsVolatile = isVolatile;
			if (value.IsEmpty && IsFixedBuffer(out var fixedSize, out var fixedElementType))
			{
				_lazyFixedSize = fixedSize;
				_lazyFixedImplementationType = metadataType as NamedTypeSymbol;
				metadataType = new PointerTypeSymbol(fixedElementType);
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref _lazyCustomModifiers, value, default);
			Interlocked.CompareExchange(ref _lazyType, metadataType, null);
		}
	}

	private bool IsFixedBuffer(out int fixedSize, out TypeSymbol fixedElementType)
	{
		fixedSize = 0;
		fixedElementType = null;
		PEModuleSymbol containingPEModule = ContainingPEModule;
		if (containingPEModule.Module.HasFixedBufferAttribute(_handle, out var elementTypeName, out var bufferSize))
		{
			TypeSymbol typeSymbolForSerializedType = new MetadataDecoder(containingPEModule).GetTypeSymbolForSerializedType(elementTypeName);
			if (typeSymbolForSerializedType.FixedBufferElementSizeInBytes() != 0)
			{
				fixedSize = bufferSize;
				fixedElementType = typeSymbolForSerializedType;
				return true;
			}
		}
		return false;
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		EnsureSignatureIsLoaded();
		return _lazyType;
	}

	internal override NamedTypeSymbol FixedImplementationType(PEModuleBuilder emitModule)
	{
		EnsureSignatureIsLoaded();
		return _lazyFixedImplementationType;
	}

	internal override ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes)
	{
		if (_lazyConstantValue == Microsoft.CodeAnalysis.ConstantValue.Unset)
		{
			ConstantValue value = null;
			if ((_flags & FieldAttributes.Literal) != 0)
			{
				value = _containingType.ContainingPEModule.Module.GetConstantFieldValue(_handle);
			}
			if (Type.SpecialType == SpecialType.System_Decimal && _containingType.ContainingPEModule.Module.HasDecimalConstantAttribute(Handle, out var defaultValue))
			{
				value = defaultValue;
			}
			Interlocked.CompareExchange(ref _lazyConstantValue, value, Microsoft.CodeAnalysis.ConstantValue.Unset);
		}
		return _lazyConstantValue;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_lazyCustomAttributes.IsDefault)
		{
			PEModuleSymbol pEModuleSymbol = (PEModuleSymbol)ContainingModule;
			if (FilterOutDecimalConstantAttribute())
			{
				ImmutableArray<CSharpAttributeData> customAttributesForToken = pEModuleSymbol.GetCustomAttributesForToken(_handle, out var _, AttributeDescription.DecimalConstantAttribute);
				ImmutableInterlocked.InterlockedInitialize(ref _lazyCustomAttributes, customAttributesForToken);
			}
			else
			{
				pEModuleSymbol.LoadCustomAttributes(_handle, ref _lazyCustomAttributes);
			}
		}
		return _lazyCustomAttributes;
	}

	private bool FilterOutDecimalConstantAttribute()
	{
		ConstantValue constantValue;
		if (Type.SpecialType == SpecialType.System_Decimal && (object)(constantValue = GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false)) != null)
		{
			return constantValue.Discriminator == ConstantValueTypeDiscriminator.Decimal;
		}
		return false;
	}

	internal override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		foreach (CSharpAttributeData attribute in GetAttributes())
		{
			yield return attribute;
		}
		if (FilterOutDecimalConstantAttribute())
		{
			PEModuleSymbol containingPEModule = _containingType.ContainingPEModule;
			yield return new PEAttributeData(containingPEModule, containingPEModule.Module.FindLastTargetAttribute(_handle, AttributeDescription.DecimalConstantAttribute).Handle);
		}
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
