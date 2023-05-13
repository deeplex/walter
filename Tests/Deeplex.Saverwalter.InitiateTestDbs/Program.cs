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
            await ctx.Database.EnsureCreatedAsync();
            
            await ctx.SaveChangesAsync();

            await FullGenericDatabase.PopulateDatabase(ctx, databaseUser, databasePass);
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