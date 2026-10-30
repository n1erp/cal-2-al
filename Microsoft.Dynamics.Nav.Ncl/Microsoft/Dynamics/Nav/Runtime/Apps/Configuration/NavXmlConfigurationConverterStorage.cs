using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Designer;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.Configuration;

internal class NavXmlConfigurationConverterStorage : INavXmlConverterStorage
{
	private readonly DatabaseNavPageCustomizationRepository deltaStorage;

	private readonly NavXmlConfigurationConversionOptions options;

	private readonly bool downloadResult;

	private readonly ZipArchive archive;

	public NavXmlConfigurationConverterStorage(DatabaseNavPageCustomizationRepository deltaStorage, NavXmlConfigurationConversionOptions options, bool downloadResult, ZipArchive archive)
	{
		this.deltaStorage = deltaStorage;
		this.options = options;
		this.downloadResult = downloadResult;
		this.archive = archive;
	}

	public void SaveConvertedDeltas(IEnumerable<NavXmlDeltaPageConversionResult> convertedPages)
	{
		IEnumerable<ObjectSyntax> enumerable = from y in convertedPages
			where !y.HadErrors
			select y.ConvertedObject;
		if (!enumerable.Any())
		{
			return;
		}
		(int, string, ProfileScope, Guid) profileInformation = NavConfigurationProfile.GetProfileInformation(options.SourceProfileKey);
		ObjectSyntax profileSyntax = GetProfileSyntax(enumerable, profileInformation);
		if (profileSyntax != null)
		{
			enumerable = enumerable.Concat(new ObjectSyntax[1] { profileSyntax });
		}
		ObjectSyntax[] objectSyntaxList = enumerable?.ToArray() ?? new ObjectSyntax[0];
		NavDesignerCompiler navDesignerCompiler = new NavDesignerCompiler();
		CompilationOptions compilationOptions = new CompilationOptions(concurrentBuild: true, 4, ReportDiagnostic.Default, reportSuppressedDiagnostics: false, null, Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Personalization, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings: true, CompilerFeatures.None, showMyCode: false, null, CompilationGenerationOptions.Code);
		IEnumerable<SymbolReferenceSpecification> personalizationReferences = NavDesignerUtil.GetPersonalizationReferences();
		NavAppPackageMetadataOutputter outputter = new NavAppPersonalizationMetadataOutputter();
		IEnumerable<NavAppObjectMetadata> enumerable2 = navDesignerCompiler.CompilePersonalizationObjects(objectSyntaxList, personalizationReferences, compilationOptions, outputter);
		if (downloadResult)
		{
			SaveConversionInArchive(profileSyntax, enumerable2);
			return;
		}
		foreach (NavAppObjectMetadata item in enumerable2)
		{
			deltaStorage.SaveConfiguration(options.DestinationProfileKey, int.Parse(item.ObjectSubtype, CultureInfo.InvariantCulture), item.Metadata, item.UserALCode, ProfilePageMetadataOwner.Tenant);
		}
		InsertTenantProfile(options.DestinationProfileKey, profileInformation.Item1, profileInformation.Item2, NavCurrentThread.Session);
	}

	private ObjectSyntax GetProfileSyntax(IEnumerable<ObjectSyntax> convertedObjects, (int RoleCenterId, string Description, ProfileScope Scope, Guid AppId) profileInfo)
	{
		NavDesignerProfile navDesignerProfile = NavDesignerProfile.Create(options.SourceProfileKey.ProfileId, profileInfo.Description, profileInfo.RoleCenterId.ToString(CultureInfo.InvariantCulture));
		foreach (ObjectSyntax convertedObject in convertedObjects)
		{
			navDesignerProfile.AddPageCustomization(convertedObject.Name.ToString());
		}
		return navDesignerProfile.Syntax;
	}

	private static void InsertTenantProfile(NavProfileKey profileKey, int roleCenterId, string description, NavSession session)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000177);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(2);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(1);
		navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
		navRecord.SetFilter(fieldByNo2, "%1", new NavText(description));
		navRecord.SetFilter(fieldByNo3, "%1", profileKey.AppId);
		session.BeginTransaction();
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			navRecord.ALModify(DataError.ThrowError, runApplicationTrigger: true);
		}
		else
		{
			navRecord.SetFieldValue(2, new NavOemCode(2048, profileKey.ProfileId));
			navRecord.SetFieldValue(3, new NavText(int.MaxValue, description));
			navRecord.SetFieldValue(4, NavInteger.Create(roleCenterId));
			navRecord.SetFieldValue(5, NavBoolean.Create(value: false));
			navRecord.SetFieldValue(1, new NavGuid(profileKey.AppId.Value));
			navRecord.ALInsert(runApplicationTrigger: true);
		}
		session.EndTransaction(commit: true);
	}

	private void SaveConversionInArchive(ObjectSyntax profileSyntax, IEnumerable<NavAppObjectMetadata> compiledObjects)
	{
		string nameStringValue = profileSyntax.GetNameStringValue();
		CreateFolderInsideArchive(nameStringValue);
		ZipArchiveEntry zipArchiveEntry = CreateArchiveEntry(nameStringValue, nameStringValue + ".Profile.al");
		using (Stream stream = zipArchiveEntry.Open())
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(profileSyntax.NormalizeWhiteSpace().ToString());
		}
		foreach (NavAppObjectMetadata compiledObject in compiledObjects)
		{
			zipArchiveEntry = CreateArchiveEntry(nameStringValue, compiledObject.ObjectName + ".PageCustomization.al");
			using Stream stream2 = zipArchiveEntry.Open();
			using StreamWriter streamWriter2 = new StreamWriter(stream2);
			streamWriter2.Write(compiledObject.UserALCode);
		}
	}

	private ZipArchiveEntry CreateArchiveEntry(string basePath, string fileName)
	{
		basePath = NavDesignerALFunctions.SanitizeDesignerPathName(basePath);
		fileName = NavDesignerALFunctions.SanitizeDesignerPathName(fileName);
		string entryName = Path.Combine(basePath, fileName);
		return archive.CreateEntry(entryName);
	}

	private void CreateFolderInsideArchive(string filePath)
	{
		archive.CreateEntry(NavDesignerALFunctions.SanitizeDesignerPathName(filePath) + "/");
	}
}
