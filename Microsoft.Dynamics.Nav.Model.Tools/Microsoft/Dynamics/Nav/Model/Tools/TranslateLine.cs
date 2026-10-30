namespace Microsoft.Dynamics.Nav.Model.Tools;

public class TranslateLine
{
	public string Key { get; set; }

	public string Value { get; set; }

	public string Line => LanguageKeyHelper.CreateLanguageString(Key, Value);

	public override string ToString()
	{
		return Line;
	}
}
