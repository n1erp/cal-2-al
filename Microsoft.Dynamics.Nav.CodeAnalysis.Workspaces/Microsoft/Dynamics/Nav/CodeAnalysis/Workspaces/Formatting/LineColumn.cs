namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct LineColumn
{
	public static LineColumn Default = new LineColumn
	{
		Line = 0,
		Column = 0,
		WhitespaceOnly = true
	};

	public int Line { get; private set; }

	public int Column { get; private set; }

	public bool WhitespaceOnly { get; private set; }

	public LineColumn(int line, int column, bool whitespaceOnly)
	{
		this = default;
		Line = line;
		Column = column;
		WhitespaceOnly = whitespaceOnly;
	}

	public LineColumn With(LineColumnDelta delta)
	{
		if (delta.Lines <= 0)
		{
			return new LineColumn
			{
				Line = Line,
				Column = Column + delta.Spaces,
				WhitespaceOnly = (WhitespaceOnly && delta.WhitespaceOnly)
			};
		}
		return new LineColumn
		{
			Line = Line + delta.Lines,
			Column = delta.Spaces,
			WhitespaceOnly = delta.WhitespaceOnly
		};
	}
}
