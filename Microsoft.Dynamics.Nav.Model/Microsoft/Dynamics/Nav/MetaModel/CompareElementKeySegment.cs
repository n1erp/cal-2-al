using System.Globalization;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaModel;

public class CompareElementKeySegment : IElementKeySegment
{
	private readonly string id;

	private readonly string name;

	private readonly ElementType type;

	public string Id => id;

	public string Name => name;

	public ElementType ElementType => type;

	public CompareElementKeySegment(string id, string name, ElementType type)
	{
		this.id = id;
		this.name = name;
		this.type = type;
	}

	public override string ToString()
	{
		string text = (string.IsNullOrEmpty(Id) ? EnumNameLookup<ElementType>.EnumToString(ElementType) : string.Format(CultureInfo.InvariantCulture, "{0} {1}", EnumNameLookup<ElementType>.EnumToString(ElementType), Id));
		if (!string.IsNullOrEmpty(Name))
		{
			text = string.Format(CultureInfo.InvariantCulture, "{0}({1})", ModelTextHelper.QuoteIdentifierIfNeeded(Name), text);
		}
		return text;
	}
}
