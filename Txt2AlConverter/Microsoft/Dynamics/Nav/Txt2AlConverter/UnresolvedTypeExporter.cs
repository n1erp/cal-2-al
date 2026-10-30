using System;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;
using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class UnresolvedTypeExporter(IDotNetDeclarationExtractor dotNetDeclarationExtractor, ITranslationExtractor translationExtractor) : BaseTypeResolver(dotNetDeclarationExtractor, translationExtractor)
{
	private readonly string[] ComplexTypeNames = GetApplicationObjectTypeNames();

	protected override string ResolveSubTypeName(string subType)
	{
		string[] complexTypeNames = ComplexTypeNames;
		foreach (string dataTypeName in complexTypeNames)
		{
			if (TryGetIdForUnresolvedReference(subType, dataTypeName, out var id))
			{
				return id.ToString(CultureInfo.InvariantCulture);
			}
		}
		return AlUtilities.QuoteIdentifierIfNeeded(subType);
	}

	private bool TryGetIdForUnresolvedReference(string dataType, string dataTypeName, out int id)
	{
		if (dataType.StartsWith(dataTypeName, StringComparison.OrdinalIgnoreCase))
		{
			return int.TryParse(dataType.Substring(dataTypeName.Length), out id);
		}
		id = 0;
		return false;
	}

	private static string[] GetApplicationObjectTypeNames()
	{
		return new ElementType[6]
		{
			ElementType.Table,
			ElementType.Codeunit,
			ElementType.Report,
			ElementType.Query,
			ElementType.XmlPort,
			ElementType.Page
		}.Select((ElementType x) => x.ToString()).ToArray();
	}
}
