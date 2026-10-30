using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;

internal class ReferenceSymbolToSyntaxXmlport : ReferenceSymbolToSyntax
{
	protected override ApplicationObjectSyntax ConvertApplicationSymbol(ISymbol symbol)
	{
		return ReferenceSymbolToSyntax.ConvertApplicationSymbol<XmlPortTypeSymbol>(symbol, ConvertXmlport);
	}

	private XmlPortSyntax ConvertXmlport(XmlPortTypeSymbol xmlport)
	{
		XmlPortSyntax xmlPortSyntax = SyntaxFactory.XmlPort(SyntaxFactory.ObjectId(xmlport.Id), SyntaxFactory.IdentifierName(xmlport.Name));
		ImmutableArray<Symbol> members = xmlport.GetMembers();
		if (!xmlport.Properties.IsDefaultOrEmpty)
		{
			XmlPortSyntax xmlPortSyntax2 = xmlPortSyntax;
			PropertySyntaxOrEmpty[] items = ReferenceSymbolToSyntax.ConvertProperties(xmlport.Properties, xmlport);
			xmlPortSyntax = xmlPortSyntax2.AddPropertyListProperties(items);
		}
		if (members != null && !members.IsEmpty)
		{
			xmlPortSyntax = xmlPortSyntax.WithMembers(ConvertMembers(members));
		}
		return xmlPortSyntax;
	}
}
