using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaReport : NCLMetaApplicationObject
{
	private delegate NavReport CreateNavReport(ITreeObject parent, NCLMetaReport metaReport);

	private delegate NavReport CreateLegacyNavReport(ITreeObject parent);

	private NCLCaptionStrings captionML;

	protected override Type ApplicationObjectConstructorDelegateType => typeof(CreateNavReport);

	protected override Type ApplicationObjectLegacyConstructorDelegateType => typeof(CreateLegacyNavReport);

	public string Name { get; private set; }

	public bool UseRequestForm { get; private set; }

	public bool UseSystemPrinter { get; private set; }

	public bool ProcessingOnly { get; private set; }

	public bool EnableExternalImages { get; private set; }

	public bool EnableHyperlinks { get; private set; }

	public bool EnableExternalAssemblies { get; private set; }

	public int DefaultPaperSourceKindRaw { get; private set; }

	public int FirstPagePaperSourceKindRaw { get; private set; }

	public int LastPagePaperSourceKindRaw { get; private set; }

	public TransactionType TransactionType { get; private set; }

	public bool ShowPrintStatus { get; private set; }

	public PreviewMode PreviewMode { get; private set; }

	public DefaultLayout DefaultLayout { get; private set; }

	public string WordMergeDataItem { get; private set; }

	public int RootDataItemTableNumber { get; private set; }

	public string Caption => captionML.GetValueOrDefault();

	public XmlDocument MetaReportXml => GetMetadataFromLoader().Document;

	private NCLMetaReport(INCLMetaApplicationObjectLoader loader, int reportId, NavAppGroup metadataAppGroup, int appDependencyOrder)
		: base(loader, ObjectType.Report, reportId, metadataAppGroup, appDependencyOrder)
	{
	}

	internal static NCLMetaReport CreateEmptyNCLMetaReport(INCLMetaApplicationObjectLoader loader, int reportId, NavAppGroup appGroup, int appDependencyOrder = -1)
	{
		return new NCLMetaReport(loader, reportId, appGroup, appDependencyOrder);
	}

	protected override void LoadMetadata()
	{
		base.LoadMetadata();
		try
		{
			NCLObjectXmlMetadata metadataFromLoader = GetMetadataFromLoader();
			Timestamp = metadataFromLoader.Timestamp;
			MetaReport metaReport = new MetaReport(metadataFromLoader.Document.DocumentElement, null, MetadataAppGroup.GroupId, NavCurrentThread.ResolveAppGroup().GroupId);
			Name = metaReport.Name;
			captionML = new NCLCaptionStrings(metaReport.CaptionML.ToArray(), Name);
			UseRequestForm = metaReport.UseRequestForm;
			UseSystemPrinter = metaReport.UseSystemPrinter;
			EnableExternalImages = metaReport.EnableExternalImages;
			EnableHyperlinks = metaReport.EnableHyperlinks;
			EnableExternalAssemblies = metaReport.EnableExternalAssemblies;
			ProcessingOnly = metaReport.ProcessingOnly;
			ShowPrintStatus = metaReport.ShowPrintStatus;
			TransactionType = metaReport.TransactionType;
			FirstPagePaperSourceKindRaw = metaReport.FirstPagePaperSourceKindRaw.GetValueOrDefault();
			DefaultPaperSourceKindRaw = metaReport.DefaultPaperSourceKindRaw.GetValueOrDefault();
			LastPagePaperSourceKindRaw = metaReport.LastPagePaperSourceKindRaw.GetValueOrDefault();
			PreviewMode = metaReport.PreviewMode;
			DefaultLayout = metaReport.DefaultLayout;
			WordMergeDataItem = metaReport.WordMergeDataItem;
			RootDataItemTableNumber = ((metaReport.RootDataItems != null && metaReport.RootDataItems.Count > 0) ? metaReport.RootDataItems[0].DataItemTable : 0);
		}
		catch (NavMetadataNotFoundException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000290", Category.Runtime, ex);
			throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentCulture, ApplicationObjectId.ObjectType, ApplicationObjectId.ObjectNumber, ex.InnerException);
		}
	}

	public NavReport CreateObjectInstance(ITreeObject parent, bool skipRestoreSavedReportSettings = false)
	{
		bool skipRestoreSavedReportSettings2 = NavCurrentThread.Session.SkipRestoreSavedReportSettings;
		NavCurrentThread.Session.SkipRestoreSavedReportSettings = skipRestoreSavedReportSettings;
		try
		{
			Delegate obj = ApplicationObjectConstructor;
			if (!ApplicationObjectLegacyConstructor)
			{
				return ((CreateNavReport)obj)(parent, this);
			}
			return ((CreateLegacyNavReport)obj)(parent);
		}
		finally
		{
			NavCurrentThread.Session.SkipRestoreSavedReportSettings = skipRestoreSavedReportSettings2;
		}
	}
}
