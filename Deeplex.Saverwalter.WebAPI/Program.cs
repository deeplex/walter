using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using SimpleInjector;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Deeplex.Saverwalter.WebAPI
{
    public class Program
    {
        public static string AppVersion = Environment.GetEnvironmentVariable("WalterVersion") ?? "v0.0.0";
        public static string AppName = "Saverwalter";
        private static string APMServer = Environment.GetEnvironmentVariable("APM-Server") ?? "http://192.168.178.61:8200"; // TODO change to localhost...

        private static Container Container { get; set; } = null!;
        public static SaverwalterContext ctx => Container.GetInstance<IWalterDbService>().ctx;
        public static TEntity LoadNavigations<TEntity>(TEntity entity)
        {
            ctx.Entry(entity).Collections.ToList().ForEach(e => e.Load());
            return entity;
        }

        public static void Main(string[] args)
        {
            Container = new Container();
            Container.Register<INotificationService, NotificationService>(Lifestyle.Transient);
            Container.Register<IFileService, FileService>(Lifestyle.Transient);
            Container.Register<IWalterDbService, WalterDbService>(Lifestyle.Transient);
            Container.Verify();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
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

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseStaticFiles();

            //app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}