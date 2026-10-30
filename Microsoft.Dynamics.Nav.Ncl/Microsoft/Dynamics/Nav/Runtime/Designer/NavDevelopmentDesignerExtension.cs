using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal sealed class NavDevelopmentDesignerExtension : NavDesignerExtension
{
	private readonly INavDesignerStorage extensionStorage;

	public NavDevelopmentDesignerExtension(INavDesignerStorage storage, INavDesignerCompiler compiler, AppId extensionAppId = null, Version appVersion = null)
		: base(compiler)
	{
		extensionStorage = storage;
		if ((WrappedComparable<Guid>)(object)extensionAppId != (WrappedComparable<Guid>)null)
		{
			PackageType = storage.GetPackageType(extensionAppId, appVersion);
			Manifest = storage.GetManifest(extensionAppId, appVersion);
			Objects = storage.GetObjects(extensionAppId, appVersion);
			PackageFiles = storage.GetPackageFiles(extensionAppId, appVersion);
		}
		else
		{
			PackageType = NavAppPackageType.Designer;
			SetExtensionProperties();
		}
	}

	public void SaveDesignerExtensionAsFullExtension(string name, string publisher)
	{
		if (PackageType == NavAppPackageType.Designer)
		{
			Manifest.AppName = name;
			Manifest.AppPublisher = publisher;
			PackageType = NavAppPackageType.Extension;
		}
		Save();
	}

	public override void Save()
	{
		try
		{
			SaveExtensionData();
		}
		catch (NavDesignerCompilerException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001QS", Category.Extensions, Verbosity.Warning, "Error(s) saving designer extension -- Details:{0}", DiagnosticParameter.SystemMetadata(string.Join(Environment.NewLine, ex.CompileMessages)));
			throw;
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000OQ", Category.Extensions, exception);
			throw;
		}
	}

	protected override void ApplyRuntimeDeltasToPageDefinition(ApplicationObjectId objectId, PageDefinition pageDefinition, string viewName)
	{
		IEnumerable<NavAppObjectMetadataRuntimeDeltas> deltas = GetPageExtensionObjectMetadataForObject(objectId, out var viewsToFlag) ?? Enumerable.Empty<NavAppObjectMetadataRuntimeDeltas>();
		NavDesignerExtension.ApplyDeltas(objectId, pageDefinition, deltas);
		MetadataProvider.SetFlagOnViewsInUserScope(pageDefinition, viewsToFlag);
	}

	internal override void SaveExtensionData()
	{
		SaveDesignerData();
	}

	public override INavDesignerPageOperations GetPageOperationInterfaceForPage(int pageId)
	{
		NavDesignerObject designerPageObject = GetDesignerPageObject(pageId, createIfNotFound: true);
		if (designerPageObject is NavDesignerPageExtension navDesignerPageExtension)
		{
			return navDesignerPageExtension.Modifications;
		}
		return designerPageObject as NavDesignerPage;
	}

	public override NavDesignerObject GetDesignerPageObject(int pageId, bool createIfNotFound)
	{
		string pageName = NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Page, pageId));
		if (string.IsNullOrEmpty(pageName))
		{
			throw new NavMetadataNotFoundException(ObjectType.Page, pageId);
		}
		if (Objects.Pages.ContainsKey(pageId))
		{
			return Objects.Pages[pageId];
		}
		NavDesignerPageExtension navDesignerPageExtension = Objects.PageExtensions.Values.SingleOrDefault((NavDesignerPageExtension p) => p.BasePageName.Equals(pageName) || p.BasePageName.Equals(pageId.ToString(CultureInfo.InvariantCulture)));
		if ((navDesignerPageExtension == null) & createIfNotFound)
		{
			int unusedObjectId = NavDesignerUtil.GetUnusedObjectId(ObjectType.PageExtension, Manifest.IdSpaces);
			string objectName = NavDesignerUtil.GenerateExtensionObjectName(ObjectType.PageExtension, unusedObjectId);
			navDesignerPageExtension = NavDesignerPageExtension.Create(unusedObjectId, objectName, pageName);
			Objects.PageExtensions.Add(navDesignerPageExtension.ObjectId.ObjectNumber, navDesignerPageExtension);
		}
		return navDesignerPageExtension;
	}

	public override NavDesignerPageModifications GetPageModifications(int basePageId)
	{
		string basePageName = NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Page, basePageId));
		return Objects.PageExtensions.Values.FirstOrDefault((NavDesignerPageExtension pc) => pc.BasePageName.Equals(basePageName, StringComparison.OrdinalIgnoreCase) || pc.BasePageName.Equals(basePageId.ToString(CultureInfo.InvariantCulture)))?.Modifications;
	}

	private void SaveDesignerData()
	{
		Stream stream = CreatePackage();
		stream.Position = 0L;
		SaveFullExtension(AppId.op_Implicit(Manifest.AppId), stream, PackageType);
	}

	private Stream CreatePackage()
	{
		bool flag = PackageType == NavAppPackageType.Designer;
		if (flag)
		{
			NavDesignerUtil.SetDesignerExtensionReferencesInManifest(Manifest);
		}
		Stream result = Compiler.BuildDesignerPackage(this.GetNavObjectsSyntax(), Manifest, new MemoryFileSystem(PackageFiles ?? new Dictionary<string, byte[]>()), flag);
		ClearPageExtensionDeltasFromInstalledExtensionsCache();
		return result;
	}

	private void SaveFullExtension(AppId appId, Stream packageStream, NavAppPackageType packageType)
	{
		Result val = extensionStorage.SaveData(appId, packageStream, packageType);
		if (val.Failure)
		{
			throw new NavDesignerExtensionStorageException(val.Error.Message);
		}
	}
}
