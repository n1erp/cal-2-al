using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class PageDependencyResult
{
	private List<PageResponseDependencyEntry> masterPages = new List<PageResponseDependencyEntry>();

	private List<MetaTable> metaTables = new List<MetaTable>();

	public List<MetaTable> MetaTables => metaTables;

	public List<PageResponseDependencyEntry> MasterPages => masterPages;
}
