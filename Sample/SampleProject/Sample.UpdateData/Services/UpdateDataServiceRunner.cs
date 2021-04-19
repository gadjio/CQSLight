using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Sample.Utils.Extensions;

namespace Sample.UpdateData.Services
{
	public interface IUpdateDataServiceRunner
	{
		Task Run();
	}

	public class UpdateDataServiceRunner : IUpdateDataServiceRunner
	{
		private readonly IComponentContext context;

		public UpdateDataServiceRunner(IComponentContext context)
		{
			this.context = context;
		}

		public async Task Run()
		{
			var dataUpdateServices = context.ResolveAll<IUpdateDataService>();
			foreach (var dataUpdateService in dataUpdateServices.OrderBy(x => x.GetType().FullName))
			{
				await dataUpdateService.Update();
			}
		}

	}

}