using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavAutomation : NavComplexValue, IDisposable
{
	public class Factory : ElementFactory<NavAutomation>
	{
		private ComponentType componentType;

		private string classId;

		private string licenseKey;

		private object eventSink;

		public Factory(ITreeObject parentForArray, ComponentType componentType, string classId, string licenseKey, object eventSink)
			: base(parentForArray)
		{
			this.componentType = componentType;
			this.classId = classId;
			this.licenseKey = licenseKey;
			this.eventSink = eventSink;
		}

		public override NavAutomation CreateElement()
		{
			return new NavAutomation(ParentForElements, componentType, classId, licenseKey, eventSink);
		}

		public override void AssignElement(NavAutomation destinationElement, NavAutomation sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavAutomation> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone, componentType, classId, licenseKey, eventSink);
		}
	}

	public enum ComponentType
	{
		Automation,
		OCX
	}

	private ComponentType componentType;

	private Guid classId = Guid.Empty;

	private readonly object eventSink;

	private IConnectionPoint eventConnectionPoint;

	private int connectionPointCookie;

	private string licenseKey;

	private bool isClear = true;

	private bool runOnClient;

	private NavServerHandle serverHandle;

	private NavClientHandle remoteHandle;

	public bool IsClear => isClear;

	public object Value
	{
		get
		{
			if (serverHandle == null)
			{
				serverHandle = new NavServerHandle(null, new object(), suppressDispose: false);
			}
			return serverHandle.Instance;
		}
	}

	public Guid ClassId => classId;

	internal bool RunOnClient => runOnClient;

	public bool IsComObject
	{
		get
		{
			if (runOnClient)
			{
				return remoteHandle.Handle.IsComObject;
			}
			if (serverHandle == null)
			{
				return false;
			}
			return serverHandle.ObjectType.IsCOMObject;
		}
	}

	internal ComponentType AutomationComponentType => componentType;

	public override NavNclType NclType => NavNclType.NavAutomation;

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public bool Create(DataError errorLevel)
	{
		return Create(errorLevel, newServer: false, clientSide: false);
	}

	public bool Create(DataError errorLevel, bool newServer)
	{
		return Create(errorLevel, newServer, clientSide: false);
	}

	public bool Create(DataError errorLevel, bool newServer, bool clientSide)
	{
		if (!IsClear)
		{
			throw new NavNCLAutomationVariableAlreadyCreatedException();
		}
		try
		{
			runOnClient = clientSide;
			if (!RunOnClient)
			{
				throw new NavNCLNotSupportedOperationException();
			}
			remoteHandle = new NavClientHandle(NavCurrentThread.Session.ClientCallback.CreateAutomationObject(ClassId, licenseKey, newServer), suppressDispose: false);
			isClear = false;
		}
		catch (Exception ex)
		{
			LogWriterHelper.LogExceptionEvent(ex);
			if (DataError.ThrowError == errorLevel)
			{
				if (ex is NavBaseException)
				{
					throw;
				}
				throw NavNCLAutomationCreateException.Create(CultureInfo.CurrentCulture, NavAutomationHelper.GetHumanReadableName(classId), classId, ex);
			}
			Clear();
			return false;
		}
		return true;
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
			serverHandle.Dispose();
		}
		remoteHandle = null;
		serverHandle = null;
	}

	public void ALAssign(NavAutomation other)
	{
		Assign(other);
	}

	public void ALAssign(object other)
	{
		if (other is NavAutomation other2)
		{
			Assign(other2);
			return;
		}
		throw new NavNCLAutomationInvokeException();
	}

	public NavAutomation ALByValue(ITreeObject parentForResult)
	{
		return Clone(parentForResult);
	}

	internal NavAutomation(ITreeObject parent)
		: base(parent)
	{
	}

	internal NavAutomation(ITreeObject parent, object value)
		: this(parent, value, eventWrapper: false)
	{
	}

	public NavAutomation(ITreeObject parent, object value, bool eventWrapper)
		: base(parent)
	{
		if (value != null)
		{
			serverHandle = new NavServerHandle(value.GetType(), value, eventWrapper);
			isClear = false;
		}
		else
		{
			isClear = true;
		}
	}

	internal NavAutomation(ITreeObject parent, NavAutomationHandle handle)
		: base(parent)
	{
		remoteHandle = new NavClientHandle(handle, suppressDispose: false);
		runOnClient = true;
		isClear = false;
	}

	public NavAutomation(ITreeObject parent, ComponentType componentType, string classId)
		: this(parent, componentType, classId, string.Empty, null)
	{
	}

	public NavAutomation(ITreeObject parent, ComponentType componentType, string classId, object eventSink)
		: this(parent, componentType, classId, string.Empty, eventSink)
	{
	}

	public NavAutomation(ITreeObject parent, ComponentType componentType, string classId, string licenseKey, object eventSink)
		: base(parent)
	{
		this.componentType = componentType;
		this.classId = new Guid(classId);
		this.licenseKey = licenseKey;
		this.eventSink = eventSink;
		runOnClient = this.componentType == ComponentType.OCX;
	}

	internal static NavAutomation Create(ITreeObject parent, object comObject)
	{
		if (comObject == null)
		{
			return new NavAutomation(parent);
		}
		return new NavAutomation(parent, comObject);
	}

	private object GetComObjectFromNavObject(Type targetType, object value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is NavAutomation { IsClear: false } navAutomation)
		{
			if (navAutomation.RunOnClient != RunOnClient)
			{
				throw new NavNCLAutomationMixedTiersException();
			}
			if (navAutomation.RunOnClient)
			{
				return navAutomation.remoteHandle.Handle;
			}
		}
		if (RunOnClient && (value is IStream || value is NavInStream || value is NavOutStream))
		{
			throw new NavNCLNotSupportedTypeException(Lang._00095_50172);
		}
		INavValueMetadata navValueMetadata = null;
		NavValue navValue = value as NavValue;
		if (navValue == null)
		{
			if (typeof(string).Equals(targetType) || typeof(float).Equals(targetType) || typeof(double).Equals(targetType) || typeof(short).Equals(targetType) || typeof(int).Equals(targetType) || typeof(long).Equals(targetType) || typeof(bool).Equals(targetType) || typeof(byte).Equals(targetType))
			{
				try
				{
					return Convert.ChangeType(value, targetType, NavCurrentThread.Session.WindowsCulture);
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
				if (IsComObject)
				{
					return new CurrencyWrapper(value);
				}
				return value;
			}
			if (value is Decimal18)
			{
				if (IsComObject)
				{
					return new CurrencyWrapper((decimal)(Decimal18)value);
				}
				return (decimal)(Decimal18)value;
			}
			if (value is DateTime)
			{
				try
				{
					return ((DateTime)value).ToOADate();
				}
				catch (OverflowException)
				{
					throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(DateTime), value, typeof(double));
				}
			}
			if (value.GetType().Equals(targetType))
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
			try
			{
				navValueMetadata = NavValueMetadata.CalcMetadataFromObject(value);
			}
			catch (NavNCLNotSupportedTypeException)
			{
				navValueMetadata = NavValueMetadata.DefaultMetadata(NavNclType.NavAutomation);
			}
			navValue = NavValue.CreateNavValueFromObject(navValueMetadata, value);
		}
		if (typeof(Decimal18).Equals(targetType))
		{
			decimal num = Convert.ToDecimal(value, NavCurrentThread.Session.WindowsCulture);
			if (IsComObject)
			{
				return new CurrencyWrapper(num);
			}
			return num;
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
			return new CurrencyWrapper((decimal)((NavDecimal)navValue).Value);
		case NavNclType.NavDuration:
			return ((NavDuration)navValue).Value;
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
			throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(NavTime), navValue, typeof(DateTime));
		case NavNclType.NavVariant:
		{
			NavVariant navVariant = (NavVariant)navValue;
			if (navVariant.IsInitialized)
			{
				return GetComObjectFromNavObject(navVariant.InnerValue.GetType(), navVariant.InnerValue);
			}
			return null;
		}
		case NavNclType.NavAutomation:
			return ((NavAutomation)navValue).Value;
		case NavNclType.NavInStream:
			return new NavComStreamWrapper((NavInStream)value);
		case NavNclType.NavOutStream:
			return new NavComStreamWrapper((NavOutStream)value);
		case NavNclType.NavStream:
			return new NavComStreamWrapper((NavStream)value);
		case NavNclType.NavClrObject:
			return ((NavByteArray)navValue).Value;
		case NavNclType.NavText:
			return ((NavText)navValue).Value;
		case NavNclType.NavCode:
			return ((NavCode)navValue).Value;
		case NavNclType.NavChar:
			return ((NavChar)navValue).Value;
		case NavNclType.NavDotNet:
		{
			NavDotNet navDotNet = (NavDotNet)navValue;
			if (navDotNet.RunOnClient)
			{
				return navDotNet.GetRemoteHandle();
			}
			throw new NavNCLAutomationMixedTiersException(string.Format(CultureInfo.CurrentCulture, Lang.DotNetServerObjectCannotApplyToAutomation, navValueMetadata.NavType.ToString()));
		}
		default:
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, navValueMetadata.NavType.ToString());
		}
	}

	private object GetNavObjectFromComObject(Type targetType, object value)
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
			return string.Empty;
		}
		if (targetType == typeof(object) && value == null)
		{
			return NavVariant.Default(this);
		}
		if (value is NavAutomationHandle navAutomationHandle)
		{
			if (typeof(short).Equals(targetType) || typeof(int).Equals(targetType) || typeof(long).Equals(targetType) || typeof(ushort).Equals(targetType) || typeof(uint).Equals(targetType) || typeof(ulong).Equals(targetType) || typeof(sbyte).Equals(targetType) || typeof(byte).Equals(targetType))
			{
				return NavCurrentThread.Session.ClientCallback.InvokeAutomationMethod(new InvokeAutomationMethodRequest<object>
				{
					Handle = navAutomationHandle.Handle,
					TypeHandle = 0,
					MethodName = "#Value",
					Binding = BindingFlags.Default,
					Modifiers = new ParameterModifier[0],
					Arguments = new object[0]
				});
			}
			if (targetType == typeof(NavAutomation) || targetType == typeof(object))
			{
				return new NavAutomation(this, navAutomationHandle);
			}
			throw NavValue.CreateUnableToCreateFromObjectException(targetType, value);
		}
		if (value is VariantWrapper variantWrapper)
		{
			value = ALCompiler.ObjectToNavValue(variantWrapper.WrappedObject);
		}
		if (value is CurrencyWrapper currencyWrapper)
		{
			value = (Decimal18)currencyWrapper.WrappedObject;
		}
		IStream stream = value as IStream;
		if (targetType == typeof(NavStream) && stream != null)
		{
			return new NavStream(this, new NavStreamProvider(stream));
		}
		if (targetType == typeof(NavAutomation))
		{
			return new NavAutomation(this, value);
		}
		if (value is Enum && (typeof(short).Equals(targetType) || typeof(int).Equals(targetType) || typeof(long).Equals(targetType) || typeof(ushort).Equals(targetType) || typeof(uint).Equals(targetType) || typeof(ulong).Equals(targetType) || typeof(sbyte).Equals(targetType) || typeof(byte).Equals(targetType)))
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
		INavValueMetadata navValueMetadata;
		try
		{
			navValueMetadata = NavValueMetadata.CalcMetadataFromObject(value);
			if (navValueMetadata == null || navValueMetadata.NclType == NavNclType.None)
			{
				navValueMetadata = NavValueMetadata.DefaultMetadata(NavNclTypeHelper.GetNavTypeFromType(targetType));
			}
			if (navValueMetadata.NclType == NavNclType.NavText && targetType == typeof(NavOemText))
			{
				navValueMetadata = NavValueMetadata.DefaultMetadata(NavNclType.NavOemText);
			}
		}
		catch (NavNCLNotSupportedTypeException)
		{
			navValueMetadata = NavValueMetadata.DefaultMetadata(NavNclType.NavAutomation);
		}
		if (navValueMetadata.NclType == NavNclType.NavAutomation)
		{
			return new NavAutomation(this, value);
		}
		if (targetType == typeof(NavVariant))
		{
			if (navValueMetadata.NclType == NavNclType.None)
			{
				navValueMetadata = NavValueMetadata.DefaultMetadata(NavNclTypeHelper.GetNavTypeFromType(targetType));
			}
			return new NavVariant(this, NavValue.CreateNavValueFromObject(navValueMetadata, value));
		}
		if (value is DateTime && targetType == typeof(NavDate))
		{
			DateTime dateTime = (DateTime)value;
			if (dateTime.Kind == DateTimeKind.Unspecified)
			{
				dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			}
			DateTimeReferenceFrame referenceFrame = ((!runOnClient) ? DateTimeReferenceFrame.Server : DateTimeReferenceFrame.Client);
			return NavDateTime.Create(dateTime, referenceFrame).DatePart;
		}
		if (value != null && targetType.Equals(value.GetType()))
		{
			return value;
		}
		if (targetType == typeof(NavDate) && value is double)
		{
			try
			{
				value = DateTime.FromOADate((double)value);
			}
			catch (ArgumentException)
			{
				throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, typeof(double), value, typeof(DateTime));
			}
		}
		if (targetType == typeof(Decimal18) && (value is decimal || value is double || value is float))
		{
			return (Decimal18)Convert.ToDecimal(value, NavCurrentThread.Session.WindowsCulture);
		}
		if (targetType == typeof(int) && value is short)
		{
			return Convert.ToInt32((short)value);
		}
		if (targetType.Equals(typeof(object)))
		{
			if (value == null)
			{
				return NavVariant.Default(this);
			}
			if (navValueMetadata == null || navValueMetadata.NclType == NavNclType.NavByte || navValueMetadata.NclType == NavNclType.NavBoolean || navValueMetadata.NclType == NavNclType.NavInteger || navValueMetadata.NclType == NavNclType.NavBigInteger || navValueMetadata.NclType == NavNclType.NavDecimal || navValueMetadata.NclType == NavNclType.NavGuid)
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

	public void Assign(NavAutomation other)
	{
		if (this == other)
		{
			return;
		}
		Clear();
		if (other == null)
		{
			return;
		}
		if (!other.IsClear)
		{
			runOnClient = other.runOnClient;
			serverHandle = other.serverHandle;
			remoteHandle = other.remoteHandle;
			if (remoteHandle != null)
			{
				remoteHandle.AddReference();
			}
			else if (serverHandle != null)
			{
				serverHandle.AddReference();
			}
			isClear = false;
		}
		if (((ITreeObject)other).Tree.Parent is NavAutomation)
		{
			other.Dispose();
		}
	}

	public NavAutomation Clone(ITreeObject parentForResult)
	{
		if (eventSink != null)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		NavAutomation navAutomation = new NavAutomation(parentForResult);
		navAutomation.componentType = componentType;
		navAutomation.classId = classId;
		navAutomation.licenseKey = licenseKey;
		navAutomation.Assign(this);
		return navAutomation;
	}

	public T InvokeMethod<T>(string methodName, params object[] arguments)
	{
		CheckClientOCXInitialized();
		if (componentType == ComponentType.OCX && !RunOnClient)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		int num = arguments.Length;
		object[] array = null;
		IByRef[] array2 = null;
		ParameterModifier modifier = ((num != 0) ? new ParameterModifier(num) : default(ParameterModifier));
		ParameterInfo[] array3 = null;
		if (serverHandle != null && serverHandle.ObjectType != null)
		{
			MethodInfo method = serverHandle.ObjectType.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
			if (method != null)
			{
				array3 = method.GetParameters();
			}
		}
		for (int i = 0; i < num; i++)
		{
			IByRef byRef = arguments[i] as IByRef;
			if (byRef != null)
			{
				if (array2 == null)
				{
					array = new object[num];
					array2 = new IByRef[num];
				}
				array[i] = byRef.ObjectValue;
				if (byRef.ObjectType == typeof(object) && IsComObject)
				{
					arguments[i] = new VariantWrapper(GetComObjectFromNavObject(typeof(object), array[i]));
				}
				else
				{
					arguments[i] = array[i];
				}
				modifier[i] = true;
				array2[i] = byRef;
			}
			arguments[i] = GetComObjectFromNavObject(arguments[i].GetType(), arguments[i]);
			if (byRef == null && !IsComObject)
			{
				Type type = ((array3 != null && i < array3.Length && array3[i] != null) ? array3[i].ParameterType : null);
				if (type != null && type.IsEnum)
				{
					arguments[i] = Enum.ToObject(type, arguments[i]);
				}
			}
		}
		BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding;
		if (num == 0)
		{
			bindingFlags |= BindingFlags.GetProperty;
		}
		object value = Invoke(methodName, bindingFlags, modifier, arguments);
		if (array2 != null)
		{
			for (int j = 0; j < num; j++)
			{
				if (array2[j] != null)
				{
					array2[j].ObjectValue = GetNavObjectFromComObject(array2[j].ObjectType, arguments[j]);
					arguments[j] = array2[j];
				}
			}
		}
		return (T)GetNavObjectFromComObject(typeof(T), value);
	}

	public T InvokePropertySet<T>(string propertyName, params object[] arguments)
	{
		CheckClientOCXInitialized();
		if (componentType == ComponentType.OCX && !RunOnClient)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		int num = arguments.Length;
		object[] array = new object[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = GetComObjectFromNavObject(arguments[i].GetType(), arguments[i]);
		}
		if (!IsComObject && serverHandle != null && serverHandle.ObjectType != null)
		{
			PropertyInfo property = serverHandle.ObjectType.GetProperty(propertyName);
			if (property != null && property.PropertyType != null && property.PropertyType.IsEnum)
			{
				array[num - 1] = Enum.ToObject(property.PropertyType, array[num - 1]);
			}
		}
		Invoke(propertyName, BindingFlags.SetProperty, new ParameterModifier(num), array);
		return default;
	}

	public T InvokePropertyGet<T>(string propertyName)
	{
		CheckClientOCXInitialized();
		if (componentType == ComponentType.OCX && !RunOnClient)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		return (T)GetNavObjectFromComObject(typeof(T), Invoke(propertyName, BindingFlags.InvokeMethod | BindingFlags.GetProperty, default));
	}

	public T InvokePropertyGet<T>(string propertyName, params object[] dimensions)
	{
		CheckClientOCXInitialized();
		if (componentType == ComponentType.OCX && !RunOnClient)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		object[] array = new object[dimensions.Length];
		int i = 0;
		for (int num = dimensions.Length; i < num; i++)
		{
			array[i] = GetComObjectFromNavObject(dimensions[i].GetType(), dimensions[i]);
		}
		return (T)GetNavObjectFromComObject(typeof(T), Invoke(propertyName, BindingFlags.InvokeMethod | BindingFlags.GetProperty, new ParameterModifier(dimensions.Length), array));
	}

	private void CheckClientOCXInitialized()
	{
		if (IsClear && componentType == ComponentType.OCX)
		{
			remoteHandle = new NavClientHandle(NavCurrentThread.Session.ClientCallback.CreateAutomationObject(ClassId, licenseKey, newServer: true), suppressDispose: false);
			isClear = false;
		}
	}

	private object Invoke(string methodName, BindingFlags flags, ParameterModifier modifier, params object[] arguments)
	{
		if (IsClear)
		{
			throw new NavNCLAutomationVariableNotCreatedException();
		}
		try
		{
			flags = flags | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
			return NavCurrentThread.Session.ClientCallback.InvokeAutomationMethod(new InvokeAutomationMethodRequest<object>
			{
				Handle = remoteHandle.Handle.Handle,
				TypeHandle = 0,
				MethodName = methodName,
				Binding = flags,
				Modifiers = new ParameterModifier[1] { modifier },
				Arguments = arguments
			});
		}
		catch (TargetInvocationException ex)
		{
			Exception baseException = ex.GetBaseException();
			if (baseException is COMException { ErrorCode: -2147319784 })
			{
				throw NavNCLAutomationInvokeException.CreateNoLanguageSupport(CultureInfo.InvariantCulture, NavCurrentThread.Session.WindowsCulture.DisplayName);
			}
			throw NavNCLAutomationInvokeException.Create(CultureInfo.InvariantCulture, methodName, baseException);
		}
		catch (Exception ex3) when (!(ex3 is NavBaseException))
		{
			throw NavNCLAutomationInvokeException.Create(CultureInfo.InvariantCulture, methodName, ex3);
		}
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
				if (eventConnectionPoint != null)
				{
					eventConnectionPoint.Unadvise(connectionPointCookie);
					connectionPointCookie = 0;
					eventConnectionPoint = null;
				}
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
