using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NCLObjectMetadataLoaderExtensions
{
	public static NavigationDefinition GetNavigationDefintion(this INCLObjectXmlMetadataLoader loader, string idRangeFilter, NavAppGroup appGroup)
	{
		Tuple<XmlDocument, int> metaObjectAsXmlDocument = loader.GetMetaObjectAsXmlDocument(ObjectType.MenuSuite, idRangeFilter, returnFirst: false);
		if (metaObjectAsXmlDocument == null)
		{
			return new NavigationDefinition
			{
				ActionContainers = new List<ActionContainerDefinition>()
			};
		}
		return MetaPage.CreateNavigationDefinition(metaObjectAsXmlDocument.Item1, metaObjectAsXmlDocument.Item2, appGroup.GroupId);
	}

	public static TimestampedObject<PageDefinition> GetMetaPage(this INCLObjectXmlMetadataLoader loader, int pageId, NavAppGroup appGroup, int languageAppGroupId)
	{
		return RetrieveRuntimeObject(loader, ObjectType.Page, pageId, appGroup, (XmlException xEx, ApplicationObjectId id) => new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.InvalidPageMetadata, id), xEx), (NCLObjectXmlMetadata m) => MetaPage.CreatePageDefinition(m.Document, languageAppGroupId));
	}

	public static TimestampedObject<MetaTable> GetMetaTable(this INCLObjectXmlMetadataLoader loader, int tableId, NavAppGroup appGroup)
	{
		return RetrieveRuntimeObject(loader, ObjectType.Table, tableId, appGroup, (XmlException xEx, ApplicationObjectId id) => new NavNSInvalidTableDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.InvalidTableMetadata, id), xEx), (NCLObjectXmlMetadata m) => MetaTable.CreateMetaTableFromXml(m.Document.DocumentElement, appGroup.GroupId, m.Timestamp));
	}

	public static TimestampedObject<MetaXmlPort> GetMetaXmlPort(this INCLObjectXmlMetadataLoader loader, int xmlportId, NavAppGroup appGroup, int languageAppGroupId)
	{
		return RetrieveRuntimeObject(loader, ObjectType.XmlPort, xmlportId, appGroup, (XmlException xEx, ApplicationObjectId id) => new NavNCLXmlPortMetadataException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.InvalidXmlPortMetadata, id.ObjectNumber), xEx), (NCLObjectXmlMetadata m) => new MetaXmlPort(m.Document, null, appGroup.GroupId, languageAppGroupId));
	}

	public static TimestampedObject<MetaQuery> GetMetaQuery(this INCLObjectXmlMetadataLoader loader, int queryId, NavAppGroup appGroup, int languageAppGroupId)
	{
		return RetrieveRuntimeObject(loader, ObjectType.Query, queryId, appGroup, (XmlException xEx, ApplicationObjectId id) => NavNCLQueryDefinitionInvalidException.Create(NavCurrentThread.Session.RuntimeLanguage, id.ObjectNumber, xEx), (NCLObjectXmlMetadata m) => new MetaQuery(m.Document.DocumentElement, appGroup.GroupId, languageAppGroupId));
	}

	public static TimestampedObject<MetaReport> GetMetaReport(this INCLObjectXmlMetadataLoader loader, int reportId, NavAppGroup appGroup, int languageAppGroupId)
	{
		return RetrieveRuntimeObject(loader, ObjectType.Report, reportId, appGroup, (XmlException xEx, ApplicationObjectId id) => new NavNCLReportDefinitionInvalidException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.InvalidReportMetadata, id.ObjectNumber), xEx), (NCLObjectXmlMetadata m) => new MetaReport(m.Document.DocumentElement, null, appGroup.GroupId, languageAppGroupId));
	}

	private static TimestampedObject<T> RetrieveRuntimeObject<T>(INCLObjectXmlMetadataLoader loader, ObjectType objectType, int objectId, NavAppGroup appGroup, Func<XmlException, ApplicationObjectId, Exception> exceptWrapperGenerator, Func<NCLObjectXmlMetadata, T> objectGenerator)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId(objectType, objectId);
		try
		{
			NCLObjectXmlMetadata metaObjectAsXmlDocument = loader.GetMetaObjectAsXmlDocument(applicationObjectId, appGroup);
			T value = objectGenerator(metaObjectAsXmlDocument);
			return new TimestampedObject<T>
			{
				Value = value,
				Timestamp = metaObjectAsXmlDocument.Timestamp
			};
		}
		catch (XmlException ex)
		{
			LogWriterHelper.LogExceptionEvent(ex);
			throw exceptWrapperGenerator(ex, applicationObjectId);
		}
	}
}
