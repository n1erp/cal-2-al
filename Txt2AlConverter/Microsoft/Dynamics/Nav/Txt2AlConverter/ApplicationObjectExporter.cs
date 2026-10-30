using System.Globalization;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class ApplicationObjectExporter
{
	private readonly IElementExporter contentExporter;

	private readonly AlExportOptions options;

	public ApplicationObjectExporter(IElementExporter contentExporter, AlExportOptions options)
	{
		this.options = options ?? AlExportOptions.Default;
		this.contentExporter = contentExporter;
	}

	public virtual void Export(ApplicationObject applicationObject, IAlWriter writer)
	{
		writer.ResetFilePosition();
		string applicationObjectHeader = GetApplicationObjectHeader(applicationObject);
		writer.WriteSection(applicationObjectHeader, () =>
		{
			contentExporter.Export(applicationObject.RootElement, writer);
		});
		writer.WriteLine();
	}

	private static string GetApplicationObjectDeclaration(ApplicationObject applicationObject, string name, int applicationObjectId)
	{
		if (applicationObject.ElementTypeInfo.ElementType != ElementType.Modification)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", applicationObject.ElementTypeInfo.Name.ToLowerInvariant(), applicationObjectId, name);
		}
		return ModificationContentExporter.GetModificationObjectDeclaration(applicationObject, applicationObjectId);
	}

	private static string GetApplicationObjectName(IElement element)
	{
		return AlUtilities.QuoteIdentifierIfNeeded(element.GetStringProperty(PropertyType.Name) ?? string.Empty);
	}

	private string GetApplicationObjectHeader(ApplicationObject applicationObject)
	{
		string applicationObjectName = GetApplicationObjectName(applicationObject.RootElement);
		return GetApplicationObjectDeclaration(applicationObject, applicationObjectName, GetApplicationObjectId(applicationObject));
	}

	private int GetApplicationObjectId(ApplicationObject applicationObject)
	{
		if (applicationObject.ElementTypeInfo.ElementType != ElementType.Modification)
		{
			return applicationObject.Id.ObjectNumber;
		}
		return options.ReserveExtensionObjectId(applicationObject.Id.ObjectType, applicationObject.RootElement);
	}
}
