namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct LineColumnDelta
{
	public static LineColumnDelta Default = new LineColumnDelta(0, 0, whitespaceOnly: true, forceUpdate: false);

	public int Lines { get; private set; }

	public int Spaces { get; private set; }

	public bool WhitespaceOnly { get; private set; }

	public bool ForceUpdate { get; private set; }

	public LineColumnDelta(int lines, int spaces)
	{
		this = default;
		Lines = lines;
		Spaces = spaces;
		WhitespaceOnly = true;
		ForceUpdate = false;
	}

	public LineColumnDelta(int lines, int spaces, bool whitespaceOnly)
		: this(lines, spaces)
	{
		WhitespaceOnly = whitespaceOnly;
		ForceUpdate = false;
	}

	public LineColumnDelta(int lines, int spaces, bool whitespaceOnly, bool forceUpdate)
		: this(lines, spaces, whitespaceOnly)
	{
		ForceUpdate = forceUpdate;
	}

	internal LineColumnDelta With(LineColumnDelta delta)
	{
		if (delta.Lines <= 0)
		{
			return new LineColumnDelta
			{
				Lines = Lines,
				Spaces = Spaces + delta.Spaces,
				WhitespaceOnly = (WhitespaceOnly && delta.WhitespaceOnly),
				ForceUpdate = (ForceUpdate || delta.ForceUpdate)
			};
		}
		return new LineColumnDelta
		{
			Lines = Lines + delta.Lines,
			Spaces = delta.Spaces,
			WhitespaceOnly = delta.WhitespaceOnly,
			ForceUpdate = (ForceUpdate || delta.ForceUpdate || Spaces > 0)
		};
	}
}
