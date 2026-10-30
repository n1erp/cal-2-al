using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public struct Printer
{
	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public string Driver { get; set; }

	[DataMember]
	public string Device { get; set; }

	[DataMember]
	public string Comment { get; set; }

	[DataMember]
	public string Location { get; set; }

	[DataMember]
	public bool DefaultPrinter { get; set; }

	public override int GetHashCode()
	{
		return Name.GetHashCode() ^ Driver.GetHashCode() ^ Device.GetHashCode() ^ Comment.GetHashCode() ^ Location.GetHashCode() ^ DefaultPrinter.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is Printer printer)
		{
			if (Name == printer.Name && Driver == printer.Driver && Device == printer.Device && Comment == printer.Comment && Location == printer.Location)
			{
				return DefaultPrinter == printer.DefaultPrinter;
			}
			return false;
		}
		return false;
	}

	public static bool operator ==(Printer leftPrinter, Printer rightPrinter)
	{
		return leftPrinter.Equals(rightPrinter);
	}

	public static bool operator !=(Printer leftPrinter, Printer rightPrinter)
	{
		return !leftPrinter.Equals(rightPrinter);
	}
}
