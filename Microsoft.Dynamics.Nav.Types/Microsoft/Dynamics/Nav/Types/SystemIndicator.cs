using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SystemIndicator
{
	[DataMember]
	private readonly string text;

	[DataMember]
	private readonly SystemIndicatorStyle style;

	private static readonly string styleOptionString = CalculateStyleOptionString();

	public string Text => text;

	public SystemIndicatorStyle Style => style;

	public static string StyleOptionString => styleOptionString;

	public SystemIndicator(string text, SystemIndicatorStyle style)
	{
		this.text = text;
		if (style < SystemIndicatorStyle.Standard || style > SystemIndicatorStyle.Accent9)
		{
			this.style = SystemIndicatorStyle.Standard;
		}
		else
		{
			this.style = style;
		}
	}

	private static string CalculateStyleOptionString()
	{
		StringBuilder stringBuilder = new StringBuilder(100);
		stringBuilder.Append(SystemIndicatorStyle.Standard.ToString());
		for (SystemIndicatorStyle systemIndicatorStyle = SystemIndicatorStyle.Accent1; systemIndicatorStyle <= SystemIndicatorStyle.Accent9; systemIndicatorStyle++)
		{
			stringBuilder.Append(',');
			stringBuilder.Append(systemIndicatorStyle.ToString());
		}
		return stringBuilder.ToString();
	}
}
