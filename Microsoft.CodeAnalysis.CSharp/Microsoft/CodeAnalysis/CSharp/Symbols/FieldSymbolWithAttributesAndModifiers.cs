using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class FieldSymbolWithAttributesAndModifiers : FieldSymbol, IAttributeTargetSymbol
{
	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	protected SymbolCompletionState state;

	internal abstract Location ErrorLocation { get; }

	protected abstract DeclarationModifiers Modifiers { get; }

	protected abstract SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList { get; }

	protected abstract IAttributeTargetSymbol AttributeOwner { get; }

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => AttributeOwner;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Field;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations => AttributeLocation.Field;

	public sealed override bool IsStatic => (Modifiers & DeclarationModifiers.Static) != 0;

	public sealed override bool IsReadOnly => (Modifiers & DeclarationModifiers.ReadOnly) != 0;

	public sealed override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(Modifiers);

	public sealed override bool IsConst => (Modifiers & DeclarationModifiers.Const) != 0;

	public sealed override bool IsVolatile => (Modifiers & DeclarationModifiers.Volatile) != 0;

	public sealed override bool IsFixed => (Modifiers & DeclarationModifiers.Fixed) != 0;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!((SourceMemberContainerTypeSymbol)ContainingType).AnyMemberHasAttributes)
			{
				return null;
			}
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
			{
				return ((CommonFieldEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData)?.ObsoleteAttributeData;
			}
			return ObsoleteAttributeData.Uninitialized;
		}
	}

	internal sealed override bool HasSpecialName
	{
		get
		{
			if (HasRuntimeSpecialName)
			{
				return true;
			}
			return GetDecodedWellKnownAttributeData()?.HasSpecialNameAttribute ?? false;
		}
	}

	internal sealed override bool IsNotSerialized => GetDecodedWellKnownAttributeData()?.HasNonSerializedAttribute ?? false;

	internal sealed override MarshalPseudoCustomAttributeData MarshallingInformation => GetDecodedWellKnownAttributeData()?.MarshallingInformation;

	internal sealed override int? TypeLayoutOffset => GetDecodedWellKnownAttributeData()?.Offset;

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsSealed)
		{
			return lazyCustomAttributesBag;
		}
		if (LoadAndValidateAttributes(OneOrMany.Create(AttributeDeclarationSyntaxList), ref _lazyCustomAttributesBag))
		{
			state.NotePartComplete(CompletionPart.Attributes);
		}
		return _lazyCustomAttributesBag;
	}

	protected CommonFieldWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonFieldWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal sealed override CSharpAttributeData EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (Symbol.EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref arguments, out var attributeData, out var obsoleteData))
		{
			if (obsoleteData != null)
			{
				arguments.GetOrCreateData<CommonFieldEarlyWellKnownAttributeData>().ObsoleteAttributeData = obsoleteData;
			}
			return attributeData;
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	internal override void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(this, AttributeDescription.SpecialNameAttribute))
		{
			arguments.GetOrCreateData<CommonFieldWellKnownAttributeData>().HasSpecialNameAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.NonSerializedAttribute))
		{
			arguments.GetOrCreateData<CommonFieldWellKnownAttributeData>().HasNonSerializedAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.FieldOffsetAttribute))
		{
			if (IsStatic || IsConst)
			{
				arguments.Diagnostics.Add(ErrorCode.ERR_StructOffsetOnBadField, arguments.AttributeSyntaxOpt.Name.Location, arguments.AttributeSyntaxOpt.GetErrorDisplayName());
				return;
			}
			int num = attribute.CommonConstructorArguments[0].DecodeValue<int>(SpecialType.System_Int32);
			if (num < 0)
			{
				CSharpSyntaxNode attributeArgumentSyntax = attribute.GetAttributeArgumentSyntax(0, arguments.AttributeSyntaxOpt);
				arguments.Diagnostics.Add(ErrorCode.ERR_InvalidAttributeArgument, attributeArgumentSyntax.Location, arguments.AttributeSyntaxOpt.GetErrorDisplayName());
				num = 0;
			}
			arguments.GetOrCreateData<CommonFieldWellKnownAttributeData>().SetFieldOffset(num);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.MarshalAsAttribute))
		{
			MarshalAsAttributeDecoder<CommonFieldWellKnownAttributeData, AttributeSyntax, CSharpAttributeData, AttributeLocation>.Decode(ref arguments, AttributeTargets.Field, MessageProvider.Instance);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.DynamicAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitDynamicAttr, arguments.AttributeSyntaxOpt.Location);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IsReadOnlyAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsReadOnlyAttribute.FullName);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IsUnmanagedAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsUnmanagedAttribute.FullName);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IsByRefLikeAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsByRefLikeAttribute.FullName);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.DateTimeConstantAttribute))
		{
			VerifyConstantValueMatches(attribute.DecodeDateTimeConstantValue(), ref arguments);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.DecimalConstantAttribute))
		{
			VerifyConstantValueMatches(attribute.DecodeDecimalConstantValue(), ref arguments);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.TupleElementNamesAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitTupleElementNamesAttribute, arguments.AttributeSyntaxOpt.Location);
		}
	}

	private void VerifyConstantValueMatches(ConstantValue attrValue, ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		if (attrValue.IsBad)
		{
			return;
		}
		CommonFieldWellKnownAttributeData orCreateData = arguments.GetOrCreateData<CommonFieldWellKnownAttributeData>();
		ConstantValue constantValue;
		if (IsConst)
		{
			if (Type.SpecialType == SpecialType.System_Decimal)
			{
				constantValue = GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
				if ((object)constantValue != null && !constantValue.IsBad && constantValue != attrValue)
				{
					arguments.Diagnostics.Add(ErrorCode.ERR_FieldHasMultipleDistinctConstantValues, arguments.AttributeSyntaxOpt.Location);
				}
			}
			else
			{
				arguments.Diagnostics.Add(ErrorCode.ERR_FieldHasMultipleDistinctConstantValues, arguments.AttributeSyntaxOpt.Location);
			}
			if (orCreateData.ConstValue == Microsoft.CodeAnalysis.ConstantValue.Unset)
			{
				orCreateData.ConstValue = attrValue;
			}
			return;
		}
		constantValue = orCreateData.ConstValue;
		if (constantValue != Microsoft.CodeAnalysis.ConstantValue.Unset)
		{
			if (constantValue != attrValue)
			{
				arguments.Diagnostics.Add(ErrorCode.ERR_FieldHasMultipleDistinctConstantValues, arguments.AttributeSyntaxOpt.Location);
			}
		}
		else
		{
			orCreateData.ConstValue = attrValue;
		}
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, DiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		if ((((CommonFieldWellKnownAttributeData)decodedData)?.Offset).HasValue)
		{
			if (ContainingType.Layout.Kind != LayoutKind.Explicit)
			{
				int index = boundAttributes.IndexOfAttribute(this, AttributeDescription.FieldOffsetAttribute);
				diagnostics.Add(ErrorCode.ERR_StructOffsetOnBadStruct, allAttributeSyntaxNodes[index].Name.Location);
			}
		}
		else if (!IsStatic && !IsConst && ContainingType.Layout.Kind == LayoutKind.Explicit)
		{
			diagnostics.Add(ErrorCode.ERR_MissingStructOffset, ErrorLocation, AttributeOwner);
		}
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		if (Type.ContainsDynamic())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, DeclaringCompilation.SynthesizeDynamicAttribute(Type, CustomModifiers.Length));
		}
		if (Type.ContainsTupleNames())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, DeclaringCompilation.SynthesizeTupleNamesAttribute(Type));
		}
	}
}
