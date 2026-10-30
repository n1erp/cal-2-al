using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class NavFormExcelWriterFactory
{
	internal static IExcelWriter Create(NavForm form, IOfficeDownloadHandler downloadHandler = null, NavFilterGroup[] filterGroups = null)
	{
		if (form == null)
		{
			throw new ArgumentNullException("form");
		}
		MasterPage masterPage = form.MasterPage;
		switch (masterPage.PageProperties.SourceObject.PageDataSource)
		{
		case DataSourceType.Query:
		{
			NavQueryForm navQueryForm = form as NavQueryForm;
			NavQuery query = navQueryForm.NclMetaQuery.CreateObjectInstance(form);
			return new ExcelWriterQuery(query, masterPage, navQueryForm.FieldDecimalPlaces, downloadHandler, filterGroups);
		}
		case DataSourceType.Table:
			return new ExcelWriterTable(form, downloadHandler);
		default:
			throw new NavExportException(string.Format(CultureInfo.CurrentCulture, Lang.ExportToExcelNotSupported));
		}
	}
}
