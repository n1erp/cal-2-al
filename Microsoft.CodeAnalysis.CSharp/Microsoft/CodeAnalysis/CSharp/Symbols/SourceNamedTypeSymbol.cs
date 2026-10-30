using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceNamedTypeSymbol : SourceMemberContainerTypeSymbol, IAttributeTargetSymbol
{
	private ImmutableArray<TypeParameterSymbol> _lazyTypeParameters;

	private ImmutableArray<TypeParameterConstraintClause> _lazyTypeParameterConstraints;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	private string _lazyDocComment;

	private ThreeState _lazyIsExplicitDefinitionOfNoPiaLocalType;

	private Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> _lazyDeclaredBases;

	private NamedTypeSymbol _lazyBaseType = ErrorTypeSymbol.UnknownResultType;

	private ImmutableArray<NamedTypeSymbol> _lazyInterfaces;

	private SynthesizedEnumValueFieldSymbol _lazyEnumValueField;

	private NamedTypeSymbol _lazyEnumUnderlyingType = ErrorTypeSymbol.UnknownResultType;

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>();

	internal override bool HasTypeArgumentsCustomModifiers => false;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters
	{
		get
		{
			if (_lazyTypeParameters.IsDefault)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				if (ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeParameters, MakeTypeParameters(instance)))
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _lazyTypeParameters;
		}
	}

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => this;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Type;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations
	{
		get
		{
			switch (TypeKind)
			{
			case TypeKind.Delegate:
				return AttributeLocation.Type | AttributeLocation.Return;
			case TypeKind.Enum:
			case TypeKind.Interface:
				return AttributeLocation.Type;
			case TypeKind.Class:
			case TypeKind.Struct:
				return AttributeLocation.Type;
			default:
				return AttributeLocation.None;
			}
		}
	}

	internal override ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
			if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
			{
				return ((CommonTypeEarlyWellKnownAttributeData)lazyCustomAttributesBag.EarlyDecodedWellKnownAttributeData)?.ObsoleteAttributeData;
			}
			foreach (SingleTypeDeclaration declaration in declaration.Declarations)
			{
				if (declaration.HasAnyAttributes)
				{
					return ObsoleteAttributeData.Uninitialized;
				}
			}
			return null;
		}
	}

	internal override bool IsExplicitDefinitionOfNoPiaLocalType
	{
		get
		{
			if (_lazyIsExplicitDefinitionOfNoPiaLocalType == ThreeState.Unknown)
			{
				CheckPresenceOfTypeIdentifierAttribute();
				if (_lazyIsExplicitDefinitionOfNoPiaLocalType == ThreeState.Unknown)
				{
					_lazyIsExplicitDefinitionOfNoPiaLocalType = ThreeState.False;
				}
			}
			return _lazyIsExplicitDefinitionOfNoPiaLocalType == ThreeState.True;
		}
	}

	internal override bool IsComImport => GetEarlyDecodedWellKnownAttributeData()?.HasComImportAttribute ?? false;

	internal override NamedTypeSymbol ComImportCoClass => GetDecodedWellKnownAttributeData()?.ComImportCoClass;

	internal override bool HasSpecialName => GetDecodedWellKnownAttributeData()?.HasSpecialNameAttribute ?? false;

	internal override bool HasCodeAnalysisEmbeddedAttribute => GetEarlyDecodedWellKnownAttributeData()?.HasCodeAnalysisEmbeddedAttribute ?? false;

	internal sealed override bool ShouldAddWinRTMembers => false;

	internal sealed override bool IsWindowsRuntimeImport => GetDecodedWellKnownAttributeData()?.HasWindowsRuntimeImportAttribute ?? false;

	public sealed override bool IsSerializable => GetDecodedWellKnownAttributeData()?.HasSerializableAttribute ?? false;

	internal override bool IsDirectlyExcludedFromCodeCoverage => GetDecodedWellKnownAttributeData()?.HasExcludeFromCodeCoverageAttribute ?? false;

	internal sealed override TypeLayout Layout
	{
		get
		{
			TypeWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData != null && decodedWellKnownAttributeData.HasStructLayoutAttribute)
			{
				return decodedWellKnownAttributeData.Layout;
			}
			if (TypeKind == TypeKind.Struct)
			{
				return new TypeLayout(LayoutKind.Sequential, (!HasInstanceFields()) ? 1 : 0, 0);
			}
			return default;
		}
	}

	internal bool HasStructLayoutAttribute => GetDecodedWellKnownAttributeData()?.HasStructLayoutAttribute ?? false;

	internal override CharSet MarshallingCharSet
	{
		get
		{
			TypeWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData == null || !decodedWellKnownAttributeData.HasStructLayoutAttribute)
			{
				return DefaultMarshallingCharSet;
			}
			return decodedWellKnownAttributeData.MarshallingCharSet;
		}
	}

	internal sealed override bool HasDeclarativeSecurity => GetDecodedWellKnownAttributeData()?.HasDeclarativeSecurity ?? false;

	internal bool HasSecurityCriticalAttributes => GetDecodedWellKnownAttributeData()?.HasSecurityCriticalAttributes ?? false;

	internal sealed override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics
	{
		get
		{
			if ((object)_lazyBaseType == ErrorTypeSymbol.UnknownResultType)
			{
				if ((object)ContainingType != null)
				{
					_ = ContainingType.BaseTypeNoUseSiteDiagnostics;
				}
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				NamedTypeSymbol value = MakeAcyclicBaseType(instance);
				if ((object)Interlocked.CompareExchange(ref _lazyBaseType, value, ErrorTypeSymbol.UnknownResultType) == ErrorTypeSymbol.UnknownResultType)
				{
					AddDeclarationDiagnostics(instance);
				}
				instance.Free();
			}
			return _lazyBaseType;
		}
	}

	public override NamedTypeSymbol EnumUnderlyingType
	{
		get
		{
			if ((object)_lazyEnumUnderlyingType == ErrorTypeSymbol.UnknownResultType)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				if ((object)Interlocked.CompareExchange(ref _lazyEnumUnderlyingType, GetEnumUnderlyingType(instance), ErrorTypeSymbol.UnknownResultType) == ErrorTypeSymbol.UnknownResultType)
				{
					AddDeclarationDiagnostics(instance);
					state.NotePartComplete(CompletionPart.EnumUnderlyingType);
				}
				instance.Free();
			}
			return _lazyEnumUnderlyingType;
		}
	}

	internal FieldSymbol EnumValueField
	{
		get
		{
			if (TypeKind != TypeKind.Enum)
			{
				return null;
			}
			if ((object)_lazyEnumValueField == null)
			{
				Interlocked.CompareExchange(ref _lazyEnumValueField, new SynthesizedEnumValueFieldSymbol(this), null);
			}
			return _lazyEnumValueField;
		}
	}

	protected override Location GetCorrespondingBaseListLocation(NamedTypeSymbol @base)
	{
		Location location = null;
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		foreach (SyntaxReference syntaxReference in SyntaxReferences)
		{
			BaseListSyntax baseList = ((TypeDeclarationSyntax)syntaxReference.GetSyntax()).BaseList;
			if (baseList == null)
			{
				continue;
			}
			SeparatedSyntaxList<BaseTypeSyntax> types = baseList.Types;
			Binder binder = DeclaringCompilation.GetBinder(baseList);
			binder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
			if ((object)location == null)
			{
				location = types[0].Type.GetLocation();
			}
			foreach (BaseTypeSyntax item in types)
			{
				TypeSyntax type = item.Type;
				if (binder.BindType(type, instance) == @base)
				{
					instance.Free();
					return type.GetLocation();
				}
			}
		}
		instance.Free();
		return location;
	}

	internal SourceNamedTypeSymbol(NamespaceOrTypeSymbol containingSymbol, MergedTypeDeclaration declaration, DiagnosticBag diagnostics)
		: base(containingSymbol, declaration, diagnostics)
	{
		if (containingSymbol.Kind == SymbolKind.NamedType)
		{
			_lazyIsExplicitDefinitionOfNoPiaLocalType = ThreeState.False;
		}
	}

	private static SyntaxToken GetName(CSharpSyntaxNode node)
	{
		switch (node.Kind())
		{
		case SyntaxKind.EnumDeclaration:
			return ((EnumDeclarationSyntax)node).Identifier;
		case SyntaxKind.DelegateDeclaration:
			return ((DelegateDeclarationSyntax)node).Identifier;
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
			return ((BaseTypeDeclarationSyntax)node).Identifier;
		default:
			return default;
		}
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return SourceDocumentationCommentUtils.GetAndCacheDocumentationComment(this, expandIncludes, ref _lazyDocComment);
	}

	private ImmutableArray<TypeParameterSymbol> MakeTypeParameters(DiagnosticBag diagnostics)
	{
		if (declaration.Arity == 0)
		{
			return ImmutableArray<TypeParameterSymbol>.Empty;
		}
		bool flag = false;
		string[] array = new string[declaration.Arity];
		string[] array2 = new string[declaration.Arity];
		List<List<TypeParameterBuilder>> list = new List<List<TypeParameterBuilder>>();
		foreach (SyntaxReference syntaxReference in SyntaxReferences)
		{
			CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)syntaxReference.GetSyntax();
			SyntaxTree syntaxTree = syntaxReference.SyntaxTree;
			SyntaxKind syntaxKind = cSharpSyntaxNode.Kind();
			TypeParameterListSyntax typeParameterList;
			switch (syntaxKind)
			{
			case SyntaxKind.ClassDeclaration:
			case SyntaxKind.StructDeclaration:
			case SyntaxKind.InterfaceDeclaration:
				typeParameterList = ((TypeDeclarationSyntax)cSharpSyntaxNode).TypeParameterList;
				break;
			case SyntaxKind.DelegateDeclaration:
				typeParameterList = ((DelegateDeclarationSyntax)cSharpSyntaxNode).TypeParameterList;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(cSharpSyntaxNode.Kind());
			}
			bool flag2 = syntaxKind == SyntaxKind.InterfaceDeclaration || syntaxKind == SyntaxKind.DelegateDeclaration;
			List<TypeParameterBuilder> list2 = new List<TypeParameterBuilder>();
			list.Add(list2);
			int num = 0;
			foreach (TypeParameterSyntax parameter in typeParameterList.Parameters)
			{
				if (parameter.VarianceKeyword.Kind() != SyntaxKind.None && !flag2)
				{
					diagnostics.Add(ErrorCode.ERR_IllegalVarianceSyntax, parameter.VarianceKeyword.GetLocation());
				}
				string text = array[num];
				SourceLocation location = new SourceLocation(parameter.Identifier);
				string text2 = array2[num];
				if (text == null)
				{
					text = (array[num] = parameter.Identifier.ValueText);
					array2[num] = parameter.VarianceKeyword.ValueText;
					int num2 = 0;
					while (true)
					{
						if (num2 < num)
						{
							if (text == array[num2])
							{
								flag = true;
								diagnostics.Add(ErrorCode.ERR_DuplicateTypeParameter, location, text);
								break;
							}
							num2++;
							continue;
						}
						if ((object)ContainingType != null)
						{
							TypeParameterSymbol typeParameterSymbol = ContainingType.FindEnclosingTypeParameter(text);
							if ((object)typeParameterSymbol != null)
							{
								diagnostics.Add(ErrorCode.WRN_TypeParameterSameAsOuterTypeParameter, location, text, typeParameterSymbol.ContainingType);
							}
						}
						break;
					}
				}
				else if (!flag)
				{
					if (text2 != parameter.VarianceKeyword.ValueText)
					{
						flag = true;
						diagnostics.Add(ErrorCode.ERR_PartialWrongTypeParamsVariance, declaration.NameLocations.First(), this);
					}
					else if (text != parameter.Identifier.ValueText)
					{
						flag = true;
						diagnostics.Add(ErrorCode.ERR_PartialWrongTypeParams, declaration.NameLocations.First(), this);
					}
				}
				list2.Add(new TypeParameterBuilder(syntaxTree.GetReference(parameter), this, location));
				num++;
			}
		}
		return list.Transpose().Select((IList<TypeParameterBuilder> builders, int i) => builders[0].MakeSymbol(i, builders, diagnostics)).AsImmutable();
	}

	internal TypeParameterConstraintKind GetTypeParameterConstraints(int ordinal)
	{
		return GetTypeParameterConstraintClause(ordinal)?.Constraints ?? TypeParameterConstraintKind.None;
	}

	internal ImmutableArray<TypeSymbol> GetTypeParameterConstraintTypes(int ordinal)
	{
		return GetTypeParameterConstraintClause(ordinal)?.ConstraintTypes ?? ImmutableArray<TypeSymbol>.Empty;
	}

	private TypeParameterConstraintClause GetTypeParameterConstraintClause(int ordinal)
	{
		if (_lazyTypeParameterConstraints.IsDefault)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			if (ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeParameterConstraints, MakeTypeParameterConstraints(instance)))
			{
				AddDeclarationDiagnostics(instance);
			}
			instance.Free();
		}
		ImmutableArray<TypeParameterConstraintClause> lazyTypeParameterConstraints = _lazyTypeParameterConstraints;
		if (lazyTypeParameterConstraints.Length <= 0)
		{
			return null;
		}
		return lazyTypeParameterConstraints[ordinal];
	}

	private ImmutableArray<TypeParameterConstraintClause> MakeTypeParameterConstraints(DiagnosticBag diagnostics)
	{
		ImmutableArray<TypeParameterSymbol> typeParameters = TypeParameters;
		ImmutableArray<TypeParameterConstraintClause> result = ImmutableArray<TypeParameterConstraintClause>.Empty;
		int length = typeParameters.Length;
		if (length > 0)
		{
			foreach (SingleTypeDeclaration declaration in declaration.Declarations)
			{
				SyntaxReference syntaxReference = declaration.SyntaxReference;
				SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = GetConstraintClauses((CSharpSyntaxNode)syntaxReference.GetSyntax());
				if (constraintClauses.Count == 0)
				{
					continue;
				}
				ImmutableArray<TypeParameterConstraintClause> immutableArray = DeclaringCompilation.GetBinderFactory(syntaxReference.SyntaxTree).GetBinder(constraintClauses[0]).WithContainingMemberOrLambda(this)
					.WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.GenericConstraintsClause)
					.BindTypeParameterConstraintClauses(this, typeParameters, constraintClauses, diagnostics);
				if (result.Length == 0)
				{
					this.CheckConstraintTypesVisibility(declaration.NameLocation, immutableArray, diagnostics);
					result = immutableArray;
					continue;
				}
				for (int i = 0; i < length; i++)
				{
					if (!HaveSameConstraints(result[i], immutableArray[i]))
					{
						diagnostics.Add(ErrorCode.ERR_PartialWrongConstraints, Locations[0], this, typeParameters[i]);
					}
				}
			}
		}
		return result;
	}

	private static SyntaxList<TypeParameterConstraintClauseSyntax> GetConstraintClauses(CSharpSyntaxNode node)
	{
		switch (node.Kind())
		{
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
			return ((TypeDeclarationSyntax)node).ConstraintClauses;
		case SyntaxKind.DelegateDeclaration:
			return ((DelegateDeclarationSyntax)node).ConstraintClauses;
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind());
		}
	}

	private static bool HaveSameConstraints(TypeParameterConstraintClause clause1, TypeParameterConstraintClause clause2)
	{
		if (clause1 == null || clause2 == null)
		{
			if (clause1 == null)
			{
				return clause2 == null;
			}
			return false;
		}
		if (clause1.Constraints != clause2.Constraints)
		{
			return false;
		}
		ImmutableArray<TypeSymbol> constraintTypes = clause1.ConstraintTypes;
		ImmutableArray<TypeSymbol> constraintTypes2 = clause2.ConstraintTypes;
		int length = constraintTypes.Length;
		if (constraintTypes2.Length != length)
		{
			return false;
		}
		HashSet<TypeSymbol> hashSet = new HashSet<TypeSymbol>();
		foreach (TypeSymbol item in constraintTypes2)
		{
			hashSet.Add(item);
		}
		foreach (TypeSymbol item2 in constraintTypes)
		{
			if (!hashSet.Contains(item2))
			{
				return false;
			}
		}
		return true;
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		return GetEmptyTypeArgumentCustomModifiers(ordinal);
	}

	internal ImmutableArray<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return declaration.GetAttributeDeclarations();
	}

	private CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsSealed)
		{
			return lazyCustomAttributesBag;
		}
		if (LoadAndValidateAttributes(OneOrMany.Create(GetAttributeDeclarations()), ref _lazyCustomAttributesBag))
		{
			state.NotePartComplete(CompletionPart.Attributes);
		}
		return _lazyCustomAttributesBag;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return GetAttributesBag().Attributes;
	}

	private TypeWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (TypeWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal CommonTypeEarlyWellKnownAttributeData GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (CommonTypeEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	internal override CSharpAttributeData EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		bool generatedDiagnostics;
		CSharpAttributeData attribute;
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.ComImportAttribute))
		{
			attribute = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, out generatedDiagnostics);
			if (!attribute.HasErrors)
			{
				arguments.GetOrCreateData<CommonTypeEarlyWellKnownAttributeData>().HasComImportAttribute = true;
				if (!generatedDiagnostics)
				{
					return attribute;
				}
			}
			return null;
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CodeAnalysisEmbeddedAttribute))
		{
			attribute = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, out generatedDiagnostics);
			if (!attribute.HasErrors)
			{
				arguments.GetOrCreateData<CommonTypeEarlyWellKnownAttributeData>().HasCodeAnalysisEmbeddedAttribute = true;
				if (!generatedDiagnostics)
				{
					return attribute;
				}
			}
			return null;
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.ConditionalAttribute))
		{
			attribute = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, out generatedDiagnostics);
			if (!attribute.HasErrors)
			{
				string constructorArgument = attribute.GetConstructorArgument<string>(0, SpecialType.System_String);
				arguments.GetOrCreateData<CommonTypeEarlyWellKnownAttributeData>().AddConditionalSymbol(constructorArgument);
				if (!generatedDiagnostics)
				{
					return attribute;
				}
			}
			return null;
		}
		if (Symbol.EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref arguments, out attribute, out var obsoleteData))
		{
			if (obsoleteData != null)
			{
				arguments.GetOrCreateData<CommonTypeEarlyWellKnownAttributeData>().ObsoleteAttributeData = obsoleteData;
			}
			return attribute;
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.AttributeUsageAttribute))
		{
			attribute = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, out generatedDiagnostics);
			if (!attribute.HasErrors)
			{
				AttributeUsageInfo attributeUsageInfo = DecodeAttributeUsageAttribute(attribute, arguments.AttributeSyntax, diagnose: false);
				if (!attributeUsageInfo.IsNull)
				{
					CommonTypeEarlyWellKnownAttributeData orCreateData = arguments.GetOrCreateData<CommonTypeEarlyWellKnownAttributeData>();
					if (orCreateData.AttributeUsageInfo.IsNull)
					{
						orCreateData.AttributeUsageInfo = attributeUsageInfo;
					}
					if (!generatedDiagnostics)
					{
						return attribute;
					}
				}
			}
			return null;
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		CommonTypeEarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = GetEarlyDecodedWellKnownAttributeData();
		if (earlyDecodedWellKnownAttributeData != null && !earlyDecodedWellKnownAttributeData.AttributeUsageInfo.IsNull)
		{
			return earlyDecodedWellKnownAttributeData.AttributeUsageInfo;
		}
		if ((object)BaseTypeNoUseSiteDiagnostics == null)
		{
			return AttributeUsageInfo.Default;
		}
		return BaseTypeNoUseSiteDiagnostics.GetAttributeUsageInfo();
	}

	internal sealed override void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (attribute.IsTargetAttribute(this, AttributeDescription.AttributeUsageAttribute))
		{
			DecodeAttributeUsageAttribute(attribute, arguments.AttributeSyntaxOpt, diagnose: true, arguments.Diagnostics);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.DefaultMemberAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasDefaultMemberAttribute = true;
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.CoClassAttribute))
		{
			DecodeCoClassAttribute(ref arguments);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.ConditionalAttribute))
		{
			ValidateConditionalAttribute(attribute, arguments.AttributeSyntaxOpt, arguments.Diagnostics);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.GuidAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().GuidString = attribute.DecodeGuidAttribute(arguments.AttributeSyntaxOpt, arguments.Diagnostics);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.SpecialNameAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasSpecialNameAttribute = true;
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.SerializableAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasSerializableAttribute = true;
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.ExcludeFromCodeCoverageAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasExcludeFromCodeCoverageAttribute = true;
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.StructLayoutAttribute))
		{
			AttributeData.DecodeStructLayoutAttribute<TypeWellKnownAttributeData, AttributeSyntax, CSharpAttributeData, AttributeLocation>(ref arguments, DefaultMarshallingCharSet, 0, MessageProvider.Instance);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.SuppressUnmanagedCodeSecurityAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasSuppressUnmanagedCodeSecurityAttribute = true;
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.ClassInterfaceAttribute))
		{
			attribute.DecodeClassInterfaceAttribute(arguments.AttributeSyntaxOpt, arguments.Diagnostics);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.InterfaceTypeAttribute))
		{
			attribute.DecodeInterfaceTypeAttribute(arguments.AttributeSyntaxOpt, arguments.Diagnostics);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.WindowsRuntimeImportAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasWindowsRuntimeImportAttribute = true;
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.RequiredAttributeAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_CantUseRequiredAttribute, arguments.AttributeSyntaxOpt.Name.Location);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.CaseSensitiveExtensionAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitExtension, arguments.AttributeSyntaxOpt.Location);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.DynamicAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitDynamicAttr, arguments.AttributeSyntaxOpt.Location);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.IsReadOnlyAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsReadOnlyAttribute.FullName);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.IsUnmanagedAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsUnmanagedAttribute.FullName);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.IsByRefLikeAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitReservedAttr, arguments.AttributeSyntaxOpt.Location, AttributeDescription.IsByRefLikeAttribute.FullName);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.TupleElementNamesAttribute))
		{
			arguments.Diagnostics.Add(ErrorCode.ERR_ExplicitTupleElementNamesAttribute, arguments.AttributeSyntaxOpt.Location);
			return;
		}
		if (attribute.IsTargetAttribute(this, AttributeDescription.SecurityCriticalAttribute) || attribute.IsTargetAttribute(this, AttributeDescription.SecuritySafeCriticalAttribute))
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().HasSecurityCriticalAttributes = true;
			return;
		}
		if (_lazyIsExplicitDefinitionOfNoPiaLocalType == ThreeState.Unknown && attribute.IsTargetAttribute(this, AttributeDescription.TypeIdentifierAttribute))
		{
			_lazyIsExplicitDefinitionOfNoPiaLocalType = ThreeState.True;
			return;
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (attribute.IsSecurityAttribute(declaringCompilation))
		{
			attribute.DecodeSecurityAttribute<TypeWellKnownAttributeData>(this, declaringCompilation, ref arguments);
		}
	}

	private void CheckPresenceOfTypeIdentifierAttribute()
	{
		CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag = _lazyCustomAttributesBag;
		if (lazyCustomAttributesBag != null && lazyCustomAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			return;
		}
		foreach (SyntaxList<AttributeListSyntax> attributeDeclaration in GetAttributeDeclarations())
		{
			_ = attributeDeclaration.Node.SyntaxTree;
			QuickAttributeChecker quickAttributeChecker = DeclaringCompilation.GetBinderFactory(attributeDeclaration.Node.SyntaxTree).GetBinder(attributeDeclaration.Node).QuickAttributeChecker;
			foreach (AttributeListSyntax item in attributeDeclaration)
			{
				foreach (AttributeSyntax attribute in item.Attributes)
				{
					if (quickAttributeChecker.IsPossibleMatch(attribute, QuickAttributes.TypeIdentifier))
					{
						GetAttributes();
						return;
					}
				}
			}
		}
	}

	private AttributeUsageInfo DecodeAttributeUsageAttribute(CSharpAttributeData attribute, AttributeSyntax node, bool diagnose, DiagnosticBag diagnosticsOpt = null)
	{
		if (!DeclaringCompilation.IsAttributeType(this))
		{
			if (diagnose)
			{
				diagnosticsOpt.Add(ErrorCode.ERR_AttributeUsageOnNonAttributeClass, node.Name.Location, node.GetErrorDisplayName());
			}
			return AttributeUsageInfo.Null;
		}
		AttributeUsageInfo result = attribute.DecodeAttributeUsageAttribute();
		if (!result.HasValidAttributeTargets)
		{
			if (diagnose)
			{
				CSharpSyntaxNode attributeArgumentSyntax = attribute.GetAttributeArgumentSyntax(0, node);
				diagnosticsOpt.Add(ErrorCode.ERR_InvalidAttributeArgument, attributeArgumentSyntax.Location, node.GetErrorDisplayName());
			}
			return AttributeUsageInfo.Null;
		}
		return result;
	}

	private void DecodeCoClassAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		if (this.IsInterfaceType() && (!arguments.HasDecodedData || (object)((TypeWellKnownAttributeData)arguments.DecodedData).ComImportCoClass == null) && attribute.CommonConstructorArguments[0].Value is NamedTypeSymbol { TypeKind: TypeKind.Class } namedTypeSymbol)
		{
			arguments.GetOrCreateData<TypeWellKnownAttributeData>().ComImportCoClass = namedTypeSymbol;
		}
	}

	private void ValidateConditionalAttribute(CSharpAttributeData attribute, AttributeSyntax node, DiagnosticBag diagnostics)
	{
		if (!DeclaringCompilation.IsAttributeType(this))
		{
			diagnostics.Add(ErrorCode.ERR_ConditionalOnNonAttributeClass, node.Location, node.GetErrorDisplayName());
			return;
		}
		string constructorArgument = attribute.GetConstructorArgument<string>(0, SpecialType.System_String);
		if (constructorArgument == null || !SyntaxFacts.IsValidIdentifier(constructorArgument))
		{
			CSharpSyntaxNode attributeArgumentSyntax = attribute.GetAttributeArgumentSyntax(0, node);
			diagnostics.Add(ErrorCode.ERR_BadArgumentToAttribute, attributeArgumentSyntax.Location, node.GetErrorDisplayName());
		}
	}

	private bool HasInstanceFields()
	{
		ImmutableArray<Symbol> membersUnordered = GetMembersUnordered();
		for (int i = 0; i < membersUnordered.Length; i++)
		{
			Symbol symbol = membersUnordered[i];
			if (symbol.IsStatic)
			{
				continue;
			}
			switch (symbol.Kind)
			{
			case SymbolKind.Field:
				return true;
			case SymbolKind.Event:
				if (((EventSymbol)symbol).AssociatedField != null)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		CustomAttributesBag<CSharpAttributeData> attributesBag = GetAttributesBag();
		TypeWellKnownAttributeData typeWellKnownAttributeData = (TypeWellKnownAttributeData)attributesBag.DecodedWellKnownAttributeData;
		if (typeWellKnownAttributeData != null)
		{
			SecurityWellKnownAttributeData securityInformation = typeWellKnownAttributeData.SecurityInformation;
			if (securityInformation != null)
			{
				return securityInformation.GetSecurityAttributes(attributesBag.Attributes);
			}
		}
		return null;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return GetEarlyDecodedWellKnownAttributeData()?.ConditionalSymbols ?? ImmutableArray<string>.Empty;
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, DiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		TypeWellKnownAttributeData typeWellKnownAttributeData = (TypeWellKnownAttributeData)decodedData;
		if (IsComImport)
		{
			if (typeWellKnownAttributeData == null || typeWellKnownAttributeData.GuidString == null)
			{
				int index = boundAttributes.IndexOfAttribute(this, AttributeDescription.ComImportAttribute);
				diagnostics.Add(ErrorCode.ERR_ComImportWithoutUuidAttribute, allAttributeSyntaxNodes[index].Name.Location, Name);
			}
			if (TypeKind == TypeKind.Class)
			{
				NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
				if ((object)baseTypeNoUseSiteDiagnostics != null && baseTypeNoUseSiteDiagnostics.SpecialType != SpecialType.System_Object)
				{
					diagnostics.Add(ErrorCode.ERR_ComImportWithBase, Locations[0], Name);
				}
				ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> staticInitializers = StaticInitializers;
				if (!staticInitializers.IsDefaultOrEmpty)
				{
					foreach (ImmutableArray<FieldOrPropertyInitializer> item in staticInitializers)
					{
						foreach (FieldOrPropertyInitializer item2 in item)
						{
							if (!item2.FieldOpt.IsMetadataConstant)
							{
								diagnostics.Add(ErrorCode.ERR_ComImportWithInitializers, item2.Syntax.GetLocation(), Name);
							}
						}
					}
				}
				staticInitializers = InstanceInitializers;
				if (!staticInitializers.IsDefaultOrEmpty)
				{
					foreach (ImmutableArray<FieldOrPropertyInitializer> item3 in staticInitializers)
					{
						foreach (FieldOrPropertyInitializer item4 in item3)
						{
							diagnostics.Add(ErrorCode.ERR_ComImportWithInitializers, item4.Syntax.GetLocation(), Name);
						}
					}
				}
			}
		}
		else if ((object)ComImportCoClass != null)
		{
			int index2 = boundAttributes.IndexOfAttribute(this, AttributeDescription.CoClassAttribute);
			diagnostics.Add(ErrorCode.WRN_CoClassWithoutComImport, allAttributeSyntaxNodes[index2].Location, Name);
		}
		if (typeWellKnownAttributeData != null && typeWellKnownAttributeData.HasDefaultMemberAttribute && Indexers.Any())
		{
			int index3 = boundAttributes.IndexOfAttribute(this, AttributeDescription.DefaultMemberAttribute);
			diagnostics.Add(ErrorCode.ERR_DefaultMemberOnIndexedType, allAttributeSyntaxNodes[index3].Name.Location);
		}
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (ContainsExtensionMethods)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_ExtensionAttribute__ctor));
		}
		if (IsByRefLikeType)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsByRefLikeAttribute(this));
			if (ObsoleteAttributeData == null && !this.IsRestrictedType(ignoreSpanLikeTypes: true))
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_ObsoleteAttribute__ctor, ImmutableArray.Create(new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_String), TypedConstantKind.Primitive, "Types with embedded references are not supported in this version of your compiler."), new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_Boolean), TypedConstantKind.Primitive, true)), default, isOptionalUse: true));
			}
		}
		if (IsReadOnly)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(this));
		}
		if (Indexers.Any())
		{
			string metadataName = Indexers.First().MetadataName;
			TypedConstant item = new TypedConstant(declaringCompilation.GetSpecialType(SpecialType.System_String), TypedConstantKind.Primitive, metadataName);
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Reflection_DefaultMemberAttribute__ctor, ImmutableArray.Create(item)));
		}
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			if (baseTypeNoUseSiteDiagnostics.ContainsDynamic())
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(baseTypeNoUseSiteDiagnostics, 0));
			}
			if (baseTypeNoUseSiteDiagnostics.ContainsTupleNames())
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeTupleNamesAttribute(baseTypeNoUseSiteDiagnostics));
			}
		}
	}

	internal sealed override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved)
	{
		if (_lazyInterfaces.IsDefault)
		{
			if (basesBeingResolved != null && basesBeingResolved.ContainsReference(OriginalDefinition))
			{
				return ImmutableArray<NamedTypeSymbol>.Empty;
			}
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			ImmutableArray<NamedTypeSymbol> value = MakeAcyclicInterfaces(basesBeingResolved, instance);
			if (ImmutableInterlocked.InterlockedCompareExchange(ref _lazyInterfaces, value, default).IsDefault)
			{
				AddDeclarationDiagnostics(instance);
			}
			instance.Free();
		}
		return _lazyInterfaces;
	}

	protected override void CheckBase(DiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			if (IsGenericType && !baseTypeNoUseSiteDiagnostics.IsErrorType() && DeclaringCompilation.IsAttributeType(baseTypeNoUseSiteDiagnostics))
			{
				SourceLocation location = FindBaseRefSyntax(baseTypeNoUseSiteDiagnostics);
				diagnostics.Add(ErrorCode.ERR_GenericDerivingFromAttribute, location, baseTypeNoUseSiteDiagnostics);
			}
			SingleTypeDeclaration singleTypeDeclaration = FirstDeclarationWithExplicitBases();
			if (singleTypeDeclaration != null)
			{
				TypeConversions conversions = new TypeConversions(ContainingAssembly.CorLibrary);
				SourceLocation nameLocation = singleTypeDeclaration.NameLocation;
				baseTypeNoUseSiteDiagnostics.CheckAllConstraints(conversions, nameLocation, diagnostics);
			}
		}
	}

	protected override void CheckInterfaces(DiagnosticBag diagnostics)
	{
		ImmutableHashSet<NamedTypeSymbol> interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics = InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics;
		if (interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.IsEmpty)
		{
			return;
		}
		SingleTypeDeclaration singleTypeDeclaration = FirstDeclarationWithExplicitBases();
		if (singleTypeDeclaration == null)
		{
			return;
		}
		TypeConversions conversions = new TypeConversions(ContainingAssembly.CorLibrary);
		SourceLocation nameLocation = singleTypeDeclaration.NameLocation;
		foreach (NamedTypeSymbol item in interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics)
		{
			item.CheckAllConstraints(conversions, nameLocation, diagnostics);
		}
		if (interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Count <= 1)
		{
			return;
		}
		Dictionary<NamedTypeSymbol, NamedTypeSymbol> dictionary = new Dictionary<NamedTypeSymbol, NamedTypeSymbol>(EqualsIgnoringComparer.InstanceIgnoringTupleNames);
		foreach (NamedTypeSymbol item2 in interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics)
		{
			if (dictionary.TryGetValue(item2, out var value))
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateInterfaceWithTupleNamesInBaseList, nameLocation, item2, value, this);
			}
			else
			{
				dictionary.Add(item2, item2);
			}
		}
	}

	private SourceLocation FindBaseRefSyntax(NamedTypeSymbol baseSym)
	{
		foreach (SingleTypeDeclaration declaration in declaration.Declarations)
		{
			BaseListSyntax baseListOpt = GetBaseListOpt(declaration);
			if (baseListOpt == null)
			{
				continue;
			}
			Binder binder = DeclaringCompilation.GetBinder(baseListOpt);
			binder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
			foreach (BaseTypeSyntax type2 in baseListOpt.Types)
			{
				TypeSyntax type = type2.Type;
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				TypeSymbol other = binder.BindType(type, instance);
				instance.Free();
				if (baseSym.Equals(other))
				{
					return new SourceLocation(type);
				}
			}
		}
		return null;
	}

	private SingleTypeDeclaration FirstDeclarationWithExplicitBases()
	{
		foreach (SingleTypeDeclaration declaration in declaration.Declarations)
		{
			if (GetBaseListOpt(declaration) != null)
			{
				return declaration;
			}
		}
		return null;
	}

	internal Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> GetDeclaredBases(ConsList<Symbol> basesBeingResolved)
	{
		if (_lazyDeclaredBases == null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			if (Interlocked.CompareExchange(ref _lazyDeclaredBases, MakeDeclaredBases(basesBeingResolved, instance), null) == null)
			{
				AddDeclarationDiagnostics(instance);
			}
			instance.Free();
		}
		return _lazyDeclaredBases;
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<Symbol> basesBeingResolved)
	{
		return GetDeclaredBases(basesBeingResolved).Item1;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<Symbol> basesBeingResolved)
	{
		return GetDeclaredBases(basesBeingResolved).Item2;
	}

	private Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> MakeDeclaredBases(ConsList<Symbol> basesBeingResolved, DiagnosticBag diagnostics)
	{
		if (TypeKind == TypeKind.Enum)
		{
			return new Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>>(null, ImmutableArray<NamedTypeSymbol>.Empty);
		}
		bool flag = false;
		ConsList<Symbol> newBasesBeingResolved = basesBeingResolved.Prepend(OriginalDefinition);
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		NamedTypeSymbol namedTypeSymbol = null;
		SourceLocation location = null;
		PooledDictionary<NamedTypeSymbol, SourceLocation> instance2 = PooledDictionary<NamedTypeSymbol, SourceLocation>.GetInstance();
		foreach (SingleTypeDeclaration declaration in declaration.Declarations)
		{
			Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> tuple = MakeOneDeclaredBases(newBasesBeingResolved, declaration, diagnostics);
			if (tuple == null)
			{
				continue;
			}
			NamedTypeSymbol item = tuple.Item1;
			ImmutableArray<NamedTypeSymbol> immutableArray = tuple.Item2;
			if (!flag)
			{
				if ((object)namedTypeSymbol == null)
				{
					namedTypeSymbol = item;
					location = declaration.NameLocation;
				}
				else if (namedTypeSymbol.TypeKind == TypeKind.Error && (object)item != null)
				{
					immutableArray = immutableArray.Add(namedTypeSymbol);
					namedTypeSymbol = item;
					location = declaration.NameLocation;
				}
				else if ((object)item != null && item != namedTypeSymbol && item.TypeKind != TypeKind.Error)
				{
					CSDiagnosticInfo errorInfo = diagnostics.Add(ErrorCode.ERR_PartialMultipleBases, Locations[0], this);
					namedTypeSymbol = new ExtendedErrorTypeSymbol(namedTypeSymbol, LookupResultKind.Ambiguous, errorInfo);
					location = declaration.NameLocation;
					flag = true;
				}
			}
			foreach (NamedTypeSymbol item3 in immutableArray)
			{
				if (!instance2.ContainsKey(item3))
				{
					instance.Add(item3);
					instance2.Add(item3, declaration.NameLocation);
				}
			}
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if ((object)namedTypeSymbol != null)
		{
			if (namedTypeSymbol.IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_StaticBaseClass, location, namedTypeSymbol, this);
			}
			if (!this.IsNoMoreVisibleThan(namedTypeSymbol, ref useSiteDiagnostics))
			{
				diagnostics.Add(ErrorCode.ERR_BadVisBaseClass, location, this, namedTypeSymbol);
			}
		}
		ImmutableArray<NamedTypeSymbol> item2 = instance.ToImmutableAndFree();
		if (DeclaredAccessibility != Accessibility.Private && IsInterface)
		{
			foreach (NamedTypeSymbol item4 in item2)
			{
				if (!item4.IsAtLeastAsVisibleAs(this, ref useSiteDiagnostics))
				{
					diagnostics.Add(ErrorCode.ERR_BadVisBaseInterface, instance2[item4], this, item4);
				}
			}
		}
		instance2.Free();
		diagnostics.Add(Locations[0], useSiteDiagnostics);
		return new Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>>(namedTypeSymbol, item2);
	}

	private static BaseListSyntax GetBaseListOpt(SingleTypeDeclaration decl)
	{
		if (decl.HasBaseDeclarations)
		{
			return ((BaseTypeDeclarationSyntax)decl.SyntaxReference.GetSyntax()).BaseList;
		}
		return null;
	}

	private Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>> MakeOneDeclaredBases(ConsList<Symbol> newBasesBeingResolved, SingleTypeDeclaration decl, DiagnosticBag diagnostics)
	{
		BaseListSyntax baseListOpt = GetBaseListOpt(decl);
		if (baseListOpt == null)
		{
			return null;
		}
		NamedTypeSymbol namedTypeSymbol = null;
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		Binder binder = DeclaringCompilation.GetBinder(baseListOpt);
		binder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
		int num = -1;
		foreach (BaseTypeSyntax type2 in baseListOpt.Types)
		{
			num++;
			TypeSyntax type = type2.Type;
			if (type.Kind() != SyntaxKind.PredefinedType && !SyntaxFacts.IsName(type.Kind()))
			{
				diagnostics.Add(ErrorCode.ERR_BadBaseType, type.GetLocation());
			}
			SourceLocation location = new SourceLocation(type);
			TypeSymbol typeSymbol;
			if (num == 0 && TypeKind == TypeKind.Class)
			{
				typeSymbol = binder.BindType(type, diagnostics, newBasesBeingResolved);
				SpecialType specialType = typeSymbol.SpecialType;
				if (IsRestrictedBaseType(specialType) && (SpecialType != SpecialType.System_Enum || specialType != SpecialType.System_ValueType) && (SpecialType != SpecialType.System_MulticastDelegate || specialType != SpecialType.System_Delegate) && (specialType != SpecialType.System_Array || !(ContainingAssembly.CorLibrary == ContainingAssembly)))
				{
					diagnostics.Add(ErrorCode.ERR_DeriveFromEnumOrValueType, Locations[0], this, typeSymbol);
					continue;
				}
				if (typeSymbol.IsSealed && !IsStatic)
				{
					diagnostics.Add(ErrorCode.ERR_CantDeriveFromSealedType, Locations[0], this, typeSymbol);
					continue;
				}
				bool flag = false;
				if (typeSymbol.TypeKind == TypeKind.Error)
				{
					flag = true;
					if (typeSymbol.GetNonErrorTypeKindGuess() == TypeKind.Interface)
					{
						flag = false;
					}
				}
				if (((typeSymbol.TypeKind == TypeKind.Class || typeSymbol.TypeKind == TypeKind.Delegate || typeSymbol.TypeKind == TypeKind.Struct) | flag) && (object)namedTypeSymbol == null)
				{
					namedTypeSymbol = (NamedTypeSymbol)typeSymbol;
					if (IsStatic && namedTypeSymbol.SpecialType != SpecialType.System_Object)
					{
						CSDiagnosticInfo errorInfo = diagnostics.Add(ErrorCode.ERR_StaticDerivedFromNonObject, location, this, namedTypeSymbol);
						namedTypeSymbol = new ExtendedErrorTypeSymbol(namedTypeSymbol, LookupResultKind.NotReferencable, errorInfo);
					}
					continue;
				}
			}
			else
			{
				typeSymbol = binder.BindType(type, diagnostics, newBasesBeingResolved);
			}
			switch (typeSymbol.TypeKind)
			{
			case TypeKind.Interface:
				foreach (NamedTypeSymbol item in instance)
				{
					if (item == typeSymbol)
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateInterfaceInBaseList, location, typeSymbol);
					}
				}
				if (IsStatic)
				{
					diagnostics.Add(ErrorCode.ERR_StaticClassInterfaceImpl, location, this, typeSymbol);
				}
				if (IsByRefLikeType)
				{
					diagnostics.Add(ErrorCode.ERR_RefStructInterfaceImpl, location, this, typeSymbol);
				}
				if (typeSymbol.ContainsDynamic())
				{
					diagnostics.Add(ErrorCode.ERR_DeriveFromConstructedDynamic, location, this, typeSymbol);
				}
				instance.Add((NamedTypeSymbol)typeSymbol);
				continue;
			case TypeKind.Class:
				if (TypeKind == TypeKind.Class)
				{
					if ((object)namedTypeSymbol == null)
					{
						namedTypeSymbol = (NamedTypeSymbol)typeSymbol;
						diagnostics.Add(ErrorCode.ERR_BaseClassMustBeFirst, location, typeSymbol);
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_NoMultipleInheritance, location, this, namedTypeSymbol, typeSymbol);
					}
					continue;
				}
				break;
			case TypeKind.TypeParameter:
				diagnostics.Add(ErrorCode.ERR_DerivingFromATyVar, location, typeSymbol);
				continue;
			case TypeKind.Error:
				instance.Add((NamedTypeSymbol)typeSymbol);
				continue;
			case TypeKind.Dynamic:
				diagnostics.Add(ErrorCode.ERR_DeriveFromDynamic, location, this);
				continue;
			case TypeKind.Submission:
				throw ExceptionUtilities.UnexpectedValue(typeSymbol.TypeKind);
			}
			diagnostics.Add(ErrorCode.ERR_NonInterfaceInInterfaceList, location, typeSymbol);
		}
		if (SpecialType == SpecialType.System_Object && ((object)namedTypeSymbol != null || instance.Count != 0))
		{
			diagnostics.Add(ErrorCode.ERR_ObjectCantHaveBases, new SourceLocation(GetName(baseListOpt.Parent)));
		}
		return new Tuple<NamedTypeSymbol, ImmutableArray<NamedTypeSymbol>>(namedTypeSymbol, instance.ToImmutableAndFree());
	}

	private static bool IsRestrictedBaseType(SpecialType specialType)
	{
		if ((uint)(specialType - 2) <= 3u || specialType == SpecialType.System_Array)
		{
			return true;
		}
		return false;
	}

	private ImmutableArray<NamedTypeSymbol> MakeAcyclicInterfaces(ConsList<Symbol> basesBeingResolved, DiagnosticBag diagnostics)
	{
		TypeKind typeKind = TypeKind;
		if (typeKind == TypeKind.Enum)
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		ImmutableArray<NamedTypeSymbol> declaredInterfaces = GetDeclaredInterfaces(basesBeingResolved);
		bool flag = typeKind == TypeKind.Class;
		ArrayBuilder<NamedTypeSymbol> arrayBuilder = (flag ? null : ArrayBuilder<NamedTypeSymbol>.GetInstance());
		foreach (NamedTypeSymbol item in declaredInterfaces)
		{
			if (!flag)
			{
				if (BaseTypeAnalysis.InterfaceDependsOn(item, this))
				{
					arrayBuilder.Add(new ExtendedErrorTypeSymbol(item, LookupResultKind.NotReferencable, diagnostics.Add(ErrorCode.ERR_CycleInInterfaceInheritance, Locations[0], this, item)));
					continue;
				}
				arrayBuilder.Add(item);
			}
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			if (item.DeclaringCompilation != DeclaringCompilation)
			{
				item.AddUseSiteDiagnostics(ref useSiteDiagnostics);
				foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in item.AllInterfacesNoUseSiteDiagnostics)
				{
					if (allInterfacesNoUseSiteDiagnostic.DeclaringCompilation != DeclaringCompilation)
					{
						allInterfacesNoUseSiteDiagnostic.AddUseSiteDiagnostics(ref useSiteDiagnostics);
					}
				}
			}
			if (!useSiteDiagnostics.IsNullOrEmpty())
			{
				diagnostics.Add(Locations[0], useSiteDiagnostics);
			}
		}
		if (!flag)
		{
			return arrayBuilder.ToImmutableAndFree();
		}
		return declaredInterfaces;
	}

	private NamedTypeSymbol MakeAcyclicBaseType(DiagnosticBag diagnostics)
	{
		TypeKind typeKind = TypeKind;
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		NamedTypeSymbol namedTypeSymbol = ((typeKind != TypeKind.Enum) ? GetDeclaredBaseType(null) : declaringCompilation.GetSpecialType(SpecialType.System_Enum));
		if ((object)namedTypeSymbol == null)
		{
			switch (typeKind)
			{
			case TypeKind.Class:
				if (SpecialType == SpecialType.System_Object)
				{
					return null;
				}
				namedTypeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_Object);
				break;
			case TypeKind.Struct:
				namedTypeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_ValueType);
				break;
			case TypeKind.Interface:
				return null;
			case TypeKind.Delegate:
				namedTypeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_MulticastDelegate);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(typeKind);
			}
		}
		if (BaseTypeAnalysis.ClassDependsOn(namedTypeSymbol, this))
		{
			return new ExtendedErrorTypeSymbol(namedTypeSymbol, LookupResultKind.NotReferencable, diagnostics.Add(ErrorCode.ERR_CircularBase, Locations[0], namedTypeSymbol, this));
		}
		SetKnownToHaveNoDeclaredBaseCycles();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		NamedTypeSymbol namedTypeSymbol2 = namedTypeSymbol;
		while (namedTypeSymbol2.DeclaringCompilation != DeclaringCompilation)
		{
			namedTypeSymbol2.AddUseSiteDiagnostics(ref useSiteDiagnostics);
			namedTypeSymbol2 = namedTypeSymbol2.BaseTypeNoUseSiteDiagnostics;
			if ((object)namedTypeSymbol2 == null)
			{
				break;
			}
		}
		if (!useSiteDiagnostics.IsNullOrEmpty())
		{
			diagnostics.Add(FindBaseRefSyntax(namedTypeSymbol) ?? Locations[0], useSiteDiagnostics);
		}
		return namedTypeSymbol;
	}

	private NamedTypeSymbol GetEnumUnderlyingType(DiagnosticBag diagnostics)
	{
		if (TypeKind != TypeKind.Enum)
		{
			return null;
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		BaseListSyntax baseListOpt = GetBaseListOpt(declaration.Declarations[0]);
		if (baseListOpt != null)
		{
			SeparatedSyntaxList<BaseTypeSyntax> types = baseListOpt.Types;
			if (types.Count > 0)
			{
				TypeSyntax type = types[0].Type;
				TypeSymbol typeSymbol = declaringCompilation.GetBinder(baseListOpt).BindType(type, diagnostics);
				if (!typeSymbol.SpecialType.IsValidEnumUnderlyingType())
				{
					diagnostics.Add(ErrorCode.ERR_IntegralTypeExpected, type.Location);
					typeSymbol = declaringCompilation.GetSpecialType(SpecialType.System_Int32);
				}
				return (NamedTypeSymbol)typeSymbol;
			}
		}
		NamedTypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_Int32);
		Binder.ReportUseSiteDiagnostics(specialType, diagnostics, Locations[0]);
		return specialType;
	}
}
