// Copyright (c) 2023-2024 Henrik S. Gaßmann, Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Security.Claims;
using System.Text;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SimpleInjector;
using SimpleInjector.Lifestyles;

[assembly: ApiController]

namespace Deeplex.Saverwalter.WebAPI
{
    public class Program
    {
        public readonly static string AppName = "Saverwalter";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var container = GetServiceContainer();

            var app = Configure(builder, container);

            container.Verify();

            await CreateRootIfNoUserExists(container);

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

            app.MapControllers();
            app.UseRouting();
            app.UseAuthorization();

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
            builder.Services.AddHttpClient();
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

                options.AddPolicy("RequireAdmin", policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, [UserRole.Admin.ToString()]);
                });

                options.AddPolicy("RequireOwner", policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, [UserRole.Owner.ToString()]);
                });
            });

            builder.Services.AddTransient(c => container.GetInstance<TokenService>());
            builder.Services.AddTransient(c => container.GetInstance<SaverwalterContext>());
            // TODO? AbrechnungsPermissionHandler?
            builder.Services.AddSingleton<IAuthorizationHandler, WohnungPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, AdressePermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, BetriebskostenrechnungPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, ErhaltungsaufwendungPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, KontaktPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, MietePermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, MietminderungPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, UmlagePermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, UmlagenPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, UmlagetypPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, VertragPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, VertragVersionPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, WohnungenPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, ZaehlerPermissionHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, ZaehlerstandPermissionHandler>();
        }

        private static Container GetServiceContainer()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Register(CreateDbContextOptions, Lifestyle.Singleton);
            container.Register<SaverwalterContext>(Lifestyle.Scoped);

            container.Register<AbrechnungsresultatDbService>(Lifestyle.Scoped);
            container.Register<AdresseDbService>(Lifestyle.Scoped);
            container.Register<BetriebskostenrechnungDbService>(Lifestyle.Scoped);
            container.Register<ErhaltungsaufwendungDbService>(Lifestyle.Scoped);
            container.Register<MieteDbService>(Lifestyle.Scoped);
            container.Register<MietminderungDbService>(Lifestyle.Scoped);
            container.Register<KontaktDbService>(Lifestyle.Scoped);
            container.Register<UmlageDbService>(Lifestyle.Scoped);
            container.Register<UmlagetypDbService>(Lifestyle.Scoped);
            container.Register<VertragDbService>(Lifestyle.Scoped);
            container.Register<VertragVersionDbService>(Lifestyle.Scoped);
            container.Register<WohnungDbService>(Lifestyle.Scoped);
            container.Register<ZaehlerDbService>(Lifestyle.Scoped);
            container.Register<ZaehlerstandDbService>(Lifestyle.Scoped);

            container.Register<BetriebskostenabrechnungHandler>(Lifestyle.Scoped);

            container.Register<TokenService>(Lifestyle.Singleton);
            container.Register<AccountDbService>(Lifestyle.Scoped);
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
            var connection = $@"Server={databaseHost}
                ;Port={databasePort}
                ;Database={databaseName}
                ;Username={databaseUser}
                ;Password={databasePass}";
            optionsBuilder.UseNpgsql(connection);

            return optionsBuilder.Options;
        }

        private static async Task CreateRootIfNoUserExists(Container container)
        {
            await using (AsyncScopedLifestyle.BeginScope(container))
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
                var rootAccount = await userService.CreateUserAccount("root", "root");
                rootAccount.Role = UserRole.Admin;
                await userService.UpdateUserPassword(rootAccount, Encoding.UTF8.GetBytes(rootPassword));

                await tx.CommitAsync();
            }
        }
    }
}
