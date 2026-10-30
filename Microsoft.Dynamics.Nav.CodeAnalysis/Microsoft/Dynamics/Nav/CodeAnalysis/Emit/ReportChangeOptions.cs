namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class ReportChangeOptions
{
	public enum ChangeAction
	{
		Delete,
		Rename,
		Namespace
	}

	public enum DeleteAction
	{
		None,
		DeleteNode,
		DeleteContent,
		DeleteRepeater,
		DeleteRepeaterAndContent,
		ReplaceWithText
	}

	public enum ElementType
	{
		Field,
		DataItem,
		Label
	}
}
