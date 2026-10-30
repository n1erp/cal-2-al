namespace Microsoft.CodeAnalysis;

public class SymbolDisplayFormat
{
	internal static readonly SymbolDisplayFormat TestFormat = new SymbolDisplayFormat(SymbolDisplayCompilerInternalOptions.UseMetadataMethodNames | SymbolDisplayCompilerInternalOptions.FlagMissingMetadataTypes | SymbolDisplayCompilerInternalOptions.IncludeScriptType | SymbolDisplayCompilerInternalOptions.IncludeCustomModifiers, SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance, SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeExplicitInterface | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeRef, SymbolDisplayParameterOptions.IncludeExtensionThis | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeDefaultValue | SymbolDisplayParameterOptions.IncludeOptionalBrackets, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayPropertyStyle.ShowReadWriteDescriptor, SymbolDisplayLocalOptions.IncludeType, SymbolDisplayKindOptions.IncludeMemberKeyword, SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName);

	internal static readonly SymbolDisplayFormat QualifiedNameOnlyFormat = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

	internal static readonly SymbolDisplayFormat QualifiedNameArityFormat = new SymbolDisplayFormat(SymbolDisplayCompilerInternalOptions.UseArityForGenericTypes, SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

	internal static readonly SymbolDisplayFormat ShortFormat = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining, SymbolDisplayTypeQualificationStyle.NameOnly, SymbolDisplayGenericsOptions.None, SymbolDisplayMemberOptions.None, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.IncludeName, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

	internal static readonly SymbolDisplayFormat ILVisualizationFormat = new SymbolDisplayFormat(SymbolDisplayCompilerInternalOptions.UseMetadataMethodNames, SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.IncludeTypeParameters, SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeRef, SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.IncludeMemberKeyword, SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

	internal static readonly SymbolDisplayFormat ExplicitInterfaceImplementationFormat = new SymbolDisplayFormat(SymbolDisplayCompilerInternalOptions.ReverseArrayRankSpecifiers, SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.IncludeTypeParameters, SymbolDisplayMemberOptions.None, SymbolDisplayParameterOptions.None, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

	public static SymbolDisplayFormat CSharpErrorMessageFormat { get; } = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.IncludeTypeParameters, SymbolDisplayMemberOptions.IncludeExplicitInterface | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName);

	public static SymbolDisplayFormat CSharpShortErrorMessageFormat { get; } = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining, SymbolDisplayTypeQualificationStyle.NameAndContainingTypes, SymbolDisplayGenericsOptions.IncludeTypeParameters, SymbolDisplayMemberOptions.IncludeExplicitInterface | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName);

	public static SymbolDisplayFormat VisualBasicErrorMessageFormat { get; } = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance, SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeModifiers | SymbolDisplayMemberOptions.IncludeAccessibility | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeRef, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.IncludeExtensionThis | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeDefaultValue | SymbolDisplayParameterOptions.IncludeOptionalBrackets, SymbolDisplayPropertyStyle.ShowReadWriteDescriptor, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.IncludeMemberKeyword, SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName);

