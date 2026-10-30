using System.Globalization;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class ObjectIdType : SimpleType
{
	private string rawValue;

	private ObjectTypes objectType;

	private int id;

	private static ObjectIdType empty = new ObjectIdType();

	public override DataTypes DataType => DataTypes.ObjectId;

	public override bool IsValidated => rawValue == null;

	public override bool HasValue
	{
		get
		{
			if (objectType == ObjectTypes.Unknown && id == 0)
			{
				return !string.IsNullOrEmpty(rawValue);
			}
			return true;
		}
	}

	public ObjectTypes ObjectType
	{
		get
		{
			AssertValidation();
			return objectType;
		}
	}

	public int Id
	{
		get
		{
			AssertValidation();
			return id;
		}
	}

	public override string ValueAsString
	{
		get
		{
			if (!IsValidated)
			{
				return rawValue;
			}
			if (!HasValue)
			{
				return string.Empty;
			}
			return EnumNameLookup<ObjectTypes>.EnumToString(ObjectType) + Id.ToString(CultureInfo.InvariantCulture);
		}
	}

	public static ObjectIdType Empty => empty;

	private ObjectIdType()
	{
	}

	private ObjectIdType(ObjectTypes objectType, int id)
	{
		this.objectType = objectType;
		this.id = id;
	}

	private ObjectIdType(string rawValue)
	{
		this.rawValue = rawValue;
	}

	public static ObjectIdType Create(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return Empty;
		}
		return new ObjectIdType(value);
	}
}
