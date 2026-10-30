using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Runtime.Designer.Objects;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerTableExtension : NavDesignerApplicationObject
{
	private readonly TableExtensionSyntax tableExtensionSyntax;

	private readonly NavDesignerIDManager fieldIDManager = new NavDesignerIDManager(50000, 99000);

	private readonly List<NavDesignerTableFieldBase> fields = new List<NavDesignerTableFieldBase>();

	public IEnumerable<NavDesignerTableField> Fields => fields.OfType<NavDesignerTableField>();

	public string ExtendsTableName { get; private set; }

	internal override ObjectSyntax Syntax => tableExtensionSyntax.WithFields(SyntaxFactory.FieldExtensionList(default(SyntaxList<FieldBaseSyntax>).AddRange(fields.Select((NavDesignerTableFieldBase x) => x.Syntax))));

	protected NavDesignerTableExtension(TableExtensionSyntax tableExtensionSyntax, int id, string name)
		: base(new ApplicationObjectId(ObjectType.TableExtension, id), name)
	{
		this.tableExtensionSyntax = tableExtensionSyntax;
		TableExtensionSyntax tableExtensionSyntax2 = this.tableExtensionSyntax;
		object obj;
		if (tableExtensionSyntax2 == null)
		{
			obj = null;
		}
		else
		{
			FieldExtensionListSyntax fieldExtensionListSyntax = tableExtensionSyntax2.Fields;
			obj = ((fieldExtensionListSyntax != null) ? fieldExtensionListSyntax.Fields.Select(NavDesignerTableFieldFactory.CreateTableField) : null);
		}
		IEnumerable<NavDesignerTableFieldBase> enumerable = (IEnumerable<NavDesignerTableFieldBase>)obj;
		if (enumerable != null)
		{
			fields.AddRange(enumerable);
		}
		ExtendsTableName = tableExtensionSyntax.BaseObject.Identifier.ToString().UnquoteIdentifier();
		TableExtensionSyntax tableExtensionSyntax3 = this.tableExtensionSyntax;
		object obj2;
		if (tableExtensionSyntax3 == null)
		{
			obj2 = null;
		}
		else
		{
			FieldExtensionListSyntax fieldExtensionListSyntax2 = tableExtensionSyntax3.Fields;
			obj2 = ((fieldExtensionListSyntax2 != null) ? (from x in fieldExtensionListSyntax2.Fields.OfType<FieldSyntax>()
				select (int)x.No.Value) : null);
		}
		if (obj2 == null)
		{
			obj2 = Enumerable.Empty<int>();
		}
		IEnumerable<int> ids = (IEnumerable<int>)obj2;
		fieldIDManager.AddIds(ids);
	}

	public static NavDesignerTableExtension Create(int id, string objectName, string extendsObjectName)
	{
		TableExtensionSyntax tableExtensionSyntax = SyntaxFactory.TableExtension(SyntaxFactory.ObjectId(id), objectName, SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(extendsObjectName)));
		return new NavDesignerTableExtension(tableExtensionSyntax, id, objectName);
	}

	public static NavDesignerTableExtension Create(TableExtensionSyntax tableExtensionSyntax)
	{
		int objectId = tableExtensionSyntax.GetObjectId();
		string objectName = tableExtensionSyntax.GetObjectName();
		return new NavDesignerTableExtension(tableExtensionSyntax, objectId, objectName);
	}

	public int AddField(string fieldName, NavType fieldType, int length = 0)
	{
		int num = 0;
		if (!Fields.Any((NavDesignerTableField x) => x.Name.Equals(fieldName)))
		{
			num = fieldIDManager.ReserveNewId();
		}
		if (num > 0)
		{
			fields.Add(NavDesignerTableFieldFactory.CreateTableField(num, fieldName, fieldType, length));
		}
		return num;
	}
}
