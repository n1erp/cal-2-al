using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Types.Channels;

internal class ReceiveCorrelator
{
	public class AsyncResult : IAsyncResult
	{
		private readonly AsyncCallback asyncCallback;

		private WaitHandle waithandle;

		private readonly object syncRoot = new object();

		public int ChunkNumber { get; private set; }

		public ChunkingMessageInfo ResultMessage { get; private set; }

		public Exception ResultException { get; set; }

		public Guid ChunkSequenceId { get; private set; }

		public bool IsCompleted { get; private set; }

		public object AsyncState { get; private set; }

		public bool CompletedSynchronously { get; private set; }

		public WaitHandle AsyncWaitHandle => waithandle ?? (waithandle = new ManualResetEvent(IsCompleted));

		public AsyncResult(object state, AsyncCallback asyncCallback)
			: this(state, Guid.Empty, 0, asyncCallback)
		{
		}

		public AsyncResult(object state, Guid chunkSequenceId, int chunkNumber, AsyncCallback asyncCallback)
		{
			ChunkNumber = chunkNumber;
			this.asyncCallback = asyncCallback;
			AsyncState = state;
			ChunkSequenceId = chunkSequenceId;
		}

		public AsyncResult(object state, ChunkingMessageInfo resultMessage, AsyncCallback asyncCallback)
		{
			this.asyncCallback = asyncCallback;
			AsyncState = state;
			ResultMessage = resultMessage;
			CompletedSynchronously = true;
			IsCompleted = true;
			ChunkSequenceId = resultMessage.ChunkSequenceId;
			if (this.asyncCallback != null)
			{
				this.asyncCallback(this);
			}
		}

		public void Complete(ChunkingMessageInfo message)
		{
			Complete(() =>
			{
				ResultMessage = message;
			});
		}

		public void Complete(Exception exception)
		{
			Complete(() =>
			{
				ResultException = exception;
			});
		}

		private void Complete(Action setValue)
		{
			lock (syncRoot)
			{
				setValue();
				IsCompleted = true;
				Monitor.PulseAll(syncRoot);
			}
			if (asyncCallback != null)
			{
				asyncCallback(this);
			}
		}

		public void WaitUntilCompleted()
		{
			lock (syncRoot)
			{
				while (!IsCompleted)
				{
					Monitor.Wait(syncRoot);
				}
			}
		}
	}

	private readonly ChunkingDuplexSessionChannel chunkingChannel;

	private readonly IDuplexSessionChannel innerChannel;

	private readonly object syncRoot = new object();

	private readonly List<AsyncResult> existingRequests = new List<AsyncResult>();

	private readonly List<ChunkingMessageInfo> parkingLot = new List<ChunkingMessageInfo>();

	private bool isListening;

	public ReceiveCorrelator(ChunkingDuplexSessionChannel chunkingChannel, IDuplexSessionChannel innerChannel)
	{
		this.chunkingChannel = chunkingChannel;
		this.innerChannel = innerChannel;
	}

