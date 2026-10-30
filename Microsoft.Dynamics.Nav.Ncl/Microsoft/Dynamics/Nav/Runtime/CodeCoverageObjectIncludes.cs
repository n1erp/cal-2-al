using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CodeCoverageObjectIncludes : TreeObject
{
	private readonly List<ApplicationObjectId> objectIds;

	private CodeCoverageObjectIncludes(NavCompany company, List<ApplicationObjectId> applicationObjectIds)
		: base(company)
	{
		objectIds = applicationObjectIds;
	}

	public static CodeCoverageObjectIncludes CreateAndAdd(NavRecord objectTableRecord)
	{
		if (objectTableRecord.TableID != 2000000001)
		{
			throw new NavNCLTableIdMismatchException(Lang.CodeCovereageIncludeMustBeCalledWithObjectTable);
		}
		List<ApplicationObjectId> list = new List<ApplicationObjectId>();
		NCLMetaField fieldByNo = objectTableRecord.MetaTable.GetFieldByNo(1);
		NCLMetaField fieldByNo2 = objectTableRecord.MetaTable.GetFieldByNo(3);
		while (objectTableRecord.ALNext() > 0)
		{
			ObjectType objectType = (ObjectType)objectTableRecord.GetFieldValue(fieldByNo).ToInt32();
			switch (objectType)
			{
			case ObjectType.Table:
			case ObjectType.Report:
			case ObjectType.CodeUnit:
			case ObjectType.XmlPort:
			case ObjectType.Page:
			case ObjectType.Query:
				list.Add(new ApplicationObjectId(objectType, objectTableRecord.GetFieldValue(fieldByNo2).ToInt32()));
				break;
			}
		}
		return new CodeCoverageObjectIncludes(objectTableRecord.Session.Company, list);
	}

	public static IEnumerable<ApplicationObjectId> GetAnyIncludesDefinedForSession(NavSession session)
	{
		return session.Company.Tree.Children.OfType<CodeCoverageObjectIncludes>().ToList().SelectMany((CodeCoverageObjectIncludes inc) => inc.objectIds);
	}

	public static void RemoveAnyOnSession(NavSession session)
	{
		foreach (CodeCoverageObjectIncludes item in session.Company.Tree.Children.OfType<CodeCoverageObjectIncludes>().ToList())
		{
			item.Dispose();
		}
	}
}
