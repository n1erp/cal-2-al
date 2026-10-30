namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public static class CompilerFeaturesExtensions
{
	public static bool ShouldGenerateTranslationFile(this CompilerFeatures compilerFeatures)
	{
		return (compilerFeatures & (CompilerFeatures.GenerateLcgTranslationFile | CompilerFeatures.GenerateXliffTranslationFile)) != 0;
	}

	public static bool ShouldGenerateXliffTranslationFile(this CompilerFeatures compilerFeatures)
	{
		if ((compilerFeatures & CompilerFeatures.GenerateXliffTranslationFile) != 0)
		{
			return !compilerFeatures.ShouldGenerateLcgTranslationFile();
		}
		return false;
	}

	public static bool ShouldGenerateLcgTranslationFile(this CompilerFeatures compilerFeatures)
	{
		return (compilerFeatures & CompilerFeatures.GenerateLcgTranslationFile) != 0;
	}

	public static bool TryGetFromRawString(string rawString, out CompilerFeatures compilerFeatures)
	{
		compilerFeatures = CompilerFeatures.None;
		if (string.IsNullOrEmpty(rawString))
		{
			return true;
		}
		string[] array = rawString.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim().ToUpperInvariant();
			if (!(text == "LCGTRANSLATIONFILE"))
			{
				if (!(text == "TRANSLATIONFILE"))
				{
					return false;
				}
				compilerFeatures |= CompilerFeatures.GenerateXliffTranslationFile;
			}
			else
			{
				compilerFeatures |= CompilerFeatures.GenerateLcgTranslationFile;
			}
		}
		return true;
	}
}
