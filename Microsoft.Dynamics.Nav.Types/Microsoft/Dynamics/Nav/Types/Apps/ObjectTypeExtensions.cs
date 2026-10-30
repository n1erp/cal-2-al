namespace Microsoft.Dynamics.Nav.Types.Apps;

public static class ObjectTypeExtensions
{
	public static bool IsExtensionType(this ObjectType extensionType)
	{
		bool result = false;
		if ((uint)(extensionType - 14) <= 1u || extensionType == ObjectType.EnumExtension)
		{
			result = true;
		}
		return result;
	}

	public static ObjectType ToExtensionType(this ObjectType baseType)
	{
		ObjectType result = baseType;
		switch (baseType)
		{
		case ObjectType.Page:
			result = ObjectType.PageExtension;
			break;
		case ObjectType.Table:
			result = ObjectType.TableExtension;
			break;
		case ObjectType.Enum:
			result = ObjectType.EnumExtension;
			break;
		}
		return result;
	}

	public static bool HasExtensionType(this ObjectType baseType)
	{
		bool result = false;
		if (baseType == ObjectType.Table || baseType == ObjectType.Page || baseType == ObjectType.Enum)
		{
			result = true;
		}
		return result;
	}

	public static ObjectType ToBaseObjectType(this ObjectType extensionType)
	{
		ObjectType result = extensionType;
		switch (extensionType)
		{
		case ObjectType.TableExtension:
			result = ObjectType.Table;
			break;
		case ObjectType.PageExtension:
			result = ObjectType.Page;
			break;
		case ObjectType.EnumExtension:
			result = ObjectType.Enum;
			break;
		}
		return result;
	}
}
