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

internal sealed class NavPersonalizationDesignerExtension : NavDesignerExtension
{
	public NavDesignerProfile PersonalizationProfile { get; }

	public NavPersonalizationDesignerExtension(INavPageCustomizationRepository pageCustomizationRepository, INavDesignerCompiler compiler)
		: base(compiler)
	{
		base.pageCustomizationRepository = pageCustomizationRepository;
		SetExtensionProperties();
		string rolecenterId = NavConfigurationProfile.GetProfileRoleCenterId(NavCurrentThread.Session.ProfileKey).ToString(CultureInfo.InvariantCulture);
		PersonalizationProfile = NavDesignerProfile.Create("TemporaryDesignerProfile", "TemporaryDesignerProfile", rolecenterId);
		Objects.Profiles.Add(PersonalizationProfile.Name, PersonalizationProfile);
		IEnumerable<string> personalizationALCode = base.pageCustomizationRepository.GetPersonalizationALCode(NavCurrentThread.Session.User.Id);
		foreach (NavDesignerPageCustomization item in personalizationALCode.SelectMany((string src) => NavDesignerPageCustomization.CreateWithUniqueId(src, this)))
		{
			if (item.Syntax is PageCustomizationSyntax)
			{
				Objects.PageCustomizations.Add(item.ObjectName, item);
				PersonalizationProfile.AddPageCustomization(item.ObjectName);
			}
			else
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000010X", Category.Extensions, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Error(s) loading personalization Page extension -- Details:Object {0} Source is a invalid for personalization", item.ObjectName);
			}
		}
		Compile();
	}

	private void Compile()
	{
		IEnumerable<SymbolReferenceSpecification> personalizationReferences = NavDesignerUtil.GetPersonalizationReferences();
		CompilationOptions compilationOptions = new CompilationOptions(concurrentBuild: true, 4, ReportDiagnostic.Default, reportSuppressedDiagnostics: false, null, Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Personalization, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings: true, CompilerFeatures.None, showMyCode: false, null, CompilationGenerationOptions.Code);
		INavDesignerCompiler compiler = Compiler;
		IEnumerable<ObjectSyntax> navObjectsSyntax = this.GetNavObjectsSyntax();
		NavAppPackageMetadataOutputter outputter = new NavAppPersonalizationMetadataOutputter();
		ObjectMetadata = compiler.CompilePersonalizationObjects(navObjectsSyntax, personalizationReferences, compilationOptions, outputter);
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

	private void SavePersonalizationData()
	{
		if (ObjectMetadata.Any((NavAppObjectMetadata o) => o.ObjectType == ObjectType.PageExtension && o.ObjectSubtype == null))
		{
			throw new NavDesignerTargetObjectSymbolMissingException(string.Format(CultureInfo.InvariantCulture, "\"Base Page Object Symbols missing for personalization.\""));
		}
		foreach (NavAppObjectMetadata item in ObjectMetadata.Where((NavAppObjectMetadata o) => o.ObjectType == ObjectType.PageExtension && o.ObjectSubtype != null))
		{
			pageCustomizationRepository?.SavePersonalization(int.Parse(item.ObjectSubtype, CultureInfo.InvariantCulture), item.Metadata, item.UserALCode);
		}
		NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
	}

	protected override void ApplyRuntimeDeltasToPageDefinition(ApplicationObjectId objectId, PageDefinition pageDefinition, string viewName)
	{
		IEnumerable<NavAppObjectMetadataRuntimeDeltas> pageExtensionObjectMetadataForObject = GetPageExtensionObjectMetadataForObject(objectId);
		NavDesignerExtension.ApplyDeltas(objectId, pageDefinition, pageExtensionObjectMetadataForObject);
		NavAppObjectMetadataRuntimeDeltas configurationDeltas = NavEnvironment.Instance.NavProfilePageMetadataCache.Get(NavCurrentThread.Session.ProfileKey, objectId.ObjectNumber);
		NavAppObjectMetadataRuntimeDeltas currentDesignerPageDeltas = GetCurrentDesignerPageDeltas(objectId);
		MetadataProvider.ApplyPageCustomizationDeltas(pageDefinition, configurationDeltas, currentDesignerPageDeltas, viewName);
	}

	public NavAppObjectMetadataRuntimeDeltas GetCurrentDesignerPageDeltas(ApplicationObjectId objectId)
	{
		string pageName = NavGlobal.NCLMetadata.GetObjectName(objectId);
		return Objects.PageCustomizations.Values.SingleOrDefault((NavDesignerPageCustomization p) => p.BasePageName.Equals(pageName))?.RuntimeDeltas;
	}

	internal override void SaveExtensionData()
	{
		SavePersonalizationData();
	}

	public override INavDesignerPageOperations GetPageOperationInterfaceForPage(int pageId)
	{
		return (GetDesignerPageObject(pageId, createIfNotFound: true) as NavDesignerPageCustomization)?.Modifications;
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
			PersonalizationProfile.AddPageCustomization(navDesignerPageCustomization.ObjectName);
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
