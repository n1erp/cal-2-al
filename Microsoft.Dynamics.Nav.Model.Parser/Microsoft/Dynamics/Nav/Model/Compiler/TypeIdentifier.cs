namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class TypeIdentifier : AstNode
{
	public int TypeId { get; private set; }

	public int SubType { get; private set; }

	public int DataSize { get; private set; }

	public int[] ArrayInfo { get; private set; }

	public string OptionName { get; set; }

	public bool IsVar { get; set; }

	public bool IsTemp { get; set; }

	public TypeIdentifier(int typeId, TextPosition position = null)
		: this(typeId, 0, position)
	{
	}

	public TypeIdentifier(int typeId, int subType, TextPosition position = null)
		: this(typeId, subType, -1, null, position)
	{
	}

	public TypeIdentifier(int typeId, int subType, int dataSize, int[] arrayInfo, TextPosition position = null)
		: base(AstNodeType.TypeNode, position)
	{
		TypeId = typeId;
		SubType = subType;
		DataSize = dataSize;
		ArrayInfo = arrayInfo;
	}

	public bool Equals(TypeIdentifier other)
	{
		if (other == null)
		{
			return false;
		}
		if (TypeId != other.TypeId)
		{
			return false;
		}
		if (SubType != other.SubType)
		{
			return false;
		}
		if (DataSize != other.DataSize)
		{
			return false;
		}
		if (OptionName != other.OptionName)
		{
			return false;
		}
		return true;
	}

	protected internal override void Accept(AstVisitor visitor)
	{
		visitor.VisitTypeNode(this);
	}
}
