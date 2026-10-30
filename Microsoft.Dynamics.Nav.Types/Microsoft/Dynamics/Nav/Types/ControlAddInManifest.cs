using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Types;

public class ControlAddInManifest
{
	private List<string> scriptUrls;

	private List<string> styleSheetUrls;

	private List<string> imageResources;

	private List<string> scriptResources;

	private List<string> styleSheetResources;

	public const int NoValue = -1;

	public IList<string> ScriptUrls => scriptUrls ?? (scriptUrls = new List<string>());

	public IList<string> StyleSheetUrls => styleSheetUrls ?? (styleSheetUrls = new List<string>());

	public IList<string> ImageResourceNames => imageResources ?? (imageResources = new List<string>());

	public IList<string> ScriptResourceNames => scriptResources ?? (scriptResources = new List<string>());

	public IList<string> StyleSheetResourceNames => styleSheetResources ?? (styleSheetResources = new List<string>());

	public string Script { get; set; }

	public string RecreateScript { get; set; }

	public string RefreshScript { get; set; }

	public int RequestedHeight { get; set; }

	public int RequestedWidth { get; set; }

	public int MinimumHeight { get; set; }

	public int MinimumWidth { get; set; }

	public int MaximumHeight { get; set; }

	public int MaximumWidth { get; set; }

	public bool VerticalShrink { get; set; }

	public bool HorizontalShrink { get; set; }

	public bool VerticalStretch { get; set; }

	public bool HorizontalStretch { get; set; }

	public bool AllowCaption { get; set; }

	public int Version { get; set; }
}
