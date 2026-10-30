using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Designer.Compiler;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal sealed class NavConfigurationDesignerExtension : NavDesignerExtension
{
	public NavDesignerProfile ConfigurationProfile { get; }

	public NavProfileKey ProfileKey { get; }

	public ConfigurationDesignerMode Mode { get; }

	public NavConfigurationDesignerExtension(INavPageCustomizationRepository pageCustomizationRepository, INavDesignerCompiler compiler, NavProfileKey profileKey, ConfigurationDesignerMode mode = ConfigurationDesignerMode.System)
		: base(compiler)
	{
		if (mode == ConfigurationDesignerMode.System && profileKey.Scope != ProfileScope.System)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "Profile Configuration in System Mode is not applicable for Tenant Profile: {0}", profileKey.ProfileId);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000024E", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, message);
			throw new InvalidOperationException(message);
		}
		base.pageCustomizationRepository = pageCustomizationRepository;
		SetExtensionProperties();
		ProfileKey = profileKey ?? NavCurrentThread.Session.ProfileKey;
		Mode = mode;
		ConfigurationProfile = NavDesignerProfile.Create("TemporaryConfigDesignerProfile", "TemporaryConfigDesignerProfile", NavConfigurationProfile.GetProfileRoleCenterId(ProfileKey).ToString(CultureInfo.InvariantCulture));
		Objects.Profiles.Add(ConfigurationProfile.Name, ConfigurationProfile);
		foreach (NavDesignerPageCustomization item in LoadProfileConfigurationALSource(ProfileKey, base.pageCustomizationRepository, (mode != ConfigurationDesignerMode.System) ? ProfilePageMetadataOwner.Tenant : ProfilePageMetadataOwner.SystemOrApp).SelectMany((string src) => NavDesignerPageCustomization.CreateWithUniqueId(src, this)))
		{
			if (item.Syntax != null && item.Syntax is PageCustomizationSyntax)
			{
				Objects.PageCustomizations.Add(item.ObjectName, item);
				ConfigurationProfile.AddPageCustomization(item.ObjectName);
			}
			else
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001F5", Category.Extensions, Verbosity.Verbose, "Error(s) loading Configuration Page extension -- Details:Object {0} Source is a invalid for Profile configurations", DiagnosticParameter.SystemMetadata(item.ObjectName));
			}
		}
		Compile();
	}

	private void Compile()
	{
		CompilationOptions compilationOptions = new CompilationOptions(concurrentBuild: true, 4, ReportDiagnostic.Default, reportSuppressedDiagnostics: false, null, Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Personalization, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings: true, CompilerFeatures.None, showMyCode: false, null, CompilationGenerationOptions.Code);
		INavDesignerCompiler compiler = Compiler;
		IEnumerable<ObjectSyntax> navObjectsSyntax = this.GetNavObjectsSyntax();
		IEnumerable<SymbolReferenceSpecification> allReferences = Manifest.GetAllReferences();
		NavAppPackageMetadataOutputter outputter = new NavAppPersonalizationMetadataOutputter();
		ObjectMetadata = compiler.CompilePersonalizationObjects(navObjectsSyntax, allReferences, compilationOptions, outputter);
		this.UpdateDesignerPageCustomizationsMetadata();
	}

	public override void Save()
	{
		try
		{
			Compile();
			SaveExtensionData();
		}
		catch (NavDesignerCompilerException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001QR", Category.Extensions, Verbosity.Warning, "Error(s) saving designer extension -- Details:{0}", DiagnosticParameter.SystemMetadata(string.Join(Environment.NewLine, ex.CompileMessages)));
			throw;
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001VL", Category.Extensions, exception);
			throw;
		}
	}

	private static IEnumerable<string> LoadProfileConfigurationALSource(NavProfileKey profileKey, INavPageCustomizationRepository pageCustomizationRepository, ProfilePageMetadataOwner owner = ProfilePageMetadataOwner.Tenant)
	{
		return pageCustomizationRepository.GetConfigurationALCode(profileKey, owner);
	}

	protected override void ApplyRuntimeDeltasToPageDefinition(ApplicationObjectId objectId, PageDefinition pageDefinition, string viewName)
	{
		List<NavAppObjectMetadataRuntimeDeltas> list = GetPageExtensionObjectMetadataForObject(objectId)?.ToList() ?? Enumerable.Empty<NavAppObjectMetadataRuntimeDeltas>().ToList();
		if (Mode == ConfigurationDesignerMode.Tenant)
		{
			NavAppObjectMetadataRuntimeDeltas val = NavEnvironment.Instance.NavProfilePageMetadataCache.Get(NavCurrentThread.Session.ProfileKey, objectId.ObjectNumber, ProfilePageMetadataOwner.SystemOrApp);
			if (val != null)
			{
				list.Add(val);
				pageDefinition.Properties.ConfigurationTypeApplied = NavAppObjectMetadataDeltaCreatorExtensions.CustomizationType((NavAppObjectMetadataDeltaCreator<Delta>)(object)val);
			}
		}
		if (objectId.ObjectType == ObjectType.Page)
		{
			NavAppObjectMetadataRuntimeDeltas currentDesignerPageDeltas = GetCurrentDesignerPageDeltas(objectId);
			if (currentDesignerPageDeltas != null)
			{
				list.Add(currentDesignerPageDeltas);
				pageDefinition.Properties.ConfigurationTypeApplied = NavAppObjectMetadataDeltaCreatorExtensions.CustomizationType((NavAppObjectMetadataDeltaCreator<Delta>)(object)currentDesignerPageDeltas);
			}
		}
		NavDesignerExtension.ApplyDeltas(objectId, pageDefinition, list);
	}

	public NavAppObjectMetadataRuntimeDeltas GetCurrentDesignerPageDeltas(ApplicationObjectId objectId)
	{
		string pageName = NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Page, objectId.ObjectNumber));
		return Objects.PageCustomizations.Values.SingleOrDefault((NavDesignerPageCustomization p) => p.BasePageName.Equals(pageName))?.RuntimeDeltas;
	}

	internal override void SaveExtensionData()
	{
		if (ObjectMetadata.Any((NavAppObjectMetadata o) => o.ObjectType == ObjectType.PageExtension && o.ObjectSubtype == null))
		{
			throw new NavDesignerTargetObjectSymbolMissingException(string.Format(CultureInfo.InvariantCulture, "\"Base Page Object Symbols missing for personalization.\""));
		}
		foreach (NavAppObjectMetadata item in ObjectMetadata.Where((NavAppObjectMetadata o) => o.ObjectType == ObjectType.PageExtension))
		{
			pageCustomizationRepository?.SaveConfiguration(ProfileKey, int.Parse(item.ObjectSubtype, CultureInfo.InvariantCulture), item.Metadata, item.UserALCode, (Mode == ConfigurationDesignerMode.Tenant) ? ProfilePageMetadataOwner.Tenant : ProfilePageMetadataOwner.SystemOrApp);
		}
		NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
	}

	public override INavDesignerPageOperations GetPageOperationInterfaceForPage(int pageId)
	{
		return (GetDesignerPageObject(pageId, createIfNotFound: true) as NavDesignerPageCustomization).Modifications;
	}

	public override NavDesignerObject GetDesignerPageObject(int pageId, bool createIfNotFound)
	{
		string pageName = NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Page, pageId));
		if (string.IsNullOrEmpty(pageName))
		{
			throw new NavMetadataNotFoundException(ObjectType.Page, pageId);
		}
		NavDesignerPageCustomization navDesignerPageCustomization = Objects.PageCustomizations.Values.SingleOrDefault((NavDesignerPageCustomization p) => p.BasePageName.Equals(pageName));
		if ((navDesignerPageCustomization == null) & createIfNotFound)
		{
			string unusedPageCustomizationId = this.GetUnusedPageCustomizationId();
			navDesignerPageCustomization = NavDesignerPageCustomization.Create(unusedPageCustomizationId, pageName);
			ConfigurationProfile.AddPageCustomization(navDesignerPageCustomization.ObjectName);
			Objects.PageCustomizations.Add(navDesignerPageCustomization.ObjectName, navDesignerPageCustomization);
		}
		return navDesignerPageCustomization;
	}

	public override NavDesignerPageModifications GetPageModifications(int basePageId)
	{
		string basePageName = NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Page, basePageId));
		return Objects.PageCustomizations.Values.FirstOrDefault((NavDesignerPageCustomization pc) => pc.BasePageName.Equals(basePageName, StringComparison.OrdinalIgnoreCase) || pc.BasePageName.Equals(basePageId.ToString(CultureInfo.InvariantCulture)))?.Modifications;
	}
}
