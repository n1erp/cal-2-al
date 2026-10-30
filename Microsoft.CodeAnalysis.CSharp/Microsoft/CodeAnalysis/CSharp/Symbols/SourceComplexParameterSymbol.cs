using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SourceComplexParameterSymbol : SourceParameterSymbol, IAttributeTargetSymbol
{
	[Flags]
	private enum ParameterSyntaxKind : byte
	{
		Regular = 0,
		ParamsParameter = 1,
		ExtensionThisParameter = 2,
		DefaultParameter = 4
	}

	private readonly SyntaxReference _syntaxRef;

	private readonly ParameterSyntaxKind _parameterSyntaxKind;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private ThreeState _lazyHasOptionalAttribute;

	protected ConstantValue _lazyDefaultSyntaxValue;

	private Binder ParameterBinderOpt => (ContainingSymbol as LocalFunctionSymbol)?.ParameterBinder;

	internal override SyntaxReference SyntaxReference => _syntaxRef;

	internal ParameterSyntax CSharpSyntaxNode => (ParameterSyntax)(_syntaxRef?.GetSyntax());

	internal SyntaxTree SyntaxTree
	{
		get
		{
			if (_syntaxRef != null)
			{
				return _syntaxRef.SyntaxTree;
			}
			return null;
		}
	}

	internal override ConstantValue ExplicitDefaultConstantValue => DefaultSyntaxValue ?? DefaultValueFromAttributes;

	internal override ConstantValue DefaultValueFromAttributes
	{
		get
		{
			ParameterEarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = GetEarlyDecodedWellKnownAttributeData();
			if (earlyDecodedWellKnownAttributeData == null || !(earlyDecodedWellKnownAttributeData.DefaultParameterValue != ConstantValue.Unset))
			{
				return null;
			}
			return earlyDecodedWellKnownAttributeData.DefaultParameterValue;
		}
	}

	internal override bool IsIDispatchConstant => GetDecodedWellKnownAttributeData()?.HasIDispatchConstantAttribute ?? false;

	internal override bool IsIUnknownConstant => GetDecodedWellKnownAttributeData()?.HasIUnknownConstantAttribute ?? false;

	private bool HasCallerLineNumberAttribute => GetEarlyDecodedWellKnownAttributeData()?.HasCallerLineNumberAttribute ?? false;

	private bool HasCallerFilePathAttribute => GetEarlyDecodedWellKnownAttributeData()?.HasCallerFilePathAttribute ?? false;

	private bool HasCallerMemberNameAttribute => GetEarlyDecodedWellKnownAttributeData()?.HasCallerMemberNameAttribute ?? false;

	internal override bool IsCallerLineNumber => HasCallerLineNumberAttribute;

	internal override bool IsCallerFilePath
	{
		get
		{
			if (!HasCallerLineNumberAttribute)
			{
				return HasCallerFilePathAttribute;
			}
			return false;
		}
	}

	internal override bool IsCallerMemberName
	{
		get
		{
			if (!HasCallerLineNumberAttribute && !HasCallerFilePathAttribute)
			{
				return HasCallerMemberNameAttribute;
			}
			return false;
		}
	}

	private ConstantValue DefaultSyntaxValue
	{
		get
		{
			if (_lazyDefaultSyntaxValue == ConstantValue.Unset)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				if (Interlocked.CompareExchange(ref _lazyDefaultSyntaxValue, MakeDefaultExpression(instance, ParameterBinderOpt), ConstantValue.Unset) == ConstantValue.Unset)
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _lazyDefaultSyntaxValue;
		}
	}

	public override string MetadataName
	{
		get
		{
			if (!(ContainingSymbol is SourceOrdinaryMethodSymbol { SourcePartialDefinition: var sourcePartialDefinition }))
			{
				return base.MetadataName;
			}
			if ((object)sourcePartialDefinition == null)
			{
				return base.MetadataName;
			}
			return sourcePartialDefinition.Parameters[Ordinal].MetadataName;
		}
	}

	protected virtual IAttributeTargetSymbol AttributeOwner => this;

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => AttributeOwner;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Parameter;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations => AttributeLocation.Parameter;

	private SourceParameterSymbol BoundAttributesSource
	{
		get
		{
			if (!(ContainingSymbol is SourceOrdinaryMethodSymbol { SourcePartialImplementation: var sourcePartialImplementation }))
			{
				return null;
			}
			if ((object)sourcePartialImplementation == null)
			{
				return null;
			}
			return (SourceParameterSymbol)sourcePartialImplementation.Parameters[Ordinal];
		}
	}

	internal sealed override SyntaxList<AttributeListSyntax> AttributeDeclarationList => CSharpSyntaxNode?.AttributeLists ?? default(SyntaxList<AttributeListSyntax>);

	internal override bool HasDefaultArgumentSyntax => (_parameterSyntaxKind & ParameterSyntaxKind.DefaultParameter) != 0;

	internal sealed override bool HasOptionalAttribute
	{
		get
		{
			if (_lazyHasOptionalAttribute == ThreeState.Unknown)
			{
				SourceParameterSymbol boundAttributesSource = BoundAttributesSource;
				if ((object)boundAttributesSource != null)
				{
					_lazyHasOptionalAttribute = boundAttributesSource.HasOptionalAttribute.ToThreeState();
				}
				else if (!GetAttributes().Any())
				{
					_lazyHasOptionalAttribute = ThreeState.False;
				}
			}
			return _lazyHasOptionalAttribute.Value();
		}
	}

	internal override bool IsMetadataOptional
	{
		get
		{
			if (!HasDefaultArgumentSyntax)
			{
				return HasOptionalAttribute;
			}
			return true;
		}
	}

	internal sealed override bool IsMetadataIn
	{
		get
		{
			if (!base.IsMetadataIn)
			{
				return GetDecodedWellKnownAttributeData()?.HasInAttribute ?? false;
			}
			return true;
		}
	}

	internal sealed override bool IsMetadataOut
	{
		get
		{
			if (!base.IsMetadataOut)
			{
				return GetDecodedWellKnownAttributeData()?.HasOutAttribute ?? false;
			}
			return true;
		}
	}

	internal sealed override MarshalPseudoCustomAttributeData MarshallingInformation => GetDecodedWellKnownAttributeData()?.MarshallingInformation;

	public override bool IsParams => (_parameterSyntaxKind & ParameterSyntaxKind.ParamsParameter) != 0;

	internal override bool IsExtensionMethodThis => (_parameterSyntaxKind & ParameterSyntaxKind.ExtensionThisParameter) != 0;

	public override ImmutableArray<CustomModifier> CustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal SourceComplexParameterSymbol(Symbol owner, int ordinal, TypeSymbol parameterType, RefKind refKind, string name, ImmutableArray<Location> locations, SyntaxReference syntaxRef, ConstantValue defaultSyntaxValue, bool isParams, bool isExtensionMethodThis)
		: base(owner, parameterType, ordinal, refKind, name, locations)
	{
		_lazyHasOptionalAttribute = ThreeState.Unknown;
		_syntaxRef = syntaxRef;
		if (isParams)
		{
			_parameterSyntaxKind |= ParameterSyntaxKind.ParamsParameter;
		}
		if (isExtensionMethodThis)
		{
			_parameterSyntaxKind |= ParameterSyntaxKind.ExtensionThisParameter;
		}
		ParameterSyntax cSharpSyntaxNode = CSharpSyntaxNode;
		if (cSharpSyntaxNode != null && cSharpSyntaxNode.Default != null)
		{
			_parameterSyntaxKind |= ParameterSyntaxKind.DefaultParameter;
		}
		_lazyDefaultSyntaxValue = defaultSyntaxValue;
	}

	protected ConstantValue MakeDefaultExpression(DiagnosticBag diagnostics, Binder binder)
	{
		ParameterSyntax cSharpSyntaxNode = CSharpSyntaxNode;
		if (cSharpSyntaxNode == null)
		{
			return null;
		}
		EqualsValueClauseSyntax equalsValueClauseSyntax = cSharpSyntaxNode.Default;
		if (equalsValueClauseSyntax == null)
		{
			return null;
		}
		if (binder == null)
		{
			SyntaxTree syntaxTree = _syntaxRef.SyntaxTree;
			binder = DeclaringCompilation.GetBinderFactory(syntaxTree).GetBinder(equalsValueClauseSyntax);
		}
		BoundExpression boundExpression = binder.CreateBinderForParameterDefaultValue(this, equalsValueClauseSyntax).BindParameterDefaultValue(equalsValueClauseSyntax, this, diagnostics, out var valueBeforeConversion).Value;
		if (valueBeforeConversion.HasErrors)
		{
			return ConstantValue.Bad;
		}
		if (ParameterHelpers.ReportDefaultParameterErrors(binder, ContainingSymbol, cSharpSyntaxNode, this, valueBeforeConversion, diagnostics))
		{
			return ConstantValue.Bad;
		}
		if (boundExpression.ConstantValue == null && boundExpression.Kind == BoundKind.Conversion && (valueBeforeConversion.Kind != BoundKind.DefaultExpression || !(valueBeforeConversion.Type == null)) && parameterType.IsNullableType())
		{
			boundExpression = binder.GenerateConversionForAssignment(parameterType.GetNullableUnderlyingType(), valueBeforeConversion, diagnostics, isDefaultParameter: true);
		}
		ConstantValue constantValue = boundExpression.ConstantValue ?? ConstantValue.Null;
		VerifyParamDefaultValueMatchesAttributeIfAny(constantValue, equalsValueClauseSyntax.Value, diagnostics);
		return constantValue;
	}

	internal virtual OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		SyntaxList<AttributeListSyntax> attributeDeclarationList = AttributeDeclarationList;
		if (!(ContainingSymbol is SourceOrdinaryMethodSymbol { OtherPartOfPartial: var otherPartOfPartial }))
		{
			return OneOrMany.Create(attributeDeclarationList);
		}
		SyntaxList<AttributeListSyntax> syntaxList = (((object)otherPartOfPartial == null) ? default(SyntaxList<AttributeListSyntax>) : ((SourceParameterSymbol)otherPartOfPartial.Parameters[Ordinal]).AttributeDeclarationList);
		if (attributeDeclarationList.Equals(default))
		{
			return OneOrMany.Create(syntaxList);
		}
		if (syntaxList.Equals(default))
		{
			return OneOrMany.Create(attributeDeclarationList);
		}
		return OneOrMany.Create(ImmutableArray.Create(attributeDeclarationList, syntaxList));
	}

	internal CommonParameterWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonParameterWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal ParameterEarlyWellKnownAttributeData GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (ParameterEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	internal sealed override CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		if (_lazyCustomAttributesBag == null || !_lazyCustomAttributesBag.IsSealed)
		{
			SourceParameterSymbol boundAttributesSource = BoundAttributesSource;
			bool flag;
			if ((object)boundAttributesSource != null)
			{
				CustomAttributesBag<CSharpAttributeData> attributesBag = boundAttributesSource.GetAttributesBag();
				flag = Interlocked.CompareExchange(ref _lazyCustomAttributesBag, attributesBag, null) == null;
			}
			else
			{
				OneOrMany<SyntaxList<AttributeListSyntax>> attributeDeclarations = GetAttributeDeclarations();
				flag = LoadAndValidateAttributes(attributeDeclarations, ref _lazyCustomAttributesBag, AttributeLocation.None, earlyDecodingOnly: false, ParameterBinderOpt);
			}
			if (flag)
			{
				state.NotePartComplete(CompletionPart.Attributes);
			}
		}
		return _lazyCustomAttributesBag;
	}

	internal override void EarlyDecodeWellKnownAttributeType(NamedTypeSymbol attributeType, AttributeSyntax attributeSyntax)
	{
		if (CSharpAttributeData.IsTargetEarlyAttribute(attributeType, attributeSyntax, AttributeDescription.OptionalAttribute))
		{
			_lazyHasOptionalAttribute = ThreeState.True;
		}
	}

	internal override void PostEarlyDecodeWellKnownAttributeTypes()
	{
		if (_lazyHasOptionalAttribute == ThreeState.Unknown)
		{
			_lazyHasOptionalAttribute = ThreeState.False;
		}
		base.PostEarlyDecodeWellKnownAttributeTypes();
	}

	internal override CSharpAttributeData EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.DefaultParameterValueAttribute))
		{
			return EarlyDecodeAttributeForDefaultParameterValue(AttributeDescription.DefaultParameterValueAttribute, ref arguments);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.DecimalConstantAttribute))
		{
			return EarlyDecodeAttributeForDefaultParameterValue(AttributeDescription.DecimalConstantAttribute, ref arguments);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.DateTimeConstantAttribute))
		{
			return EarlyDecodeAttributeForDefaultParameterValue(AttributeDescription.DateTimeConstantAttribute, ref arguments);
		}
		if (!IsOnPartialImplementation(arguments.AttributeSyntax))
		{
			if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CallerLineNumberAttribute))
			{
				arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>().HasCallerLineNumberAttribute = true;
			}
			else if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CallerFilePathAttribute))
			{
				arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>().HasCallerFilePathAttribute = true;
			}
			else if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CallerMemberNameAttribute))
			{
				arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>().HasCallerMemberNameAttribute = true;
			}
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	private CSharpAttributeData EarlyDecodeAttributeForDefaultParameterValue(AttributeDescription description, ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, out var generatedDiagnostics);
		ConstantValue defaultParameterValue;
		if (attribute.HasErrors)
		{
			defaultParameterValue = ConstantValue.Bad;
			generatedDiagnostics = true;
		}
		else
		{
			defaultParameterValue = DecodeDefaultParameterValueAttribute(description, attribute, arguments.AttributeSyntax, diagnose: false, null);
		}
		ParameterEarlyWellKnownAttributeData orCreateData = arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>();
		if (orCreateData.DefaultParameterValue == ConstantValue.Unset)
		{
			orCreateData.DefaultParameterValue = defaultParameterValue;
		}
		if (generatedDiagnostics)
		{
			return null;
		}
		return attribute;
	}

	internal override void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(this, AttributeDescription.DefaultParameterValueAttribute))
		{
			DecodeDefaultParameterValueAttribute(AttributeDescription.DefaultParameterValueAttribute, ref arguments);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.DecimalConstantAttribute))
		{
			DecodeDefaultParameterValueAttribute(AttributeDescription.DecimalConstantAttribute, ref arguments);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.DateTimeConstantAttribute))
		{
			DecodeDefaultParameterValueAttribute(AttributeDescription.DateTimeConstantAttribute, ref arguments);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.OptionalAttribute))
		{
			if (HasDefaultArgumentSyntax)
			{
				arguments.Diagnostics.Add(ErrorCode.ERR_DefaultValueUsedWithAttributes, arguments.AttributeSyntaxOpt.Name.Location);
			}
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.ParamArrayAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitParamArray, arguments.AttributeSyntaxOpt.Name.Location);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.InAttribute))
		{
			arguments.GetOrCreateData<CommonParameterWellKnownAttributeData>().HasInAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.OutAttribute))
		{
			arguments.GetOrCreateData<CommonParameterWellKnownAttributeData>().HasOutAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.MarshalAsAttribute))
		{
			MarshalAsAttributeDecoder<CommonParameterWellKnownAttributeData, AttributeSyntax, CSharpAttributeData, AttributeLocation>.Decode(ref arguments, AttributeTargets.Parameter, MessageProvider.Instance);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IDispatchConstantAttribute))
		{
			arguments.GetOrCreateData<CommonParameterWellKnownAttributeData>().HasIDispatchConstantAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.IUnknownConstantAttribute))
		{
			arguments.GetOrCreateData<CommonParameterWellKnownAttributeData>().HasIUnknownConstantAttribute = true;
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.CallerLineNumberAttribute))
		{
			ValidateCallerLineNumberAttribute(arguments.AttributeSyntaxOpt, arguments.Diagnostics);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.CallerFilePathAttribute))
		{
			ValidateCallerFilePathAttribute(arguments.AttributeSyntaxOpt, arguments.Diagnostics);
		}
		else if (attribute.IsTargetAttribute(this, AttributeDescription.CallerMemberNameAttribute))
		{
			ValidateCallerMemberNameAttribute(arguments.AttributeSyntaxOpt, arguments.Diagnostics);
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
		else if (attribute.IsTargetAttribute(this, AttributeDescription.TupleElementNamesAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitTupleElementNamesAttribute, arguments.AttributeSyntaxOpt.Location);
		}
	}

	private void DecodeDefaultParameterValueAttribute(AttributeDescription description, ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		AttributeSyntax attributeSyntaxOpt = arguments.AttributeSyntaxOpt;
		DiagnosticBag diagnostics = arguments.Diagnostics;
		ConstantValue constantValue = DecodeDefaultParameterValueAttribute(description, attribute, attributeSyntaxOpt, diagnose: true, diagnostics);
		if (!constantValue.IsBad)
		{
			VerifyParamDefaultValueMatchesAttributeIfAny(constantValue, attributeSyntaxOpt, diagnostics);
		}
	}

	private void VerifyParamDefaultValueMatchesAttributeIfAny(ConstantValue value, CSharpSyntaxNode syntax, DiagnosticBag diagnostics)
	{
		ParameterEarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = GetEarlyDecodedWellKnownAttributeData();
		if (earlyDecodedWellKnownAttributeData != null)
		{
			ConstantValue defaultParameterValue = earlyDecodedWellKnownAttributeData.DefaultParameterValue;
			if (defaultParameterValue != ConstantValue.Unset && value != defaultParameterValue)
			{
				diagnostics.Add(ErrorCode.ERR_ParamDefaultValueDiffersFromAttribute, syntax.Location);
			}
		}
	}

	private ConstantValue DecodeDefaultParameterValueAttribute(AttributeDescription description, CSharpAttributeData attribute, AttributeSyntax node, bool diagnose, DiagnosticBag diagnosticsOpt)
	{
		if (description.Equals(AttributeDescription.DefaultParameterValueAttribute))
		{
			return DecodeDefaultParameterValueAttribute(attribute, node, diagnose, diagnosticsOpt);
		}
		if (description.Equals(AttributeDescription.DecimalConstantAttribute))
		{
			return attribute.DecodeDecimalConstantValue();
		}
		return attribute.DecodeDateTimeConstantValue();
	}

	private ConstantValue DecodeDefaultParameterValueAttribute(CSharpAttributeData attribute, AttributeSyntax node, bool diagnose, DiagnosticBag diagnosticsOpt)
	{
		if (HasDefaultArgumentSyntax)
		{
			if (diagnose)
			{
				diagnosticsOpt.Add(ErrorCode.ERR_DefaultValueUsedWithAttributes, node.Name.Location);
			}
			return ConstantValue.Bad;
		}
		TypedConstant typedConstant = attribute.CommonConstructorArguments[0];
		SpecialType st = ((typedConstant.Kind == TypedConstantKind.Enum) ? ((INamedTypeSymbol)typedConstant.Type).EnumUnderlyingType.SpecialType : typedConstant.Type.SpecialType);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		ConstantValueTypeDiscriminator constantValueTypeDiscriminator = ConstantValue.GetDiscriminator(st);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (constantValueTypeDiscriminator == ConstantValueTypeDiscriminator.Bad)
		{
			if (typedConstant.Kind == TypedConstantKind.Array || typedConstant.Value != null)
			{
				if (diagnose)
				{
					diagnosticsOpt.Add(ErrorCode.ERR_DefaultValueBadValueType, node.Name.Location, typedConstant.Type);
				}
				return ConstantValue.Bad;
			}
			if (!Type.IsReferenceType)
			{
				if (diagnose)
				{
					diagnosticsOpt.Add(ErrorCode.ERR_DefaultValueTypeMustMatch, node.Name.Location);
				}
				return ConstantValue.Bad;
			}
			constantValueTypeDiscriminator = ConstantValueTypeDiscriminator.Nothing;
		}
		else if (!declaringCompilation.Conversions.ClassifyConversionFromType((TypeSymbol)typedConstant.Type, Type, ref useSiteDiagnostics).Kind.IsImplicitConversion())
		{
			if (diagnose)
			{
				diagnosticsOpt.Add(ErrorCode.ERR_DefaultValueTypeMustMatch, node.Name.Location);
				diagnosticsOpt.Add(node.Name.Location, useSiteDiagnostics);
			}
			return ConstantValue.Bad;
		}
		if (diagnose)
		{
			diagnosticsOpt.Add(node.Name.Location, useSiteDiagnostics);
		}
		return ConstantValue.Create(typedConstant.Value, constantValueTypeDiscriminator);
	}

	private bool IsValidCallerInfoContext(AttributeSyntax node)
	{
		if (!ContainingSymbol.IsExplicitInterfaceImplementation() && !ContainingSymbol.IsOperator())
		{
			return !IsOnPartialImplementation(node);
		}
		return false;
	}

	private bool IsOnPartialImplementation(AttributeSyntax node)
	{
		if (!(ContainingSymbol is MethodSymbol methodSymbol))
		{
			return false;
		}
		MethodSymbol methodSymbol2 = (methodSymbol.IsPartialImplementation() ? methodSymbol : methodSymbol.PartialImplementationPart);
		if ((object)methodSymbol2 == null)
		{
			return false;
		}
		if (!(node.Parent.Parent.Parent is ParameterListSyntax parameterListSyntax))
		{
			return false;
		}
		if (!(parameterListSyntax.Parent is MethodDeclarationSyntax methodDeclarationSyntax))
		{
			return false;
		}
		foreach (SyntaxReference declaringSyntaxReference in methodSymbol2.DeclaringSyntaxReferences)
		{
			if (declaringSyntaxReference.GetSyntax() == methodDeclarationSyntax)
			{
				return true;
			}
		}
		return false;
	}

	private void ValidateCallerLineNumberAttribute(AttributeSyntax node, DiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!IsValidCallerInfoContext(node))
		{
			diagnostics.Add(ErrorCode.WRN_CallerLineNumberParamForUnconsumedLocation, node.Name.Location, CSharpSyntaxNode.Identifier.ValueText);
		}
		else if (!declaringCompilation.Conversions.HasCallerLineNumberConversion(Type, ref useSiteDiagnostics))
		{
			TypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_Int32);
			diagnostics.Add(ErrorCode.ERR_NoConversionForCallerLineNumberParam, node.Name.Location, specialType, Type);
		}
		else if (!HasExplicitDefaultValue && !ContainingSymbol.IsPartialImplementation())
		{
			diagnostics.Add(ErrorCode.ERR_BadCallerLineNumberParamWithoutDefaultValue, node.Name.Location);
		}
		diagnostics.Add(node.Name.Location, useSiteDiagnostics);
	}

	private void ValidateCallerFilePathAttribute(AttributeSyntax node, DiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!IsValidCallerInfoContext(node))
		{
			diagnostics.Add(ErrorCode.WRN_CallerFilePathParamForUnconsumedLocation, node.Name.Location, CSharpSyntaxNode.Identifier.ValueText);
		}
		else if (!declaringCompilation.Conversions.HasCallerInfoStringConversion(Type, ref useSiteDiagnostics))
		{
			TypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_String);
			diagnostics.Add(ErrorCode.ERR_NoConversionForCallerFilePathParam, node.Name.Location, specialType, Type);
		}
		else if (!HasExplicitDefaultValue && !ContainingSymbol.IsPartialImplementation())
		{
			diagnostics.Add(ErrorCode.ERR_BadCallerFilePathParamWithoutDefaultValue, node.Name.Location);
		}
		else if (HasCallerLineNumberAttribute)
		{
			diagnostics.Add(ErrorCode.WRN_CallerLineNumberPreferredOverCallerFilePath, node.Name.Location, CSharpSyntaxNode.Identifier.ValueText);
		}
		diagnostics.Add(node.Name.Location, useSiteDiagnostics);
	}

	private void ValidateCallerMemberNameAttribute(AttributeSyntax node, DiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!IsValidCallerInfoContext(node))
		{
			diagnostics.Add(ErrorCode.WRN_CallerMemberNameParamForUnconsumedLocation, node.Name.Location, CSharpSyntaxNode.Identifier.ValueText);
		}
		else if (!declaringCompilation.Conversions.HasCallerInfoStringConversion(Type, ref useSiteDiagnostics))
		{
			TypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_String);
			diagnostics.Add(ErrorCode.ERR_NoConversionForCallerMemberNameParam, node.Name.Location, specialType, Type);
		}
		else if (!HasExplicitDefaultValue && !ContainingSymbol.IsPartialImplementation())
		{
			diagnostics.Add(ErrorCode.ERR_BadCallerMemberNameParamWithoutDefaultValue, node.Name.Location);
		}
		else if (HasCallerLineNumberAttribute)
		{
			diagnostics.Add(ErrorCode.WRN_CallerLineNumberPreferredOverCallerMemberName, node.Name.Location, CSharpSyntaxNode.Identifier.ValueText);
		}
		else if (HasCallerFilePathAttribute)
		{
			diagnostics.Add(ErrorCode.WRN_CallerFilePathPreferredOverCallerMemberName, node.Name.Location, CSharpSyntaxNode.Identifier.ValueText);
		}
		diagnostics.Add(node.Name.Location, useSiteDiagnostics);
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, DiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		CommonParameterWellKnownAttributeData commonParameterWellKnownAttributeData = (CommonParameterWellKnownAttributeData)decodedData;
		if (commonParameterWellKnownAttributeData != null)
		{
			switch (RefKind)
			{
			case RefKind.Ref:
				if (commonParameterWellKnownAttributeData.HasOutAttribute && !commonParameterWellKnownAttributeData.HasInAttribute)
				{
					diagnostics.Add(ErrorCode.ERR_OutAttrOnRefParam, Locations[0]);
				}
				break;
			case RefKind.Out:
				if (commonParameterWellKnownAttributeData.HasInAttribute)
				{
					diagnostics.Add(ErrorCode.ERR_InAttrOnOutParam, Locations[0]);
				}
				break;
			case RefKind.In:
				if (commonParameterWellKnownAttributeData.HasOutAttribute)
				{
					diagnostics.Add(ErrorCode.ERR_OutAttrOnInParam, Locations[0]);
				}
				break;
			}
		}
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		base.ForceComplete(locationOpt, cancellationToken);
		_ = ExplicitDefaultConstantValue;
	}
}
