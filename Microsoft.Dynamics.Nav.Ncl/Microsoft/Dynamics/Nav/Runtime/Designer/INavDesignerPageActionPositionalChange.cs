namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal interface INavDesignerPageActionPositionalChange : INavDesignerPageActionChange
{
	string ParentName { get; set; }

	string AnchorActionName { get; set; }
}
