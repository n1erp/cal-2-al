using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavCompany : TreeObject, IDisposable
{
	private ITreeSharedObjectContainer sharedObjects;

	private Dictionary<Guid, NavFormHandle> registeredForms;

	private Dictionary<Guid, NavReportHandle> registeredReports;

	private Dictionary<Type, NavCodeunitHandle> singleInstanceCodeunits;

	private string companyName;

	private readonly bool evaluationCompany;

	private NavGuid companyTableId = NavGuid.Default;

	private NavTrackChanges trackChanges;

	private readonly int companyNameToken;

	private NavOpenedDialogRegistry openedDialogRegistry = new NavOpenedDialogRegistry();

	public NavSession Parent => (NavSession)Tree.Parent;

	public ITreeSharedObjectContainer SharedObjects => sharedObjects;

	public NavTrackChanges TrackChanges => trackChanges;

	public string CompanyName => companyName;

	public bool IsEvaluationCompany => evaluationCompany;

	public string CompanyDisplayName
	{
		get
		{
			if (string.IsNullOrEmpty(companyName))
			{
				return string.Empty;
			}
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000006 }))
			{
				using NavRecord navRecord = new NavRecord(Parent, 2000000006);
				string companyDisplayName = string.Empty;
				if (navRecord.ALGet(DataError.TrapError, new NavText(companyName)))
				{
					companyDisplayName = navRecord.GetFieldValue(3).ToString();
				}
				return GetCompanyDisplayNameDefaulted(companyDisplayName, companyName);
			}
		}
	}

	public NavGuid CompanyTableId
	{
		get
		{
			if (companyTableId == NavGuid.Default)
			{
				using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000006 }))
				{
					using NavRecord navRecord = new NavRecord(Parent, 2000000006);
					navRecord.ALSetFilter(1, CompanyName);
					navRecord.ALFindFirst();
					companyTableId = (NavGuid)navRecord.GetFieldValue(8000);
				}
			}
			return companyTableId;
		}
	}

	public int CompanyNameToken
	{
		[DebuggerStepThrough]
		get
		{
			return companyNameToken;
		}
	}

	internal IDictionary<Type, NavCodeunitHandle> SingleInstanceCodeunits => singleInstanceCodeunits;

	internal NavSystemCodeunitFactory SystemCodeunitFactory { get; }

	public NavCompany(NavSession parent, string companyName, bool evaluationCompany)
		: base(parent)
	{
		SystemCodeunitFactory = new NavSystemCodeunitFactory(this);
		sharedObjects = new TreeSharedObjectContainer(this);
		registeredForms = new Dictionary<Guid, NavFormHandle>();
		registeredReports = new Dictionary<Guid, NavReportHandle>();
		singleInstanceCodeunits = new Dictionary<Type, NavCodeunitHandle>();
		trackChanges = new NavTrackChanges(this);
		this.evaluationCompany = evaluationCompany;
		if (companyName == null)
		{
			this.companyName = string.Empty;
		}
		else
		{
			if (companyName.Length == 0)
			{
				throw new NavNCLInvalidCompanyNameException();
			}
			this.companyName = companyName;
		}
		companyNameToken = Parent.Database.GetCompanyNameToken(this.companyName);
	}

	public static string GetCompanyDisplayNameDefaulted(string companyDisplayName, string companyName)
	{
		if (!string.IsNullOrEmpty(companyDisplayName))
		{
			return companyDisplayName;
		}
		return companyName;
	}

	public void RegisterForm(NavForm form)
	{
		RegisterFormInternal(form, enableFormChangeTracking: true);
	}

	public void RegisterFormWithoutChangeTracking(NavForm form)
	{
		RegisterFormInternal(form, enableFormChangeTracking: false);
	}

	private void RegisterFormInternal(NavForm form, bool enableFormChangeTracking)
	{
		if (form == null)
		{
			throw new ArgumentNullException("form");
		}
		Guid handle = form.Handle;
		lock (registeredForms)
		{
			if (registeredForms.ContainsKey(handle))
			{
				throw new NavNCLInvalidFormHandleException(Lang._00095_50059);
			}
			NavFormHandle value = new NavFormHandle(this, form);
			registeredForms.Add(handle, value);
			if (enableFormChangeTracking)
			{
				EnableFormChangeTracking(form);
			}
		}
	}

	internal void EnableFormChangeTracking(NavForm form)
	{
		if (form.SourceTable != null)
		{
			TrackChanges.RegisterFormTableChanges(form.Handle, form.SourceTable.InstanceIdentifier);
		}
	}

	public void RegisterReport(NavReport report)
	{
		if (report == null)
		{
			throw new ArgumentNullException("report");
		}
		Guid executionGuid = report.ExecutionGuid;
		lock (registeredReports)
		{
			if (registeredReports.ContainsKey(executionGuid))
			{
				throw new NavNCLInvalidReportHandleException("A report with an identical handle has previously been registered.");
			}
			NavReportHandle value = new NavReportHandle(this, report);
			registeredReports.Add(executionGuid, value);
		}
	}

	public void UnregisterForm(NavForm form)
	{
		if (form == null)
		{
			throw new ArgumentNullException("form");
		}
		Guid handle = form.Handle;
		NavFormHandle value = null;
		try
		{
			lock (registeredForms)
			{
				if (!registeredForms.TryGetValue(handle, out value))
				{
					throw new NavNCLInvalidFormHandleException(Lang._00095_50061);
				}
				registeredForms.Remove(handle);
				if (form.SourceTable != null)
				{
					TrackChanges.UnregisterFormTableChanges(handle, form.SourceTable.InstanceIdentifier);
				}
				if (Parent.RunTableContext != null && !registeredForms.Values.Any((NavFormHandle tableFormHandle) => tableFormHandle.Target is NavTableForm))
				{
					Parent.ClearRunTableContext();
				}
			}
		}
		finally
		{
			value?.Dispose();
		}
	}

	public void UnregisterReport(NavReport report)
	{
		if (report == null)
		{
			throw new ArgumentNullException("report");
		}
		Guid executionGuid = report.ExecutionGuid;
		NavReportHandle value = null;
		try
		{
			lock (registeredReports)
			{
				if (!registeredReports.TryGetValue(executionGuid, out value))
				{
					throw new NavNCLInvalidReportHandleException("A report with an identical handle has previously been registered.");
				}
				registeredReports.Remove(executionGuid);
			}
		}
		finally
		{
			value?.Dispose();
		}
	}

	public NavForm GetRegisteredForm(Guid formIdentifier)
	{
		lock (registeredForms)
		{
			if (!registeredForms.TryGetValue(formIdentifier, out var value))
			{
				return null;
			}
			return value.Target;
		}
	}

	public NavReport GetRegisteredReport(Guid reportHandle)
	{
		lock (registeredReports)
		{
			if (!registeredReports.TryGetValue(reportHandle, out var value))
			{
				return null;
			}
			return value.Target;
		}
	}

	public NavFormHandle CreateRegisteredFormReference(ITreeObject parentOfResult, Guid formIdentifier)
	{
		lock (registeredForms)
		{
			if (!registeredForms.TryGetValue(formIdentifier, out var value))
			{
				return null;
			}
			return new NavFormHandle(parentOfResult, value.Target);
		}
	}

	public bool GetRegisteredFormIntoFormReference(Guid formIdentifier, NavFormHandle formReferenceToStoreResult)
	{
		if (formReferenceToStoreResult == null)
		{
			throw new ArgumentNullException("formReferenceToStoreResult");
		}
		lock (registeredForms)
		{
			if (!registeredForms.TryGetValue(formIdentifier, out var value))
			{
				return false;
			}
			formReferenceToStoreResult.Target = value.Target;
			return true;
		}
	}

	internal void RegisterOpenDialog(NavDialog dialog)
	{
		openedDialogRegistry.RegisterOpenDialog(dialog);
	}

	public void UnregisterOpenDialog(NavDialog dialog)
	{
		openedDialogRegistry.UnregisterOpenDialog(dialog);
	}

	internal void PushOpenedDialogTracking(NavOpenDialogTracking tracking)
	{
		openedDialogRegistry.RegisteredOpenDialogTracking.Push(tracking);
	}

	internal void PopUntilOpenedDialogTracking(NavOpenDialogTracking tracking)
	{
		openedDialogRegistry.PopUntilOpenedDialogTracking(tracking);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !IsDisposed)
			{
				try
				{
					Tree.DisposeAllChildren();
					trackChanges = null;
					return;
				}
				finally
				{
					companyName = string.Empty;
					registeredForms.Clear();
					registeredReports.Clear();
					singleInstanceCodeunits.Clear();
				}
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
