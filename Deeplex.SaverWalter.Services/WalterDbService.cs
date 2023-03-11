using Deeplex.Saverwalter.Model;
namespace Deeplex.Saverwalter.Services
{
    public interface IWalterDbService
    {
        string databaseURL { get; set; }
        string databasePort { get; set; }
        string databaseUser { get; set; }
        string databasePass { get; set; }

        SaverwalterContext ctx { get; set; }
        public void SaveWalter();
    }
}
