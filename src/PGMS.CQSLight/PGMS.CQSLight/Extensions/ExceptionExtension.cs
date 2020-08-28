using System;
using System.Text;

namespace PGMS.CQSLight.Extensions
{
	public static class ExceptionExtension
	{
		public static string GetErrorDetails(this Exception ex)
		{
			var sb = new StringBuilder();
			sb.AppendLine(ex.Message);
			sb.AppendLine("***********");
			sb.AppendLine(ex.StackTrace);
			sb.AppendLine("");

			if (ex.InnerException != null)
			{
				sb.AppendLine(GetErrorDetails(ex.InnerException));
			}

			return sb.ToString();
		}
	}
}