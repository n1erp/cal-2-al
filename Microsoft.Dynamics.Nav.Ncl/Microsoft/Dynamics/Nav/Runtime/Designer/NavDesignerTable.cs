using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Designer.Objects;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerTable : NavDesignerApplicationObject
{
	private readonly TableSyntax tableSyntax;

	private readonly NavDesignerIDManager fieldIdManager = new NavDesignerIDManager(50000, 99000);

	private readonly List<NavDesignerTableField> fields = new List<NavDesignerTableField>();

	public IEnumerable<NavDesignerTableField> Fields => fields;

	internal override ObjectSyntax Syntax => tableSyntax.WithFields(SyntaxFactory.FieldList(default(SyntaxList<FieldSyntax>).AddRange(fields.Select((NavDesignerTableField x) => x.Syntax as FieldSyntax))));

	protected NavDesignerTable(TableSyntax tableSyntax, int id, string name)
		: base(new ApplicationObjectId(ObjectType.Table, id), name)
	{
		this.tableSyntax = tableSyntax;
		TableSyntax tableSyntax2 = this.tableSyntax;
		object obj;
		if (tableSyntax2 == null)
		{
			obj = null;
		}
		else
		{
			FieldListSyntax fieldListSyntax = tableSyntax2.Fields;
			obj = ((fieldListSyntax != null) ? fieldListSyntax.Fields.Select(NavDesignerTableFieldFactory.CreateTableField).OfType<NavDesignerTableField>() : null);
		}
		IEnumerable<NavDesignerTableField> enumerable = (IEnumerable<NavDesignerTableField>)obj;
		if (enumerable != null)
		{
			fields.AddRange(enumerable);
		}
		IEnumerable<int> ids = Fields.Select((NavDesignerTableField x) => x.Number);
		fieldIdManager.AddIds(ids);
	}

	public static NavDesignerTable Create(int id, string objectName)
	{
		TableSyntax tableSyntax = SyntaxFactory.Table(SyntaxFactory.ObjectId(id), objectName);
		return new NavDesignerTable(tableSyntax, id, objectName);
	}

	public static NavDesignerTable Create(TableSyntax tableSyntax)
	{
		int objectId = tableSyntax.GetObjectId();
		string objectName = tableSyntax.GetObjectName();
		return new NavDesignerTable(tableSyntax, objectId, objectName);
	}

	public int AddField(string fieldName, NavType fieldType, int length = 0)
	{
		int num = 0;
		if (!Fields.Any((NavDesignerTableField x) => x.Name.Equals(fieldName)))
		{
			num = fieldIdManager.ReserveNewId();
		}
		if (num > 0)
		{
			fields.Add(NavDesignerTableFieldFactory.CreateTableField(num, fieldName, fieldType, length));
		}
		return num;
	}
}
