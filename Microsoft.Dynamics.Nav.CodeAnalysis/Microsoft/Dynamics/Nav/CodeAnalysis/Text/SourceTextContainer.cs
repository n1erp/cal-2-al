using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public abstract class SourceTextContainer
{
	public abstract SourceText CurrentText { get; }

	public abstract event EventHandler<TextChangeEventArgs> TextChanged;
}
