using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DiagnosticAnalyzerAttribute : Attribute
{
	public string[] Languages { get; }

	public DiagnosticAnalyzerAttribute(string firstLanguage, params string[] additionalLanguages)
	{
		if (firstLanguage == null)
		{
			throw new ArgumentNullException("firstLanguage");
		}
		if (additionalLanguages == null)
		{
			throw new ArgumentNullException("additionalLanguages");
		}
		string[] array = new string[additionalLanguages.Length + 1];
		array[0] = firstLanguage;
		for (int i = 0; i < additionalLanguages.Length; i++)
		{
			array[i + 1] = additionalLanguages[i];
		}
		Languages = array;
	}
}
