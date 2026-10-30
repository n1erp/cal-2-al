namespace Microsoft.Dynamics.Nav.Types;

public enum DataItemLinkType
{
	LeftOuterJoin = 0,
	InnerJoin = 1,
	RightOuterJoin = 2,
	FullOuterJoin = 3,
	CrossJoin = 4,
	CrossApply = 5,
	OuterApply = 6,
	None = 99
}
