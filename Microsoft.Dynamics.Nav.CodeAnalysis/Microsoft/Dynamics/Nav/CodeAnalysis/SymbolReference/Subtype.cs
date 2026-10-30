using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public class Subtype : IEquatable<Subtype>
{
	internal static readonly Subtype Empty = new Subtype(string.Empty, 0);

	public string Name { get; }

	public int? Id { get; }

	public bool IsEmpty
	{
		get
		{
			if (!Id.HasValue || Id == 0)
			{
				return string.IsNullOrEmpty(Name);
			}
			return false;
		}
	}

	public Subtype(string name, int? id)
	{
		Name = name;
		Id = id;
	}

	public static implicit operator Subtype(long value)
	{
		return new Subtype(string.Empty, (int)value);
	}

	public static implicit operator Subtype(int value)
	{
		return new Subtype(string.Empty, value);
	}

	public static implicit operator Subtype(string value)
	{
		return new Subtype(value, 0);
	}

	public static implicit operator long(Subtype value)
	{
		return (int)value;
	}

	public static implicit operator int(Subtype value)
	{
		if ((object)value == null || !value.Id.HasValue)
		{
			return 0;
		}
		return value.Id.Value;
	}

	public static implicit operator string(Subtype value)
	{
		if ((object)value == null || value.Name == null)
		{
			return string.Empty;
		}
		return value.Name;
	}

	public static bool operator ==(Subtype left, Subtype right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(Subtype left, Subtype right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		Subtype other = (Subtype)obj;
		return Equals(other);
	}

	public override int GetHashCode()
	{
		if (string.IsNullOrEmpty(Name) && !Id.HasValue)
		{
			return 0;
		}
		if (!Id.HasValue)
		{
			return Name.GetHashCode();
		}
		return Id.GetHashCode();
	}

	public bool Equals(Subtype other)
	{
		if (other == null)
		{
			return false;
		}
		if (Id == other.Id)
		{
			return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}
