using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xaml;
using System.Xaml.Permissions;
using System.Xml;
using Microsoft.Dynamics.Nav.Runtime.Workflow;
using XamlStaticHelperNamespace;

namespace Microsoft.Dynamics.Nav.Runtime;

[ComVisible(false)]
public class NavExecuteUpgradeCodeunitMethodWithRetryActivity : Activity, ISupportInitialize, ICompiledExpressionRoot
{
	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private class NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0 : CompiledDataContext
	{
		private int locationsOffset;

		private static int expectedLocationsCount;

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0(IList<LocationReference> locations, ActivityContext activityContext, bool computelocationsOffset)
			: base(locations, activityContext)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0(IList<Location> locations, bool computelocationsOffset)
			: base(locations)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0(IList<LocationReference> locationReferences)
			: base(locationReferences)
		{
		}

		internal static object GetDataContextActivitiesHelper(Activity compiledRoot, bool forImplementation)
		{
			return CompiledDataContext.GetDataContextActivities(compiledRoot, forImplementation);
		}

		internal static CompiledDataContext[] GetCompiledDataContextCacheHelper(object dataContextActivities, ActivityContext activityContext, Activity compiledRoot, bool forImplementation, int compiledDataContextCount)
		{
			return CompiledDataContext.GetCompiledDataContextCache(dataContextActivities, activityContext, compiledRoot, forImplementation, compiledDataContextCount);
		}

		public virtual void SetLocationsOffset(int locationsOffsetValue)
		{
			locationsOffset = locationsOffsetValue;
		}

		public static bool Validate(IList<LocationReference> locationReferences, bool validateLocationCount, int offset)
		{
			if (validateLocationCount && locationReferences.Count < 0)
			{
				return false;
			}
			expectedLocationsCount = 0;
			return true;
		}
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private class NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0_ForReadOnly : CompiledDataContext
	{
		private int locationsOffset;

		private static int expectedLocationsCount;

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0_ForReadOnly(IList<LocationReference> locations, ActivityContext activityContext, bool computelocationsOffset)
			: base(locations, activityContext)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0_ForReadOnly(IList<Location> locations, bool computelocationsOffset)
			: base(locations)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0_ForReadOnly(IList<LocationReference> locationReferences)
			: base(locationReferences)
		{
		}

		internal static object GetDataContextActivitiesHelper(Activity compiledRoot, bool forImplementation)
		{
			return CompiledDataContext.GetDataContextActivities(compiledRoot, forImplementation);
		}

		internal static CompiledDataContext[] GetCompiledDataContextCacheHelper(object dataContextActivities, ActivityContext activityContext, Activity compiledRoot, bool forImplementation, int compiledDataContextCount)
		{
			return CompiledDataContext.GetCompiledDataContextCache(dataContextActivities, activityContext, compiledRoot, forImplementation, compiledDataContextCount);
		}

		public virtual void SetLocationsOffset(int locationsOffsetValue)
		{
			locationsOffset = locationsOffsetValue;
		}

		public static bool Validate(IList<LocationReference> locationReferences, bool validateLocationCount, int offset)
		{
			if (validateLocationCount && locationReferences.Count < 0)
			{
				return false;
			}
			expectedLocationsCount = 0;
			return true;
		}
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private class NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1 : NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0
	{
		private int locationsOffset;

		private static int expectedLocationsCount;

		protected NavExecuteUpgradeCodeunitMethodOnErrorAction OnErrorAction;

		protected int CodeunitId;

		protected string BookmarkName
		{
			get
			{
				return (string)GetVariableValue(locationsOffset);
			}
			set
			{
				SetVariableValue(locationsOffset, value);
			}
		}

		protected string CompanyName
		{
			get
			{
				return (string)GetVariableValue(1 + locationsOffset);
			}
			set
			{
				SetVariableValue(1 + locationsOffset, value);
			}
		}

		protected string MethodName
		{
			get
			{
				return (string)GetVariableValue(3 + locationsOffset);
			}
			set
			{
				SetVariableValue(3 + locationsOffset, value);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1(IList<LocationReference> locations, ActivityContext activityContext, bool computelocationsOffset)
			: base(locations, activityContext, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1(IList<Location> locations, bool computelocationsOffset)
			: base(locations, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1(IList<LocationReference> locationReferences)
			: base(locationReferences)
		{
		}

		internal new static CompiledDataContext[] GetCompiledDataContextCacheHelper(object dataContextActivities, ActivityContext activityContext, Activity compiledRoot, bool forImplementation, int compiledDataContextCount)
		{
			return CompiledDataContext.GetCompiledDataContextCache(dataContextActivities, activityContext, compiledRoot, forImplementation, compiledDataContextCount);
		}

		public new virtual void SetLocationsOffset(int locationsOffsetValue)
		{
			locationsOffset = locationsOffsetValue;
			base.SetLocationsOffset(locationsOffset);
		}

		protected override void GetValueTypeValues()
		{
			OnErrorAction = (NavExecuteUpgradeCodeunitMethodOnErrorAction)GetVariableValue(2 + locationsOffset);
			CodeunitId = (int)GetVariableValue(4 + locationsOffset);
			base.GetValueTypeValues();
		}

		protected override void SetValueTypeValues()
		{
			SetVariableValue(2 + locationsOffset, OnErrorAction);
			SetVariableValue(4 + locationsOffset, CodeunitId);
			base.SetValueTypeValues();
		}

		public new static bool Validate(IList<LocationReference> locationReferences, bool validateLocationCount, int offset)
		{
			if (validateLocationCount && locationReferences.Count < 5)
			{
				return false;
			}
			if (validateLocationCount)
			{
				offset = locationReferences.Count - 5;
			}
			expectedLocationsCount = 5;
			if (locationReferences[offset].Name != "BookmarkName" || locationReferences[offset].Type != typeof(string))
			{
				return false;
			}
			if (locationReferences[offset + 1].Name != "CompanyName" || locationReferences[offset + 1].Type != typeof(string))
			{
				return false;
			}
			if (locationReferences[offset + 3].Name != "MethodName" || locationReferences[offset + 3].Type != typeof(string))
			{
				return false;
			}
			if (locationReferences[offset + 2].Name != "OnErrorAction" || locationReferences[offset + 2].Type != typeof(NavExecuteUpgradeCodeunitMethodOnErrorAction))
			{
				return false;
			}
			if (locationReferences[offset + 4].Name != "CodeunitId" || locationReferences[offset + 4].Type != typeof(int))
			{
				return false;
			}
			return NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0.Validate(locationReferences, validateLocationCount: false, offset);
		}
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private class NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1_ForReadOnly : NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0_ForReadOnly
	{
		private int locationsOffset;

		private static int expectedLocationsCount;

		protected NavExecuteUpgradeCodeunitMethodOnErrorAction OnErrorAction;

		protected int CodeunitId;

		protected string BookmarkName => (string)GetVariableValue(locationsOffset);

		protected string CompanyName => (string)GetVariableValue(1 + locationsOffset);

		protected string MethodName => (string)GetVariableValue(3 + locationsOffset);

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1_ForReadOnly(IList<LocationReference> locations, ActivityContext activityContext, bool computelocationsOffset)
			: base(locations, activityContext, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1_ForReadOnly(IList<Location> locations, bool computelocationsOffset)
			: base(locations, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1_ForReadOnly(IList<LocationReference> locationReferences)
			: base(locationReferences)
		{
		}

		internal new static CompiledDataContext[] GetCompiledDataContextCacheHelper(object dataContextActivities, ActivityContext activityContext, Activity compiledRoot, bool forImplementation, int compiledDataContextCount)
		{
			return CompiledDataContext.GetCompiledDataContextCache(dataContextActivities, activityContext, compiledRoot, forImplementation, compiledDataContextCount);
		}

		public new virtual void SetLocationsOffset(int locationsOffsetValue)
		{
			locationsOffset = locationsOffsetValue;
			base.SetLocationsOffset(locationsOffset);
		}

		protected override void GetValueTypeValues()
		{
			OnErrorAction = (NavExecuteUpgradeCodeunitMethodOnErrorAction)GetVariableValue(2 + locationsOffset);
			CodeunitId = (int)GetVariableValue(4 + locationsOffset);
			base.GetValueTypeValues();
		}

		public new static bool Validate(IList<LocationReference> locationReferences, bool validateLocationCount, int offset)
		{
			if (validateLocationCount && locationReferences.Count < 5)
			{
				return false;
			}
			if (validateLocationCount)
			{
				offset = locationReferences.Count - 5;
			}
			expectedLocationsCount = 5;
			if (locationReferences[offset].Name != "BookmarkName" || locationReferences[offset].Type != typeof(string))
			{
				return false;
			}
			if (locationReferences[offset + 1].Name != "CompanyName" || locationReferences[offset + 1].Type != typeof(string))
			{
				return false;
			}
			if (locationReferences[offset + 3].Name != "MethodName" || locationReferences[offset + 3].Type != typeof(string))
			{
				return false;
			}
			if (locationReferences[offset + 2].Name != "OnErrorAction" || locationReferences[offset + 2].Type != typeof(NavExecuteUpgradeCodeunitMethodOnErrorAction))
			{
				return false;
			}
			if (locationReferences[offset + 4].Name != "CodeunitId" || locationReferences[offset + 4].Type != typeof(int))
			{
				return false;
			}
			return NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0_ForReadOnly.Validate(locationReferences, validateLocationCount: false, offset);
		}
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private class NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2 : NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1
	{
		private int locationsOffset;

		private static int expectedLocationsCount;

		protected Exception Exception
		{
			get
			{
				return (Exception)GetVariableValue(5 + locationsOffset);
			}
			set
			{
				SetVariableValue(5 + locationsOffset, value);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2(IList<LocationReference> locations, ActivityContext activityContext, bool computelocationsOffset)
			: base(locations, activityContext, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2(IList<Location> locations, bool computelocationsOffset)
			: base(locations, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2(IList<LocationReference> locationReferences)
			: base(locationReferences)
		{
		}

		internal new static CompiledDataContext[] GetCompiledDataContextCacheHelper(object dataContextActivities, ActivityContext activityContext, Activity compiledRoot, bool forImplementation, int compiledDataContextCount)
		{
			return CompiledDataContext.GetCompiledDataContextCache(dataContextActivities, activityContext, compiledRoot, forImplementation, compiledDataContextCount);
		}

		public new virtual void SetLocationsOffset(int locationsOffsetValue)
		{
			locationsOffset = locationsOffsetValue;
			base.SetLocationsOffset(locationsOffset);
		}

		internal Expression __Expr1GetTree()
		{
			Expression<Func<Exception>> originalExpression = () => Exception;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public Exception __Expr1Get()
		{
			return Exception;
		}

		public Exception ValueType___Expr1Get()
		{
			GetValueTypeValues();
			return __Expr1Get();
		}

		[DebuggerHidden]
		public void __Expr1Set(Exception value)
		{
			Exception = value;
		}

		public void ValueType___Expr1Set(Exception value)
		{
			GetValueTypeValues();
			__Expr1Set(value);
			SetValueTypeValues();
		}

		public new static bool Validate(IList<LocationReference> locationReferences, bool validateLocationCount, int offset)
		{
			if (validateLocationCount && locationReferences.Count < 6)
			{
				return false;
			}
			if (validateLocationCount)
			{
				offset = locationReferences.Count - 6;
			}
			expectedLocationsCount = 6;
			if (locationReferences[offset + 5].Name != "Exception" || locationReferences[offset + 5].Type != typeof(Exception))
			{
				return false;
			}
			return NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1.Validate(locationReferences, validateLocationCount: false, offset);
		}
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private class NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly : NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1_ForReadOnly
	{
		private int locationsOffset;

		private static int expectedLocationsCount;

		protected Exception Exception => (Exception)GetVariableValue(5 + locationsOffset);

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(IList<LocationReference> locations, ActivityContext activityContext, bool computelocationsOffset)
			: base(locations, activityContext, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(IList<Location> locations, bool computelocationsOffset)
			: base(locations, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(IList<LocationReference> locationReferences)
			: base(locationReferences)
		{
		}

		internal new static CompiledDataContext[] GetCompiledDataContextCacheHelper(object dataContextActivities, ActivityContext activityContext, Activity compiledRoot, bool forImplementation, int compiledDataContextCount)
		{
			return CompiledDataContext.GetCompiledDataContextCache(dataContextActivities, activityContext, compiledRoot, forImplementation, compiledDataContextCount);
		}

		public new virtual void SetLocationsOffset(int locationsOffsetValue)
		{
			locationsOffset = locationsOffsetValue;
			base.SetLocationsOffset(locationsOffset);
		}

		internal Expression __Expr0GetTree()
		{
			Expression<Func<Exception>> originalExpression = () => (Exception)null;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public Exception __Expr0Get()
		{
			return null;
		}

		public Exception ValueType___Expr0Get()
		{
			GetValueTypeValues();
			return __Expr0Get();
		}

		internal Expression __Expr2GetTree()
		{
			Expression<Func<string>> originalExpression = () => CompanyName;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public string __Expr2Get()
		{
			return CompanyName;
		}

		public string ValueType___Expr2Get()
		{
			GetValueTypeValues();
			return __Expr2Get();
		}

		internal Expression __Expr3GetTree()
		{
			Expression<Func<string>> originalExpression = () => MethodName;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public string __Expr3Get()
		{
			return MethodName;
		}

		public string ValueType___Expr3Get()
		{
			GetValueTypeValues();
			return __Expr3Get();
		}

		internal Expression __Expr4GetTree()
		{
			Expression<Func<int>> originalExpression = () => CodeunitId;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public int __Expr4Get()
		{
			return CodeunitId;
		}

		public int ValueType___Expr4Get()
		{
			GetValueTypeValues();
			return __Expr4Get();
		}

		internal Expression __Expr7GetTree()
		{
			Expression<Func<string>> originalExpression = () => BookmarkName;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public string __Expr7Get()
		{
			return BookmarkName;
		}

		public string ValueType___Expr7Get()
		{
			GetValueTypeValues();
			return __Expr7Get();
		}

		internal Expression __Expr8GetTree()
		{
			Expression<Func<bool>> originalExpression = () => Exception != null;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public bool __Expr8Get()
		{
			return Exception != null;
		}

		public bool ValueType___Expr8Get()
		{
			GetValueTypeValues();
			return __Expr8Get();
		}

		internal Expression __Expr9GetTree()
		{
			Expression<Func<Exception>> originalExpression = () => Exception;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public Exception __Expr9Get()
		{
			return Exception;
		}

		public Exception ValueType___Expr9Get()
		{
			GetValueTypeValues();
			return __Expr9Get();
		}

		internal Expression __Expr10GetTree()
		{
			Expression<Func<NavExecuteUpgradeCodeunitMethodOnErrorAction>> originalExpression = () => OnErrorAction;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public NavExecuteUpgradeCodeunitMethodOnErrorAction __Expr10Get()
		{
			return OnErrorAction;
		}

		public NavExecuteUpgradeCodeunitMethodOnErrorAction ValueType___Expr10Get()
		{
			GetValueTypeValues();
			return __Expr10Get();
		}

		public new static bool Validate(IList<LocationReference> locationReferences, bool validateLocationCount, int offset)
		{
			if (validateLocationCount && locationReferences.Count < 6)
			{
				return false;
			}
			if (validateLocationCount)
			{
				offset = locationReferences.Count - 6;
			}
			expectedLocationsCount = 6;
			if (locationReferences[offset + 5].Name != "Exception" || locationReferences[offset + 5].Type != typeof(Exception))
			{
				return false;
			}
			return NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext1_ForReadOnly.Validate(locationReferences, validateLocationCount: false, offset);
		}
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private class NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3 : NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2
	{
		private int locationsOffset;

		private static int expectedLocationsCount;

		protected Exception exception
		{
			get
			{
				return (Exception)GetVariableValue(6 + locationsOffset);
			}
			set
			{
				SetVariableValue(6 + locationsOffset, value);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3(IList<LocationReference> locations, ActivityContext activityContext, bool computelocationsOffset)
			: base(locations, activityContext, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3(IList<Location> locations, bool computelocationsOffset)
			: base(locations, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3(IList<LocationReference> locationReferences)
			: base(locationReferences)
		{
		}

		internal new static CompiledDataContext[] GetCompiledDataContextCacheHelper(object dataContextActivities, ActivityContext activityContext, Activity compiledRoot, bool forImplementation, int compiledDataContextCount)
		{
			return CompiledDataContext.GetCompiledDataContextCache(dataContextActivities, activityContext, compiledRoot, forImplementation, compiledDataContextCount);
		}

		public new virtual void SetLocationsOffset(int locationsOffsetValue)
		{
			locationsOffset = locationsOffsetValue;
			base.SetLocationsOffset(locationsOffset);
		}

		internal Expression __Expr6GetTree()
		{
			Expression<Func<Exception>> originalExpression = () => Exception;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public Exception __Expr6Get()
		{
			return Exception;
		}

		public Exception ValueType___Expr6Get()
		{
			GetValueTypeValues();
			return __Expr6Get();
		}

		[DebuggerHidden]
		public void __Expr6Set(Exception value)
		{
			Exception = value;
		}

		public void ValueType___Expr6Set(Exception value)
		{
			GetValueTypeValues();
			__Expr6Set(value);
			SetValueTypeValues();
		}

		public new static bool Validate(IList<LocationReference> locationReferences, bool validateLocationCount, int offset)
		{
			if (validateLocationCount && locationReferences.Count < 7)
			{
				return false;
			}
			if (validateLocationCount)
			{
				offset = locationReferences.Count - 7;
			}
			expectedLocationsCount = 7;
			if (locationReferences[offset + 6].Name != "exception" || locationReferences[offset + 6].Type != typeof(Exception))
			{
				return false;
			}
			return NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2.Validate(locationReferences, validateLocationCount: false, offset);
		}
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private class NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly : NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly
	{
		private int locationsOffset;

		private static int expectedLocationsCount;

		protected Exception exception => (Exception)GetVariableValue(6 + locationsOffset);

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly(IList<LocationReference> locations, ActivityContext activityContext, bool computelocationsOffset)
			: base(locations, activityContext, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly(IList<Location> locations, bool computelocationsOffset)
			: base(locations, computelocationsOffset: false)
		{
			if (computelocationsOffset)
			{
				SetLocationsOffset(locations.Count - expectedLocationsCount);
			}
		}

		public NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly(IList<LocationReference> locationReferences)
			: base(locationReferences)
		{
		}

		internal new static CompiledDataContext[] GetCompiledDataContextCacheHelper(object dataContextActivities, ActivityContext activityContext, Activity compiledRoot, bool forImplementation, int compiledDataContextCount)
		{
			return CompiledDataContext.GetCompiledDataContextCache(dataContextActivities, activityContext, compiledRoot, forImplementation, compiledDataContextCount);
		}

		public new virtual void SetLocationsOffset(int locationsOffsetValue)
		{
			locationsOffset = locationsOffsetValue;
			base.SetLocationsOffset(locationsOffset);
		}

		internal Expression __Expr5GetTree()
		{
			Expression<Func<Exception>> originalExpression = () => exception;
			return RewriteExpressionTree(originalExpression);
		}

		[DebuggerHidden]
		public Exception __Expr5Get()
		{
			return exception;
		}

		public Exception ValueType___Expr5Get()
		{
			GetValueTypeValues();
			return __Expr5Get();
		}

		public new static bool Validate(IList<LocationReference> locationReferences, bool validateLocationCount, int offset)
		{
			if (validateLocationCount && locationReferences.Count < 7)
			{
				return false;
			}
			if (validateLocationCount)
			{
				offset = locationReferences.Count - 7;
			}
			expectedLocationsCount = 7;
			if (locationReferences[offset + 6].Name != "exception" || locationReferences[offset + 6].Type != typeof(Exception))
			{
				return false;
			}
			return NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.Validate(locationReferences, validateLocationCount: false, offset);
		}
	}

	private bool _contentLoaded;

	private InArgument<int> _CodeunitId;

	private InArgument<string> _MethodName;

	private InArgument<string> _BookmarkName;

	private InArgument<string> _CompanyName;

	private InArgument<NavExecuteUpgradeCodeunitMethodOnErrorAction> _OnErrorAction;

	private Activity rootActivity;

	private object dataContextActivities;

	private bool forImplementation = true;

	[RequiredArgument]
	public InArgument<int> CodeunitId
	{
		get
		{
			return _CodeunitId;
		}
		set
		{
			_CodeunitId = value;
		}
	}

	[RequiredArgument]
	public InArgument<string> MethodName
	{
		get
		{
			return _MethodName;
		}
		set
		{
			_MethodName = value;
		}
	}

	[RequiredArgument]
	public InArgument<string> BookmarkName
	{
		get
		{
			return _BookmarkName;
		}
		set
		{
			_BookmarkName = value;
		}
	}

	[RequiredArgument]
	public InArgument<string> CompanyName
	{
		get
		{
			return _CompanyName;
		}
		set
		{
			_CompanyName = value;
		}
	}

	[RequiredArgument]
	public InArgument<NavExecuteUpgradeCodeunitMethodOnErrorAction> OnErrorAction
	{
		get
		{
			return _OnErrorAction;
		}
		set
		{
			_OnErrorAction = value;
		}
	}

	private void BeforeInitializeComponent(ref bool isInitialized)
	{
		if (!isInitialized)
		{
			ActivityXamlServices.InitializeComponent(typeof(NavExecuteUpgradeCodeunitMethodWithRetryActivity), this);
			isInitialized = true;
		}
	}

	[GeneratedCode("XamlBuildTask", "15.0.0.0")]
	public NavExecuteUpgradeCodeunitMethodWithRetryActivity()
	{
		InitializeComponent();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("XamlBuildTask", "15.0.0.0")]
	public void InitializeComponent()
	{
		if (_contentLoaded)
		{
			return;
		}
		_contentLoaded = true;
		bool isInitialized = false;
		BeforeInitializeComponent(ref isInitialized);
		if (isInitialized)
		{
			return;
		}
		string name = FindResource();
		Stream manifestResourceStream = typeof(NavExecuteUpgradeCodeunitMethodWithRetryActivity).Assembly.GetManifestResourceStream(name);
		XmlReader xmlReader = null;
		XamlReader xamlReader = null;
		XamlObjectWriter xamlObjectWriter = null;
		try
		{
			XamlSchemaContext schemaContext = _XamlStaticHelper.SchemaContext;
			xmlReader = XmlReader.Create(manifestResourceStream);
			XamlXmlReaderSettings xamlXmlReaderSettings = new XamlXmlReaderSettings();
			xamlXmlReaderSettings.LocalAssembly = Assembly.GetExecutingAssembly();
			xamlXmlReaderSettings.AllowProtectedMembersOnRoot = true;
			xamlReader = new XamlXmlReader(xmlReader, schemaContext, xamlXmlReaderSettings);
			XamlObjectWriterSettings xamlObjectWriterSettings = new XamlObjectWriterSettings();
			xamlObjectWriterSettings.RootObjectInstance = this;
			xamlObjectWriterSettings.AccessLevel = XamlAccessLevel.PrivateAccessTo(typeof(NavExecuteUpgradeCodeunitMethodWithRetryActivity));
			xamlObjectWriter = new XamlObjectWriter(schemaContext, xamlObjectWriterSettings);
			XamlServices.Transform(xamlReader, xamlObjectWriter);
		}
		finally
		{
			((IDisposable)xmlReader)?.Dispose();
			((IDisposable)xamlReader)?.Dispose();
			((IDisposable)xamlObjectWriter)?.Dispose();
		}
	}

	[GeneratedCode("XamlBuildTask", "15.0.0.0")]
	private string FindResource()
	{
		string[] manifestResourceNames = typeof(NavExecuteUpgradeCodeunitMethodWithRetryActivity).Assembly.GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			if (text.Contains(".NavExecuteUpgradeCodeunitMethodWithRetryActivity.g.xaml") || text.Equals("NavExecuteUpgradeCodeunitMethodWithRetryActivity.g.xaml"))
			{
				return text;
			}
		}
		throw new InvalidOperationException("Resource not found.");
	}

	[GeneratedCode("XamlBuildTask", "15.0.0.0")]
	void ISupportInitialize.BeginInit()
	{
	}

	[GeneratedCode("XamlBuildTask", "15.0.0.0")]
	void ISupportInitialize.EndInit()
	{
		InitializeComponent();
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string GetLanguage()
	{
		return "C#";
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public object InvokeExpression(int expressionId, IList<LocationReference> locations, ActivityContext activityContext)
	{
		if (rootActivity == null)
		{
			rootActivity = this;
		}
		if (dataContextActivities == null)
		{
			dataContextActivities = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext0_ForReadOnly.GetDataContextActivitiesHelper(rootActivity, forImplementation);
		}
		switch (expressionId)
		{
		case 0:
		{
			CompiledDataContext[] compiledDataContextCacheHelper6 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper6[0] == null)
			{
				compiledDataContextCacheHelper6[0] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly5 = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly)compiledDataContextCacheHelper6[0];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly5.ValueType___Expr0Get();
		}
		case 1:
		{
			CompiledDataContext[] compiledDataContextCacheHelper8 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper8[1] == null)
			{
				compiledDataContextCacheHelper8[1] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2 navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2 = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2)compiledDataContextCacheHelper8[1];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2.GetLocation(navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2.ValueType___Expr1Get, navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2.ValueType___Expr1Set, expressionId, rootActivity, activityContext);
		}
		case 2:
		{
			CompiledDataContext[] compiledDataContextCacheHelper5 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper5[0] == null)
			{
				compiledDataContextCacheHelper5[0] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly4 = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly)compiledDataContextCacheHelper5[0];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly4.ValueType___Expr2Get();
		}
		case 3:
		{
			CompiledDataContext[] compiledDataContextCacheHelper3 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper3[0] == null)
			{
				compiledDataContextCacheHelper3[0] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly2 = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly)compiledDataContextCacheHelper3[0];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly2.ValueType___Expr3Get();
		}
		case 4:
		{
			CompiledDataContext[] compiledDataContextCacheHelper11 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper11[0] == null)
			{
				compiledDataContextCacheHelper11[0] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly8 = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly)compiledDataContextCacheHelper11[0];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly8.ValueType___Expr4Get();
		}
		case 5:
		{
			CompiledDataContext[] compiledDataContextCacheHelper9 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper9[2] == null)
			{
				compiledDataContextCacheHelper9[2] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly)compiledDataContextCacheHelper9[2];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly.ValueType___Expr5Get();
		}
		case 6:
		{
			CompiledDataContext[] compiledDataContextCacheHelper2 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper2[3] == null)
			{
				compiledDataContextCacheHelper2[3] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3 navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3)compiledDataContextCacheHelper2[3];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext.GetLocation(navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext.ValueType___Expr6Get, navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext.ValueType___Expr6Set, expressionId, rootActivity, activityContext);
		}
		case 7:
		{
			CompiledDataContext[] compiledDataContextCacheHelper10 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper10[0] == null)
			{
				compiledDataContextCacheHelper10[0] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly7 = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly)compiledDataContextCacheHelper10[0];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly7.ValueType___Expr7Get();
		}
		case 8:
		{
			CompiledDataContext[] compiledDataContextCacheHelper7 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper7[0] == null)
			{
				compiledDataContextCacheHelper7[0] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly6 = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly)compiledDataContextCacheHelper7[0];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly6.ValueType___Expr8Get();
		}
		case 9:
		{
			CompiledDataContext[] compiledDataContextCacheHelper4 = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper4[0] == null)
			{
				compiledDataContextCacheHelper4[0] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly3 = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly)compiledDataContextCacheHelper4[0];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly3.ValueType___Expr9Get();
		}
		case 10:
		{
			CompiledDataContext[] compiledDataContextCacheHelper = NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.GetCompiledDataContextCacheHelper(dataContextActivities, activityContext, rootActivity, forImplementation, 4);
			if (compiledDataContextCacheHelper[0] == null)
			{
				compiledDataContextCacheHelper[0] = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, activityContext, computelocationsOffset: true);
			}
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly = (NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly)compiledDataContextCacheHelper[0];
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.ValueType___Expr10Get();
		}
		default:
			return null;
		}
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public object InvokeExpression(int expressionId, IList<Location> locations)
	{
		if (rootActivity == null)
		{
			rootActivity = this;
		}
		switch (expressionId)
		{
		case 0:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly8 = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly8.ValueType___Expr0Get();
		}
		case 1:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2 navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2 = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2.GetLocation(navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2.ValueType___Expr1Get, navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2.ValueType___Expr1Set);
		}
		case 2:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly7 = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly7.ValueType___Expr2Get();
		}
		case 3:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly6 = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly6.ValueType___Expr3Get();
		}
		case 4:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly5 = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly5.ValueType___Expr4Get();
		}
		case 5:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly.ValueType___Expr5Get();
		}
		case 6:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3 navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext.GetLocation(navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext.ValueType___Expr6Get, navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext.ValueType___Expr6Set);
		}
		case 7:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly4 = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly4.ValueType___Expr7Get();
		}
		case 8:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly3 = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly3.ValueType___Expr8Get();
		}
		case 9:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly2 = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly2.ValueType___Expr9Get();
		}
		case 10:
		{
			NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly = new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locations, computelocationsOffset: true);
			return navExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.ValueType___Expr10Get();
		}
		default:
			return null;
		}
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool CanExecuteExpression(string expressionText, bool isReference, IList<LocationReference> locations, out int expressionId)
	{
		if (!isReference && expressionText == "null" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 0;
			return true;
		}
		if (isReference && expressionText == "Exception" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 1;
			return true;
		}
		if (!isReference && expressionText == "CompanyName" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 2;
			return true;
		}
		if (!isReference && expressionText == "MethodName" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 3;
			return true;
		}
		if (!isReference && expressionText == "CodeunitId" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 4;
			return true;
		}
		if (!isReference && expressionText == "exception" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 5;
			return true;
		}
		if (isReference && expressionText == "Exception" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 6;
			return true;
		}
		if (!isReference && expressionText == "BookmarkName" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 7;
			return true;
		}
		if (!isReference && expressionText == "Exception != null" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 8;
			return true;
		}
		if (!isReference && expressionText == "Exception" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 9;
			return true;
		}
		if (!isReference && expressionText == "OnErrorAction" && NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly.Validate(locations, validateLocationCount: true, 0))
		{
			expressionId = 10;
			return true;
		}
		expressionId = -1;
		return false;
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public IList<string> GetRequiredLocations(int expressionId)
	{
		return null;
	}

	[GeneratedCode("System.Activities", "4.0.0.0")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Expression GetExpressionTreeForExpression(int expressionId, IList<LocationReference> locationReferences)
	{
		return expressionId switch
		{
			0 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locationReferences).__Expr0GetTree(), 
			1 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2(locationReferences).__Expr1GetTree(), 
			2 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locationReferences).__Expr2GetTree(), 
			3 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locationReferences).__Expr3GetTree(), 
			4 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locationReferences).__Expr4GetTree(), 
			5 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3_ForReadOnly(locationReferences).__Expr5GetTree(), 
			6 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext3(locationReferences).__Expr6GetTree(), 
			7 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locationReferences).__Expr7GetTree(), 
			8 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locationReferences).__Expr8GetTree(), 
			9 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locationReferences).__Expr9GetTree(), 
			10 => new NavExecuteUpgradeCodeunitMethodWithRetryActivity_TypedDataContext2_ForReadOnly(locationReferences).__Expr10GetTree(), 
			_ => null, 
		};
	}
}
