using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PGMS.CQSLight.Extensions
{
	public static class JsonStringExtensions
	{
		public static string JsonPrettify(this string json)
		{
			if (string.IsNullOrEmpty(json))
			{
				return json;
			}

			return JValue.Parse(json).ToString(Formatting.Indented);
		}
	}
}