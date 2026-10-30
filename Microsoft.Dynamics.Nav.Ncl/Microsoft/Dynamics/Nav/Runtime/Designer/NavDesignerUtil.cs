using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal static class NavDesignerUtil
{
	internal static readonly CompilationOptions PersonalizationCompilationOptions = new CompilationOptions(concurrentBuild: true, 4, ReportDiagnostic.Default, reportSuppressedDiagnostics: false, null, Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Personalization, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings: true, CompilerFeatures.None, showMyCode: false, null, CompilationGenerationOptions.Code);

	internal static Compilation CompilationForALIdentification
	{
		get
		{
			if (NavEnvironment.Instance.CompilationWithSystemSymbols == null)
			{
				Compilation compilation = Compilation.Create("SymbolLookup", null, null, null, null, null, null, NavGlobal.SystemTenant.DotNetResolverFactory);
				NavEnvironment.Instance.CompilationWithSystemSymbols = compilation.WithReferenceLoader(new NavAppSymbolReferenceLoader(compilation.ModuleInfo, NavGlobal.SystemTenant.SymbolReferenceCache, NavCurrentThread.Session.Tenant.Id)).WithReferences(GetSystemSymbolReferences());
			}
			return NavEnvironment.Instance.CompilationWithSystemSymbols;
		}
	}

	internal static IEnumerable<SymbolReferenceSpecification> GetPersonalizationReferences(NavAppManifest manifest = null)
	{
		return GetSystemSymbolReferences().Concat(GetSymbolReferenceSpecifications(GetAllInstalledV2ExtensionsAsDependencies(manifest)));
	}

	internal static IEnumerable<NavAppDependency> GetAllInstalledV2ExtensionsAsDependencies(NavAppManifest manifest = null)
	{
		List<NavAppDependency> list = new List<NavAppDependency>();
		IReadOnlyList<NavAppRuntimeMetadata> orderedAppMetadata = NavCurrentThread.ResolveAppGroup().OrderedAppMetadata;
		foreach (NavAppRuntimeMetadata item in orderedAppMetadata)
		{
			if (item.ExtensionType == NavExtensionType.ModernDev && (manifest == null || manifest.AppId != ((WrappedValue<Guid>)(object)item.AppId).Value))
			{
				list.Add(new NavAppDependency
				{
					AppId = ((WrappedValue<Guid>)(object)item.AppId).Value,
					CompatibilityId = item.CompatibilityId,
					MinVersion = item.Version,
					Name = item.Name,
					Publisher = item.Publisher
				});
			}
		}
		return list;
	}

	internal static int GetUnusedObjectId(ObjectType objectType, IdSpaces idSpaces, NavDesignerExtension designerExtension = null)
	{
		int result = 0;
		if ((uint)(objectType - 14) <= 1u)
		{
			IReadOnlyList<SpecificNavAppObjectMetadataSummaryPair> extensionObjects = NavCurrentThread.ResolveAppGroup().GetExtensionObjects();
			foreach (IdSpace applicationObjectIdRange in idSpaces.ApplicationObjectIdRanges)
			{
				for (int i = applicationObjectIdRange.ApplicationObjectIdRangeStart; i < applicationObjectIdRange.ApplicationObjectIdRangeEnd; i++)
				{
					if (!IsObjectIdInUse(extensionObjects, i, objectType, designerExtension))
					{
						result = i;
						break;
					}
				}
			}
		}
		else
		{
			foreach (IdSpace applicationObjectIdRange2 in idSpaces.ApplicationObjectIdRanges)
			{
				for (int j = applicationObjectIdRange2.ApplicationObjectIdRangeStart; j < applicationObjectIdRange2.ApplicationObjectIdRangeEnd; j++)
				{
					try
					{
						if (!NavGlobal.NCLMetadata.TryGetMetaApplicationObject(new ApplicationObjectId(objectType, j), out var _, -1, requireCompiled: false))
						{
							result = j;
							break;
						}
					}
					catch (NavMetadataException)
					{
						result = j;
						break;
					}
				}
			}
		}
		return result;
	}

	internal static bool IsObjectIdInUse(ObjectType objectType, int id, NavDesignerExtension designerExtension = null)
	{
		bool result = false;
		if ((uint)(objectType - 14) <= 1u)
		{
			IReadOnlyList<SpecificNavAppObjectMetadataSummaryPair> extensionObjects = NavCurrentThread.ResolveAppGroup().GetExtensionObjects();
			result = IsObjectIdInUse(extensionObjects, id, objectType, designerExtension);
		}
		else
		{
			try
			{
				NCLMetaApplicationObject metaApplicationObject = NavGlobal.NCLMetadata.GetMetaApplicationObject(objectType, id, -1, requireCompiled: false);
				if (metaApplicationObject != null)
				{
					result = true;
				}
			}
			catch (NavMetadataException)
			{
			}
		}
		return result;
	}

	internal static string GenerateExtensionObjectName(ObjectType extObjectType, int objectId)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}{1}", extObjectType, objectId);
	}

	internal static string GeneratePageCusomizationObjectName(bool isPersonalization = true)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}", isPersonalization ? "Personalization" : "Configuration");
	}

	private static bool IsObjectIdInUse(IEnumerable<SpecificNavAppObjectMetadataSummaryPair> knownObjects, int id, ObjectType objectType, NavDesignerExtension designerExtension)
	{
		designerExtension = designerExtension ?? NavCurrentThread.Session.CurrentDesignerExtension;
		bool flag = knownObjects.Any((SpecificNavAppObjectMetadataSummaryPair o) => o.Summary.ObjectType == objectType && o.Summary.ObjectId == id);
		if (designerExtension != null && objectType == ObjectType.PageExtension)
		{
			flag |= designerExtension.Objects.PageExtensions.ContainsKey(id);
		}
		return flag;
	}

	internal static string GetUnusedPageCustomizationId(this NavDesignerExtension designerExtension)
	{
		bool isPersonalization = designerExtension is NavPersonalizationDesignerExtension;
		string text = GeneratePageCusomizationObjectName(isPersonalization);
		int num = 0;
		do
		{
			num++;
		}
		while (designerExtension.IsPageCustomizationIdInUse(text + num));
		return text + num;
	}

	internal static bool IsPageCustomizationIdInUse(this NavDesignerExtension designerExtension, string name)
	{
		designerExtension = designerExtension ?? NavCurrentThread.Session.CurrentDesignerExtension;
		return designerExtension?.Objects.PageCustomizations.ContainsKey(name) ?? false;
	}

	internal static NavAppManifest WithSystemReferences(this NavAppManifest manifest)
	{
		IEnumerable<SymbolReferenceSpecification> systemSymbolReferences = GetSystemSymbolReferences();
		foreach (SymbolReferenceSpecification item in systemSymbolReferences)
		{
			if (item.Name.Equals("Application", StringComparison.Ordinal))
			{
				manifest.Application = item.Version;
			}
			else if (item.Name.Equals("System", StringComparison.Ordinal))
			{
				manifest.Platform = item.Version;
			}
			else
			{
				item.Name.Equals("Test", StringComparison.Ordinal);
			}
		}
		return manifest;
	}

	internal static IEnumerable<SymbolReferenceSpecification> GetSystemSymbolReferences()
	{
		List<SymbolReferenceSpecification> symbolReferences = new List<SymbolReferenceSpecification>();
		using (NavSqlConnectionScope connectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Browse))
		{
			AppMetadataStorage appMetadataStorage = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope);
			Result val = ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>>(appMetadataStorage.GetAllSymbolPackageMetadataRecords(), (Action<IEnumerable<NavAppMetadata>>)((IEnumerable<NavAppMetadata> symbolRecords) =>
			{
				symbolRecords.ForEach((NavAppMetadata s) =>
				{
					symbolReferences.Add(new SymbolReferenceSpecification(s.Publisher, s.Name, new Version(s.VersionMajor, s.VersionMinor, s.VersionBuild, s.VersionRevision)));
				});
			}));
			if (val.Failure)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000PN", Category.Extensions, Verbosity.Warning, "Error(s) loading symbol references from database. -- Details:{0}", DiagnosticParameter.SystemMetadata(string.Join(Environment.NewLine, val.Error.Message)));
			}
			if (symbolReferences.All((SymbolReferenceSpecification reference) => !reference.IsApplicationSymbolReference()))
			{
				symbolReferences.Add(SymbolReferenceSpecification.ApplicationReference(new Version()));
			}
		}
		return symbolReferences;
	}

	internal static AreaKind ControlContainerTypeToAreaKind(ControlContainerType controlContainerType)
	{
		return controlContainerType switch
		{
			ControlContainerType.ContentArea => AreaKind.Content, 
			ControlContainerType.FactBoxArea => AreaKind.FactBoxes, 
			ControlContainerType.RoleCenterArea => AreaKind.RoleCenter, 
			_ => AreaKind.None, 
		};
	}

	internal static ActionAreaKind ActionContainerTypeToAreaKind(ActionContainerType actionContainerType)
	{
		return actionContainerType switch
		{
			ActionContainerType.NewDocumentItems => ActionAreaKind.Creation, 
			ActionContainerType.HomeItems => ActionAreaKind.Embedding, 
			ActionContainerType.RelatedInformation => ActionAreaKind.Navigation, 
			ActionContainerType.ActionItems => ActionAreaKind.Processing, 
			ActionContainerType.Reports => ActionAreaKind.Reporting, 
			ActionContainerType.ActivityButtons => ActionAreaKind.Sections, 
			_ => ActionAreaKind.None, 
		};
	}

	internal static IDictionary<string, byte[]> MakePackageFilesDictionary(NavAppPackageReader packageReader, NavAppManifest manifest, bool includeEmittedfiles = false)
	{
		ImmutableDictionary<string, byte[]> immutableDictionary = packageReader.MakeDictionaryForPackageRecompilation();
		List<NavAppMediaEntry> source = packageReader.ReadMedia().ToList();
		if (!string.IsNullOrEmpty(manifest.AppLogo))
		{
			NavAppMediaEntry mediaEntry = source.SingleOrDefault((NavAppMediaEntry m) => m.IsLogo && m.FileName == Path.GetFileName(manifest.AppLogo));
			immutableDictionary = AddMediaItemToDictionary(immutableDictionary, packageReader, mediaEntry, manifest.AppLogo);
		}
		foreach (string screenshot in manifest.AppScreenshots)
		{
			NavAppMediaEntry mediaEntry2 = source.SingleOrDefault((NavAppMediaEntry m) => !m.IsLogo && m.FileName == Path.GetFileName(screenshot));
			immutableDictionary = AddMediaItemToDictionary(immutableDictionary, packageReader, mediaEntry2, screenshot);
		}
		if (includeEmittedfiles)
		{
			foreach (string item in packageReader.ReadEmittedFilePaths())
			{
				using Stream stream = packageReader.ReadFile(item);
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				immutableDictionary = immutableDictionary.Add(RemoveTopFolder(item), array);
			}
		}
		return immutableDictionary;
	}

	internal static void SetDesignerExtensionReferencesInManifest(NavAppManifest manifest)
	{
		ImmutableArray<SymbolReferenceSpecification> referenceSpecifications = GetPersonalizationReferences(manifest).ToImmutableArray();
		manifest.UpdateDependencies(referenceSpecifications);
	}

	internal static IEnumerable<SymbolReferenceSpecification> GetSymbolReferenceSpecifications(IEnumerable<NavAppDependency> dependencies)
	{
		return dependencies.Select((NavAppDependency x) => new SymbolReferenceSpecification(x.Publisher, x.Name, x.MinVersion, exact: false, x.AppId));
	}

	internal static IEnumerable<SymbolReferenceSpecification> GetSymbolReferenceSpecifications(IEnumerable<IModuleSymbol> modules)
	{
		return modules.Select((IModuleSymbol x) => new SymbolReferenceSpecification(x.PublisherName, x.Name, x.Version, exact: false, x.AppId));
	}

	private static string RemoveTopFolder(string filename)
	{
		int startIndex = 0;
		if (filename.StartsWith("/", StringComparison.OrdinalIgnoreCase) && filename.Length >= 2)
		{
			startIndex = 1;
		}
		int num = filename.IndexOf("/", startIndex, StringComparison.OrdinalIgnoreCase);
		if (num < 0)
		{
			return filename;
		}
		return filename.Substring(num);
	}

	private static ImmutableDictionary<string, byte[]> AddMediaItemToDictionary(ImmutableDictionary<string, byte[]> dictionary, NavAppPackageReader packageReader, NavAppMediaEntry mediaEntry, string originalPath)
	{
		if (mediaEntry == null || string.IsNullOrWhiteSpace(originalPath))
		{
			return dictionary;
		}
		using Stream stream = packageReader.ReadMediaFile(mediaEntry);
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		if (!dictionary.ContainsKey(originalPath))
		{
			dictionary = dictionary.Add(originalPath, array);
		}
		return dictionary;
	}

	internal static bool ControlExistsOnPage(this MasterPage masterPage, string controlName)
	{
		return masterPage.GetControlContext(controlName, throwIfNotFound: false) != null;
	}

	internal static bool ActionExistsOnPage(this MasterPage masterPage, string actionName)
	{
		return masterPage.GetActionContext(actionName, throwIfNotFound: false) != null;
	}

	internal static bool ViewExistsOnPage(this MasterPage masterPage, string viewName)
	{
		return masterPage.GetActionViewContext(viewName, throwIfNotFound: false) != null;
	}

	internal static string GetControlALIdentifier(this MasterPage masterPage, string controlName, bool throwIfNotFound = true)
	{
		return masterPage.GetControlContext(controlName, throwIfNotFound)?.GetControlALIdentifier();
	}

	internal static string GetActionALIdentifier(this MasterPage masterPage, string actionName, bool throwIfNotFound = true)
	{
		return masterPage.GetActionContext(actionName, throwIfNotFound)?.GetActionALIdentifier();
	}

	internal static string GetActionViewALIdentifier(this MasterPage masterPage, string actionName, bool throwIfNotFound = true)
	{
		return masterPage.GetActionViewContext(actionName, throwIfNotFound)?.GetActionALIdentifier();
	}

	internal static NavDesignerControlContext GetControlContext(this MasterPage masterPage, string controlName, bool throwIfNotFound = true)
	{
		if (string.IsNullOrEmpty(controlName))
		{
			return null;
		}
		NavDesignerControlContext controlContext = masterPage.GetControlContext((ControlBaseDefinition cbd) => string.Equals(cbd.Name, controlName));
		if (controlContext != null)
		{
			return controlContext;
		}
		if (throwIfNotFound)
		{
			throw new NavNCLControlMetadataNotFoundException(CultureInfo.CurrentCulture, masterPage.ID, controlName);
		}
		return null;
	}

	internal static NavDesignerControlContext GetControlContext<T>(this MasterPage masterPage, Predicate<T> match) where T : ControlBaseDefinition
	{
		NavDesignerControlContext controlContextForArea = GetControlContextForArea(masterPage.ContentArea, ControlContainerType.ContentArea, masterPage.Name, match);
		if (controlContextForArea != null)
		{
			return controlContextForArea;
		}
		controlContextForArea = GetControlContextForArea(masterPage.InfopartsArea, ControlContainerType.FactBoxArea, masterPage.Name, match);
		if (controlContextForArea != null)
		{
			return controlContextForArea;
		}
		return GetControlContextForArea(masterPage.ContentArea, ControlContainerType.RoleCenterArea, masterPage.Name, match);
	}

	internal static NavDesignerActionContext GetActionContext(this MasterPage masterPage, string actionName, bool throwIfNotFound = true)
	{
		if (string.IsNullOrEmpty(actionName))
		{
			return null;
		}
		NavDesignerActionContext actionContext = masterPage.GetActionContext((ActionBaseDefinition abd) => string.Equals(abd.Name, actionName));
		if (actionContext != null)
		{
			return actionContext;
		}
		if (throwIfNotFound)
		{
			throw new NavNCLActionMetadataNotFoundException(CultureInfo.CurrentCulture, masterPage.ID, actionName);
		}
		return null;
	}

	internal static NavDesignerActionContext GetActionViewContext(this MasterPage masterPage, string actionName, bool throwIfNotFound = true)
	{
		if (string.IsNullOrEmpty(actionName))
		{
			return null;
		}
		NavDesignerActionContext actionContextForArea = GetActionContextForArea(ActionContainerType.ViewActions, masterPage, (ActionBaseDefinition abd) => string.Equals(abd.Name, actionName));
		if (actionContextForArea != null)
		{
			return actionContextForArea;
		}
		if (throwIfNotFound)
		{
			throw new NavNCLViewMetadataNotFoundException(CultureInfo.CurrentCulture, masterPage.ID, actionName);
		}
		return null;
	}

	internal static NavDesignerActionContext GetActionContext<T>(this MasterPage masterPage, Predicate<T> match) where T : ActionBaseDefinition
	{
		NavDesignerActionContext actionContextForArea = GetActionContextForArea(ActionContainerType.NewDocumentItems, masterPage, match);
		if (actionContextForArea != null)
		{
			return actionContextForArea;
		}
		actionContextForArea = GetActionContextForArea(ActionContainerType.ActionItems, masterPage, match);
		if (actionContextForArea != null)
		{
			return actionContextForArea;
		}
		actionContextForArea = GetActionContextForArea(ActionContainerType.Reports, masterPage, match);
		if (actionContextForArea != null)
		{
			return actionContextForArea;
		}
		actionContextForArea = GetActionContextForArea(ActionContainerType.RelatedInformation, masterPage, match);
		if (actionContextForArea != null)
		{
			return actionContextForArea;
		}
		return null;
	}

	private static NavDesignerActionContext GetActionContextForArea<T>(ActionContainerType actionContainerType, MasterPage masterPage, Predicate<T> match) where T : ActionBaseDefinition
	{
		return FindActionContextInActionList(null, GetActionAreaFromMasterPage(masterPage, actionContainerType)?.Actions, actionContainerType, match, masterPage.Name);
	}

	private static ActionBaseDefinition GetActionAreaFromMasterPage(MasterPage masterPage, ActionContainerType actionContainerType)
	{
		List<ActionBaseDefinition> list = masterPage.CommandBar?.Actions;
		if (list == null || list.Count == 0)
		{
			return null;
		}
		switch (actionContainerType)
		{
		case ActionContainerType.NewDocumentItems:
			return list.OfType<NavigationGroupDefinition>().FirstOrDefault((NavigationGroupDefinition x) => x.NavigationGroup == NavigationGroupType.Actions)?.Actions.FirstOrDefault((ActionBaseDefinition x) => IsNewDocumentItemArea(x));
		case ActionContainerType.ActionItems:
			return list.OfType<NavigationGroupDefinition>().FirstOrDefault((NavigationGroupDefinition x) => x.NavigationGroup == NavigationGroupType.Actions);
		case ActionContainerType.Reports:
			return list.OfType<NavigationGroupDefinition>().FirstOrDefault((NavigationGroupDefinition x) => x.NavigationGroup == NavigationGroupType.Reports);
		case ActionContainerType.RelatedInformation:
			return list.OfType<NavigationGroupDefinition>().FirstOrDefault((NavigationGroupDefinition x) => x.NavigationGroup == NavigationGroupType.RelatedInformation);
		case ActionContainerType.ViewActions:
			return list.OfType<NavigationGroupDefinition>().FirstOrDefault((NavigationGroupDefinition x) => x.NavigationGroup == NavigationGroupType.Views);
		case ActionContainerType.HomeItems:
		case ActionContainerType.ActivityButtons:
		case ActionContainerType.Departments:
			return null;
		default:
			return null;
		}
	}

	private static bool IsNewDocumentItemArea(ActionBaseDefinition action)
	{
		if (action.ControlGUID == "{1AF878F7-1EB4-4B40-A6D5-55E87F26029B}")
		{
			return action.Name == "Control_New_Document";
		}
		return false;
	}

	private static NavDesignerControlContext GetControlContextForArea<T>(AreaBaseDefinition areaBase, ControlContainerType controlContainerType, string masterPageName, Predicate<T> match) where T : ControlBaseDefinition
	{
		return FindControlContextInControlList(null, areaBase.Controls.OfType<ControlContainerPlaceHolder>().SingleOrDefault((ControlContainerPlaceHolder control) => control.ContainerType == controlContainerType)?.Controls, controlContainerType, match, masterPageName);
	}

	private static NavDesignerActionContext FindActionContextInActionList<T>(ActionGroupBaseDefinition groupDefinition, List<ActionBaseDefinition> groupActionList, ActionContainerType actionContainerType, Predicate<T> match, string PageName) where T : ActionBaseDefinition
	{
		if (groupActionList == null)
		{
			return null;
		}
		ActionBaseDefinition previousAction = null;
		using (List<ActionBaseDefinition>.Enumerator enumerator = groupActionList.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T val = enumerator.Current as T;
				if (val != null && match(val))
				{
					ActionBaseDefinition nextAction = (enumerator.MoveNext() ? enumerator.Current : null);
					return new NavDesignerActionContext(val, actionContainerType, groupDefinition, previousAction, nextAction, PageName);
				}
				if (enumerator.Current is ActionGroupBaseDefinition actionGroupBaseDefinition)
				{
					NavDesignerActionContext navDesignerActionContext = FindActionContextInActionList(actionGroupBaseDefinition, actionGroupBaseDefinition.Actions, actionContainerType, match, PageName);
					if (navDesignerActionContext != null)
					{
						return navDesignerActionContext;
					}
				}
				previousAction = val;
			}
		}
		return null;
	}

	private static NavDesignerControlContext FindControlContextInControlList<T>(ControlGroupBaseDefinition groupDefinition, List<ControlBaseDefinition> groupControlList, ControlContainerType controlContainerType, Predicate<T> match, string PageName) where T : ControlBaseDefinition
	{
		if (groupControlList == null)
		{
			return null;
		}
		ControlBaseDefinition previousControl = null;
		using (List<ControlBaseDefinition>.Enumerator enumerator = groupControlList.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T val = enumerator.Current as T;
				if (val != null && match(val))
				{
					ControlBaseDefinition nextControl = (enumerator.MoveNext() ? enumerator.Current : null);
					return new NavDesignerControlContext(val, controlContainerType, groupDefinition, previousControl, nextControl, PageName);
				}
				if (enumerator.Current is ControlGroupBaseDefinition controlGroupBaseDefinition)
				{
					NavDesignerControlContext navDesignerControlContext = FindControlContextInControlList(controlGroupBaseDefinition, controlGroupBaseDefinition.Controls, controlContainerType, match, PageName);
					if (navDesignerControlContext != null)
					{
						return navDesignerControlContext;
					}
				}
				previousControl = val;
			}
		}
		return null;
	}

	public static IEnumerable<ObjectSyntax> GetNavObjectsSyntax(this NavDesignerExtension extension)
	{
		return from o in extension.Objects.AllObjects
			select o.Syntax into s
			where s != null
			select s;
	}

	public static ObjectType GetApplicationObjectTypeFromSyntaxKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.TableObject => ObjectType.Table, 
			SyntaxKind.TableExtensionObject => ObjectType.TableExtension, 
			SyntaxKind.PageObject => ObjectType.Page, 
			SyntaxKind.PageExtensionObject => ObjectType.PageExtension, 
			SyntaxKind.CodeunitObject => ObjectType.CodeUnit, 
			SyntaxKind.ReportObject => ObjectType.Report, 
			SyntaxKind.QueryObject => ObjectType.Query, 
			SyntaxKind.XmlPortObject => ObjectType.XmlPort, 
			SyntaxKind.EnumType => ObjectType.Enum, 
			SyntaxKind.EnumExtensionType => ObjectType.EnumExtension, 
			_ => throw new NavNCLArgumentException(kind, "GetApplicationObjectTypeFromSyntaxKind", 1), 
		};
	}

	public static void UpdateDesignerPageCustomizationsMetadata(this NavPersonalizationDesignerExtension extension)
	{
		UpdateDesignerPageCustomizationsMetadataImplementation(extension);
	}

	public static void UpdateDesignerPageCustomizationsMetadata(this NavConfigurationDesignerExtension extension)
	{
		UpdateDesignerPageCustomizationsMetadataImplementation(extension);
	}

	private static void UpdateDesignerPageCustomizationsMetadataImplementation(NavDesignerExtension extension)
	{
		NavPersonalizationDesignerExtension navPersonalizationDesignerExtension = extension as NavPersonalizationDesignerExtension;
		NavConfigurationDesignerExtension navConfigurationDesignerExtension = extension as NavConfigurationDesignerExtension;
		if (navPersonalizationDesignerExtension == null && navConfigurationDesignerExtension == null)
		{
			return;
		}
		foreach (NavAppObjectMetadata item in extension.ObjectMetadata.Where((NavAppObjectMetadata o) => o.ObjectType == ObjectType.PageExtension))
		{
			string pageName = ((item.ObjectSubtype == null) ? (NavDesignerObjectFactory.CreateFromSource(item.UserALCode).First().Syntax as PageCustomizationSyntax).BaseObject.Identifier.ToString().UnquoteIdentifier() : NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Page, int.Parse(item.ObjectSubtype, CultureInfo.InvariantCulture))));
			NavDesignerPageCustomization navDesignerPageCustomization = extension.Objects.PageCustomizations.Values.SingleOrDefault((NavDesignerPageCustomization p) => p.BasePageName.Equals(pageName));
			if (item.ObjectSubtype == null)
			{
				extension.Objects.PageCustomizations.Remove(navDesignerPageCustomization.ObjectName);
				if (navPersonalizationDesignerExtension != null)
				{
					navPersonalizationDesignerExtension.PersonalizationProfile.RemovePageCustomization(navDesignerPageCustomization.ObjectName);
				}
				else
				{
					navConfigurationDesignerExtension.ConfigurationProfile.RemovePageCustomization(navDesignerPageCustomization.ObjectName);
				}
			}
			else
			{
				navDesignerPageCustomization.PageCustomizationMetadata = item.Metadata;
			}
		}
		NavCurrentThread.Session.MetadataProvider.UpdateMetadataVersion();
	}

	internal static PropertyListSyntax GetDefaultPropertyList()
	{
		CommaSeparatedPropertyValueSyntax propertyValue = SyntaxFactory.CommaSeparatedPropertyValue().AddValues(SyntaxFactory.IdentifierName("All"));
		PropertySyntax propertySyntax = SyntaxFactory.Property(PropertyKind.ApplicationArea, propertyValue);
		return SyntaxFactory.PropertyList().AddProperties(propertySyntax);
	}

	public static string GetControlALIdentifier(NamedElementDefinition control, string pageIdentifier)
	{
		if (control == null)
		{
			return null;
		}
		if (control.CachedALIdentifier == null)
		{
			control.CachedALIdentifier = CompilationForALIdentification.ResolvePageControlName(pageIdentifier, control.ID);
		}
		if (string.IsNullOrEmpty(control.CachedALIdentifier))
		{
			control.CachedALIdentifier = control.Name;
		}
		return control.CachedALIdentifier;
	}

	public static string GetActionALIdentifier(NamedElementDefinition action, string pageIdentifier)
	{
		if (action == null)
		{
			return null;
		}
		if (action.CachedALIdentifier == null)
		{
			action.CachedALIdentifier = CompilationForALIdentification.ResolvePageActionName(pageIdentifier, action.ID);
		}
		if (string.IsNullOrEmpty(action.CachedALIdentifier))
		{
			action.CachedALIdentifier = action.Name;
		}
		return action.CachedALIdentifier;
	}

	internal static bool IsPropertyAllowedForPage(string propertyName, out PropertyKind propertyKind)
	{
		if (propertyName != null && propertyName == "PromotedActionCategories")
		{
			propertyKind = PropertyKind.PromotedActionCategories;
			return true;
		}
		propertyKind = PropertyKind.None;
		return false;
	}

	internal static bool IsPropertyAllowedForAction(string propertyName, out PropertyKind propertyKind)
	{
		switch (propertyName)
		{
		case "Visible":
			propertyKind = PropertyKind.Visible;
			return true;
		case "Caption":
			propertyKind = PropertyKind.Caption;
			return true;
		case "Promoted":
			propertyKind = PropertyKind.Promoted;
			return true;
		case "PromotedIsBig":
			propertyKind = PropertyKind.PromotedIsBig;
			return true;
		case "PromotedOnly":
			propertyKind = PropertyKind.PromotedOnly;
			return true;
		case "PromotedCategory":
			propertyKind = PropertyKind.PromotedCategory;
			return true;
		default:
			propertyKind = PropertyKind.None;
			return false;
		}
	}

	internal static bool IsPropertyAllowedForField(string propertyName, out PropertyKind propertyKind)
	{
		switch (propertyName)
		{
		case "Visible":
			propertyKind = PropertyKind.Visible;
			return true;
		case "Caption":
			propertyKind = PropertyKind.Caption;
			return true;
		case "FreezeColumnID":
			propertyKind = PropertyKind.FreezeColumn;
			return true;
		case "Width":
			propertyKind = PropertyKind.Width;
			return true;
		case "QuickEntry":
			propertyKind = PropertyKind.QuickEntry;
			return true;
		case "Importance":
			propertyKind = PropertyKind.Importance;
			return true;
		default:
			propertyKind = PropertyKind.None;
			return false;
		}
	}

	internal static bool IsPropertyAllowedForView(string propertyName, out PropertyKind propertyKind)
	{
		switch (propertyName)
		{
		case "Visible":
			propertyKind = PropertyKind.Visible;
			return true;
		case "Caption":
			propertyKind = PropertyKind.Caption;
			return true;
		case "Filters":
			propertyKind = PropertyKind.Filters;
			return true;
		case "OrderBy":
			propertyKind = PropertyKind.OrderBy;
			return true;
		default:
			propertyKind = PropertyKind.None;
			return false;
		}
	}
}
