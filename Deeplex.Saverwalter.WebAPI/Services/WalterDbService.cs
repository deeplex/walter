using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class WalterDbService : IWalterDbService
    {
        public string databasePort { get; set; }
        public string databaseURL { get; set; }
        public string databaseUser { get; set; }
        public string databasePass { get; set; }

        public SaverwalterContext ctx { get; set; }

        public WalterDbService()
        {
            databasePort = "5432";
            databaseURL = "192.168.178.61";
            databaseUser = "postgres";
            databasePass = "admin";

            var optionsBuilder = new DbContextOptionsBuilder<SaverwalterContext>();
            optionsBuilder.UseNpgsql(
                "Server=" + databaseURL +
                ";Port=" + databasePort +
                ";Database=postgres;Username=" + databaseUser +
                ";Password=" + databasePass);

            ctx = new SaverwalterContext(optionsBuilder.Options);
        }

        public void SaveWalter()
        {
            ctx.SaveChanges();
        }
    }
}
