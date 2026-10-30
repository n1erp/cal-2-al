using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class AttributeDataExtensions
{
	internal static int IndexOfAttribute(this ImmutableArray<CSharpAttributeData> attributes, Symbol targetSymbol, AttributeDescription description)
	{
		for (int i = 0; i < attributes.Length; i++)
		{
			if (attributes[i].IsTargetAttribute(targetSymbol, description))
			{
				return i;
			}
		}
		return -1;
	}

	internal static CSharpSyntaxNode GetAttributeArgumentSyntax(this AttributeData attribute, int parameterIndex, AttributeSyntax attributeSyntax)
	{
		return ((SourceAttributeData)attribute).GetAttributeArgumentSyntax(parameterIndex, attributeSyntax);
	}

	internal static Location GetAttributeArgumentSyntaxLocation(this AttributeData attribute, int parameterIndex, AttributeSyntax attributeSyntaxOpt)
	{
		if (attributeSyntaxOpt == null)
		{
			return NoLocation.Singleton;
		}
		return ((SourceAttributeData)attribute).GetAttributeArgumentSyntax(parameterIndex, attributeSyntaxOpt).Location;
	}
}
