using System;

namespace Sample.Data.Models.App
{
	public class UpdateServiceLookup : BaseReportingWithSqlId
	{
		public string ServiceName { get; set; }
		public DateTime LastRunTime { get; set; }
	}
}