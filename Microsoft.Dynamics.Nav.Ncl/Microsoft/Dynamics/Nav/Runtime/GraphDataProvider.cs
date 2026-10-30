#define TRACE
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Web;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Data.Graph;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class GraphDataProvider : ExternalDataProvider
{
	private const string ActionGet = "GET";

	private const string ActionDelete = "DELETE";

	private const string ActionPatch = "PATCH";

	private const string ActionPost = "POST";

	public const string ColumnNameDeltaToken = "DeltaToken";

	public const string RelativeExternalNamePathIndicator = "/";

	private const string DeltaTokenKey = "$deltaToken";

	private const string ExpandTokenKey = "$expand";

	private const string OdataDeltaLinkFieldName = "@odata.deltaLink";

	private const string OdataNextLinkFieldName = "@odata.nextLink";

	private const string OdataValueFieldName = "value";

	private const string MessageFieldName = "message";

	private const string ErrorFieldName = "error";

	private const string ContentTypeJson = "application/json";

	private const string ExternalEtagName = "@odata.etag";

	internal const string IfMatchKey = "If-Match";

	public const string ClientRequestIdHeaderName = "client-request-id";

	public const string ReturnClientRequestIdHeaderName = "return-client-request-id";

	private const int BlobChunkSize = 28672;

	private const int GraphSubscriptionTableId = 5455;

	private const string GraphSubscriptionResourceFieldExternalName = "Resource";

	private readonly int deltaTokenColumnIndex;

	private readonly int eTagColumnIndex;

	private readonly IGraphTableConnection connection;

	private readonly IDictionary<string, int> fieldIndexByExternalName;

	private readonly string openTypeExtensionQuery;

	private readonly string singleValueExtendedPropertiesQuery;

	private const string Slash = "/";

	private readonly IList<string> openTypeExtensionIds;

	private readonly IList<string> singleValueExtendedPropertiesIds;

	private readonly JsonSerializer serializer = new JsonSerializer();

	private readonly Func<HttpWebRequest, NavDiagnostics, Stream> getResponseStreamAction;

	private readonly Func<HttpWebRequest, Stream> getRequestStreamAction;

	private readonly Func<HttpWebRequest, long> getRequestContentLengthAction;

	private NCLMetaField PrimaryKeyField => MetaTable.GetFieldByIndex(MetaTable.PrimaryKey.DefinedKeyFields.First().ColumnIndex);

	public override bool SupportReversedLoading => false;

	internal GraphDataProvider(Func<IGraphTableConnection> connectionProvider, NCLMetaTable table, Func<HttpWebRequest, long> getRequestContentLength = null, Func<HttpWebRequest, NavDiagnostics, Stream> getResponseStream = null, Func<HttpWebRequest, Stream> getRequestStream = null)
		: base(table)
	{
		connection = connectionProvider();
		connection.Diagnostics.SendTraceTag("00000CQ", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Creates a GraphDataProvider for TableName={0}, ExternalName={1}", table.TableName, table.ExternalName);
		PopulateLookups(connection, MetaTable, out openTypeExtensionIds, out singleValueExtendedPropertiesIds, out fieldIndexByExternalName);
		if (!fieldIndexByExternalName.TryGetValue("DeltaToken", out deltaTokenColumnIndex))
		{
			deltaTokenColumnIndex = -1;
		}
		if (!fieldIndexByExternalName.TryGetValue("@odata.etag", out eTagColumnIndex))
		{
			eTagColumnIndex = -1;
		}
		singleValueExtendedPropertiesQuery = ExtensionsHelper.GetExpandedExtensionQuery(singleValueExtendedPropertiesIds, ExtensionsHelper.ExtensionType.SingleValueExtendedProperties);
		openTypeExtensionQuery = ExtensionsHelper.GetExpandedExtensionQuery(openTypeExtensionIds, ExtensionsHelper.ExtensionType.OpenTypeExtension);
		serializer.Converters.Add(new RecordBufferListJsonConverter());
		serializer.Converters.Add(new RecordBufferJsonConverter(MetaTable, connection.Diagnostics));
		getResponseStreamAction = getResponseStream ?? new Func<HttpWebRequest, NavDiagnostics, Stream>(DefaultGetResponseStreamFromRequestAction);
		getRequestStreamAction = getRequestStream ?? new Func<HttpWebRequest, Stream>(DefaultGetRequestStreamAction);
		getRequestContentLengthAction = getRequestContentLength ?? new Func<HttpWebRequest, long>(DefaultGetRequestContentLength);
	}

	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		using (connection.Diagnostics.CreateTraceScope("00000L1", DiagnosticParameter.SystemMetadata("GraphDataProvider.TryGetByPrimaryKey"), MonitorType.None, Category.Runtime))
		{
			recordBuffer = null;
			string idFromRecordId = GetIdFromRecordId(request.RecordId);
			if (string.IsNullOrEmpty(idFromRecordId))
			{
				return false;
			}
			try
			{
				GetByPrimaryKey(idFromRecordId, out recordBuffer);
				connection.Diagnostics.SendTraceTag("00000D6", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Graph - try get by primary key called on id {0}.", idFromRecordId);
			}
			catch (NavCSideRecordNotFoundException)
			{
				connection.Diagnostics.SendTraceTag("00000EO", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Graph - try get by primary key called on id {0}, but was not found.", idFromRecordId);
			}
			catch (NavGraphException)
			{
				connection.Diagnostics.SendTraceTag("00000D5", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Graph - try get by primary key called on id {0}, but was not found.", idFromRecordId);
				return false;
			}
			return recordBuffer != null;
		}
	}

	public override int Count(DataProviderRequest request)
	{
		using (connection.Diagnostics.CreateTraceScope("00000L2", DiagnosticParameter.SystemMetadata("GraphDataProvider.Count"), MonitorType.None, Category.Runtime))
		{
			string resourceUriCount = GetResourceUriCount();
			int count = 0;
			ExecuteRequestWithRetry("GET", resourceUriCount, null, (Stream responsestream) =>
			{
				using StreamReader reader = new StreamReader(responsestream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
				using JsonReader jsonReader = new JsonTextReader(reader);
				int? num = jsonReader.ReadAsInt32();
				if (num.HasValue)
				{
					count = num.Value;
				}
			});
			return count;
		}
	}

	public override ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		string id = GetIdFromRecordBuffer(recordBuffer);
		string etagFromRecordBuffer = GetEtagFromRecordBuffer(recordBuffer);
		string resourceUriFromId = GetResourceUriFromId(id, connection.EntityEndpoint);
		connection.Diagnostics.SendTraceTag("00000D7", Category.Runtime, Verbosity.Verbose, "Graph modify called on record with id {0}.", DiagnosticParameter.Eupi(id));
		using (connection.Diagnostics.CreateTraceScope("00000L3", DiagnosticParameter.SystemMetadata("GraphDataProvider.Modify"), MonitorType.None, Category.Runtime))
		{
			try
			{
				ExecuteRequestWithRetry("PATCH", resourceUriFromId, (HttpWebRequest request) =>
				{
					using (connection.Diagnostics.CreateTraceScope("00000HQ", DiagnosticParameter.SystemMetadata("Preparing Graph modify request"), MonitorType.None, Category.Runtime))
					{
						using Stream stream = getRequestStreamAction(request);
						HandleGraphSubscriptionChanges(recordBuffer);
						SerializeRecordBufferToStream(recordBuffer, stream);
						request.ContentType = "application/json";
						if (getRequestContentLengthAction(request) > 0)
						{
							return true;
						}
						connection.Diagnostics.SendTraceTag("00000EP", Category.Runtime, Verbosity.Verbose, "No change to entity data for record with id {0}", DiagnosticParameter.Eupi(id));
						return false;
					}
				}, null, (WebException webException) =>
				{
					if (webException.Status == WebExceptionStatus.ProtocolError)
					{
						HttpWebResponse obj = webException.Response as HttpWebResponse;
						if (obj != null && obj.StatusCode == HttpStatusCode.NotFound)
						{
							throw new NavCSideRecordNotFoundException(id);
						}
					}
					return false;
				}, 3, etagFromRecordBuffer);
				ProcessExtensions(resourceUriFromId, "PATCH", openTypeExtensionIds, recordBuffer, ExtensionsHelper.ExtensionType.OpenTypeExtension);
				ProcessDeepLinks(resourceUriFromId, "PATCH", recordBuffer);
				GetByPrimaryKey(id, out outputRecordBuffer);
			}
			catch (NavCSideRecordNotFoundException exception)
			{
				outputRecordBuffer = null;
				connection.Diagnostics.SendExceptionTag("000012V", Category.Runtime, exception);
				return ModifyResult.RecordNotFound;
			}
			catch (Exception exception2)
			{
				outputRecordBuffer = null;
				connection.Diagnostics.SendExceptionTag("000012W", Category.Runtime, exception2);
				throw;
			}
			return ModifyResult.Success;
		}
	}

	public override void ModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues)
	{
		throw new NavGraphException(Lang.MethodNotImplemented);
	}

	public override DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction)
	{
		using (connection.Diagnostics.CreateTraceScope("00000L4", DiagnosticParameter.SystemMetadata("GraphDataProvider.Delete"), MonitorType.None, Category.Runtime))
		{
			string idFromRecordBuffer = GetIdFromRecordBuffer(recordBuffer);
			string etagFromRecordBuffer = GetEtagFromRecordBuffer(recordBuffer);
			string resourceUriFromId = GetResourceUriFromId(idFromRecordBuffer, connection.EntityEndpoint);
			connection.Diagnostics.SendTraceTag("00000D8", Category.Runtime, Verbosity.Verbose, "Graph delete called on id {0}.", DiagnosticParameter.Eupi(idFromRecordBuffer));
			ExecuteRequestWithRetry("DELETE", resourceUriFromId, null, null, null, 3, etagFromRecordBuffer);
			return DeleteResult.Success;
		}
	}

	public override void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		throw new NavGraphException(Lang.MethodNotImplemented);
	}

	public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		string text = connection.EntityListEndpoint;
		using (connection.Diagnostics.CreateTraceScope("00000L5", DiagnosticParameter.SystemMetadata("GraphDataProvider.Insert"), MonitorType.None, Category.Runtime))
		{
			try
			{
				ReadOnlyRecordBuffer createdRecord = null;
				ExecuteRequestWithRetry("POST", text, (HttpWebRequest webRequest) =>
				{
					using (connection.Diagnostics.CreateTraceScope("00000HR", DiagnosticParameter.SystemMetadata("Prepare Graph insert request"), MonitorType.None, Category.Runtime))
					{
						using Stream stream = getRequestStreamAction(webRequest);
						HandleGraphSubscriptionChanges(recordBuffer);
						SerializeRecordBufferToStream(recordBuffer, stream);
						webRequest.ContentType = "application/json";
						return true;
					}
				}, (Stream responseStream) =>
				{
					createdRecord = DeserializeSingleRecordFromStream(responseStream);
				}, (WebException ex) => false);
				if (string.IsNullOrWhiteSpace(connection.EntityEndpoint))
				{
					connection.Diagnostics.SendTraceTag("000012M", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "No EntityEndpoint defined, returning result from response.");
					outputRecordBuffer = createdRecord;
					return InsertResult.Success;
				}
				string idFromRecordBuffer = GetIdFromRecordBuffer(createdRecord);
				text = GetResourceUriFromId(idFromRecordBuffer, connection.EntityEndpoint);
				ProcessExtensions(text, "PATCH", openTypeExtensionIds, recordBuffer, ExtensionsHelper.ExtensionType.OpenTypeExtension);
				GetByPrimaryKey(idFromRecordBuffer, out outputRecordBuffer);
			}
			catch (Exception exception)
			{
				outputRecordBuffer = null;
				connection.Diagnostics.SendExceptionTag("000012X", Category.Runtime, exception, Verbosity.Error, DiagnosticParameter.CustomerContent(string.Format(CultureInfo.InvariantCulture, "Graph insert threw exception on resource {0}.", text)));
				throw;
			}
			return InsertResult.Success;
		}
	}

	public override FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request)
	{
		Tuple<INavFieldMetadata, NavValue>[] array = new Tuple<INavFieldMetadata, NavValue>[request.BlobsToGet.Count];
		if (!(request.RecordBuffer is MutableRecordBuffer { ReadOnlyBuffer: var readOnlyBuffer }))
		{
			NCLMetaField fieldByIndex = MetaTable.GetFieldByIndex(MetaTable.Keys.First().DefinedKeyFields.First().ColumnIndex);
			throw RecordImplementationHelper.GetRecordNotFoundException(MetaTable, new Tuple<NCLMetaField, NavValue>[1]
			{
				new Tuple<NCLMetaField, NavValue>(fieldByIndex, request.RecordBuffer[fieldByIndex.FieldIndex])
			});
		}
		for (int i = 0; i < request.BlobsToGet.Count; i++)
		{
			NCLMetaField nCLMetaField = (NCLMetaField)request.BlobsToGet[i];
			if (nCLMetaField.FieldNclType != NavNclType.NavBlob)
			{
				throw new NavGraphException(string.Format(CultureInfo.CurrentCulture, Lang.GraphInvalidFieldType, nCLMetaField.FieldNclType));
			}
			NavBLOB navBLOB = ((NavBLOB)readOnlyBuffer[nCLMetaField.FieldIndex]) ?? new NavBLOB(new byte[0]);
			if (navBLOB.GetLength() == 0 && nCLMetaField.ExternalName.StartsWith("/", StringComparison.Ordinal) && readOnlyBuffer[PrimaryKeyField.FieldIndex] != null)
			{
				string idFromRecordBuffer = GetIdFromRecordBuffer(readOnlyBuffer);
				string resourceUriFromId = GetResourceUriFromId(idFromRecordBuffer, connection.EntityEndpoint);
				string endpoint = resourceUriFromId + nCLMetaField.ExternalName;
				connection.Diagnostics.SendTraceTag("0000131", Category.Runtime, Verbosity.Verbose, "Requesting blob content for record with id {0} and field {1}.", DiagnosticParameter.Eupi(idFromRecordBuffer), DiagnosticParameter.SystemMetadata(nCLMetaField.ExternalName));
				navBLOB = RequestBlob(endpoint);
			}
			NavBLOB item = new NavBLOB(navBLOB.GetBytes());
			array[i] = new Tuple<INavFieldMetadata, NavValue>(nCLMetaField, item);
		}
		return new FieldDictionary<NavValue>(array);
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> FindByPositionImplementation(PositionedFindProviderRequest request)
	{
		using (connection.Diagnostics.CreateTraceScope("00000L6", DiagnosticParameter.SystemMetadata("GraphDataProvider.FindByPositionImplementation"), MonitorType.None, Category.Runtime))
		{
			if (request.StartingPosition != null && !string.IsNullOrEmpty(GetIdFromRecordBuffer(request.StartingPosition)))
			{
				return ReadOnlyRecordBuffer.EmptyArray;
			}
			if (request.FiltersAndMarks != null && request.FiltersAndMarks.Filters != null && request.FiltersAndMarks.Filters.FieldNames.Contains("DeltaToken"))
			{
				if (request.FiltersAndMarks.Filters.FieldNames == "DeltaToken")
				{
					string text = request.FiltersAndMarks.Filters.Items[0].Item2.ToString();
					text = text.Replace("'", string.Empty);
					string text2 = "<>";
					text = text.TrimStart(text2.ToCharArray());
					return ReadRecords(text);
				}
				throw new NavGraphException(Lang.GraphDeltaTokenOnlyFilter);
			}
			return ReadRecords();
		}
	}

	private NavBLOB RequestBlob(string endpoint)
	{
		NavBLOB newBlob = new NavBLOB(new byte[0]);
		ExecuteRequestWithRetry("GET", endpoint, null, (Stream responseStream) =>
		{
			byte[] array = new byte[28672];
			using MemoryStream memoryStream = new MemoryStream();
			int num = 0;
			int valueInBytes = ServerUserSettings.Instance.NavHttpClientMaxResponseContentSize.ValueInBytes;
			int num2;
			while ((num2 = responseStream.Read(array, 0, Math.Min(array.Length, valueInBytes - num))) > 0)
			{
				memoryStream.Write(array, 0, num2);
				num += num2;
			}
			newBlob = new NavBLOB(memoryStream.ToArray());
		}, (WebException webException) =>
		{
			HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;
			if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
			{
				newBlob = new NavBLOB(new byte[0]);
				return true;
			}
			return false;
		});
		return newBlob;
	}

	private IEnumerable<ReadOnlyRecordBuffer> ReadRecords(string requestDeltaToken = null)
	{
		List<ReadOnlyRecordBuffer> foundRecords = new List<ReadOnlyRecordBuffer>();
		string resourceEndpoint = GetResourceUriTrackingChanges(requestDeltaToken);
		while (!string.IsNullOrWhiteSpace(resourceEndpoint))
		{
			ExecuteRequestWithRetry("GET", resourceEndpoint, (HttpWebRequest webRequest) =>
			{
				webRequest.Headers.Add("Prefer", "odata.track-changes");
				if (connection.MaxPageSize > 0)
				{
					webRequest.Headers.Add("Prefer", "odata.maxpagesize=" + connection.MaxPageSize);
				}
				return true;
			}, (Stream responseStream) =>
			{
				resourceEndpoint = null;
				using StreamReader reader = new StreamReader(responseStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
				using JsonReader jsonReader = new JsonTextReader(reader);
				jsonReader.SupportMultipleContent = true;
				while (jsonReader.TokenType == JsonToken.None && jsonReader.Read())
				{
				}
				string text = null;
				while (jsonReader.Read())
				{
					if (jsonReader.Value != null)
					{
						if (jsonReader.Value.Equals("@odata.deltaLink"))
						{
							string text2 = jsonReader.ReadAsString();
							text = ExtractDeltaToken(text2);
							if (string.IsNullOrEmpty(requestDeltaToken))
							{
								requestDeltaToken = text;
								resourceEndpoint = text2;
							}
						}
						if (jsonReader.Value.Equals("@odata.nextLink"))
						{
							resourceEndpoint = jsonReader.ReadAsString();
						}
						if (jsonReader.Value.Equals("value"))
						{
							foundRecords.AddRange(serializer.Deserialize<List<ReadOnlyRecordBuffer>>(jsonReader));
						}
					}
				}
				foundRecords = AddDeltaTokenToRecords(text, foundRecords);
			});
		}
		return foundRecords;
	}

	private void HandleGraphSubscriptionChanges(MutableRecordBuffer recordBuffer)
	{
		if (MetaTable.TableId == 5455)
		{
			int num = (fieldIndexByExternalName.ContainsKey("Resource") ? fieldIndexByExternalName["Resource"] : (-1));
			if (num > -1)
			{
				NavText navText = recordBuffer[num] as NavText;
				recordBuffer[num] = NavText.Create(connection.ReplaceSharedMailboxToken(navText?.Value));
			}
		}
	}

	private static string ExtractDeltaToken(string deltaLink)
	{
		if (string.IsNullOrEmpty(deltaLink))
		{
			return null;
		}
		int num = deltaLink.ToLower(CultureInfo.InvariantCulture).IndexOf("DeltaToken".ToLower(CultureInfo.InvariantCulture), StringComparison.Ordinal);
		if (num == -1 || deltaLink.Length <= num + "DeltaToken".Length + 1)
		{
			return null;
		}
		return deltaLink.Substring(num + "DeltaToken".Length + 1);
	}

	private List<ReadOnlyRecordBuffer> AddDeltaTokenToRecords(string deltaToken, List<ReadOnlyRecordBuffer> records)
	{
		if (records.Count == 0 || string.IsNullOrEmpty(deltaToken))
		{
			return records;
		}
		if (deltaTokenColumnIndex == -1)
		{
			return records;
		}
		List<ReadOnlyRecordBuffer> list = new List<ReadOnlyRecordBuffer>();
		foreach (ReadOnlyRecordBuffer record in records)
		{
			NavValue[] array = record.ToArray();
			array[deltaTokenColumnIndex] = new NavText(deltaToken);
			list.Add(new ReadOnlyRecordBuffer(MetaTable, array));
		}
		return list;
	}

	private void GetByPrimaryKey(string recordId, out ReadOnlyRecordBuffer record)
	{
		string text = GetResourceUriFromId(recordId, connection.EntityEndpoint);
		if (!string.IsNullOrEmpty(singleValueExtendedPropertiesQuery))
		{
			text = AppendNameValueToQueryString(text, "$expand", singleValueExtendedPropertiesQuery);
		}
		if (!string.IsNullOrEmpty(openTypeExtensionQuery))
		{
			text = AppendNameValueToQueryString(text, "$expand", openTypeExtensionQuery);
		}
		ReadOnlyRecordBuffer foundRecordBuffer = null;
		ExecuteRequestWithRetry("GET", text, null, (Stream responseStream) =>
		{
			foundRecordBuffer = DeserializeSingleRecordFromStream(responseStream);
		}, (WebException webException) =>
		{
			if (webException.Status == WebExceptionStatus.ProtocolError)
			{
				HttpWebResponse obj = webException.Response as HttpWebResponse;
				if (obj != null && obj.StatusCode == HttpStatusCode.NotFound)
				{
					throw new NavCSideRecordNotFoundException(recordId);
				}
			}
			return false;
		});
		record = foundRecordBuffer;
	}

	private void ProcessDeepLinks(string resourceUri, string method, MutableRecordBuffer recordBuffer)
	{
		IEnumerable<NCLMetaField> enumerable = MetaTable.Fields.Where((NCLMetaField field) => field?.ExternalName?.StartsWith("/", StringComparison.Ordinal) == true);
		foreach (NCLMetaField metaField in enumerable)
		{
			NavValue navValue = recordBuffer[metaField.ColumnIndex];
			if (navValue.IsZeroOrEmpty)
			{
				continue;
			}
			resourceUri += metaField.ExternalName;
			byte[] contentBytes = navValue.GetBytes();
			ExecuteRequestWithRetry(method, resourceUri, (HttpWebRequest patchRequest) =>
			{
				patchRequest.ContentLength = contentBytes.Length;
				patchRequest.ContentType = metaField.ExternalType;
				using (Stream stream = getRequestStreamAction(patchRequest))
				{
					int num;
					for (int i = 0; i < contentBytes.Length; i += num)
					{
						num = Math.Min(28672, contentBytes.Length - i);
						stream.Write(contentBytes, i, Math.Min(28672, contentBytes.Length - i));
					}
				}
				return true;
			});
		}
	}

	private void ProcessExtensions(string resourceUri, string method, IEnumerable<string> extensionNames, MutableRecordBuffer recordBuffer, ExtensionsHelper.ExtensionType type)
	{
		using (connection.Diagnostics.CreateTraceScope("00000L7", DiagnosticParameter.SystemMetadata("GraphDataProvider.ProcessExtensions"), MonitorType.None, Category.Runtime))
		{
			if (type == ExtensionsHelper.ExtensionType.OpenTypeExtension)
			{
				resourceUri = AppendOperationSegment(resourceUri, "Extensions");
			}
			foreach (string extensionName in extensionNames)
			{
				if (string.IsNullOrWhiteSpace(extensionName))
				{
					connection.Diagnostics.SendTraceTag("00000ED", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Missing ExternalName for extension of type: {0} on table: {1}.", type.ToString(), recordBuffer.MetaTable.TableName);
				}
				else
				{
					if (!fieldIndexByExternalName.TryGetValue(extensionName, out var value) || (recordBuffer.ReadOnlyBuffer[value] != null && recordBuffer.IsChangedValueSameAsOriginalValue(value)) || !(recordBuffer[value] is NavBLOB { IsInMemory: not false, GetBytesSize: >=0 } navBLOB))
					{
						continue;
					}
					string resourceUri2 = resourceUri;
					string jsonText = Encoding.UTF8.GetString(navBLOB.GetBytes()).TrimEnd(default(char));
					switch (type)
					{
					case ExtensionsHelper.ExtensionType.OpenTypeExtension:
						resourceUri2 = GetResourceUriFromId(extensionName, resourceUri);
						break;
					case ExtensionsHelper.ExtensionType.SingleValueExtendedProperties:
						jsonText = string.Format(CultureInfo.InvariantCulture, "{{ \"{0}\": [ {1} ] }}", "SingleValueExtendedProperties", jsonText);
						break;
					}
					ExecuteRequestWithRetry(method, resourceUri2, (HttpWebRequest request) =>
					{
						request.ContentType = "application/json";
						using (Stream stream = getRequestStreamAction(request))
						{
							using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
							connection.Diagnostics.SendTraceTag("00000DR", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Inserting extension with the name {0}", extensionName);
							streamWriter.Write(jsonText);
						}
						return true;
					}, null, (WebException webException) =>
					{
						if (webException.Status == WebExceptionStatus.ProtocolError)
						{
							HttpWebResponse obj = webException.Response as HttpWebResponse;
							if (obj != null && obj.StatusCode == HttpStatusCode.NotFound)
							{
								throw new NavCSideRecordNotFoundException(resourceUri);
							}
						}
						return false;
					});
				}
			}
		}
	}

	private void SerializeRecordBufferToStream(MutableRecordBuffer recordBuffer, Stream stream)
	{
		using StreamWriter textWriter = new StreamWriter(stream, Encoding.UTF8);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		serializer.Serialize(jsonWriter, recordBuffer);
	}

	private ReadOnlyRecordBuffer DeserializeSingleRecordFromStream(Stream responsestream)
	{
		using StreamReader reader = new StreamReader(responsestream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
		using JsonReader reader2 = new JsonTextReader(reader);
		return DeserializeFromReader(reader2);
	}

	private ReadOnlyRecordBuffer DeserializeFromReader(JsonReader reader)
	{
		while (reader.TokenType == JsonToken.None && reader.Read())
		{
		}
		if (reader.TokenType != JsonToken.StartObject)
		{
			throw new NavGraphException(string.Format(CultureInfo.CurrentCulture, Lang.GraphUnexpectedTokenType, reader.TokenType, JsonToken.StartObject));
		}
		return serializer.Deserialize<ReadOnlyRecordBuffer>(reader);
	}

	private void ExecuteRequestWithRetry(string method, string resourceUri, Func<HttpWebRequest, bool> prepareRequest = null, Action<Stream> processResponsestream = null, Func<WebException, bool> errorHandler = null, int maxRetryAttempts = 3, string eTag = null)
	{
		using (connection.Diagnostics.CreateTraceScope("00000HS", DiagnosticParameter.SystemMetadata("Execute Graph Request"), MonitorType.None, Category.Runtime))
		{
			int num = 0;
			while (true)
			{
				connection.Diagnostics.SendTraceTag("00000EQ", Category.Runtime, Verbosity.Verbose, "RetryAttempt:{0}, Querying: {1}", DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.SystemMetadata(method));
				HttpWebRequest httpWebRequest = WebRequest.CreateHttp(resourceUri);
				httpWebRequest.KeepAlive = false;
				httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
				httpWebRequest.Method = method;
				try
				{
					if (!string.IsNullOrEmpty(connection.Proxy))
					{
						httpWebRequest.Proxy = new WebProxy(connection.Proxy);
					}
					AddSystemHeaders(httpWebRequest, eTag);
					if (prepareRequest != null && !prepareRequest(httpWebRequest))
					{
						connection.Diagnostics.SendTraceTag("00000ER", Category.Runtime, Verbosity.Verbose, "Query canceled: {0}.", DiagnosticParameter.SystemMetadata(method));
						break;
					}
					connection.Diagnostics.SendTraceTag("00000GG", Category.Runtime, Verbosity.Verbose, "Querying: {0}. Headers: {1}", DiagnosticParameter.SystemMetadata(method), DiagnosticParameter.Euii(httpWebRequest.Headers?.ToString() ?? string.Empty));
					using (connection.Diagnostics.CreateTraceScope("00000L8", DiagnosticParameter.SystemMetadata("GraphDataProvider.Request.GetResponse"), MonitorType.None, Category.Runtime))
					{
						using Stream obj = getResponseStreamAction(httpWebRequest, connection.Diagnostics);
						processResponsestream?.Invoke(obj);
						break;
					}
				}
				catch (WebException ex)
				{
					connection.Diagnostics.SendTraceTag("00000ES", Category.Runtime, Verbosity.Error, "RetryAttempt:{4}, Query Failed: {0} {1}. Headers: {2}. Error: {3}", DiagnosticParameter.SystemMetadata(method), DiagnosticParameter.CustomerContent(resourceUri), DiagnosticParameter.Euii(ex.Response?.Headers?.ToString() ?? string.Empty), DiagnosticParameter.CustomerContent(ex), DiagnosticParameter.SystemMetadata(num));
					if (WebExceptionHandler.IsTransientException(ex) && ++num < maxRetryAttempts)
					{
						continue;
					}
					if (ex.Status == WebExceptionStatus.ProtocolError)
					{
						connection.ClearCredentialCache();
					}
					if (errorHandler != null && errorHandler(ex))
					{
						break;
					}
					string arg = ExtractErrorFromWebException(ex);
					throw new NavGraphException(string.Format(CultureInfo.CurrentCulture, Lang.GraphRequestFailure, arg), ex);
				}
				catch (IOException ex2)
				{
					connection.Diagnostics.SendTraceTag("000016B", Category.Runtime, Verbosity.Error, "RetryAttempt:{3}, Query Failed: {0} {1}. Error: {2}", DiagnosticParameter.SystemMetadata(method), DiagnosticParameter.CustomerContent(resourceUri), DiagnosticParameter.CustomerContent(ex2), DiagnosticParameter.SystemMetadata(num));
					if (++num < maxRetryAttempts)
					{
						continue;
					}
					throw new NavGraphException(string.Format(CultureInfo.CurrentCulture, Lang.GraphRequestFailure, ex2), ex2);
				}
				catch (NavBaseException exception)
				{
					connection.Diagnostics.SendExceptionTag("000012Y", Category.Runtime, exception);
					throw;
				}
				catch (Exception ex3)
				{
					connection.Diagnostics.SendExceptionTag("000012Z", Category.Runtime, ex3);
					throw new NavGraphException(string.Format(CultureInfo.CurrentCulture, Lang.GraphRequestFailure, ex3.Message), ex3);
				}
			}
		}
	}

	[Conditional("DEBUG")]
	private void WriteDebugTrace(MutableRecordBuffer recordBuffer)
	{
		using MemoryStream memoryStream = new MemoryStream();
		SerializeRecordBufferToStream(recordBuffer, memoryStream);
		Trace.WriteLine("Failed \n" + Encoding.UTF8.GetString(memoryStream.GetBuffer()));
	}

	private void AddSystemHeaders(HttpWebRequest webRequest, string eTag)
	{
		webRequest.UserAgent = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", ProductNames.ProductName, Assembly.GetExecutingAssembly().GetName().Version);
		webRequest.Date = DateTime.UtcNow;
		SetHeaderValue(webRequest.Headers, "client-request-id", Guid.NewGuid().ToString("D"));
		SetHeaderValue(webRequest.Headers, "return-client-request-id", "true");
		if (!connection.DisableIfMatch)
		{
			SetHeaderValue(webRequest.Headers, "If-Match", eTag);
		}
		connection.SetAuthorizationHeader(webRequest);
		if (string.IsNullOrEmpty(connection.RequestHeaders))
		{
			return;
		}
		string[] array = connection.RequestHeaders.Split('|');
		foreach (string text in array)
		{
			string[] array2 = text.Split(':');
			if (!string.IsNullOrEmpty(array2[0]))
			{
				webRequest.Headers[array2[0]] = ((array2.Length > 1) ? array2[1] : string.Empty);
			}
		}
	}

	public static void SetHeaderValue(WebHeaderCollection headers, string key, string value)
	{
		if (!string.IsNullOrWhiteSpace(value) && !headers.AllKeys.Contains(key))
		{
			headers[key] = value;
		}
	}

	private string GetEtagFromRecordBuffer(IRecordBuffer recordBuffer)
	{
		if (eTagColumnIndex <= 0)
		{
			return null;
		}
		return recordBuffer[eTagColumnIndex].ToString();
	}

	private string GetIdFromRecordBuffer(IRecordBuffer recordBuffer)
	{
		return recordBuffer[PrimaryKeyField.FieldIndex].ToString();
	}

	private string GetIdFromRecordId(NavRecordId recordId)
	{
		return recordId.FieldValueByIndex(PrimaryKeyField, 0).ToString();
	}

	private static string GetResourceUriFromId(string id, string resourceUri)
	{
		Uri uri = new Uri(resourceUri);
		string text = (uri.IsDefaultPort ? string.Empty : string.Format(CultureInfo.InvariantCulture, ":{0}", uri.Port));
		return string.Format(CultureInfo.InvariantCulture, "{0}://{1}{2}/{3}('{4}'){5}", uri.Scheme, uri.Host, text, uri.AbsolutePath, id, uri.Query);
	}

	private string GetResourceUriCount()
	{
		Uri uri = new Uri(connection.EntityListEndpoint);
		string text = (uri.IsDefaultPort ? string.Empty : string.Format(CultureInfo.InvariantCulture, ":{0}", uri.Port));
		return string.Format(CultureInfo.InvariantCulture, "{0}://{1}{2}/{3}/$count{4}", uri.Scheme, uri.Host, text, uri.AbsolutePath, uri.Query);
	}

	private static void PopulateLookups(IGraphTableConnection connection, NCLMetaTable metaTable, out IList<string> openTypeExtensions, out IList<string> singleValueExtensions, out IDictionary<string, int> indexByExternalName)
	{
		openTypeExtensions = new List<string>();
		singleValueExtensions = new List<string>();
		indexByExternalName = new Dictionary<string, int>();
		NCLMetaField[] allFields = metaTable.AllFields;
		foreach (NCLMetaField nCLMetaField in allFields)
		{
			if (string.IsNullOrWhiteSpace(nCLMetaField.ExternalType) ^ string.IsNullOrWhiteSpace(nCLMetaField.ExternalName))
			{
				connection.Diagnostics.SendTraceTag("00000EF", Category.Runtime, Verbosity.Error, PrivacyClassification.SystemMetadata, "Both of the properties ExternalType and ExternalName must be specified. Misconfigured field: {0}", nCLMetaField.FieldName);
				continue;
			}
			if (!string.IsNullOrWhiteSpace(nCLMetaField.ExternalName))
			{
				indexByExternalName.Add(nCLMetaField.ExternalName, nCLMetaField.FieldIndex);
			}
			if (ExtensionsHelper.IsOpenTypeExtension(nCLMetaField))
			{
				openTypeExtensions.Add(nCLMetaField.ExternalName);
			}
			if (ExtensionsHelper.IsSingleValueExtendedProperties(nCLMetaField))
			{
				singleValueExtensions.Add(nCLMetaField.ExternalName);
			}
		}
	}

	private string GetResourceUriTrackingChanges(string deltaToken)
	{
		string text = connection.EntityListEndpoint;
		if (!string.IsNullOrEmpty(deltaToken))
		{
			text = AppendNameValueToQueryString(text, "$deltaToken", deltaToken);
		}
		if (!string.IsNullOrEmpty(singleValueExtendedPropertiesQuery))
		{
			text = AppendNameValueToQueryString(text, "$expand", singleValueExtendedPropertiesQuery);
		}
		if (!string.IsNullOrEmpty(openTypeExtensionQuery))
		{
			text = AppendNameValueToQueryString(text, "$expand", openTypeExtensionQuery);
		}
		return text;
	}

	private string AppendNameValueToQueryString(string uri, string name, string value)
	{
		if (string.IsNullOrWhiteSpace(uri))
		{
			connection.Diagnostics.SendTraceTag("00000DS", Category.Runtime, Verbosity.Verbose, "AppendNameValueToQueryString invoked with an empty uri. Name = {0}", DiagnosticParameter.SystemMetadata(name));
			return uri;
		}
		GraphTableConnection obj = connection as GraphTableConnection;
		if (obj != null && obj.IsTestConnection)
		{
			return uri;
		}
		UriBuilder uriBuilder = new UriBuilder(uri);
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
		if (nameValueCollection[name] != null)
		{
			return uri;
		}
		StringBuilder stringBuilder = new StringBuilder(uriBuilder.Query);
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(0, 1).Append('&');
		}
		uriBuilder.Query = stringBuilder.Append(HttpUtility.UrlEncode(name)).Append('=').Append(HttpUtility.UrlEncode(value))
			.ToString();
		return uriBuilder.ToString();
	}

	private string AppendOperationSegment(string uri, string httpOperation)
	{
		if (string.IsNullOrWhiteSpace(uri))
		{
			connection.Diagnostics.SendTraceTag("00000DT", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "AppendOperationSegment invoked with and empty uri. HttpOperation = {0}", httpOperation);
			return uri;
		}
		UriBuilder uriBuilder = new UriBuilder(uri);
		if (!uriBuilder.Path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
		{
			uriBuilder.Path += "/";
		}
		if (!uriBuilder.Path.EndsWith(httpOperation, StringComparison.OrdinalIgnoreCase))
		{
			uriBuilder.Path += httpOperation;
		}
		return uriBuilder.ToString();
	}

	private static Stream DefaultGetResponseStreamFromRequestAction(HttpWebRequest request, NavDiagnostics diagnostics)
	{
		HttpWebResponse httpWebResponse = request.GetResponse() as HttpWebResponse;
		diagnostics?.SendTraceTag("00000GH", Category.Runtime, Verbosity.Verbose, "Response with headers: {0}.", DiagnosticParameter.Euii(httpWebResponse?.Headers?.ToString() ?? string.Empty));
		return httpWebResponse?.GetResponseStream();
	}

	private static Stream DefaultGetRequestStreamAction(HttpWebRequest request)
	{
		return request.GetRequestStream();
	}

	private static long DefaultGetRequestContentLength(HttpWebRequest request)
	{
		return request.ContentLength;
	}

	private static string ExtractErrorFromWebException(WebException errorException)
	{
		string text = errorException?.ToString();
		if (errorException?.Response != null)
		{
			try
			{
				using Stream stream = errorException.Response.GetResponseStream();
				if (stream == null)
				{
					throw new ArgumentException("Unexpected null error stream");
				}
				using StreamReader reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
				using JsonReader jsonReader = new JsonTextReader(reader);
				while (jsonReader.TokenType == JsonToken.None && jsonReader.Read())
				{
				}
				if (jsonReader.TokenType == JsonToken.StartObject)
				{
					JObject jObject = JObject.Load(jsonReader);
					JObject jObject2 = jObject["error"]?.Value<JObject>();
					if (jObject2 != null)
					{
						text = jObject2["message"].Value<string>() + " - " + text;
					}
				}
			}
			catch (Exception ex)
			{
				throw new NavGraphException(string.Format(CultureInfo.CurrentCulture, Lang.GraphRequestFailure, ex.Message), ex);
			}
		}
		return text;
	}
}
