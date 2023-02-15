using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Saverwalter.WebAPI.Services;
using SimpleInjector;

namespace Deeplex.Saverwalter.WebAPI
{
    public class Program
    {
        private static Container Container { get; set; } = null!;
        public static SaverwalterContext ctx => Container.GetInstance<IWalterDbService>().ctx;
        public static IPerson FindPerson(Guid guid) => Container.GetInstance<PersonMap>().FindPerson(guid);
        public static TEntity LoadNavigations<TEntity>(TEntity entity)
        {
            ctx.Entry(entity).Collections.ToList().ForEach(e => e.Load());
            return entity;
        }

        public static void Main(string[] args)
        {
            Container = new Container();
            Container.Register<INotificationService, NotificationService>(Lifestyle.Singleton);
            Container.Register<IFileService, FileService>(Lifestyle.Singleton);
            Container.Register<IWalterDbService, WalterDbService>(Lifestyle.Singleton);
            Container.Register<PersonMap>(Lifestyle.Singleton);
            Container.Verify();

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