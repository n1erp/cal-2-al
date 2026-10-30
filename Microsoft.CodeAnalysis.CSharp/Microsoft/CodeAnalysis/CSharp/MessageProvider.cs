using System;
using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MessageProvider : CommonMessageProvider, IObjectWritable
{
	public static readonly MessageProvider Instance;

	bool IObjectWritable.ShouldReuseInSerialization => true;

	public override string CodePrefix => "CS";

	public override Type ErrorCodeType => typeof(ErrorCode);

	public override int ERR_FailedToCreateTempFile => 1619;

	public override int ERR_ExpectedSingleScript => 7018;

	public override int ERR_OpenResponseFile => 2011;

	public override int ERR_InvalidPathMap => 8101;

	public override int FTL_InvalidInputFileName => 2021;

	public override int ERR_FileNotFound => 2001;

	public override int ERR_NoSourceFile => 1504;

	public override int ERR_CantOpenFileWrite => 2012;

	public override int ERR_OutputWriteFailed => 16;

	public override int WRN_NoConfigNotOnCommandLine => 2023;

	public override int ERR_BinaryFile => 2015;

	public override int WRN_AnalyzerCannotBeCreated => 8032;

	public override int WRN_NoAnalyzerInAssembly => 8033;

	public override int WRN_UnableToLoadAnalyzer => 8034;

	public override int INF_UnableToLoadSomeTypesInAnalyzer => 8040;

	public override int ERR_CantReadRulesetFile => 8035;

	public override int ERR_CompileCancelled => 1600;

	public override int ERR_BadSourceCodeKind => 8190;

	public override int ERR_BadDocumentationMode => 8191;

	public override int ERR_BadCompilationOptionValue => 7088;

	public override int ERR_MutuallyExclusiveOptions => 7102;

	public override int ERR_InvalidDebugInformationFormat => 2042;

	public override int ERR_InvalidOutputName => 2041;

	public override int ERR_InvalidFileAlignment => 2024;

	public override int ERR_InvalidSubsystemVersion => 1773;

	public override int ERR_InvalidInstrumentationKind => 8111;

	public override int ERR_InvalidHashAlgorithmName => 8113;

	public override int ERR_MetadataFileNotAssembly => 1509;

	public override int ERR_MetadataFileNotModule => 1542;

	public override int ERR_InvalidAssemblyMetadata => 9;

	public override int ERR_InvalidModuleMetadata => 9;

	public override int ERR_ErrorOpeningAssemblyFile => 9;

	public override int ERR_ErrorOpeningModuleFile => 9;

	public override int ERR_MetadataFileNotFound => 6;

	public override int ERR_MetadataReferencesNotSupported => 7099;

	public override int ERR_LinkedNetmoduleMetadataMustProvideFullPEImage => 7098;

	public override int ERR_PublicKeyFileFailure => 7027;

	public override int ERR_PublicKeyContainerFailure => 7028;

	public override int ERR_OptionMustBeAbsolutePath => 8106;

	public override int ERR_CantReadResource => 1566;

	public override int ERR_CantOpenWin32Resource => 1719;

	public override int ERR_CantOpenWin32Manifest => 1926;

	public override int ERR_CantOpenWin32Icon => 7064;

	public override int ERR_ErrorBuildingWin32Resource => 7065;

	public override int ERR_BadWin32Resource => 1583;

	public override int ERR_ResourceFileNameNotUnique => 7041;

	public override int ERR_ResourceNotUnique => 1508;

	public override int ERR_ResourceInModule => 1507;

	public override int ERR_PermissionSetAttributeFileReadError => 7057;

	public override int ERR_EncodinglessSyntaxTree => 8055;

	public override int WRN_PdbUsingNameTooLong => 811;

	public override int WRN_PdbLocalNameTooLong => 8029;

	public override int ERR_PdbWritingFailed => 41;

	public override int ERR_MetadataNameTooLong => 7013;

	public override int ERR_EncReferenceToAddedMember => 7101;

	public override int ERR_TooManyUserStrings => 8103;

	public override int ERR_PeWritingFailure => 8104;

	public override int ERR_ModuleEmitFailure => 7038;

	public override int ERR_EncUpdateFailedMissingAttribute => 7043;

	public override int ERR_InvalidDebugInfo => 7103;

	public override int ERR_BadAssemblyName => 8203;

	static MessageProvider()
	{
		Instance = new MessageProvider();
		ObjectBinder.RegisterTypeReader(typeof(MessageProvider), (ObjectReader r) => Instance);
	}

	private MessageProvider()
	{
	}

	void IObjectWritable.WriteTo(ObjectWriter writer)
	{
	}

	public override DiagnosticSeverity GetSeverity(int code)
	{
		return ErrorFacts.GetSeverity((ErrorCode)code);
	}

	public override string LoadMessage(int code, CultureInfo language)
	{
		return ErrorFacts.GetMessage((ErrorCode)code, language);
	}

	public override LocalizableString GetMessageFormat(int code)
	{
		return ErrorFacts.GetMessageFormat((ErrorCode)code);
	}

	public override LocalizableString GetDescription(int code)
	{
		return ErrorFacts.GetDescription((ErrorCode)code);
	}

	public override LocalizableString GetTitle(int code)
	{
		return ErrorFacts.GetTitle((ErrorCode)code);
	}

	public override string GetHelpLink(int code)
	{
		return ErrorFacts.GetHelpLink((ErrorCode)code);
	}

	public override string GetCategory(int code)
	{
		return ErrorFacts.GetCategory((ErrorCode)code);
	}

	public override string GetMessagePrefix(string id, DiagnosticSeverity severity, bool isWarningAsError, CultureInfo culture)
	{
		return string.Format(culture, "{0} {1}", ((severity == DiagnosticSeverity.Error) | isWarningAsError) ? "error" : "warning", id);
	}

	public override int GetWarningLevel(int code)
	{
		return ErrorFacts.GetWarningLevel((ErrorCode)code);
	}

	public override Diagnostic CreateDiagnostic(int code, Location location, params object[] args)
	{
		return new CSDiagnostic(new CSDiagnosticInfo((ErrorCode)code, args, ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty), location);
	}

	public override Diagnostic CreateDiagnostic(DiagnosticInfo info)
	{
		return new CSDiagnostic(info, Location.None);
	}

	public override string GetErrorDisplayString(ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.Assembly || symbol.Kind == SymbolKind.Namespace)
		{
			return symbol.ToString();
		}
		return SymbolDisplay.ToDisplayString(symbol, SymbolDisplayFormat.CSharpShortErrorMessageFormat);
	}

	public override ReportDiagnostic GetDiagnosticReport(DiagnosticInfo diagnosticInfo, CompilationOptions options)
	{
		bool hasPragmaSuppression;
		return CSharpDiagnosticFilter.GetDiagnosticReport(diagnosticInfo.Severity, isEnabledByDefault: true, diagnosticInfo.MessageIdentifier, diagnosticInfo.WarningLevel, Location.None, diagnosticInfo.Category, options.WarningLevel, options.GeneralDiagnosticOption, options.SpecificDiagnosticOptions, out hasPragmaSuppression);
	}

	public override void ReportDuplicateMetadataReferenceStrong(DiagnosticBag diagnostics, Location location, MetadataReference reference, AssemblyIdentity identity, MetadataReference equivalentReference, AssemblyIdentity equivalentIdentity)
	{
		diagnostics.Add(ErrorCode.ERR_DuplicateImport, location, reference.Display ?? identity.GetDisplayName(), equivalentReference.Display ?? equivalentIdentity.GetDisplayName());
	}

	public override void ReportDuplicateMetadataReferenceWeak(DiagnosticBag diagnostics, Location location, MetadataReference reference, AssemblyIdentity identity, MetadataReference equivalentReference, AssemblyIdentity equivalentIdentity)
	{
		diagnostics.Add(ErrorCode.ERR_DuplicateImportSimple, location, identity.Name, reference.Display ?? identity.GetDisplayName());
	}

	public override void ReportInvalidAttributeArgument(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int parameterIndex, AttributeData attribute)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		CSharpSyntaxNode attributeArgumentSyntax = attribute.GetAttributeArgumentSyntax(parameterIndex, attributeSyntax2);
		diagnostics.Add(ErrorCode.ERR_InvalidAttributeArgument, attributeArgumentSyntax.Location, attributeSyntax2.GetErrorDisplayName());
	}

	public override void ReportInvalidNamedArgument(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int namedArgumentIndex, ITypeSymbol attributeClass, string parameterName)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		diagnostics.Add(ErrorCode.ERR_InvalidNamedArgument, attributeSyntax2.ArgumentList.Arguments[namedArgumentIndex].Location, parameterName);
	}

	public override void ReportParameterNotValidForType(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int namedArgumentIndex)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		diagnostics.Add(ErrorCode.ERR_ParameterNotValidForType, attributeSyntax2.ArgumentList.Arguments[namedArgumentIndex].Location);
	}

	public override void ReportMarshalUnmanagedTypeNotValidForFields(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int parameterIndex, string unmanagedTypeName, AttributeData attribute)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		CSharpSyntaxNode attributeArgumentSyntax = attribute.GetAttributeArgumentSyntax(parameterIndex, attributeSyntax2);
		diagnostics.Add(ErrorCode.ERR_MarshalUnmanagedTypeNotValidForFields, attributeArgumentSyntax.Location, unmanagedTypeName);
	}

	public override void ReportMarshalUnmanagedTypeOnlyValidForFields(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int parameterIndex, string unmanagedTypeName, AttributeData attribute)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		CSharpSyntaxNode attributeArgumentSyntax = attribute.GetAttributeArgumentSyntax(parameterIndex, attributeSyntax2);
		diagnostics.Add(ErrorCode.ERR_MarshalUnmanagedTypeOnlyValidForFields, attributeArgumentSyntax.Location, unmanagedTypeName);
	}

	public override void ReportAttributeParameterRequired(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, string parameterName)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		diagnostics.Add(ErrorCode.ERR_AttributeParameterRequired1, attributeSyntax2.Name.Location, parameterName);
	}

	public override void ReportAttributeParameterRequired(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, string parameterName1, string parameterName2)
	{
		AttributeSyntax attributeSyntax2 = (AttributeSyntax)attributeSyntax;
		diagnostics.Add(ErrorCode.ERR_AttributeParameterRequired2, attributeSyntax2.Name.Location, parameterName1, parameterName2);
	}
}
