﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Logging;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;

namespace PGMS.CQSLight.Infra.Commands.Services
{
	public interface IBus
	{
		void Publish<T>(T @event) where T : class, IEvent;
		void Publish<T>(IEnumerable<T> events) where T : class, IEvent;
		void Send(ICommand command);
	}

	public class DirectBus : IBus
	{
		private readonly IComponentContext context;
		private readonly ILogger<IBus> logger;

		public DirectBus(IComponentContext context, ILogger<IBus> logger)
		{
			this.context = context;
			this.logger = logger;
		}

		

		public void Send(ICommand command)
		{
			var type = typeof(IHandleCommand<>);
			var genericType = type.MakeGenericType(command.GetType());
			var service = context.Resolve(genericType);

			MethodInfo methodInfo = genericType.GetMethod("Execute");
			methodInfo.Invoke(service, new object[] { command });
		}

		public void Publish<T>(T @event) where T : class, IEvent
		{
			var entityRepository = context.Resolve<IUnitOfWorkProvider>();

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				using (var transaction = unitOfWork.GetTransaction())
				{
					InvokeEventHandlers(@event, unitOfWork, transaction);

					unitOfWork.Save();
				}
			}

		}

		public void Publish<T>(IEnumerable<T> events) where T : class, IEvent
		{
			var entityRepository = context.Resolve<IUnitOfWorkProvider>();

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				using (var transaction = unitOfWork.GetTransaction())
				{
					InvokeEventHandlers(@events, unitOfWork, transaction);
					unitOfWork.Save();
				}
			}
		}

		private void InvokeEventHandlers<T>(IEnumerable<T> events, IUnitOfWork unitOfWork, IUnitOfWorkTransaction transaction) where T : class, IEvent
		{
			try
			{
				foreach (var @event in @events)
				{
					InvokeFromType(@event.GetType(), @event, unitOfWork);
					foreach (var i in @event.GetType().GetInterfaces())
					{
						InvokeFromType(i, @event, unitOfWork);
					}
				}

				transaction.Commit();
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				logger.LogError(ex.GetErrorDetails());
				throw;
			}
		}

		private void InvokeEventHandlers<T>(T @event, IUnitOfWork unitOfWork, IUnitOfWorkTransaction transaction) where T : class, IEvent
		{
			try
			{
				InvokeFromType(@event.GetType(), @event, unitOfWork);
				foreach (var i in @event.GetType().GetInterfaces())
				{
					InvokeFromType(i, @event, unitOfWork);
				}

				transaction.Commit();
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				logger.LogError(ex.GetErrorDetails());
				throw;
			}
		}

		private void InvokeFromType<T>(Type t, T @event, IUnitOfWork unitOfWork)
		{
			if (typeof(IEvent).IsAssignableFrom((Type) t))
			{
				InvokeHandler(typeof(IHandleEvent<>).MakeGenericType(new[] { t }), @event, unitOfWork);
				InvokeFromType(t.GetTypeInfo().BaseType, @event, unitOfWork);
			}
		}

		private void InvokeHandler<T>(Type makeGenericType, T @event, IUnitOfWork unitOfWork)
		{
			logger.LogDebug("TG: " + makeGenericType.FullName);
			var resolveAll = context.ResolveAll(makeGenericType);
			foreach (var service in resolveAll)
			{
				logger.LogDebug(service.GetType().FullName);
				MethodInfo methodInfo = makeGenericType.GetMethod("Handle");
				methodInfo.Invoke(service, new object[] { @event, unitOfWork });
			}
		}
	}

	
	public static class AutoFacComponentContextExtension
	{
		public static IEnumerable<object> ResolveAll(this IComponentContext context, Type type)
		{
			var listGenericType = typeof(IEnumerable<>).MakeGenericType(new[] { type });
			var result = context.Resolve(listGenericType);

			return (IEnumerable<object>)result;
		}
	}
}