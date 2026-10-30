using System;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

[Flags]
public enum TokenizerOptions
{
	None = 0,
	SkipLeadingWhiteSpace = 1,
	TrimTrailingWhiteSpace = 2,
	SkipLeadingLineBreak = 4,
	TrimTrailingLineBreak = 8,
	NoPingProcessing = 0x10,
	NoKeywordLookup = 0x20,
	NoBracketProcessing = 0x40,
	SkipWhiteSpace = SkipLeadingWhiteSpace | TrimTrailingWhiteSpace,
	SkipLineBreak = SkipLeadingLineBreak | TrimTrailingLineBreak
}
