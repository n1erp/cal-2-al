using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NCLOptionMetadataTable2000000041Field5 : NCLOptionMetadata
{
	private static int[] typeIndexes = new int[21]
	{
		4913, 4989, 11520, 11776, 11777, 11798, 12800, 26208, 26209, 31489,
		31490, 33792, 33794, 34048, 34560, 35072, 35584, 36096, 36864, 37120,
		37376
	};

	public NCLOptionMetadataTable2000000041Field5(string option)
		: base(option)
	{
		if (Options.Length != typeIndexes.Length)
		{
			throw new ArgumentException(Lang.OptionStringHasWrongNumberOfParameters);
		}
	}

	public override string GetOptionFromIndex(int index)
	{
		int num = index + 1;
		for (int i = 0; i < typeIndexes.Length; i++)
		{
			if (num == typeIndexes[i])
			{
				return Options[i];
			}
		}
		return index.ToString(CultureInfo.InvariantCulture);
	}

	public override int GetIndexFromOption(string option)
	{
		int result = StringHelper.FindStringInStringArrayUsingCurrentCulture(Options, option);
		if (result != -1)
		{
			return typeIndexes[result] - 1;
		}
		if (int.TryParse(option, out result))
		{
			return result;
		}
		throw new NavNCLOptionStringNotFoundException();
	}
}
