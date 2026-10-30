namespace Microsoft.Dynamics.Nav.Runtime;

public interface IArrayAccess
{
	object this[params int[] indexes] { get; set; }

	int Length { get; }

	int[] Dimensions { get; }
}
