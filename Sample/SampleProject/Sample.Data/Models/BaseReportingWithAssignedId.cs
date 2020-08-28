using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.Data.Models
{
	public abstract class BaseReportingWithAssignedId
	{
		[Required, Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long Id { get; set; }
	}

	public abstract class BaseReportingWithSqlId
	{
		[Required, Key]
		public long Id { get; set; }
	}

	public abstract class BaseAggregateRootReporting
	{
		[Required, Key]
		public long Id { get; set; }

		[Required]
		public Guid AggregateRootId { get; set; }

		public long LastUpdate { get; set; }
	}
}