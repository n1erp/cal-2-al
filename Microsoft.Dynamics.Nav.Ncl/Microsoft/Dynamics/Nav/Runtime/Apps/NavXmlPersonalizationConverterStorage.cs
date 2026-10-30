using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Designer;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavXmlPersonalizationConverterStorage : INavXmlConverterStorage
{
	private readonly DatabaseNavPageCustomizationRepository deltaStorage;

	private readonly Guid userId;

	public NavXmlPersonalizationConverterStorage(DatabaseNavPageCustomizationRepository deltaStorage, NavXmlPersonalizationConversionOptions options)
	{
		this.deltaStorage = deltaStorage;
		userId = options.UserSid;
	}

	public void SaveConvertedDeltas(IEnumerable<NavXmlDeltaPageConversionResult> convertedPages)
	{
		IEnumerable<ObjectSyntax> enumerable = from x in convertedPages
			where !x.HadErrors
			select x.ConvertedObject;
		if (!enumerable.Any())
		{
			return;
		}
		ObjectSyntax profileSyntax = GetProfileSyntax(enumerable);
		if (profileSyntax != null)
		{
			enumerable = enumerable.Concat(new ObjectSyntax[1] { profileSyntax });
		}
		ObjectSyntax[] objectSyntaxList = enumerable?.ToArray() ?? new ObjectSyntax[0];
		NavDesignerCompiler navDesignerCompiler = new NavDesignerCompiler();
		CompilationOptions compilationOptions = new CompilationOptions(concurrentBuild: true, 4, ReportDiagnostic.Default, reportSuppressedDiagnostics: false, null, CompilationTarget.Personalization, allowExtensionAndTargetInSameModule: false, readonlyErrorsAsWarnings: true, CompilerFeatures.None, showMyCode: false, null, CompilationGenerationOptions.Code);
		IEnumerable<SymbolReferenceSpecification> personalizationReferences = NavDesignerUtil.GetPersonalizationReferences();
		NavAppPackageMetadataOutputter outputter = new NavAppPersonalizationMetadataOutputter();
		IEnumerable<NavAppObjectMetadata> enumerable2 = navDesignerCompiler.CompilePersonalizationObjects(objectSyntaxList, personalizationReferences, compilationOptions, outputter);
		foreach (NavAppObjectMetadata item in enumerable2)
		{
			deltaStorage.SavePersonalization(userId, int.Parse(item.ObjectSubtype, CultureInfo.InvariantCulture), item.Metadata, item.UserALCode);
		}
	}

	private static ObjectSyntax GetProfileSyntax(IEnumerable<ObjectSyntax> convertedObjects)
	{
		ObjectSyntax result = null;
		if (convertedObjects.Any())
		{
			NavDesignerProfile navDesignerProfile = NavDesignerProfile.Create("TemporaryDesignerProfile", "TemporaryDesignerProfile", NavConfigurationProfile.GetProfileRoleCenterId(NavCurrentThread.Session.ProfileKey).ToString(CultureInfo.InvariantCulture));
			foreach (ObjectSyntax convertedObject in convertedObjects)
			{
				navDesignerProfile.AddPageCustomization(convertedObject.Name.ToString());
			}
			result = navDesignerProfile.Syntax;
		}
		return result;
	}
}
