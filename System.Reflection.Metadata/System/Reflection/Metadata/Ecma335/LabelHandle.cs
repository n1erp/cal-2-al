namespace System.Reflection.Metadata.Ecma335;

public readonly struct LabelHandle : IEquatable<LabelHandle>
{
	public int Id { get; }

	public bool IsNil => Id == 0;

	internal LabelHandle(int id)
	{
		Id = id;
	}

	public bool Equals(LabelHandle other)
	{
		return Id == other.Id;
	}

	public override bool Equals(object obj)
	{
		if (obj is LabelHandle)
		{
			return Equals((LabelHandle)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public static bool operator ==(LabelHandle left, LabelHandle right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LabelHandle left, LabelHandle right)
	{
		return !left.Equals(right);
	}
}
