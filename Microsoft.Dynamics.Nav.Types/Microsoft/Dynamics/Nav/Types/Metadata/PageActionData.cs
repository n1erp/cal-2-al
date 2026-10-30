using System;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
internal class PageActionData
{
	internal int PageId { get; set; }

	internal int ActionIndex { get; set; }

	internal int ActionId { get; set; }

	internal int Indentation { get; set; }

	internal int ParentActionId { get; set; }

	internal int ActionType { get; set; }

	internal int ActionSubtype { get; set; }

	internal string Name { get; set; }

	internal string CaptionML { get; set; }

	internal string ToolTipML { get; set; }

	internal bool Promoted { get; set; }

	internal int RunObjectType { get; set; }

	internal int RunObjectId { get; set; }

	internal int RunPageMode { get; set; }

	internal int RunSourceTable { get; set; }

	internal string ApplicationArea { get; set; }
}
