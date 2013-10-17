﻿using System;
using System.Collections.Generic;

namespace nimbus
{
	public interface IHandleMarker<in TMessage>
	{
		
	}

	public interface IHandle<in TMessage> : IHandleMarker<TMessage>
	{
		void Handle(TMessage message);
	}

	public interface IHandle<in TMessage, TResult> : IHandleMarker<TMessage>
	{
		TResult Handle(TResult result, TMessage message);
	}

	public interface IHandleWithMediator<in TMessage> : IHandleMarker<TMessage>
	{
		void Handle(IMediator mediator, TMessage message);
	}

	public interface IHandleWithMediator<in TMessage, TResult> : IHandleMarker<TMessage>
	{
		TResult Handle(IMediator mediator, TResult result, TMessage message);
	}

	public class Response
	{
		public virtual Exception Exception { get; set; }
		public virtual bool Ok() { return Exception == null; }
	}

	public class Response<TResult> : Response
	{
		public virtual TResult Result { get; set; }
	}

	public interface IRegisterHandlers
	{
		void Register<TMessage, TResult>(Func<TResult> initial, Func<IHandleMarker<TMessage>[]> handlers);
	}

	public interface IMediator
	{
		Response<TResult> Send<TMessage, TResult>(TMessage message);
	}

	public class Mediator : IRegisterHandlers, IMediator
	{
		private readonly Dictionary<Type, Registration> _registrations;

		public void Register<TMessage, TResult>(Func<TResult> initial, Func<IHandleMarker<TMessage>[]> handlers)
		{
			_registrations.Add(typeof(TMessage), new Registration(() => initial(), handlers));
		}

		public Response<TResult> Send<TMessage, TResult>(TMessage message)
		{
			if (!_registrations.ContainsKey(typeof(TMessage)))
				throw new ApplicationException("No Handlers registered for " + typeof(TMessage).Name);

			var registration = _registrations[typeof(TMessage)];
			var handlers = registration.CreateHandlers();
			var response = new Response<TResult>();
			response.Result = registration.InitializeResponse();

			try
			{
				foreach (var handler in handlers)
				{
					response.Result = Dispatch(handler, message, response.Result);
				}
			}
			catch (Exception e)
			{
				response.Exception = e;
			}

			response.Result = response.Result;
			return response;
		}

		private TResult Dispatch<TMessage, TResult>(IHandle<TMessage> handler, TMessage message, TResult result)
		{
			handler.Handle(message);
			return result;
		}

		private TResult Dispatch<TMessage, TResult>(IHandle<TMessage, TResult> handler, TMessage message, TResult result)
		{
			return handler.Handle(result, message);
		}

		private TResult Dispatch<TMessage, TResult>(IHandleWithMediator<TMessage> handler, TMessage message, TResult result)
		{
			handler.Handle(this, message);
			return result;
		}

		private TResult Dispatch<TMessage, TResult>(IHandleWithMediator<TMessage, TResult> handler, TMessage message, TResult result)
		{
			return handler.Handle(this, result, message);
		}

		public Mediator()
		{
			_registrations = new Dictionary<Type, Registration>();
		}

		class Registration
		{
			public Registration(Func<dynamic> initializeResponse, Func<dynamic[]> createHandlers)
			{
				InitializeResponse = initializeResponse;
				CreateHandlers = createHandlers;
			}

			public Func<dynamic> InitializeResponse { get; private set; }
			public Func<dynamic[]> CreateHandlers { get; private set; }
		}
	}
}