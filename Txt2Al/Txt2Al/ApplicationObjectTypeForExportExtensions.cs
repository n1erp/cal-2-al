using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Txt2Al;

internal static class ApplicationObjectTypeForExportExtensions
{
	public static bool ShouldExportElementType(this ApplicationObjectTypeForExport appObjectType, ElementType elementType)
	{
		return appObjectType switch
		{
			ApplicationObjectTypeForExport.All => true, 
			ApplicationObjectTypeForExport.Codeunit => elementType == ElementType.Codeunit, 
			ApplicationObjectTypeForExport.Table => elementType == ElementType.Table, 
			ApplicationObjectTypeForExport.Page => elementType == ElementType.Page, 
			ApplicationObjectTypeForExport.Report => elementType == ElementType.Report, 
			ApplicationObjectTypeForExport.Query => elementType == ElementType.Query, 
			ApplicationObjectTypeForExport.XmlPort => elementType == ElementType.XmlPort, 
			_ => false, 
		};
	}
}
