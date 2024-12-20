using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.Extensions.Localization;
using PGMS.BlazorComponents;
using PGMS.CQSLight.Helpers;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Querying.Services;
using PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;
using PGMS.Data.Services;
using TestBlazorApp.Components;
using TestBlazorApp.Resources;
using TestBlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

builder.Services.AddPgmsBlazorComponents(opt => 
    opt.ActionModalSettings.PreventClose = true
);

builder.Services.AddLocalization();
var stringLocalizer = builder.Services.BuildServiceProvider().GetService<IStringLocalizer<SiteText>>();
builder.Services.AddSingleton<IStringLocalizer>(stringLocalizer);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//Will ned to register a context + dataservice
builder.Services.AddSingleton<IDataService>(new FakeDataService());
builder.Services.AddSingleton<IEntityRepository>(new InMemoryEntityRepository());

builder.Services.AddSingleton<IBus>(new FakeBus());
builder.Services.AddSingleton<IQueryProcessor>(new FakeQueryProcessor());

builder.Services.AddSingleton<CommandHelper>();
builder.Services.AddSingleton<QueryHelper>();
builder.Services.AddSingleton<IErrorHandlerService>(new ErrorHandlerService());
builder.Services.AddSingleton<ISessionInfoProvider>(new SessionInfoProvider());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
