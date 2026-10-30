using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal abstract class NavXmlAbstractConverter : INavMetadataConverter
{
	protected INavXmlConverterMetadataProvider metadataProvider;

	protected INavXmlDeltaConverter deltaConverter;

	protected readonly List<NavXmlDeltaPageConversionResult> convertedPages;

	protected abstract ConversionType ConversionType { get; }

	protected abstract string ObjectPrefix { get; }

	public NavXmlAbstractConverter(INavXmlConverterMetadataProvider metadataProvider, INavXmlDeltaConverter deltaConverter)
	{
		this.metadataProvider = metadataProvider;
		this.deltaConverter = deltaConverter;
		convertedPages = new List<NavXmlDeltaPageConversionResult>();
	}

	public abstract NavXmlDeltaConversionResult Convert(IEnumerable<IMetadataRecord> records);

	protected void ConvertInternal(IEnumerable<IGrouping<int, IMetadataRecord>> deltasByPageList, MasterNavigation navigationFrame)
	{
		foreach (IGrouping<int, IMetadataRecord> deltasByPage in deltasByPageList)
		{
			List<NavXmlDeltaConversionDiagnostics> pageDiagnostics = new List<NavXmlDeltaConversionDiagnostics>();
			ObjectSyntax convertedPage = null;
			HandleGenericErrors(pageDiagnostics, () =>
			{
				PageDefinition pageDefinition = metadataProvider.GetPageDefinition(deltasByPage.Key);
				MasterPage masterPage = metadataProvider.GetMasterPage(pageDefinition.Properties.PageType);
				MasterPage masterPageWithoutConfiguration = metadataProvider.GetMasterPageWithoutConfiguration(pageDefinition.ID);
				IMetadataRecord metadataRecord = ((deltasByPage.Count() > 1) ? SelectConfiguration(deltasByPage.Key, deltasByPage, navigationFrame) : deltasByPage.ElementAt(0));
				ConverterResult converterResult = deltaConverter.Convert(masterPage, pageDefinition, masterPageWithoutConfiguration, metadataRecord.PageMetadataDelta);
				pageDiagnostics.AddRange(converterResult.ConversionWarnings);
				convertedPage = GetPageCustomization(converterResult.SyntaxNodes, pageDefinition.Name, pageDefinition.ID);
			});
			convertedPages.Add(new NavXmlDeltaPageConversionResult(deltasByPage.Key, convertedPage, pageDiagnostics));
		}
	}

	private static IMetadataRecord SelectConfiguration(int targetPageId, IEnumerable<IMetadataRecord> recordsForPage, MasterNavigation navigation)
	{
		recordsForPage = recordsForPage.ToList();
		string pageIdString = targetPageId.ToString(CultureInfo.InvariantCulture);
		IMetadataRecord metadataRecord = recordsForPage.FirstOrDefault((IMetadataRecord r) => string.Equals(r.PersonalizationId, pageIdString, StringComparison.OrdinalIgnoreCase));
		if (metadataRecord != null)
		{
			return metadataRecord;
		}
		if (navigation.Find((ElementDefinition e) => e is ActionDefinition definition && SelectTargetAction(definition, targetPageId)) is ActionDefinition actionDefinition)
		{
			if (Guid.TryParse(actionDefinition.ControlGUID, out var controlGuid))
			{
				IMetadataRecord metadataRecord2 = recordsForPage.FirstOrDefault((IMetadataRecord r) => Guid.TryParse(r.PersonalizationId, out var result) && result == controlGuid);
				if (metadataRecord2 != null)
				{
					return metadataRecord2;
				}
			}
		}
		return recordsForPage.ElementAt(0);
	}

	private PageCustomizationSyntax GetPageCustomization(IEnumerable<SyntaxNode> nodes, string pageName, int pageId)
	{
		PageCustomizationSyntax pageCustomizationSyntax = SyntaxFactory.PageCustomization(ObjectPrefix + pageId, SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(pageName)));
		return pageCustomizationSyntax.AddLayoutChanges(nodes.OfType<ControlChangeBaseSyntax>().ToArray()).AddActionsChanges(nodes.OfType<ActionChangeBaseSyntax>().ToArray());
	}

	private static bool SelectTargetAction(ActionDefinition definition, int targetPageId)
	{
		if (definition.RunObjectType == RunObjectType.Page && definition.TargetID == targetPageId)
		{
			return definition.RunFormView == null;
		}
		return false;
	}

	protected static void HandleGenericErrors(ICollection<NavXmlDeltaConversionDiagnostics> errors, Action fallibleAction)
	{
		try
		{
			fallibleAction();
		}
		catch (Exception ex)
		{
			errors.Add(new NavXmlDeltaConversionDiagnostics(ex.Message, Verbosity.Error));
		}
	}
}
