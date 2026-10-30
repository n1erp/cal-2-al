using System;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal interface IAlWriter
{
	IAlWriterState State { get; }

	int Indentation { get; }

	string Indent { get; }

	int LinePosition { get; }

	bool PendingEmptyLine { get; set; }

	void PopIndent();

	void PushIndent();

	void PushIndent(int increment);

	void ResetFilePosition();

	void SetIndent(int value);

	void LowWrite(string value);

	void Write(char value);

	void Write(string value, bool singleQuoteValue = false, bool ignoreIndent = false);

	void WriteIndent();

	void WriteLine();

	void WriteLine(char value);

	void WriteLine(string value, bool singleQuoteVale = false);

	void WriteLinePossiblyCommentedOut(string value, bool commentedOut);

	void WriteMultilineText(string value);

	void WriteProperty(string propName, string value);

	void WritePropertyWithValueAsAction(string propName, Action value);

	void WritePropertyPossiblyCommentedOut(string propName, string value, bool commentedOut);

	void WriteSection(string sectionName, Action writeSectionContent);

	void WriteSection(Action writeSectionContent);
}
