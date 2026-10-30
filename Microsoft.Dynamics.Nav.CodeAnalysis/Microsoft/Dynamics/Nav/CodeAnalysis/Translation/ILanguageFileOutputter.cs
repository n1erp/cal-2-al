using System;
using System.Xml;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

public interface ILanguageFileOutputter
{
	void WriteDefaultNodesAndContinue(XmlWriter writer, string appName, string targetLanguage, Action action);

	void WriteLabel(XmlWriter writer, TranslationItem translationItem);
}
