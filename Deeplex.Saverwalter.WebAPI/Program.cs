using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using SimpleInjector;

namespace Deeplex.Saverwalter.WebAPI
{
    public class Program
    {
        private static Container Container { get; set; } = null!;
        public static SaverwalterContext ctx => Container.GetInstance<IWalterDbService>().ctx;

        public static void Main(string[] args)
        {
            Container = new Container();
            Container.Register<INotificationService, NotificationService>(Lifestyle.Singleton);
            Container.Register<IFileService, FileService>(Lifestyle.Singleton);
            Container.Register<IWalterDbService, WalterDbService>(Lifestyle.Singleton);

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

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