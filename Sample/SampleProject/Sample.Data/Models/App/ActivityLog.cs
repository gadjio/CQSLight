using System;

namespace Sample.Data.Models.App
{
	public class ActivityLog
	{
		public long Id { get; set; }

		public string Username { get; set; }

		public long Timestamp { get; set; }
		public DateTime DateHappened { get; set; }

		public string Action { get; set; }
		public string Area { get; set; }
		public string Controller { get; set; }

		public string RawUrl { get; set; }

		public string Ip { get; set; }

		public string UserSessionUniqueId { get; set; }
    }
}