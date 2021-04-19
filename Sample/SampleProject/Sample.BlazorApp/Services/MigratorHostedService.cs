using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.DataProvider.Contexts;
using Sample.UpdateData.Services;

namespace Sample.BlazorApp.Services
{
	public class MigratorHostedService : IHostedService
	{
		// We need to inject the IServiceProvider so we can create 
		// the scoped service, MyDbContext
		private readonly IServiceProvider _serviceProvider;
		public MigratorHostedService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var myDbContext = scope.ServiceProvider.GetRequiredService<SampleContext>();

				await myDbContext.Database.MigrateAsync();

				var updateDataServiceRunner = scope.ServiceProvider.GetService<IUpdateDataServiceRunner>();
				await updateDataServiceRunner.Run();
			}
		}


		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}