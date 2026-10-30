using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppSymbolReferenceLoader : ISymbolReferenceLoader
{
	private readonly SymbolReferenceSpecification moduleInfo;

	private readonly NavAppSymbolReferenceCache symbolCache;

	private readonly string tenantId;

	public NavAppSymbolReferenceLoader(SymbolReferenceSpecification moduleInfo, NavAppSymbolReferenceCache referenceCache = null, string tenantId = null)
	{
		this.moduleInfo = moduleInfo;
		symbolCache = referenceCache ?? new NavAppSymbolReferenceCache();
		this.tenantId = tenantId;
	}

	public ModuleDefinition LoadModule(SymbolReferenceSpecification reference, IList<Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic> diagnostics)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Loading dependency module with ID '{0}', name '{1}', publisher '{2}' and version '{3}' for target app with ID '{4}', name '{5}', publisher '{6}' and version '{7}'.", reference.AppId, reference.Name, reference.Publisher, reference.Version, moduleInfo.AppId, moduleInfo.Name, moduleInfo.Publisher, moduleInfo.Version);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("0000AA9", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			ModuleDefinition module = null;
			if (module == null && !symbolCache.TryGetModuleDefinition(reference.Name, reference.Publisher, reference.Version, out module))
			{
				module = GetModuleDefinitionUsingBaseAggregator(reference);
				if (module != null)
				{
					symbolCache.AddOrUpdateModuleDefinition(reference.Name, reference.Publisher, null, module, null);
				}
				else
				{
					NavAppMetadata symbolPackageMetadataUsingNavAppTable = GetSymbolPackageMetadataUsingNavAppTable(reference, tenantId);
					if (symbolPackageMetadataUsingNavAppTable != null)
					{
						module = GetModuleDefinitionUsingNavAppTable(symbolPackageMetadataUsingNavAppTable.PackageId, reference);
						if (string.IsNullOrEmpty(symbolPackageMetadataUsingNavAppTable.TenantId))
						{
							symbolCache.AddOrUpdateModuleDefinition(reference.Name, reference.Publisher, symbolPackageMetadataUsingNavAppTable.Version, module, symbolPackageMetadataUsingNavAppTable.PackageId);
						}
					}
				}
			}
			if (module == null)
			{
				throw new NavAppNotFoundException(string.Format(CultureInfo.InvariantCulture, "Symbols for the requested app {0} by {1} {2} could not be found in the database", reference.Name, reference.Publisher, reference.Version));
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000AA6", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, GetTraceStringForFoundDependency(reference, module.AppId, module.Name, module.Publisher, module.Version));
			return module;
		}
	}

	public IEnumerable<SymbolReferenceSpecification> GetDependencies(SymbolReferenceSpecification reference, IList<Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic> diagnostics)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Loading dependencies for dependency module with ID '{0}', name '{1}', publisher '{2}' and version '{3}' which targets app with ID '{4}', name '{5}', publisher '{6}' and version '{7}'.", reference.AppId, reference.Name, reference.Publisher, reference.Version, moduleInfo.AppId, moduleInfo.Name, moduleInfo.Publisher, moduleInfo.Version);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("0000AAA", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			IEnumerable<SymbolReferenceSpecification> enumerable = Enumerable.Empty<SymbolReferenceSpecification>();
			IEnumerable<NavAppRuntimeMetadata> enumerable2 = ((!string.IsNullOrEmpty(tenantId) && !string.Equals("nav-systemapplication", tenantId)) ? NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(tenantId) : (from m in NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata()
				where !m.IsDevExtension(tenantId)
				select m));
			NavAppRuntimeMetadata val = (from m in enumerable2
				where m.Name.Equals(reference.Name, StringComparison.OrdinalIgnoreCase) && m.Publisher.Equals(reference.Publisher, StringComparison.OrdinalIgnoreCase) && m.Version >= reference.Version
				orderby m.Version descending
				select m).FirstOrDefault();
			if (val != null)
			{
				NavAppManifest manifest = NavEnvironment.Instance.NavAppMetadataRetriever.GetManifest(val.PackageId);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000AA7", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, GetTraceStringForFoundDependency(reference, manifest.AppId, manifest.AppName, manifest.AppPublisher, manifest.AppVersion.ToString()));
				enumerable = manifest.GetAllReferences();
			}
			else if (SymbolReferenceSpecification.IsApplicationSymbolReference(reference.Name, reference.Publisher) && (reference.Version == null || reference.Version <= SymbolAggregator.GetApplicationVersion()))
			{
				IEnumerable<SymbolReferenceSpecification> dependenciesForNonNavAppApplication = GetDependenciesForNonNavAppApplication(enumerable2);
				if (dependenciesForNonNavAppApplication != null)
				{
					enumerable = dependenciesForNonNavAppApplication;
				}
			}
			foreach (SymbolReferenceSpecification item in enumerable)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000AA8", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "For dependency module with ID '{0}', name '{1}', publisher '{2}' and version '{3}', found secondary reference app with ID '{4}', name '{5}', publisher '{6}' and version '{7}'.", reference.AppId, reference.Name, reference.Publisher, reference.Version, item.AppId, item.Name, item.Publisher, item.Version);
			}
			return enumerable;
		}
	}

	private static string GetTraceStringForFoundDependency(SymbolReferenceSpecification reference, Guid foundReferenceAppId, string foundReferenceName, string foundReferencePublisher, string foundReferenceVersion)
	{
		return string.Format(CultureInfo.InvariantCulture, "For dependency module with ID '{0}', name '{1}', publisher '{2}' and version '{3}', found dependency module with ID '{4}', name '{5}', publisher '{6}' and version '{7}'.", reference.AppId, reference.Name, reference.Publisher, reference.Version, foundReferenceAppId, foundReferenceName, foundReferencePublisher, foundReferenceVersion);
	}

	private static IEnumerable<SymbolReferenceSpecification> GetDependenciesForNonNavAppApplication(IEnumerable<NavAppRuntimeMetadata> metadata)
	{
		NavAppRuntimeMetadata val = (from m in metadata
			where m.Name.Equals("System") && m.Publisher.Equals("Microsoft")
			orderby m.Version descending
			select m).FirstOrDefault();
		if (val != null)
		{
			SymbolReferenceSpecification symbolReferenceSpecification = SymbolReferenceSpecification.PlatformReference(val.Version);
			return new SymbolReferenceSpecification[1] { symbolReferenceSpecification };
		}
		return null;
	}

	private static ModuleDefinition GetModuleDefinitionUsingBaseAggregator(SymbolReferenceSpecification reference)
	{
		if (reference.IsApplicationSymbolReference())
		{
			SymbolAggregator symbolAggregator = NavGlobal.NCLMetadata.SymbolAggregator;
			if (symbolAggregator != null)
			{
				using MemoryStream memoryStream = symbolAggregator.CreateBaseAppSymbolReferenceStream();
				if (memoryStream != null && memoryStream.Length > 0)
				{
					ModuleDefinition moduleDefinition = SymbolReferenceJsonReader.ReadModule(memoryStream);
					moduleDefinition.Publisher = "Microsoft";
					moduleDefinition.AddBuiltInControlAddIns();
					return moduleDefinition;
				}
			}
		}
		return null;
	}

	private static NavAppMetadata GetSymbolPackageMetadataUsingNavAppTable(SymbolReferenceSpecification reference, string tenantId = null)
	{
		using NavSqlConnectionScope connectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Browse);
		AppMetadataStorage appMetadataStorage = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope);
		Result<NavAppMetadata> latestAppMetadata = appMetadataStorage.GetLatestAppMetadata(reference.Name, reference.Publisher, reference.Version, tenantId);
		if (((Result)latestAppMetadata).Success)
		{
			return latestAppMetadata.Value;
		}
		return null;
	}

	private static ModuleDefinition GetModuleDefinitionUsingNavAppTable(PackageId packageId, SymbolReferenceSpecification reference)
	{
		using (NavSqlConnectionScope connectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Browse))
		{
			List<Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic> diagnostics = new List<Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic>();
			AppMetadataStorage appMetadataStorage = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope);
			Result<ModuleDefinition> val = ResultExtensions.OnSuccess<Stream, ModuleDefinition>(appMetadataStorage.GetAppSymbolsStream(packageId), (Func<Stream, ModuleDefinition>)((Stream symbolsStream) => SymbolReferenceJsonReader.ReadModule(symbolsStream, diagnostics)));
			if (diagnostics.Count != 0)
			{
				string errorMessage = GetErrorMessage(diagnostics);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000966", Category.Extensions, Verbosity.Warning, "Error(s) getting module definition -- PackageId: {0} - Extension: {1} - Details: {2}", DiagnosticParameter.SystemMetadata(packageId), DiagnosticParameter.SystemMetadata(reference.ToDisplayString()), DiagnosticParameter.SystemMetadata(errorMessage));
			}
			if (((Result)val).Success)
			{
				return val.Value;
			}
		}
		return null;
	}

	private static string GetErrorMessage(List<Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic> diagnostics)
	{
		return string.Join(Environment.NewLine, diagnostics.Select((Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Diagnostic x) => x.GetMessage()));
	}
}
