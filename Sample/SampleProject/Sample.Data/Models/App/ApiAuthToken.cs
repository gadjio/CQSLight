using System;

namespace Sample.Data.Models.App;

public class ApiAuthToken : BaseReportingWithSqlId
{
	public string Name { get; set; }

	public string AuthorizationLevel { get; set; }

	public string AuthKey { get; set; }

	public bool IsDisabled { get; set; }

	public string Note { get; set; }
}

public enum AuthorizationSection
{
	General,
}