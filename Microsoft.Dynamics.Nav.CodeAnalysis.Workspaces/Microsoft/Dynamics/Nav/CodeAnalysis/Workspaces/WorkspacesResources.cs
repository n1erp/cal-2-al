using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class WorkspacesResources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.WorkspacesResources", typeof(WorkspacesResources).GetTypeInfo().Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string ActionsCanNotBeEmpty => ResourceManager.GetString("ActionsCanNotBeEmpty", resourceCulture);

	internal static string AddingAdditionalDocumentsNotSupported => ResourceManager.GetString("AddingAdditionalDocumentsNotSupported", resourceCulture);

	internal static string AddingAnalyzerReferencesNotSupported => ResourceManager.GetString("AddingAnalyzerReferencesNotSupported", resourceCulture);

	internal static string AddingDocumentsNotSupported => ResourceManager.GetString("AddingDocumentsNotSupported", resourceCulture);

	internal static string AddingProjectReferencesNotSupported => ResourceManager.GetString("AddingProjectReferencesNotSupported", resourceCulture);

	internal static string AddingProjectsNotSupported => ResourceManager.GetString("AddingProjectsNotSupported", resourceCulture);

	internal static string AnalyzerIsAlreadyPresent => ResourceManager.GetString("AnalyzerIsAlreadyPresent", resourceCulture);

	internal static string AnalyzerIsNotPresent => ResourceManager.GetString("AnalyzerIsNotPresent", resourceCulture);

	internal static string AtLeastOneDiagnosticMustBeSupplied => ResourceManager.GetString("AtLeastOneDiagnosticMustBeSupplied", resourceCulture);

	internal static string Available => ResourceManager.GetString("Available", resourceCulture);

	internal static string CausesCircularProjectReference => ResourceManager.GetString("CausesCircularProjectReference", resourceCulture);

	internal static string ChangeTo => ResourceManager.GetString("ChangeTo", resourceCulture);

	internal static string ChangingAdditionalDocumentsIsNotSupported => ResourceManager.GetString("ChangingAdditionalDocumentsIsNotSupported", resourceCulture);

	internal static string ChangingCompilationOptionsNotSupported => ResourceManager.GetString("ChangingCompilationOptionsNotSupported", resourceCulture);

	internal static string ChangingDocumentsNotSupported => ResourceManager.GetString("ChangingDocumentsNotSupported", resourceCulture);

	internal static string ChangingParseOptionsNotSupported => ResourceManager.GetString("ChangingParseOptionsNotSupported", resourceCulture);

	internal static string CodeActionCommandTitle => ResourceManager.GetString("CodeActionCommandTitle", resourceCulture);

	internal static string ConvertToCase => ResourceManager.GetString("ConvertToCase", resourceCulture);

	internal static string Deprecated => ResourceManager.GetString("Deprecated", resourceCulture);

	internal static string DiagnosticMustHaveSpan => ResourceManager.GetString("DiagnosticMustHaveSpan", resourceCulture);

	internal static string DocumentAlreadyInSolution => ResourceManager.GetString("DocumentAlreadyInSolution", resourceCulture);

	internal static string DocumentIsNotOpen => ResourceManager.GetString("DocumentIsNotOpen", resourceCulture);

	internal static string DocumentIsOpen => ResourceManager.GetString("DocumentIsOpen", resourceCulture);

	internal static string DocumentNotInProject => ResourceManager.GetString("DocumentNotInProject", resourceCulture);

	internal static string DocumentNotInSolution => ResourceManager.GetString("DocumentNotInSolution", resourceCulture);

	internal static string DocumentVersionIsDifferent => ResourceManager.GetString("DocumentVersionIsDifferent", resourceCulture);

	internal static string DuplicateKey => ResourceManager.GetString("DuplicateKey", resourceCulture);

	internal static string FixTypo => ResourceManager.GetString("FixTypo", resourceCulture);

	internal static string IdCanNotBeEmpty => ResourceManager.GetString("IdCanNotBeEmpty", resourceCulture);

	internal static string IdIsEmpty => ResourceManager.GetString("IdIsEmpty", resourceCulture);

	internal static string InvalidLanguageNameOption => ResourceManager.GetString("InvalidLanguageNameOption", resourceCulture);

	internal static string InvalidLanguageNameOption2 => ResourceManager.GetString("InvalidLanguageNameOption2", resourceCulture);

	internal static string NameIsEmpty => ResourceManager.GetString("NameIsEmpty", resourceCulture);

	internal static string NotAvailable => ResourceManager.GetString("NotAvailable", resourceCulture);

	internal static string OpenDocumentNotSupported => ResourceManager.GetString("OpenDocumentNotSupported", resourceCulture);

	internal static string Overload => ResourceManager.GetString("Overload", resourceCulture);

	internal static string Overloads => ResourceManager.GetString("Overloads", resourceCulture);

	internal static string ProjectAlreadyInSolution => ResourceManager.GetString("ProjectAlreadyInSolution", resourceCulture);

	internal static string ProjectAlreadyReferenced => ResourceManager.GetString("ProjectAlreadyReferenced", resourceCulture);

	internal static string ProjectAvailability => ResourceManager.GetString("ProjectAvailability", resourceCulture);

	internal static string ProjectContainsOpenDocuments => ResourceManager.GetString("ProjectContainsOpenDocuments", resourceCulture);

	internal static string ProjectDirectlyReferencesTargetProject => ResourceManager.GetString("ProjectDirectlyReferencesTargetProject", resourceCulture);

	internal static string ProjectNotInSolution => ResourceManager.GetString("ProjectNotInSolution", resourceCulture);

	internal static string ProjectNotReferenced => ResourceManager.GetString("ProjectNotReferenced", resourceCulture);

	internal static string ProjectOrDocumentAlreadyInWorkspace => ResourceManager.GetString("ProjectOrDocumentAlreadyInWorkspace", resourceCulture);

	internal static string ProjectOrDocumentNotInWorkspace => ResourceManager.GetString("ProjectOrDocumentNotInWorkspace", resourceCulture);

	internal static string ProjectTransitivelyReferencesTargetProject => ResourceManager.GetString("ProjectTransitivelyReferencesTargetProject", resourceCulture);

	internal static string Property => ResourceManager.GetString("Property", resourceCulture);

	internal static string RemovingAdditionalDocumentsIsNotSupported => ResourceManager.GetString("RemovingAdditionalDocumentsIsNotSupported", resourceCulture);

	internal static string RemovingAnalyzerReferencesNotSupported => ResourceManager.GetString("RemovingAnalyzerReferencesNotSupported", resourceCulture);

	internal static string RemovingDocumentsNotSupported => ResourceManager.GetString("RemovingDocumentsNotSupported", resourceCulture);

	internal static string RemovingProjectReferencesNotSupported => ResourceManager.GetString("RemovingProjectReferencesNotSupported", resourceCulture);

	internal static string RemovingProjectsNotSupported => ResourceManager.GetString("RemovingProjectsNotSupported", resourceCulture);

	internal static string ReturnedUninitializedImmutableArray => ResourceManager.GetString("ReturnedUninitializedImmutableArray", resourceCulture);

	internal static string SuppliedDiagnosticCannotBeNull => ResourceManager.GetString("SuppliedDiagnosticCannotBeNull", resourceCulture);

	internal static string SymbolIsAlreadyReferenced => ResourceManager.GetString("SymbolIsAlreadyReferenced", resourceCulture);

	internal static string SymbolIsNotReferenced => ResourceManager.GetString("SymbolIsNotReferenced", resourceCulture);

	internal static string UnsupportedLanguage => ResourceManager.GetString("UnsupportedLanguage", resourceCulture);

	internal static string UseTheNavigationBarToSwitchContext => ResourceManager.GetString("UseTheNavigationBarToSwitchContext", resourceCulture);

	internal static string VerifyDiagnosticsArgumentIsNotInitialized => ResourceManager.GetString("VerifyDiagnosticsArgumentIsNotInitialized", resourceCulture);

	internal static string WorkspaceIsNotEmpty => ResourceManager.GetString("WorkspaceIsNotEmpty", resourceCulture);

	internal static string WorkspaceServicesUnavailable => ResourceManager.GetString("WorkspaceServicesUnavailable", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal WorkspacesResources()
	{
	}
}
