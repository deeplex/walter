using Deeplex.Saverwalter.Model;
namespace Deeplex.Saverwalter.WalterDbService
{
    public interface WalterDb
    {
        SaverwalterContext ctx { get; set; }
        public void SaveWalter();
    }
}
