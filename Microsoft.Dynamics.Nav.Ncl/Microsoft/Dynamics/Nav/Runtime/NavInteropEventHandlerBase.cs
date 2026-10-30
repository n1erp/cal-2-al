using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavInteropEventHandlerBase
{
	private static readonly MethodInfo genericShimMethodDefinition = typeof(NavInteropEventHandlerBase).GetMethod("HandlerShim", BindingFlags.Static | BindingFlags.NonPublic);

	private static readonly MethodInfo genericHandleEventMethodDefinition = typeof(NavInteropEventHandlerBase).GetMethod("HandleEvent", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly ConcurrentDictionary<Type, Tuple<Type, MethodInfo>> shimTypeCache = new ConcurrentDictionary<Type, Tuple<Type, MethodInfo>>();

	private readonly NavSession session;

	private Action unregisterAllHandlers;

	protected NavInteropEventHandlerBase()
	{
		session = NavCurrentThread.Session;
	}

	private void HandleEvent<TArg1, TArg2>(Action<TArg1, TArg2> innerHandler, TArg1 arg1, TArg2 arg2)
	{
		if (session.AccessLock.IsCurrentThreadExecuting)
		{
			if (NavCurrentThread.Session == null)
			{
				session.ValidateSessionIsActive();
				NavCurrentThread.Session = session;
			}
			innerHandler(arg1, arg2);
			return;
		}
		NavSession navSession = NavCurrentThread.Session;
		NavCurrentThread.Session = session;
		bool flag = false;
		bool flag2 = false;
		try
		{
			try
			{
				session.AccessLock.BeginExclusiveExecution();
			}
			catch (NavSessionTerminatedException)
			{
				return;
			}
			flag2 = true;
			if (session.ClientCallbackOverride == null)
			{
				session.ClientCallbackOverride = new HeadlessClientCallback(ProductNames.AppServerName, session);
				flag = true;
			}
			bool commit = false;
			session.BeginTransaction();
			try
			{
				innerHandler(arg1, arg2);
				commit = true;
			}
			finally
			{
				session.EndTransaction(commit);
			}
		}
		catch (Exception ex2)
		{
			session.LastException = ex2;
			LogWriterHelper.LogExceptionEvent(ex2);
			if (!(ex2 is NavSessionTerminatedException))
			{
				flag2 = false;
				ConnectionType clientConnectionType = session.ClientConnectionType;
				session.DisposeAsync().LogOrFailOnException();
				if (clientConnectionType != ConnectionType.Nas)
				{
					throw;
				}
			}
		}
		finally
		{
			NavCurrentThread.Session = navSession;
			if (flag)
			{
				session.ClientCallbackOverride = null;
			}
			if (flag2)
			{
				session.AccessLock.EndExclusiveExecution();
			}
		}
	}

	internal void RegisterEvent(EventInfo eventInfo, object eventSourceInstance, MethodInfo innerHandler)
	{
		Delegate eventHandler = CreateEventHandler(eventInfo, innerHandler);
		eventInfo.AddEventHandler(eventSourceInstance, eventHandler);
		unregisterAllHandlers = (Action)Delegate.Combine(unregisterAllHandlers, (Action)(() =>
		{
			eventInfo.RemoveEventHandler(eventSourceInstance, eventHandler);
		}));
	}

	internal void UnregisterAllEventHandlers()
	{
		if (unregisterAllHandlers != null)
		{
			unregisterAllHandlers();
			unregisterAllHandlers = null;
		}
	}

	private Delegate CreateEventHandler(EventInfo eventInfo, MethodInfo innerHandler)
	{
		if (!TryCreateSimpleHandler(eventInfo, innerHandler, out var handler))
		{
			return CreateHandlerForUncommonSignature(eventInfo, innerHandler);
		}
		return handler;
	}

	private bool TryCreateSimpleHandler(EventInfo eventInfo, MethodInfo innerHandler, out Delegate handler)
	{
		if (!shimTypeCache.TryGetValue(eventInfo.EventHandlerType, out var value))
		{
			MethodInfo method = eventInfo.EventHandlerType.GetMethod("Invoke");
			Type[] array = Array.ConvertAll(method.GetParameters(), (ParameterInfo p) => p.ParameterType);
			if (method.ReturnType != typeof(void) || array.Length != 2 || !Expression.TryGetActionType(array, out var actionType))
			{
				handler = null;
				return false;
			}
			MethodInfo item = genericShimMethodDefinition.MakeGenericMethod(array);
			value = Tuple.Create(actionType, item);
			shimTypeCache[eventInfo.EventHandlerType] = value;
		}
		Delegate firstArgument = Delegate.CreateDelegate(value.Item1, this, innerHandler);
		handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, firstArgument, value.Item2);
		return true;
	}

	private static void HandlerShim<T1, T2>(Action<T1, T2> innerHandler, T1 arg1, T2 arg2)
	{
		((NavInteropEventHandlerBase)innerHandler.Target).HandleEvent(innerHandler, arg1, arg2);
	}

	private Delegate CreateHandlerForUncommonSignature(EventInfo eventInfo, MethodInfo innerHandler)
	{
		MethodInfo method = eventInfo.EventHandlerType.GetMethod("Invoke");
		ParameterExpression[] array = Array.ConvertAll(method.GetParameters(), (ParameterInfo p) => Expression.Parameter(p.ParameterType));
		List<Expression> list = new List<Expression>();
		List<Expression> list2 = new List<Expression>();
		List<ParameterExpression> list3 = new List<ParameterExpression>();
		ParameterExpression parameterExpression = ((method.ReturnType != typeof(void)) ? Expression.Variable(method.ReturnType) : null);
		if (parameterExpression != null)
		{
			list3.Add(parameterExpression);
		}
		Expression[] array2 = new Expression[array.Length];
		for (int num = 0; num < array.Length; num++)
		{
			ParameterExpression parameterExpression2 = array[num];
			if (parameterExpression2.IsByRef)
			{
				ParameterExpression parameterExpression3 = Expression.Variable(parameterExpression2.Type);
				list3.Add(parameterExpression3);
				list.Add(Expression.Assign(parameterExpression3, parameterExpression2));
				list2.Add(Expression.Assign(parameterExpression2, parameterExpression3));
				array2[num] = parameterExpression3;
			}
			else
			{
				array2[num] = parameterExpression2;
			}
		}
		MethodCallExpression methodCallExpression = Expression.Call(Expression.Constant(this), innerHandler, array2);
		LambdaExpression arg = Expression.Lambda(typeof(Action<object, object>), (parameterExpression == null) ? ((Expression)methodCallExpression) : ((Expression)Expression.Assign(parameterExpression, methodCallExpression)), Expression.Parameter(typeof(object)), Expression.Parameter(typeof(object)));
		MethodInfo method2 = genericHandleEventMethodDefinition.MakeGenericMethod(typeof(object), typeof(object));
		MethodCallExpression item = Expression.Call(Expression.Constant(this), method2, arg, Expression.Constant(null), Expression.Constant(null));
		list.Add(item);
		if (parameterExpression != null)
		{
			list.Add(parameterExpression);
		}
		BlockExpression body = Expression.Block(list3, (list2.Count == 0) ? ((Expression)Expression.Block(list)) : ((Expression)Expression.TryFinally(Expression.Block(list), Expression.Block(list2))));
		return Expression.Lambda(eventInfo.EventHandlerType, body, array).Compile();
	}
}
