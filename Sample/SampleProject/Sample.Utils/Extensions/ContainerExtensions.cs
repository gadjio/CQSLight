using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace Sample.Utils.Extensions
{
	public static class ContainerExtensions
	{
		public static T[] ResolveAll<T>(this IComponentContext self)
		{
			return self.Resolve<IEnumerable<T>>().ToArray();
		}
	}
}