	public static SymbolDisplayFormat VisualBasicShortErrorMessageFormat { get; } = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining, SymbolDisplayTypeQualificationStyle.NameAndContainingTypes, SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance, SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeModifiers | SymbolDisplayMemberOptions.IncludeAccessibility | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeRef, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.IncludeExtensionThis | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeDefaultValue | SymbolDisplayParameterOptions.IncludeOptionalBrackets, SymbolDisplayPropertyStyle.ShowReadWriteDescriptor, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.IncludeMemberKeyword, SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName);

	public static SymbolDisplayFormat FullyQualifiedFormat { get; } = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Included, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.IncludeTypeParameters, SymbolDisplayMemberOptions.None, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.None, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

	public static SymbolDisplayFormat MinimallyQualifiedFormat { get; } = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameOnly, SymbolDisplayGenericsOptions.IncludeTypeParameters, SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeRef, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeDefaultValue, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.IncludeType, SymbolDisplayKindOptions.IncludeMemberKeyword, SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

	public SymbolDisplayGlobalNamespaceStyle GlobalNamespaceStyle { get; }

	public SymbolDisplayTypeQualificationStyle TypeQualificationStyle { get; }

	public SymbolDisplayGenericsOptions GenericsOptions { get; }

	public SymbolDisplayMemberOptions MemberOptions { get; }

	public SymbolDisplayParameterOptions ParameterOptions { get; }

	public SymbolDisplayDelegateStyle DelegateStyle { get; }

	public SymbolDisplayExtensionMethodStyle ExtensionMethodStyle { get; }

	public SymbolDisplayPropertyStyle PropertyStyle { get; }

	public SymbolDisplayLocalOptions LocalOptions { get; }

	public SymbolDisplayKindOptions KindOptions { get; }

	public SymbolDisplayMiscellaneousOptions MiscellaneousOptions { get; }

	internal SymbolDisplayCompilerInternalOptions CompilerInternalOptions { get; }

	public SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle globalNamespaceStyle = SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle typeQualificationStyle = SymbolDisplayTypeQualificationStyle.NameOnly, SymbolDisplayGenericsOptions genericsOptions = SymbolDisplayGenericsOptions.None, SymbolDisplayMemberOptions memberOptions = SymbolDisplayMemberOptions.None, SymbolDisplayDelegateStyle delegateStyle = SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle extensionMethodStyle = SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions parameterOptions = SymbolDisplayParameterOptions.None, SymbolDisplayPropertyStyle propertyStyle = SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions localOptions = SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions kindOptions = SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions miscellaneousOptions = SymbolDisplayMiscellaneousOptions.None)
		: this(SymbolDisplayCompilerInternalOptions.None, globalNamespaceStyle, typeQualificationStyle, genericsOptions, memberOptions, parameterOptions, delegateStyle, extensionMethodStyle, propertyStyle, localOptions, kindOptions, miscellaneousOptions)
	{
	}

	internal SymbolDisplayFormat(SymbolDisplayCompilerInternalOptions compilerInternalOptions, SymbolDisplayGlobalNamespaceStyle globalNamespaceStyle = SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle typeQualificationStyle = SymbolDisplayTypeQualificationStyle.NameOnly, SymbolDisplayGenericsOptions genericsOptions = SymbolDisplayGenericsOptions.None, SymbolDisplayMemberOptions memberOptions = SymbolDisplayMemberOptions.None, SymbolDisplayParameterOptions parameterOptions = SymbolDisplayParameterOptions.None, SymbolDisplayDelegateStyle delegateStyle = SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle extensionMethodStyle = SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayPropertyStyle propertyStyle = SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions localOptions = SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions kindOptions = SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions miscellaneousOptions = SymbolDisplayMiscellaneousOptions.None)
	{
		GlobalNamespaceStyle = globalNamespaceStyle;
		TypeQualificationStyle = typeQualificationStyle;
		GenericsOptions = genericsOptions;
		MemberOptions = memberOptions;
		ParameterOptions = parameterOptions;
		DelegateStyle = delegateStyle;
		ExtensionMethodStyle = extensionMethodStyle;
		PropertyStyle = propertyStyle;
		LocalOptions = localOptions;
		KindOptions = kindOptions;
		MiscellaneousOptions = miscellaneousOptions;
		CompilerInternalOptions = compilerInternalOptions;
	}

	public SymbolDisplayFormat WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions options)
	{
		return new SymbolDisplayFormat(GlobalNamespaceStyle, TypeQualificationStyle, GenericsOptions, MemberOptions, DelegateStyle, ExtensionMethodStyle, ParameterOptions, PropertyStyle, LocalOptions, KindOptions, options);
	}

	public SymbolDisplayFormat AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions options)
	{
		return WithMiscellaneousOptions(MiscellaneousOptions | options);
	}

	public SymbolDisplayFormat RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions options)
	{
		return WithMiscellaneousOptions(MiscellaneousOptions & ~options);
	}

	public SymbolDisplayFormat WithGenericsOptions(SymbolDisplayGenericsOptions options)
	{
		return new SymbolDisplayFormat(GlobalNamespaceStyle, TypeQualificationStyle, options, MemberOptions, DelegateStyle, ExtensionMethodStyle, ParameterOptions, PropertyStyle, LocalOptions, KindOptions, MiscellaneousOptions);
	}

	public SymbolDisplayFormat AddGenericsOptions(SymbolDisplayGenericsOptions options)
	{
		return WithGenericsOptions(GenericsOptions | options);
	}

	public SymbolDisplayFormat RemoveGenericsOptions(SymbolDisplayGenericsOptions options)
	{
		return WithGenericsOptions(GenericsOptions & ~options);
	}

	public SymbolDisplayFormat WithMemberOptions(SymbolDisplayMemberOptions options)
	{
		return new SymbolDisplayFormat(GlobalNamespaceStyle, TypeQualificationStyle, GenericsOptions, options, DelegateStyle, ExtensionMethodStyle, ParameterOptions, PropertyStyle, LocalOptions, KindOptions, MiscellaneousOptions);
	}

	public SymbolDisplayFormat AddMemberOptions(SymbolDisplayMemberOptions options)
	{
		return WithMemberOptions(MemberOptions | options);
	}

	public SymbolDisplayFormat RemoveMemberOptions(SymbolDisplayMemberOptions options)
	{
		return WithMemberOptions(MemberOptions & ~options);
	}

	public SymbolDisplayFormat WithKindOptions(SymbolDisplayKindOptions options)
	{
		return new SymbolDisplayFormat(GlobalNamespaceStyle, TypeQualificationStyle, GenericsOptions, MemberOptions, DelegateStyle, ExtensionMethodStyle, ParameterOptions, PropertyStyle, LocalOptions, options, MiscellaneousOptions);
	}

	public SymbolDisplayFormat AddKindOptions(SymbolDisplayKindOptions options)
	{
		return WithKindOptions(KindOptions | options);
	}

	public SymbolDisplayFormat RemoveKindOptions(SymbolDisplayKindOptions options)
	{
		return WithKindOptions(KindOptions & ~options);
	}

	public SymbolDisplayFormat WithParameterOptions(SymbolDisplayParameterOptions options)
	{
		return new SymbolDisplayFormat(GlobalNamespaceStyle, TypeQualificationStyle, GenericsOptions, MemberOptions, DelegateStyle, ExtensionMethodStyle, options, PropertyStyle, LocalOptions, KindOptions, MiscellaneousOptions);
	}

	public SymbolDisplayFormat AddParameterOptions(SymbolDisplayParameterOptions options)
	{
		return WithParameterOptions(ParameterOptions | options);
	}

	public SymbolDisplayFormat RemoveParameterOptions(SymbolDisplayParameterOptions options)
	{
		return WithParameterOptions(ParameterOptions & ~options);
	}

	public SymbolDisplayFormat WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle style)
	{
		return new SymbolDisplayFormat(style, TypeQualificationStyle, GenericsOptions, MemberOptions, DelegateStyle, ExtensionMethodStyle, ParameterOptions, PropertyStyle, LocalOptions, KindOptions, MiscellaneousOptions);
	}

	public SymbolDisplayFormat WithLocalOptions(SymbolDisplayLocalOptions options)
	{
		return new SymbolDisplayFormat(GlobalNamespaceStyle, TypeQualificationStyle, GenericsOptions, MemberOptions, DelegateStyle, ExtensionMethodStyle, ParameterOptions, PropertyStyle, options, KindOptions, MiscellaneousOptions);
	}

	public SymbolDisplayFormat AddLocalOptions(SymbolDisplayLocalOptions options)
	{
		return WithLocalOptions(LocalOptions | options);
	}

	public SymbolDisplayFormat RemoveLocalOptions(SymbolDisplayLocalOptions options)
	{
		return WithLocalOptions(LocalOptions & ~options);
	}
}
