using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDataUpgradeManager
{
	private NavDataUpgradeProcessExecutionState cachedState;

	private readonly string tenantId;

	private readonly INavDataUpgradeMetadata upgradeMetadata;

	private INavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState> navDataUpgradeProcessWorkflow;

	private readonly Func<INavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState>> getNavDataUpgradeProcessWorkflow;

	private readonly object lockObject = new object();

	public IEnumerable<NavAppJointUpgradeContext> ExtensionUpgradeContexts
	{
		get
		{
			lock (lockObject)
			{
				INavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState> workflow = GetWorkflow();
				return (workflow as NavDataUpgradeWorkflow)?.ExtensionUpgradeContexts;
			}
		}
	}

	public NavDataUpgradeManager(INavDataUpgradeMetadata upgradeMetadata, NavTenant tenant)
		: this(upgradeMetadata, () => new NavDataUpgradeWorkflow(tenant, upgradeMetadata), tenant.Id)
	{
	}

	public NavDataUpgradeManager(INavDataUpgradeMetadata upgradeMetadata, Func<INavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState>> getNavUpgradeProcessWorkflow, string tenantId)
	{
		if (upgradeMetadata == null)
		{
			throw new ArgumentNullException("upgradeMetadata");
		}
		if (getNavUpgradeProcessWorkflow == null)
		{
			throw new ArgumentNullException("getNavUpgradeProcessWorkflow");
		}
		this.upgradeMetadata = upgradeMetadata;
		getNavDataUpgradeProcessWorkflow = getNavUpgradeProcessWorkflow;
		this.tenantId = tenantId;
	}

	public NavDataUpgradeProcessExecutionDetails GetLegacyUpgradeInformation()
	{
		NavDataUpgradeProcessExecutionState navDataUpgradeProcessExecutionState = null;
		lock (lockObject)
		{
			if (cachedState != null)
			{
				navDataUpgradeProcessExecutionState = cachedState;
			}
			else
			{
				navDataUpgradeProcessExecutionState = ((navDataUpgradeProcessWorkflow != null) ? navDataUpgradeProcessWorkflow.GetWorkflowStatus() : NavDataUpgradeProcessExecutionState.Generate(new List<NavDataUpgradeExecutionDetails>(), NavWorkflowState.NotStarted, tenantId));
			}
		}
		return navDataUpgradeProcessExecutionState.ToNavDataUpgradeProcessExecutionDetails();
	}

	public NavDataUpgradeProcessExecutionState GetUpgradeInformation()
	{
		NavDataUpgradeProcessExecutionState result = null;
		lock (lockObject)
		{
			if (cachedState != null)
			{
				result = cachedState;
			}
			else
			{
				if (navDataUpgradeProcessWorkflow == null)
				{
					return NavDataUpgradeProcessExecutionState.Generate(new List<NavDataUpgradeExecutionDetails>(), NavWorkflowState.NotStarted, tenantId);
				}
				result = navDataUpgradeProcessWorkflow.GetWorkflowStatus();
			}
		}
		return result;
	}

	public void StartUpgrade(IEnumerable<string> companyNames, int lcid, bool parallelExecutionEnabled, bool continueOnErrorEnabled, bool skipCompanyInitialization, bool singleTransaction, bool excludeExtensions, bool stopRunningSessions = true)
	{
		NavDataUpgradeWorkflowStartRequest startRequest = new NavDataUpgradeWorkflowStartRequest
		{
			TenantId = tenantId,
			Companies = companyNames,
			LanguageLcid = lcid,
			ParallelExecutionEnabled = parallelExecutionEnabled,
			ContinueOnErrorEnabled = continueOnErrorEnabled,
			SingleTransaction = singleTransaction,
			SkipCompanyInitialization = skipCompanyInitialization,
			ExcludeExtensions = excludeExtensions,
			StopRunningSessions = stopRunningSessions,
			OnDone = (NavDataUpgradeProcessExecutionState state) =>
			{
				lock (lockObject)
				{
					cachedState = state;
					navDataUpgradeProcessWorkflow = null;
				}
			}
		};
		cachedState = null;
		navDataUpgradeProcessWorkflow = getNavDataUpgradeProcessWorkflow();
		navDataUpgradeProcessWorkflow.StartWorkflow(startRequest);
	}

	public void ResumeUpgrade()
	{
		lock (lockObject)
		{
			INavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState> workflow = GetWorkflow();
			workflow.ResumeWorkflow(new NavDataUpgradeWorkflowResumeRequest
			{
				ResumeAllMode = true
			});
		}
	}

	public void ResumeUpgrade(int codeunitId, string methodName, string companyName)
	{
		lock (lockObject)
		{
			INavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState> workflow = GetWorkflow();
			NavDataUpgradeWorkflowResumeRequest resumeRequest = new NavDataUpgradeWorkflowResumeRequest
			{
				CodeunitId = codeunitId,
				MethodName = methodName,
				CompanyName = companyName
			};
			workflow.ResumeWorkflow(resumeRequest);
		}
	}

	public void StopUpgrade()
	{
		lock (lockObject)
		{
			INavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState> workflow = GetWorkflow();
			workflow.StopWorkflow();
		}
	}

	private INavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState> GetWorkflow()
	{
		INavWorkflow<NavDataUpgradeWorkflowStartRequest, NavDataUpgradeWorkflowResumeRequest, NavDataUpgradeProcessExecutionState> navWorkflow = navDataUpgradeProcessWorkflow;
		if (navWorkflow == null)
		{
			throw new NavUpgradeProcessExecutionException(Lang.UpgradeProcessNotFoundError);
		}
		return navWorkflow;
	}

	internal void RunTableSyncSetup(NavSession navSession, IEnumerable<int> tableIds)
	{
		TableSynchSetupDataProvider instanceForSession = TableSynchSetupDataProvider.GetInstanceForSession(navSession);
		instanceForSession.SetTableIds(tableIds);
		using (DiagnosticsResolver.CreateTimingScope("00000RB", Category.DataUpgrade, DiagnosticParameter.SystemMetadata("Running table sync setup on all code units")))
		{
			foreach (NCLMetaCodeunit upgradeCodeunit in upgradeMetadata.UpgradeCodeunits)
			{
				using NavUpgradeCodeunit navUpgradeCodeunit = new NavUpgradeCodeunitObjectFactory().CreateCodeunitObject(navSession, upgradeCodeunit);
				navUpgradeCodeunit.RunTableSyncSetup();
			}
		}
	}
}
