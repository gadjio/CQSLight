
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Exceptions;
using PGMS.CQSLight.Extensions;

namespace PGMS.CQSLight.Infra.Commands
{
	public abstract class BaseCommandHandlerAsync<T> : IHandleCommand<T> where T : BaseCommand
	{
		private readonly IBus bus;

		protected BaseCommandHandlerAsync(IBus bus)
		{
			this.bus = bus;
		}

		public abstract Task<List<ValidationResult>> ValidateCommand(T command);

		public abstract Task<IEvent> PublishEvent(T command);

		public async Task Execute(T command)
		{
			command.Validate(out var commandValidationResult);
			if (commandValidationResult != null && commandValidationResult.Any())
			{
				throw new DomainValidationException(GetErrorMessage(commandValidationResult), commandValidationResult);
			}

			var validationResults = await ValidateCommand(command);
			if (validationResults != null && validationResults.Any())
			{
				throw new DomainValidationException(GetErrorMessage(validationResults.ToList()), validationResults);
			}

			var @event = await PublishEvent(command);
			@event.ByUsername = command.ByUsername;
            @event.ByUserId = command.ByUserId;
			@event.AggregateId = command.AggregateRootId;

			await bus.Publish(@event);
		}

		private static string GetErrorMessage(IEnumerable<ValidationResult> commandValidationResult)
		{
			var sb = new StringBuilder();
			foreach (var result in commandValidationResult)
			{
				var prefix = "";
				if (result.MemberNames != null && result.MemberNames.Any())
				{
					prefix = result.MemberNames.Aggregate(prefix, (current, memberName) => current + memberName + " ");
					prefix = prefix + " - ";
				}

				sb.AppendLine(prefix + result.ErrorMessage);
			}
			return sb.ToString();
		}

		public Task OnFail(T command)
		{
			return Task.CompletedTask;
		}
	}



	public abstract class BaseCommandHandlerMultipleEventsAsync<T> : IHandleCommand<T> where T : BaseCommand
	{
		private readonly IBus bus;

		protected BaseCommandHandlerMultipleEventsAsync(IBus bus)
		{
			this.bus = bus;
		}

		public abstract Task<List<ValidationResult>> ValidateCommand(T command);

		public abstract Task<List<IEvent>> PublishEvents(T command);

		public async Task Execute(T command)
		{
			command.Validate(out var commandValidationResult);
			if (commandValidationResult != null && commandValidationResult.Any())
			{
				throw new DomainValidationException(GetErrorMessage(commandValidationResult), commandValidationResult);
			}

			var validationResults = await ValidateCommand(command);
			if (validationResults != null && validationResults.Any())
			{
				throw new DomainValidationException(GetErrorMessage(validationResults.ToList()), validationResults);
			}

			var @events = await PublishEvents(command);

			foreach (var @event in events)
			{
				if (string.IsNullOrEmpty(@event.ByUsername))
				{
					@event.ByUsername = command.ByUsername;
				}

                if (string.IsNullOrEmpty(@event.ByUserId))
                {
                    @event.ByUserId = command.ByUserId;
                }

				if (@event.AggregateId == Guid.Empty)
				{
					@event.AggregateId = command.AggregateRootId;
				}
			}

			await bus.Publish(@events);
		}

		private static string GetErrorMessage(IEnumerable<ValidationResult> commandValidationResult)
		{
			var sb = new StringBuilder();
			foreach (var result in commandValidationResult)
			{
				var prefix = "";
				if (result.MemberNames != null && result.MemberNames.Any())
				{
					prefix = result.MemberNames.Aggregate(prefix, (current, memberName) => current + memberName + " ");
					prefix = prefix + " - ";
				}

				sb.AppendLine(prefix + result.ErrorMessage);
			}
			return sb.ToString();
		}

		public Task OnFail(T command)
		{
			return Task.CompletedTask;
		}
	}



}