	public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback asyncCallback, object state)
	{
		timeout.EnsureInfinite();
		EnsureListening();
		ChunkingMessageInfo chunkingMessageInfo;
		lock (syncRoot)
		{
			chunkingMessageInfo = parkingLot.FirstOrDefault((ChunkingMessageInfo m) => m.ChunkNumber <= 1);
			if (chunkingMessageInfo == null)
			{
				AsyncResult asyncResult = new AsyncResult(state, asyncCallback);
				existingRequests.Add(asyncResult);
				return asyncResult;
			}
			parkingLot.Remove(chunkingMessageInfo);
		}
		return new AsyncResult(state, chunkingMessageInfo, asyncCallback);
	}

	public static bool EndTryReceive(IAsyncResult asyncResult, out ChunkingMessageInfo message)
	{
		AsyncResult asyncResult2 = (AsyncResult)asyncResult;
		asyncResult2.WaitUntilCompleted();
		if (asyncResult2.ResultException == null)
		{
			message = asyncResult2.ResultMessage;
			return true;
		}
		if (asyncResult2.ResultException is CommunicationException ex)
		{
			ChunkingHelper.MapException(ex);
		}
		throw asyncResult2.ResultException;
	}

	public bool TryGetChunkMessage(Guid chunkSequenceId, int chunkNumber, out ChunkingMessageInfo message)
	{
		bool result = EndTryReceive(BeginTryGetChunkMessage(chunkSequenceId, chunkNumber, null), out message);
		EnsureListening();
		return result;
	}

	private IAsyncResult BeginTryGetChunkMessage(Guid chunkSequenceId, int chunkNumber, AsyncCallback callback)
	{
		EnsureListening();
		lock (syncRoot)
		{
			ChunkingMessageInfo chunkingMessageInfo = parkingLot.FirstOrDefault((ChunkingMessageInfo m) => m.ChunkSequenceId == chunkSequenceId && m.ChunkNumber == chunkNumber);
			if (chunkingMessageInfo != null)
			{
				parkingLot.Remove(chunkingMessageInfo);
				return new AsyncResult(null, chunkingMessageInfo, callback);
			}
			AsyncResult asyncResult = new AsyncResult(null, chunkSequenceId, chunkNumber, null);
			existingRequests.Add(asyncResult);
			return asyncResult;
		}
	}

	private void OnNewMessage(IAsyncResult ar)
	{
		Message message;
		try
		{
			innerChannel.EndTryReceive(ar, out message);
		}
		catch (Exception exception)
		{
			OnAsyncException(exception);
			return;
		}
		if (message == null)
		{
			AsyncResult[] array;
			lock (syncRoot)
			{
				array = existingRequests.ToArray();
				existingRequests.Clear();
			}
			AsyncResult[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Complete((ChunkingMessageInfo)null);
			}
			return;
		}
		ChunkingMessageInfo messageInfo = new ChunkingMessageInfo(message);
		AsyncResult asyncResult = null;
		bool flag = true;
		lock (syncRoot)
		{
			if (messageInfo.IsCancellation)
			{
				chunkingChannel.CancelSendingChunkSequence(messageInfo.ChunkSequenceId);
			}
			else if (messageInfo.ChunkNumber == 1 || !messageInfo.IsChunk)
			{
				AsyncResult asyncResult2 = existingRequests.FirstOrDefault((AsyncResult r) => r.ChunkSequenceId == Guid.Empty);
				if (asyncResult2 != null)
				{
					asyncResult = asyncResult2;
					existingRequests.Remove(asyncResult2);
				}
				else
				{
					parkingLot.Add(messageInfo);
					if (!existingRequests.Any())
					{
						flag = false;
					}
				}
			}
			else
			{
				AsyncResult asyncResult3 = existingRequests.FirstOrDefault((AsyncResult r) => messageInfo.ChunkSequenceId == r.ChunkSequenceId && messageInfo.ChunkNumber == r.ChunkNumber);
				if (asyncResult3 != null)
				{
					asyncResult = asyncResult3;
					existingRequests.Remove(asyncResult3);
				}
				else
				{
					parkingLot.Add(messageInfo);
					if (!messageInfo.IsLastchunk && !existingRequests.Any())
					{
						flag = false;
					}
				}
			}
			isListening = flag;
		}
		if (flag)
		{
			BeginListen();
		}
		asyncResult?.Complete(messageInfo);
	}

	private void OnAsyncException(Exception exception)
	{
		AsyncResult[] array;
		lock (syncRoot)
		{
			array = existingRequests.ToArray();
			existingRequests.Clear();
			isListening = false;
		}
		AsyncResult[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Complete(exception);
		}
	}

	private void EnsureListening()
	{
		if (innerChannel.State == CommunicationState.Faulted || innerChannel.State == CommunicationState.Closed)
		{
			throw new CommunicationObjectAbortedException();
		}
		bool flag = false;
		lock (syncRoot)
		{
			if (!isListening)
			{
				flag = true;
				isListening = true;
			}
		}
		if (flag)
		{
			BeginListen();
		}
	}

	private void BeginListen()
	{
		try
		{
			innerChannel.BeginTryReceive(TimeSpan.MaxValue, OnNewMessage, null);
		}
		catch (Exception exception)
		{
			OnAsyncException(exception);
		}
	}
}
