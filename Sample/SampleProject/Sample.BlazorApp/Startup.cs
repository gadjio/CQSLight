using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Core.Infra.Configs;
using Sample.Core.Installers;
using Sample.DataProvider.Contexts;
using Sample.DataProvider.Installers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PGMS.CQSLight.Helpers;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.Data.Services;
using Sample.BlazorApp.Installers;
using Sample.Core.AppCore.RegionalWeatherForecast.Commands;
using Sample.Data.Models.WeatherForecast;


namespace Sample.BlazorApp
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<SampleContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

			services.AddRazorPages();
			services.AddServerSideBlazor();
			
			services.AddSingleton<CommandHelper>();
			services.AddSingleton<QueryHelper>();

			services.AddHttpContextAccessor();
		}

		// ConfigureContainer is where you can register things directly
		// with Autofac. This runs after ConfigureServices so the things
		// here will override registrations made in ConfigureServices.
		// Don't build the container; that gets done for you by the factory.
		public void ConfigureContainer(ContainerBuilder builder)
		{
			var connection = Configuration.GetConnectionString("DefaultConnection");

			var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();

			builder.Register(c => Options.Create(appSettings)).As<IOptions<AppSettings>>().SingleInstance();
			DataLayerInstaller.ConfigureServices(builder, connection);
			ServiceLayerInstaller.ConfigureServices(builder);
			BlazorAppInstaller.ConfigureServices(builder, connection, appSettings);
		}


		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			// Workaround for https://github.com/aspnet/AspNetCore/issues/13470
			app.Use((context, next) =>
			{
				context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = null;
				return next.Invoke();
			});

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseRequestLocalization();
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});

			var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();

			var context = app.ApplicationServices.GetService<SampleContext>();
			context.Database.Migrate();

			var entityRepository = app.ApplicationServices.GetService<IEntityRepository>();
			var bus = app.ApplicationServices.GetService<IBus>();
			var dataService = app.ApplicationServices.GetService<IDataService>();

			InitialDataGenerator.InitializeDate(entityRepository, bus, dataService);
		}
	}

	public static class InitialDataGenerator
	{
		public static void InitializeDate(IEntityRepository entityRepository, IBus bus, IDataService dataService)
		{
			var region = entityRepository.FindFirst<RegionReporting>(x => x.Name == "Shawinigan");
			Guid regionId;
			if (region == null)
			{
				regionId = Guid.NewGuid();
				bus.Send(new AddANewRegionCommand{AggregateRootId = regionId, Name = "Shawinigan", ByUsername = "InitialDataGenerator" });
			}
			else
			{
				regionId = region.AggregateRootId;
			}

			var todaysForecasts = entityRepository.FindAll<WeatherInfoReporting>(x => x.AggregateRootId == regionId && x.Date == DateTime.Today);
			if (!todaysForecasts.Any())
			{
				var rng = new Random();
				bus.Send(new AddWeatherForecastCommand
				{
					AggregateRootId = regionId, 
					EntityId = dataService.GenerateId(), 
					TemperatureC = rng.Next(-20, 55),
					Summary = Summaries[rng.Next(Summaries.Length)]
				});
			}
		}

		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

	}
}
