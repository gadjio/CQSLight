using System;
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
		
		Task Publish<T>(IEnumerable<T> events) where T : class, IEvent;
		
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


		public async Task Publish<T>(T @event) where T : class, IEvent
		{
			var entityRepository = context.Resolve<IUnitOfWorkProvider>();

			using (var unitOfWork = entityRepository.GetUnitOfWork(directBusConfigurationProvider.UseAutoFlush))
			{
				using (var transaction = await unitOfWork.GetTransactionAsync())
				{
					try
					{
						await InvokeEventHandlersAsync(@event, unitOfWork, EventHandlerProcessingPriority.Standard);
						await unitOfWork.SaveAsync();
						
						await InvokeEventHandlersAsync(@event, unitOfWork, EventHandlerProcessingPriority.RunLast);
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
							await InvokeEventHandlersAsync(@event, unitOfWork, EventHandlerProcessingPriority.Standard);
							await unitOfWork.SaveAsync();
							await InvokeEventHandlersAsync(@event, unitOfWork, EventHandlerProcessingPriority.RunLast);
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

		

		private async Task InvokeEventHandlersAsync<T>(T @event, IUnitOfWork unitOfWork, EventHandlerProcessingPriority priority) where T : class, IEvent
		{
			await InvokeFromTypeAsync(@event.GetType(), @event, unitOfWork, priority);
			foreach (var i in @event.GetType().GetInterfaces())
			{
				await InvokeFromTypeAsync(i, @event, unitOfWork, priority);
			}
		}

		
		private async Task InvokeFromTypeAsync<T>(Type t, T @event, IUnitOfWork unitOfWork, EventHandlerProcessingPriority priority)
		{
			if (typeof(IEvent).IsAssignableFrom((Type)t))
			{
				await InvokeHandlerAsync(typeof(IHandleEvent<>).MakeGenericType(new[] { t }), @event, unitOfWork, priority);
				await InvokeFromTypeAsync(t.GetTypeInfo().BaseType, @event, unitOfWork, priority);
			}
		}

		private async Task InvokeHandlerAsync<T>(Type makeGenericType, T @event, IUnitOfWork unitOfWork, EventHandlerProcessingPriority priority)
		{
			logger.LogDebug("TG: " + makeGenericType.FullName);
			var resolveAll = context.ResolveAll(makeGenericType);
			foreach (var service in resolveAll)
			{
				if (!ValidateHandlerPriority(priority, service.GetType()))
				{
					continue;
				}

				logger.LogDebug(service.GetType().FullName);
				MethodInfo methodInfo = makeGenericType.GetMethod("Handle");
				await (Task)methodInfo.Invoke(service, new object[] { @event, unitOfWork });

				await unitOfWork.SaveAsync();
			}
		}

		private bool ValidateHandlerPriority(EventHandlerProcessingPriority currentPriority, Type handlerType)
		{
			var priorityAttribute = handlerType.GetCustomAttribute<EventHandlerPriorityAttribute>();

			var typeAttribute = priorityAttribute?.Priority ?? EventHandlerProcessingPriority.Standard;

			return typeAttribute == currentPriority;
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