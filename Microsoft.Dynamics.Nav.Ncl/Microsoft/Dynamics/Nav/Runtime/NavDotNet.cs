using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavDotNet : NavComplexValue, IDisposable
{
	public class Factory : ElementFactory<NavDotNet>
	{
		private string assemblyFullName;

		private string typeName;

		private bool runOnClient;

		public Factory(ITreeObject parentForArray, string assemblyFullName, string typeName, bool runOnClient)
			: base(parentForArray)
		{
			this.assemblyFullName = assemblyFullName;
			this.typeName = typeName;
			this.runOnClient = runOnClient;
		}

		public override NavDotNet CreateElement()
		{
			return new NavDotNet(ParentForElements, assemblyFullName, typeName, runOnClient);
		}

		public override void AssignElement(NavDotNet destinationElement, NavDotNet sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavDotNet> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone, assemblyFullName, typeName, runOnClient);
		}
	}

	private string assemblyFullName;

	private string typeName;

	private bool runOnClient;

	private bool suppressDispose;

	private bool isInterface;

	private NavForm form;

	private int controlId;

	private string variableName;

	private NavServerHandle serverHandle;

	private NavClientHandle remoteHandle;

	private Type interfaceType;

	private Type staticType;

	private NavAutomationHandle remoteInterfaceTypeHandle;

	private NavInteropEventHandlerBase eventSink;

	private bool isClear;

	private static ParameterModifier emptyParameterModifer;

	private const uint SetterMask = 1073741824u;

	private const uint EnumMask = 2147483648u;

	private const uint IndexMask = 268435455u;

	private const uint NoCache = 268435455u;

	public override bool IsNull
	{
		get
		{
			if (isClear)
			{
				return true;
			}
			if (RunOnClient)
			{
				if (controlId != 0)
				{
					return false;
				}
				return !ValidRemoteObject;
			}
			if (Value == null)
			{
				return true;
			}
			if (!(Value is NavDotNet navDotNet))
			{
				return false;
			}
			return navDotNet.IsNull;
		}
	}

	public object Value
	{
		get
		{
			if (serverHandle == null)
			{
				return null;
			}
			return serverHandle.Instance;
		}
	}

	internal bool RunOnClient => runOnClient;

	internal bool ValidRemoteObject
	{
		get
		{
			if (remoteHandle != null)
			{
				return !IsClientValueNull(remoteHandle);
			}
			return false;
		}
	}

	internal string TypeName => typeName;

	internal bool HasEventSink => eventSink != null;

	public override NavNclType NclType => NavNclType.NavDotNet;

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public static NavDotNet CreateDotNetEnumerator(ITreeObject parent, bool runOnClient)
	{
		Type typeFromHandle = typeof(IEnumerator);
		return new NavDotNet(parent, typeFromHandle.Assembly.FullName, typeFromHandle.FullName, runOnClient, isInterface: true);
	}

	public NavDotNet CreateDotNet(params object[] arguments)
	{
		try
		{
			NavDotNet navDotNet = new NavDotNet(this, null, RunOnClient);
			int num = arguments.Length;
			object[] array = new object[num];
			List<NavDotNet> disposeObjects = null;
			for (int i = 0; i < num; i++)
			{
				array[i] = GetDotNetObjectFromNavObject(arguments[i].GetType(), arguments[i], ref disposeObjects);
			}
			if (RunOnClient)
			{
				navDotNet.remoteHandle = new NavClientHandle(NavCurrentThread.Session.ClientCallback.CreateDotNetHandle(assemblyFullName, typeName, (form != null) ? form.Handle : Guid.Empty, variableName, createInstance: true, array), suppressDispose);
			}
			else
			{
				navDotNet.serverHandle = CreateNavServerHandle(assemblyFullName, typeName, createInstance: true, createType: false, suppressDispose, includeAddInsTable: true, null, includeNavApps: true, array);
			}
			navDotNet.variableName = variableName;
			navDotNet.form = form;
			navDotNet.controlId = controlId;
			navDotNet.isClear = false;
			return navDotNet;
		}
		catch (Exception ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000016K", Category.Runtime, ex, Verbosity.Warning);
			if (ex is NavBaseException)
			{
				throw;
			}
			throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentCulture, assemblyFullName, typeName, ex);
		}
	}

	public override void Clear()
	{
		isClear = true;
		if (remoteHandle != null)
		{
			remoteHandle.Dispose();
		}
		else if (serverHandle != null)
		{
			try
			{
				serverHandle.Dispose();
			}
			finally
			{
				if (eventSink != null)
				{
					TearDownEventSink();
				}
			}
		}
		serverHandle = null;
		remoteHandle = null;
	}

	public T As<T>()
	{
		if (typeof(T).IsEnum)
		{
			return (T)Enum.ToObject(typeof(T), GetValue<object>());
		}
		object value;
		if (IsNull && NavAutomationHelper.IsNullableNavRuntimeType(typeof(T)))
		{
			if (typeof(T) == typeof(object))
			{
				return default;
			}
			value = default(T);
		}
		else
		{
			value = GetValue<object>();
		}
		return (T)GetNavObjectFromDotNetObject(typeof(T), value);
	}

	private object GetRawValueForConversion<T>()
	{
		if (IsNull && NavAutomationHelper.IsNullableNavRuntimeType(typeof(T)))
		{
			return default(T);
		}
		return GetValue<object>();
	}

	public static NavDotNet GetDotNetType(ITreeObject parentForResult, object value)
	{
		Type type = null;
		if (value == null)
		{
			type = typeof(object);
		}
		else if (value is NavDotNet navDotNet)
		{
			if (!string.IsNullOrEmpty(navDotNet.assemblyFullName) && !string.IsNullOrEmpty(navDotNet.typeName))
			{
				type = NavAutomationHelper.GetTypeFromAssembly(navDotNet.assemblyFullName, navDotNet.typeName);
			}
			else if (!navDotNet.runOnClient)
			{
				if (navDotNet.serverHandle != null)
				{
					type = navDotNet.serverHandle.ObjectType;
				}
			}
			else
			{
				type = typeof(NavDotNet);
			}
		}
		else if (!(value is NavValue))
		{
			type = ((!(value is Decimal18)) ? value.GetType() : typeof(decimal));
		}
		else
		{
			INavValueMetadata navValueMetadata = NavValueMetadata.CalcMetadataFromDotNetObject(value);
			switch (navValueMetadata.NclType)
			{
			case NavNclType.NavBigInteger:
				type = typeof(decimal);
				break;
			case NavNclType.NavBigText:
				type = typeof(string);
				break;
			case NavNclType.NavBoolean:
				type = typeof(bool);
				break;
			case NavNclType.NavByte:
				type = typeof(byte);
				break;
			case NavNclType.NavOemCode:
				type = typeof(string);
				break;
			case NavNclType.NavCode:
				type = typeof(string);
				break;
			case NavNclType.NavDate:
				type = typeof(DateTime);
				break;
			case NavNclType.NavDateTime:
				type = typeof(DateTime);
				break;
			case NavNclType.NavDecimal:
				type = typeof(decimal);
				break;
			case NavNclType.NavDuration:
				type = typeof(TimeSpan);
				break;
			case NavNclType.NavGuid:
				type = typeof(Guid);
				break;
			case NavNclType.NavInteger:
				type = typeof(int);
				break;
			case NavNclType.NavOption:
				type = typeof(int);
				break;
			case NavNclType.NavOemText:
				type = typeof(string);
				break;
			case NavNclType.NavTextConstant:
				type = typeof(string);
				break;
			case NavNclType.NavTime:
				type = typeof(DateTime);
				break;
			case NavNclType.NavVariant:
				type = typeof(object);
				break;
			case NavNclType.NavStream:
			case NavNclType.NavInStream:
			case NavNclType.NavOutStream:
				type = typeof(Stream);
				break;
			case NavNclType.NavText:
				type = typeof(string);
				break;
			case NavNclType.NavChar:
				type = typeof(byte);
				break;
			}
		}
		if (type == null)
		{
			throw NavNCLNotSupportedTypeException.CreateUnsupportedGetType(CultureInfo.CurrentCulture, value.GetType().ToString());
		}
		return new NavDotNet(parentForResult, type, runOnClient: false);
	}

	public static bool CanLoadType(NavDotNet value, bool includeAddInsTable = true)
	{
		if (value == null || string.IsNullOrEmpty(value.assemblyFullName) || string.IsNullOrEmpty(value.typeName))
		{
			return false;
		}
		Type type = null;
		try
		{
			if (value.RunOnClient)
			{
				return NavCurrentThread.Session.ClientCallback.CanLoadDotNetType(value.assemblyFullName, value.typeName);
			}
			type = NavAutomationHelper.GetTypeFromAssembly(value.assemblyFullName, value.typeName);
		}
		catch (NavNCLDotNetCreateException)
		{
			if (includeAddInsTable)
			{
				string addInAssemblyLocationFromAssemblyName = NavAutomationHelper.GetAddInAssemblyLocationFromAssemblyName(value.assemblyFullName);
				if (addInAssemblyLocationFromAssemblyName == null)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001VF", Category.Runtime, Verbosity.Normal, "Failed to load assembly from disk, trying from add-in tables. Assembly: {0}", DiagnosticParameter.SystemMetadata(value.assemblyFullName));
					if (NavGlobal.SystemTenant.AddInProvider.ExtractAddInResourceFromAddInTable(value.assemblyFullName))
					{
						return CanLoadType(value, !includeAddInsTable);
					}
				}
			}
		}
		return type != null;
	}

	public void ALAssign(NavDotNet other)
	{
		Assign(other);
	}

	public void ALAssign(NavDotNet other, bool inheritRunOnClient)
	{
		if (inheritRunOnClient && other != null)
		{
			runOnClient = other.RunOnClient;
		}
		Assign(other);
	}

	public void ALAssign(object value)
	{
		if (RunOnClient)
		{
			NavInteropHandleReferenceCount navInteropHandleReferenceCount = remoteHandle;
			remoteHandle = new NavClientHandle(NavCurrentThread.Session.ClientCallback.CreateDotNetHandle(assemblyFullName, typeName, Guid.Empty, null, false, value), suppressDispose);
			isClear = false;
			navInteropHandleReferenceCount?.Dispose();
			return;
		}
		if (serverHandle == null)
		{
			serverHandle = CreateNavServerHandle(assemblyFullName, typeName, false, true, suppressDispose, true, null, false);
		}
		if (serverHandle.ObjectType != null)
		{
			serverHandle.Instance = ALCompiler.ConvertRuntimeValueToDotNetValue(value, serverHandle.ObjectType);
			isClear = false;
			return;
		}
		throw new NavNCLNotSupportedOperationException();
	}

	public NavDotNet ALByValue(ITreeObject parentForResult)
	{
		return Clone(parentForResult);
	}

	internal NavDotNet(ITreeObject parent, NavDotNet dotNetObject)
		: base(parent)
	{
		runOnClient = dotNetObject.runOnClient;
		assemblyFullName = dotNetObject.assemblyFullName;
		typeName = dotNetObject.typeName;
		serverHandle = dotNetObject.serverHandle;
		remoteHandle = dotNetObject.remoteHandle;
		if (serverHandle != null)
		{
			serverHandle.AddReference();
		}
		if (remoteHandle != null)
		{
			remoteHandle.AddReference();
		}
		isClear = dotNetObject.isClear;
		controlId = dotNetObject.controlId;
		form = dotNetObject.form;
		variableName = dotNetObject.variableName;
		isInterface = dotNetObject.isInterface;
		interfaceType = dotNetObject.interfaceType;
	}

	internal NavDotNet(ITreeObject parent, object dotNetObject, bool runOnClient)
		: this(parent, dotNetObject, runOnClient, suppressDispose: false)
	{
	}

	public NavDotNet(ITreeObject parent, object dotNetValue, bool runOnClient, bool suppressDispose)
		: base(parent)
	{
		this.runOnClient = runOnClient;
		if (dotNetValue != null)
		{
			serverHandle = CreateNavServerHandle(null, null, false, false, suppressDispose, true, dotNetValue, false);
			isClear = serverHandle.Instance == null;
		}
		else
		{
			isClear = true;
		}
	}

	public NavDotNet(ITreeObject parent, string assemblyFullName, string typeName, bool runOnClient, bool isInterface, NavInteropEventHandlerBase eventSink)
		: base(parent)
	{
		this.assemblyFullName = assemblyFullName;
		this.typeName = typeName;
		this.runOnClient = runOnClient;
		this.isInterface = isInterface;
		this.eventSink = eventSink;
		isClear = true;
		string assemblyFullNameString = NavAutomationHelper.GetAssemblyFullNameString(assemblyFullName);
		if (assemblyFullNameString != null)
		{
			this.assemblyFullName = assemblyFullNameString;
		}
	}

	public NavDotNet(ITreeObject parent, string assemblyFullName, string typeName, bool runOnClient)
		: this(parent, assemblyFullName, typeName, runOnClient, isInterface: false)
	{
	}

	public NavDotNet(ITreeObject parent, string assemblyFullName, string typeName, bool runOnClient, bool isInterface)
		: this(parent, assemblyFullName, typeName, runOnClient, isInterface, null)
	{
	}

	public NavDotNet(ITreeObject parent, string assemblyFullName, string typeName, bool runOnClient, bool isInterface, bool suppressDispose)
		: this(parent, assemblyFullName, typeName, runOnClient, isInterface, null)
	{
		this.suppressDispose = suppressDispose;
	}

	public NavDotNet(NavForm parent, string assemblyFullName, string typeName, bool runOnClient, bool isInterface, string variableName)
		: base(parent)
	{
		this.assemblyFullName = assemblyFullName;
		this.typeName = typeName;
		this.runOnClient = runOnClient;
		this.isInterface = isInterface;
		form = parent;
		this.variableName = variableName;
		isClear = true;
	}

	internal NavDotNet(ITreeObject parent, NavAutomationHandle handle)
		: base(parent)
	{
		remoteHandle = new NavClientHandle(handle, suppressDispose: false);
		runOnClient = true;
	}

	public NavDotNet(NavForm parent, int controlId)
		: base(parent)
	{
		form = parent;
		this.controlId = controlId;
		runOnClient = true;
	}

	private object GetDotNetObjectFromNavObject(Type targetType, object value, ref List<NavDotNet> disposeObjects)
	{
		if (value == null)
		{
			return null;
		}
		NavDotNet navDotNet = value as NavDotNet;
		if (navDotNet != null)
		{
			if (RunOnClient && !navDotNet.RunOnClient && navDotNet.serverHandle == null && navDotNet.IsNull)
			{
				navDotNet = new NavDotNet(navDotNet, navDotNet);
				navDotNet.runOnClient = true;
				navDotNet.remoteHandle = new NavClientHandle(NavCurrentThread.Session.ClientCallback.CreateDotNetTypeHandle(navDotNet.assemblyFullName, navDotNet.typeName), suppressDispose: false);
			}
			if (((ITreeObject)navDotNet).Tree.Parent is NavDotNet)
			{
				if (disposeObjects == null)
				{
					disposeObjects = new List<NavDotNet>();
				}
				disposeObjects.Add(navDotNet);
			}
			if (navDotNet.RunOnClient && navDotNet.remoteHandle == null && navDotNet.IsNull)
			{
				navDotNet.CheckTypeIsLoaded();
			}
			if (!RunOnClient && navDotNet.RunOnClient)
			{
				return GetObjectDataFromClient(navDotNet.remoteHandle);
			}
			if (navDotNet.RunOnClient)
			{
				return navDotNet.remoteHandle.Handle;
			}
		}
		if (RunOnClient && (value is Stream || value is NavInStream || value is NavOutStream))
		{
			throw new NavNCLNotSupportedTypeException(Lang._00095_50172);
		}
		INavValueMetadata navValueMetadata = null;
		NavValue navValue = value as NavValue;
		if (navValue == null)
		{
			if (value is Decimal18)
			{
				return (decimal)(Decimal18)value;
			}
			if (value.GetType().Equals(targetType))
			{
				return value;
			}
			if (typeof(string).Equals(targetType) || typeof(float).Equals(targetType) || typeof(double).Equals(targetType) || typeof(short).Equals(targetType) || typeof(int).Equals(targetType) || typeof(long).Equals(targetType) || typeof(ushort).Equals(targetType) || typeof(uint).Equals(targetType) || typeof(ulong).Equals(targetType) || typeof(bool).Equals(targetType) || typeof(char).Equals(targetType) || typeof(sbyte).Equals(targetType) || typeof(byte).Equals(targetType))
			{
				try
				{
					return Convert.ChangeType(value, targetType, NavCurrentThread.Session.WindowsCulture);
				}
				catch (OverflowException)
				{
					throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, value.GetType(), value, targetType);
				}
				catch (InvalidCastException)
				{
					throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, value.GetType(), value, targetType);
				}
			}
			if (value is byte[])
			{
				return value;
			}
			if (value is decimal)
			{
				return value;
			}
		}
		else
		{
			navValueMetadata = navValue;
		}
		if (navValue == null)
		{
			navValueMetadata = NavValueMetadata.CalcMetadataFromDotNetObject(value);
			navValue = NavValue.CreateNavValueFromObject(navValueMetadata, value);
		}
		if (typeof(Decimal18).Equals(targetType))
		{
			decimal num = Convert.ToDecimal(value, NavCurrentThread.Session.WindowsCulture);
			return num;
		}
		if (navValue is NavJsonToken navJsonToken)
		{
			return new JsonSerializableObjectContract(navJsonToken.BackingToken);
		}
		switch (navValueMetadata.NclType)
		{
		case NavNclType.NavBigInteger:
			return ((NavBigInteger)navValue).Value;
		case NavNclType.NavBigText:
			return ((NavBigText)navValue).Value;
		case NavNclType.NavBlob:
			return ((NavBLOB)navValue).GetBytes();
		case NavNclType.NavBoolean:
			return ((NavBoolean)navValue).Value;
		case NavNclType.NavByte:
			return ((NavByte)navValue).Value;
		case NavNclType.NavOemCode:
			return ((NavOemCode)navValue).Value;
		case NavNclType.NavDate:
			return ((NavDate)navValue).Value;
		case NavNclType.NavDateFormula:
			return ((NavDateFormula)navValue).Value;
		case NavNclType.NavDateTime:
			return ((NavDateTime)navValue).Value;
		case NavNclType.NavDecimal:
			return (decimal)((NavDecimal)navValue).Value;
		case NavNclType.NavDuration:
			return (TimeSpan)(NavDuration)navValue;
		case NavNclType.NavGuid:
			return ((NavGuid)navValue).Value;
		case NavNclType.NavInteger:
			return ((NavInteger)navValue).Value;
		case NavNclType.NavOption:
			return ((NavOption)navValue).Value;
		case NavNclType.NavRecordId:
			return ((NavRecordId)navValue).GetBytes();
		case NavNclType.NavTableFilter:
			return ((NavTableFilter)navValue).Value;
		case NavNclType.NavOemText:
			return ((NavOemText)navValue).Value;
		case NavNclType.NavTextConstant:
			return ((NavTextConstant)navValue).Value;
		case NavNclType.NavTime:
			return ((NavTime)navValue).Value;
		case NavNclType.NavVariant:
		{
			NavVariant navVariant = (NavVariant)navValue;
			if (navVariant.IsInitialized)
			{
				object dotNetObjectFromNavObject = GetDotNetObjectFromNavObject(navVariant.InnerValue.GetType(), navVariant.InnerValue, ref disposeObjects);
				if (dotNetObjectFromNavObject != null)
				{
					return GetDotNetObjectFromNavObject(targetType, dotNetObjectFromNavObject, ref disposeObjects);
				}
				return null;
			}
			return null;
		}
		case NavNclType.NavDotNet:
			return ((NavDotNet)navValue).Value;
		case NavNclType.NavInStream:
			return ((NavInStream)navValue).InternalStreamChecked;
		case NavNclType.NavOutStream:
			return ((NavOutStream)navValue).InternalStreamChecked;
		case NavNclType.NavStream:
			return ((NavStream)navValue).InternalStreamChecked;
		case NavNclType.NavText:
			return ((NavText)navValue).Value;
		case NavNclType.NavCode:
			return ((NavCode)navValue).Value;
		case NavNclType.NavChar:
			return ((NavChar)navValue).Value;
		default:
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, navValueMetadata.NavType.ToString());
		}
	}

	internal NavAutomationHandle GetRemoteHandle()
	{
		if (!RunOnClient)
		{
			return null;
		}
		return remoteHandle.Handle;
	}

	internal object GetNavObjectFromDotNetObject(Type targetType, object value)
	{
		if (targetType == null)
		{
			throw new ArgumentNullException("targetType");
		}
		if (value is DBNull)
		{
			value = null;
		}
		if (targetType == typeof(string) && value == null)
		{
			return null;
		}
		if (targetType == typeof(NavVoidType))
		{
			return null;
		}
		if (value is NavAutomationHandle handle)
		{
			return new NavDotNet(this, handle);
		}
		if (targetType == typeof(Stream) && (value == null || value is Stream))
		{
			return value;
		}
		if (value is Enum && (typeof(short).Equals(targetType) || typeof(int).Equals(targetType) || typeof(long).Equals(targetType) || typeof(ushort).Equals(targetType) || typeof(uint).Equals(targetType) || typeof(ulong).Equals(targetType) || typeof(sbyte).Equals(targetType) || typeof(char).Equals(targetType) || typeof(byte).Equals(targetType)))
		{
			try
			{
				return Convert.ChangeType(value, targetType, NavCurrentThread.Session.WindowsCulture);
			}
			catch (OverflowException)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, value.GetType(), value, targetType);
			}
			catch (InvalidCastException)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, value.GetType(), value, targetType);
			}
		}
		if (targetType == typeof(NavDotNet))
		{
			return new NavDotNet(this, value, IsResultValueOnClient(value));
		}
		INavValueMetadata navValueMetadata = NavValueMetadata.CalcMetadataFromDotNetObject(value);
		if (navValueMetadata == null || navValueMetadata.NclType == NavNclType.None)
		{
			navValueMetadata = NavValueMetadata.DefaultMetadata(NavNclTypeHelper.GetNavTypeFromNclType(NavNclTypeHelper.GetNclTypeFromType(targetType, forDotNet: true)));
		}
		if (navValueMetadata.NclType == NavNclType.NavDotNet)
		{
			return new NavDotNet(this, value, RunOnClient);
		}
		if (targetType == typeof(NavVariant))
		{
			if (navValueMetadata.NclType == NavNclType.None)
			{
				navValueMetadata = NavValueMetadata.DefaultMetadata(NavNclTypeHelper.GetNavTypeFromType(targetType));
			}
			return new NavVariant(this, NavValue.CreateNavValueFromObject(navValueMetadata, value));
		}
		if (value is DateTime dateTime)
		{
			if (targetType == typeof(NavDate))
			{
				return NavDate.Create(DateTime.SpecifyKind(dateTime, DateTimeKind.Local));
			}
			if (targetType == typeof(NavTime))
			{
				return NavTime.Create(DateTime.SpecifyKind(dateTime, DateTimeKind.Local));
			}
			if (targetType == typeof(NavDateTime))
			{
				if (dateTime.Kind == DateTimeKind.Unspecified)
				{
					dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
				}
				DateTimeReferenceFrame referenceFrame = ((!runOnClient) ? DateTimeReferenceFrame.Server : DateTimeReferenceFrame.Client);
				return NavDateTime.Create(dateTime, referenceFrame);
			}
		}
		if (value != null && targetType.Equals(value.GetType()))
		{
			return value;
		}
		if (targetType == typeof(Decimal18) && (value is decimal || value is double || value is float))
		{
			return (Decimal18)Convert.ToDecimal(value, NavCurrentThread.Session.WindowsCulture);
		}
		if (targetType == typeof(int))
		{
			if (value is short)
			{
				return Convert.ToInt32(value, NavCurrentThread.Session.WindowsCulture);
			}
		}
		else if (targetType == typeof(long))
		{
			if (value is short || value is int)
			{
				return Convert.ToInt64(value, NavCurrentThread.Session.WindowsCulture);
			}
		}
		else if (targetType == typeof(object))
		{
			if (value == null)
			{
				return NavVariant.Default(this);
			}
			if (navValueMetadata == null || navValueMetadata.NclType == NavNclType.NavByte || navValueMetadata.NclType == NavNclType.NavBoolean || navValueMetadata.NclType == NavNclType.NavInteger || navValueMetadata.NclType == NavNclType.NavBigInteger || navValueMetadata.NclType == NavNclType.NavDecimal || navValueMetadata.NclType == NavNclType.NavGuid || navValueMetadata.NclType == NavNclType.NavClrObject)
			{
				return value;
			}
			return NavValue.CreateNavValueFromObject(navValueMetadata, value);
		}
		if (typeof(NavValue).IsAssignableFrom(targetType))
		{
			return NavValue.CreateNavValueFromObject(navValueMetadata, value);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(targetType, value);
	}

	private bool IsResultValueOnClient(object value)
	{
		if (!RunOnClient)
		{
			return false;
		}
		if (value == null)
		{
			return RunOnClient;
		}
		return !NavAutomationHelper.IsKnownNavType(value.GetType());
	}

	public void Assign(NavDotNet other)
	{
		if (this != other)
		{
			Clear();
			if (other == null || other.IsNull)
			{
				return;
			}
			if (other.RunOnClient)
			{
				other.CheckClientControlAddIn();
			}
			if (!runOnClient && other.runOnClient)
			{
				serverHandle = CreateNavServerHandle(null, null, true, false, suppressDispose, true, other.remoteHandle, false);
			}
			else if (runOnClient && !other.runOnClient)
			{
				object obj = other.serverHandle.Instance;
				Type objectType = other.serverHandle.ObjectType;
				if (objectType != typeof(NavAutomationHandle) && !NavAutomationHelper.IsKnownNavType(objectType))
				{
					if (!NavAutomationHelper.IsSerializable(obj, objectType))
					{
						throw NavNCLDotNetCreateException.CreateSerializedObject(CultureInfo.CurrentCulture, objectType.Assembly.FullName, objectType.FullName, null);
					}
					try
					{
						obj = new SerializableObjectContract(other.serverHandle.Instance);
					}
					catch (SerializationException innerException)
					{
						throw NavNCLDotNetCreateException.CreateSerializedObject(CultureInfo.CurrentCulture, objectType.Assembly.FullName, objectType.FullName, innerException);
					}
					catch (SecurityException innerException2)
					{
						throw NavNCLDotNetCreateException.CreateSerializedObject(CultureInfo.CurrentCulture, objectType.Assembly.FullName, objectType.FullName, innerException2);
					}
				}
				remoteHandle = new NavClientHandle(NavCurrentThread.Session.ClientCallback.CreateDotNetHandle(assemblyFullName, typeName, Guid.Empty, null, false, obj), suppressDispose: false);
			}
			else
			{
				serverHandle = other.serverHandle;
				remoteHandle = other.remoteHandle;
				form = other.form;
				variableName = other.variableName;
			}
			interfaceType = null;
			controlId = other.controlId;
			if (remoteHandle != null)
			{
				remoteHandle.AddReference();
				if (isInterface)
				{
					int oldTypeHandle = ((remoteInterfaceTypeHandle != null) ? remoteInterfaceTypeHandle.Handle : 0);
					remoteInterfaceTypeHandle = NavCurrentThread.Session.ClientCallback.AssignDotNet(assemblyFullName, typeName, oldTypeHandle, other.remoteHandle.Handle.Handle);
				}
			}
			else if (serverHandle != null)
			{
				serverHandle.AddReference();
				if (isInterface)
				{
					try
					{
						interfaceType = NavAutomationHelper.GetInterfaceType(assemblyFullName, typeName, serverHandle.ObjectType);
					}
					catch (NavNCLDotNetCreateException)
					{
						interfaceType = TenantNavAppsAddInRetriever.GetInterfaceType(assemblyFullName, typeName, serverHandle.ObjectType);
					}
					staticType = interfaceType;
				}
				if (staticType == null && !string.IsNullOrEmpty(assemblyFullName) && !string.IsNullOrEmpty(typeName))
				{
					try
					{
						staticType = NavAutomationHelper.GetTypeFromAssembly(assemblyFullName, typeName);
					}
					catch (NavNCLDotNetCreateException)
					{
						staticType = TenantNavAppsAddInRetriever.GetTypeFromAssembly(assemblyFullName, typeName);
					}
				}
			}
			if (eventSink != null && !runOnClient)
			{
				HookUpEventSink();
			}
			isClear = false;
		}
		if (((ITreeObject)other).Tree.Parent is NavDotNet && runOnClient == other.runOnClient)
		{
			other.Dispose();
		}
	}

	private static object GetObjectDataFromClient(NavClientHandle clientHandle)
	{
		object obj = null;
		try
		{
			if (clientHandle != null && clientHandle.Handle != null)
			{
				obj = NavCurrentThread.Session.ClientCallback.InvokeAutomationMethod(new InvokeAutomationMethodRequest<object>
				{
					Handle = clientHandle.Handle.Handle,
					TypeHandle = 0,
					MethodName = "#Serialized",
					Binding = BindingFlags.Default,
					Modifiers = new ParameterModifier[0],
					Arguments = new object[0]
				});
			}
		}
		catch (Exception ex) when (!(ex is NavBaseException))
		{
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentCulture, "#Serialized", null, ex);
		}
		if (obj != null && obj is SerializableObjectContract serializedObject)
		{
			return NavAutomationHelper.GetSerializedObject(serializedObject);
		}
		return null;
	}

	private static bool IsClientValueNull(NavClientHandle clientHandle)
	{
		bool result = true;
		try
		{
			if (clientHandle != null && clientHandle.Handle != null)
			{
				object obj = NavCurrentThread.Session.ClientCallback.InvokeAutomationMethod(new InvokeAutomationMethodRequest<object>
				{
					Handle = clientHandle.Handle.Handle,
					TypeHandle = 0,
					MethodName = "#IsNull",
					Binding = BindingFlags.Default,
					Modifiers = new ParameterModifier[0],
					Arguments = new object[0]
				});
				result = (bool)obj;
			}
		}
		catch (Exception ex) when (!(ex is NavBaseException))
		{
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentCulture, "#IsNull", null, ex);
		}
		return result;
	}

	public NavDotNet Clone(ITreeObject parentForResult)
	{
		NavDotNet navDotNet = new NavDotNet(parentForResult, this);
		navDotNet.Assign(this);
		return navDotNet;
	}

	public T InvokeMethod<T>(string methodName, uint methodIndex, params object[] arguments)
	{
		return InvokeMethod<T>(isStatic: false, methodName, methodIndex, arguments);
	}

	public T InvokeStaticMethod<T>(string methodName, uint methodIndex, params object[] arguments)
	{
		CheckTypeIsLoaded();
		return InvokeMethod<T>(isStatic: true, methodName, methodIndex, arguments);
	}

	public T InvokePropertySet<T>(string propertyName, uint methodIndex, params object[] arguments)
	{
		return InvokePropertySet<T>(isStatic: false, propertyName, methodIndex, arguments);
	}

	public T InvokeStaticPropertySet<T>(string propertyName, uint methodIndex, params object[] arguments)
	{
		CheckTypeIsLoaded();
		return InvokePropertySet<T>(isStatic: true, propertyName, methodIndex, arguments);
	}

	public T InvokePropertyGet<T>(string propertyName, uint methodIndex)
	{
		return InvokePropertyGet<T>(isStatic: false, propertyName, methodIndex);
	}

	public T InvokeStaticPropertyGet<T>(string propertyName, uint methodIndex)
	{
		CheckTypeIsLoaded();
		return InvokePropertyGet<T>(isStatic: true, propertyName, methodIndex);
	}

	public T InvokePropertyGet<T>(string propertyName, uint methodIndex, params object[] dimensions)
	{
		return InvokePropertyGet<T>(isStatic: false, propertyName, methodIndex, dimensions);
	}

	public T InvokeStaticPropertyGet<T>(string propertyName, uint methodIndex, params object[] dimensions)
	{
		CheckTypeIsLoaded();
		return InvokePropertyGet<T>(isStatic: true, propertyName, methodIndex, dimensions);
	}

	public T InvokeStaticField<T>(string methodName, uint methodIndex)
	{
		object value;
		if (RunOnClient)
		{
			value = NavCurrentThread.Session.ClientCallback.CreateDotNetEnumerationHandle(assemblyFullName, typeName, methodName);
		}
		else
		{
			CheckTypeIsLoaded();
			FieldInfo field = serverHandle.ObjectType.GetField(methodName);
			if (field == null)
			{
				throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentCulture, methodName, serverHandle.ObjectType, (Exception)null);
			}
			value = field.GetValue(serverHandle.Instance);
		}
		return (T)GetNavObjectFromDotNetObject(typeof(T), value);
	}

	private T InvokeMethod<T>(bool isStatic, string methodName, uint methodIndex, params object[] arguments)
	{
		int num = arguments.Length;
		Type[] array = null;
		IByRef[] array2 = null;
		List<NavDotNet> disposeObjects = null;
		ParameterModifier modifier = ((num != 0) ? new ParameterModifier(num) : emptyParameterModifer);
		ParameterInfo[] array3 = null;
		bool flag = (methodIndex & 0x80000000u) == 2147483648u;
		if (flag && serverHandle != null && serverHandle.ObjectType != null)
		{
			array3 = GetMethodParameters(methodIndex, methodName);
		}
		for (int i = 0; i < num; i++)
		{
			IByRef byRef = arguments[i] as IByRef;
			if (byRef != null)
			{
				if (array2 == null)
				{
					array2 = new IByRef[num];
				}
				if (byRef.ObjectValue is NavDotNet navDotNet)
				{
					if (array == null)
					{
						array = new Type[num];
					}
					array[i] = NavAutomationHelper.GetTypeFromAssembly(navDotNet.assemblyFullName, navDotNet.typeName);
				}
				arguments[i] = byRef.ObjectValue;
				array2[i] = byRef;
				modifier[i] = true;
			}
			else if (!RunOnClient && arguments[i] is NavDotNet navDotNet2)
			{
				if (array == null)
				{
					array = new Type[num];
				}
				if (navDotNet2.Value == null)
				{
					array[i] = NavAutomationHelper.GetTypeFromAssembly(navDotNet2.assemblyFullName, navDotNet2.typeName);
				}
				else
				{
					array[i] = navDotNet2.Value.GetType();
				}
			}
			arguments[i] = GetDotNetObjectFromNavObject(arguments[i].GetType(), arguments[i], ref disposeObjects);
			if (flag && byRef == null && array3 != null)
			{
				Type type = ((i < array3.Length && array3[i] != null) ? array3[i].ParameterType : null);
				if (type != null && type.IsEnum && !arguments[i].GetType().IsEnum)
				{
					arguments[i] = Enum.ToObject(type, arguments[i]);
				}
			}
		}
		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.InvokeMethod;
		bindingFlags = (BindingFlags)((int)bindingFlags | (isStatic ? 72 : 4));
		object value = Invoke<T>(methodName, methodIndex & 0xFFFFFFF, bindingFlags, modifier, array, arguments);
		if (array2 != null)
		{
			for (int j = 0; j < num; j++)
			{
				if (array2[j] != null)
				{
					array2[j].ObjectValue = GetNavObjectFromDotNetObject(array2[j].ObjectType, arguments[j]);
					arguments[j] = array2[j];
				}
			}
		}
		if (disposeObjects != null)
		{
			foreach (NavDotNet item in disposeObjects)
			{
				item.Dispose();
			}
		}
		return (T)GetNavObjectFromDotNetObject(typeof(T), value);
	}

	private ParameterInfo[] GetMethodParameters(uint methodIndex, string methodName)
	{
		MethodInfo methodInfos = null;
		bool flag = (methodIndex & 0xFFFFFFF) != 268435455;
		if (!flag || !NavCurrentThread.Session.MethodParameterInfoDictionary.TryGetvalue(serverHandle.ObjectType, methodIndex & 0xFFFFFFF, out methodInfos))
		{
			MemberInfo[] member = serverHandle.ObjectType.GetMember(methodName);
			if (member != null && member.Length == 1)
			{
				methodInfos = serverHandle.ObjectType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
				if (flag)
				{
					NavCurrentThread.Session.MethodParameterInfoDictionary.InsertValue(serverHandle.ObjectType, methodIndex & 0xFFFFFFF, methodInfos);
				}
			}
		}
		if (methodInfos != null)
		{
			return methodInfos.GetParameters();
		}
		return null;
	}

	private T InvokePropertyGet<T>(bool isStatic, string propertyName, uint methodIndex)
	{
		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.GetProperty;
		bindingFlags = (BindingFlags)((int)bindingFlags | (isStatic ? 72 : 4));
		return (T)GetNavObjectFromDotNetObject(typeof(T), Invoke<T>(propertyName, methodIndex, bindingFlags, emptyParameterModifer, null, Array.Empty<object>()));
	}

	private T InvokePropertyGet<T>(bool isStatic, string propertyName, uint methodIndex, params object[] dimensions)
	{
		object[] array = new object[dimensions.Length];
		List<NavDotNet> disposeObjects = null;
		int i = 0;
		for (int num = dimensions.Length; i < num; i++)
		{
			array[i] = GetDotNetObjectFromNavObject(dimensions[i].GetType(), dimensions[i], ref disposeObjects);
		}
		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.GetProperty;
		bindingFlags = (BindingFlags)((int)bindingFlags | (isStatic ? 72 : 4));
		return (T)GetNavObjectFromDotNetObject(typeof(T), Invoke<T>(propertyName, methodIndex, bindingFlags, new ParameterModifier(dimensions.Length), null, array));
	}

	private T InvokePropertySet<T>(bool isStatic, string propertyName, uint methodIndex, params object[] arguments)
	{
		int num = arguments.Length;
		object[] array = new object[num];
		List<NavDotNet> disposeObjects = null;
		for (int i = 0; i < num; i++)
		{
			array[i] = GetDotNetObjectFromNavObject((i == num - 1) ? typeof(T) : arguments[i].GetType(), arguments[i], ref disposeObjects);
		}
		bool flag = (methodIndex & 0x80000000u) == 2147483648u;
		methodIndex &= 0xFFFFFFF;
		if (flag && serverHandle != null && serverHandle.ObjectType != null)
		{
			PropertyInfo propertyInfo = null;
			try
			{
				propertyInfo = serverHandle.ObjectType.GetProperty(propertyName);
			}
			catch (AmbiguousMatchException)
			{
			}
			if (propertyInfo != null && propertyInfo.PropertyType != null && propertyInfo.PropertyType.IsEnum && !array[num - 1].GetType().IsEnum)
			{
				array[num - 1] = ((array[num - 1].GetType() == typeof(char)) ? Enum.ToObject(propertyInfo.PropertyType, (int)(char)array[num - 1]) : Enum.ToObject(propertyInfo.PropertyType, array[num - 1]));
			}
		}
		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.SetProperty;
		bindingFlags = (BindingFlags)((int)bindingFlags | (isStatic ? 72 : 4));
		Invoke<T>(propertyName, methodIndex | 0x40000000, bindingFlags, new ParameterModifier(num), null, array);
		return default;
	}

	private object Invoke<T>(string methodName, uint methodIndex, BindingFlags flags, ParameterModifier modifier, Type[] referenceTypes, params object[] arguments)
	{
		if (IsNull && (flags & BindingFlags.Instance) == BindingFlags.Instance)
		{
			NavMethodScope currentMethodScope = NavCurrentThread.Session.CurrentMethodScope;
			throw NavNCLDotNetVariableNotCreatedException.Create(CultureInfo.CurrentCulture, currentMethodScope.ScopeName, currentMethodScope.ApplicationObject.ObjectId.ObjectType, currentMethodScope.ApplicationObject.ObjectName, methodName, TypeName);
		}
		try
		{
			object obj;
			if (RunOnClient)
			{
				CheckClientControlAddIn();
				int typeHandle = ((remoteInterfaceTypeHandle != null) ? remoteInterfaceTypeHandle.Handle : 0);
				NavForm navForm = form ?? (Tree.Parent as NavForm);
				obj = NavCurrentThread.Session.ClientCallback.InvokeAutomationMethod(new InvokeAutomationMethodRequest<object>
				{
					Handle = remoteHandle.Handle.Handle,
					TypeHandle = typeHandle,
					FormHandle = (navForm?.Handle ?? default(Guid)),
					VariableName = variableName,
					MethodName = methodName,
					Binding = flags,
					Modifiers = new ParameterModifier[1] { modifier },
					Arguments = arguments
				});
			}
			else if (interfaceType != null)
			{
				obj = NavAutomationHelper.InvokeOnInterface(methodName, flags, interfaceType, serverHandle.Instance, arguments, new ParameterModifier[1] { modifier }, NavCurrentThread.Session.WindowsCulture);
			}
			else
			{
				Type objectType = serverHandle.ObjectType;
				MethodInfo methodInfos = null;
				bool flag = (methodIndex & 0xFFFFFFF) != 268435455 && objectType.Equals(staticType);
				if (!flag || !NavCurrentThread.Session.MethodParameterInfoDictionary.TryGetvalue(objectType, methodIndex, out methodInfos))
				{
					methodInfos = NavAutomationHelper.GetMethodInfo(methodName, objectType, flags, new ParameterModifier[1] { modifier }, arguments, referenceTypes);
					if (flag)
					{
						NavCurrentThread.Session.MethodParameterInfoDictionary.InsertValue(objectType, methodIndex, methodInfos);
					}
				}
				obj = methodInfos.Invoke(serverHandle.Instance, arguments);
				if (obj == null && (object)methodInfos.ReturnType != typeof(void) && NavAutomationHelper.IsNullableType(methodInfos.ReturnType))
				{
					obj = new NavDotNet(this, null, runOnClient);
				}
			}
			if (obj != null && obj.GetType().IsEnum && (typeof(short).Equals(typeof(T)) || typeof(int).Equals(typeof(T)) || typeof(long).Equals(typeof(T)) || typeof(ushort).Equals(typeof(T)) || typeof(uint).Equals(typeof(T)) || typeof(ulong).Equals(typeof(T)) || typeof(sbyte).Equals(typeof(T)) || typeof(char).Equals(typeof(T)) || typeof(byte).Equals(typeof(T))))
			{
				obj = NavAutomationHelper.ConvertEnumToInteger(obj);
			}
			return obj;
		}
		catch (TargetInvocationException ex)
		{
			if (ex.InnerException is NavBaseException exception)
			{
				exception.Rethrow();
			}
			Exception baseException = ex.GetBaseException();
			throw CreateNavNCLDotNetInvokeException("000016L", interfaceType, methodName, serverHandle, baseException);
		}
		catch (Exception ex2) when (!(ex2 is NavBaseException))
		{
			throw CreateNavNCLDotNetInvokeException("000016N", interfaceType, methodName, serverHandle, ex2);
		}
	}

	private static NavNCLDotNetInvokeException CreateNavNCLDotNetInvokeException(string tag, Type interfaceType, string methodName, NavServerHandle serverHandle, Exception e)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "DotNet invoke error. SessionID:{0}, Interface:{1}, type:{2}, methodName:{3}", NavCurrentThread.Session.Id, interfaceType, serverHandle?.ObjectType, methodName);
		DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag(tag, Category.Runtime, e, Verbosity.Warning, DiagnosticParameter.SystemMetadata(content));
		return NavNCLDotNetInvokeException.Create(CultureInfo.CurrentCulture, methodName, serverHandle?.ObjectType, e);
	}

	private void CheckTypeIsLoaded()
	{
		if (RunOnClient)
		{
			if (remoteHandle == null)
			{
				remoteHandle = new NavClientHandle(NavCurrentThread.Session.ClientCallback.CreateDotNetHandle(assemblyFullName, typeName, (form != null) ? form.Handle : Guid.Empty, null, false), suppressDispose);
			}
		}
		else if (serverHandle == null)
		{
			try
			{
				serverHandle = CreateNavServerHandle(assemblyFullName, typeName, false, true, suppressDispose, true, null, false);
			}
			catch (Exception ex) when (!(ex is NavBaseException))
			{
				throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentCulture, assemblyFullName, typeName, ex);
			}
		}
	}

	private void CheckClientControlAddIn()
	{
		if (form == null || controlId == 0 || remoteHandle != null)
		{
			return;
		}
		NavAutomationHandle dotNetObject = NavCurrentThread.Session.ClientCallback.GetDotNetObject(form.Handle, controlId);
		if (dotNetObject == null || dotNetObject.Handle == -1)
		{
			string text = form.MasterPage.Caption;
			if (string.IsNullOrEmpty(text))
			{
				text = form.Name;
			}
			ControlDefinition controlDefinitionById = form.MetadataHelper.GetControlDefinitionById(controlId);
			string text2 = null;
			if (controlDefinitionById != null)
			{
				text2 = controlDefinitionById.Caption;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = controlDefinitionById.Name;
				}
			}
			if (string.IsNullOrEmpty(text2))
			{
				text2 = controlId.ToString(CultureInfo.CurrentCulture);
			}
			string message = string.Format(CultureInfo.CurrentCulture, Lang.DotNetControlAddInNotCreated, text, text2);
			throw new NavNCLDotNetControlAddInNotCreatedException(message);
		}
		remoteHandle = new NavClientHandle(dotNetObject, suppressDispose: true);
	}

	internal object GetEnumValue<T>()
	{
		if (IsNull)
		{
			NavMethodScope currentMethodScope = NavCurrentThread.Session.CurrentMethodScope;
			throw NavNCLDotNetVariableNotCreatedException.Create(CultureInfo.CurrentCulture, currentMethodScope.ScopeName, currentMethodScope.ApplicationObject.ObjectId.ObjectType, currentMethodScope.ApplicationObject.ObjectName, TypeName);
		}
		try
		{
			object obj;
			if (RunOnClient)
			{
				CheckClientControlAddIn();
				obj = NavCurrentThread.Session.ClientCallback.InvokeAutomationMethod(new InvokeAutomationMethodRequest<object>
				{
					Handle = remoteHandle.Handle.Handle,
					TypeHandle = 0,
					MethodName = "#Value",
					Binding = BindingFlags.Default,
					Modifiers = new ParameterModifier[0],
					Arguments = new object[0]
				});
			}
			else
			{
				obj = serverHandle.Instance;
			}
			if (obj != null && obj.GetType().IsEnum && (typeof(short).Equals(typeof(T)) || typeof(int).Equals(typeof(T)) || typeof(long).Equals(typeof(T)) || typeof(ushort).Equals(typeof(T)) || typeof(uint).Equals(typeof(T)) || typeof(ulong).Equals(typeof(T)) || typeof(sbyte).Equals(typeof(T)) || typeof(byte).Equals(typeof(T))))
			{
				obj = NavAutomationHelper.ConvertEnumToInteger(obj);
			}
			return obj;
		}
		catch (Exception ex) when (!(ex is NavBaseException))
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(NavDotNet), this, typeof(T));
		}
	}

	internal object GetValue<T>()
	{
		if (IsNull)
		{
			NavMethodScope currentMethodScope = NavCurrentThread.Session.CurrentMethodScope;
			throw NavNCLDotNetVariableNotCreatedException.Create(CultureInfo.CurrentCulture, currentMethodScope.ScopeName, currentMethodScope.ApplicationObject.ObjectId.ObjectType, currentMethodScope.ApplicationObject.ObjectName, TypeName);
		}
		try
		{
			if (RunOnClient)
			{
				CheckClientControlAddIn();
				return NavCurrentThread.Session.ClientCallback.InvokeAutomationMethod(new InvokeAutomationMethodRequest<object>
				{
					Handle = remoteHandle.Handle.Handle,
					TypeHandle = 0,
					MethodName = "#Value",
					Binding = BindingFlags.Default,
					Modifiers = new ParameterModifier[0],
					Arguments = new object[0]
				});
			}
			return serverHandle.Instance;
		}
		catch (Exception ex) when (!(ex is NavBaseException))
		{
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(NavDotNet), this, typeof(T));
		}
	}

	private void HookUpEventSink()
	{
		if (serverHandle == null || serverHandle.Instance == null || !(serverHandle.ObjectType != null) || eventSink == null)
		{
			return;
		}
		EventInfo eventInfo = null;
		try
		{
			Type type = eventSink.GetType();
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
			foreach (MethodInfo methodInfo in methods)
			{
				eventInfo = serverHandle.ObjectType.GetEvent(methodInfo.Name);
				if (eventInfo != null)
				{
					eventSink.RegisterEvent(eventInfo, serverHandle.Instance, methodInfo);
				}
			}
		}
		catch (Exception innerException)
		{
			string message = string.Format(CultureInfo.CurrentCulture, Lang.DotNetControlEventHandler, (eventInfo != null) ? eventInfo.ToString() : "");
			throw new NavNCLDotNetCreateException(message, innerException);
		}
	}

	private void TearDownEventSink()
	{
		if (eventSink != null)
		{
			eventSink.UnregisterAllEventHandlers();
		}
	}

	private NavServerHandle CreateNavServerHandle(string fullyQualifiedAssemblyName = null, string fullyQualifiedTypeName = null, bool createInstance = false, bool createType = false, bool suppressDisposeInvocation = true, bool includeAddInsTable = true, object instance = null, bool includeNavApps = false, params object[] arguments)
	{
		try
		{
			Type type = null;
			if (createInstance)
			{
				if (instance == null)
				{
					instance = NavAutomationHelper.CreateDotNetObject(assemblyFullName, typeName, arguments);
				}
				else if (instance is NavClientHandle clientHandle)
				{
					instance = GetObjectDataFromClient(clientHandle);
				}
			}
			if (createType)
			{
				type = NavAutomationHelper.GetTypeFromAssembly(assemblyFullName, typeName);
			}
			if (type == null)
			{
				type = ((instance == null) ? typeof(object) : instance.GetType());
			}
			return new NavServerHandle(type, instance, suppressDisposeInvocation);
		}
		catch (NavNCLDotNetCreateException)
		{
			if (includeAddInsTable)
			{
				string addInAssemblyLocationFromAssemblyName = NavAutomationHelper.GetAddInAssemblyLocationFromAssemblyName(assemblyFullName);
				if (addInAssemblyLocationFromAssemblyName == null)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001VE", Category.Runtime, Verbosity.Normal, "Failed to load assembly from disk, trying from add-in tables. Assembly: {0}", DiagnosticParameter.SystemMetadata(assemblyFullName));
					if (NavGlobal.SystemTenant.AddInProvider.ExtractAddInResourceFromAddInTable(assemblyFullName))
					{
						return CreateNavServerHandle(fullyQualifiedAssemblyName, fullyQualifiedTypeName, createInstance, createType, suppressDisposeInvocation, !includeAddInsTable, instance, includeNavApps, arguments);
					}
				}
				if (includeNavApps)
				{
					return CreateNavAppServerHandle(createInstance, createType, suppressDisposeInvocation, instance, arguments);
				}
			}
			throw;
		}
	}

	private NavServerHandle CreateNavAppServerHandle(bool createInstance = false, bool createType = false, bool suppressDisposeInvocation = true, object instance = null, params object[] arguments)
	{
		Type type = null;
		if (createInstance)
		{
			if (instance == null)
			{
				instance = TenantNavAppsAddInRetriever.CreateDotNetObject(assemblyFullName, typeName, arguments);
			}
			else if (instance is NavClientHandle clientHandle)
			{
				instance = GetObjectDataFromClient(clientHandle);
			}
		}
		if (createType)
		{
			type = TenantNavAppsAddInRetriever.GetTypeFromAssembly(assemblyFullName, typeName);
		}
		if (type == null)
		{
			type = ((instance == null) ? typeof(object) : instance.GetType());
		}
		return new NavServerHandle(type, instance, suppressDisposeInvocation);
	}

	public override int GetHashCode()
	{
		return runOnClient.GetHashCode();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !IsDisposed)
			{
				Clear();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
