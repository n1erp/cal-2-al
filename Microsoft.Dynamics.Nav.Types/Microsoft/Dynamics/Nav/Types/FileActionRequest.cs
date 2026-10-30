using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FileActionRequest : ServerResponse
{
	private FileAction action;

	private bool displayDialog = true;

	private bool displayUserActionDialog = true;

	private bool displayCanOpenFileDialog;

	private string title = string.Empty;

	private string initialFolder = string.Empty;

	private string fileName = string.Empty;

	private string typeFilter = string.Empty;

	private Guid automationId;

	private FileTypeFilter prohibitedFileTypes;

	private FileTypeFilter allowedFileTypes;

	private bool ignoreFileSize;

	[DataMember]
	public FileAction Action
	{
		get
		{
			return action;
		}
		set
		{
			action = value;
		}
	}

	[DataMember]
	public bool DisplayUserActionDialog
	{
		get
		{
			return displayUserActionDialog;
		}
		set
		{
			displayUserActionDialog = value;
		}
	}

	[DataMember]
	public bool DisplayCanOpenFileDialog
	{
		get
		{
			return displayCanOpenFileDialog;
		}
		set
		{
			displayCanOpenFileDialog = value;
		}
	}

	[DataMember]
	public bool DisplayDialog
	{
		get
		{
			return displayDialog;
		}
		set
		{
			displayDialog = value;
		}
	}

	[DataMember]
	public string Title
	{
		get
		{
			return title;
		}
		set
		{
			title = value;
		}
	}

	[DataMember]
	public string InitialFolder
	{
		get
		{
			return initialFolder;
		}
		set
		{
			initialFolder = value;
		}
	}

	[DataMember]
	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}

	[DataMember]
	public string TypeFilter
	{
		get
		{
			return typeFilter;
		}
		set
		{
			typeFilter = value;
		}
	}

	[DataMember]
	public Guid AutomationId
	{
		get
		{
			return automationId;
		}
		set
		{
			automationId = value;
		}
	}

	[DataMember]
	public bool IgnoreFileSize
	{
		get
		{
			return ignoreFileSize;
		}
		set
		{
			ignoreFileSize = value;
		}
	}

	[DataMember]
	public string ProhibitedFileTypes
	{
		get
		{
			if (!(prohibitedFileTypes == null))
			{
				return prohibitedFileTypes.Value;
			}
			return string.Empty;
		}
		set
		{
			if (!FileTypeFilter.TryParse(value, out var result))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			prohibitedFileTypes = result;
		}
	}

	[DataMember]
	public string AllowedFileTypes
	{
		get
		{
			if (!(allowedFileTypes == null))
			{
				return allowedFileTypes.Value;
			}
			return string.Empty;
		}
		set
		{
			if (!FileTypeFilter.TryParse(value, out var result))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			allowedFileTypes = result;
		}
	}

	public bool IsFileTypeAllowed(string filePath)
	{
		return FileTypeFilter.IsFileTypeAllowedByConfiguration(filePath, allowedFileTypes, prohibitedFileTypes);
	}
}
