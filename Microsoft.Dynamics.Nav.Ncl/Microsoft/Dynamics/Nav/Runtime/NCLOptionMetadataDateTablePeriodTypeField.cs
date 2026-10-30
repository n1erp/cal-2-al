using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLOptionMetadataDateTablePeriodTypeField : NCLOptionMetadata
{
	public override string OptionCaption
	{
		get
		{
			string datePeriodTypeCaptions = Lang.DatePeriodTypeCaptions;
			if (string.IsNullOrEmpty(datePeriodTypeCaptions))
			{
				datePeriodTypeCaptions = OptionString;
			}
			return datePeriodTypeCaptions;
		}
	}

	protected string[] OptionCaptions => OptionCaption.Split(NavOption.OptionDelimiter);

	public override bool HasCaption => !string.IsNullOrEmpty(OptionCaption);

	public NCLOptionMetadataDateTablePeriodTypeField(string option)
		: base(option)
	{
	}

	public override int GetIndexFromCaption(string caption)
	{
		if (HasCaption)
		{
			int num = StringHelper.FindStringInStringArrayUsingCurrentCulture(OptionCaptions, caption);
			if (num >= 0)
			{
				return num;
			}
		}
		return base.GetIndexFromCaption(caption);
	}

	public override string GetCaptionFromIndex(int index)
	{
		if (HasCaption)
		{
			string[] optionCaptions = OptionCaptions;
			if (index >= optionCaptions.GetLowerBound(0) && index <= optionCaptions.GetUpperBound(0))
			{
				return optionCaptions[index];
			}
		}
		return base.GetCaptionFromIndex(index);
	}
}
