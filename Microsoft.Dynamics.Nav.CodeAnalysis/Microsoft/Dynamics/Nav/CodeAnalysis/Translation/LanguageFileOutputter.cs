using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

internal abstract class LanguageFileOutputter : ILanguageFileOutputter
{
	public static XmlWriterSettings DefaultXmlWriterSettings = new XmlWriterSettings
	{
		Encoding = Encoding.UTF8,
		Indent = true,
		NewLineChars = Environment.NewLine
	};

	internal static void OutputLanguageFile(Compilation compilation, Stream outputStream, string appName)
	{
		ILanguageFileOutputter outputter;
		if (compilation.Options.CompilerFeatures.ShouldGenerateLcgTranslationFile())
		{
			outputter = new LcgOutputter();
		}
		else
		{
			outputter = new XliffOutputter();
		}
		XmlWriter writer = XmlWriter.Create(outputStream, DefaultXmlWriterSettings);
		try
		{
			outputter.WriteDefaultNodesAndContinue(writer, appName, null, () =>
			{
				GlobalLogger.LogVerbose("Collecting all the labels and generating the translation template file.");
				LabelWriterVisitor visitor = new LabelWriterVisitor(writer, outputter);
				compilation.CompiledModule.Accept(visitor);
			});
		}
		finally
		{
			if (writer != null)
			{
				((IDisposable)writer).Dispose();
			}
		}
	}

	public abstract void WriteDefaultNodesAndContinue(XmlWriter writer, string appName, string targetLanguage, Action action);

	public abstract void WriteLabel(XmlWriter writer, TranslationItem translationItem);
}
