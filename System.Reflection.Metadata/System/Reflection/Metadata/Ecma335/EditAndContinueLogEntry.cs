namespace System.Reflection.Metadata.Ecma335;

public readonly struct EditAndContinueLogEntry : IEquatable<EditAndContinueLogEntry>
{
	public EntityHandle Handle { get; }

	public EditAndContinueOperation Operation { get; }

	public EditAndContinueLogEntry(EntityHandle handle, EditAndContinueOperation operation)
	{
		Handle = handle;
		Operation = operation;
	}

	public override bool Equals(object obj)
	{
		if (obj is EditAndContinueLogEntry)
		{
			return Equals((EditAndContinueLogEntry)obj);
		}
		return false;
	}

	public bool Equals(EditAndContinueLogEntry other)
	{
		if (Operation == other.Operation)
		{
			return Handle == other.Handle;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)Operation ^ Handle.GetHashCode();
	}
}
