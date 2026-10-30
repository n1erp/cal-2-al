using System;

namespace CommandLine;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ValueOptionAttribute : Attribute
{
	private readonly int _index;

	public int Index => _index;

	public ValueOptionAttribute(int index)
	{
		_index = index;
	}
}
