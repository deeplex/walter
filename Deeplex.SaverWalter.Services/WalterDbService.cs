using Deeplex.Saverwalter.Model;
namespace Deeplex.Saverwalter.Services
{
    public interface WalterDbService
    {
        SaverwalterContext ctx { get; set; }
        public void SaveWalter();
    }
}
