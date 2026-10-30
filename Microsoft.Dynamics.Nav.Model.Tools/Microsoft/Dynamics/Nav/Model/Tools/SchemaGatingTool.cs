using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Tools.Language;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal sealed class SchemaGatingTool : ReadWriteModelTool
{
	private List<string> disallowedChanges = new List<string>();

	public IReadOnlyCollection<string> DisallowedChanges => disallowedChanges.AsReadOnly();

	internal SchemaGatingTool(ILogger logger)
		: base(logger)
	{
		disallowedChanges = new List<string>();
	}

	public bool ValidateApplicationTableChange(string[] original, string[] modified)
	{
		ObjectCollection objectCollection = ReadObjects(original, null).ObjectCollection;
		CheckObjects(objectCollection, InputType.ApplicationObject, NonLocalizable.CheckObjectsOnlyApplicationObjects, NonLocalizable.OriginalParameterText);
		ObjectCollection objectCollection2 = ReadObjects(modified, null).ObjectCollection;
		CheckObjects(objectCollection2, InputType.ApplicationObject, NonLocalizable.CheckObjectsOnlyApplicationObjects, NonLocalizable.ModifiedParameterText);
		WhitelistValidator.Initialize();
		disallowedChanges.Clear();
		IEnumerable<ApplicationObject> applicationObjects = objectCollection.GetApplicationObjects(ElementType.Table);
		IEnumerable<ApplicationObject> applicationObjects2 = objectCollection2.GetApplicationObjects(ElementType.Table);
		List<ApplicationObject> list = applicationObjects2.ToList();
		var source = (from o in applicationObjects
			join m in applicationObjects2 on o.Id equals m.Id
			select new
			{
				Original = o,
				Modified = m
			}).ToArray();
		bool flag = true;
		foreach (ApplicationObject originalTable in applicationObjects)
		{
			var anon = source.FirstOrDefault(p => p.Original.Id.Equals(originalTable.Id));
			ApplicationObject applicationObject;
			if (anon != null)
			{
				applicationObject = anon.Modified;
				list.Remove(applicationObject);
			}
			else
			{
				applicationObject = new ApplicationObject(null);
			}
			bool flag2 = WhitelistValidator.IsAllowed(originalTable, applicationObject);
			flag = flag && flag2;
		}
		foreach (ApplicationObject item in list)
		{
			bool flag3 = WhitelistValidator.IsAllowed(new ApplicationObject(null), item);
			flag = flag && flag3;
		}
		if (!flag)
		{
			disallowedChanges.AddRange(WhitelistValidator.DisallowedChanges);
		}
		return flag;
	}

	private static void CheckObjects(ObjectCollection objects, InputType inputType, string errorMessage, string objectSource)
	{
		if (inputType == InputType.ApplicationObject && objects.Any((ApplicationObject o) => o.ElementTypeInfo.ElementType == ElementType.Modification))
		{
			throw new ModelToolException(errorMessage, objectSource);
		}
	}
}
