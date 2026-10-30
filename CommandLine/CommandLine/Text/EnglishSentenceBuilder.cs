namespace CommandLine.Text;

public class EnglishSentenceBuilder : BaseSentenceBuilder
{
	public override string OptionWord => "option";

	public override string AndWord => "and";

	public override string RequiredOptionMissingText => "required option is missing";

	public override string ViolatesFormatText => "violates format";

	public override string ViolatesMutualExclusivenessText => "violates mutual exclusiveness";

	public override string ErrorsHeadingText => "ERROR(S):";
}
