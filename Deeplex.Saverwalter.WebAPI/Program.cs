using Deeplex.Saverwalter.Services;
using SimpleInjector;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Deeplex.Saverwalter.WebAPI
{
    public class Program
    {
        public static string AppVersion = Environment.GetEnvironmentVariable("WalterVersion") ?? "v0.0.0";
        public static string AppName = "Saverwalter";
        private static string APMServer = Environment.GetEnvironmentVariable("APM-Server") ?? "http://192.168.178.61:8200"; // TODO change to localhost...

        public static void Main(string[] args)
        {
            var container = new Container();

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
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IWalterDbService, WalterDbService>();
            builder.Services.AddSimpleInjector(container);

            var app = builder.Build();
            
            // Can't use app.UseSimpleInjector because of amiguity https://github.com/simpleinjector/SimpleInjector/issues/933
            SimpleInjectorUseOptionsAspNetCoreExtensions.UseSimpleInjector(app, container);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseStaticFiles();

            //app.UseAuthorization();
            app.MapControllers();

            container.Verify();
            app.Run();
        }
    }
}