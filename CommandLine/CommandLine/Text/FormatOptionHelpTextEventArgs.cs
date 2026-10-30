using System;

namespace CommandLine.Text;

public class FormatOptionHelpTextEventArgs : EventArgs
{
	private readonly BaseOptionAttribute _option;

	public BaseOptionAttribute Option => _option;

	public FormatOptionHelpTextEventArgs(BaseOptionAttribute option)
	{
		_option = option;
	}
}
