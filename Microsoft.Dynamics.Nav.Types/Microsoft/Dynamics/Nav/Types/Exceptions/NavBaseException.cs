using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[KnownType(typeof(NavExceptionFatalityScope))]
public abstract class NavBaseException : Exception
{
	private struct NavBaseExceptionMessages
	{
		public string Exception;

		public string Diagnostics;
	}

	private string navTenantId;

	private NavExceptionFatalityScope fatalityScope;

	public const string UnsafeConstructorUsedTelemetryMessage = "Message not shown because the NavBaseException(string, Exception, bool) constructor was used.";

	private const string FatalityScopeName = "FatalityScope";

	public bool SuppressMessage { get; set; }

	public bool ContainsPersonalOrRestrictedInformation => !string.IsNullOrEmpty(DiagnosticsMessage);

	public bool DiagnosticsSuppress { get; set; }

	public string DiagnosticsMessage { get; set; }

	public string NavTenantId
	{
		get
		{
			string text = navTenantId;
			if (text == null)
			{
				NavBaseException obj = InnerException as NavBaseException;
				if (obj == null)
				{
					return null;
				}
				text = obj.NavTenantId;
			}
			return text;
		}
		set
		{
			navTenantId = value;
		}
	}

	public string MessageWithoutPrivateInformation
	{
		get
		{
			string text;
			if (!ContainsPersonalOrRestrictedInformation)
			{
				text = Message;
				if (text == null)
				{
					return string.Empty;
				}
			}
			else
			{
				text = DiagnosticsMessage;
			}
			return text;
		}
	}

	public bool SuppressExceptionCreatedEvent { get; }

	public NavExceptionFatalityScope FatalityScope
	{
		get
		{
			return fatalityScope;
		}
		set
		{
			if (CanChangeFatalityScope(value))
			{
				fatalityScope = value;
			}
		}
	}

	public static event EventHandler<ExceptionCreatedEventArgs> ExceptionCreated;

	private bool CanChangeFatalityScope(NavExceptionFatalityScope newFatalityScope)
	{
		if (newFatalityScope == fatalityScope)
		{
			return true;
		}
		return fatalityScope switch
		{
			NavExceptionFatalityScope.None => true, 
			NavExceptionFatalityScope.Call => true, 
			NavExceptionFatalityScope.Page => newFatalityScope != NavExceptionFatalityScope.Call, 
			_ => false, 
		};
	}

	public NavBaseException MarkFatalityScope(NavExceptionFatalityScope exceptionFatalityScope)
	{
		FatalityScope = exceptionFatalityScope;
		return this;
	}

	protected NavBaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		SuppressMessage = info.GetBoolean("SuppressMessage");
		fatalityScope = (NavExceptionFatalityScope)info.GetInt32("FatalityScope");
	}

	protected NavBaseException()
	{
		OnExceptionCreated(this);
	}

	protected NavBaseException(bool suppressExceptionCreatedEvent)
	{
		SuppressExceptionCreatedEvent = suppressExceptionCreatedEvent;
		OnExceptionCreated(this, suppressExceptionCreatedEvent);
	}

	protected NavBaseException(string message, bool suppressExceptionCreatedEvent = false)
		: base(message)
	{
		SuppressExceptionCreatedEvent = suppressExceptionCreatedEvent;
		OnExceptionCreated(this, suppressExceptionCreatedEvent);
	}

	private NavBaseException(NavBaseExceptionMessages exceptionMessages)
		: this(exceptionMessages.Exception)
	{
		DiagnosticsMessage = exceptionMessages.Diagnostics;
	}

	private NavBaseException(NavBaseExceptionMessages exceptionMessages, Exception inner)
		: this(exceptionMessages.Exception, inner)
	{
		DiagnosticsMessage = exceptionMessages.Diagnostics;
	}

	protected NavBaseException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: this(CreateExceptionMessages(culture, message, args))
	{
	}

	protected NavBaseException(Exception inner, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: this(CreateExceptionMessages(culture, message, args), inner)
	{
	}

	private static NavBaseExceptionMessages CreateExceptionMessages(CultureInfo culture, string message, params DiagnosticParameter[] args)
	{
		NavBaseExceptionMessages result = default;
		if (args == null)
		{
			result.Exception = message;
		}
		else
		{
			string[] array = new string[args.Length];
			string[] array2 = new string[args.Length];
			bool flag = false;
			int num = 0;
			foreach (DiagnosticParameter diagnosticParameter in args)
			{
				if (PrivacyHelper.IsPersonalOrRestrictedData(diagnosticParameter))
				{
					array2[num] = "<redacted>";
					flag = true;
				}
				else
				{
					array2[num] = diagnosticParameter.Content.ToString();
				}
				array[num++] = diagnosticParameter.Content.ToString();
			}
			object[] args2 = array;
			result.Exception = string.Format(culture, message, args2);
			object diagnostics;
			if (!flag)
			{
				diagnostics = null;
			}
			else
			{
				args2 = array2;
				diagnostics = string.Format(culture, message, args2);
			}
			result.Diagnostics = (string)diagnostics;
		}
		return result;
	}

	protected NavBaseException(string message, Exception inner, bool suppressExceptionCreatedEvent = false)
		: this(message, "Message not shown because the NavBaseException(string, Exception, bool) constructor was used.", inner, suppressExceptionCreatedEvent)
	{
	}

	protected NavBaseException(string message, string diagnosticsMessage, Exception inner, bool suppressExceptionCreatedEvent = false)
		: base(message, inner)
	{
		DiagnosticsMessage = diagnosticsMessage;
		SuppressExceptionCreatedEvent = suppressExceptionCreatedEvent;
		OnExceptionCreated(this, suppressExceptionCreatedEvent);
	}

	internal static void OnExceptionCreated(Exception exception, bool suppress = false)
	{
		if (ExceptionCreated != null && !suppress)
		{
			ExceptionCreated(null, new ExceptionCreatedEventArgs(exception));
		}
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("SuppressMessage", SuppressMessage);
		info.AddValue("FatalityScope", fatalityScope);
		base.GetObjectData(info, context);
	}

	public NavBaseException Clone(Exception innerException)
	{
		NavBaseException ex = CreateCloneCore(innerException);
		UpdateCloneCore(ex);
		return ex;
	}

	protected static string ExtractExceptionMessage(Exception originalException)
	{
		if (originalException == null)
		{
			throw new ArgumentNullException("originalException");
		}
		return originalException.Message;
	}

	protected static string ExtractDiagnosticsMessage(Exception originalException)
	{
		return (originalException as NavBaseException)?.DiagnosticsMessage;
	}

	protected virtual NavBaseException CreateCloneCore(Exception innerException)
	{
		return (NavBaseException)GetType().GetConstructor(new Type[2]
		{
			typeof(string),
			typeof(Exception)
		}).Invoke(new object[2] { Message, innerException });
	}

	protected virtual void UpdateCloneCore(NavBaseException exceptionClone)
	{
		exceptionClone.SuppressMessage = SuppressMessage;
		exceptionClone.DiagnosticsMessage = DiagnosticsMessage;
		exceptionClone.DiagnosticsSuppress = DiagnosticsSuppress;
		exceptionClone.FatalityScope = FatalityScope;
		foreach (DictionaryEntry datum in Data)
		{
			if (!exceptionClone.Data.Contains(datum.Key))
			{
				exceptionClone.Data.Add(datum.Key, datum.Value);
			}
		}
	}
}
