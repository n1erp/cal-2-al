using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavXmlPort : NavApplicationObjectBase
{
	private NavControlStatement initializeXmlPort;

	private MetaXmlPort metadata;

	private List<NavXmlPortNode> nodes = new List<NavXmlPortNode>();

	private NavOutStream destination;

	private NavInStream source;

	private NavForm requestOptionsPage;

	private bool useRequestForm;

	private List<NavXmlPortNode> currentExportPath = new List<NavXmlPortNode>();

	private string fileName;

	private NCLMetaXmlPort nclMetaXmlPort;

	private string fieldDelimiter;

	private string fieldSeparator;

	private string recordSeparator;

	private string tableSeparator;

	private TextEncoding textEncoding;

	private bool importFile = true;

	private NCLCaptionStrings captionML;

	private Dictionary<string, NavValue> requestPageVariables;

	public string RootNodeName
	{
		get
		{
			return nodes[0].MetaData.NodeName;
		}
		set
		{
			nodes[0].MetaData.NodeName = value;
		}
	}

	public NavOutStream Destination
	{
		get
		{
			return destination;
		}
		set
		{
			destination = value;
		}
	}

	public NavInStream Source
	{
		get
		{
			return source;
		}
		set
		{
			source = value;
		}
	}

	protected internal NavRecord Rec
	{
		get
		{
			if (RequestOptionsPage != null)
			{
				return RequestOptionsPage.SourceTable;
			}
			return null;
		}
		set
		{
			RequestOptionsPage.SetSourceTable(value, clone: true);
		}
	}

	protected internal NavRecord xRec => RequestOptionsPage.OldRecord;

	public int XmlPortId => ObjectId.ObjectNumber;

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

	public string FieldDelimiter
	{
		get
		{
			return fieldDelimiter;
		}
		set
		{
			fieldDelimiter = value;
		}
	}

	public string FieldSeparator
	{
		get
		{
			return fieldSeparator;
		}
		set
		{
			fieldSeparator = value;
		}
	}

	public string RecordSeparator
	{
		get
		{
			return recordSeparator;
		}
		set
		{
			recordSeparator = value;
		}
	}

	public string TableSeparator
	{
		get
		{
			return tableSeparator;
		}
		set
		{
			tableSeparator = value;
		}
	}

	public TextEncoding TextEncoding
	{
		get
		{
			return textEncoding;
		}
		set
		{
			textEncoding = value;
		}
	}

	public bool ImportFile
	{
		get
		{
			return importFile;
		}
		set
		{
			if (value && metadata.Direction == XmlPortDirection.Export)
			{
				throw NavNCLXmlPortExportDirectionException.Create(CultureInfo.CurrentCulture, metadata.Name);
			}
			if (!value && metadata.Direction == XmlPortDirection.Import)
			{
				throw NavNCLXmlPortImportDirectionException.Create(CultureInfo.CurrentCulture, metadata.Name);
			}
			importFile = value;
		}
	}

	public MetaXmlPort Metadata => metadata;

	public NCLMetaXmlPort NclMetaXmlPort
	{
		get
		{
			if (nclMetaXmlPort == null)
			{
				nclMetaXmlPort = Session.NCLMetadata.GetMetaXmlPortById(ObjectId.ObjectNumber, OriginalAppGroupId);
			}
			return nclMetaXmlPort;
		}
	}

	public XmlSchema Schema => metadata.Schema;

	public XmlQualifiedName SchemaTypeName => metadata.SchemaTypeName;

	public bool UseRequestForm
	{
		get
		{
			return useRequestForm;
		}
		set
		{
			useRequestForm = value;
		}
	}

	internal List<NavXmlPortNode> Nodes => nodes;

	protected NavForm RequestOptionsPage
	{
		get
		{
			return requestOptionsPage;
		}
		set
		{
			if (requestOptionsPage != null)
			{
				requestOptionsPage.ApplyReportOptions -= OnApplyReportOptions;
				requestOptionsPage.GetReportOptions -= OnGetReportOptions;
			}
			requestOptionsPage = value;
			if (requestOptionsPage.SaveValues)
			{
				requestOptionsPage.ApplyReportOptions += OnApplyReportOptions;
				requestOptionsPage.GetReportOptions += OnGetReportOptions;
			}
		}
	}

	protected NavOption RequestDirection
	{
		get
		{
			if (importFile)
			{
				return NavOption.Create(NCLOptionMetadata.Default, 0);
			}
			return NavOption.Create(NCLOptionMetadata.Default, 1);
		}
		set
		{
			if (value == null || value.Value == 0)
			{
				importFile = true;
			}
			else
			{
				importFile = false;
			}
		}
	}

	public override NavNclType NclType => NavNclType.NavXmlPort;

	public override Type HandleType => typeof(NavXmlPortHandle);

	internal override NCLMetaApplicationObject MetaApplicationObject => NclMetaXmlPort;

	public override string ObjectName => NclMetaXmlPort.Name;

	private void RunXmlPort()
	{
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			if (!CallRequestForm())
			{
				return;
			}
			if (string.IsNullOrEmpty(fileName))
			{
				fileName = GetDefaultFileName();
			}
			if (importFile)
			{
				using (NavCurrentThread.Session.Diagnostics.CreateTraceScope("0000052", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Running Import for XmlPort ID {0}", ObjectId)), MonitorType.ALStatementsExecuted, Category.Runtime))
				{
					FileBufferedStream fileBufferedStream = null;
					try
					{
						fileBufferedStream = NavFile.InternalUpload(displayDialog: true, Lang.Import, string.Empty, ref fileName, Lang.XmlPortRunFileDialogFilter, Guid.NewGuid());
						if (fileBufferedStream != null)
						{
							NavInStream navInStream = Source;
							try
							{
								using NavScope parent = new NavScope(NavCurrentThread.Session);
								Source = new NavInStream(parent, new NavStreamProvider(fileBufferedStream));
								Import();
								return;
							}
							finally
							{
								Source = navInStream;
							}
						}
						return;
					}
					catch (Exception ex)
					{
						NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: true);
						throw;
					}
					finally
					{
						fileBufferedStream?.Dispose();
					}
				}
			}
			using (NavCurrentThread.Session.Diagnostics.CreateTraceScope("0000053", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Running Export for XmlPort ID {0}", ObjectId)), MonitorType.ALStatementsExecuted, Category.Runtime))
			{
				NavOutStream navOutStream = null;
				string tempFileName = NavFile.GetTempFileName();
				FileStream stream = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
				try
				{
					using NavScope parent2 = new NavScope(NavCurrentThread.Session);
					navOutStream = Destination;
					Destination = new NavOutStream(parent2, new NavStreamProvider(stream));
					Export();
					try
					{
						NavFile.InternalDownload(displayDialog: true, Lang.Export, string.Empty, ref fileName, Lang.XmlPortRunFileDialogFilter, Destination.InternalStream, Guid.NewGuid());
					}
					catch (Exception ex2)
					{
						NavFileHelper.MapAndRethrowClrException(ex2, writeExceptionToEventLog: true);
						throw;
					}
				}
				finally
				{
					Destination = navOutStream;
				}
			}
		});
	}

	private string GetDefaultFileName()
	{
		string arg = ((metadata.Format != XmlPortNodeFormat.Xml) ? "txt" : "xml");
		StringBuilder stringBuilder = new StringBuilder(metadata.Name);
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char oldChar in invalidFileNameChars)
		{
			stringBuilder.Replace(oldChar, '_');
		}
		return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", stringBuilder.ToString(), arg);
	}

	private bool CallRequestForm()
	{
		bool commit = false;
		NavCurrentThread.Session.BeginTransaction();
		ALDatabase.ALCurrentTransactionType = metadata.TransactionType;
		try
		{
			if (useRequestForm && RequestOptionsPage != null)
			{
				requestPageVariables = null;
				RequestOptionsPage.OnSaveValues += (object sender, Dictionary<string, NavValue> values) =>
				{
					requestPageVariables = values;
				};
				FormResult formResult = requestOptionsPage.RunModal();
				if (formResult == FormResult.Cancel)
				{
					return false;
				}
				commit = true;
			}
			return true;
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(commit);
		}
	}

	private void ApplyNodeSourceTableViewAndRequestFormFilters()
	{
		foreach (NavXmlPortNode node in nodes)
		{
			if (node.MetaData.SourceType != XmlPortNodeSourceType.Table)
			{
				continue;
			}
			NavXmlPortTableNode navXmlPortTableNode = node as NavXmlPortTableNode;
			if (!(navXmlPortTableNode.MetaData is MetaXmlPortTableNode metaXmlPortTableNode))
			{
				throw new NavNCLXmlPortMetadataException();
			}
			if (!string.IsNullOrEmpty(metaXmlPortTableNode.SourceTableView))
			{
				NavTableView navTableView = new NavTableView();
				navXmlPortTableNode.SourceTableViewRecord.SecurityFiltering = navXmlPortTableNode.Record.SecurityFiltering;
				NavTableView tableView;
				using (NavRecord navRecord = new NavRecord(navXmlPortTableNode.Record.Session, navXmlPortTableNode.Record.TableID, navXmlPortTableNode.Record.SecurityFiltering))
				{
					navTableView = navXmlPortTableNode.SourceTableViewRecord.GetTableView();
					navRecord.ALSetView(metaXmlPortTableNode.SourceTableView);
					tableView = navRecord.GetTableView();
				}
				if (tableView.Ascending != navTableView.Ascending)
				{
					navXmlPortTableNode.Record.ALAscending = tableView.Ascending;
				}
				if (!tableView.CompareCurrentSortingFieldIds(navTableView.CurrentSortingFieldIds))
				{
					navXmlPortTableNode.Record.SetCurrentKey(tableView);
				}
			}
			if (navXmlPortTableNode.SourceTableViewRecord.ALGetFilters == navXmlPortTableNode.Record.ALGetFilters)
			{
				navXmlPortTableNode.Record.RecordImplementation.CopyFilters(navXmlPortTableNode.SourceTableViewRecord);
			}
			navXmlPortTableNode.SourceTableViewRecord.RecordImplementation.CopyFiltersSortingMarkList(navXmlPortTableNode.Record);
		}
	}

	private NavControlStatement FireOnInitXmlPort()
	{
		bool commit = false;
		NavControlStatement result = NavControlStatement.None;
		NavCurrentThread.Session.BeginTransaction();
		ALDatabase.ALCurrentTransactionType = metadata.TransactionType;
		try
		{
			OnInitXMLport();
			commit = true;
		}
		catch (NavControlException ex)
		{
			commit = true;
			result = ex.ControlStatement;
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(commit);
		}
		return result;
	}

	private void FindNodeChildren(NavXmlPortNode node)
	{
		node.ElementChildren.Clear();
		node.AttributeChildren.Clear();
		int num = nodes.FindIndex((NavXmlPortNode nodeTagName) => nodeTagName.MetaData.Id == node.MetaData.Id);
		for (int num2 = num + 1; num2 < nodes.Count && nodes[num2].MetaData.Indentation > node.MetaData.Indentation; num2++)
		{
			if (nodes[num2].MetaData.Indentation > node.MetaData.Indentation && nodes[num2].MetaData.Indentation == node.MetaData.Indentation + 1)
			{
				if (nodes[num2].MetaData.NodeType == XmlPortNodeType.Element)
				{
					node.ElementChildren.Add(nodes[num2]);
				}
				else
				{
					node.AttributeChildren.Add(nodes[num2]);
				}
			}
		}
	}

	private void FindSourceField(NavXmlPortFieldNode fieldNode)
	{
		if (!(fieldNode.MetaData is MetaXmlPortFieldNode metaXmlPortFieldNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		fieldNode.SourceFieldName = metaXmlPortFieldNode.SourceFieldName;
		foreach (NavXmlPortNode node in nodes)
		{
			if (node.MetaData.SourceType == XmlPortNodeSourceType.Table)
			{
				if (!(node.MetaData is MetaXmlPortTableNode metaXmlPortTableNode))
				{
					throw new NavNCLXmlPortMetadataException();
				}
				if (metaXmlPortTableNode.VariableName == metaXmlPortFieldNode.SourceTableName)
				{
					fieldNode.SourceFieldTableNode = node as NavXmlPortTableNode;
					break;
				}
			}
		}
	}

	private static void ValidateAndFindCalcFields(NavXmlPortFieldNode fieldNode)
	{
		if (!(fieldNode.MetaData is MetaXmlPortFieldNode metaXmlPortFieldNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		if (fieldNode.SourceFieldTableNode == null)
		{
			throw new NavNCLXmlPortMetadataException();
		}
		NCLMetaField fieldByName = fieldNode.SourceFieldTableNode.Record.MetaTable.GetFieldByName(fieldNode.SourceFieldName);
		ObsolescenceGuard.ThrowIfObsoleted(fieldByName);
		if (metaXmlPortFieldNode.AutoCalcField && fieldByName.FieldClass == FieldClass.FlowField)
		{
			fieldNode.CalcField = true;
		}
	}

	private void FindTableNodeLinkTable(NavXmlPortTableNode tableNode)
	{
		foreach (NavXmlPortNode node in nodes)
		{
			if (node.MetaData.SourceType != XmlPortNodeSourceType.Table)
			{
				continue;
			}
			MetaXmlPortTableNode metaXmlPortTableNode = tableNode.MetaData as MetaXmlPortTableNode;
			MetaXmlPortTableNode metaXmlPortTableNode2 = node.MetaData as MetaXmlPortTableNode;
			if (metaXmlPortTableNode == null || metaXmlPortTableNode2 == null)
			{
				throw new NavNCLXmlPortMetadataException();
			}
			if (metaXmlPortTableNode2.VariableName == metaXmlPortTableNode.LinkTable)
			{
				tableNode.LinkTableNode = node as NavXmlPortTableNode;
				if (tableNode.LinkTableNode == null)
				{
					throw new NavNCLXmlPortMetadataException();
				}
				break;
			}
		}
	}

	private void FindNodeTableParent(NavXmlPortNode node)
	{
		node.NodeTableParent = null;
		int num = nodes.FindIndex((NavXmlPortNode nodeGuid) => nodeGuid.MetaData.Id == node.MetaData.Id);
		for (int num2 = num; num2 != -1; num2--)
		{
			if (nodes[num2].MetaData.Indentation < node.MetaData.Indentation && nodes[num2].MetaData.SourceType == XmlPortNodeSourceType.Table)
			{
				node.NodeTableParent = nodes[num2];
				break;
			}
		}
	}

	private bool WriteContentToStream()
	{
		return (metadata.Format switch
		{
			XmlPortNodeFormat.Xml => new NavXmlPortExporter(this, GetXmlWriterSettings()), 
			XmlPortNodeFormat.VariableText => new NavXmlPortExporter(this), 
			XmlPortNodeFormat.FixedText => new NavXmlPortExporter(this), 
			_ => throw new NavNCLXmlPortMetadataException(), 
		}).Export();
	}

	private Encoding GetEncoding()
	{
		return metadata.Encoding switch
		{
			XmlPortEncoding.UTF16 => Encoding.Unicode, 
			XmlPortEncoding.UTF8 => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 
			XmlPortEncoding.ISO88592 => Encoding.GetEncoding(28592), 
			_ => Encoding.Unicode, 
		};
	}

	private static TextEncoding GetTextEncoding(XmlPortTextEncoding encoding)
	{
		return encoding switch
		{
			XmlPortTextEncoding.MSDos => TextEncoding.MSDos, 
			XmlPortTextEncoding.UTF16 => TextEncoding.UTF16, 
			XmlPortTextEncoding.UTF8 => TextEncoding.UTF8, 
			XmlPortTextEncoding.Windows => TextEncoding.Windows, 
			_ => TextEncoding.UTF16, 
		};
	}

	internal XmlWriterSettings GetXmlWriterSettings()
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Encoding = GetEncoding();
		xmlWriterSettings.Indent = true;
		xmlWriterSettings.CheckCharacters = false;
		xmlWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
		return xmlWriterSettings;
	}

	internal static void WriteStartDocument(XmlWriter xmlWriter)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "version=\"1.0\" encoding=\"{0}\" standalone=\"no\"", xmlWriter.Settings.Encoding.WebName.ToUpper(CultureInfo.InvariantCulture));
		xmlWriter.WriteProcessingInstruction("xml", text);
	}

	internal void AddNodeToPath(NavXmlPortNode node)
	{
		currentExportPath.Add(node);
	}

	internal void RemoveNodeFromPath()
	{
		currentExportPath.RemoveAt(currentExportPath.Count - 1);
	}

	protected NavXmlPort(ITreeObject parent, int objectId)
		: base(parent, new ApplicationObjectId(ObjectType.XmlPort, objectId))
	{
	}

	protected NavXmlPort(ITreeObject parent, int objectId, NCLMetaXmlPort metaXmlPort)
		: base(parent, new ApplicationObjectId(ObjectType.XmlPort, objectId))
	{
		nclMetaXmlPort = metaXmlPort;
	}

	protected virtual void OnInitXMLport()
	{
	}

	protected internal virtual void OnPreXMLport()
	{
	}

	protected internal virtual void OnPostXMLport()
	{
	}

	protected void Add(NavXmlPortTableNode node)
	{
		if (metadata.Nodes[nodes.Count] is MetaXmlPortTableNode)
		{
			node.MetaData = metadata.Nodes[nodes.Count] as MetaXmlPortTableNode;
			nodes.Add(node);
			return;
		}
		throw new NavNCLXmlPortMetadataException();
	}

	protected void Add(NavXmlPortFieldNode node)
	{
		if (metadata.Nodes[nodes.Count] is MetaXmlPortFieldNode)
		{
			node.MetaData = metadata.Nodes[nodes.Count] as MetaXmlPortFieldNode;
			nodes.Add(node);
			return;
		}
		throw new NavNCLXmlPortMetadataException();
	}

	protected void Add(NavXmlPortTextNode node)
	{
		if (metadata.Nodes[nodes.Count] is MetaXmlPortTextNode)
		{
			node.MetaData = metadata.Nodes[nodes.Count] as MetaXmlPortTextNode;
			nodes.Add(node);
			return;
		}
		throw new NavNCLXmlPortMetadataException();
	}

	protected void Break()
	{
		throw new NavControlException(NavControlStatement.Break);
	}

	protected void Skip()
	{
		throw new NavControlException(NavControlStatement.Skip);
	}

	protected void Quit()
	{
		throw new NavControlException(NavControlStatement.Quit);
	}

	protected void BreakUnbound()
	{
		throw new NavControlException(NavControlStatement.BreakUnbound);
	}

	protected void BeginInitialization()
	{
		NavCurrentThread.Session.VerifyExecutePermission(this);
		metadata = Tree.Session.MetadataProvider.GetXmlPortMetadata(NclMetaXmlPort);
		fileName = metadata.FileName;
		fieldDelimiter = metadata.FieldDelimiter;
		fieldSeparator = metadata.FieldSeparator;
		recordSeparator = metadata.RecordSeparator;
		tableSeparator = metadata.TableSeparator;
		textEncoding = GetTextEncoding(metadata.TextEncoding);
		captionML = new NCLCaptionStrings(Metadata.CaptionML, Metadata.Name);
		importFile = metadata.Direction != XmlPortDirection.Export;
	}

	protected void EndInitialization()
	{
		new TreeObjectReference(this, RequestOptionsPage);
		useRequestForm = metadata.UseRequestForm;
		foreach (NavXmlPortNode node in nodes)
		{
			FindNodeChildren(node);
			FindNodeTableParent(node);
			if (node.MetaData.SourceType == XmlPortNodeSourceType.Table)
			{
				if (!(node is NavXmlPortTableNode tableNode))
				{
					throw new NavNCLXmlPortMetadataException();
				}
				FindTableNodeLinkTable(tableNode);
			}
			if (node.MetaData.SourceType == XmlPortNodeSourceType.Field)
			{
				if (!(node is NavXmlPortFieldNode fieldNode))
				{
					throw new NavNCLXmlPortMetadataException();
				}
				FindSourceField(fieldNode);
				ValidateAndFindCalcFields(fieldNode);
			}
		}
		initializeXmlPort = FireOnInitXmlPort();
	}

	protected abstract object GetTextVariableValue(string node);

	protected abstract void SetTextVariableValue(string node, string value);

	public string CurrentPath()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (NavXmlPortNode item in currentExportPath)
		{
			if (item.MetaData.NodeType == XmlPortNodeType.Attribute)
			{
				stringBuilder.Append("[@");
				stringBuilder.Append(item.MetaData.NodeName);
				stringBuilder.Append(']');
			}
			else
			{
				stringBuilder.Append('/');
				stringBuilder.Append(item.MetaData.NodeName);
			}
		}
		return stringBuilder.ToString();
	}

	public string ObjectID()
	{
		return ObjectID(useCaption: true);
	}

	public string ObjectID(bool useCaption)
	{
		return string.Format(CultureInfo.InvariantCulture, "XmlPort {0}", useCaption ? captionML.GetValueOrDefault() : XmlPortId.ToString(CultureInfo.InvariantCulture));
	}

	public void SetTableView(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		foreach (NavXmlPortNode node in nodes)
		{
			if (node.MetaData.SourceType == XmlPortNodeSourceType.Table)
			{
				if (!(node is NavXmlPortTableNode navXmlPortTableNode))
				{
					throw new NavNCLXmlPortMetadataException();
				}
				navXmlPortTableNode.SourceTableViewRecord.SecurityFiltering = navXmlPortTableNode.Record.SecurityFiltering;
				if (navXmlPortTableNode.Record.TableID == record.TableID)
				{
					navXmlPortTableNode.Record.RecordImplementation.CopyFiltersSortingMarkList(record);
					navXmlPortTableNode.SourceTableViewRecord.RecordImplementation.CopyFiltersSortingMarkList(record);
					return;
				}
			}
		}
		throw NavNCLXmlPortNodeNotFoundException.CreateSetTableView(CultureInfo.CurrentCulture, metadata.Name, record.TableID, record.ALTableName);
	}

	public bool Import()
	{
		return Import(DataError.ThrowError);
	}

	public bool Import(DataError errorLevel)
	{
		if (initializeXmlPort == NavControlStatement.Quit)
		{
			return true;
		}
		ApplyNodeSourceTableViewAndRequestFormFilters();
		bool commit = false;
		NavSession navSession = NavCurrentThread.Session;
		NavInStream navInStream = null;
		using (NavScope scope = new NavScope(NavCurrentThread.Session))
		{
			using (new NavOpenDialogTracking())
			{
				if (metadata.Direction == XmlPortDirection.Export)
				{
					throw NavNCLXmlPortExportDirectionException.Create(CultureInfo.CurrentCulture, metadata.Name);
				}
				if (errorLevel == DataError.ThrowError)
				{
					if (source == null)
					{
						throw new ArgumentNullException("Source");
					}
					if (Source.Length == 0L)
					{
						throw new NavNCLXmlPortDataException(Lang.XmlPortImportFileIsEmpty);
					}
					navSession.BeginTransaction();
					ALDatabase.ALCurrentTransactionType = metadata.TransactionType;
					try
					{
						navInStream = DoImport(scope);
						commit = true;
					}
					catch (NavControlException ex)
					{
						if (ex.ControlStatement != NavControlStatement.Quit)
						{
							commit = true;
						}
					}
					finally
					{
						if (navInStream != null)
						{
							Source = navInStream;
						}
						navSession.EndTransaction(commit);
					}
				}
				else
				{
					if (source == null)
					{
						return false;
					}
					navSession.BeginTransactionWorldAndTransaction();
					ALDatabase.ALCurrentTransactionType = metadata.TransactionType;
					try
					{
						if (Source.Length == 0L)
						{
							return false;
						}
						navInStream = DoImport(scope);
						Session.DataAccessSource.SessionTransactionManager.FlushBufferedWrites();
						commit = true;
					}
					catch (NavControlException ex2)
					{
						if (ex2.ControlStatement != NavControlStatement.Quit)
						{
							commit = true;
						}
					}
					catch (NavBaseException)
					{
						return false;
					}
					finally
					{
						if (navInStream != null)
						{
							Source = navInStream;
						}
						navSession.EndTransactionWorldAndTransaction(commit);
					}
				}
			}
		}
		return true;
	}

	private NavInStream DoImport(NavScope scope)
	{
		NavInStream tmpSource = null;
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			OnPreXMLport();
			if (Source.Length == 0L)
			{
				throw new NavNCLXmlPortDataException(Lang.XmlPortImportFileIsEmpty);
			}
			XmlPortNodeFormat format = metadata.Format;
			if ((uint)(format - 1) <= 1u)
			{
				if (nodes[0].MetaData.SourceType != XmlPortNodeSourceType.Text)
				{
					throw new NavNCLXmlPortInvalidNodeTypeException(Lang._00095_50124);
				}
				tmpSource = Source;
				NavXmlPortTextReader navXmlPortTextReader = new NavXmlPortTextReader(this);
				navXmlPortTextReader.ConvertTextToXml(scope);
			}
			NavXmlPortImporter navXmlPortImporter = new NavXmlPortImporter(this);
			navXmlPortImporter.Import();
			OnPostXMLport();
		});
		return tmpSource;
	}

	public static bool Import(int xmlPortId, NavInStream navInStream)
	{
		return Import(DataError.ThrowError, xmlPortId, navInStream, null);
	}

	public static bool Import(DataError errorLevel, int xmlPortId, NavInStream navInStream)
	{
		return Import(errorLevel, xmlPortId, navInStream, null);
	}

	public static bool Import(int xmlPortId, NavInStream navInStream, NavRecord record)
	{
		return Import(DataError.ThrowError, xmlPortId, navInStream, record);
	}

	public static bool Import(DataError errorLevel, int xmlPortId, NavInStream navInStream, NavRecord record)
	{
		try
		{
			using NavXmlPort navXmlPort = NavGlobal.NCLMetadata.GetMetaXmlPortById(xmlPortId).CreateObjectInstance(NavCurrentThread.Session);
			if (record != null)
			{
				navXmlPort.SetTableView(record);
			}
			navXmlPort.Source = navInStream;
			return navXmlPort.Import(errorLevel);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public bool Export()
	{
		return Export(DataError.ThrowError);
	}

	public bool Export(DataError errorLevel)
	{
		if (initializeXmlPort == NavControlStatement.Quit)
		{
			return true;
		}
		try
		{
			ValidateNamesBeforeExport(Nodes);
		}
		catch (NavNCLXmlPortNodeProcessException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		return ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			ApplyNodeSourceTableViewAndRequestFormFilters();
			currentExportPath.Clear();
			bool commit = false;
			NavCurrentThread.Session.BeginTransaction();
			ALDatabase.ALCurrentTransactionType = metadata.TransactionType;
			bool flag = false;
			try
			{
				using (new NavOpenDialogTracking())
				{
					if (metadata.Direction == XmlPortDirection.Import)
					{
						throw NavNCLXmlPortImportDirectionException.Create(CultureInfo.CurrentCulture, metadata.Name);
					}
					if (destination == null)
					{
						destination = new NavOutStream(this, new NavStreamProvider(Stream.Null));
						flag = true;
					}
					if (WriteContentToStream())
					{
						OnPostXMLport();
					}
					commit = true;
				}
			}
			catch (NavControlException)
			{
				commit = true;
			}
			catch (NavBaseException) when (errorLevel == DataError.TrapError)
			{
				return false;
			}
			finally
			{
				NavCurrentThread.Session.EndTransaction(commit);
				if (flag)
				{
					destination.InternalStream.Close();
					destination = null;
				}
			}
			return true;
		});
	}

	public static bool Export(int xmlPortId, NavOutStream outStream)
	{
		return Export(DataError.ThrowError, xmlPortId, outStream, null);
	}

	public static bool Export(DataError errorLevel, int xmlPortId, NavOutStream outStream)
	{
		return Export(errorLevel, xmlPortId, outStream, null);
	}

	public static bool Export(int xmlPortId, NavOutStream outStream, NavRecord record)
	{
		return Export(DataError.ThrowError, xmlPortId, outStream, record);
	}

	public static bool Export(DataError errorLevel, int xmlPortId, NavOutStream outStream, NavRecord record)
	{
		bool flag = outStream == null;
		try
		{
			if (flag)
			{
				outStream = new NavOutStream(NavCurrentThread.Session, new NavStreamProvider(Stream.Null));
			}
			using NavXmlPort navXmlPort = NavGlobal.NCLMetadata.GetMetaXmlPortById(xmlPortId).CreateObjectInstance(NavCurrentThread.Session);
			navXmlPort.ValidateNamesBeforeExport(navXmlPort.Nodes);
			if (record != null)
			{
				navXmlPort.SetTableView(record);
			}
			navXmlPort.destination = outStream;
			return navXmlPort.Export();
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		finally
		{
			if (flag)
			{
				outStream?.Dispose();
			}
		}
	}

	public void Run()
	{
		RunXmlPort();
	}

	public static void Run(int xmlPortId)
	{
		using NavXmlPort navXmlPort = NavGlobal.NCLMetadata.GetMetaXmlPortById(xmlPortId).CreateObjectInstance(NavCurrentThread.Session);
		if (navXmlPort.metadata.Direction == XmlPortDirection.Export)
		{
			navXmlPort.ImportFile = false;
		}
		navXmlPort.RunXmlPort();
	}

	public static void Run(int xmlPortId, bool requestWindow)
	{
		using NavXmlPort navXmlPort = NavGlobal.NCLMetadata.GetMetaXmlPortById(xmlPortId).CreateObjectInstance(NavCurrentThread.Session);
		if (navXmlPort.metadata.Direction == XmlPortDirection.Export)
		{
			navXmlPort.ImportFile = false;
		}
		navXmlPort.useRequestForm = requestWindow;
		navXmlPort.RunXmlPort();
	}

	public static void Run(int xmlPortId, bool requestWindow, bool import)
	{
		Run(xmlPortId, requestWindow, import, null);
	}

	public static void Run(int xmlPortId, bool requestWindow, bool import, NavRecord record)
	{
		using NavXmlPort navXmlPort = NavGlobal.NCLMetadata.GetMetaXmlPortById(xmlPortId).CreateObjectInstance(NavCurrentThread.Session);
		navXmlPort.useRequestForm = requestWindow;
		if (record != null)
		{
			navXmlPort.SetTableView(record);
		}
		navXmlPort.ImportFile = import;
		navXmlPort.RunXmlPort();
	}

	internal object GetTextVariable(string node)
	{
		return GetTextVariableValue(node);
	}

	internal void SetTextVariable(string node, string value)
	{
		SetTextVariableValue(node, value);
	}

	public override int GetHashCode()
	{
		return XmlPortId;
	}

	private void ValidateNamesBeforeExport(IEnumerable<NavXmlPortNode> nodesToValidate)
	{
		if (Metadata.Format != XmlPortNodeFormat.Xml)
		{
			return;
		}
		foreach (NavXmlPortNode item in nodesToValidate)
		{
			if (MaxOccurs(item.MetaData.NodeName, ':', 1))
			{
				throw new NavNCLXmlPortNodeProcessException(string.Format(CultureInfo.CurrentCulture, Lang.XmlMultipleColonsNotSupported, item.MetaData.NodeName));
			}
			ValidateNamesBeforeExport(item.AttributeChildren);
			ValidateNamesBeforeExport(item.ElementChildren);
		}
	}

	private static bool MaxOccurs(string text, char value, int maxCount)
	{
		int num = 0;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == value)
			{
				num++;
			}
			if (num > maxCount)
			{
				return true;
			}
		}
		return false;
	}

	public override NavComplexValue CreateHandle(ITreeObject parent)
	{
		NavXmlPortHandle navXmlPortHandle = new NavXmlPortHandle(parent, ObjectId.ObjectNumber);
		navXmlPortHandle.Target = this;
		return navXmlPortHandle;
	}

	private void OnGetReportOptions(object sender, GetReportOptionsEventArgs reportOptionsEventArgs)
	{
		reportOptionsEventArgs.ObjectType = ObjectType.XmlPort;
		IEnumerable<(string, string)> dataItems = from NavXmlPortTableNode node in nodes.Where((NavXmlPortNode n) => n.MetaData.SourceType == XmlPortNodeSourceType.Table)
			select (NodeName: node.MetaData.NodeName, node.Record.ALGetView(useCaptions: false));
		reportOptionsEventArgs.ReportOptions = RequestPageParameterHelper.GetReportParameterXml(ObjectName, ObjectId.ObjectNumber, requestPageVariables, dataItems);
	}

	private void OnApplyReportOptions(object sender, EventArgs<string> parameters)
	{
		NavObjectOptions navObjectOptions = new NavObjectOptions(ObjectType.XmlPort, ObjectId.ObjectNumber, parameters.Data);
		string parameters2 = navObjectOptions.Load();
		Dictionary<string, NavValue> variables = RequestPageParameterHelper.ConvertXmlToVariables(parameters2, RequestOptionsPage);
		RequestOptionsPage.ApplyLatestValues(variables);
		RequestPageParameterHelper.ApplyXmlViews(parameters2, ((string name, string view) args) =>
		{
			nodes.Where((NavXmlPortNode n) => n.MetaData.SourceType == XmlPortNodeSourceType.Table && n.MetaData.NodeName == args.name).Cast<NavXmlPortTableNode>().FirstOrDefault()?.Record.ALSetView(args.view);
		});
	}
}
