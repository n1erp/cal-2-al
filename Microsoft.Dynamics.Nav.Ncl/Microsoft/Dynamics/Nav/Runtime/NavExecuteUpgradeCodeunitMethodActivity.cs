using System.Activities;
using System.Globalization;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Workflow;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavExecuteUpgradeCodeunitMethodActivity : NavAsynchronousBaseActivity
{
	private int sessionId;

	private int codeunitId;

	private string methodName;

	private string companyName;

	private INavDataUpgradeMetadata upgradeMetadata;

	private INavDataUpgradeProgressMonitor upgradeProgressMonitor;

	private INavUpgradeCodeunitObjectFactory upgradeCodeunitObjectFactory;

	[RequiredArgument]
	public InArgument<int> CodeunitId { get; set; }

	[RequiredArgument]
	public InArgument<string> MethodName { get; set; }

	[RequiredArgument]
	public InArgument<string> CompanyName { get; set; }

	protected override void ExecuteBeforeAsync(AsyncCodeActivityContext context)
	{
		codeunitId = ArgumentsHandler.LoadInArgument(context, CodeunitId);
		methodName = ArgumentsHandler.LoadInArgument(context, MethodName);
		ArgumentsHandler.CheckForNull(methodName, "MethodName");
		companyName = ArgumentsHandler.LoadInArgument(context, CompanyName);
		upgradeMetadata = context.GetExtension<INavDataUpgradeMetadata>();
		ArgumentsHandler.CheckForNull(upgradeMetadata, "upgradeMetadata");
		upgradeProgressMonitor = context.GetExtension<INavDataUpgradeProgressMonitor>();
		ArgumentsHandler.CheckForNull(upgradeProgressMonitor, "upgradeProgressMonitor");
		upgradeCodeunitObjectFactory = context.GetExtension<INavUpgradeCodeunitObjectFactory>();
		ArgumentsHandler.CheckForNull(upgradeCodeunitObjectFactory, "upgradeCodeunitObjectFactory");
		base.ExecuteBeforeAsync(context);
	}

	private void RunCompanyinitialization(NavSession navSession)
	{
		using (DiagnosticsResolver.CreateTimingScope("00000UM", Category.DataUpgrade, DiagnosticParameter.Oii(string.Format(CultureInfo.CurrentCulture, "Running Company Initialization on session {0} for company {1} during upgrade", navSession.Id, companyName))))
		{
			NCLMetaCodeunit companyInitializationCodeunit = upgradeMetadata.GetCompanyInitializationCodeunit(codeunitId);
			if (companyInitializationCodeunit == null)
			{
				throw new NavUpgradeProcessExecutionException(string.Format(CultureInfo.CurrentCulture, Lang.CompanyInitializationCodeunitNotFound, codeunitId));
			}
			try
			{
				navSession.OpenCompany(companyName, updateUserPersonalization: false);
				using (new NavCodeunitHandle(navSession, companyInitializationCodeunit.CodeunitId))
				{
					NavCodeunit.InvokeRun(companyInitializationCodeunit.CodeunitId, new object[0]);
				}
			}
			catch (NavBaseException ex)
			{
				string message = string.Format(CultureInfo.CurrentCulture, Lang.CompanyInitializationCodeunitExecutionError, methodName, companyInitializationCodeunit.CodeunitId, companyName, ex.Message);
				throw new NavUpgradeCodeunitExecutionException(message, ex);
			}
		}
	}

	private void RunUpgradeMethod(NavSession navSession)
	{
		NCLMetaCodeunit upgradeCodeunit = upgradeMetadata.GetUpgradeCodeunit(codeunitId);
		if (upgradeCodeunit == null)
		{
			throw new NavUpgradeProcessExecutionException(string.Format(CultureInfo.CurrentCulture, Lang.UpgradeCodeunitNotFound, codeunitId));
		}
		navSession.Diagnostics.SendTraceTag("00000E3", Category.DataUpgrade, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Running upgrade method on session {0}, id = {1}, method={2}", navSession.Id, upgradeCodeunit.CodeunitId, methodName);
		using NavUpgradeCodeunit navUpgradeCodeunit = upgradeCodeunitObjectFactory.CreateCodeunitObject(navSession, upgradeCodeunit);
		navUpgradeCodeunit.InvokeCodeunitMethod(methodName, companyName, new object[0]);
	}

	protected override void ExecuteAsync()
	{
		try
		{
			SessionManager.RunActionInSystemSession((NavSession navSession) =>
			{
				sessionId = navSession.Id;
				if (codeunitId == upgradeMetadata.CompanyInitializationMethod.Item1)
				{
					RunCompanyinitialization(navSession);
				}
				else
				{
					RunUpgradeMethod(navSession);
				}
			});
		}
		catch (NavBaseException exception)
		{
			upgradeProgressMonitor.ReportError(codeunitId, methodName, companyName, exception);
			throw;
		}
		catch (TargetException ex)
		{
			upgradeProgressMonitor.ReportError(codeunitId, methodName, companyName, ex);
			throw new NavUpgradeCodeunitExecutionException(ex.Message, ex);
		}
	}

	protected override void Cancel(AsyncCodeActivityContext context)
	{
		if (context.IsCancellationRequested && sessionId != 0)
		{
			SessionManager.StopSynchronouslyExecutionAndDisposeSession(sessionId, removedFromTrackedCollection: true);
		}
		base.Cancel(context);
		context.MarkCanceled();
	}
}
