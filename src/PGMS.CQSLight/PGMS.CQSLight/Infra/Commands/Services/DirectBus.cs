﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;

namespace PGMS.CQSLight.Infra.Commands.Services
{
	public interface IBus
	{
		Task Publish<T>(T @event) where T : class, IEvent;
		//Task PublishAsync<T>(T @event) where T : class, IEvent;

		Task Publish<T>(IEnumerable<T> events) where T : class, IEvent;
		//Task PublishAsync<T>(IEnumerable<T> events) where T : class, IEvent;

		Task Send(ICommand command);
	}

	public interface IDirectBusConfigurationProvider
	{
		bool UseAutoFlush { get; }
	}

	public class DirectBus : IBus
	{
		private readonly IComponentContext context;
		private readonly IDirectBusConfigurationProvider directBusConfigurationProvider;
		private readonly ILogger<IBus> logger;

		public DirectBus(IComponentContext context, IDirectBusConfigurationProvider directBusConfigurationProvider, ILogger<IBus> logger)
		{
			this.context = context;
			this.directBusConfigurationProvider = directBusConfigurationProvider;
			this.logger = logger;
		}

		

		public async Task Send(ICommand command)
		{
			var type = typeof(IHandleCommand<>);
			var genericType = type.MakeGenericType(command.GetType());
			var service = context.Resolve(genericType);

			MethodInfo methodInfo = genericType.GetMethod("Execute");
			await (Task)methodInfo.Invoke(service, new object[] { command });
		}


		//public void Publish<T>(T @event) where T : class, IEvent
		//{
		//	var entityRepository = context.Resolve<IUnitOfWorkProvider>();

		//	using (var unitOfWork = entityRepository.GetUnitOfWork(directBusConfigurationProvider.UseAutoFlush))
		//	{
		//		using (var transaction = unitOfWork.GetTransaction())
		//		{
		//			try
		//			{
		//				InvokeEventHandlers(@event, unitOfWork);
		//				unitOfWork.Save();

		//				transaction.Commit();
		//			}
		//			catch (Exception ex)
		//			{
		//				transaction.Rollback();
		//				logger.LogError(ex.GetErrorDetails());
		//				throw;
		//			}

		//		}
		//	}

		//}

		public async Task Publish<T>(T @event) where T : class, IEvent
		{
			var entityRepository = context.Resolve<IUnitOfWorkProvider>();

			using (var unitOfWork = entityRepository.GetUnitOfWork(directBusConfigurationProvider.UseAutoFlush))
			{
				using (var transaction = await unitOfWork.GetTransactionAsync())
				{
					try
					{
						await InvokeEventHandlersAsync(@event, unitOfWork);
						await unitOfWork.SaveAsync();

						await transaction.CommitAsync();
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						logger.LogError(ex.GetErrorDetails());
						throw;
					}
					
				}
			}

		}

		//public void Publish<T>(IEnumerable<T> events) where T : class, IEvent
		//{
		//	var entityRepository = context.Resolve<IUnitOfWorkProvider>();

		//	using (var unitOfWork = entityRepository.GetUnitOfWork())
		//	{
		//		using (var transaction = unitOfWork.GetTransaction())
		//		{
		//			try
		//			{
		//				foreach (var @event in @events)
		//				{
		//					InvokeEventHandlers(@event, unitOfWork);
		//				}

		//				transaction.Commit();
		//			}
		//			catch (Exception ex)
		//			{
		//				transaction.Rollback();
		//				logger.LogError(ex.GetErrorDetails());
		//				throw;
		//			}
		//		}
		//	}
		//}

		public async Task Publish<T>(IEnumerable<T> events) where T : class, IEvent
		{
			var entityRepository = context.Resolve<IUnitOfWorkProvider>();

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				using (var transaction = await unitOfWork.GetTransactionAsync())
				{
					try
					{
						foreach (var @event in @events)
						{
							await InvokeEventHandlersAsync(@event, unitOfWork);
						}

						await transaction.CommitAsync();
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						logger.LogError(ex.GetErrorDetails());
						throw;
					}
				}
			}
		}

		//private void InvokeEventHandlers<T>(T @event, IUnitOfWork unitOfWork) where T : class, IEvent
		//{
		//	InvokeFromType(@event.GetType(), @event, unitOfWork);
		//	foreach (var i in @event.GetType().GetInterfaces())
		//	{
		//		InvokeFromType(i, @event, unitOfWork);
		//	}
		//}

		private async Task InvokeEventHandlersAsync<T>(T @event, IUnitOfWork unitOfWork) where T : class, IEvent
		{
			await InvokeFromTypeAsync(@event.GetType(), @event, unitOfWork);
			foreach (var i in @event.GetType().GetInterfaces())
			{
				await InvokeFromTypeAsync(i, @event, unitOfWork);
			}
		}

		//private void InvokeFromType<T>(Type t, T @event, IUnitOfWork unitOfWork)
		//{
		//	if (typeof(IEvent).IsAssignableFrom((Type) t))
		//	{
		//		InvokeHandler(typeof(IHandleEvent<>).MakeGenericType(new[] { t }), @event, unitOfWork);
		//		InvokeFromType(t.GetTypeInfo().BaseType, @event, unitOfWork);
		//	}
		//}

		private async Task InvokeFromTypeAsync<T>(Type t, T @event, IUnitOfWork unitOfWork)
		{
			if (typeof(IEvent).IsAssignableFrom((Type)t))
			{
				await InvokeHandlerAsync(typeof(IHandleEvent<>).MakeGenericType(new[] { t }), @event, unitOfWork);
				await InvokeFromTypeAsync(t.GetTypeInfo().BaseType, @event, unitOfWork);
			}
		}

		//private void InvokeHandler<T>(Type makeGenericType, T @event, IUnitOfWork unitOfWork)
		//{
		//	logger.LogDebug("TG: " + makeGenericType.FullName);
		//	var resolveAll = context.ResolveAll(makeGenericType);
		//	foreach (var service in resolveAll)
		//	{
		//		logger.LogDebug(service.GetType().FullName);
		//		MethodInfo methodInfo = makeGenericType.GetMethod("Handle");
		//		var result = (Task)methodInfo.Invoke(service, new object[] { @event, unitOfWork });
		//		if (result != null)
		//		{
		//			result.Wait();
		//		}

		//		unitOfWork.Save();
		//	}
		//}

		private async Task InvokeHandlerAsync<T>(Type makeGenericType, T @event, IUnitOfWork unitOfWork)
		{
			logger.LogDebug("TG: " + makeGenericType.FullName);
			var resolveAll = context.ResolveAll(makeGenericType);
			foreach (var service in resolveAll)
			{
				logger.LogDebug(service.GetType().FullName);
				MethodInfo methodInfo = makeGenericType.GetMethod("Handle");
				await (Task)methodInfo.Invoke(service, new object[] { @event, unitOfWork });

				await unitOfWork.SaveAsync();
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