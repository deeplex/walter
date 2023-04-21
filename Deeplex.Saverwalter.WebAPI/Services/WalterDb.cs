using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WalterDbService;

namespace Deeplex.Saverwalter.WebAPI
{
    public sealed class WalterDbImpl : WalterDb
    {
        public SaverwalterContext ctx { get; set; }

        public WalterDbImpl(SaverwalterContext saverwalterContext)
        {
            ctx = saverwalterContext;
        }

        public void SaveWalter()
        {
            ctx.SaveChanges();
        }
    }
}
