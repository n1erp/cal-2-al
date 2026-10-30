using System.Diagnostics;
using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

[DebuggerDisplay("Property={property}, EndPos={endPos}")]
public struct InlineItem(PropertyType property, int endPos)
{
	private readonly PropertyType property = property;

	private readonly int endPos = endPos;

	internal static readonly InlineItem[] IdAndName = new InlineItem[2]
	{
		new InlineItem(PropertyType.Id, 0),
		new InlineItem(PropertyType.Name, 0)
	};

	public PropertyType Property => property;

	public int EndPos => endPos;

	public static bool operator ==(InlineItem left, InlineItem right)
	{
		if (left.Property == right.Property)
		{
			return left.EndPos == right.EndPos;
		}
		return false;
	}

	public static bool operator !=(InlineItem left, InlineItem right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Property.GetHashCode() ^ EndPos.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return this == (InlineItem)obj;
	}
}
