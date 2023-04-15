using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WalterDbService;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Authorization;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SimpleInjector;

namespace Deeplex.Saverwalter.WebAPI
{
    public class Program
    {
        public static string AppVersion = Environment.GetEnvironmentVariable("WalterVersion") ?? "v0.0.0";
        public static string AppName = "Saverwalter";
        private static string APMServer = Environment.GetEnvironmentVariable("APM-Server") ?? "http://192.168.178.61:8200"; // TODO change to localhost...

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var container = GetServiceContainer();

            var app = Configure(builder, container);

            container.Verify();
            app.Run();
        }

        private static WebApplication Configure(WebApplicationBuilder builder, Container container)
        {
            AddServices(builder, container);

            var app = builder.Build();

            app.Services.UseSimpleInjector(container);
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            // app.UseStaticFiles();
            app.MapControllers();

            return app;
        }

        private static void AddServices(WebApplicationBuilder builder, Container container)
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder => tracerProviderBuilder
                .AddSource(AppName)
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(APMServer);
                    opt.Protocol = OtlpExportProtocol.Grpc;
                })
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: AppName, serviceVersion: AppVersion))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddSqlClientInstrumentation());

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new AuthorizeFilter("RequireAuthenticatedUser"));
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSimpleInjector(container, options =>
            {
                options.AddAspNetCore().AddControllerActivation();
            });

            builder.Services.AddAuthentication("TokenAuthentication")
                .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>("TokenAuthentication", null);

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                {
                    policy.AddAuthenticationSchemes("TokenAuthentication");
                    policy.RequireAuthenticatedUser();
                });
            });

            builder.Services.AddTransient(c => container.GetInstance<TokenService>());
            builder.Services.AddTransient(c => container.GetInstance<SaverwalterContext>());
        }

        private static Container GetServiceContainer()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new SimpleInjector.Lifestyles.AsyncScopedLifestyle();
            container.Register<WalterDb, WalterDbImpl>(Lifestyle.Scoped);
            container.Register(() => container.GetInstance<WalterDb>().ctx, Lifestyle.Scoped);

            container.Register<AdresseDbService>(Lifestyle.Scoped);
            container.Register<BetriebskostenrechnungDbService>(Lifestyle.Scoped);
            container.Register<ErhaltungsaufwendungDbService>(Lifestyle.Scoped);
            container.Register<JuristischePersonDbService>(Lifestyle.Scoped);
            container.Register<MieteDbService>(Lifestyle.Scoped);
            container.Register<MietminderungDbService>(Lifestyle.Scoped);
            container.Register<NatuerlichePersonDbService>(Lifestyle.Scoped);
            container.Register<UmlageDbService>(Lifestyle.Scoped);
            container.Register<VertragDbService>(Lifestyle.Scoped);
            container.Register<VertragVersionDbService>(Lifestyle.Scoped);
            container.Register<WohnungDbService>(Lifestyle.Scoped);
            container.Register<ZaehlerDbService>(Lifestyle.Scoped);
            container.Register<ZaehlerstandDbService>(Lifestyle.Scoped);

            container.Register<BetriebskostenabrechnungHandler>(Lifestyle.Scoped);

            container.Register<TokenService>(Lifestyle.Singleton);
            container.Register<UserService>(Lifestyle.Scoped);

            return container;
        }
    }
}