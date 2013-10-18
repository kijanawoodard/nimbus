using System;
using System.Collections.Generic;

namespace nimbus
{
	public interface ISubscribeFor<in TMessage> { }
	public interface ISubscribeFor<in TMessage, TResult> : ISubscribeFor<TMessage> { }

	public interface IHandle<in TMessage> : ISubscribeFor<TMessage>
	{
		void Handle(TMessage message);
	}

	public interface IHandle<in TMessage, TResult> : ISubscribeFor<TMessage, TResult>
	{
		TResult Handle(TMessage message, TResult result);
	}

	public interface IHandleWithMediator<in TMessage> : ISubscribeFor<TMessage>
	{
		void Handle(IMediator mediator, TMessage message);
	}

	public interface IHandleWithMediator<in TMessage, TResult> : ISubscribeFor<TMessage, TResult>
	{
		TResult Handle(IMediator mediator, TMessage message, TResult result);
	}

	public interface ISubscribeHandlers
	{
		void Subscribe<TMessage, THandler>() where THandler : ISubscribeFor<TMessage>, new();
		void Subscribe<TMessage>(Func<ISubscribeFor<TMessage>[]> handlers);
		void Subscribe<TMessage, TResult>(Func<ISubscribeFor<TMessage>[]> handlers) where TResult : new();
		void Subscribe<TMessage, TResult>(Func<ISubscribeFor<TMessage>[]> handlers, Func<TResult> initializeResult);
		void SubscribeScalar<TMessage, TResult>(Func<ISubscribeFor<TMessage>[]> handlers);
	}

	public interface IMediator
	{
		void Send<TMessage>(TMessage message);
		TResult Send<TMessage, TResult>(TMessage message);
	}

	public class Mediator : ISubscribeHandlers, IMediator
	{
		private readonly Dictionary<Type, Subscription> _subscriptions;

		public void Subscribe<TMessage, THandler>() where THandler : ISubscribeFor<TMessage>, new()
		{
			SubscribeInternal(() => new ISubscribeFor<TMessage>[] { new THandler() }, () => new ResultTypeNotSpecifiedInSubscription());
		}

		public void Subscribe<TMessage>(Func<ISubscribeFor<TMessage>[]> handlers)
		{
			SubscribeInternal(handlers, () => new ResultTypeNotSpecifiedInSubscription());
		}

		public void Subscribe<TMessage, TResult>(Func<ISubscribeFor<TMessage>[]> handlers) where TResult : new()
		{
			SubscribeInternal(handlers, () => new TResult());
		}

		public void Subscribe<TMessage, TResult>(Func<ISubscribeFor<TMessage>[]> handlers, Func<TResult> initializeResult)
		{
			SubscribeInternal(handlers, () => initializeResult());
		}

		public void SubscribeScalar<TMessage, TResult>(Func<ISubscribeFor<TMessage>[]> handlers)
		{
			SubscribeInternal(handlers, () => default(TResult));
		}

		private void SubscribeInternal<TMessage>(Func<ISubscribeFor<TMessage>[]> handlers, Func<dynamic> initializeResult)
		{
			_subscriptions.Add(typeof(TMessage), new Subscription(handlers, initializeResult));
		}

		public void Send<TMessage>(TMessage message)
		{
			Execute(message);
		}

		public TResult Send<TMessage, TResult>(TMessage message)
		{
			return Execute(message);
		}
		
		private dynamic Execute<TMessage>(TMessage message)
		{
			if (!_subscriptions.ContainsKey(typeof(TMessage)))
				throw new ApplicationException("No Handlers subscribed for " + typeof(TMessage).Name);

			var registration = _subscriptions[typeof(TMessage)];
			var handlers = registration.CreateHandlers();
			var response = registration.InitializeResult();

			/*
			 * Use dynamic dispatch instead of if statements
			 * If you get a RuntimeBinderException, more than likely you have 
			 *		mixed types for TResult in your subscribed handlers
			 *		asked for a TResult in Send with a different type than in the subscription
			 */
			foreach (var handler in handlers)
			{
				response = Dispatch(handler, message, response);
			}

			return response;
		}

		private TResult Dispatch<TMessage, TResult>(IHandle<TMessage> handler, TMessage message, TResult result)
		{
			handler.Handle(message);
			return result;
		}

		private TResult Dispatch<TMessage, TResult>(IHandle<TMessage, TResult> handler, TMessage message, TResult result)
		{
			return handler.Handle(message, result);
		}

		private TResult Dispatch<TMessage, TResult>(IHandleWithMediator<TMessage> handler, TMessage message, TResult result)
		{
			handler.Handle(this, message);
			return result;
		}

		private TResult Dispatch<TMessage, TResult>(IHandleWithMediator<TMessage, TResult> handler, TMessage message, TResult result)
		{
			return handler.Handle(this, message, result);
		}

		public Mediator()
		{
			_subscriptions = new Dictionary<Type, Subscription>();
		}

		class Subscription
		{
			public Subscription(Func<dynamic[]> createHandlers, Func<dynamic> initializeResult)
			{
				CreateHandlers = createHandlers;
				InitializeResult = initializeResult;
			}

			public Func<dynamic[]> CreateHandlers { get; private set; }
			public Func<dynamic> InitializeResult { get; private set; }
		}

		class ResultTypeNotSpecifiedInSubscription{ }
	}
}
