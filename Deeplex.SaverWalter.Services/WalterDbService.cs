using Deeplex.Saverwalter.Model;
namespace Deeplex.Saverwalter.Services
{
    public interface IWalterDbService
    {
        SaverwalterContext ctx { get; set; }
        public void SaveWalter();
    }
}
