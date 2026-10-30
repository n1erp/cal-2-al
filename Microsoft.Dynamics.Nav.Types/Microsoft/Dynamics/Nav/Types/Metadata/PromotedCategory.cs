using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class PromotedCategory
{
	private static readonly ReadOnlyCollection<PromotedCategoryType> types = new ReadOnlyCollection<PromotedCategoryType>(Enum.GetValues(typeof(PromotedCategoryType)).OfType<PromotedCategoryType>().ToList());

	private string caption;

	private PromotedCategoryType categoryType;

	private List<string> actionGuidsByOrder = new List<string>();

	public static ReadOnlyCollection<PromotedCategoryType> Types => types;

	[XmlAttribute]
	public PromotedCategoryType CategoryType
	{
		get
		{
			return categoryType;
		}
		set
		{
			categoryType = value;
		}
	}

	[XmlAttribute]
	public string Caption
	{
		get
		{
			return caption;
		}
		set
		{
			caption = value;
		}
	}

	public List<string> ActionGuidsByOrder => actionGuidsByOrder;

	public override bool Equals(object obj)
	{
		if (obj is PromotedCategory promotedCategory && promotedCategory.Caption == Caption)
		{
			return promotedCategory.CategoryType == CategoryType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public PromotedCategory()
	{
	}

	public PromotedCategory(PromotedCategoryType type, string caption)
	{
		categoryType = type;
		this.caption = caption;
	}
}
