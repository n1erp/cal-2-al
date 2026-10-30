using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Designer.Images;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FieldTypeDesignerProvider : EagerVirtualDataProvider
{
	private enum FieldType
	{
		NumberField,
		TextField,
		BooleanField,
		DateTimeField,
		TableRelationField,
		OptionField
	}

	private class StaticEntry
	{
		public string DisplayName { get; }

		public string Description { get; }

		public NavDesignerImage Image { get; }

		public FieldType FieldType { get; }

		public StaticEntry(string displayName, string description, NavDesignerImage image, FieldType fieldType)
		{
			DisplayName = displayName;
			Description = description;
			Image = image;
			FieldType = fieldType;
		}
	}

	private const int NameLength = 30;

	private const int DescriptionLength = 250;

	private readonly NavDesignerVirtualTableMediaFactory mediaFactory = new NavDesignerVirtualTableMediaFactory();

	private readonly List<StaticEntry> entries = new List<StaticEntry>
	{
		new StaticEntry(Lang.Number, Lang.NumberField, new NavDesignerImage("BrickIcon_Field_Number", Image.BrickIcon_Field_Number), FieldType.NumberField),
		new StaticEntry(Lang.Text, Lang.TextField, new NavDesignerImage("BrickIcon_Field_Text", Image.BrickIcon_Field_Text), FieldType.TextField),
		new StaticEntry(Lang.Boolean, Lang.BooleanField, new NavDesignerImage("BrickIcon_Field_Boolean", Image.BrickIcon_Field_Boolean), FieldType.BooleanField),
		new StaticEntry(Lang.DateTime, Lang.DateTimeField, new NavDesignerImage("BrickIcon_Field_DateTime", Image.BrickIcon_Field_DateTime), FieldType.DateTimeField),
		new StaticEntry(Lang.Option, Lang.OptionField, new NavDesignerImage("BrickIcon_Field_Option", Image.BrickIcon_Field_Option), FieldType.OptionField)
	};

	public override int TableId => 2000000172;

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = false;
		return IterateItems();
	}

	private IEnumerable<ReadOnlyRecordBuffer> IterateItems()
	{
		for (int i = 1; i <= entries.Count; i++)
		{
			StaticEntry staticEntry = entries[i - 1];
			yield return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(i), NavText.CreateTruncated(30, staticEntry.DisplayName), NavText.CreateTruncated(250, staticEntry.Description), mediaFactory.GetNamedMedia(staticEntry.Image, TableId), NavText.CreateTruncated(1, string.Empty), GetOptionValue(6, (int)staticEntry.FieldType));
		}
	}
}
