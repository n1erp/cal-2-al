using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

[CLSCompliant(true)]
internal class ReportChange
{
	internal const string EnumValueNone = "None";

	public string SourceName { get; protected set; }

	public string TargetName { get; protected set; }

	public string SourceParentPath { get; }

	public string TargetParentPath { get; }

	public ReportChangeOptions.DeleteAction ActionOnDelete { get; set; }

	public ReportChangeOptions.ElementType ItemType { get; }

	public string ReplacementText { get; set; }

	public ReportChangeOptions.ChangeAction ActionOnChange { get; set; }

	public ReportChange(string id, string sourceName, string targetName, string sourcePath, string targetPath, ReportChangeOptions.ElementType itemType, string dataType = "None")
	{
		SourceName = sourceName;
		TargetName = targetName;
		SourceParentPath = ((itemType != ReportChangeOptions.ElementType.Label) ? sourcePath : (string.IsNullOrEmpty(sourcePath) ? "/Labels" : sourcePath));
		TargetParentPath = targetPath;
		ItemType = itemType;
		ActionOnChange = ((!string.IsNullOrEmpty(targetName)) ? ReportChangeOptions.ChangeAction.Rename : ReportChangeOptions.ChangeAction.Delete);
	}

	public ReportChange(ReportChange changeInterface)
	{
		if (changeInterface != null)
		{
			SourceName = changeInterface.SourceName;
			TargetName = changeInterface.TargetName;
			SourceParentPath = changeInterface.SourceParentPath;
			TargetParentPath = changeInterface.TargetParentPath;
			ActionOnDelete = changeInterface.ActionOnDelete;
			ItemType = changeInterface.ItemType;
			ReplacementText = changeInterface.ReplacementText;
			ActionOnChange = changeInterface.ActionOnChange;
		}
	}
}
