using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavVariant : NavIndirectValue
{
	public class Factory : ElementFactory<NavVariant>
	{
		public Factory(ITreeObject parentForArray)
			: base(parentForArray)
		{
		}

		public override NavVariant CreateElement()
		{
			return Default(ParentForElements);
		}

		public override void AssignElement(NavVariant destinationElement, NavVariant sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavVariant> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone);
		}
	}

	private NavValue value;

	public bool ALIsAction => IsOfNavType(NavType.Action);

	public bool ALIsAutomation => IsOfNavType(NavType.Automation);

	public bool ALIsDotNet => IsOfNavType(NavType.DotNet);

	public bool ALIsBigInteger => IsOfNavType(NavType.BigInteger);

	public bool ALIsBigText => IsOfNavType(NavType.BigText);

	public bool ALIsBinary => false;

	public bool ALIsBoolean => IsOfNavType(NavType.Boolean);

	public bool ALIsCode
	{
		get
		{
			if (!IsOfNavType(NavType.OemCode))
			{
				return IsOfNavType(NavType.Code);
			}
			return true;
		}
	}

	public bool ALIsByte => IsOfNavType(NavType.Byte);

	public bool ALIsCodeunit
	{
		get
		{
			if (!IsOfNavType(NavType.Codeunit))
			{
				return IsOfNavType(NavType.CodeunitReference);
			}
			return true;
		}
	}

	public bool ALIsDate => IsOfNavType(NavType.Date);

	public bool ALIsDateFormula => IsOfNavType(NavType.DateFormula);

	public bool ALIsDateTime => IsOfNavType(NavType.DateTime);

	public bool ALIsDecimal => IsOfNavType(NavType.Decimal);

	public bool ALIsDuration => IsOfNavType(NavType.Duration);

	public bool ALIsFieldClass => IsOfNavType(NavType.FieldClass);

	public bool ALIsFieldType => IsOfNavType(NavType.TypeId);

	public bool ALIsFieldRef => IsOfNavType(NavType.FieldRef);

	public bool ALIsFile => IsOfNavType(NavType.File);

	public bool ALIsGuid => IsOfNavType(NavType.GUID);

	public bool ALIsInStream => IsOfNavType(NavType.InStream);

	public bool ALIsInteger => IsOfNavType(NavType.Integer);

	public bool ALIsKeyRef => IsOfNavType(NavType.KeyRef);

	public bool ALIsOption => IsOfNavType(NavType.Option);

	public bool ALIsOutStream => IsOfNavType(NavType.OutStream);

	public bool ALIsRecord
	{
		get
		{
			if (!IsOfNavType(NavType.Record))
			{
				return IsOfNavType(NavType.RecordReference);
			}
			return true;
		}
	}

	public bool ALIsRecordId => IsOfNavType(NavType.RecordID);

	public bool ALIsRecordRef => IsOfNavType(NavType.RecordRef);

	public bool ALIsReport
	{
		get
		{
			if (!IsOfNavType(NavType.Report))
			{
				return IsOfNavType(NavType.ReportReference);
			}
			return true;
		}
	}

	public bool ALIsTableFilter => IsOfNavType(NavType.TableFilter);

	public bool ALIsTextConst => IsOfNavType(NavType.TextConst);

	public bool ALIsTime => IsOfNavType(NavType.Time);

	public bool ALIsTransactionType => IsOfNavType(NavType.TransactionType);

	public bool ALIsExecutionMode => IsOfNavType(NavType.ExecutionMode);

	public bool ALIsExecutionContext => IsOfNavType(NavType.ExecutionContext);

	public bool ALIsTypeId => IsOfNavType(NavType.TypeId);

	public bool ALIsXmlPort
	{
		get
		{
			if (!IsOfNavType(NavType.XmlPort))
			{
				return IsOfNavType(NavType.XmlPortReference);
			}
			return true;
		}
	}

	public bool ALIsString => IsOfNavType(NavType.String);

	public bool ALIsText
	{
		get
		{
			if (!IsOfNavType(NavType.Text))
			{
				return IsOfNavType(NavType.OemText);
			}
			return true;
		}
	}

	public bool ALIsWideChar => ALIsChar;

	public bool ALIsChar => IsOfNavType(NavType.Char);

	public bool ALIsFilterPageBuilder => IsOfNavType(NavType.FilterPageBuilder);

	public bool ALIsClientType => IsOfNavType(NavType.ClientType);

	public bool ALIsSecurityFiltering => IsOfNavType(NavType.SecurityFiltering);

	public bool ALIsObjectType => IsOfNavType(NavType.ObjectType);

	public bool ALIsTextEncoding => IsOfNavType(NavType.TextEncoding);

	public bool ALIsReportFormat => IsOfNavType(NavType.ReportFormat);

	public bool ALIsDefaultLayout => IsOfNavType(NavType.DefaultLayout);

	public bool ALIsTableConnectionType => IsOfNavType(NavType.TableConnectionType);

	public bool ALIsNotification => IsOfNavType(NavType.Notification);

	public bool ALIsNotificationScope => IsOfNavType(NavType.NotificationScope);

	public bool ALIsSessionSettings => IsOfNavType(NavType.SessionSettings);

	public bool ALIsTestPermissions => IsOfNavType(NavType.TestPermissions);

	public bool ALIsDataClassification => IsOfNavType(NavType.DataClassification);

	public bool ALIsDataScope => IsOfNavType(NavType.DataScope);

	public bool ALIsVerbosity => IsOfNavType(NavType.Verbosity);

	public bool ALIsTransactionModel => IsOfNavType(NavType.TestTransactionModel);

	public bool ALIsJsonArray => IsOfNavType(NavType.JsonArray);

	public bool ALIsJsonObject => IsOfNavType(NavType.JsonObject);

	public bool ALIsJsonToken => IsOfNavType(NavType.JsonToken);

	public bool ALIsJsonValue => IsOfNavType(NavType.JsonValue);

	public bool ALIsXmlAttribute => IsOfNavType(NavType.XmlAttribute);

	public bool ALIsXmlAttributeCollection => IsOfNavType(NavType.XmlAttributeCollection);

	public bool ALIsXmlCData => IsOfNavType(NavType.XmlCData);

	public bool ALIsXmlComment => IsOfNavType(NavType.XmlComment);

	public bool ALIsXmlDeclaration => IsOfNavType(NavType.XmlDeclaration);

	public bool ALIsXmlDocument => IsOfNavType(NavType.XmlDocument);

	public bool ALIsXmlDocumentType => IsOfNavType(NavType.XmlDocumentType);

	public bool ALIsXmlElement => IsOfNavType(NavType.XmlElement);

	public bool ALIsXmlNamespaceManager => IsOfNavType(NavType.XmlNamespaceManager);

	public bool ALIsXmlNameTable => IsOfNavType(NavType.XmlNameTable);

	public bool ALIsXmlNode => IsOfNavType(NavType.XmlNode);

	public bool ALIsXmlNodeList => IsOfNavType(NavType.XmlNodeList);

	public bool ALIsXmlProcessingInstruction => IsOfNavType(NavType.XmlProcessingInstruction);

	public bool ALIsXmlPortReference => IsOfNavType(NavType.XmlPortReference);

	public bool ALIsXmlReadOptions => IsOfNavType(NavType.XmlReadOptions);

	public bool ALIsXmlText => IsOfNavType(NavType.XmlText);

	public bool ALIsXmlWriteOptions => IsOfNavType(NavType.XmlWriteOptions);

	public bool ALIsTextBuilder => IsOfNavType(NavType.TextBuilder);

	public override NavNclType NclType => NavNclType.NavVariant;

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override bool IsInitialized => value != null;

	public override NavValue InnerValue
	{
		get
		{
			CheckInitialized();
			return value;
		}
	}

	public void ALAssign(byte other)
	{
		ClearAndSetValue(NavByte.Create(other));
	}

	public void ALAssign(char other)
	{
		ClearAndSetValue(NavChar.Create(other));
	}

	public void ALAssign(bool other)
	{
		ClearAndSetValue(NavBoolean.Create(other));
	}

	public void ALAssign(NavOption other)
	{
		ClearAndSetValue(other);
	}

	public void ALAssign(int other)
	{
		ClearAndSetValue(NavInteger.Create(other));
	}

	public void ALAssign(uint other)
	{
		ClearAndSetValue(NavBigInteger.Create(other));
	}

	public void ALAssign(long other)
	{
		ClearAndSetValue(NavBigInteger.Create(other));
	}

	public void ALAssign(NavDuration other)
	{
		ClearAndSetValue(other);
	}

	public void ALAssign(Decimal18 other)
	{
		ClearAndSetValue(NavDecimal.Create(other));
	}

	public void ALAssign(Guid other)
	{
		ClearAndSetValue(new NavGuid(other));
	}

	public void ALAssign(FieldClass other)
	{
		ClearAndSetValue(new NavFieldClass(other));
	}

	public void ALAssign(TransactionType other)
	{
		ClearAndSetValue(new NavTransactionType(other));
	}

	public void ALAssign(TextEncoding other)
	{
		ClearAndSetValue(new NavEnumTextEncoding(other));
	}

	public void ALAssign(FormResult other)
	{
		ClearAndSetValue(new NavFormResult(other));
	}

	public void ALAssign(ExecutionMode other)
	{
		ClearAndSetValue(new NavExecutionMode(other));
	}

	public void ALAssign(ExecutionContext other)
	{
		ClearAndSetValue(new NavExecutionContext(other));
	}

	public void ALAssign(NavFilterPageBuilder other)
	{
		ClearAndSetValue(new NavFilterPageBuilder(other));
	}

	public void ALAssign(NavObjectType other)
	{
		ClearAndSetValue(new NavEnumObjectType(other));
	}

	public void ALAssign(SecurityFiltering other)
	{
		ClearAndSetValue(new NavSecurityFiltering(other));
	}

	public void ALAssign(NavClientType other)
	{
		ClearAndSetValue(new NavEnumClientType(other));
	}

	public void ALAssign(NavReportFormat other)
	{
		ClearAndSetValue(new NavEnumReportFormat(other));
	}

	public void ALAssign(NavDefaultLayout other)
	{
		ClearAndSetValue(new NavEnumDefaultLayout(other));
	}

	public void ALAssign(TableConnectionType other)
	{
		ClearAndSetValue(new NavTableConnectionType(other));
	}

	public void ALAssign(NotificationScope other)
	{
		ClearAndSetValue(new NavNotificationScope(other));
	}

	public void ALAssign(NavTestPermissions other)
	{
		ClearAndSetValue(new NavEnumTestPermissions(other));
	}

	public void ALAssign(NavType other)
	{
		ClearAndSetValue(new NavFieldType(other));
	}

	public void ALAssign(DataClassification other)
	{
		ClearAndSetValue(new NavDataClassification(other));
	}

	public void ALAssign(ALVerbosity other)
	{
		ClearAndSetValue(new NavVerbosity(other));
	}

	public void ALAssign(TestTransactionModel other)
	{
		ClearAndSetValue(new NavTestTransactionModel(other));
	}

	internal void ALAssignNavComplexValue(NavValue other)
	{
		Clear();
		switch (other.NclType)
		{
		case NavNclType.NavAutomation:
			SetValue(((NavAutomation)other).ALByValue(this));
			break;
		case NavNclType.NavDotNet:
			SetValue(((NavDotNet)other).ALByValue(this));
			break;
		case NavNclType.NavBlob:
			SetValue(((NavBLOB)other).ALByValue(this));
			break;
		case NavNclType.NavCodeunit:
			SetValue(other);
			break;
		case NavNclType.NavRecordRef:
			SetValue(((NavRecordRef)other).ALByValue(this));
			break;
		case NavNclType.NavCodeunitReference:
			SetValue(((NavCodeunitHandle)other).Target);
			break;
		case NavNclType.NavFieldRef:
		{
			NavFieldRef navFieldRef = (NavFieldRef)other;
			if (navFieldRef.IsInitialized)
			{
				SetValue(navFieldRef.ALByValue(this));
			}
			else
			{
				Clear();
			}
			break;
		}
		case NavNclType.NavFile:
			SetValue(((NavFile)other).ALByValue(this));
			break;
		case NavNclType.NavInStream:
			SetValue(((NavInStream)other).ALByValue(this));
			break;
		case NavNclType.NavOutStream:
			SetValue(((NavOutStream)other).ALByValue(this));
			break;
		case NavNclType.NavKeyRef:
			SetValue(((NavKeyRef)other).ALByValue(this));
			break;
		case NavNclType.NavRecord:
			SetValue(((NavRecord)other).CloneForVariant(this));
			break;
		case NavNclType.NavRecordReference:
		{
			NavRecordHandle navRecordHandle = (NavRecordHandle)other;
			SetValue(navRecordHandle.Target.CloneForVariant(this, navRecordHandle.Target.IsTemporary));
			break;
		}
		case NavNclType.NavFilterPageBuilder:
		{
			NavFilterPageBuilder navFilterPageBuilder = (NavFilterPageBuilder)other;
			SetValue((NavValue)navFilterPageBuilder.Clone());
			break;
		}
		case NavNclType.NavVariant:
		{
			NavVariant navVariant = (NavVariant)other;
			if (navVariant.IsInitialized)
			{
				ALAssign(navVariant.InnerValue);
			}
			else
			{
				Clear();
			}
			break;
		}
		case NavNclType.NavMedia:
			ClearAndSetValue(new NavMedia(((NavMedia)other).InnerValue.ToGuid()));
			break;
		case NavNclType.NavMediaSet:
			ClearAndSetValue(new NavMediaSet(((NavMediaSet)other).InnerValue.ToGuid()));
			break;
		case NavNclType.NavNotification:
			ClearAndSetValue(new NavNotification((NavNotification)other));
			break;
		case NavNclType.NavJsonArray:
			ClearAndSetValue((NavJsonArray)other);
			break;
		case NavNclType.NavJsonObject:
			ClearAndSetValue((NavJsonObject)other);
			break;
		case NavNclType.NavJsonToken:
			ClearAndSetValue((NavJsonToken)other);
			break;
		case NavNclType.NavJsonValue:
			ClearAndSetValue((NavJsonValue)other);
			break;
		case NavNclType.NavXmlAttribute:
			ClearAndSetValue((NavXmlAttribute)other);
			break;
		case NavNclType.NavXmlAttributeCollection:
			ClearAndSetValue((NavXmlAttributeCollection)other);
			break;
		case NavNclType.NavXmlCData:
			ClearAndSetValue((NavXmlCData)other);
			break;
		case NavNclType.NavXmlComment:
			ClearAndSetValue((NavXmlComment)other);
			break;
		case NavNclType.NavXmlDeclaration:
			ClearAndSetValue((NavXmlDeclaration)other);
			break;
		case NavNclType.NavXmlDocument:
			ClearAndSetValue((NavXmlDocument)other);
			break;
		case NavNclType.NavXmlDocumentType:
			ClearAndSetValue((NavXmlDocumentType)other);
			break;
		case NavNclType.NavXmlElement:
			ClearAndSetValue((NavXmlElement)other);
			break;
		case NavNclType.NavXmlNamespaceManager:
			ClearAndSetValue((NavXmlNamespaceManager)other);
			break;
		case NavNclType.NavXmlNameTable:
			ClearAndSetValue((NavXmlNameTable)other);
			break;
		case NavNclType.NavXmlNode:
			ClearAndSetValue((NavXmlNode)other);
			break;
		case NavNclType.NavXmlNodeList:
			ClearAndSetValue((NavXmlNodeList)other);
			break;
		case NavNclType.NavXmlProcessingInstruction:
			ClearAndSetValue((NavXmlProcessingInstruction)other);
			break;
		case NavNclType.NavXmlReadOptions:
			ClearAndSetValue((NavXmlReadOptions)other);
			break;
		case NavNclType.NavXmlText:
			ClearAndSetValue((NavXmlText)other);
			break;
		case NavNclType.NavXmlWriteOptions:
			ClearAndSetValue((NavXmlWriteOptions)other);
			break;
		case NavNclType.NavTextBuilder:
			ClearAndSetValue((NavTextBuilder)other);
			break;
		default:
			throw new NavNCLNotSupportedOperationException();
		}
	}

	public void ALAssign(object other)
	{
		if (other == null)
		{
			Clear();
			return;
		}
		if (other is NavValue navValue)
		{
			if (!navValue.IsMutable)
			{
				ClearAndSetValue(navValue);
			}
			else
			{
				ALAssignNavComplexValue(navValue);
			}
			return;
		}
		if (other is string text)
		{
			ClearAndSetValue(new NavText((text.Length < 2048) ? 2048 : int.MaxValue, text));
			return;
		}
		INavValueMetadata navValueMetadata = NavValueMetadata.CalcMetadataFromObject(other);
		if (navValueMetadata != null)
		{
			ClearAndSetValue(NavValue.CreateNavValueFromObject(navValueMetadata, other));
			return;
		}
		throw new NavNCLNotSupportedOperationException();
	}

	public NavVariant ALByValue(ITreeObject parentForResult)
	{
		NavVariant navVariant = new NavVariant(parentForResult, null);
		if (IsInitialized)
		{
			navVariant.ALAssign(InnerValue);
		}
		return navVariant;
	}

	public NavVariant(ITreeObject parent, NavValue initValue)
		: base(parent)
	{
		ALAssign(initValue);
	}

	public static NavVariant Default(ITreeObject parent)
	{
		return new NavVariant(parent, null);
	}

	private void ClearAndSetValue(NavValue newValue)
	{
		if (value != newValue)
		{
			Clear();
			SetValue(newValue);
		}
	}

	private void SetValue(NavValue newValue)
	{
		ITreeObject treeObject = newValue as ITreeObject;
		Tree.DisposeAllChildrenExcept(treeObject);
		value = newValue;
		if (treeObject != null && newValue is ITreeSharedObject initialTarget)
		{
			new TreeObjectReference(this, initialTarget);
		}
	}

	public override void Clear()
	{
		Tree.DisposeAllChildren();
		value = null;
	}
}
