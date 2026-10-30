using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class AttributeLocationExtensions
{
	internal static string ToDisplayString(this AttributeLocation locations)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int num = 1; num < 1024; num <<= 1)
		{
			if (((uint)locations & (uint)(short)num) != 0)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				switch ((AttributeLocation)(short)num)
				{
				case AttributeLocation.Assembly:
					stringBuilder.Append("assembly");
					break;
				case AttributeLocation.Module:
					stringBuilder.Append("module");
					break;
				case AttributeLocation.Type:
					stringBuilder.Append("type");
					break;
				case AttributeLocation.Method:
					stringBuilder.Append("method");
					break;
				case AttributeLocation.Field:
					stringBuilder.Append("field");
					break;
				case AttributeLocation.Property:
					stringBuilder.Append("property");
					break;
				case AttributeLocation.Event:
					stringBuilder.Append("event");
					break;
				case AttributeLocation.Return:
					stringBuilder.Append("return");
					break;
				case AttributeLocation.Parameter:
					stringBuilder.Append("param");
					break;
				case AttributeLocation.TypeParameter:
					stringBuilder.Append("typevar");
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(num);
				}
			}
		}
		return stringBuilder.ToString();
	}

	internal static AttributeLocation ToAttributeLocation(this SyntaxToken token)
	{
		return ToAttributeLocation(token.ValueText);
	}

	internal static AttributeLocation ToAttributeLocation(this Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken token)
	{
		return ToAttributeLocation(token.ValueText);
	}

	private static AttributeLocation ToAttributeLocation(string text)
	{
		return text switch
		{
			"assembly" => AttributeLocation.Assembly, 
			"module" => AttributeLocation.Module, 
			"type" => AttributeLocation.Type, 
			"return" => AttributeLocation.Return, 
			"method" => AttributeLocation.Method, 
			"field" => AttributeLocation.Field, 
			"event" => AttributeLocation.Event, 
			"param" => AttributeLocation.Parameter, 
			"property" => AttributeLocation.Property, 
			"typevar" => AttributeLocation.TypeParameter, 
			_ => AttributeLocation.None, 
		};
	}
}
