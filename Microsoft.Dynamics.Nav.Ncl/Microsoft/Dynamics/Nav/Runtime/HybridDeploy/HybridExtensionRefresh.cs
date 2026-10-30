using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.HybridDeploy;

public static class HybridExtensionRefresh
{
	public static Result UninstallReinstallExtension(KeyValuePair<Guid, List<string>> extensionDetail, NavTenant tenant, NavSession session)
	{
		string text = null;
		try
		{
			List<NavAppRuntimeMetadata> published = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(tenant.Id).ToList();
			List<NavAppRuntimeMetadata> list = tenant.NavAppGroup.OrderedAppMetadata.ToList();
			if (list.Any())
			{
				NavAppRuntimeMetadata val = list.FirstOrDefault((NavAppRuntimeMetadata e) => ((WrappedValue<Guid>)(object)e.AppId).Value == extensionDetail.Key);
				if (val != null)
				{
					text = val.Name;
					IEnumerable<NavAppRuntimeMetadata> first = NavAppV2DependencyResolver.AppsDependentOnPackage(val, list, published);
					IEnumerable<NavAppRuntimeMetadata> source = first.Concat(new List<NavAppRuntimeMetadata> { val });
					NavManagementTasks navManagementTasks = new NavManagementTasks(session);
					UninstallNavAppArguments uninstallNavAppArguments = new UninstallNavAppArguments
					{
						Name = val.Name,
						Publisher = val.Publisher,
						Version = val.Version,
						TenantId = tenant.Id,
						DoNotSaveData = false,
						Force = true,
						Origin = "HybridExtensionRefresh"
					};
					tenant.Diagnostics.SendTraceTag("00007EE", Category.HybridDeploy, Verbosity.Normal, "Uninstalling extension {0} on tenant {1}", DiagnosticParameter.Oii(text), DiagnosticParameter.Oii(tenant.Id));
					ChangeNavAppStateResponse changeNavAppStateResponse = navManagementTasks.UninstallNavApp(uninstallNavAppArguments);
					ChangeNavAppStateResult result = changeNavAppStateResponse.Result;
					if (result != ChangeNavAppStateResult.AppChangeSuccessful && result != ChangeNavAppStateResult.AppNotFound && result != ChangeNavAppStateResult.AppAlreadyChangedWarning)
					{
						tenant.Diagnostics.SendTraceTag("00007EF", Category.HybridDeploy, Verbosity.Error, "Error {0} while uninstalling extension {1} on tenant {2}", DiagnosticParameter.Oii(changeNavAppStateResponse.Error), DiagnosticParameter.Oii(text), DiagnosticParameter.Oii(tenant.Id));
						return Result.Fail(Error.FromMessage(text, ErrorType.Unspecified));
					}
					IEnumerable<NavAppRuntimeMetadata> enumerable = source.Reverse();
					foreach (NavAppRuntimeMetadata item in enumerable)
					{
						InstallNavAppArguments installNavAppArguments = new InstallNavAppArguments
						{
							Name = item.Name,
							Publisher = item.Publisher,
							Version = item.Version,
							TenantId = tenant.Id,
							Origin = "HybridExtensionRefresh"
						};
						tenant.Diagnostics.SendTraceTag("00007EG", Category.HybridDeploy, Verbosity.Normal, "Installing extension {0} on tenant {1}", DiagnosticParameter.Oii(item.Name), DiagnosticParameter.Oii(tenant.Id));
						changeNavAppStateResponse = navManagementTasks.InstallNavApp(installNavAppArguments);
						if (!changeNavAppStateResponse.Result.In(ChangeNavAppStateResult.AppChangeSuccessful, ChangeNavAppStateResult.AppAlreadyChangedWarning))
						{
							tenant.Diagnostics.SendTraceTag("00007EH", Category.HybridDeploy, Verbosity.Error, "Error {0} while installing extension {1} on tenant {2}", DiagnosticParameter.Oii(changeNavAppStateResponse.Error), DiagnosticParameter.Oii(item.Name), DiagnosticParameter.Oii(tenant.Id));
							return Result.Fail(Error.FromMessage(item.Name, ErrorType.Unspecified));
						}
					}
					tenant.Diagnostics.SendTraceTag("00007H0", Category.HybridDeploy, Verbosity.Normal, "Unblocking extension tables corresponding to app {0} on tenant {1}", DiagnosticParameter.Oii(extensionDetail.Key), DiagnosticParameter.Oii(tenant.Id));
					foreach (string tableName in extensionDetail.Value)
					{
						HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
						{
							using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenant.Database, TransactionType.UpdateNoLocks);
							NavSqlCloudReadyReplicationStatus.UpdateBlockedStatus(conScope, tableName, value: false);
						}, "", null, "UnblockRefreshedExtensionTables");
					}
				}
			}
			return Result.Ok();
		}
		catch (Exception ex)
		{
			tenant.Diagnostics.SendTraceTag("00007EI", Category.HybridDeploy, Verbosity.Error, "Error {0} during extension refresh for extension {1} on tenant {2}", DiagnosticParameter.Oii(ex.Message), DiagnosticParameter.Oii(extensionDetail.Key), DiagnosticParameter.Oii(tenant.Id));
			return Result.Fail(Error.FromMessage(text ?? extensionDetail.Key.ToString(), ErrorType.Unspecified));
		}
	}
}
