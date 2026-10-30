namespace Microsoft.Dynamics.Nav.Runtime;

internal class FormatOptions
{
	internal int Field { get; set; }

	internal int Length { get; set; }

	internal char Overflow { get; set; }

	internal char FillerCharacter { get; set; }

	internal char Comma { get; set; }

	internal char Character1000 { get; set; }

	internal bool LeftJustify { get; set; }

	internal byte PrecisionMin { get; set; }

	internal byte PrecisionMax { get; set; }

	internal FormatOptions()
	{
		PrecisionMax = byte.MaxValue;
	}

	internal FormatOptions(char overflow, char fillerCharacter, char comma, char character1000, bool leftJustify, byte precisionMin, byte precisionMax)
	{
		Field = 0;
		Length = 0;
		Overflow = overflow;
		FillerCharacter = fillerCharacter;
		Comma = comma;
		Character1000 = character1000;
		LeftJustify = leftJustify;
		PrecisionMin = precisionMin;
		PrecisionMax = precisionMax;
	}

	internal void Initialize(FormatOptions other)
	{
		Overflow = other.Overflow;
		FillerCharacter = other.FillerCharacter;
		Comma = other.Comma;
		Character1000 = other.Character1000;
		LeftJustify = other.LeftJustify;
	}

	internal void Clear()
	{
		Field = 0;
		Length = 0;
		Overflow = '\0';
		FillerCharacter = '\0';
		Comma = '\0';
		Character1000 = '\0';
		LeftJustify = false;
	}
}
