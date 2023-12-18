using Deeplex.Saverwalter.InitiateTestDbs.Templates;

namespace Deeplex.Saverwalter.InitiateTestDbs
{
    internal class Program
    {
        static async Task Main()
        {
            var databaseUser = Environment.GetEnvironmentVariable("DATABASE_USER");
            if (databaseUser == null)
            {
                Console.WriteLine("Can't find environment variable DATABASE_USER");
            }

            var databasePass = Environment.GetEnvironmentVariable("DATABASE_PASS");
            if (databaseUser == null)
            {
                Console.WriteLine("Can't find environment variable DATABASE_PASS");
            }

            var databaseHost = Environment.GetEnvironmentVariable("DATABASE_HOST");
            if (databaseUser == null)
            {
                Console.WriteLine("Can't find environment variable DATABASE_HOST");
            }

            var databasePort = Environment.GetEnvironmentVariable("DATABASE_PORT");
            if (databaseUser == null)
            {
                Console.WriteLine("Can't find environment variable DATABASE_PORT");
            }

            await GenericDatabase.ConnectAndPopulate(
                databaseHost!,
                databasePort!,
                "walter_dev_generic_db",
                databaseUser!,
                databasePass!,
                10);

            await GenericDatabase.ConnectAndPopulate(
                databaseHost!,
                databasePort!,
                "walter_dev_full_generic_db",
                databaseUser!,
                databasePass!,
                100);
        }
    }
}
