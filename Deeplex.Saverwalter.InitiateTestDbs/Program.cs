using Deeplex.Saverwalter.InitiateTestDbs.Templates;
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Model.Auth;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Deeplex.Saverwalter.InitiateTestDbs
{
    internal class Program
    {
        private static string databaseUser = "root";
        private static string databasePass = "securepw";
        private static string databaseHost = "localhost";
        private static string databasePort = "5432";
        private static string databaseName = "walter_dev_full_generic_db";

        static async Task Main(string[] args)
        {
            var options = CreateDbContextOptions();
            var ctx = new SaverwalterContext(options);
            await ctx.Database.EnsureDeletedAsync();
            await ctx.SaveChangesAsync();
            ctx.Dispose();

            var options2 = CreateDbContextOptions();
            var ctx2 = new SaverwalterContext(options2);
            await ctx2.Database.MigrateAsync();
            await ctx2.SaveChangesAsync();

            await FullGenericDatabase.PopulateDatabase(ctx2, databaseUser, databasePass);
        }

        private static DbContextOptions<SaverwalterContext> CreateDbContextOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseNpgsql(
                 $@"Server={databaseHost}
                ;Port={databasePort}
                ;Database={databaseName}
                ;Username={databaseUser}
                ;Password={databasePass}");

            return optionsBuilder.Options;
        }
    }
}