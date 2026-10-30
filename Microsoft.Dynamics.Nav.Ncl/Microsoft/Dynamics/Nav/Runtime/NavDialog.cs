using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavDialog : NavComplexValue, IDisposable
{
	internal class Token
	{
		private TokenType tokenType;

		private string text;

		private int fieldNumber;

		public TokenType TokenType => tokenType;

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
			}
		}

		public int FieldNumber => fieldNumber;

		public Token(TokenType tokenType)
		{
			this.tokenType = tokenType;
			fieldNumber = -1;
		}

		public Token(TokenType tokenType, string text)
		{
			this.tokenType = tokenType;
			this.text = text;
			fieldNumber = -1;
		}

		public Token(TokenType tokenType, string text, int fieldNumber)
		{
			this.tokenType = tokenType;
			Text = text;
			this.fieldNumber = fieldNumber;
		}
	}

	internal class NewTokenizer
	{
		private string tokenString;

		private int currentPosition;

		public NewTokenizer(string message)
		{
			tokenString = message;
		}

		public Token GetNext()
		{
			int i = currentPosition;
			int value = 0;
			if (i < tokenString.Length)
			{
				char c = tokenString[i];
				if (c == '#' || c == '@')
				{
					TokenType tokenType = ((c != '#') ? TokenType.IntegerParameter : TokenType.StringParameter);
					int num = i;
					i = Skip(i, c);
					StringHelper.TryParseInt32FromString(tokenString, ref i, out value);
					i = Skip(i, c);
					string text = tokenString.Substring(num, i - num);
					currentPosition = i;
					return new Token(tokenType, text, value);
				}
				int num2 = i;
				for (; i < tokenString.Length && tokenString[i] != '#' && tokenString[i] != '@'; i++)
				{
				}
				Token result = new Token(TokenType.Text, tokenString.Substring(num2, i - num2));
				currentPosition = i;
				return result;
			}
			return new Token(TokenType.EndOfString);
		}

		private int Skip(int pos, char skipChar)
		{
			while (pos < tokenString.Length && tokenString[pos] == skipChar)
			{
				pos++;
			}
			return pos;
		}
	}

	public class Factory : ElementFactory<NavDialog>
	{
		public Factory(ITreeObject parentForArray)
			: base(parentForArray)
		{
		}

		public override NavDialog CreateElement()
		{
			return new NavDialog(ParentForElements);
		}

		public override void AssignElement(NavDialog destinationElement, NavDialog sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavDialog> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone);
		}
	}

	internal class NavDialogParameter
	{
		private int label;

		private TokenType parameterType;

		private NavValue objectValue;

		private NavFormSourceExpressionGetter defaultValueGetter;

		public int Label
		{
			get
			{
				return label;
			}
			set
			{
				label = value;
			}
		}

		public TokenType ParameterType
		{
			get
			{
				return parameterType;
			}
			set
			{
				parameterType = value;
			}
		}

		public NavValue Value
		{
			get
			{
				return objectValue;
			}
			set
			{
				if (ParameterType == TokenType.IntegerParameter)
				{
					try
					{
						if (value is NavDecimal || value is NavDuration)
						{
							objectValue = NavInteger.Create(value.ToInt32());
						}
						else
						{
							objectValue = value;
						}
						return;
					}
					catch (InvalidCastException)
					{
						throw new NavNCLArgumentException(Lang._00095_50082);
					}
				}
				objectValue = ((value is NavText) ? value : new NavText(NavFormatEvaluateHelper.Format(value)));
			}
		}

		public NavFormSourceExpressionGetter DefaultValueGetter
		{
			get
			{
				return defaultValueGetter;
			}
			set
			{
				defaultValueGetter = value;
			}
		}

		public NavValue DefaultValue()
		{
			object value = DefaultValueGetter();
			return NavValue.CreateNavValueFromObject(NavValueMetadata.CalcMetadataFromObject(value), value);
		}
	}

	public enum TokenType
	{
		EndOfString,
		IntegerParameter,
		StringParameter,
		Text
	}

	private bool isOpen;

	private bool hideSubsequentDialogs;

	private bool isHiddingSubsequentDialogs;

	private bool isDialogAllowed;

	private Guid handle = Guid.NewGuid();

	private Guid automationIdField;

	private NavDialogParameter[] parameters;

	private string formatString = string.Empty;

	private const string NewLineCharacterAL = "\\";

	private const string UseTextConstant = "Use ERROR with a text constant to improve telemetry details";

	private ulong updateThreshold = (ulong)ServerUserSettings.Instance.ALUpdateDialogThreshold.Value;

	private ulong lastUpdateTime;

	private bool IsUpdateUnderThreshold
	{
		get
		{
			if (lastUpdateTime != 0L)
			{
				return NativeMethods.GetTickCount64() - lastUpdateTime <= updateThreshold;
			}
			return false;
		}
	}

	public bool ALHideSubsequentDialogs
	{
		get
		{
			return hideSubsequentDialogs;
		}
		set
		{
			if (isOpen)
			{
				throw new NavNCLDialogException(Lang.DialogIsOpen);
			}
			hideSubsequentDialogs = value;
		}
	}

	public Guid Handle => handle;

	private Guid AutomationId
	{
		get
		{
			return automationIdField;
		}
		set
		{
			automationIdField = value;
		}
	}

	private bool IsOpen
	{
		get
		{
			return isOpen;
		}
		set
		{
			isOpen = value;
		}
	}

	public override NavNclType NclType => NavNclType.NavDialog;

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	internal static string ReplaceBackSlashWithCRLF(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		return text.Replace("\\", Environment.NewLine);
	}

	internal static string ReplaceCRLFWithBackSlash(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		return text.Replace(Environment.NewLine, "\\");
	}

	private void InternalUpdate()
	{
		if (!isOpen)
		{
			throw new NavNCLDialogException(Lang._00095_50081);
		}
		if (isDialogAllowed && !IsUpdateUnderThreshold)
		{
			NavCurrentThread.Session.ClientCallback.DialogUpdate(Handle, GetParameterValues());
			lastUpdateTime = NativeMethods.GetTickCount64();
		}
	}

	internal void CloseDialogIfOpen()
	{
		if (!IsDisposed && IsOpen)
		{
			ALClose();
		}
	}

	private void UpdateValuesFromDefaults()
	{
		NavDialogParameter[] array = parameters;
		foreach (NavDialogParameter navDialogParameter in array)
		{
			if (navDialogParameter.DefaultValueGetter != null)
			{
				navDialogParameter.Value = navDialogParameter.DefaultValue();
			}
		}
	}

	private NavDialogParameter GetParameter(int label)
	{
		NavDialogParameter[] array = parameters;
		foreach (NavDialogParameter navDialogParameter in array)
		{
			if (navDialogParameter.Label == label)
			{
				return navDialogParameter;
			}
		}
		return null;
	}

	private object[] GetParameterValues()
	{
		object[] array = new object[parameters.Length];
		for (int i = 0; i < array.Length; i++)
		{
			NavValue value = parameters[i].Value;
			if (value != null)
			{
				array[i] = parameters[i].Value.ValueAsObject;
			}
			else
			{
				array[i] = parameters[i];
			}
		}
		return array;
	}

	private static bool IsWebServiceClientRequest()
	{
		return NavCurrentThread.Session.ClientConnectionType == ConnectionType.WebServiceClient;
	}

	public static void ALBeep(int frequency, int duration)
	{
		throw new NavObsoleteMethodException("Beep");
	}

	public void ALClose()
	{
		if (IsWebServiceClientRequest())
		{
			return;
		}
		if (!isOpen)
		{
			throw new NavNCLDialogException(Lang._00095_50081);
		}
		try
		{
			if (isDialogAllowed)
			{
				if (isHiddingSubsequentDialogs)
				{
					NavCurrentThread.Session.ReleaseRootDialog(this);
				}
				NavCurrentThread.Session.ClientCallback.DialogClose(Handle);
			}
			else
			{
				NavCurrentThread.Session.Diagnostics.SendTraceTag("00000R4", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "ALClose CallbackAllowed not possible, could result in orphant unclosed dialog, sessionId:{0}, reason:{1}", NavCurrentThread.Session.Id, NavCurrentThread.Session.SessionCallbackState);
			}
		}
		finally
		{
			isOpen = false;
			NavCurrentThread.Session.Company.UnregisterOpenDialog(this);
		}
	}

	public static bool ALConfirm(Guid automationId, string message)
	{
		return ALConfirm(automationId, message, false);
	}

	public static bool ALConfirm(Guid automationId, string message, bool defaultButton, params NavValue[] values)
	{
		bool confirmation = defaultButton;
		if (NavCurrentThread.Session.TestExecution.TestHandleConfirm(message, ref confirmation))
		{
			return confirmation;
		}
		return NavCurrentThread.Session.ClientCallback.DialogConfirm(ALSystemString.ALStrSubstNo(ReplaceBackSlashWithCRLF(message), values), defaultButton, automationId);
	}

	internal static void ProcessServerRequests()
	{
		if (NavCurrentThread.Session.RequestsToClient.Count > 0)
		{
			NavCurrentThread.Session.ClientCallback.ProcessServerRequests();
		}
	}

	public static void ALError(Guid automationId, string message)
	{
		ALError(automationId, message, new NavValue[0]);
	}

	public static void ALError(Guid automationId, string message, params NavValue[] values)
	{
		string source = ReplaceBackSlashWithCRLF(message);
		string text = (string.IsNullOrEmpty(message) ? string.Empty : "Use ERROR with a text constant to improve telemetry details");
		LogALErrorTelemetry(text);
		throw NavNCLDialogException.Create(automationId, ALSystemString.ALStrSubstNo(source, values), text);
	}

	public static void ALError(Guid automationId, NavTextConstant message)
	{
		ALError(automationId, message, new NavValue[0]);
	}

	public static void ALError(Guid automationId, NavTextConstant message, params NavValue[] values)
	{
		string text = ReplaceBackSlashWithCRLF(message.Value);
		LogALErrorTelemetry(text);
		throw NavNCLDialogException.Create(automationId, ALSystemString.ALStrSubstNo(text, values), text);
	}

	public static void ALError(Guid automationId, NavALErrorInfo errorInfo)
	{
		Guid distinct = Guid.NewGuid();
		string text = string.Empty;
		if (errorInfo != null && !string.IsNullOrEmpty(errorInfo.ALMessage))
		{
			text = ReplaceBackSlashWithCRLF(errorInfo.ALMessage);
			LogALErrorTelemetry(text, errorInfo, distinct);
		}
		if (errorInfo != null && errorInfo.ALErrorType == ALErrorType.Internal)
		{
			text = string.Format(CultureInfo.CurrentCulture, Lang.InternalDialogErrorMessage, distinct.ToString());
		}
		throw NavNCLDialogException.Create(automationId, text, text);
	}

	private static void LogALErrorTelemetry(string errorMessage, NavALErrorInfo errorInfo, Guid distinct)
	{
		if (!string.IsNullOrEmpty(errorMessage))
		{
			string aLCallStack = SessionContextHelper.GetALCallStack();
			string tag;
			string message;
			if (errorInfo.ALErrorType == ALErrorType.Client)
			{
				tag = "00001ZV";
				message = "Client ALError Correlation ID {0}{1}{2}{1}{3}";
			}
			else
			{
				tag = "00007XC";
				message = "Internal ALError Correlation ID {0}{1}{2}{1}{3}";
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag(tag, Category.AL, errorInfo.Verbosity, message, DiagnosticParameter.SystemMetadata(distinct), DiagnosticParameter.SystemMetadata(Environment.NewLine), new DiagnosticParameter(errorMessage, errorInfo.ALDataClassification.ConvertDataClassificationToPrivacyClassification()), DiagnosticParameter.SystemMetadata(aLCallStack));
		}
	}

	private static void LogALErrorTelemetry(string errorMessage)
	{
		if (!string.IsNullOrEmpty(errorMessage))
		{
			string aLCallStack = SessionContextHelper.GetALCallStack();
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001ZV", Category.AL, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Client ALError: " + errorMessage + Environment.NewLine + aLCallStack);
		}
	}

	public int ALInput()
	{
		return ALInput(1, null);
	}

	public int ALInput(int controlId)
	{
		return ALInput(controlId, null);
	}

	public int ALInput(int controlId, NavValue value)
	{
		throw new NavObsoleteMethodException("Input");
	}

	public static void ALMessage(Guid automationId, string message, params NavValue[] values)
	{
		string message2 = ALSystemString.ALStrSubstNo(ReplaceBackSlashWithCRLF(message), values);
		if (!NavCurrentThread.Session.TestExecution.TestHandleMessage(message2))
		{
			if (NavCurrentThread.Session.ClientConnectionType == ConnectionType.Nas || NavCurrentThread.Session.ClientConnectionType == ConnectionType.Background)
			{
				NavServerEventSource.Log.WriteALInfoMessageEvent(NavEnvironment.ServiceInstanceName, NavEnvironment.CurrentTenantId, message2);
			}
			else
			{
				NavCurrentThread.Session.ClientCallbackOrNull?.DialogMessage(message2, automationId);
			}
		}
	}

	public void ALOpen(Guid automationId, string message, params NavFormSourceExpressionGetter[] getters)
	{
		if (IsWebServiceClientRequest())
		{
			return;
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		if (getters == null)
		{
			throw new ArgumentNullException("getters");
		}
		if (isOpen)
		{
			ALClose();
		}
		AutomationId = automationId;
		List<NavDialogParameter> list = new List<NavDialogParameter>();
		StringBuilder stringBuilder = new StringBuilder();
		string message2 = ReplaceBackSlashWithCRLF(message.Replace("%", "\\%"));
		NewTokenizer newTokenizer = new NewTokenizer(message2);
		Token next;
		while ((next = newTokenizer.GetNext()).TokenType != TokenType.EndOfString)
		{
			if (next.TokenType == TokenType.Text)
			{
				stringBuilder.Append(next.Text);
				continue;
			}
			NavDialogParameter navDialogParameter = new NavDialogParameter();
			navDialogParameter.Label = next.FieldNumber;
			navDialogParameter.ParameterType = next.TokenType;
			if (next.FieldNumber > 0 && next.FieldNumber < getters.Length + 1)
			{
				navDialogParameter.DefaultValueGetter = getters[next.FieldNumber - 1];
			}
			if (next.TokenType == TokenType.IntegerParameter)
			{
				stringBuilder.Append("%d");
				navDialogParameter.Value = NavInteger.Default;
			}
			else
			{
				stringBuilder.Append("%s");
				navDialogParameter.Value = NavText.Default(2048);
			}
			list.Add(navDialogParameter);
		}
		formatString = stringBuilder.ToString();
		parameters = list.ToArray();
		UpdateValuesFromDefaults();
		try
		{
			isDialogAllowed = NavCurrentThread.Session.DialogAllowed(this);
			if (isDialogAllowed)
			{
				if (hideSubsequentDialogs)
				{
					isHiddingSubsequentDialogs = NavCurrentThread.Session.TrySetRootDialog(this);
				}
				NavCurrentThread.Session.ClientCallback.DialogOpen(Handle, AutomationId, formatString, GetParameterValues());
			}
		}
		finally
		{
			isOpen = true;
			NavCurrentThread.Session.Company.RegisterOpenDialog(this);
		}
	}

	public static int ALStrMenu(string option, Guid automationId)
	{
		return ALStrMenu(option, 1, string.Empty, automationId);
	}

	public static int ALStrMenu(string option, int defaultNumber, Guid automationId)
	{
		return ALStrMenu(option, defaultNumber, string.Empty, automationId);
	}

	public static int ALStrMenu(string option, int defaultNumber, string instruction, Guid automationId)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		int choice = defaultNumber;
		if (NavCurrentThread.Session.TestExecution.TestHandleStrMenu(option, ref choice, instruction))
		{
			return choice;
		}
		int num = NavCurrentThread.Session.ClientCallback.DialogSelectionMenu(option.Split(','), defaultNumber - 1, ReplaceBackSlashWithCRLF(instruction), automationId);
		return num + 1;
	}

	public void ALUpdate()
	{
		if (!IsWebServiceClientRequest())
		{
			if (!isOpen)
			{
				throw new NavNCLDialogException(Lang._00095_50081);
			}
			UpdateValuesFromDefaults();
			InternalUpdate();
		}
	}

	public void ALUpdate(int controlId)
	{
		if (!IsWebServiceClientRequest())
		{
			if (!isOpen)
			{
				throw new NavNCLDialogException(Lang._00095_50081);
			}
			NavDialogParameter parameter = GetParameter(controlId);
			if (parameter == null)
			{
				throw new NavNCLObjectNotFoundException();
			}
			if (parameter.DefaultValueGetter != null)
			{
				parameter.Value = parameter.DefaultValue();
				InternalUpdate();
			}
		}
	}

	public void ALUpdate(int controlId, NavValue value)
	{
		if (!IsWebServiceClientRequest())
		{
			if (!isOpen)
			{
				throw new NavNCLDialogException(Lang._00095_50081);
			}
			NavDialogParameter parameter = GetParameter(controlId);
			if (parameter == null)
			{
				throw new NavNCLObjectNotFoundException();
			}
			parameter.Value = value;
			InternalUpdate();
		}
	}

	public static void ALYield()
	{
		throw new NavObsoleteMethodException("Yield");
	}

	public void ALAssign(NavDialog other)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public NavDialog ALByValue(ITreeObject parentOfResult)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !IsDisposed && IsOpen)
			{
				ALClose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode();
	}

	public NavDialog(ITreeObject parent)
		: base(parent)
	{
	}

	public override void Clear()
	{
		if (isOpen)
		{
			ALClose();
		}
	}
}
