using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Common.Printing;

public sealed class ReportExecutionItem : IDisposable
{
	private NavDataTableCollectionEnumerator dataEnumerator;

	private Guid reportIdentifier;

	private ILocalReportHandle reportHandle;

	private string reportCaption;

	private ReportRenderer reportRenderer;

	private IReportCallbackHandler reportCallbackHandler;

	private Semaphore semaphore = new Semaphore(0, 1);

	public int? DefaultPaperSourceKindRaw { get; set; }

	public int? FirstPagePaperSourceKindRaw { get; set; }

	public int? LastPagePaperSourceKindRaw { get; set; }

	public int LastPage { get; set; }

	public KeyValuePair<string, string>[] ReportParameters { get; set; }

	public NavDataTableCollectionEnumerator DataEnumerator => dataEnumerator;

	public Guid ReportIdentifier => reportIdentifier;

	public int ReportId { get; private set; }

	public string ReportCaption => reportCaption;

	public ReportRenderer ReportRenderer
	{
		get
		{
			return reportRenderer;
		}
		internal set
		{
			reportRenderer = value;
		}
	}

	public IReportCallbackHandler ReportCallbackHandler => reportCallbackHandler;

	public ILocalReportHandle ReportHandle => reportHandle;

	public ReportExecutionItem(Guid reportIdentifier, NavDataTableCollectionEnumerator dataEnumerator, ILocalReportHandle reportHandle, string reportCaption, int reportId, IReportCallbackHandler reportCallbackHandler, int? defaultPaperSourceKindRaw, int? firstPagePaperSourceKindRaw, int? lastPagePaperSourceKindRaw, KeyValuePair<string, string>[] reportParameters)
	{
		if (dataEnumerator == null)
		{
			throw new ArgumentNullException("dataEnumerator");
		}
		if (reportHandle == null)
		{
			throw new ArgumentNullException("reportHandle");
		}
		if (reportCallbackHandler == null)
		{
			throw new ArgumentNullException("reportCallbackHandler");
		}
		this.reportIdentifier = reportIdentifier;
		this.dataEnumerator = dataEnumerator;
		this.reportHandle = reportHandle;
		this.reportCaption = reportCaption;
		ReportId = reportId;
		reportRenderer = null;
		this.reportCallbackHandler = reportCallbackHandler;
		DefaultPaperSourceKindRaw = defaultPaperSourceKindRaw;
		FirstPagePaperSourceKindRaw = firstPagePaperSourceKindRaw;
		LastPagePaperSourceKindRaw = lastPagePaperSourceKindRaw;
		ReportParameters = reportParameters;
	}

	public int? GetPaperSourceKindRaw(int page)
	{
		if (page == 1 && FirstPagePaperSourceKindRaw.HasValue)
		{
			return FirstPagePaperSourceKindRaw;
		}
		if (page == LastPage && LastPagePaperSourceKindRaw.HasValue)
		{
			return LastPagePaperSourceKindRaw;
		}
		return DefaultPaperSourceKindRaw;
	}

	public void Wait()
	{
		semaphore.WaitOne();
	}

	public void Release()
	{
		semaphore.Release();
	}

	public void AddDataSetPage(DataSetRequest request)
	{
		DataEnumerator.AddDataTable(request.DataSet.Tables[0]);
		if (request.FinalDataSet)
		{
			DataEnumerator.FinalDataTableAdded();
		}
		ReportRenderer.UpdateProgressNotificationBodyText(string.Format(CultureInfo.CurrentCulture, Lang.ReportProgressBody, DataEnumerator.RowsAdded));
	}

	public void Cancel()
	{
		ReportRenderer.CancelReport();
		ReportRenderer.CloseProgressNotification();
		if (DataEnumerator != null)
		{
			DataEnumerator.FinalDataTableAdded();
		}
	}

	public void Dispose()
	{
		if (reportHandle != null)
		{
			reportHandle.Dispose();
			reportHandle = null;
		}
		if (reportRenderer != null)
		{
			reportRenderer.Dispose();
			reportRenderer = null;
		}
		if (semaphore != null)
		{
			((IDisposable)semaphore).Dispose();
			semaphore = null;
		}
		GC.SuppressFinalize(this);
	}

	~ReportExecutionItem()
	{
		Dispose();
	}
}
