namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal interface INavDesignerPageControlPositionalChange : INavDesignerPageControlChange
{
	string ParentName { get; set; }

	string AnchorControlName { get; set; }
}
