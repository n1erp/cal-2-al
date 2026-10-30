using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference.DotNet;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class BaseTypeResolver : ITypeResolver
{
	private readonly IDotNetDeclarationExtractor dotNetDeclarationExtractor;

	private readonly ITranslationExtractor translationExtractor;

	public BaseTypeResolver(IDotNetDeclarationExtractor dotNetDeclarationExtractor, ITranslationExtractor translationExtractor)
	{
		this.dotNetDeclarationExtractor = dotNetDeclarationExtractor;
		this.translationExtractor = translationExtractor;
	}

	public string ResolveType(IElement element)
	{
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		if (element.GetPropertySetState(PropertyType.Dimensions))
		{
			stringWriter.Write(string.Format(CultureInfo.InvariantCulture, "array [{0}] of ", element.GetStringProperty(PropertyType.Dimensions)));
		}
		string stringProperty = element.GetStringProperty(PropertyType.DataType);
		switch (stringProperty.ToUpperInvariant())
		{
		case "TEXTCONST":
			if (element.GetPropertySetState(PropertyType.TextConstString))
			{
				stringWriter.Write(TranslationUtilities.ExtractTextConst(element, translationExtractor, stringProperty));
			}
			break;
		case "OPTION":
			WriteOptionType(element, stringWriter, stringProperty);
			break;
		case "AUTOMATION":
		case "OCX":
		case "DOTNET":
		{
			stringWriter.Write(stringProperty);
			string stringProperty2 = element.GetStringProperty(PropertyType.ExternalDataType);
			if (stringProperty2 != null)
			{
				stringWriter.Write(" ");
				stringWriter.Write(dotNetDeclarationExtractor.AddTypeDeclarationAndGetAlias(stringProperty2));
			}
			break;
		}
		case "XMLPORT":
			stringWriter.Write("XMLport");
			break;
		case "SECURITYFILTERING":
			stringWriter.Write("SecurityFilter");
			break;
		case "GUID":
			stringWriter.Write("Guid");
			break;
		default:
			stringWriter.Write(stringProperty);
			break;
		}
		if (element.GetPropertySetState(PropertyType.Length))
		{
			stringWriter.Write(string.Format(CultureInfo.InvariantCulture, "[{0}]", element.GetInt32Property(PropertyType.Length)));
		}
		if (element.GetPropertySetState(PropertyType.Subtype))
		{
			string stringProperty3 = element.GetStringProperty(PropertyType.Subtype);
			stringWriter.Write(' ');
			stringWriter.Write(ResolveSubTypeName(stringProperty3));
		}
		return stringWriter.ToString();
	}

	private static void WriteOptionType(IElement element, StringWriter writer, string value)
	{
		writer.Write("Option");
		if (!element.TryGetStringProperty(PropertyType.OptionString, out var value2) || string.IsNullOrEmpty(value2))
		{
			return;
		}
		writer.Write(' ');
		StringBuilder stringBuilder = new StringBuilder(value2.Length);
		string[] array = value2.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(AlUtilities.QuoteIdentifierIfNeeded(array[i]));
			if (i < array.Length - 1)
			{
				stringBuilder.Append(",");
			}
		}
		writer.Write(stringBuilder.ToString());
	}

	protected virtual string ResolveSubTypeName(string subtype)
	{
		return AlUtilities.QuoteIdentifierIfNeeded(subtype);
	}
}
