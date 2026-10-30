using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.Model.Language;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class NonLocalizable
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
				resourceMan = new ResourceManager("Microsoft.Dynamics.Nav.Model.Language.NonLocalizable", typeof(NonLocalizable).Assembly);
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

	internal static string ConflictHint_CodeIsMergedButIsOutCommented => ResourceManager.GetString("ConflictHint_CodeIsMergedButIsOutCommented", resourceCulture);

	internal static string ConflictHint_DeleteOfModifiedTarget => ResourceManager.GetString("ConflictHint_DeleteOfModifiedTarget", resourceCulture);

	internal static string ConflictHint_DoubleInsertOfElement => ResourceManager.GetString("ConflictHint_DoubleInsertOfElement", resourceCulture);

	internal static string ConflictHint_InlineCodeConflict => ResourceManager.GetString("ConflictHint_InlineCodeConflict", resourceCulture);

	internal static string ConflictHint_InsertAfterTargetNotFound => ResourceManager.GetString("ConflictHint_InsertAfterTargetNotFound", resourceCulture);

	internal static string ConflictHint_PropertyChangedInBothTargetAndModified => ResourceManager.GetString("ConflictHint_PropertyChangedInBothTargetAndModified", resourceCulture);

	internal static string ConflictHint_PropertyInsertedInBothTargetAndModified => ResourceManager.GetString("ConflictHint_PropertyInsertedInBothTargetAndModified", resourceCulture);

	internal static string ConflictHint_Unprocessed => ResourceManager.GetString("ConflictHint_Unprocessed", resourceCulture);

	internal static string ConflictHint_UnprocessedMove => ResourceManager.GetString("ConflictHint_UnprocessedMove", resourceCulture);

	internal static string ConflictHint_UnprocessedMoveSibling => ResourceManager.GetString("ConflictHint_UnprocessedMoveSibling", resourceCulture);

	internal static string GenericElement_InsertAfterNonSubElement => ResourceManager.GetString("GenericElement_InsertAfterNonSubElement", resourceCulture);

	internal static string GenericElement_ParentCanNotBeSetToNull => ResourceManager.GetString("GenericElement_ParentCanNotBeSetToNull", resourceCulture);

	internal static string GenericElement_ParentIsAlreadyAssigned => ResourceManager.GetString("GenericElement_ParentIsAlreadyAssigned", resourceCulture);

	internal static string GenericElement_SubElementNotFound => ResourceManager.GetString("GenericElement_SubElementNotFound", resourceCulture);

	internal static string GenericElement_ValidSubElement => ResourceManager.GetString("GenericElement_ValidSubElement", resourceCulture);

	internal static string ModelChanges_AppObjsMustHaveSameTypeAndId => ResourceManager.GetString("ModelChanges_AppObjsMustHaveSameTypeAndId", resourceCulture);

	internal static string ModelChanges_CodeCompressionCheck => ResourceManager.GetString("ModelChanges_CodeCompressionCheck", resourceCulture);

	internal static string ModelChanges_InvalidElementKeyList => ResourceManager.GetString("ModelChanges_InvalidElementKeyList", resourceCulture);

	internal static string ModelChanges_RdlChangeNotSupported => ResourceManager.GetString("ModelChanges_RdlChangeNotSupported", resourceCulture);

	internal static string ModelChanges_UnknownOrUnsupportedProperty => ResourceManager.GetString("ModelChanges_UnknownOrUnsupportedProperty", resourceCulture);

	internal static string TxtImport_CodeOrEndCurlyExpected => ResourceManager.GetString("TxtImport_CodeOrEndCurlyExpected", resourceCulture);

	internal static string TxtImport_ElementContainerNotSupported => ResourceManager.GetString("TxtImport_ElementContainerNotSupported", resourceCulture);

	internal static string TxtImport_InvalidPropertyOrTriggerName => ResourceManager.GetString("TxtImport_InvalidPropertyOrTriggerName", resourceCulture);

	internal static string TxtImport_ObjectTypeExpected => ResourceManager.GetString("TxtImport_ObjectTypeExpected", resourceCulture);

	internal static string TxtImport_ObjectTypeNotSupported => ResourceManager.GetString("TxtImport_ObjectTypeNotSupported", resourceCulture);

	internal static string TxtImport_ObsoletePropertyName => ResourceManager.GetString("TxtImport_ObsoletePropertyName", resourceCulture);

	internal static string TxtImport_ObsoleteTriggerName => ResourceManager.GetString("TxtImport_ObsoleteTriggerName", resourceCulture);

	internal static string TxtImport_ReportLayoutExpected => ResourceManager.GetString("TxtImport_ReportLayoutExpected", resourceCulture);

	internal static string TxtImport_SectionIsAlreadyImported => ResourceManager.GetString("TxtImport_SectionIsAlreadyImported", resourceCulture);

	internal static string TxtImport_TokenExpected => ResourceManager.GetString("TxtImport_TokenExpected", resourceCulture);

	internal static string TxtImport_TokenUnexpected => ResourceManager.GetString("TxtImport_TokenUnexpected", resourceCulture);

	internal static string UnimportableCode_Line1 => ResourceManager.GetString("UnimportableCode_Line1", resourceCulture);

	internal static string UnimportableCode_Line2 => ResourceManager.GetString("UnimportableCode_Line2", resourceCulture);

	internal NonLocalizable()
	{
	}
}
