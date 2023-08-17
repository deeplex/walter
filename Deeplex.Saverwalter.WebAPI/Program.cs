using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Text;

[assembly: ApiController]

namespace Deeplex.Saverwalter.WebAPI
{
    public class Program
    {
        public static string AppName = "Saverwalter";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var container = GetServiceContainer();

            var app = Configure(builder, container);

            container.Verify();

            CreateRootIfNoUserExists();

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

            app.MapControllers();
            app.UseRouting();

            app.UseStaticFiles();
            app.MapFallbackToFile("index.html");

            return app;
        }

        private static void AddServices(WebApplicationBuilder builder, Container container)
        {
            if (Environment.GetEnvironmentVariable("OTEL_ENDPOINT") is string otel_endpoint)
            {
                builder.Services.AddOpenTelemetry()
                    .WithTracing(tracerProviderBuilder => tracerProviderBuilder
                    .AddSource(AppName)
                    .AddOtlpExporter(opt =>
                    {
                        opt.Endpoint = new Uri(otel_endpoint);
                        opt.Protocol = OtlpExportProtocol.Grpc;
                    })
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: AppName))
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddSqlClientInstrumentation());
            }

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
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Register(CreateDbContextOptions, Lifestyle.Singleton);
            container.Register<SaverwalterContext>(Lifestyle.Scoped);

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

        private static DbContextOptions<SaverwalterContext> CreateDbContextOptions()
        {
            var databaseHost = Environment.GetEnvironmentVariable("DATABASE_HOST");
            var databasePort = Environment.GetEnvironmentVariable("DATABASE_PORT");
            var databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
            var databaseUser = Environment.GetEnvironmentVariable("DATABASE_USER");
            var databasePass = Environment.GetEnvironmentVariable("DATABASE_PASS");

            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseNpgsql(
                 $@"Server={databaseHost}
                ;Port={databasePort}
                ;Database={databaseName}
                ;Username={databaseUser}
                ;Password={databasePass}");

            return optionsBuilder.Options;
        }

        private static async Task CreateRootIfNoUserExists(Container container)
        {
            var dbContext = container.GetInstance<SaverwalterContext>();

            if (await dbContext.UserAccounts.CountAsync() > 0)
            {
                return;
            }

            var rootPassword = Environment.GetEnvironmentVariable("WALTER_PASSWORD");
            if (string.IsNullOrEmpty(rootPassword))
            {
                return;
            }

            // either create the account _and_ associate a password or do nothing
            using var tx = await dbContext.Database.BeginTransactionAsync();

            var userService = container.GetInstance<UserService>();
            var rootAccount = await userService.CreateUserAccount("root");
            await userService.UpdateUserPassword(rootAccount, Encoding.UTF8.GetBytes(rootPassword));

            await tx.CommitAsync();
        }
    }
}