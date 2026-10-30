using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

internal class InlineTemplate
{
	internal static InlineTemplate[] Definitions = new InlineTemplate[24]
	{
		new InlineTemplate(ElementType.Change, new InlineItem(PropertyType.ChangeType, 20), new InlineItem(PropertyType.Unknown, 20)),
		new InlineTemplate(ElementType.Variable, new InlineItem(PropertyType.Id, 6), new InlineItem(PropertyType.Name, 32), new InlineItem(PropertyType.DataType, 46), new InlineItem(PropertyType.Unknown, 46)),
		new InlineTemplate(ElementType.Parameter, new InlineItem(PropertyType.Id, 6), new InlineItem(PropertyType.Name, 32), new InlineItem(PropertyType.DataType, 46), new InlineItem(PropertyType.Unknown, 46)),
		new InlineTemplate(ElementType.ReportSection),
		new InlineTemplate(ElementType.ReportItem),
		new InlineTemplate(ElementType.ReportControl, new InlineItem(PropertyType.Id, 6), new InlineItem(PropertyType.ControlType, 20), new InlineItem(PropertyType.X, 26), new InlineItem(PropertyType.Y, 32), new InlineItem(PropertyType.ControlWidth, 38), new InlineItem(PropertyType.ControlHeight, 44), new InlineItem(PropertyType.Unknown, 44)),
		new InlineTemplate(ElementType.Item, new InlineItem(PropertyType.ObjectType, 12), new InlineItem(PropertyType.Id, 22), new InlineItem(PropertyType.Name, 52), new InlineItem(PropertyType.Unknown, 52)),
		new InlineTemplate(ElementType.Reference, new InlineItem(PropertyType.Reference, 50), new InlineItem(PropertyType.Unknown, 50)),
		new InlineTemplate(ElementType.MenuNode, new InlineItem(PropertyType.MenuNodeType, 17), new InlineItem(PropertyType.MenuNodeId, 59), new InlineItem(PropertyType.Unknown, 59)),
		new InlineTemplate(ElementType.DataportItem),
		new InlineTemplate(ElementType.DataportField, new InlineItem(PropertyType.StartPos, 7), new InlineItem(PropertyType.Width, 13), new InlineItem(PropertyType.SourceExpression, 34), new InlineItem(PropertyType.Unknown, 34)),
		new InlineTemplate(ElementType.RequestForm),
		new InlineTemplate(ElementType.Control, new InlineItem(PropertyType.Id, 6), new InlineItem(PropertyType.Level, 11), new InlineItem(PropertyType.ControlType, 22), new InlineItem(PropertyType.Unknown, -11)),
		new InlineTemplate(ElementType.Action, new InlineItem(PropertyType.Id, 10), new InlineItem(PropertyType.Level, 15), new InlineItem(PropertyType.ActionType, 26), new InlineItem(PropertyType.Unknown, 15)),
		new InlineTemplate(ElementType.Field, new InlineItem(PropertyType.FieldNo, 6), new InlineItem(PropertyType.Enabled, 10), new InlineItem(PropertyType.Name, 31), new InlineItem(PropertyType.DataType, 46), new InlineItem(PropertyType.Unknown, 46)),
		new InlineTemplate(ElementType.Key, new InlineItem(PropertyType.Enabled, 5), new InlineItem(PropertyType.Key, 46), new InlineItem(PropertyType.Unknown, 46)),
		new InlineTemplate(ElementType.FieldGroup, new InlineItem(PropertyType.Id, 6), new InlineItem(PropertyType.Name, 27), new InlineItem(PropertyType.Fields, 68), new InlineItem(PropertyType.Unknown, 68)),
		new InlineTemplate(ElementType.FormControl, new InlineItem(PropertyType.Id, 6), new InlineItem(PropertyType.ControlType, 20), new InlineItem(PropertyType.X, 26), new InlineItem(PropertyType.Y, 32), new InlineItem(PropertyType.ControlWidth, 38), new InlineItem(PropertyType.ControlHeight, 44), new InlineItem(PropertyType.Unknown, 44)),
		new InlineTemplate(ElementType.XmlPortElement, new InlineItem(PropertyType.ElementId, 42), new InlineItem(PropertyType.Indentation, 45), new InlineItem(PropertyType.Name, 66), new InlineItem(PropertyType.NodeType, 75), new InlineItem(PropertyType.SourceType, 84), new InlineItem(PropertyType.Unknown, -45)),
		new InlineTemplate(ElementType.XmlPortEvent, new InlineItem(PropertyType.Name, 3), new InlineItem(PropertyType.Unknown, 3)),
		new InlineTemplate(ElementType.RequestPage),
		new InlineTemplate(ElementType.Label, new InlineItem(PropertyType.Id, 6), new InlineItem(PropertyType.Name, 27), new InlineItem(PropertyType.Unknown, 27)),
		new InlineTemplate(ElementType.DataItem, new InlineItem(PropertyType.Id, 6), new InlineItem(PropertyType.Indentation, 11), new InlineItem(PropertyType.ReportDataItemDataType, 20), new InlineItem(PropertyType.Name, 41), new InlineItem(PropertyType.Unknown, -10)),
		new InlineTemplate(ElementType.QueryElement, new InlineItem(PropertyType.Id, 6), new InlineItem(PropertyType.Level, 11), new InlineItem(PropertyType.QueryElementType, 20), new InlineItem(PropertyType.Name, 41), new InlineItem(PropertyType.Unknown, -10))
	};

	public ElementType ElementType { get; private set; }

	public InlineItem[] Items { get; private set; }

	public InlineTemplate(ElementType elementType, params InlineItem[] items)
	{
		ElementType = elementType;
		Items = items;
	}
}
