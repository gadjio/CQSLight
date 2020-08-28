using System;

namespace Sample.Data.Models.App
{
	public class DomainEventReporting : BaseReportingWithAssignedId
	{
		public Guid EventId { get; set; }
		public string JSonDomainEvent { get; set; }
		public Guid? EventProviderId { get; set; }

		public string Type { get; set; }
		public string User { get; set; }
		public long Timestamp { get; set; }
		public DateTime DateHappened { get; set; }
	}

	public class DomainEventProviderReporting : BaseReportingWithAssignedId
	{
		public Guid EventProviderId { get; set; }

		public string Type { get; set; }
		public string FullQualifiedType { get; set; }

		public long EntityId { get; set; }

	}
}