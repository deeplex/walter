using Deeplex.Saverwalter.Services;
using SimpleInjector;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;

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
            container.Options.DefaultScopedLifestyle = new SimpleInjector.Lifestyles.ThreadScopedLifestyle();
            container.Register<INotificationService, NotificationService>(Lifestyle.Scoped);
            container.Register<IFileService, FileService>(Lifestyle.Scoped);
            container.Register<IWalterDbService, WalterDbService>(Lifestyle.Scoped);

            container.Register<AdresseDbService>(Lifestyle.Scoped);
            container.Register<AnhangDbService>(Lifestyle.Scoped);
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

            container.Register<BetriebskostenabrechnungSerivce>(Lifestyle.Scoped);

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
            builder.Services.AddSimpleInjector(container, options =>
            {
                options.AddAspNetCore().AddControllerActivation();
            });

            var app = builder.Build();
            
            app.Services.UseSimpleInjector(container);

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